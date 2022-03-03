using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal class ConstantRowSource : IComponentOutput
    {
        int currentRow = 0;
        List<List<Expression>> columnValues;
        List<FullColumnName> columnNames;

        internal ConstantRowSource(List<FullColumnName> columnNames, List<List<Expression>> columnValues)
        {
            this.currentRow = 0;
            this.columnValues = columnValues;
            this.columnNames = columnNames;
        }

        public ResultSet GetRows(int max)
        {
            ResultSet resultSet = new ResultSet();
            resultSet.SetColumnNames(columnNames);

            int t = 0;
            while (t < max && currentRow < columnValues.Count)
            {
                ExpressionOperand[] generatedValues = new ExpressionOperand[columnValues[0].Count];    

                for (int i = 0; i < columnValues[currentRow].Count; i++)
                {
                    generatedValues[i] = columnValues[currentRow][i].Evaluate(null);
                }

                resultSet.AddRow(generatedValues);

                currentRow++;
            }

            return resultSet;
        }

        public void Rewind()
        {
            currentRow = 0;
        }
    }
}
