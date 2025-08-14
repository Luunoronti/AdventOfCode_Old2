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
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                char ch = " .:-=+*#%@".AsSpan()[rnd.Next(10)];
                var fg = new Rgb((byte)rnd.Next(50, 255), (byte)rnd.Next(50, 255), (byte)rnd.Next(50, 255));
                _cells[x, y] = new Cell(ch, fg, Rgb.Black);
            }
    }

    public Cell? GetCell(int x, int y)
    {
        if ((uint)x < (uint)Width && (uint)y < (uint)Height)
            return _cells[x, y];
        return null;
    }
}