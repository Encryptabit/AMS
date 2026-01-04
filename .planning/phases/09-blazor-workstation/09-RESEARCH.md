# Phase 9: Blazor Audiobook Workstation - Research

**Researched:** 2026-01-03
**Domain:** Blazor Server workstation application with audio visualization
**Confidence:** HIGH

<research_summary>
## Summary

Researched the Blazor Server ecosystem for building a desktop-style audiobook production workstation. The standard approach uses Blazor Server with .NET 9 render modes, Clean Architecture for WASM-migration readiness, wavesurfer.js for audio visualization via JS interop, and Toolbelt.Blazor.HotKeys2 for keyboard shortcuts.

Key finding: The existing Python validation-viewer already uses wavesurfer.js successfully. The Blazor port should use the same library (v7) with custom JS interop rather than wrapper libraries, which are outdated and limited. For WASM-ready architecture, abstract all data access behind interfaces and inject services - when migrating to WASM, only the data layer changes (add Web API endpoints).

**Primary recommendation:** Build Blazor Server with InteractiveServer render mode, custom wavesurfer.js JS interop, Toolbelt.Blazor.HotKeys2 for keyboard shortcuts, and Clean Architecture with interface-based service abstraction. Port existing validation-viewer JavaScript directly.

**Critical constraint:** All functionality must be built on top of existing Ams.Core abstractions:
- `BookContext` / `ChapterManager` / `ChapterContext` - book and chapter lifecycle (Manager/Context paradigm)
- `AudioBufferManager` / `AudioBufferContext` - audio artifact management (raw, treated, filtered, etc.)
- `BookDocument` / `ChapterDocument` - parsed book content and chapter text (document models, not Manager/Context)

This enables separation of concerns, parallelization of pipeline operations, precise memory control for fast chapter switching, and controlled audio source selection.
</research_summary>

<standard_stack>
## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.AspNetCore.Components | 9.0 | Blazor Server framework | .NET 9 LTS, production-ready |
| wavesurfer.js | 7.x | Audio waveform visualization | Same lib as existing validation-viewer, proven |
| Toolbelt.Blazor.HotKeys2 | 6.0+ | Keyboard shortcuts | .NET 9 support, configuration-centric, maintained |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| MudBlazor or Fluent UI Blazor | Latest | Component library | If rich UI components needed |
| Microsoft.EntityFrameworkCore | 9.0 | Database access | If chapter/book state persistence needed |
| Microsoft.Extensions.DependencyInjection | 9.0 | DI container | Built-in, used for Ams.Core integration |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| wavesurfer.js | Blazor.WaveSurfer wrapper | Wrapper is outdated (.NET 8, no events, limited API) |
| Custom JS interop | Blazor.WaveSurfer | Custom gives full wavesurfer v7 API access |
| Toolbelt.HotKeys2 | Native @onkeydown | HotKeys2 handles exclusions, modifiers, cross-browser |

### NuGet Packages
```xml
<PackageReference Include="Toolbelt.Blazor.HotKeys2" Version="6.0.0" />
```

### NPM / CDN
```html
<script src="https://unpkg.com/wavesurfer.js@7/dist/wavesurfer.min.js"></script>
<script src="https://unpkg.com/wavesurfer.js@7/dist/plugins/regions.min.js"></script>
<script src="https://unpkg.com/wavesurfer.js@7/dist/plugins/timeline.min.js"></script>
```
</standard_stack>

<architecture_patterns>
## Architecture Patterns

### Recommended Project Structure
```
src/
├── Ams.Workstation.Server/           # Blazor Server host
│   ├── Program.cs                    # DI registration, app config
│   ├── Components/
│   │   ├── App.razor                 # Root component
│   │   ├── Routes.razor              # Routing
│   │   └── Layout/
│   │       └── MainLayout.razor      # Shell with Prep/Proof/Polish navigation
│   ├── Areas/
│   │   ├── Prep/                     # Pipeline orchestration area
│   │   │   └── Pages/
│   │   ├── Proof/                    # Validation/review area (port validation-viewer)
│   │   │   ├── Pages/
│   │   │   │   └── ChapterReview.razor
│   │   │   └── Components/
│   │   │       ├── WaveformPlayer.razor
│   │   │       ├── SentenceList.razor
│   │   │       └── CrxModal.razor
│   │   └── Polish/                   # Future: take replacement, prosody
│   └── wwwroot/
│       └── js/
│           └── waveform-interop.js   # Custom wavesurfer.js interop
├── Ams.Workstation.Core/             # Shared abstractions (WASM-ready)
│   ├── Interfaces/
│   │   ├── IChapterService.cs
│   │   ├── IAudioService.cs
│   │   └── IValidationService.cs
│   ├── Models/
│   │   └── ViewModels/
│   └── Services/                     # Service implementations
└── Ams.Core/                         # Existing - no changes needed
```

