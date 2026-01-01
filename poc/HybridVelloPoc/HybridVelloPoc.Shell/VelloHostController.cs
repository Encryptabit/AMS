using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VelloSharp;
using VelloSharp.Scenes;

namespace HybridVelloPoc.Shell;

/// <summary>
/// Hosts a VelloSharp/Winit window as an owned window of a WPF shell.
/// The Vello window is positioned over a designated content area and
/// synchronized with the parent WPF window's position, size, and state.
/// </summary>
public sealed class VelloHostController : IWinitEventHandler, IDisposable
{
    private const int GWL_HWNDPARENT = -8;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOZORDER = 0x0004;
    private const int SWP_NOACTIVATE = 0x0010;
    private const int SWP_SHOWWINDOW = 0x0040;
    private const int SWP_HIDEWINDOW = 0x0080;
    private const int HWND_TOP = 0;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private readonly IntPtr _wpfHwnd;
    private readonly Window _wpfWindow;
    private readonly Border _contentHost;
    private readonly WinitEventLoop _eventLoop;
    private readonly Scene _scene;
    private readonly Stopwatch _clock;
    private readonly Stopwatch _frameTimer;
    private readonly object _syncLock = new();

    private ManagedSceneHost? _host;
    private IReadOnlyList<ExampleScene> _scenes = Array.Empty<ExampleScene>();

    private WgpuInstance? _wgpuInstance;
    private WgpuAdapter? _wgpuAdapter;
    private WgpuDevice? _wgpuDevice;
    private WgpuQueue? _wgpuQueue;
    private WgpuSurface? _wgpuSurface;
    private WgpuRenderer? _wgpuRenderer;
    private WgpuSurfaceConfiguration _surfaceConfig;
    private WgpuTextureFormat _surfaceFormat = WgpuTextureFormat.Bgra8Unorm;
    private bool _requiresSurfaceBlit;
    private bool _surfaceValid;

    private WinitWindow? _winitWindow;
    private IntPtr _velloHwnd;
    private Thread? _renderThread;
    private volatile bool _running;
    private volatile bool _exitRequested;
    private volatile bool _needsRedraw = true;
    private volatile bool _windowVisible = true;
    private volatile bool _statsVisible = true;

    private uint _width = 800;
    private uint _height = 600;
    private double _scaleFactor = 1.0;
    private int _sceneIndex;
    private Matrix3x2 _viewTransform = Matrix3x2.Identity;
    private RgbaColor _baseColor = RgbaColor.FromBytes(12, 16, 20);

    private bool _pointerDown;
    private Vector2? _pointerPosition;

    // Position/size commands from WPF thread
    private volatile int _pendingX;
    private volatile int _pendingY;
    private volatile int _pendingWidth;
    private volatile int _pendingHeight;
    private volatile bool _pendingPositionUpdate;

    // Frame stats
    private readonly Queue<double> _frameTimes = new();
    private double _frameTimeSum;
    private const int MaxFrameSamples = 60;

    public event Action<string, int, int>? SceneChanged;
    public event Action<double>? FpsUpdated;
    public event Action<string>? StatusUpdated;

    public VelloHostController(IntPtr wpfHwnd, Window wpfWindow, Border contentHost)
    {
        _wpfHwnd = wpfHwnd;
        _wpfWindow = wpfWindow;
        _contentHost = contentHost;
        _eventLoop = new WinitEventLoop();
        _scene = new Scene();
        _clock = Stopwatch.StartNew();
        _frameTimer = Stopwatch.StartNew();
    }

    public void Start()
    {
        if (_running)
            return;

        _running = true;
        _exitRequested = false;

        // Spawn Winit event loop on a separate thread
        _renderThread = new Thread(RenderThreadMain)
        {
            Name = "Vello Render Thread",
            IsBackground = true
        };
        _renderThread.SetApartmentState(ApartmentState.STA);
        _renderThread.Start();
    }

    public void Stop()
    {
        if (!_running)
            return;

        _exitRequested = true;
        _running = false;

        // Wait for thread to complete
        _renderThread?.Join(TimeSpan.FromSeconds(2));
        _renderThread = null;
    }

