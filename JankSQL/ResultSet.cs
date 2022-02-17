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

        List<string> columnNames;

        internal ResultSet()
        {
            rows = new List<ExpressionOperand[]>();
            columnNames = new List<string>();
        }

        public int RowCount { get { return rows.Count; } }

        public int ColumnCount {  get { return rows[0].Length; } }

        internal void SetColumnNames(List<string> names)
        {
            columnNames = names;
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
            rows.Add(row);
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


