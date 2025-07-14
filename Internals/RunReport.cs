// attempt to keep the code as clean as possible
// most of functionality is in Core and Extensions folder


using System;
using System.IO;
using System.Reflection.Metadata;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

partial class RunReport
{
    private static List<SingleRunReport> results = [];

    public static void AddResult(string result, TimeSpan time, int Year, int Day, int PartNum, bool RunTests)
    {
        results.Add(new SingleRunReport(result, time, Year, Day, PartNum, RunTests));
    }

    enum ResultResult
    {
        Unknown,
        Good,
        TooLow,
        TooLowKnown,
        TooHigh,
        TooHighKnown,
        UnknownBad,
        KnownBad,
    }
    private static ResultResult GetResultResult(int Year, int Day, int part, bool isTest, string actualResult)
    {
        var config = Configuration.Execution?.Years?.SingleOrDefault(y => y.Year == Year)?.Days?.SingleOrDefault(d => d.Day == Day);
        if (config is null)
        {
            return ResultResult.Good;
        }

        Configuration.PartConfiguration pc = null;

        if (isTest && part == 1) pc = config.Test.Part1;
        else if (isTest && part == 2) pc = config.Test.Part2;
        else if (part == 1) pc = config.Live.Part1;
        else if (part == 2) pc = config.Live.Part2;

        if (pc == null) return ResultResult.Good;

        if(string.IsNullOrEmpty(pc.Expectedresult)) return ResultResult.Unknown;

        // first of all, check if we can convert value to long
        // if not, we just compare with any known bad values 

        if (actualResult == pc.Expectedresult) return ResultResult.Good;

        bool known = pc.KnownErrors.Any(r => r == actualResult);

        if (long.TryParse(actualResult, out var reslong) && long.TryParse(pc.Expectedresult, out var resexp))
        {
            if (reslong < resexp)
            {
                return known ? ResultResult.TooLowKnown : ResultResult.TooLow;
            }
            else
            {
                return known ? ResultResult.TooHighKnown : ResultResult.TooHigh;
            }
        }
        else
        {
            return known ? ResultResult.KnownBad : ResultResult.UnknownBad;
        }
    }
    public static void PrintResults()
    {
        // simple print:
        foreach (var result in results)
        {
            Console.WriteLine($"{result}");
        }

        // we shall construct a table now
        Table table = new();
        table.AddColumn("Year");
        table.AddColumn("Day");
        table.AddColumn("Name");
        table.AddColumn("Part");
        table.AddColumn("Result");
        table.AddColumn("Time");

        foreach (var result in results)
        {
            table.AddValue("Name", result.Name);
            table.AddValue("Day", result.Day.ToString("D02"));
            table.AddValue("Year", result.Year.ToString("D02"));
            table.AddValue("Part", $"{result.Part} - {(result.IsTestRun ? "Test" : "Live")}", result.IsTestRun ? ConsoleColor.Yellow : ConsoleColor.White);

            // we need to take config for this result, to see if it's valid or not, and maybe higher or lower than expected
            var res = GetResultResult(result.Year, result.Day, result.Part, result.IsTestRun, result.Result);

            table.AddValue("Result", $"{result.Result}{res switch
            {
                ResultResult.TooLow => " v",
                ResultResult.TooLowKnown => " v(m)",
                ResultResult.TooHigh => " ^",
                ResultResult.TooHighKnown => " ^(m)",
                ResultResult.KnownBad => " (m)",
                _ => "",
            }}",
                res switch
                {
                    ResultResult.Good => ConsoleColor.Green,
                    ResultResult.TooLow => ConsoleColor.Red,
                    ResultResult.TooLowKnown => ConsoleColor.Magenta,
                    ResultResult.TooHigh => ConsoleColor.Red,
                    ResultResult.TooHighKnown => ConsoleColor.Magenta,
                    ResultResult.UnknownBad => ConsoleColor.Red,
                    ResultResult.KnownBad => ConsoleColor.Magenta,
                    ResultResult.Unknown => ConsoleColor.Cyan,
                    _ => ConsoleColor.White,
                });

            table.AddValue("Time", result.Time.FormatUltraPrecise(), ConsoleColor.Red);
        }

        table.Format();
        table.Print();
    }
    //↑
    //⇧
    //⬆
    //▲
    //▴


    //↓
    //⇩
    //⬇
    //▼
    //▾

}
