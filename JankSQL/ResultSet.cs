using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    public class ResultSet
    {
        List<ExpressionOperand[]> rows;

        List<FullColumnName> columnNames;

        internal ResultSet()
        {
            rows = new List<ExpressionOperand[]>();
            columnNames = new List<FullColumnName>();
        }

        internal static ResultSet NewWithShape(ResultSet other)
        {
            ResultSet ret = new ResultSet();

            ret.columnNames = other.columnNames;

            return ret;
        }

        internal int ColumnIndex(FullColumnName name)
        {
            return columnNames.IndexOf(name);
        }

        internal ExpressionOperand[] Row(int index)
        {
            return rows[index];
        }

        internal void Append(ResultSet other)
        {
            if (rows == null)
            {
                throw new InvalidOperationException();
            }
            if (rows.Count > 0  && rows[0].Length != other.rows[0].Length)
            {
                throw new InvalidOperationException();
            }
            if (columnNames != null && other.columnNames != null && other.columnNames.Count != columnNames.Count)
            {
                throw new InvalidOperationException();
            }
            rows.AddRange(other.rows);
        }

        public int RowCount { get { return rows.Count; } }

        public int ColumnCount {  get { return columnNames.Count; } }

        internal void SetColumnNames(List<FullColumnName> names)
        {
            columnNames = names;
        }

        internal List<FullColumnName> GetColumnNames()
        {
            return columnNames;
        }

        internal FullColumnName GetColumnName(int index)
        {
            return columnNames[index];
        }

        internal  void AddRow(ExpressionOperand[] row)
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
                throw new InvalidOperationException();
            }

            rows.Add(row);
        }

        internal void AddRowFrom(ResultSet rs, int index)
        {
            AddRow(rs.Row(index));
        }

        public void Dump()
        {
            foreach(var name in columnNames)
            {
                Console.Write(name);
                Console.Write(",");
            }
            Console.WriteLine();

            foreach (var row in rows)
            {
                foreach (var cell in row)
                {
                    Console.Write(cell.ToString());
                    Console.Write(",");
                }

                Console.WriteLine();
            }
        }
    }
}


