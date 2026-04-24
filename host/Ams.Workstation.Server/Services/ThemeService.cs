using Microsoft.JSInterop;

namespace Ams.Workstation.Server.Services;

public enum AmsTheme
{
    Dark,
    Light,
}

/// <summary>
/// Per-circuit theme state + interop. Owns the current <see cref="AmsTheme"/>
/// and flips <c>html[data-ams-theme]</c> via <c>wwwroot/js/ams-theme.js</c>.
/// Registered as Scoped so each Blazor circuit has its own selection.
/// Default is <see cref="AmsTheme.Dark"/>, which matches the static
/// <c>data-ams-theme="dark"</c> attribute emitted by <c>Components/App.razor</c>.
/// </summary>
public sealed class ThemeService
{
    private readonly IJSRuntime _js;

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>Currently selected theme for this circuit.</summary>
    public AmsTheme Current { get; private set; } = AmsTheme.Dark;

    /// <summary>Raised when <see cref="Current"/> changes; listeners should re-render.</summary>
    public event Action<AmsTheme>? ThemeChanged;

    /// <summary>
    /// Apply <paramref name="theme"/> to <c>document.documentElement</c> via interop
    /// and broadcast the change. Safe to call from <c>OnAfterRenderAsync</c>; callers
    /// should gate on <c>firstRender</c> to avoid redundant interop on every render.
    /// </summary>
    public async Task SetThemeAsync(AmsTheme theme, CancellationToken ct = default)
    {
        Current = theme;
        await _js.InvokeVoidAsync("amsTheme.apply", ct, ToAttribute(theme)).ConfigureAwait(false);
        ThemeChanged?.Invoke(theme);
    }

    /// <summary>Toggle between dark and light and apply via interop.</summary>
    public Task ToggleAsync(CancellationToken ct = default)
        => SetThemeAsync(Current == AmsTheme.Dark ? AmsTheme.Light : AmsTheme.Dark, ct);

    private static string ToAttribute(AmsTheme theme) => theme switch
    {
        AmsTheme.Light => "light",
        _ => "dark",
    };
}
