// attempt to keep the code as clean as possible
// most of functionality is in Core and Extensions folder


using System.IO;

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

    class PartRunner
    {
        public int Year { get; set; }
        public int Day { get; set; }
        private int Part { get; set; }

        private void TimedRun(bool RunTests, RunReport Report, PartInputHandler Handler, PartInput Input)
        {
            // warm up
#if !DEBUG
            _ = Handler(Input);
#endif
            var startTime = Stopwatch.GetTimestamp();
            var result = Handler(Input);
            Report.AddResult(result, Stopwatch.GetElapsedTime(startTime), Year, Day, Part, RunTests);
        }
        private void RunInternal(bool RunTests, RunReport Report)
        {
            PartInputHandler Handler = LoadHandler();

            var fileSt = $"{Configuration.RootPath}{Year}\\Day{Day:D2}\\Day{Day:D2}.{(RunTests ? "Test" : "Live")}.Part{Part}.";
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

                // actual run
                TimedRun(RunTests, Report, Handler, input);
            }
        }

        public void Run(bool RunTests, RunReport Report)
        {
            Part = 1;
            RunInternal(RunTests, Report);
            Part = 2;
            RunInternal(RunTests, Report);  
        }
        private PartInputHandler LoadHandler()
        {
            var type = Type.GetType($"Year_{Year}.Day{Day:D2}");
            if (type == null)
            {
                Console.WriteLine($"Unable to find day type Year_{Year}.Day{Day:D2}");
                return null;
            }
            var part = type.GetMethod($"Part{Part}");
            if (part == null)
            {
                Console.WriteLine($"Unable to find Part{Part} method in type {type}");
                return null;
            }

            var obj = Activator.CreateInstance(type);
            if (obj == null)
            {
                Console.WriteLine($"Unable to create an instance from type {type}");
                return null;
            }

            return (PartInputHandler)part.CreateDelegate(typeof(PartInputHandler), obj);
        }
    }


    public static void Run()
    {
        if (RunDayFromCommandLine())
            return;
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

        // create part runner
        PartRunner runner = new()
        {
            Year = Year,
            Day = Day,
        };

        // create report for this day
        RunReport Report = new();

        if (config.Test.Run)
        {
            runner.Run(true, Report);
        }
        if (config.Live.Run)
        {
            runner.Run(false, Report);
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
