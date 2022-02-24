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
        JoinType joinType;

        int leftIndex = -1;
        int rightIndex = -1;

        List<FullColumnName>? allColumnNames = null;

        ResultSet? leftRows = null;
        ResultSet? rightRows = null;

        internal Join(JoinType joinType)
        {
            this.joinType = joinType;
        }

        internal IComponentOutput LeftInput { get { return leftInput; } set { leftInput = value; } }

        internal IComponentOutput RightInput { get { return rightInput; } set { rightInput = value; } }

        internal List<Expression> PredicateExpressions { get; set; }

        List<FullColumnName> GetAllColumnNames()
        {
            if (allColumnNames == null)
            {
                allColumnNames = new List<FullColumnName>();
                allColumnNames.AddRange(leftRows!.GetColumnNames());
                allColumnNames.AddRange(rightRows!.GetColumnNames());
            }

            return allColumnNames;
        }

        bool FillLeftRows(int max)
        {
            leftRows = leftInput.GetRows(max);
            return (leftRows != null && leftRows.RowCount > 0);
        }

        bool FillRightRows(int max)
        {
            rightRows = rightInput.GetRows(max);
            return (rightRows != null && rightRows.RowCount > 0);
        }


        public void Rewind()
        {
            leftRows = null;
            rightRows = null;
            rightInput.Rewind();
            leftInput.Rewind();
        }

        public ResultSet GetRows(int max)
        {
            ResultSet outputSet = new ResultSet();

            if (leftRows == null)
            {
                FillLeftRows(max);
                leftIndex = 0;
            }
            if (rightRows == null)
            {
                FillRightRows(max);
                rightIndex = 0;
            }

            outputSet.SetColumnNames(GetAllColumnNames());

            while (outputSet.RowCount < max && leftIndex < leftRows!.RowCount && rightIndex < rightRows!.RowCount)
            {
                ExpressionOperand[] totalRow = new ExpressionOperand[allColumnNames!.Count];

                int outColumnCount = 0;
                for (int i = 0; i < leftRows.ColumnCount; i++)
                {
                    totalRow[outColumnCount++] = leftRows.Row(leftIndex)[i];
                }
                for (int i = 0; i < rightRows.ColumnCount; i++)
                {
                    totalRow[outColumnCount++] = rightRows.Row(rightIndex)[i];
                }

                // see if we need to compute predicates ...
                bool matched;
                if (joinType == JoinType.CROSS_JOIN)
                    matched = true;
                else
                {
                    ExpressionOperand op = PredicateExpressions[0].Evaluate(new TemporaryRowValueAccessor(totalRow, allColumnNames));
                    matched = op.IsTrue();
                }

                Console.WriteLine($"{leftIndex}, {rightIndex}, {matched}");

                // depending on the join type, do the right thing.
                if (joinType == JoinType.INNER_JOIN)
                {
                    // add only if matched
                    if (matched)
                        outputSet.AddRow(totalRow);
                }
                else if (joinType == JoinType.CROSS_JOIN)
                {
                    // always add
                    outputSet.AddRow(totalRow);
                }
                else
                {
                    // NYI just now
                    throw new NotImplementedException();
                }

                rightIndex += 1;
                if (rightIndex == rightRows.RowCount)
                {
                    if (!FillRightRows(max))
                    {
                        rightInput.Rewind();
                        FillRightRows(max);
                        leftIndex += 1;
                    }
                    rightIndex = 0;
                }
            }

            return outputSet;
        }
    }
}
