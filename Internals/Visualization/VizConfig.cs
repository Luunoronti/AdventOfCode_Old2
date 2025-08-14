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
}