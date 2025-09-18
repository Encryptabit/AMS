namespace Ams.Core.Align;

public static class StopwordSets
{
    public static readonly HashSet<string> EnglishPlusDomain = new(StringComparer.Ordinal)
    {
        "the","a","an","and","or","of","to","in","on","for","with","by","is","was","are","were",
        "he","she","it","they","i","you","we","his","her","their","my","your","our",
        "chapter","prelude","prologue","epilogue","contents","acknowledgements","acknowledgments",
        "one","two","three","four","five","six","seven","eight","nine","ten"
    };
}

