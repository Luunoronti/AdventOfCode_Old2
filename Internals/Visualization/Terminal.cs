using System.Runtime.InteropServices;
using System.Text;

namespace Visualization;

internal sealed class Terminal : IDisposable
{
    const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

    const uint ENABLE_PROCESSED_INPUT = 0x0001;
    const uint ENABLE_LINE_INPUT = 0x0002;
    const uint ENABLE_ECHO_INPUT = 0x0004;
    const uint ENABLE_QUICK_EDIT_MODE = 0x0040;
    const uint ENABLE_EXTENDED_FLAGS = 0x0080;
    const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;


    private readonly bool _vtOk;
    private readonly ColorMode _mode;
    private int _width, _height;

    public int Width => _width;
    public int Height => _height;
    private readonly StringBuilder _sb = new StringBuilder(64 * 1024);


    public Terminal(ColorMode mode)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        _mode = mode;
        _vtOk = EnableVT();
        _width = Console.WindowWidth;
        _height = Console.WindowHeight;
        Console.TreatControlCAsInput = true;
        Console.CursorVisible = true;
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; };
    }

    public void EnterAltScreen() => Write("\x1b[?1049h");
    public void ExitAltScreen() => Write("\x1b[?1049l");
    public void HideCursor() => Write("\x1b[?25l");
    public void ShowCursor() => Write("\x1b[?25h");
    public void Clear() => Write("\x1b[2J\x1b[H");
    public void EnableMouse(bool on)
    {
        if (on) Console.Write("\x1b[?1003h\x1b[?1006h");
        else Console.Write("\x1b[?1003l\x1b[?1006l");
    }

    public bool TryRefreshSize()
    {
        int w = Console.WindowWidth;
        int h = Console.WindowHeight;
        if (w != _width || h != _height)
        {
            _width = w; _height = h;
            return true;
        }
        return false;
    }

    // Tylko klawisze (mysz idzie przez MouseReader na stdin)
    public bool TryReadKey(InputState input)
    {
        if (!Console.KeyAvailable) return false;
        var key = Console.ReadKey(intercept: true);
        input.LastKey = key.Key;
        input.Ctrl = (key.Modifiers & ConsoleModifiers.Control) != 0;
        input.Shift = (key.Modifiers & ConsoleModifiers.Shift) != 0;
        input.Alt = (key.Modifiers & ConsoleModifiers.Alt) != 0;
        input.Esc = key.Key == ConsoleKey.Escape;
        return true;
    }

    public void Draw(CellBuffer buf)
    {
        _sb.Clear();
        _sb.Append("\x1b[H"); // Home

        for (int y = 0; y < buf.Height; y++)
        {
            for (int x = 0; x < buf.Width; x++)
            {
                var c = buf[x, y];
                if (_mode == ColorMode.TrueColor)
                {
                    _sb.Append($"\x1b[48;2;{c.Bg.R};{c.Bg.G};{c.Bg.B}m\x1b[38;2;{c.Fg.R};{c.Fg.G};{c.Fg.B}m");
                }
                else
                {
                    _sb.Append("\x1b[0m");
                }
                _sb.Append(c.Ch);
            }
            _sb.Append("\x1b[0m");
            if (y < buf.Height - 1) _sb.Append("\r\n"); // CRLF: nowa linia i powrót do kolumny 1

        }
        _sb.Append("\x1b[0m");
        Console.Write(_sb);

    }

    private static void Write(string s) => Console.Write(s);

    private static bool EnableVT()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return true;

        try
        {
            // OUT: włącz VT
            var outH = GetStdHandle(-11); // STD_OUTPUT_HANDLE
            if (!GetConsoleMode(outH, out uint outMode)) return false;
            outMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
            SetConsoleMode(outH, outMode);

            // IN: przełącz na tryb VT + "raw-ish"
            var inH = GetStdHandle(-10); // STD_INPUT_HANDLE
            if (!GetConsoleMode(inH, out uint inMode)) return false;

            // Żeby wyłączyć QUICK_EDIT, trzeba mieć EXTENDED_FLAGS ustawione.
            inMode |= ENABLE_EXTENDED_FLAGS;
            // Usuń gotowanie linii/echo/przetwarzanie, dodaj VT input.
            inMode &= ~(ENABLE_ECHO_INPUT | ENABLE_LINE_INPUT | ENABLE_PROCESSED_INPUT | ENABLE_QUICK_EDIT_MODE);
            inMode |= ENABLE_VIRTUAL_TERMINAL_INPUT;

            SetConsoleMode(inH, inMode);
            return true;
        }
        catch { return false; }
    }

    [DllImport("kernel32.dll", SetLastError = true)] static extern IntPtr GetStdHandle(int nStdHandle);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
    [DllImport("kernel32.dll", SetLastError = true)] static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    public void Dispose() { ShowCursor(); EnableMouse(false); }
}