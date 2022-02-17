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

        internal ResultSet()
        {
            rows = new List<ExpressionOperand[]>();
        }

        internal int RowCount { get { return rows.Count; } }

        internal  void AddRow(ExpressionOperand[] row)
        {
            rows.Add(row);
        }

        public void Dump()
        {
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


