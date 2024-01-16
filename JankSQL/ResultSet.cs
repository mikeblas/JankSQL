namespace JankSQL
{
    using System.Collections.Immutable;
    using System.Xml;

    public class ResultSet
    {
        private readonly List<Tuple> rows;
        private readonly FullColumnName[] columnNames;

        private bool isEOF;

        internal ResultSet(IEnumerable<FullColumnName> columnNames)
        {
            rows = new List<Tuple>();
            this.columnNames = columnNames.ToArray();
            isEOF = false;
        }

        public int RowCount
        {
            get { return rows.Count; }
        }

        public int ColumnCount
        {
            get { return columnNames.Length; }
        }

        internal bool IsEOF
        {
            get { return isEOF; }
        }

        public int ColumnIndex(FullColumnName name)
        {
            int ret = -1;
            for (int i = 0; i < columnNames.Length; i++)
            {
                if (columnNames[i].Equals(name))
                {
                    if (ret != -1)
                        throw new ExecutionException($"column name {name} is ambiguous because it matches both {columnNames[ret]} and {columnNames[i]}");
                    ret = i;
                }
            }

            return ret;
        }

        public Tuple Row(int index)
        {
            return rows[index];
        }

        public void Dump()
        {
            Console.WriteLine($"ResultSet: {string.Join(",", (object[])columnNames)}");
            if (isEOF)
                Console.WriteLine("   *** EOF ***");
            else
            {
                foreach (var row in rows)
                    Console.WriteLine($"   {row}");
            }
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
            if (isEOF)
                throw new InvalidOperationException();

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
            if (isEOF)
                throw new InvalidOperationException();

            if (rows.Count > 0)
            {
                if (row.Length != rows[0].Length)
                    throw new InvalidOperationException();
            }

            if (columnNames != null && columnNames.Length != row.Length)
                throw new InvalidOperationException($"Can't add row: expected {columnNames.Length} columns, got {row.Length} columns");

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

        internal void MarkEOF()
        {
            if (rows.Count != 0)
                throw new InvalidOperationException();

            isEOF = true;
        }
    }
}


