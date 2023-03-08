namespace JankSQL
{
    using JankSQL.Engines;
    using System.Collections.Immutable;

    internal class ColumnNameList
    {
        private FullColumnName[] names;
        private SortedList<FullColumnName, int> nameIndex;

        internal ColumnNameList(IList<FullColumnName> names)
        {
            this.names = new FullColumnName[names.Count];
            nameIndex = new SortedList<FullColumnName, int>();

            for (int i = 0; i < names.Count; i++)
            {
                this.names[i] = names[i];
                nameIndex.Add(names[i], i);
            }
        }

        internal ColumnNameList(ColumnNameList other)
        {
            names = (FullColumnName[])other.names.Clone();
            nameIndex = new SortedList<FullColumnName, int>(other.nameIndex);
        }

        internal ColumnNameList(IEngineTable table)
        {
            this.names = new FullColumnName[table.ColumnCount];
            nameIndex = new SortedList<FullColumnName, int>();

            for (int i = 0; i <  table.ColumnCount; i++)
            {
                this.names[i] = table.ColumnName(i);
                nameIndex.Add(names[i], i);
            }
        }

        internal FullColumnName[] GetColumnNames()
        {
            return names;
        }

        internal int Count { get { return nameIndex.Count; } }

        internal void Append(ColumnNameList other)
        {
            FullColumnName[] newNames = new FullColumnName[names.Length + other.Count];

            // copy existing
            Array.Copy(names, newNames, names.Length);

            for (int i = 0; i < other.names.Length; i++)
            {
                newNames[i + names.Length] = other.names[i];
                nameIndex.Add(newNames[i + names.Length], i + names.Length);
            }

            names = newNames;
        }

        internal int GetColumnIndex(FullColumnName fcn)
        {
            if (!nameIndex.ContainsKey(fcn))
                return -1;
            return nameIndex[fcn];
        }

        internal FullColumnName GetColumnName(int index)
        {
            return names[index];
        }

        public override string ToString()
        {
            return string.Join(",", (object[])names);
        }
    }

    public class ResultSet
    {
        private readonly List<Tuple> rows;
        private readonly ColumnNameList columnNames;

        private bool isEOF;

        internal ResultSet(IList<FullColumnName> columnNames)
        {
            rows = new List<Tuple>();
            this.columnNames = new ColumnNameList(columnNames);
            isEOF = false;
        }

        internal ResultSet(ColumnNameList columnNames)
        {
            rows = new List<Tuple>();
            this.columnNames = columnNames;
            isEOF = false;
        }


        public int RowCount
        {
            get { return rows.Count; }
        }

        public int ColumnCount
        {
            get { return columnNames.Count; }
        }

        internal ColumnNameList ColumnNameList
        {
            get { return columnNames; }
        }

        internal bool IsEOF
        {
            get { return isEOF; }
        }

        public int ColumnIndex(FullColumnName name)
        {
            return columnNames.GetColumnIndex(name);
        }

        public Tuple Row(int index)
        {
            return rows[index];
        }

        public void Dump()
        {
            Console.WriteLine($"ResultSet: {columnNames}");
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
            return columnNames.GetColumnNames().ToImmutableList();
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

            if (columnNames != null && other.columnNames != null && other.columnNames.Count != columnNames.Count)
                throw new InvalidOperationException();

            rows.AddRange(other.rows);
        }

        internal ColumnNameList GetColumnNameList()
        {
            return columnNames;
        }

        internal FullColumnName GetColumnName(int index)
        {
            return columnNames.GetColumnName(index);
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

            if (columnNames != null && columnNames.Count != row.Length)
                throw new InvalidOperationException($"Can't add row: expected {columnNames.Count} columns, got {row.Count} columns");

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