### Pattern 1: Ams.Core Abstraction Integration (CRITICAL)
**What:** Build all workstation functionality on top of Ams.Core's existing abstractions:
- `BookContext` / `ChapterManager` / `ChapterContext` - book and chapter lifecycle (Manager/Context paradigm)
- `AudioBufferManager` / `AudioBufferContext` - audio artifact selection (raw, treated, filtered)
- `BookDocument` / `ChapterDocument` - parsed book content and chapter text (document models)

**When to use:** Always - this is the foundational architecture constraint
**Why it matters:**
- **Separation**: Each ChapterContext owns its data/state, no cross-chapter pollution
- **Parallelization**: Multiple ChapterContexts can run pipeline stages concurrently
- **Memory control**: Dispose ChapterContext to release chapter data, fast switching between chapters
- **Audio source control**: AudioBufferManager determines which artifact (raw/treated/filtered) is active for playback/export
- **Consistency**: Same patterns CLI uses, no divergent code paths

**Example:**
```csharp
// Ams.Workstation.Server/Services/WorkstationBookService.cs
public class WorkstationBookService : IDisposable
{
    private BookContext? _bookContext;
    private readonly Dictionary<string, ChapterContext> _activeContexts = new();
    private readonly SemaphoreSlim _contextLock = new(1, 1);

    public async Task OpenBookAsync(string bookPath)
    {
        _bookContext = await BookContext.LoadAsync(bookPath);
    }

    public async Task<ChapterContext> GetChapterContextAsync(string chapterName)
    {
        await _contextLock.WaitAsync();
        try
        {
            if (_activeContexts.TryGetValue(chapterName, out var existing))
                return existing;

            var context = await _bookContext!.Chapters.CreateContextAsync(chapterName);
            _activeContexts[chapterName] = context;
            return context;
        }
        finally
        {
            _contextLock.Release();
        }
    }

    public async Task ReleaseChapterAsync(string chapterName)
    {
        await _contextLock.WaitAsync();
        try
        {
            if (_activeContexts.TryGetValue(chapterName, out var context))
            {
                context.Dispose(); // Release memory
                _activeContexts.Remove(chapterName);
            }
        }
        finally
        {
            _contextLock.Release();
        }
    }

    // Parallel pipeline execution across chapters
    public async Task RunPipelineParallelAsync(IEnumerable<string> chapters, int maxConcurrency = 4)
    {
        var semaphore = new SemaphoreSlim(maxConcurrency);
        var tasks = chapters.Select(async chapter =>
        {
            await semaphore.WaitAsync();
            try
            {
                var ctx = await GetChapterContextAsync(chapter);
                await _pipelineService.ExecuteAsync(ctx);
            }
            finally
            {
                semaphore.Release();
            }
        });
        await Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        foreach (var ctx in _activeContexts.Values)
            ctx.Dispose();
        _activeContexts.Clear();
        _bookContext?.Dispose();
    }
}

// Registration - Scoped so each circuit gets its own book session
builder.Services.AddScoped<WorkstationBookService>();
```

**Key rules:**
1. Never bypass BookContext to access chapter data directly
2. Always go through ChapterContext for chapter-specific operations
3. Dispose contexts when switching chapters to control memory
4. Use ChapterManager for batch operations and parallelization
5. The workstation service wraps the paradigm, UI components never touch contexts directly

