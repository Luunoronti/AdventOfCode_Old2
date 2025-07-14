using System.CommandLine;

class CommandLine
{
    public static Option Day { get; } = new Option<int>("--day", "Day");
    public static Option Year { get; } = new Option<int>("--year", "Year");
    public static ParseResult Parse()
    {
        return Parse(Environment.GetCommandLineArgs());
    }
    static ParseResult _result;
    public static ParseResult Parse(string[] args)
    {
        if (_result == null)
        { }
        var rootCommand = new RootCommand("Advent of Code");
        rootCommand.Options.Add(Day);
        rootCommand.Options.Add(Year);
        _result = rootCommand.Parse(args);
        return _result;
    }
}