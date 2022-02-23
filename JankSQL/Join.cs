using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JankSQL
{
    internal class Join : IComponentOutput
    {
        IComponentOutput leftInput;
        IComponentOutput rightInput;

        int leftIndex = -1;
        int rightIndex = -1;

        List<FullColumnName>? allColumnNames = null;

        ResultSet? leftRows = null;
        ResultSet? rightRows = null;

        internal Join()
        {
        }

        internal IComponentOutput LeftInput { get { return leftInput; } set { leftInput = value; } }

        internal IComponentOutput RightInput { get { return rightInput; } set { rightInput = value; } }

        internal List<List<ExpressionNode>> PredicateExpressions { get; set; }

        List<FullColumnName> GetAllColumnNames()
        {
            if (allColumnNames == null)
            {
                allColumnNames = new List<FullColumnName>();
                // allColumnNames.AddRange(leftRows.GetColumnNames());
                // allColumnNames.AddRange(rightRows.GetColumnNames());
            }

            return allColumnNames;
        }

        bool FillLeftRows(int max)
        {
            leftRows = leftInput.GetRows(max);
            leftIndex = 0;
            return (leftRows != null && leftRows.RowCount > 0);
        }

        bool FillRightRows(int max)
        {
            rightRows = rightInput.GetRows(max);
            rightIndex = 0;
            return (rightRows != null && rightRows.RowCount > 0);
        }


        public void Rewind()
        {
            leftRows = null;
            rightRows = null;
            // rightInput.Rewind();
            // leftInput.Rewind();
        }

        public ResultSet GetRows(int max)
        {
            ResultSet outputSet = new ResultSet();

            if (leftRows == null)
                FillLeftRows(max);
            if (rightRows == null)
                FillRightRows(max);

            outputSet.SetColumnNames(GetAllColumnNames());

            while (outputSet.RowCount < max && leftIndex < leftRows.RowCount && rightIndex < rightRows.RowCount)
            {
                ExpressionOperand[] totalRow = new ExpressionOperand[allColumnNames.Count];

                int outColumnCount = 0;
                for (int i = 0; i < leftRows.ColumnCount; i++)
                {
                    totalRow[outColumnCount++] = leftRows.Row(leftIndex)[i];
                }
                for (int i = 0; i < rightRows.ColumnCount; i++)
                {
                    totalRow[outColumnCount++] = rightRows.Row(rightIndex)[i];
                }
                outputSet.AddRow(totalRow);

                rightIndex += 1;
                if (rightIndex == rightRows.RowCount)
                {
                    if (!FillRightRows(max))
                    {

                    }
                    leftIndex += 1;
                }
                rightIndex += 1;
                leftIndex += 1;
            }

            return outputSet;
        }
    }
}
