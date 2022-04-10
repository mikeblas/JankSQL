namespace JankSQL
{
    using System.Collections.Immutable;

    public class ResultSet
    {
        private readonly List<Tuple> rows;
        private readonly FullColumnName[] columnNames;

        internal ResultSet(IEnumerable<FullColumnName> columnNames)
        {
            rows = new List<Tuple>();
            this.columnNames = columnNames.ToArray();
        }

        public int RowCount
        {
            get { return rows.Count; }
        }

        public int ColumnCount
        {
            get { return columnNames.Length; }
        }

        public int ColumnIndex(FullColumnName name)
        {
            return Array.IndexOf(columnNames, name);
        }

        public Tuple Row(int index)
        {
            return rows[index];
        }

        public void Dump()
        {
            Console.WriteLine($"{string.Join(",", (object[])columnNames)}");

            foreach (var row in rows)
                Console.WriteLine($"{row}");
        }

        public ImmutableList<FullColumnName> GetColumnNames()
        {
            return columnNames.ToImmutableList();
        }

        internal static ResultSet NewWithShape(ResultSet other)
        {
            ResultSet ret = new (other.columnNames);
            return ret;
        }

        internal void Append(ResultSet other)
        {
            if (rows == null)
                throw new InvalidOperationException();

            if (other.RowCount == 0)
                return;

            if (rows.Count > 0 && rows[0].Length != other.rows[0].Length)
                throw new InvalidOperationException();

            if (columnNames != null && other.columnNames != null && other.columnNames.Length != columnNames.Length)
                throw new InvalidOperationException();

            rows.AddRange(other.rows);
        }

        internal FullColumnName GetColumnName(int index)
        {
            return columnNames[index];
        }

        internal void AddRow(Tuple row)
        {
            if (rows.Count > 0)
            {
                if (row.Length != rows[0].Length)
                {
                    throw new InvalidOperationException();
                }
            }

            if (columnNames != null && columnNames.Length != row.Length)
            {
                throw new InvalidOperationException($"Can't add row: expected {columnNames.Length} columns, got {row.Length} columns");
            }

            rows.Add(row);
        }

        internal void AddRowFrom(ResultSet rs, int index)
        {
            AddRow(rs.Row(index));
        }

        internal void Sort(IComparer<Tuple> ic)
        {
            rows.Sort(ic);
        }
    }
}


