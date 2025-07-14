// attempt to keep the code as clean as possible
// most of functionality is in Core and Extensions folder


class RunReport
{
    public void AddResult(string result, TimeSpan time, int Year, int Day, int PartNum, bool RunTests)
    {
        Console.WriteLine($"Run executed. Result: {result}, time: {time.FormatUltraPrecise()}");
    }
}
static class DayRunner
{
    delegate string PartInputHandler(PartInput input);

    public static void Run()
    {
        if (RunDayFromCommandLine())
            return;

        // if program config allows us to run debug days, do that
        // also, run days specified in command line
        // note: do we really need debug/release ?
        // and non debug stuff
    }


    private static (PartInputHandler Part1, PartInputHandler Part2) LoadObject(int Year, int Day)
    {
        var type = Type.GetType($"Year_{Year}.Day{Day:D2}");
        if (type == null)
        {
            Console.WriteLine($"Unable to find day type Year_{Year}.Day{Day:D2}");
            return (null, null);
        }
        var part1 = type.GetMethod("Part1");
        if (part1 == null)
        {
            Console.WriteLine($"Unable to find Part1 method in type {type}");
            return (null, null);
        }
        var part2 = type.GetMethod("Part1");
        if (part2 == null)
        {
            Console.WriteLine($"Unable to find Part2 method in type {type}");
            return (null, null);
        }

        var obj = Activator.CreateInstance(type);
        if (obj == null)
        {
            Console.WriteLine($"Unable to create an instance from type {type}");
            return (null, null);
        }

        var handler1 = (PartInputHandler)part1.CreateDelegate(typeof(PartInputHandler), obj);
        var handler2 = (PartInputHandler)part2.CreateDelegate(typeof(PartInputHandler), obj);

        return (handler1, handler2);
    }


    private static string RunPart(PartInputHandler Handler, RunReport Report, int Year, int Day, int PartNum, bool RunTests)
    {
        // we look for files for this run
        var fileSt = $"{Configuration.RootPath}{Year}\\Day{Day:D2}\\Day{Day:D2}.{(RunTests ? "Test" : "Live")}.Part{PartNum}.";
        var files = Directory.GetFiles($"{Configuration.RootPath}{Year}\\Day{Day:D2}").ToList();
        files.Sort();

        foreach (var file in files)
        {
            if (!file.StartsWith(fileSt))
                continue;

            // create input
            PartInput input = new()
            {
                FullString = File.ReadAllText(file)
            };
            input.Lines = input.FullString.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
            input.Span = input.FullString.AsSpan();
            input.Count = input.Lines.Length;
            input.LineWidth = input.Lines == null ? 0 : (input.Lines.Length > 0 ? input.Lines[0].Length : 0);

            if (string.IsNullOrEmpty(input.FullString))
                continue;

            // warm up
#if !DEBUG
            _ = Handler(input);
#endif
            // actual run
            var startTime = Stopwatch.GetTimestamp();
            var result = Handler(input);
            Report.AddResult(result, Stopwatch.GetElapsedTime(startTime), Year, Day, PartNum, RunTests);
        }

        return null;
    }
    private static bool RunDay(int Year, int Day)
    {
        // look for config of this day
        var config = Configuration.Execution?.Years?.SingleOrDefault(y => y.Year == Year)?.Days?.SingleOrDefault(d => d.Day == Day);
        if (config == null)
        {
            Console.WriteLine($"Unable to find config for year {Year} day {Day}");
            return false;
        }

#if DEBUG
        if (!config.DebugRun)
        {
            return false;
        }
#else
        if (!config.Run)
        {
            return false;
        }
#endif
        var (Part1, Part2) = LoadObject(Year, Day);
        if (Part1 == null || Part2 == null)
        {
            return false;
        }

        // create report for this day
        RunReport Report = new();

        // run everything, according to config
        if (config.Test.Run)
        {
            RunPart(Part1, Report, Year, Day, 1, true);
            RunPart(Part2, Report, Year, Day, 2, true);
        }

        if (config.Live.Run)
        {
            RunPart(Part1, Report, Year, Day, 1, false);
            RunPart(Part2, Report, Year, Day, 2, false);
        }


        return config.Test.Run || config.Live.Run;
    }

    private static bool RunDayFromCommandLine()
    {
        var cmdLine = CommandLine.Parse();
        var year = cmdLine.GetResult(CommandLine.Year);
        var day = cmdLine.GetResult(CommandLine.Day);

        if (year is not null && day is not null && !year.Implicit && !day.Implicit)
        {
            int y = year.GetValueOrDefault<int>();
            int d = day.GetValueOrDefault<int>();

            // get this day from config. if not found, we skip
            // now load the day 

            if (y >= 2016 && y <= DateTime.Now.Year && d >= 1 && d <= 25)
            {
                if (RunDay(y, d))
                    return true;
            }
            else
            {
                Console.WriteLine("You can specify year between 2016 and current year, and days between 1 and 25, inclusive.");
                return true;
            }
        }
        return false;
    }


    private static void RunDebugDays()
    {
    }
    private static void RunReleaseDays()
    {
    }
}
