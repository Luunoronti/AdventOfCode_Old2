// attempt to keep the code as clean as possible
// most of functionality is in Core and Extensions folder

static class DayGenerator
{
    public static void GenerateDaysIfRequired()
    {
        // if a day was selected via command line, we do that

        // if none exist, create everything up to today, with first year being 2016
        if (Configuration.Execution.Years.Count == 0)
        {
            for (int y = 2016; y <= DateTime.Now.Year; ++y)
            {
                for (int d = 1; d <= 25; ++d)
                {
                    CreateDayIfDoesNotExist(Configuration.RootPath, y, d);
                }
            }
            Configuration.SaveExecutionConfiguration();
            return;
        }

        // else just create one that is today (but make sure we are running December)
        if (DateTime.Now.Month == 12)
        {
            CreateDayIfDoesNotExist(Configuration.RootPath, DateTime.Now.Year, DateTime.Now.Day);
            Configuration.SaveExecutionConfiguration();
        }
    }


    private static void CreateDayIfDoesNotExist(string RootPath, int year, int day)
    {
        if (day > 25) return;

        var prefix = $"{RootPath}{year}\\";
        Directory.CreateDirectory(prefix);

        if (File.Exists(prefix + $"Day{day:D2}.cs") == false)
            File.WriteAllText(prefix + $"Day{day:D2}.cs", DayTemplateCode.Replace("{Year}", year.ToString()).Replace("{Day}", day.ToString("D2")));
        if (File.Exists(prefix + $"test.txt") == false)
            File.WriteAllText(prefix + $"test.txt", "");


        if (!Configuration.Execution.Years.Any(y => y.Year == year))
            Configuration.Execution.Years.Add(new Configuration.YearConfiguration() { Year = year });

        if (!Configuration.Execution.Years.Single(y => y.Year == year).Days.Any(d => d.Day == day))
            Configuration.Execution.Years.Single(y => y.Year == year).Days.Add(new Configuration.DayConfiguration
            {
                Day = day,
                Year = year,
                EnableVisualization = false,
                DebugRun = false,
                Run = true,
                Name = $"Day {day}/{year}",
                Test = new Configuration.PartsConfiguration
                {
                    Run = true,
                    Part1 = new Configuration.PartConfiguration
                    {
                        EnableVisualization = false,
                        Expectedresult = "",
                        KnownErrors = []
                    },
                    Part2 = new Configuration.PartConfiguration
                    {
                        EnableVisualization = false,
                        Expectedresult = "",
                        KnownErrors = []
                    },
                },
                Live = new Configuration.PartsConfiguration
                {
                    Run = false,
                    Part1 = new Configuration.PartConfiguration
                    {
                        EnableVisualization = false,
                        Expectedresult = "",
                        KnownErrors = []
                    },
                    Part2 = new Configuration.PartConfiguration
                    {
                        EnableVisualization = false,
                        Expectedresult = "",
                        KnownErrors = []
                    },
                }
            });

    }

    private const string DayTemplateCode = @"
        using StringSpan = System.ReadOnlySpan<char>;
        namespace AdventOfCode{Year};

        class Day{Day}
        {
            // we may want to introduce an input class here
            public static string Part1(StringSpan Input, Int32 LineWidth, Int32 Count)
            {
                long response = 0;
                return response.ToString();
            }
            public static string Part2(StringSpan Input, Int32 LineWidth, Int32 Count)
            {
                long response = 0;
                return response.ToString();
            }
        }
        ";

}