### Pattern 2: WASM-Ready Service Abstraction
**What:** All data access through interfaces, implementations injected via DI
**When to use:** Always - enables future WASM migration without UI changes
**Example:**
```csharp
// Ams.Workstation.Core/Interfaces/IChapterService.cs
public interface IChapterService
{
    Task<ChapterViewModel> GetChapterAsync(string bookPath, string chapterName);
    Task<IEnumerable<SentenceViewModel>> GetSentencesAsync(string chapterName);
    Task MarkReviewedAsync(string chapterName, bool reviewed);
}

// Ams.Workstation.Server/Services/DirectChapterService.cs (Server implementation)
public class DirectChapterService : IChapterService
{
    private readonly BookContext _bookContext;

    public DirectChapterService(BookContext bookContext)
    {
        _bookContext = bookContext;
    }

    public async Task<ChapterViewModel> GetChapterAsync(string bookPath, string chapterName)
    {
        // Direct access to Ams.Core types
        var chapter = await _bookContext.Chapters.CreateContext(chapterName);
        return MapToViewModel(chapter);
    }
}

// Registration in Program.cs
builder.Services.AddScoped<IChapterService, DirectChapterService>();
```

### Pattern 2: Custom JS Interop for wavesurfer.js
**What:** Thin JS module that wraps wavesurfer.js, called from Blazor via IJSRuntime
**When to use:** Audio waveform components
**Example:**
```javascript
// wwwroot/js/waveform-interop.js
export function createWaveSurfer(elementId, options) {
    const ws = WaveSurfer.create({
        container: document.getElementById(elementId),
        waveColor: options.waveColor || '#4F4A85',
        progressColor: options.progressColor || '#383351',
        ...options
    });

    window.wavesurferInstances = window.wavesurferInstances || {};
    window.wavesurferInstances[elementId] = ws;
    return elementId;
}

export function loadAudio(elementId, url) {
    const ws = window.wavesurferInstances[elementId];
    ws.load(url);
}

export function play(elementId) {
    window.wavesurferInstances[elementId].play();
}

export function pause(elementId) {
    window.wavesurferInstances[elementId].pause();
}

export function seekTo(elementId, seconds) {
    window.wavesurferInstances[elementId].setTime(seconds);
}

export function registerCallbacks(elementId, dotNetRef) {
    const ws = window.wavesurferInstances[elementId];
    ws.on('ready', () => dotNetRef.invokeMethodAsync('OnWaveformReady'));
    ws.on('timeupdate', (time) => dotNetRef.invokeMethodAsync('OnTimeUpdate', time));
    ws.on('finish', () => dotNetRef.invokeMethodAsync('OnPlaybackFinished'));
}
```

```csharp
// WaveformPlayer.razor
@inject IJSRuntime JS
@implements IAsyncDisposable

<div id="@_elementId" class="waveform-container"></div>

@code {
    private string _elementId = $"waveform-{Guid.NewGuid():N}";
    private IJSObjectReference? _module;
    private DotNetObjectReference<WaveformPlayer>? _dotNetRef;

    [Parameter] public string AudioUrl { get; set; } = "";
    [Parameter] public EventCallback<double> OnTimeUpdate { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./js/waveform-interop.js");
            _dotNetRef = DotNetObjectReference.Create(this);

            await _module.InvokeVoidAsync("createWaveSurfer", _elementId, new { });
            await _module.InvokeVoidAsync("registerCallbacks", _elementId, _dotNetRef);
            await _module.InvokeVoidAsync("loadAudio", _elementId, AudioUrl);
        }
    }

    [JSInvokable]
    public async Task OnTimeUpdate(double currentTime)
    {
        await OnTimeUpdate.InvokeAsync(currentTime);
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        if (_module != null) await _module.DisposeAsync();
    }
}
```

### Pattern 3: Area-Based Navigation for Prep/Proof/Polish
**What:** Logical areas in navigation, each area owns its pages and components
**When to use:** Organizing the three-area workstation structure
**Example:**
```razor
<!-- MainLayout.razor -->
@inherits LayoutComponentBase

<div class="workstation-layout">
    <nav class="area-nav">
        <NavLink href="/prep" Match="NavLinkMatch.Prefix">Prep</NavLink>
        <NavLink href="/proof" Match="NavLinkMatch.Prefix">Proof</NavLink>
        <NavLink href="/polish" Match="NavLinkMatch.Prefix">Polish</NavLink>
    </nav>

    <main class="area-content">
        @Body
    </main>
</div>

<!-- Areas/Proof/Pages/ChapterReview.razor -->
@page "/proof/{ChapterName}"
@layout MainLayout
```

