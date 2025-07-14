// attempt to keep the code as clean as possible
// most of functionality is in Core and Extensions folder

partial class RunReport
{
    private class SingleRunReport
    {
        public SingleRunReport(string result, TimeSpan time, int year, int day, int part, bool isTestRun)
        {
            Result = result;
            Time = time;
            Year = year;
            Day = day;
            Part = part;
            IsTestRun = isTestRun;

            var config = Configuration.Execution?.Years?.SingleOrDefault(y => y.Year == Year)?.Days?.SingleOrDefault(d => d.Day == Day);
            if (config is not null)
            {
                Name = config.Name;
            }
        }

        public string Result { get; }
        public TimeSpan Time { get; }
        public int Year { get; }
        public int Day { get; }
        public int Part { get; }
        public bool IsTestRun { get; }

        public string Name { get; }

        public override string ToString()
        {
            return $"{Name} ({Day}/{Year}) Part {Part} {(IsTestRun ? "Test" : "Live")}: Result: {Result}, time: {Time.FormatUltraPrecise()}";
        }
    }
}
