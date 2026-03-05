---
phase: quick-9
plan: 1
type: execute
wave: 1
depends_on: []
files_modified:
  - host/Ams.Workstation.Server/wwwroot/js/keyboard-shortcuts.js
  - host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor
  - host/Ams.Workstation.Server/Components/Shared/ErrorsView.razor
  - host/Ams.Workstation.Server/Components/Shared/SentenceList.razor
  - host/Ams.Workstation.Server/Components/Shared/SentenceList.razor.css
  - host/Ams.Workstation.Server/Components/Shared/SentenceErrorCard.razor
autonomous: true
requirements: [KB-01]

must_haves:
  truths:
    - "Left/Right arrow keys switch between errors and playback views, restoring per-view position"
    - "Up/Down arrow keys navigate items within the current view (error cards or sentences)"
    - "D toggles Mark as Reviewed, E opens CRX modal on selected error"
    - "Ctrl+Right cross-navigates from errors to playback at matching sentence"
    - "Ctrl+Left cross-navigates from playback to errors at matching sentence (fallback to saved position)"
    - "Q closes CRX modal, Enter submits CRX (not in textarea), Shift+Enter is newline in textarea"
    - "View button on SentenceErrorCard switches to playback at that sentence"
    - "All shortcuts suppressed when input/textarea/select/contentEditable is focused (except modal keys)"
  artifacts:
    - path: "host/Ams.Workstation.Server/wwwroot/js/keyboard-shortcuts.js"
      provides: "JS keyboard event handler module"
    - path: "host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor"
      provides: "Position state, JSInvokable handlers, cross-nav logic"
  key_links:
    - from: "keyboard-shortcuts.js"
      to: "ChapterReview.razor"
      via: "DotNetObjectReference JSInvokable calls"
      pattern: "dotNetRef\\.invokeMethodAsync"
    - from: "ChapterReview.razor"
      to: "ErrorsView.razor"
      via: "SelectedIndex parameter + OnViewSentence callback"
    - from: "ChapterReview.razor"
      to: "SentenceList.razor"
      via: "ActiveSentenceId parameter (already exists)"
---

<objective>
Add keyboard shortcuts with cross-view navigation and per-view position memory to the Proof area ChapterReview page.

Purpose: Enable keyboard-driven QC workflow -- reviewers can navigate errors and playback without mouse, cross-reference errors with their audio context via Ctrl+arrow, and use modal shortcuts for CRX entry.

Output: JS keyboard module, updated ChapterReview with position state and JSInvokable handlers, updated child components with selection support and View button.
</objective>

<execution_context>
@/home/cari/.claude/get-shit-done/workflows/execute-plan.md
@/home/cari/.claude/get-shit-done/templates/summary.md
</execution_context>

<context>
@host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor
@host/Ams.Workstation.Server/Components/Shared/ErrorsView.razor
@host/Ams.Workstation.Server/Components/Shared/SentenceList.razor
@host/Ams.Workstation.Server/Components/Shared/SentenceList.razor.css
@host/Ams.Workstation.Server/Components/Shared/SentenceErrorCard.razor
@host/Ams.Workstation.Server/Components/Shared/CrxModal.razor
@host/Ams.Workstation.Server/Models/ProofReportModels.cs
@host/Ams.Workstation.Server/Models/SentenceViewModel.cs

<interfaces>
<!-- Key types the executor needs -->

From Models/SentenceViewModel.cs:
```csharp
public class SentenceViewModel {
    public int Id { get; set; }
    public string Text { get; set; }
    public double StartTime { get; set; }
    public double EndTime { get; set; }
    public string Status { get; set; }
    public bool HasDiff { get; set; }
}
```

From Models/ProofReportModels.cs:
```csharp
public record SentenceReport(
    int Id, string Wer, string Cer, string Status,
    string BookRange, string ScriptRange, string Timing,
    string BookText, string ScriptText, string Excerpt,
    DiffReport? Diff, double StartTime, double EndTime, int? ParagraphId);

public record ChapterReport(
    string ChapterName, string AudioPath, string ScriptPath,
    DateTime Created, ChapterStats Stats,
    IReadOnlyList<SentenceReport> Sentences,
    IReadOnlyList<ParagraphReport> Paragraphs);
```