    private void RenderThreadMain()
    {
        try
        {
            // Get initial position from WPF content host (must be called on UI thread)
            (int x, int y, int w, int h) = (0, 0, 800, 600);
            _wpfWindow.Dispatcher.Invoke(() =>
            {
                (x, y, w, h) = GetContentHostScreenRect();
            });
            _pendingX = x;
            _pendingY = y;
            _pendingWidth = w;
            _pendingHeight = h;

            var configuration = new WinitRunConfiguration
            {
                CreateWindow = true,
                Window = new WinitWindowOptions
                {
                    Width = (uint)Math.Max(100, w),
                    Height = (uint)Math.Max(100, h),
                    Title = "Vello Render Window",
                    Visible = false,    // Start hidden, show after setup
                    Decorations = false, // Borderless
                    Resizable = false,   // Size controlled by WPF
                },
            };

            var status = _eventLoop.Run(configuration, this);
            if (status != WinitStatus.Success)
            {
                RaiseStatusUpdated($"Event loop failed: {status}");
            }
        }
        catch (Exception ex)
        {
            RaiseStatusUpdated($"Render thread error: {ex.Message}");
        }
    }

    public void HandleEvent(WinitEventLoopContext context, in WinitEventArgs args)
    {
        if (_exitRequested)
        {
            context.Exit();
            return;
        }

        switch (args.Kind)
        {
            case WinitEventKind.WindowCreated:
                HandleWindowCreated(context, args);
                break;
            case WinitEventKind.WindowResized:
                HandleWindowResized(args.Width, args.Height);
                break;
            case WinitEventKind.WindowScaleFactorChanged:
                if (args.ScaleFactor > 0)
                {
                    _scaleFactor = args.ScaleFactor;
                }
                break;
            case WinitEventKind.WindowRedrawRequested:
                RenderFrame();
                break;
            case WinitEventKind.WindowCloseRequested:
                // Don't close - WPF manages the lifecycle
                break;
            case WinitEventKind.WindowDestroyed:
                DestroySurface();
                _winitWindow = null;
                _velloHwnd = IntPtr.Zero;
                break;
            case WinitEventKind.AboutToWait:
                ProcessPendingUpdates();
                if (_needsRedraw && _surfaceValid && _winitWindow is not null && _windowVisible)
                {
                    _winitWindow.RequestRedraw();
                    _needsRedraw = false;
                }
                break;
            case WinitEventKind.MouseInput:
                if (args.MouseButton == WinitMouseButton.Left)
                {
                    _pointerDown = args.ElementState == WinitElementState.Pressed;
                }
                break;
            case WinitEventKind.MouseWheel:
                HandleMouseWheel(args);
                break;
            case WinitEventKind.CursorMoved:
                HandleCursorMoved(args);
                break;
            case WinitEventKind.CursorLeft:
                _pointerPosition = null;
                break;
            case WinitEventKind.KeyboardInput:
                HandleKeyboard(args);
                break;
        }
    }

    private void HandleWindowCreated(WinitEventLoopContext context, in WinitEventArgs args)
    {
        _winitWindow = context.GetWindow() ?? throw new InvalidOperationException("Window handle unavailable.");
        context.SetControlFlow(WinitControlFlow.Poll);

        _width = Math.Max(1u, args.Width);
        _height = Math.Max(1u, args.Height);
        _scaleFactor = args.ScaleFactor > 0d ? args.ScaleFactor : _scaleFactor;

        // Get the Vello window HWND
        var velloHandle = _winitWindow.GetVelloWindowHandle();
        if (velloHandle.Kind == VelloWindowHandleKind.Win32)
        {
            _velloHwnd = velloHandle.Payload.Win32.Hwnd;
            RaiseStatusUpdated($"Vello HWND: 0x{_velloHwnd:X8}");

            // Set WPF window as owner - this is the key to the hybrid approach!
            SetWindowLongPtr(_velloHwnd, GWL_HWNDPARENT, _wpfHwnd);

            // Make it a tool window so it doesn't appear in taskbar
            var exStyle = GetWindowLongPtr(_velloHwnd, GWL_EXSTYLE);
            SetWindowLongPtr(_velloHwnd, GWL_EXSTYLE, (IntPtr)((long)exStyle | WS_EX_TOOLWINDOW));

            // Position it over the content host
            ApplyPendingPosition();

            // Show the window
            _winitWindow.SetVisible(true);
            _windowVisible = true;
        }
        else
        {
            RaiseStatusUpdated($"Unexpected window handle kind: {velloHandle.Kind}");
        }

        // Initialize GPU context
        var descriptor = CreateSurfaceDescriptor(_winitWindow, _width, _height);
        EnsureGpuContext(descriptor);
        ConfigureSurface(_width, _height);

        // Initialize scenes
        InitializeScenes();

        RequestRedraw();
    }

