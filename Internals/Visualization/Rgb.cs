namespace Visualization;

// Prosty helper kolorów TrueColor
public readonly record struct Rgb(byte R, byte G, byte B)
{
    public static Rgb Transparent => new(0, 0, 0);
    public static Rgb Black => new(0, 0, 0);
    public static Rgb White => new(255, 255, 255);
    public static Rgb Gray => new(180, 180, 180);
    public static Rgb Yellow => new(255, 220, 0);
    public static Rgb Red => new(220, 40, 40);
    public static Rgb Green => new(40, 200, 120);
    public static Rgb Blue => new(60, 120, 220);

    public static Rgb Lerp(Rgb a, Rgb b, double t) =>
        new((byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t));
}