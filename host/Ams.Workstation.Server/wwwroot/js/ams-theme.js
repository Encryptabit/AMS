// Minimal theme interop for M007/S01.
//
// ThemeService calls window.amsTheme.apply(value) where value is the string
// payload for the data-ams-theme attribute ("dark" | "light"). Keeping this
// a single attribute write lets the Sass-generated [data-ams-theme="..."]
// selectors in foundation/_root.scss flip semantic tokens atomically — no
// flash, no per-component CSS swap.

window.amsTheme = window.amsTheme || {
    apply: function (value) {
        if (!value) { return; }
        try {
            document.documentElement.setAttribute('data-ams-theme', value);
        } catch (err) {
            // Interop can fire before the DOM is ready in prerender; swallow so the
            // Blazor circuit does not tear down on first paint.
            console.warn('ams-theme: failed to apply theme', value, err);
        }
    },
    current: function () {
        return document.documentElement.getAttribute('data-ams-theme') || 'dark';
    }
};
