namespace Visualization;

// =================== Terminal & buffers ===================

internal sealed class CellBuffer
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    private Cell[,] _data;

    public CellBuffer(int w, int h)
    {
        Width = w; Height = h;
        _data = new Cell[w, h];
        Fill(new Cell(' ', Rgb.White, Rgb.Black));
    }

    public void Resize(int w, int h)
    {
        Width = w; Height = h;
        _data = new Cell[w, h];
        Fill(new Cell(' ', Rgb.White, Rgb.Black));
    }

    public void Fill(Cell c)
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                _data[x, y] = c;
    }

    public void Set(int x, int y, Cell c)
    {
        if ((uint)x < (uint)Width && (uint)y < (uint)Height)
            _data[x, y] = c;
    }
    public bool TrySet(int x, int y, Cell c)
    {
        if ((uint)x < (uint)Width && (uint)y < (uint)Height)
        { _data[x, y] = c; return true; }
        return false;
    }

    public Cell this[int x, int y] => _data[x, y];
}