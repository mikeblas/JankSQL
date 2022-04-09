namespace JankSQL
{
    public class ResultSet
    {
        private readonly List<Tuple> rows;
        private readonly List<FullColumnName> columnNames;

        internal ResultSet(List<FullColumnName> columnNames)
        {
            rows = new List<Tuple>();
            this.columnNames = columnNames;
        }

        public int RowCount
        {
            get { return rows.Count; }
        }

        public int ColumnCount
        {
            get { return columnNames.Count; }
        }

        public int ColumnIndex(FullColumnName name)
        {
            return columnNames.IndexOf(name);
        }

        public Tuple Row(int index)
        {
            return rows[index];
        }

        public void Dump()
        {
            Console.WriteLine($"{string.Join(",", columnNames)}");

            foreach (var row in rows)
                Console.WriteLine($"{row}");
        }

        public List<FullColumnName> GetColumnNames()
        {
            return columnNames;
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

            if (columnNames != null && other.columnNames != null && other.columnNames.Count != columnNames.Count)
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

            if (columnNames != null && columnNames.Count != row.Length)
            {
                throw new InvalidOperationException($"Can't add row: expected {columnNames.Count} columns, got {row.Length} columns");
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


