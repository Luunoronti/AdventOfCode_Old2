using System.CommandLine.Parsing;

// attempt to keep the code as clean as possible
// most of functionality is in Core and Extensions folder

DayGenerator.GenerateDaysIfRequired();

DayRunner.RunDebugDays();
DayRunner.RunEnabledDays();

Console.WriteLine("Test completed");
