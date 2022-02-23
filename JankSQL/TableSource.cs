using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{

    internal class TableSource : IComponentOutput
    {
        Engines.DynamicCSV source;
        int currentRow = 0;

        internal TableSource(Engines.DynamicCSV source)
        {
            this.source = source;
            this.source.Load();
        }

        internal void Describe()
        {
        }

        public void Rewind()
        {
            currentRow = 0;
        }

        public ResultSet GetRows(int max)
        {
            ResultSet rs = new ResultSet();
            List<FullColumnName> columnNames = new List<FullColumnName>();
            for (int n = 0; n < source.ColumnCount; n++)
                columnNames.Add(source.ColumnName(n));
            rs.SetColumnNames(columnNames);

            while (currentRow < source.RowCount && rs.RowCount < max)
            {
                ExpressionOperand[] thisRow = new ExpressionOperand[source.ColumnCount];

                for (int i = 0; i < source.ColumnCount; i++)
                {
                    thisRow[i] = ExpressionOperand.NVARCHARFromString(source.Row(currentRow)[i]);
                }

                rs.AddRow(thisRow);
                currentRow++;
            }

            return rs;
        }
    }
}