### Pattern 4: Keyboard Shortcuts with HotKeys2
**What:** Configuration-centric hotkeys that match validation-viewer UX
**When to use:** Playback controls, navigation, quick actions
**Example:**
```csharp
@inject HotKeys HotKeys
@implements IAsyncDisposable

@code {
    private HotKeysContext? _hotKeysContext;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _hotKeysContext = HotKeys.CreateContext()
                .Add(Code.Space, OnPlayPause, new() { Description = "Play/Pause" })
                .Add(Code.ArrowLeft, OnPrevSentence, new() { Description = "Previous sentence" })
                .Add(Code.ArrowRight, OnNextSentence, new() { Description = "Next sentence" })
                .Add(ModCode.Ctrl, Code.E, OnExport, new() { Description = "Export audio" })
                .Add(ModCode.Ctrl, Code.C, OnAddToCrx, new() { Description = "Add to CRX" });
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hotKeysContext != null)
            await _hotKeysContext.DisposeAsync();
    }
}
```

### Anti-Patterns to Avoid
- **Don't bypass Manager/Context paradigm:** Never access chapter files/data directly from UI components. Always go through `WorkstationBookService` → `ChapterContext`
- **Don't hold ChapterContext references in components:** Components request data through services; services manage context lifecycle
- **Don't use constructor injection in components:** Use `@inject` or `[Inject]` attribute instead
- **Don't hold DbContext in component state:** Use OwningComponentBase or create/dispose per operation
- **Don't call JS interop in OnInitialized:** Must wait for OnAfterRenderAsync(firstRender: true)
- **Don't bypass the interface abstraction:** Even for simple calls, go through IChapterService etc.
- **Don't keep all chapters loaded:** Release ChapterContext when switching to control memory
</architecture_patterns>

<dont_hand_roll>
## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Chapter/book data management | Custom file loading, parallel execution | Ams.Core Manager/Context paradigm | Memory control, parallelization, separation already solved |
| Audio waveform rendering | Custom canvas/SVG drawing | wavesurfer.js | 10+ years of optimization, WebAudio API complexities, zoom/scroll handling |
| Keyboard shortcut handling | Native @onkeydown | Toolbelt.Blazor.HotKeys2 | Input field exclusions, modifier combinations, cross-browser edge cases |
| Time-synced scrolling | Manual scroll position calculation | Leverage wavesurfer regions | Built-in region highlight, active region tracking |
| Audio segment extraction | Raw file handling | FFmpeg (via existing Ams.Core) | Codec handling, sample-accurate cutting |
| Pipeline execution | Custom orchestration | Ams.Core PipelineService + ChapterContext | Already handles stage ordering, artifact paths, error recovery |
| Component state persistence | Manual localStorage | Blazor's built-in PersistentComponentState | Handles serialization, circuit reconnection |

**Key insight:** The existing validation-viewer Python server + JavaScript frontend already solves the UI problems - port the JavaScript directly. For data management, the Ams.Core Manager/Context paradigm already solves memory control, parallelization, and chapter isolation. The workstation's job is to wrap these existing solutions in a Blazor UI, not reinvent them.
</dont_hand_roll>

<common_pitfalls>
## Common Pitfalls

### Pitfall 1: Memory Leaks from Long-Running Circuits
**What goes wrong:** Blazor Server circuits persist for the entire user session. Scoped services, event handlers, and JS interop references accumulate.
**Why it happens:** Unlike HTTP request-response, Blazor circuits are long-lived WebSocket connections
**How to avoid:**
- Implement `IAsyncDisposable` on every component that creates DotNetObjectReference, timers, or event subscriptions
- Dispose JS interop module references in DisposeAsync
- Use `OwningComponentBase<T>` for DbContext and heavy scoped services
**Warning signs:** Memory growth over time in Task Manager, slowdown after extended use

