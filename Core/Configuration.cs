
using Newtonsoft.Json;

public class Configuration
{
    public static ProgramConfiguration Program { get; } = LoadProgramConfiguration();
    public static ExecutionConfiguration Execution { get; } = LoadExecutionConfiguration();

    private static ProgramConfiguration LoadProgramConfiguration()
    {
        try
        {
            return JsonConvert.DeserializeObject<ProgramConfiguration>(File.ReadAllText($"{RootPath}/Configuration/ProgramConfig.json")) ?? new ProgramConfiguration();
        }
        catch
        {
        }
        // create empty program config
        try
        {
            Directory.CreateDirectory($"{RootPath}/Configuration/");
            File.WriteAllText($"{RootPath}/Configuration/ProgramConfig.json", "{}");
        }
        catch { }
        return new ProgramConfiguration();
    }
    private static ExecutionConfiguration LoadExecutionConfiguration()
    {
        try
        {
            return JsonConvert.DeserializeObject<ExecutionConfiguration>(File.ReadAllText($"{RootPath}/Configuration/ExecutionConfig.json")) ?? new ExecutionConfiguration();
        }
        catch
        {
        }
        // create empty program config
        try
        {
            Directory.CreateDirectory($"{RootPath}/Configuration/");
            File.WriteAllText($"{RootPath}/Configuration/ProgramConfig.json", "{}");
        }
        catch { }
        return new ExecutionConfiguration();
    }
    public static void SaveExecutionConfiguration()
    {
        try
        {
            Directory.CreateDirectory($"{RootPath}/Configuration/");
            File.WriteAllText($"{RootPath}/Configuration/ExecutionConfig.json", JsonConvert.SerializeObject(Execution, Formatting.Indented));
        }
        catch
        {
        }
        // create empty program config
        try
        {
            Directory.CreateDirectory($"{RootPath}/Configuration/");
            File.WriteAllText($"{RootPath}/Configuration/ExecutionConfig.json", "{}");
        }
        catch { }
    }

    private static string rootPath;
    public static string RootPath
    {
        get
        {
            // introduce some advanced logic herein - later on
            rootPath ??= File.Exists(".\\session.txt") ? ".\\" : "..\\..\\..\\";
            return rootPath;
        }
    }

    public class ProgramConfiguration
    {
        public bool RunAllInDebug
        {
            get; set;
        }
        public bool RunAllInRelease
        {
            get; set;
        }
        public VisualizerConfiguration Visualizer
        {
            get; set;
        } = new VisualizerConfiguration();
    }
    public class VisualizerConfiguration
    {
        public bool AllowMouseCapture
        {
            get; set;
        }
        public bool AlternateBuffer
        {
            get; set;
        }
        public bool HideCursor
        {
            get; set;
        }
        public bool Force16colorsMode
        {
            get; set;
        }
        public bool ClearScreenOnExit
        {
            get; set;
        }
        public int LegendVisibility
        {
            get; set;
        }
        public int CameraMoveMouseButton
        {
            get; set;
        }
        public int ClearMode
        {
            get; set;
        }
        public ConfigColor ClearColor
        {
            get; set;
        }
        public ConfigColor InfoColor
        {
            get; set;
        }
        public ConfigColor GradientStartColor
        {
            get; set;
        }
        public ConfigColor GradientEndColor
        {
            get; set;
        }
    }
    public struct ConfigColor
    {
        public int r
        {
            get; set;
        }
        public int g
        {
            get; set;
        }
        public int b
        {
            get; set;
        }
    }

    public class ExecutionConfiguration
    {
        public List<YearConfiguration> Years { get; set; }
    }
    public class YearConfiguration
    {
        public int Year { get; set; }
        public List<DayConfiguration> Days { get; set; }
    }
    public class DayConfiguration
    {
        public bool Run { get; set; }
        public bool DebugRun { get; set; }
        public string Name { get; set; }
        public int Day { get; set; }
        public int Year { get; set; }
        public bool EnableVisualization { get; set; }

        public PartsConfiguration Test { get; set; }
        public PartsConfiguration Live { get; set; }
    }
    public class PartsConfiguration
    {
        public bool Run { get; set; }
        public PartConfiguration Part1 { get; set; }
        public PartConfiguration Part2 { get; set; }
    }
    public class PartConfiguration
    {
        public string Expectedresult { get; set; }
        public List<string> KnownErrors { get; set; }
        public bool EnableVisualization { get; set; }
    }

}


