namespace Visualization;

// =================== DemoWorld (przykładowe źródło świata) ===================

public sealed class DemoWorld : IWorldSource
{
    public int Width { get; }
    public int Height { get; }
    private readonly Cell[,] _cells;

    public DemoWorld(int width, int height)
    {
        Width = width; Height = height;
        _cells = new Cell[width, height];

        var rnd = new Random(1);
        var chars = " .:-=+*#%@".AsSpan();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // --- TŁO (BG): mieszanka szachownicy + łagodny gradient po Y ---
                bool check = (((x >> 3) ^ (y >> 3)) & 1) == 0;  // kwadraty 8x8
                byte g = (byte)(40 + (y * 215 / Math.Max(1, height - 1))); // 40..255
                var bg = check ? new Rgb(g, (byte)(g * 0.9), (byte)(g * 0.8))
                               : new Rgb((byte)(g * 0.8), g, (byte)(g * 0.9));

                // --- ZNAK + FG: losowe, ale z lekkim przesunięciem kolorów ---
                char ch = chars[rnd.Next(chars.Length)];
                var fg = new Rgb(
                    (byte)(80 + rnd.Next(150)),     // 80..229
                    (byte)(80 + rnd.Next(150)),
                    (byte)(80 + rnd.Next(150)));

                // delikatny kontrast fg względem bg
                if (fg.R + fg.G + fg.B < bg.R + bg.G + bg.B)
                {
                    // rozjaśnij fg, gdy jest ciemniejszy niż tło
                    fg = new Rgb(
                        (byte)Math.Min(255, fg.R + 40),
                        (byte)Math.Min(255, fg.G + 40),
                        (byte)Math.Min(255, fg.B + 40));
                }

                _cells[x, y] = new Cell(ch, fg, bg);
            }
        }
    }


    public Cell? GetCell(int x, int y)
    {
        if ((uint)x < (uint)Width && (uint)y < (uint)Height)
            return _cells[x, y];
        return null;
    }
}