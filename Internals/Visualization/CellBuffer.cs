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


    // Alpha-blend: top nad bottom (alpha 0..255)
    private static Rgb Blend(Rgb top, byte alpha, Rgb bottom)
    {
        if (alpha >= 255) return top;
        if (alpha == 0) return bottom;
        int a = alpha;
        int ia = 255 - a;
        byte r = (byte)((top.R * a + bottom.R * ia) / 255);
        byte g = (byte)((top.G * a + bottom.G * ia) / 255);
        byte b = (byte)((top.B * a + bottom.B * ia) / 255);
        return new Rgb(r, g, b);
    }

    /// <summary>
    /// Zmieszaj tylko TŁO w (x,y) z podanym kolorem i alphą (0..255).
    /// Nie zmienia znaku ani koloru tekstu.
    /// </summary>
    public void BlendBg(int x, int y, Rgb bg, byte alpha)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) return;
        var cur = _data[x, y];
        _data[x, y] = new Cell(cur.Ch, cur.Fg, Blend(bg, alpha, cur.Bg));
    }

    /// <summary>
    /// Nałóż komórkę z alphą (osobno dla fg i bg).
    /// Jeśli replaceChar=false i ch==' ', pozostaw znak spod spodu.
    /// </summary>
    public void BlendCell(int x, int y, Cell top, byte fgAlpha = 255, byte bgAlpha = 255, bool replaceChar = true)
    {
        if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) return;
        var cur = _data[x, y];

        var outBg = Blend(top.Bg, bgAlpha, cur.Bg);

        char ch = cur.Ch;
        Rgb outFg = cur.Fg;

        if (replaceChar || top.Ch != ' ')
        {
            ch = top.Ch == '\0' ? cur.Ch : top.Ch; // '\0' = brak zmiany znaku
            outFg = Blend(top.Fg, fgAlpha, cur.Fg);
        }

        _data[x, y] = new Cell(ch, outFg, outBg);
    }
}