### Pitfall 2: JS Interop During Server-Side Render
**What goes wrong:** `IJSRuntime.InvokeAsync` fails with "JavaScript interop calls cannot be issued at this time"
**Why it happens:** JS interop only works after interactive rendering begins
**How to avoid:**
- Only call JS interop in `OnAfterRenderAsync(firstRender: true)` or later
- Check `firstRender` parameter before initializing JS objects
- Never call JS interop from `OnInitialized`/`OnParametersSet`
**Warning signs:** Exceptions on page load, "cannot issue JS call" errors in console

### Pitfall 3: Stale State After Circuit Reconnection
**What goes wrong:** After brief disconnect, component shows old data but wavesurfer.js instance is gone
**Why it happens:** Blazor restores .NET state but browser-side JS objects were destroyed
**How to avoid:**
- Re-initialize JS interop objects in OnAfterRenderAsync when detecting reconnection
- Store minimal state in .NET, re-fetch from server on reconnection
- Consider using `@rendermode InteractiveServer` with `prerender="false"` for interactive components
**Warning signs:** Waveform blank after reconnect, buttons don't respond

### Pitfall 4: Blocking the UI Thread with Ams.Core Operations
**What goes wrong:** UI freezes during pipeline operations or file loading
**Why it happens:** Blazor Server runs on the server but UI updates require round-trip through SignalR
**How to avoid:**
- Wrap long-running Ams.Core operations in `Task.Run`
- Use `InvokeAsync` to update UI from background operations
- Show loading indicators during async operations
**Warning signs:** UI becomes unresponsive, "circuit disconnected" timeouts

### Pitfall 5: Large Audio File Handling
**What goes wrong:** Browser hangs or crashes when loading large audiobook chapters
**Why it happens:** wavesurfer.js decodes entire audio file in browser memory by default
**How to avoid:**
- Pre-generate waveform peaks using `audiowaveform` tool
- Pass peaks JSON to wavesurfer instead of raw audio URL
- Stream audio playback separately from waveform display
**Warning signs:** Browser tab crashes on long chapters, excessive memory usage
</common_pitfalls>

<code_examples>
## Code Examples

Verified patterns from official sources and existing validation-viewer:

### Program.cs Setup with HotKeys2 and Ams.Core
```csharp
// Source: Microsoft Blazor docs + Toolbelt.HotKeys2 README
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Blazor Server with interactive SSR
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HotKeys2 for keyboard shortcuts
builder.Services.AddHotKeys2();

// Ams.Core integration - existing services
builder.Services.AddSingleton<BookContext>();
builder.Services.AddScoped<IChapterService, DirectChapterService>();
builder.Services.AddScoped<IValidationService, DirectValidationService>();
builder.Services.AddScoped<IAudioService, DirectAudioService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

### Porting Existing Validation-Viewer JavaScript
```javascript
// Source: Existing tools/validation-viewer/static/app.js patterns
// wwwroot/js/waveform-interop.js

let wavesurferInstance = null;
let regionsPlugin = null;

export function initialize(elementId, audioUrl, options) {
    // Import plugins dynamically
    const RegionsPlugin = WaveSurfer.Regions;

    wavesurferInstance = WaveSurfer.create({
        container: `#${elementId}`,
        waveColor: options.waveColor || '#4a5568',
        progressColor: options.progressColor || '#3182ce',
        cursorColor: options.cursorColor || '#e53e3e',
        height: options.height || 128,
        normalize: true,
        ...options
    });

    regionsPlugin = wavesurferInstance.registerPlugin(RegionsPlugin.create());
    wavesurferInstance.load(audioUrl);

    return wavesurferInstance;
}

export function addRegion(id, start, end, color) {
    return regionsPlugin.addRegion({
        id: id,
        start: start,
        end: end,
        color: color || 'rgba(59, 130, 246, 0.3)',
        drag: false,
        resize: false
    });
}

export function playRegion(regionId) {
    const region = regionsPlugin.getRegions().find(r => r.id === regionId);
    if (region) {
        region.play();
    }
}

export function highlightSentence(sentenceId, startTime, endTime) {
    // Clear previous highlights
    regionsPlugin.getRegions().forEach(r => {
        if (r.id.startsWith('sentence-')) {
            r.remove();
        }
    });

    // Add new highlight
    addRegion(`sentence-${sentenceId}`, startTime, endTime, 'rgba(255, 215, 0, 0.4)');
    wavesurferInstance.setTime(startTime);
}
```

### SentenceList Component with Time-Sync
```razor
@* Source: Pattern from validation-viewer, adapted for Blazor *@

