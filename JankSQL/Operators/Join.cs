namespace JankSQL.Operators
{
    using JankSQL.Contexts;

    internal class Join : IComponentOutput
    {
        private readonly JoinType joinType;

        private IComponentOutput leftInput;
        private IComponentOutput rightInput;
        private ResultSet? outputSet = null;
        private int outputIndex = 0;

        private int leftIndex = -1;
        private int rightIndex = -1;

        private List<FullColumnName>? allColumnNames = null;

        private ResultSet? leftRows = null;
        private ResultSet? rightRows = null;

        internal Join(JoinType joinType, IComponentOutput leftInput, IComponentOutput rightInput, List<Expression> predicateExpressions)
        {
            this.joinType = joinType;
            this.leftInput = leftInput;
            this.rightInput = rightInput;
            this.PredicateExpressions = predicateExpressions;
        }

        internal IComponentOutput LeftInput
        {
            get { return leftInput; } set { leftInput = value; }
        }

        internal IComponentOutput RightInput
        {
            get { return rightInput; } set { rightInput = value; }
        }

        internal List<Expression> PredicateExpressions { get; set; }

        public void Rewind()
        {
            outputIndex = 0;
        }

        public ResultSet? GetRows(int max)
        {
            if (outputSet is null)
                outputSet = ProduceOutputSet();

            if (outputIndex >= outputSet.RowCount)
                return null;

            ResultSet resultSlice = ResultSet.NewWithShape(outputSet);

            while (outputIndex < outputSet.RowCount && resultSlice.RowCount < max)
            {
                resultSlice.AddRowFrom(outputSet, outputIndex);
                outputIndex++;
            }

            return resultSlice;
        }


        protected List<FullColumnName> GetAllColumnNames()
        {
            if (allColumnNames == null)
            {
                allColumnNames = new List<FullColumnName>();
                allColumnNames.AddRange(leftRows!.GetColumnNames());
                allColumnNames.AddRange(rightRows!.GetColumnNames());
            }

            return allColumnNames;
        }

        protected bool FillLeftRows(int max)
        {
            leftRows = leftInput.GetRows(max);
            return leftRows != null && leftRows.RowCount > 0;
        }

        protected bool FillRightRows(int max)
        {
            rightRows = rightInput.GetRows(max);
            return rightRows != null && rightRows.RowCount > 0;
        }

        protected ResultSet ProduceOutputSet()
        {
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

            ResultSet output = new (GetAllColumnNames());

            while (leftIndex < leftRows!.RowCount && rightIndex < rightRows!.RowCount)
            {
                Tuple totalRow = Tuple.CreateEmpty(allColumnNames!.Count);

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
