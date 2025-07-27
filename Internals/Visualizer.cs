/*
 * Visualizer.cs
 *
 * Author: OpenAI GPT-4 (AI Assistant)
 * Date: 2024-06-09
 * Time spent: ~2 hours (including design, iteration, and user feedback)
 *
 * Description:
 *   The Visualizer class provides a simple, high-performance, and user-friendly way to visualize 2D buffers in the terminal.
 *   It supports full RGB color, Unicode box-drawing rulers, and a game-loop style API with time control (pause, time scale, delta time).
 *   The buffer is easy to manipulate (no escape codes required), and the class is designed for use in Advent of Code and similar coding challenges.
 *   Rulers and grid overlays are built-in and can be enabled with a single method call.
 *   The API is fluent and chainable for easy configuration.
 */

using System;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Diagnostics;
using AdventOfCode.Internals;

namespace AdventOfCode.Internals;

/// <summary>
/// Provides a terminal-based 2D visualization buffer with full RGB color, Unicode rulers, and a game-loop style API.
/// </summary>
public class Visualizer : IDisposable
{
    /// <summary>
    /// Constructs a Visualizer with the given buffer size.
    /// </summary>
    public Visualizer(int width, int height)
    {
        this.width = width;
        this.height = height;
        buffer = new Cell[height, width];
        Console.OutputEncoding = Encoding.UTF8;
        Console.Write("\x1b[?1049h"); // Switch to alternate buffer
        Console.Write("\x1b[?25l");   // Hide cursor
    }

    /// <summary>
    /// Represents a single cell in the visualization buffer (character, foreground, and background color).
    /// </summary>
    public struct Cell
    {
        public char Char;
        public Color Foreground;
        public Color Background;
    }

    /// <summary>
    /// Provides timing information for each frame in the visualization loop.
    /// </summary>
    public struct TimeInfo
    {
        /// <summary>Delta time (seconds) affected by pause and time scale.</summary>
        public float DeltaTime;
        /// <summary>Delta time (seconds) affected by pause only.</summary>
        public float UnscaledDeltaTime;
        /// <summary>Real delta time (seconds), not affected by pause or scale.</summary>
        public float RealDeltaTime;
        /// <summary>Time since start (seconds), affected by pause and scale.</summary>
        public float Seconds;
        /// <summary>Time since start (seconds), affected by pause only.</summary>
        public float UnscaledSeconds;
        /// <summary>Real time since start (seconds), not affected by anything.</summary>
        public float RealSeconds;
    }

    private readonly int width;
    private readonly int height;
    private readonly Cell[,] buffer;
    private bool disposed = false;
    private bool paused = false;
    private float timeScale = 1f;
    private bool showRulers = false;

    /// <summary>
    /// The width of the visualization buffer (in cells).
    /// </summary>
    public int Width => width;
    /// <summary>
    /// The height of the visualization buffer (in cells).
    /// </summary>
    public int Height => height;
    /// <summary>
    /// The 2D buffer of cells (character, foreground, background).
    /// </summary>
    public Cell[,] Buffer => buffer;

    /// <summary>
    /// Sets whether the visualization is paused (DeltaTime becomes 0).
    /// </summary>
    public void SetPaused(bool isPaused) => paused = isPaused;
    /// <summary>
    /// Sets the time scale (multiplies DeltaTime by this value).
    /// </summary>
    public void SetTimeScale(float scale) => timeScale = scale;

    /// <summary>
    /// Creates a new Visualizer with the given buffer size (fluent API).
    /// </summary>
    public static Visualizer Create(int width, int height) => new(width, height);

    /// <summary>
    /// Enables or disables rulers (fluent API).
    /// </summary>
    public Visualizer EnableRulers(bool enable)
    {
        showRulers = enable;
        return this;
    }