    private void HandleWindowResized(uint width, uint height)
    {
        if (_wgpuSurface is null)
            return;

        if (width == 0 || height == 0)
        {
            _surfaceValid = false;
            return;
        }

        _width = width;
        _height = height;
        ConfigureSurface(width, height);
        RequestRedraw();
    }

    private void HandleCursorMoved(in WinitEventArgs args)
    {
        var position = new Vector2((float)args.MouseX, (float)args.MouseY);
        if (_pointerDown && _pointerPosition is Vector2 previous)
        {
            var delta = position - previous;
            if (delta.LengthSquared() > 0f)
            {
                _viewTransform = Matrix3x2.CreateTranslation(delta) * _viewTransform;
                RequestRedraw();
            }
        }
        _pointerPosition = position;
    }

    private void HandleMouseWheel(in WinitEventArgs args)
    {
        if (_pointerPosition is not Vector2 position)
            return;

        const double baseFactor = 1.05;
        const double pixelsPerLine = 20.0;
        double exponent = args.ScrollDeltaKind switch
        {
            WinitMouseScrollDeltaKind.LineDelta => args.DeltaY,
            WinitMouseScrollDeltaKind.PixelDelta => args.DeltaY / pixelsPerLine,
            _ => 0.0,
        };

        var scale = (float)Math.Pow(baseFactor, exponent);
        if (float.IsFinite(scale) && scale > 0f)
        {
            var translation = Matrix3x2.CreateTranslation(-position) *
                              Matrix3x2.CreateScale(scale) *
                              Matrix3x2.CreateTranslation(position);
            _viewTransform = translation * _viewTransform;
            RequestRedraw();
        }
    }

    private void HandleKeyboard(in WinitEventArgs args)
    {
        if (args.ElementState != WinitElementState.Pressed)
            return;

        var code = (uint)args.KeyCode;
        switch (code)
        {
            case 80: // ArrowLeft
                ChangeScene(-1);
                break;
            case 81: // ArrowRight
                ChangeScene(1);
                break;
            case 62: // Space
                ResetView();
                break;
            case 37: // KeyS
                ToggleStats();
                break;
            case 35: // KeyQ
            case 23: // KeyE
                if (_pointerPosition is Vector2 pivot)
                {
                    var angle = code == 23 ? -0.05f : 0.05f;
                    var rotation = Matrix3x2.CreateTranslation(-pivot) *
                                   Matrix3x2.CreateRotation(angle) *
                                   Matrix3x2.CreateTranslation(pivot);
                    _viewTransform = rotation * _viewTransform;
                    RequestRedraw();
                }
                break;
        }
    }

    private void ProcessPendingUpdates()
    {
        if (_pendingPositionUpdate && _velloHwnd != IntPtr.Zero)
        {
            ApplyPendingPosition();
            _pendingPositionUpdate = false;
        }
    }

    private void ApplyPendingPosition()
    {
        if (_velloHwnd == IntPtr.Zero)
            return;

        var x = _pendingX;
        var y = _pendingY;
        var w = _pendingWidth;
        var h = _pendingHeight;

        if (w > 0 && h > 0)
        {
            SetWindowPos(_velloHwnd, IntPtr.Zero, x, y, w, h, SWP_NOZORDER | SWP_NOACTIVATE);

            // Also resize the surface
            if (_winitWindow is not null && ((uint)w != _width || (uint)h != _height))
            {
                _winitWindow.SetInnerSize((uint)w, (uint)h);
            }
        }
    }

    public void SyncVelloWindowPosition()
    {
        var (x, y, w, h) = GetContentHostScreenRect();
        _pendingX = x;
        _pendingY = y;
        _pendingWidth = w;
        _pendingHeight = h;
        _pendingPositionUpdate = true;
    }

    public void HandleWindowStateChanged(WindowState state)
    {
        switch (state)
        {
            case WindowState.Minimized:
                _windowVisible = false;
                if (_velloHwnd != IntPtr.Zero)
                    ShowWindow(_velloHwnd, SW_HIDE);
                break;
            case WindowState.Normal:
            case WindowState.Maximized:
                _windowVisible = true;
                if (_velloHwnd != IntPtr.Zero)
                {
                    ShowWindow(_velloHwnd, SW_SHOW);
                    SyncVelloWindowPosition();
                }
                break;
        }
    }

