namespace Ams.Cli.Repl;

/// <summary>
/// Minimal cross-platform line editor using Console.ReadKey.
/// Provides command history (Up/Down), caret movement (Left/Right/Home/End),
/// and inline editing (Backspace/Delete) that work identically on Windows and Linux.
/// </summary>
internal sealed class ReplLineEditor
{
    private readonly List<string> _history = new();
    private int _historyIndex;
    private int _maxHistory;

    public ReplLineEditor(int maxHistory = 200)
    {
        _maxHistory = maxHistory;
    }

    /// <summary>
    /// Reads a line of input with full editing support.
    /// Returns null on Ctrl+C / Ctrl+D / end-of-stream.
    /// </summary>
    public string? ReadLine(string prompt)
    {
        Console.Write(prompt);

        var buffer = new List<char>();
        var cursor = 0;
        _historyIndex = _history.Count;
        string? savedInput = null;

        while (true)
        {
            var key = Console.ReadKey(intercept: true);

            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    var line = new string(buffer.ToArray());
                    AddHistory(line);
                    return line;

                case ConsoleKey.Backspace:
                    if (cursor > 0)
                    {
                        buffer.RemoveAt(cursor - 1);
                        cursor--;
                        Redraw(prompt, buffer, cursor);
                    }
                    break;

                case ConsoleKey.Delete:
                    if (cursor < buffer.Count)
                    {
                        buffer.RemoveAt(cursor);
                        Redraw(prompt, buffer, cursor);
                    }
                    break;

                case ConsoleKey.LeftArrow:
                    if (cursor > 0)
                    {
                        cursor--;
                        SetCursorPosition(prompt, cursor);
                    }
                    break;

                case ConsoleKey.RightArrow:
                    if (cursor < buffer.Count)
                    {
                        cursor++;
                        SetCursorPosition(prompt, cursor);
                    }
                    break;

                case ConsoleKey.Home:
                    cursor = 0;
                    SetCursorPosition(prompt, cursor);
                    break;

                case ConsoleKey.End:
                    cursor = buffer.Count;
                    SetCursorPosition(prompt, cursor);
                    break;

                case ConsoleKey.UpArrow:
                    if (_historyIndex > 0)
                    {
                        if (_historyIndex == _history.Count)
                            savedInput = new string(buffer.ToArray());
                        _historyIndex--;
                        SetBuffer(buffer, _history[_historyIndex], out cursor);
                        Redraw(prompt, buffer, cursor);
                    }
                    break;

                case ConsoleKey.DownArrow:
                    if (_historyIndex < _history.Count)
                    {
                        _historyIndex++;
                        var text = _historyIndex < _history.Count
                            ? _history[_historyIndex]
                            : savedInput ?? "";
                        SetBuffer(buffer, text, out cursor);
                        Redraw(prompt, buffer, cursor);
                    }
                    break;

                case ConsoleKey.Escape:
                    buffer.Clear();
                    cursor = 0;
                    Redraw(prompt, buffer, cursor);
                    break;

                default:
                    // Ctrl+C / Ctrl+D → signal exit
                    if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        Console.WriteLine();
                        return null;
                    }
                    if (key.Key == ConsoleKey.D && key.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        Console.WriteLine();
                        return null;
                    }

                    // Printable character
                    if (key.KeyChar >= ' ')
                    {
                        buffer.Insert(cursor, key.KeyChar);
                        cursor++;
                        Redraw(prompt, buffer, cursor);
                    }
                    break;
            }
        }
    }

    private void AddHistory(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;

        // Don't add duplicates of the most recent entry
        if (_history.Count > 0 && _history[^1] == line)
            return;

        _history.Add(line);
        if (_history.Count > _maxHistory)
            _history.RemoveAt(0);
    }

    private static void SetBuffer(List<char> buffer, string text, out int cursor)
    {
        buffer.Clear();
        buffer.AddRange(text);
        cursor = buffer.Count;
    }

    private static void Redraw(string prompt, List<char> buffer, int cursor)
    {
        // Move to start of line, clear, rewrite
        Console.Write('\r');
        Console.Write(prompt);
        Console.Write(new string(buffer.ToArray()));
        // Clear any trailing characters from previous longer content
        Console.Write("  ");
        // Position cursor
        SetCursorPosition(prompt, cursor);
    }

    private static void SetCursorPosition(string prompt, int cursor)
    {
        var targetCol = prompt.Length + cursor;
        try
        {
            Console.CursorLeft = targetCol;
        }
        catch (IOException)
        {
            // Ignore if terminal doesn't support cursor positioning
        }
    }
}
