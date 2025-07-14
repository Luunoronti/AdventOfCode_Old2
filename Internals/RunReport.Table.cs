// attempt to keep the code as clean as possible
// most of functionality is in Core and Extensions folder


using System.Text;

partial class RunReport
{
    private class Table
    {
        private List<TableColumn> Columns { get; } = [];
        private Dictionary<string, TableColumn> NamedColumns { get; } = [];

        public void AddColumn(string name)
        {
            var col = new TableColumn { Header = name };
            Columns.Add(col);
            NamedColumns.Add(name, col);
        }

        public void AddValue(string column, string value, ConsoleColor Color = ConsoleColor.White)
        {
            if (NamedColumns.TryGetValue(column, out TableColumn col))
            {
                col.Add(value, Color);
            }
        }

        private void PrintHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("┌");
            foreach (var col in Columns)
            {
                sb.Append("".PadLeft(col.MaximumWidth, '─'));
                sb.Append(col == Columns.Last() ? "┐" : "┬");
            }
            Console.WriteLine(sb.ToString());

            sb.Clear();
            sb.Append("│");
            foreach (var col in Columns)
            {
                sb.Append(col.ToString(-1));
                sb.Append("│");
            }
            Console.WriteLine(sb.ToString());

            sb.Clear();
            sb.Append("├");
            foreach (var col in Columns)
            {
                sb.Append("".PadLeft(col.MaximumWidth, '─'));
                sb.Append(col == Columns.Last() ? "┤" : "┼");
            }
            Console.WriteLine(sb.ToString());
        }
        private void PrintRow(int Row)
        {
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.Append("│");
            foreach (var col in Columns)
            {
                sb.Append(col.ToString(Row));
                sb.Append("│");
            }
            Console.WriteLine(sb.ToString());
            sb.Clear();
        }
        private void PrintFooter()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("└");
            foreach (var col in Columns)
            {
                sb.Append("".PadLeft(col.MaximumWidth, '─'));
                sb.Append(col == Columns.Last() ? "┘" : "┴");
            }
            Console.WriteLine(sb.ToString());
        }
        public void Print()
        {
            PrintHeader();
            for (int i = 0; i < Columns[0].Count; i++)
                PrintRow(i);
            PrintFooter();
        }

        internal void Format()
        {
        }
    }
}