    public void HandleWindowActivated(bool activated)
    {
        // When WPF window is activated, ensure Vello window is on top of it
        // but below any dialogs
        if (activated && _velloHwnd != IntPtr.Zero && _windowVisible)
        {
            SetWindowPos(_velloHwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
    }

    private (int x, int y, int width, int height) GetContentHostScreenRect()
    {
        try
        {
            // Get the content host's position in screen coordinates
            var contentPoint = _contentHost.PointToScreen(new Point(0, 0));

            // Get the actual rendered size
            var width = (int)_contentHost.ActualWidth;
            var height = (int)_contentHost.ActualHeight;

            // Account for DPI
            var source = PresentationSource.FromVisual(_contentHost);
            if (source?.CompositionTarget != null)
            {
                var dpiX = source.CompositionTarget.TransformToDevice.M11;
                var dpiY = source.CompositionTarget.TransformToDevice.M22;
                width = (int)(width * dpiX);
                height = (int)(height * dpiY);
            }

            return ((int)contentPoint.X, (int)contentPoint.Y, width, height);
        }
        catch
        {
            return (0, 0, 800, 600);
        }
    }

    private SurfaceDescriptor CreateSurfaceDescriptor(WinitWindow window, uint width, uint height)
    {
        var handle = window.GetVelloWindowHandle();
        return new SurfaceDescriptor
        {
            Width = width,
            Height = height,
            PresentMode = PresentMode.AutoVsync,
            Handle = SurfaceHandle.FromVelloHandle(handle),
        };
    }

    private void EnsureGpuContext(SurfaceDescriptor descriptor)
    {
        if (_wgpuInstance is null)
        {
            _wgpuInstance = new WgpuInstance();
        }

        _wgpuSurface?.Dispose();
        _wgpuSurface = WgpuSurface.Create(_wgpuInstance, descriptor);

        if (_wgpuAdapter is null)
        {
            var adapterOptions = new WgpuRequestAdapterOptions
            {
                PowerPreference = WgpuPowerPreference.HighPerformance,
                CompatibleSurface = _wgpuSurface,
            };
            _wgpuAdapter = _wgpuInstance.RequestAdapter(adapterOptions);

            var deviceDescriptor = new WgpuDeviceDescriptor
            {
                Label = "hybrid_vello_poc.device",
                RequiredFeatures = WgpuFeature.None,
                Limits = WgpuLimitsPreset.Default,
            };
            _wgpuDevice = _wgpuAdapter.RequestDevice(deviceDescriptor);
            _wgpuQueue = _wgpuDevice.GetQueue();

            var options = RendererOptionsExtensions.CreateGpuOptions(
                useCpu: false,
                supportArea: true,
                supportMsaa8: true,
                supportMsaa16: false);
            _wgpuRenderer = new WgpuRenderer(_wgpuDevice, options);
        }
    }

    private void ConfigureSurface(uint width, uint height)
    {
        if (_wgpuSurface is null || _wgpuAdapter is null || _wgpuDevice is null)
            return;

        var preferredFormat = _wgpuSurface.GetPreferredFormat(_wgpuAdapter);
        _requiresSurfaceBlit = RequiresSurfaceBlit(preferredFormat);
        _surfaceFormat = NormalizeSurfaceFormat(preferredFormat);

        _surfaceConfig = new WgpuSurfaceConfiguration
        {
            Usage = WgpuTextureUsage.RenderAttachment,
            Format = _surfaceFormat,
            Width = width,
            Height = height,
            PresentMode = PresentMode.AutoVsync,
            AlphaMode = WgpuCompositeAlphaMode.Auto,
            ViewFormats = null,
        };

        _wgpuSurface.Configure(_wgpuDevice, _surfaceConfig);
        _surfaceValid = true;
    }

    private void InitializeScenes()
    {
        var assetRoot = Path.Combine(AppContext.BaseDirectory, "Assets", "vello");
        _host = ManagedSceneHost.Create(Directory.Exists(assetRoot) ? assetRoot : null);
        _scenes = _host.Scenes;
        if (_scenes.Count > 0)
        {
            RaiseSceneChanged();
        }
    }

    private void RenderFrame()
    {
        if (_wgpuSurface is null || _wgpuRenderer is null || !_surfaceValid || _host is null || !_windowVisible)
            return;

        var elapsedMs = _frameTimer.Elapsed.TotalMilliseconds;
        _frameTimer.Restart();
        UpdateFrameStats(elapsedMs);

        if (_scenes.Count == 0)
            return;

        var elapsedSeconds = _clock.Elapsed.TotalSeconds;
        var index = Math.Clamp(_sceneIndex, 0, _scenes.Count - 1);
        var result = _host.Render(index, _scene, elapsedSeconds, true, 1, _viewTransform);

        if (result.BaseColor.HasValue)
            _baseColor = result.BaseColor.Value;

        WgpuSurfaceTexture? surfaceTexture = null;
        WgpuTextureView? textureView = null;
        try
        {
            surfaceTexture = _wgpuSurface.AcquireNextTexture();
            textureView = surfaceTexture.CreateView();

            var renderFormat = _requiresSurfaceBlit
                ? RenderFormat.Rgba8
                : _surfaceFormat switch
                {
                    WgpuTextureFormat.Rgba8Unorm or WgpuTextureFormat.Rgba8UnormSrgb => RenderFormat.Rgba8,
                    _ => RenderFormat.Bgra8,
                };

            var renderParams = new RenderParams(_surfaceConfig.Width, _surfaceConfig.Height, _baseColor)
            {
                Antialiasing = AntialiasingMode.Area,
                Format = renderFormat,
            };

            if (_requiresSurfaceBlit)
            {
                _wgpuRenderer.RenderSurface(_scene, textureView, renderParams, _surfaceFormat);
            }
            else
            {
                _wgpuRenderer.Render(_scene, textureView, renderParams);
            }

            surfaceTexture.Present();
        }
        catch (Exception ex)
        {
            RaiseStatusUpdated($"Render error: {ex.Message}");
            _surfaceValid = false;
        }
        finally
        {
            textureView?.Dispose();
            surfaceTexture?.Dispose();
        }

        // Request next frame for animated scenes
        if (_scenes[index].Animated || _statsVisible)
        {
            RequestRedraw();
        }
    }

    private void UpdateFrameStats(double elapsedMs)
    {
        if (elapsedMs <= 0 || !double.IsFinite(elapsedMs))
            return;

        _frameTimes.Enqueue(elapsedMs);
        _frameTimeSum += elapsedMs;

        while (_frameTimes.Count > MaxFrameSamples)
        {
            _frameTimeSum -= _frameTimes.Dequeue();
        }

        if (_frameTimes.Count > 0)
        {
            var avgMs = _frameTimeSum / _frameTimes.Count;
            var fps = avgMs > 0 ? 1000.0 / avgMs : 0;
            RaiseFpsUpdated(fps);
        }
    }

    public void ChangeScene(int delta)
    {
        if (_scenes.Count == 0)
            return;

        var next = (_sceneIndex + delta) % _scenes.Count;
        if (next < 0)
            next += _scenes.Count;

        if (_sceneIndex != next)
        {
            _sceneIndex = next;
            RaiseSceneChanged();
            RequestRedraw();
        }
    }

    public void ToggleStats()
    {
        _statsVisible = !_statsVisible;
        RequestRedraw();
    }

    public void ResetView()
    {
        _viewTransform = Matrix3x2.Identity;
        RequestRedraw();
    }

    private void RequestRedraw() => _needsRedraw = true;

    private void DestroySurface()
    {
        _wgpuSurface?.Dispose();
        _wgpuSurface = null;
        _surfaceValid = false;
    }

    private void DestroyAllGpuResources()
    {
        DestroySurface();
        _wgpuRenderer?.Dispose();
        _wgpuRenderer = null;
        _wgpuQueue?.Dispose();
        _wgpuQueue = null;
        _wgpuDevice?.Dispose();
        _wgpuDevice = null;
        _wgpuAdapter?.Dispose();
        _wgpuAdapter = null;
        _wgpuInstance?.Dispose();
        _wgpuInstance = null;
    }

    private static bool RequiresSurfaceBlit(WgpuTextureFormat format) => format switch
    {
        WgpuTextureFormat.Rgba8Unorm => false,
        _ => true,
    };

    private static WgpuTextureFormat NormalizeSurfaceFormat(WgpuTextureFormat format) => format switch
    {
        WgpuTextureFormat.Rgba8UnormSrgb => WgpuTextureFormat.Rgba8Unorm,
        WgpuTextureFormat.Bgra8UnormSrgb => WgpuTextureFormat.Bgra8Unorm,
        _ => format,
    };

    private void RaiseSceneChanged()
    {
        if (_scenes.Count > 0 && _sceneIndex < _scenes.Count)
        {
            SceneChanged?.Invoke(_scenes[_sceneIndex].Name, _sceneIndex, _scenes.Count);
        }
    }

    private void RaiseFpsUpdated(double fps) => FpsUpdated?.Invoke(fps);
    private void RaiseStatusUpdated(string status) => StatusUpdated?.Invoke(status);

    public void Dispose()
    {
        Stop();
        DestroyAllGpuResources();
        _host?.Dispose();
        _scene.Dispose();
    }
}
