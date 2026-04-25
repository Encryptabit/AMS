namespace Ams.Tests.Workstation.Shell;

public sealed class StageShellMobileContractTests
{
    private const string AppRelativePath = "host/Ams.Workstation.Server/Components/App.razor";
    private const string HeaderControlsRelativePath = "host/Ams.Workstation.Server/Components/Layout/HeaderControls.razor";
    private const string HeaderControlsCssRelativePath = "host/Ams.Workstation.Server/Components/Layout/HeaderControls.razor.css";
    private const string MainLayoutRelativePath = "host/Ams.Workstation.Server/Components/Layout/MainLayout.razor";
    private const string MainLayoutCssRelativePath = "host/Ams.Workstation.Server/Components/Layout/MainLayout.razor.css";
    private const string StageModuleRailRelativePath = "host/Ams.Workstation.Server/Components/Layout/StageModuleRail.razor";
    private const string StageModuleRailCssRelativePath = "host/Ams.Workstation.Server/Components/Layout/StageModuleRail.razor.css";

    [Fact]
    public void App_DeclaresMobileViewportContract()
    {
        var appSource = ReadRepoFile(AppRelativePath);

        AssertContains(
            appSource,
            AppRelativePath,
            "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />",
            "mobile viewport meta contract");
    }

    [Fact]
    public void ShellMarkup_ExposesMobileOverflowAndModuleRailContractAnchors()
    {
        var headerSource = ReadRepoFile(HeaderControlsRelativePath);
        var layoutSource = ReadRepoFile(MainLayoutRelativePath);
        var railSource = ReadRepoFile(StageModuleRailRelativePath);

        AssertContains(headerSource, HeaderControlsRelativePath, "data-ams-mobile-overflow-state=", "header overflow state marker");
        AssertContains(headerSource, HeaderControlsRelativePath, "data-ams-mobile-overflow-open=", "header overflow open marker");
        AssertContains(headerSource, HeaderControlsRelativePath, "data-ams-header-control=\"mobile-overflow-trigger\"", "header overflow trigger anchor");
        AssertContains(headerSource, HeaderControlsRelativePath, "data-ams-header-control=\"mobile-overflow-panel\"", "header overflow panel anchor");
        AssertContains(headerSource, HeaderControlsRelativePath, "data-ams-header-control=\"directory-actions-mobile\"", "header overflow action host anchor");

        AssertContains(layoutSource, MainLayoutRelativePath, "data-ams-shell-region=\"header-controls\"", "header controls shell-region anchor");
        AssertContains(layoutSource, MainLayoutRelativePath, "data-ams-mobile-overflow-contract=\"secondary-actions\"", "header overflow contract anchor");
        AssertContains(layoutSource, MainLayoutRelativePath, "data-ams-mobile-module-rail-state=", "layout mobile rail state marker");
        AssertContains(layoutSource, MainLayoutRelativePath, "data-ams-header-control=\"module-rail-toggle\"", "module rail toggle anchor");
        AssertContains(layoutSource, MainLayoutRelativePath, "data-ams-mobile-module-rail-open=", "module rail toggle-open marker");
        AssertContains(layoutSource, MainLayoutRelativePath, "data-ams-mobile-module-rail=", "module rail drawer state marker");
        AssertContains(layoutSource, MainLayoutRelativePath, "data-ams-mobile-module-rail-overlay=\"visible\"", "module rail overlay visibility marker");
        AssertContains(layoutSource, MainLayoutRelativePath, "workstation-sidebar--mobile-open", "mobile-open sidebar class");
        AssertContains(layoutSource, MainLayoutRelativePath, "workstation-sidebar--mobile-closed", "mobile-closed sidebar class");

        AssertContains(railSource, StageModuleRailRelativePath, "data-ams-mobile-module-rail-contract=\"collapsible\"", "collapsible rail contract marker");
        AssertContains(railSource, StageModuleRailRelativePath, "data-ams-mobile-module-rail-link=\"module\"", "module link rail anchor");
    }

    [Fact]
    public void ShellStyles_EncodeTouchTargetAndRailVisibilityContracts()
    {
        var headerCss = ReadRepoFile(HeaderControlsCssRelativePath);
        var layoutCss = ReadRepoFile(MainLayoutCssRelativePath);
        var railCss = ReadRepoFile(StageModuleRailCssRelativePath);

        AssertContains(headerCss, HeaderControlsCssRelativePath, "@media (max-width: 768px)", "mobile header breakpoint");
        AssertContains(headerCss, HeaderControlsCssRelativePath, ".header-nav ::deep button", "stage-nav touch-target selector");
        AssertContains(headerCss, HeaderControlsCssRelativePath, ".header-mobile-overflow-trigger ::deep button", "overflow-trigger touch-target selector");
        AssertContains(headerCss, HeaderControlsCssRelativePath, ".header-actions ::deep button", "header actions touch-target selector");
        AssertContains(headerCss, HeaderControlsCssRelativePath, ".header-options ::deep label", "header options touch-target selector");
        AssertContains(headerCss, HeaderControlsCssRelativePath, ".header-row ::deep input", "input touch-target selector");
        AssertContains(headerCss, HeaderControlsCssRelativePath, "min-height: 44px;", "44px minimum touch-target contract");
        AssertContains(headerCss, HeaderControlsCssRelativePath, "font-size: 16px;", "ios zoom prevention contract");

        AssertContains(layoutCss, MainLayoutCssRelativePath, "@media (max-width: 768px)", "mobile shell breakpoint");
        AssertContains(layoutCss, MainLayoutCssRelativePath, ".workstation-module-rail-toggle", "module rail toggle style block");
        AssertContains(layoutCss, MainLayoutCssRelativePath, ".workstation-module-rail-toggle ::deep button", "module rail toggle touch-target selector");
        AssertContains(layoutCss, MainLayoutCssRelativePath, "transform: translateX(-106%);", "drawer hidden-by-default transform");
        AssertContains(layoutCss, MainLayoutCssRelativePath, "visibility: hidden;", "drawer hidden visibility contract");
        AssertContains(layoutCss, MainLayoutCssRelativePath, ".workstation-sidebar.workstation-sidebar--mobile-open", "drawer open-state selector");
        AssertContains(layoutCss, MainLayoutCssRelativePath, ".workstation-module-rail-overlay", "drawer overlay style block");

        AssertContains(railCss, StageModuleRailCssRelativePath, "@media (max-width: 768px)", "mobile rail breakpoint");
        AssertContains(railCss, StageModuleRailCssRelativePath, ".stage-module-rail__module-link", "module link selector");
        AssertContains(railCss, StageModuleRailCssRelativePath, ".stage-module-rail::deep .bit-btn", "module rail button selector");
        AssertContains(railCss, StageModuleRailCssRelativePath, "min-height: 44px;", "module link minimum touch-target size");
    }

    private static void AssertContains(string source, string relativePath, string anchor, string description)
    {
        Assert.True(
            source.Contains(anchor, StringComparison.Ordinal),
            $"Missing mobile shell contract anchor '{description}' in '{relativePath}'. Expected snippet: {anchor}");
    }

    private static string ReadRepoFile(string relativePath)
    {
        var repoRoot = FindRepoRoot();
        var fullPath = Path.Combine(repoRoot, relativePath);

        Assert.True(
            File.Exists(fullPath),
            $"Required mobile shell contract file is missing: relative='{relativePath}', full='{fullPath}'.");

        return File.ReadAllText(fullPath);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "host", "Ams.sln"))
                && Directory.Exists(Path.Combine(current.FullName, "host")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root containing host/Ams.sln.");
    }
}