From ChapterReview.razor existing state:
```csharp
private string _currentView = "playback";         // "playback" | "errors"
private ChapterReport? _report;                    // Has .Sentences list
private List<SentenceViewModel> _sentences = new();// Playback sentence list
private CrxModal? _crxModal;                       // .Open() / .Close()
```

From ErrorsView.razor (computed):
```csharp
// ErrorSentences is a computed IEnumerable<SentenceReport> filtered+sorted
private IEnumerable<SentenceReport> ErrorSentences => Report?.Sentences
    .Where(HasVisibleError).OrderByDescending(s => ParseWer(s.Wer)).ThenBy(s => s.Id);
```

From SentenceList.razor:
```csharp
[Parameter] public int? ActiveSentenceId { get; set; }  // Already exists
[Parameter] public EventCallback<SentenceViewModel> OnSentenceSelected { get; set; }
```

From CrxModal.razor:
```csharp
private bool _isVisible;
public void Open(string chapterName, double start, double end, int sentenceId, string excerpt, SentenceReport? sentenceReport = null);
public void Close();
```
</interfaces>
</context>

<tasks>

<task type="auto">
  <name>Task 1: JS keyboard module + ChapterReview JSInvokable wiring + position state</name>
  <files>
    host/Ams.Workstation.Server/wwwroot/js/keyboard-shortcuts.js,
    host/Ams.Workstation.Server/Components/Pages/Proof/ChapterReview.razor
  </files>
  <action>
**Create `wwwroot/js/keyboard-shortcuts.js`** as an ES module:

```javascript
// Export: init(dotNetRef), dispose()
// dotNetRef is a DotNetObjectReference<ChapterReview>
```

