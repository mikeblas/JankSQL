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
        ResultSet? outputSet = null;
        int outputIndex = 0;

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
            outputIndex = 0;
        }

        public ResultSet GetRows(int max)
        {
            if (outputSet is null)
                outputSet = ProduceOutputSet();

            ResultSet resultSlice = ResultSet.NewWithShape(outputSet);

            while (outputIndex < outputSet.RowCount && resultSlice.RowCount < max)
            {
                resultSlice.AddRowFrom(outputSet, outputIndex);
                outputIndex++;
            }

            return resultSlice;
        }

        ResultSet ProduceOutputSet()
        { 
            ResultSet output = new ResultSet();

            const int max = 25;

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

            output.SetColumnNames(GetAllColumnNames());

            while (leftIndex < leftRows!.RowCount && rightIndex < rightRows!.RowCount)
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
                        output.AddRow(totalRow);
                }
                else if (joinType == JoinType.CROSS_JOIN)
                {
                    // always add
                    output.AddRow(totalRow);
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

            return output;
        }
    }
}
