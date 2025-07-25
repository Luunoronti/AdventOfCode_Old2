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
        if (actualResult == pc.Expectedresult) return ResultResult.Good;
        if (string.IsNullOrEmpty(pc.Expectedresult) && pc.KnownErrors.Count == 0) return ResultResult.Unknown;

        // first of all, check if we can convert value to long
        // if not, we just compare with any known bad values 

        bool known = pc.KnownErrors.Any(r => r == actualResult);

        return long.TryParse(actualResult, out var reslong) && long.TryParse(pc.Expectedresult, out var resexp)
            ? reslong < resexp
                ? known ? ResultResult.TooLowKnown : ResultResult.TooLow
                : known ? ResultResult.TooHighKnown : ResultResult.TooHigh
            : known ? ResultResult.KnownBad : ResultResult.UnknownBad;
    }
    public static void PrintResults()
    {
        Table table = new();
//        table.AddColumn("Year");
//        table.AddColumn("Day");
//        table.AddColumn("Name");
        table.AddColumn("Part");
        table.AddColumn("Result");
        table.AddColumn("Result Remarks");
        table.AddColumn("Time");

        var lastDay = -1;
        var lastYear = -1;

        foreach (var result in results)
        {
            if (lastDay != result.Day && lastYear != result.Year)
            {
                lastDay = result.Day;
                lastYear = result.Year;
                table.AddDayEntry(lastDay, lastYear, result.Name);
            }
//            table.AddValue("Name", result.Name);
//            table.AddValue("Day", result.Day.ToString("D02"));
//            table.AddValue("Year", result.Year.ToString("D02"));
            table.AddValue("Part", $"{result.Part} - {(result.IsTestRun ? "Test" : "Live")}", result.IsTestRun ? ConsoleColor.Yellow : ConsoleColor.White);

            // we need to take config for this result, to see if it's valid or not, and maybe higher or lower than expected
            var res = GetResultResult(result.Year, result.Day, result.Part, result.IsTestRun, result.Result);

            table.AddValue("Result", $"{result.Result}",
                res switch
                {
                    ResultResult.Good => ConsoleColor.Green,
                    ResultResult.Unknown => ConsoleColor.Cyan,
                    _ => ConsoleColor.Red,
                });

            table.AddValue("Result Remarks", $"{res switch
            {
                ResultResult.TooLow => "too low",
                ResultResult.TooLowKnown => "known, too low",
                ResultResult.TooHigh => "too high",
                ResultResult.TooHighKnown => "known, too high",
                ResultResult.KnownBad => "known",
                ResultResult.UnknownBad => "not known",
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
}
