// attempt to keep the code as clean as possible
// most of functionality is in Core and Extensions folder


using System.CommandLine;
using System.Net;

static class DayGenerator
{
    public static void GenerateDaysIfRequired()
    {
        var cmdLine = CommandLine.Parse();
        var year = cmdLine.GetResult(CommandLine.Year);
        var day = cmdLine.GetResult(CommandLine.Day);

        if (year is not null && day is not null && !year.Implicit && !day.Implicit)
        {
            int y = year.GetValueOrDefault<int>();
            int d = day.GetValueOrDefault<int>();

            if (y >= 2016 && y <= DateTime.Now.Year && d >= 1 && d <= 25)
            {
                CreateDayIfDoesNotExist(Configuration.RootPath, y, d);
                Configuration.SaveExecutionConfiguration();
                return;
            }

            // wrong times: print error
            Console.WriteLine("You can specify year between 2016 and current year, and days between 1 and 25, inclusive.");
            return;
        }

        // if a day was selected via command line, we do that

        // if none exist, create everything up to today, with first year being 2016
        //if (Configuration.Execution.Years.Count == 0)
        //{
        //    for (int y = 2016; y <= DateTime.Now.Year; ++y)
        //    {
        //        for (int d = 1; d <= 25; ++d)
        //        {
        //            CreateDayIfDoesNotExist(Configuration.RootPath, y, d);
        //        }
        //    }
        //    Configuration.SaveExecutionConfiguration();
        //    return;
        //}

        // else just create one that is today (but make sure we are running December)
        if (DateTime.Now.Month == 12)
        {
            CreateDayIfDoesNotExist(Configuration.RootPath, DateTime.Now.Year, DateTime.Now.Day);
            Configuration.SaveExecutionConfiguration();
            return;
        }

        if (Configuration.Execution.Years.Count == 0)
        {
            Console.WriteLine("Please specify target day to work with (using --year and --day options).");
            //for (int y = 2016; y <= DateTime.Now.Year; ++y)
            //{
            //    for (int d = 1; d <= 25; ++d)
            //    {
            //        CreateDayIfDoesNotExist(Configuration.RootPath, y, d);
            //    }
            //}
            //Configuration.SaveExecutionConfiguration();
            return;
        }
    }


    private static void CreateDayIfDoesNotExist(string RootPath, int year, int day)
    {
        if (day > 25) return;

        var prefix = $"{RootPath}{year}\\Day{day:D2}\\";
        Directory.CreateDirectory(prefix);

        if (File.Exists(prefix + $"Day{day:D2}.cs") == false)
            File.WriteAllText(prefix + $"Day{day:D2}.cs", DayTemplateCode.Replace("{Year}", year.ToString()).Replace("{Day}", day.ToString("D2")));

        // also, create test and live input files
        // and attempt downloading live data
        if (File.Exists(prefix + $"Day{day:D2}.Test.Part1.1.txt") == false) File.WriteAllText(prefix + $"Day{day:D2}.Test.Part1.1.txt", "");
        if (File.Exists(prefix + $"Day{day:D2}.Test.Part2.1.txt") == false) File.WriteAllText(prefix + $"Day{day:D2}.Test.Part2.1.txt", "");

        if (File.Exists(prefix + $"Day{day:D2}.Live.Part1.1.txt") == false)
        {
            File.WriteAllText(prefix + $"Day{day:D2}.Live.Part1.1.txt", GetLiveData(RootPath, year, day));
        }
        if (File.Exists(prefix + $"Day{day:D2}.Live.Part2.1.txt") == false)
        {
            File.WriteAllText(prefix + $"Day{day:D2}.Live.Part2.1.txt", GetLiveData(RootPath, year, day));
        }

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


    // Note: If we are to run live data, download them from AoC. 
    // Huge thanks to Nick Kusters (https://www.youtube.com/@NKCSS) for pointing out that live data should not be kept on GitHub,
    // and allowing to copy his download code.
    private static string GetLiveData(string RootPath, int year, int day)
    {
        try
        {
            string session = File.ReadAllText($"{RootPath}session.txt");
            string url = $"https://adventofcode.com/{year}/day/{day}/input";

#pragma warning disable SYSLIB0014 // Type or member is obsolete
            var wc = new WebClient();
#pragma warning restore SYSLIB0014 // Type or member is obsolete
            wc.Headers.Add(HttpRequestHeader.UserAgent, "https://github.com/Luunoronti/AdventOfCode");
            wc.Headers.Add(HttpRequestHeader.Cookie, $"{nameof(session)}={session}");
            string contents = wc.DownloadString(url);
            return contents;

        }
        catch(Exception ex) 
        {
            return $"Unable to obtain live data: {ex}";
        }
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