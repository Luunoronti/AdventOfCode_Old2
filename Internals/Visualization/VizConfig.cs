namespace Visualization;

public record VizConfig
{
    public ColorMode ColorMode { get; init; } = ColorMode.TrueColor;
    public int TargetFps { get; init; } = 30;

    // autoplay / pętla krokowa:
    public bool AutoPlay { get; set; } = false;
    public double AutoStepPerSecond { get; set; } = 5.0;

    public double PanSpeed { get; set; } = 2.0;        // mnożnik szybkości panningu myszą (świat/px)
    public double PanKeyStepFrac { get; set; } = 0.10; // ułamek szerokości widoku świata na 1 krok klawiaturą


    // WARSTWY:
    public UiLayers Layers { get; set; } = UiLayers.All;

    public Rgb RulerHighlight { get; set; } = new Rgb(80, 140, 240);
    public Rgb RulerBgColor { get; set; } = new Rgb(40, 40, 40); // ciemny szary
    public byte RulerBgAlpha { get; set; } = 120;                 // ~47% przezroczystości
    public byte RulerHighlightAlpha { get; set; } = 160;          // podświetlenie jest odrobinę mocniejsze
    public int LeftRulerWidth { get; set; } = 4;                 // szerokość lewego rulera (kolumny 0..W-1)


    public bool ContinuousRenderWhenAutoPlay { get; set; } = true;
}