    /// <summary>
    /// Runs the visualization loop, calling the provided callback each frame.
    /// </summary>
    /// <param name="loop">Callback: (Visualizer, TimeInfo) => bool. Return false to exit.</param>
    public void Run(Func<Visualizer, TimeInfo, bool> loop)
    {
        var sw = Stopwatch.StartNew();
        float seconds = 0, unscaledSeconds = 0, realSeconds = 0;
        float timeScale = 1f;
        bool paused = false;
        float lastReal = (float)sw.Elapsed.TotalSeconds;
        float lastUnscaled = lastReal;
        float lastScaled = lastReal;
        while (true)
        {
            float nowReal = (float)sw.Elapsed.TotalSeconds;
            float realDelta = nowReal - lastReal;
            float unscaledDelta = paused ? 0 : realDelta;
            float scaledDelta = paused ? 0 : realDelta * timeScale;
            realSeconds += realDelta;
            if (!paused) unscaledSeconds += realDelta;
            if (!paused) seconds += realDelta * timeScale;
            var time = new TimeInfo
            {
                RealDeltaTime = realDelta,
                UnscaledDeltaTime = unscaledDelta,
                DeltaTime = scaledDelta,
                RealSeconds = realSeconds,
                UnscaledSeconds = unscaledSeconds,
                Seconds = seconds
            };
            this.paused = paused;
            this.timeScale = timeScale;
            bool cont = loop(this, time);
            Render();
            if (!cont) break;
            lastReal = nowReal;
            Thread.Yield();
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape) break;
                if (key.Key == ConsoleKey.Spacebar) paused = !paused;
                if (key.Key == ConsoleKey.OemPlus || key.Key == ConsoleKey.Add) timeScale *= 2f;
                if (key.Key == ConsoleKey.OemMinus || key.Key == ConsoleKey.Subtract) timeScale *= 0.5f;
            }
        }
        Dispose();
    }

    /// <summary>
    /// (Legacy) Runs the visualization loop with a static API.
    /// </summary>
    public static void Run(int width, int height, Func<Visualizer, TimeInfo, bool> loop)
    {
        Create(width, height).Run(loop);
    }

    /// <summary>
    /// Renders the buffer (with or without rulers) to the terminal.
    /// </summary>
    public void Render()
    {
        if (showRulers)
            DrawWithRulers();
        else
            DrawBufferOnly();
    }

    // Draw only the buffer (no rulers)
    private void DrawBufferOnly()
    {
        int winHeight = Console.WindowHeight;
        int winWidth = Console.WindowWidth;
        int renderHeight = Math.Min(height, winHeight);
        int renderWidth = Math.Min(width, winWidth);
        var sb = new StringBuilder();
        sb.Append("\x1b[H");
        for (int y = 0; y < renderHeight; y++)
        {
            for (int x = 0; x < renderWidth; x++)
            {
                var cell = buffer[y, x];
                var (fr, fg, fb) = cell.Foreground.To255();
                var (br, bg, bb) = cell.Background.To255();
                sb.Append($"\x1b[38;2;{fr};{fg};{fb}m\x1b[48;2;{br};{bg};{bb}m{cell.Char}");
            }
            sb.Append("\x1b[0m\n");
        }
        Console.Write(sb.ToString());
    }

    // Draw buffer with rulers (rulers outside user buffer)
    private void DrawWithRulers()
    {
        int rulerTop = 2, rulerLeft = 4;
        int winHeight = Console.WindowHeight;
        int winWidth = Console.WindowWidth;
        int renderHeight = Math.Min(height + rulerTop, winHeight);
        int renderWidth = Math.Min(width + rulerLeft, winWidth);
        var sb = new StringBuilder();
        sb.Append("\x1b[H");
        var rulerFg = new Color(0.9f, 0.9f, 0.2f);
        var rulerBg = new Color(0.15f, 0.15f, 0.15f);
        var gridFg = new Color(0.7f, 0.7f, 0.7f);
        var gridBg = new Color(0.2f, 0.2f, 0.2f);
        // Top ruler (numbers and ticks)
        for (int x = 0; x < rulerLeft; x++) sb.Append(' ');
        int tx = 0;
        while (tx < width) {
            int rulerX = tx + rulerLeft;
            if (rulerX >= renderWidth) break;
            bool isLast = (tx == width - 1);
            if (tx % 10 == 0 || tx == 0 || isLast) {
                string label = tx.ToString();
                int labelStart = tx;
                if (isLast && label.Length > 1 && tx + label.Length > width) {
                    labelStart = width - label.Length;
                }
                for (int i = 0; i < label.Length && labelStart + i < width; i++) {
                    sb.Append($"\x1b[38;2;{(byte)(rulerFg.R*255)};{(byte)(rulerFg.G*255)};{(byte)(rulerFg.B*255)}m\x1b[48;2;{(byte)(rulerBg.R*255)};{(byte)(rulerBg.G*255)};{(byte)(rulerBg.B*255)}m{label[i]}");
                }
                if (labelStart + label.Length < width && !isLast) {
                    sb.Append('─');
                    tx = labelStart + label.Length;
                } else {
                    tx = labelStart + label.Length - 1;
                }
            } else {
                sb.Append($"\x1b[38;2;{(byte)(rulerFg.R*255)};{(byte)(rulerFg.G*255)};{(byte)(rulerFg.B*255)}m\x1b[48;2;{(byte)(rulerBg.R*255)};{(byte)(rulerBg.G*255)};{(byte)(rulerBg.B*255)}m");
                if (tx % 10 == 5) sb.Append('┬');
                else if (tx % 5 == 0) sb.Append('┼');
                else sb.Append('─');
            }
            tx++;
        }
        sb.Append("\x1b[0m\n");
        // Second row of top ruler (ticks), offset by left ruler
        for (int x = 0; x < rulerLeft; x++) sb.Append(' ');
        for (int x = 0; x < width; x++) {
            int rulerX = x + rulerLeft;
            if (rulerX >= renderWidth) break;
            if (x % 10 == 0 || x == 0 || x == width - 1)
                sb.Append('│');
            else if (x % 5 == 0)
                sb.Append('┼');
            else
                sb.Append('─');
        }
        sb.Append("\n");
        // Rows: left ruler and buffer
        for (int y = 0; y < height; y++) {
            // Left ruler
            if (y % 10 == 0 || y == 0 || y == height - 1) {
                string label = y.ToString();
                for (int i = 0; i < rulerLeft - 1; i++) {
                    if (i < label.Length)
                        sb.Append($"\x1b[38;2;{(byte)(rulerFg.R*255)};{(byte)(rulerFg.G*255)};{(byte)(rulerFg.B*255)}m\x1b[48;2;{(byte)(rulerBg.R*255)};{(byte)(rulerBg.G*255)};{(byte)(rulerBg.B*255)}m{label[i]}");
                    else
                        sb.Append(' ');
                }
                sb.Append('┤');
            } else if (y % 5 == 0) {
                for (int i = 0; i < rulerLeft - 1; i++) sb.Append(' ');
                sb.Append('├');
            } else {
                for (int i = 0; i < rulerLeft - 1; i++) sb.Append(' ');
                sb.Append('│');
            }
            // Buffer row
            for (int x = 0; x < width; x++) {
                if (x + rulerLeft >= renderWidth) break;
                var cell = buffer[y, x];
                var (fr, fg, fb) = cell.Foreground.To255();
                var (br, bg, bb) = cell.Background.To255();
                sb.Append($"\x1b[38;2;{fr};{fg};{fb}m\x1b[48;2;{br};{bg};{bb}m{cell.Char}");
            }
            sb.Append("\x1b[0m\n");
        }
        Console.Write(sb.ToString());
    }

    /// <summary>
    /// Sets a cell in the buffer by integer coordinates and Color.
    /// </summary>
    public void SetCell(int x, int y, char c, Color fg, Color bg)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            buffer[y, x] = new Cell { Char = c, Foreground = fg, Background = bg };
    }
    /// <summary>
    /// Sets a cell in the buffer by Vector2 coordinates and Color.
    /// </summary>
    public void SetCell(Vector2 v, char c, Color fg, Color bg)
    {
        SetCell((int)v.X, (int)v.Y, c, fg, bg);
    }
    /// <summary>
    /// Sets a cell in the buffer by integer coordinates and byte RGB values.
    /// </summary>
    public void SetCell(int x, int y, char c, byte fr, byte fg, byte fb, byte br, byte bg, byte bb)
    {
        SetCell(x, y, c, Color.From255(fr, fg, fb), Color.From255(br, bg, bb));
    }
    /// <summary>
    /// Sets a cell in the buffer by Vector2 coordinates and byte RGB values.
    /// </summary>
    public void SetCell(Vector2 v, char c, byte fr, byte fg, byte fb, byte br, byte bg, byte bb)
    {
        SetCell((int)v.X, (int)v.Y, c, Color.From255(fr, fg, fb), Color.From255(br, bg, bb));
    }
    /// <summary>
    /// Sets a cell in the buffer by integer coordinates and float RGB values (0..1).
    /// </summary>
    public void SetCell(int x, int y, char c, float fr, float fg, float fb, float br, float bg, float bb)
    {
        SetCell(x, y, c, Color.From01(fr, fg, fb), Color.From01(br, bg, bb));
    }
    /// <summary>
    /// Sets a cell in the buffer by Vector2 coordinates and float RGB values (0..1).
    /// </summary>
    public void SetCell(Vector2 v, char c, float fr, float fg, float fb, float br, float bg, float bb)
    {
        SetCell((int)v.X, (int)v.Y, c, Color.From01(fr, fg, fb), Color.From01(br, bg, bb));
    }

    /// <summary>
    /// Clears the buffer to spaces, white foreground, and black background.
    /// </summary>
    public void Clear()
    {
        Clear(' ', new Color(1f, 1f, 1f), new Color(0f, 0f, 0f));
    }

    /// <summary>
    /// Clears the buffer to the given character, white foreground, and black background.
    /// </summary>
    public void Clear(char c)
    {
        Clear(c, new Color(1f, 1f, 1f), new Color(0f, 0f, 0f));
    }

    /// <summary>
    /// Clears the buffer to the given character, foreground color, and background color.
    /// </summary>
    public void Clear(char c, Color fg, Color bg)
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                buffer[y, x] = new Cell { Char = c, Foreground = fg, Background = bg };
    }

    /// <summary>
    /// Disposes the visualizer, restoring the terminal state.
    /// </summary>
    public void Dispose()
    {
        if (!disposed)
        {
            disposed = true;
            Console.Write("\x1b[?1049l"); // Switch back to main buffer
            Console.Write("\x1b[0m");     // Reset colors
            Console.Write("\x1b[?25h");   // Show cursor
        }
    }
} 