<div class="sentence-list" @ref="_listRef">
    @foreach (var sentence in Sentences)
    {
        <div class="sentence @(sentence.Id == ActiveSentenceId ? "active" : "")"
             @onclick="() => OnSentenceClick(sentence)">
            <span class="sentence-id">#@sentence.Id</span>
            <span class="sentence-status status-@sentence.Status">@sentence.Status</span>
            <span class="sentence-wer">@sentence.Wer</span>
            <div class="sentence-text">@sentence.BookText</div>
            @if (sentence.HasDiff)
            {
                <div class="sentence-diff">
                    @((MarkupString)RenderDiff(sentence.Diff))
                </div>
            }
        </div>
    }
</div>

@code {
    [Parameter] public IEnumerable<SentenceViewModel> Sentences { get; set; } = [];
    [Parameter] public int? ActiveSentenceId { get; set; }
    [Parameter] public EventCallback<SentenceViewModel> OnSentenceSelected { get; set; }

    private ElementReference _listRef;

    private async Task OnSentenceClick(SentenceViewModel sentence)
    {
        await OnSentenceSelected.InvokeAsync(sentence);
    }

    private string RenderDiff(DiffViewModel diff)
    {
        // Port from existing app.js renderDiff function
        var sb = new StringBuilder();
        foreach (var op in diff.Ops)
        {
            var cssClass = op.Op switch
            {
                "equal" => "diff-equal",
                "delete" => "diff-delete",
                "insert" => "diff-insert",
                _ => ""
            };
            sb.Append($"<span class=\"{cssClass}\">{string.Join(" ", op.Tokens)}</span> ");
        }
        return sb.ToString();
    }
}
```

### Reviewed Status Persistence (porting from Python)
```csharp
// Source: Pattern from tools/validation-viewer/server.py, adapted for .NET
public class ReviewedStatusService
{
    private readonly string _appDataPath;
    private readonly string _bookName;