The module:
- On `init(dotNetRef)`: store ref, add `keydown` listener on `document`.
- On `dispose()`: remove listener, null ref.
- Keydown handler logic:
  1. Check `document.activeElement` -- if tagName is `INPUT`, `TEXTAREA`, `SELECT`, or `contentEditable === "true"`, suppress ALL shortcuts EXCEPT: when CRX modal is open, allow Q and Enter (but NOT Enter if activeElement is TEXTAREA).
  2. Check if CRX modal is open: `document.querySelector('.crx-modal-overlay.visible') !== null`.
  3. If modal open:
     - `key === 'q'` or `key === 'Q'` (no modifier): call `dotNetRef.invokeMethodAsync('OnModalClose')`, preventDefault.
     - `key === 'Enter'` and NOT shiftKey and activeElement.tagName !== 'TEXTAREA': call `dotNetRef.invokeMethodAsync('OnModalSubmit')`, preventDefault.
     - All other keys: return (don't handle).
  4. If modal NOT open (and not in input field):
     - `ArrowLeft` without Ctrl: `dotNetRef.invokeMethodAsync('OnSwitchView', 'prev')`, preventDefault.
     - `ArrowRight` without Ctrl: `dotNetRef.invokeMethodAsync('OnSwitchView', 'next')`, preventDefault.
     - `ArrowUp`: `dotNetRef.invokeMethodAsync('OnNavigateItem', 'prev')`, preventDefault.
     - `ArrowDown`: `dotNetRef.invokeMethodAsync('OnNavigateItem', 'next')`, preventDefault.
     - `ArrowRight` with Ctrl (ctrlKey or metaKey): `dotNetRef.invokeMethodAsync('OnCrossNav', 'errors-to-playback')`, preventDefault.
     - `ArrowLeft` with Ctrl (ctrlKey or metaKey): `dotNetRef.invokeMethodAsync('OnCrossNav', 'playback-to-errors')`, preventDefault.
     - `d` or `D` (no modifier): `dotNetRef.invokeMethodAsync('OnToggleReviewed')`, preventDefault.
     - `e` or `E` (no modifier): `dotNetRef.invokeMethodAsync('OnOpenCrx')`, preventDefault.

**Update `ChapterReview.razor`** @code block:

Add position state fields:
```csharp
private int _selectedErrorIndex = 0;       // Index into ErrorSentences list
private int? _selectedSentenceId;           // Sentence ID in playback view
private IJSObjectReference? _keyboardModule;
private DotNetObjectReference<ChapterReview>? _dotNetRef;
private List<SentenceReport> _errorSentencesList = new(); // Materialized for indexing
```

Add a method `MaterializeErrorSentences()` called whenever `_report` changes (in `LoadReport`) that materializes the `ErrorSentences` query into `_errorSentencesList` using the same filter+sort logic as ErrorsView. This is necessary so ChapterReview can index into the list for keyboard nav. The filter logic: `_report.Sentences.Where(HasVisibleError).OrderByDescending(s => ParseWer(s.Wer)).ThenBy(s => s.Id).ToList()`. Copy the `HasVisibleError` and `HasVisibleDiffOps` methods from ErrorsView (or make them shared -- at executor's discretion, a simple copy in ChapterReview is fine for now since the logic is compact).

In `OnParametersSetAsync`, when chapter changes (the `_lastRenderedChapter` branch), reset: `_selectedErrorIndex = 0; _selectedSentenceId = null;`

In `OnAfterRenderAsync`, on firstRender: import the keyboard-shortcuts.js module and call init:
```csharp
if (firstRender)
{
    _dotNetRef = DotNetObjectReference.Create(this);
    _keyboardModule = await JS.InvokeAsync<IJSObjectReference>("import", "./js/keyboard-shortcuts.js");
    await _keyboardModule.InvokeVoidAsync("init", _dotNetRef);
}
```

In `DisposeAsync`, dispose the keyboard module + dotNetRef:
```csharp
if (_keyboardModule is not null)
{
    try { await _keyboardModule.InvokeVoidAsync("dispose"); await _keyboardModule.DisposeAsync(); }
    catch (JSDisconnectedException) { }
}
_dotNetRef?.Dispose();
```

Add JSInvokable methods:

`[JSInvokable] public void OnSwitchView(string direction)`:
- Save current position: if `_currentView == "errors"`, position is already `_selectedErrorIndex`. If `_currentView == "playback"`, save `_selectedSentenceId` from current time-based or explicitly-set sentence.
- Toggle view: errors <-> playback (only two views, so both "prev" and "next" toggle).
- Restore position: if switching to errors, use `_selectedErrorIndex`. If switching to playback, use `_selectedSentenceId`.
- Call `StateHasChanged()`.

`[JSInvokable] public void OnNavigateItem(string direction)`:
- If `_currentView == "errors"`: increment/decrement `_selectedErrorIndex` within bounds of `_errorSentencesList.Count`. Clamp to 0..Count-1.
- If `_currentView == "playback"`: find current sentence in `_sentences` by `_selectedSentenceId` (or by `_currentTime` if null), move to prev/next in list, set `_selectedSentenceId`.
- Call `StateHasChanged()`.

`[JSInvokable] public void OnCrossNav(string direction)`:
- `"errors-to-playback"`: Get `_errorSentencesList[_selectedErrorIndex]`, get its `Id`. Switch to playback view. Find matching `SentenceViewModel` in `_sentences` by Id. Set `_selectedSentenceId` to that Id.
- `"playback-to-errors"`: Get current sentence Id (from `_selectedSentenceId` or time-based lookup). Switch to errors view. Find index in `_errorSentencesList` where `s.Id == sentenceId`. If found, set `_selectedErrorIndex` to that index. If NOT found, keep existing `_selectedErrorIndex` (saved position fallback).
- Call `StateHasChanged()`.

`[JSInvokable] public void OnToggleReviewed()`:
- Call existing `ToggleReviewed()`.

`[JSInvokable] public void OnOpenCrx()`:
- Only if `_currentView == "errors"` and `_errorSentencesList.Count > 0`.
- Get sentence at `_selectedErrorIndex`, call `HandleCrxFromErrors(sentence)`.

`[JSInvokable] public void OnModalClose()`:
- Call `_crxModal?.Close()`. `StateHasChanged()`.

`[JSInvokable] public async Task OnModalSubmit()`:
- If `_crxModal` is visible, invoke its submit. The simplest approach: add a public `SubmitAsync()` method to CrxModal that calls the existing private `Submit()`. Then call `await _crxModal.SubmitAsync()`.

Update `SwitchView(string view)` to save position before switching: if leaving errors, `_selectedErrorIndex` is already current. If leaving playback, snapshot `_selectedSentenceId` from current playback position (find sentence containing `_currentTime`).

Pass `_selectedSentenceId` through to SentenceList's existing `ActiveSentenceId` parameter (it already accepts `int? ActiveSentenceId` but ChapterReview doesn't currently pass it). Add it:
```razor
<SentenceList
    Sentences="@_sentences"
    CurrentTime="@_currentTime"
    ActiveSentenceId="@_selectedSentenceId"
    OnSentenceSelected="HandleSentenceSelected" />
```

Update `HandleSentenceSelected` to also set `_selectedSentenceId = sentence.Id`.
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet build host/Ams.Workstation.Server/Ams.Workstation.Server.csproj --no-restore 2>&1 | tail -5</automated>
  </verify>
  <done>
    keyboard-shortcuts.js exists with init/dispose exports and full keydown handler. ChapterReview has position state fields, all JSInvokable methods, keyboard module lifecycle in OnAfterRenderAsync/DisposeAsync, and ActiveSentenceId wired to SentenceList.
  </done>
</task>

<task type="auto">
  <name>Task 2: Child component selection support + View button + CrxModal submit</name>
  <files>
    host/Ams.Workstation.Server/Components/Shared/ErrorsView.razor,
    host/Ams.Workstation.Server/Components/Shared/SentenceErrorCard.razor,
    host/Ams.Workstation.Server/Components/Shared/SentenceList.razor,
    host/Ams.Workstation.Server/Components/Shared/SentenceList.razor.css,
    host/Ams.Workstation.Server/Components/Shared/CrxModal.razor
  </files>
  <action>
**ErrorsView.razor:**

Add parameters:
```csharp
[Parameter] public int SelectedIndex { get; set; }
[Parameter] public EventCallback<SentenceReport> OnViewSentence { get; set; }
```

In the `@foreach` loop over `ErrorSentences`, change to a `@for` loop with index so we can pass selected state. Since `ErrorSentences` is `IEnumerable`, materialize it: `var errorList = ErrorSentences.ToList();` at the top of the render block (inside the `else` after null check). Use `@for (var i = 0; i < errorList.Count; i++)` with a local capture `var idx = i; var sentence = errorList[idx];`.

Pass `IsSelected="idx == SelectedIndex"` and `OnView="() => OnViewSentence.InvokeAsync(sentence)"` to each `SentenceErrorCard`.

Add an `id` attribute to each card wrapper div for scroll-to targeting: wrap each `<SentenceErrorCard>` in a `<div id="error-card-@sentence.Id">`. (Or add the id directly to SentenceErrorCard's root element.)

Add scroll-to-selected logic: inject `IJSRuntime JS`. In `OnAfterRenderAsync`, if `SelectedIndex` is within bounds, scroll `error-card-{errorList[SelectedIndex].Id}` into view (smooth, center). Track `_lastScrolledIndex` to avoid re-scrolling. Use the same scrollIntoView pattern as SentenceList.

**SentenceErrorCard.razor:**

Add parameters:
```csharp
[Parameter] public bool IsSelected { get; set; }
[Parameter] public EventCallback OnView { get; set; }
```

Update root `<BitCard>` Style to include a highlight when `IsSelected`: add `outline: 2px solid var(--bit-clr-pri);` when `IsSelected` is true. Modify `GetBorderStyle()` (or add inline style) to append the outline.

Add a "View" button in the action buttons row, between "Play Audio Segment" and "Export Audio":
```razor
<BitButton Variant="BitVariant.Outline" Size="BitSize.Medium"
           Color="BitColor.Tertiary"
           OnClick="() => OnView.InvokeAsync()">
    View in Playback
</BitButton>
```

**SentenceList.razor:**

The `ActiveSentenceId` parameter already exists and `GetCurrentSentence()` already respects it. The `playing` CSS class already highlights active sentences. Add a `selected` CSS class that provides a distinct highlight for keyboard-selected (non-playing) sentences:

In `GetSentenceClasses`, add: if `ActiveSentenceId.HasValue && sentence.Id == ActiveSentenceId.Value && !IsPlaying(sentence)`, add class `"selected"`.

Actually, since `IsPlaying` already returns true when `ActiveSentenceId` matches, the existing `playing` class will highlight the keyboard-selected sentence. This is correct behavior -- the `playing` class provides the visual indicator for the active sentence regardless of how it was selected.

The existing `OnAfterRenderAsync` already scrolls to the current sentence. It uses `_lastScrolledToId` to avoid re-scrolling. This will work correctly with keyboard nav since `ActiveSentenceId` changes will trigger re-render and scroll.

**SentenceList.razor.css:**

Add a `selected` class for keyboard-selected state (distinct from `playing` which is time-based). Actually, since `ActiveSentenceId` feeds into `GetCurrentSentence()` which feeds `IsPlaying()`, the `playing` class already handles this. No CSS changes needed unless we want a distinct style. Keep it simple -- the existing `playing` style (blue background + left border) is appropriate for keyboard selection too.

**CrxModal.razor:**

Add a public async method that ChapterReview can call for keyboard submit:
```csharp
public async Task SubmitAsync()
{
    if (_isVisible && !_submitting)
        await Submit();
}
```

This exposes the existing `Submit()` logic without duplicating it.

**Wire it all in ChapterReview.razor:**

Update the `<ErrorsView>` usage to pass new parameters:
```razor
<ErrorsView Report="_report" ChapterPatterns="_chapterPatterns"
            IgnoredKeys="IgnoredPatternsService.GetIgnoredKeys()"
            SelectedIndex="_selectedErrorIndex"
            OnPlaySentence="HandlePlayFromErrors"
            OnExportSentence="HandleExportFromErrors"
            OnCrxSentence="HandleCrxFromErrors"
            OnIgnoreSentence="HandleIgnoreFromErrors"
            OnTogglePatternIgnore="HandleTogglePatternIgnore"
            OnViewSentence="HandleViewFromErrors" />
```

Add `HandleViewFromErrors(SentenceReport sentence)` in ChapterReview:
- Switch to playback view.
- Find matching `SentenceViewModel` in `_sentences` by `sentence.Id`.
- Set `_selectedSentenceId = sentence.Id`.
- `StateHasChanged()`.

This is identical to the Ctrl+Right cross-nav but triggered by a specific sentence rather than the currently selected one.
  </action>
  <verify>
    <automated>cd /home/cari/repos/AMS && dotnet build host/Ams.Workstation.Server/Ams.Workstation.Server.csproj --no-restore 2>&1 | tail -5</automated>
  </verify>
  <done>
    ErrorsView passes SelectedIndex and OnViewSentence to cards. SentenceErrorCard shows "View in Playback" button and selected outline. CrxModal exposes public SubmitAsync. ChapterReview wires OnViewSentence to cross-nav handler. All keyboard shortcuts functional end-to-end through JS -> JSInvokable -> C# state -> child component parameters.
  </done>
</task>

</tasks>

<verification>
1. `dotnet build host/Ams.Workstation.Server/Ams.Workstation.Server.csproj` compiles without errors.
2. Manual verification: run workstation, navigate to a chapter's proof page, verify:
   - Left/Right arrows switch views.
   - Up/Down arrows navigate items (error cards in errors view, sentences in playback view).
   - D toggles reviewed status.
   - E opens CRX modal on selected error.
   - Ctrl+Right from errors view jumps to matching sentence in playback.
   - Ctrl+Left from playback jumps to matching error (or falls back to saved position).
   - Q closes CRX modal, Enter submits.
   - "View in Playback" button on error cards works.
   - Shortcuts are suppressed when typing in inputs/textareas.
</verification>

<success_criteria>
- All 12 keyboard shortcuts from the requirements table are functional.
- Per-view position memory works: switching away and back restores position.
- Cross-view navigation maps sentence IDs correctly between views.
- "View in Playback" button on SentenceErrorCard triggers cross-nav.
- No shortcuts fire when user is typing in input fields (except modal Q/Enter).
- Project compiles cleanly.
</success_criteria>

<output>
After completion, create `.planning/quick/9-add-workstation-keyboard-shortcuts-chapt/9-SUMMARY.md`
</output>