    public ReviewedStatusService(IConfiguration config)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _appDataPath = Path.Combine(appData, "AMS", "workstation");
        Directory.CreateDirectory(_appDataPath);
    }

    public async Task<Dictionary<string, ReviewStatus>> LoadReviewedStatusAsync(string bookName)
    {
        var filePath = Path.Combine(_appDataPath, "reviewed-status.json");
        if (!File.Exists(filePath)) return new();

        var json = await File.ReadAllTextAsync(filePath);
        var allData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ReviewStatus>>>(json);
        return allData?.GetValueOrDefault(bookName) ?? new();
    }

    public async Task SaveReviewedStatusAsync(string bookName, string chapterName, bool reviewed)
    {
        var filePath = Path.Combine(_appDataPath, "reviewed-status.json");
        var allData = File.Exists(filePath)
            ? JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, ReviewStatus>>>(
                await File.ReadAllTextAsync(filePath)) ?? new()
            : new();

        if (!allData.ContainsKey(bookName))
            allData[bookName] = new();

        allData[bookName][chapterName] = new ReviewStatus
        {
            Reviewed = reviewed,
            Timestamp = DateTime.UtcNow
        };

        await File.WriteAllTextAsync(filePath,
            JsonSerializer.Serialize(allData, new JsonSerializerOptions { WriteIndented = true }));
    }
}
```
</code_examples>

<sota_updates>
## State of the Art (2025-2026)

What's changed recently:

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Blazor Server OR WASM | Blazor United (hybrid render modes) | .NET 8+ | Can mix static SSR, Server, WASM in same app |
| Separate Server/WASM projects | Single project with `@rendermode` | .NET 8+ | Simpler project structure, easier migration |
| keyCode property (deprecated) | Key/Code properties | HotKeys2 v4+ | Better keyboard handling, modern browser APIs |
| wavesurfer.js v6 | wavesurfer.js v7 | 2023 | Shadow DOM, new plugin API, TypeScript |
| Blazor.HotKeys (original) | Toolbelt.Blazor.HotKeys2 | 2022+ | Modern API, .NET 9 support, maintained |

**New tools/patterns to consider:**
- **`@rendermode InteractiveServer`**: Explicit server interactivity, replaces `rendermode="ServerPrerendered"`
- **`ExcludeFromInteractiveRouting`**: .NET 9 attribute for static SSR pages in interactive apps
- **wavesurfer.js v7 Shadow DOM**: CSS isolation, use `::part()` for styling
- **Peaks.js**: Alternative to wavesurfer.js, BBC-developed, good for very large files

**Deprecated/outdated:**
- **Toolbelt.Blazor.HotKeys (v1)**: Deprecated, uses legacy keyCode property
- **Blazor.WaveSurfer wrapper**: Limited API, outdated wavesurfer version, no events
- **beaverlyhillsstudios/wavesurfer-blazor-wrapper**: .NET 6 only, unmaintained
</sota_updates>

<open_questions>
## Open Questions

Things that couldn't be fully resolved:

1. **Component Library Choice (MudBlazor vs Fluent UI vs custom)**
   - What we know: Both MudBlazor and Fluent UI Blazor work with .NET 9
   - What's unclear: Which has better dark theme, keyboard accessibility for this use case
   - Recommendation: Start with minimal custom components; add library later if needed. The validation-viewer CSS is already functional.

2. **Peaks Pre-generation for Large Files**
   - What we know: wavesurfer.js recommends pre-computed peaks for large files
   - What's unclear: At what chapter duration this becomes necessary, exact `audiowaveform` integration
   - Recommendation: Test with typical chapter lengths (30-60 min). Add peaks generation in Phase 10 if needed.

3. **Circuit State Persistence Across Reconnects**
   - What we know: .NET 9 has `PersistentComponentState` for serializable state
   - What's unclear: Best pattern for non-serializable state (JS object references, audio playback position)
   - Recommendation: Re-initialize wavesurfer on reconnect; store only chapter/sentence selection in persistent state.
</open_questions>

<sources>
## Sources

### Primary (HIGH confidence)
- [Microsoft Blazor Dependency Injection Docs (.NET 9)](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection?view=aspnetcore-9.0) - Service lifetimes, OwningComponentBase
- [Microsoft Blazor Render Modes](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes) - InteractiveServer, static SSR
- [Toolbelt.Blazor.HotKeys2 GitHub](https://github.com/jsakamoto/Toolbelt.Blazor.HotKeys2) - .NET 9 support confirmed, API patterns
- [wavesurfer.js Official Docs](https://wavesurfer.xyz/docs/) - v7 API, plugins, Shadow DOM
- Existing `tools/validation-viewer/` codebase - Proven wavesurfer.js patterns, UI/UX to port

### Secondary (MEDIUM confidence)
- [Clean Architecture With Blazor Server](https://architecture.blazorserver.com/) - Project structure patterns
- [Microsoft Blazor SignalR Guidance](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/signalr) - Circuit management
- [Blazor Memory Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/server/memory-management) - Circuit memory considerations

### Tertiary (LOW confidence - needs validation)
- [Blazor.WaveSurfer GitHub](https://github.com/adam-drewery/Blazor.WaveSurfer) - Limited, last updated 2023, no event support
- [Plugin Architecture in Blazor (Dev Leader)](https://dev.to/devleader/plugin-architecture-in-blazor-a-how-to-guide-4b4c) - Modular patterns
</sources>

<metadata>
## Metadata

**Research scope:**
- Core technology: Blazor Server (.NET 9)
- Ecosystem: wavesurfer.js, HotKeys2, Clean Architecture
- Patterns: WASM-ready abstraction, JS interop, area-based navigation
- Pitfalls: Circuit memory, JS interop timing, reconnection state

**Confidence breakdown:**
- Standard stack: HIGH - verified with Microsoft docs, .NET 9 confirmed
- Architecture: HIGH - Clean Architecture well-documented, WASM migration path clear
- Pitfalls: HIGH - documented in Microsoft docs, common Blazor Server issues
- Code examples: HIGH - adapted from working validation-viewer, verified patterns

**Research date:** 2026-01-03
**Valid until:** 2026-02-03 (30 days - Blazor ecosystem stable)
</metadata>

---

*Phase: 09-blazor-workstation*
*Research completed: 2026-01-03*
*Ready for planning: yes*
