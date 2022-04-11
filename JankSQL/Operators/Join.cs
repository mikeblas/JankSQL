namespace JankSQL.Operators
{
    using JankSQL.Contexts;
    using JankSQL.Expressions;

    internal class Join : IComponentOutput
    {
        private readonly JoinType joinType;

        //REVIEW: added to the right side only? Is that right?
        private readonly string? derivedTableAlias;

        private IComponentOutput leftInput;
        private IComponentOutput rightInput;
        private ResultSet? outputSet = null;
        private int outputIndex = 0;

        private int leftIndex = -1;
        private int rightIndex = -1;

        private List<FullColumnName>? allColumnNames = null;

        private ResultSet? leftRows = null;
        private ResultSet? rightRows = null;


        internal Join(JoinType joinType, IComponentOutput leftInput, IComponentOutput rightInput, List<Expression> predicateExpressions, string? derivedTableAlias)
        {
            this.joinType = joinType;
            this.leftInput = leftInput;
            this.rightInput = rightInput;
            this.PredicateExpressions = predicateExpressions;
            this.derivedTableAlias = derivedTableAlias;
        }

        internal IComponentOutput LeftInput
        {
            get { return leftInput; }
            set { leftInput = value; }
        }

        internal IComponentOutput RightInput
        {
            get { return rightInput; }
            set { rightInput = value; }
        }

        internal List<Expression> PredicateExpressions { get; set; }

        public void Rewind()
        {
            outputIndex = 0;
            // Console.WriteLine("REWIND!");
        }

        public ResultSet GetRows(int max)
        {
            if (outputSet is null)
                outputSet = ProduceOutputSet();

            ResultSet resultSlice = ResultSet.NewWithShape(outputSet);
            if (outputIndex >= outputSet.RowCount)
                resultSlice.MarkEOF();
            else
            {
                while (outputIndex < outputSet.RowCount && resultSlice.RowCount < max)
                {
                    resultSlice.AddRowFrom(outputSet, outputIndex);
                    outputIndex++;
                }
            }

            return resultSlice;
        }


        protected List<FullColumnName> GetAllColumnNames()
        {
            if (allColumnNames == null)
            {
                if (leftRows == null || rightRows == null)
                    throw new InternalErrorException("an expected rowset was null");

                allColumnNames = new List<FullColumnName>();
                foreach (var fcn in leftRows.GetColumnNames())
                {
                    //REVIEW: only the right side; is that correct?
                    // if (derivedTableAlias != null)
                    //  fcn.SetTableName(derivedTableAlias);
                    allColumnNames.Add(fcn);
                    // Console.WriteLine($"Left: {fcn}");
                }

                foreach (var fcn in rightRows.GetColumnNames())
                {
                    if (derivedTableAlias != null)
                        fcn.SetTableName(derivedTableAlias);
                    allColumnNames.Add(fcn);
                    // Console.WriteLine($"Right: {fcn}");
                }
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
            const int max = 7;

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
                    totalRow[outColumnCount++] = leftRows.Row(leftIndex)[i];
                for (int i = 0; i < rightRows.ColumnCount; i++)
                    totalRow[outColumnCount++] = rightRows.Row(rightIndex)[i];

                // see if we need to compute predicates ...
                bool matched;
                if (joinType == JoinType.CROSS_JOIN)
                    matched = true;
                else
                {
                    ExpressionOperand op = PredicateExpressions[0].Evaluate(new TemporaryRowValueAccessor(totalRow, allColumnNames));
                    matched = op.IsTrue();
                }

                // Console.WriteLine($"Join: {leftIndex + 1}/{leftRows.RowCount}, {rightIndex + 1}/{rightRows.RowCount}, {matched}");

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
                    throw new NotImplementedException($"don't know join type {joinType}");
                }

                rightIndex += 1;
                if (rightIndex == rightRows.RowCount)
                {
                    if (!FillRightRows(max))
                    {
                        rightInput.Rewind();
                        FillRightRows(max);

                        leftIndex += 1;
                        if (leftIndex == leftRows.RowCount)
                        {
                            if (!FillLeftRows(max))
                                break;
                            leftIndex = 0;
                        }
                    }

                    rightIndex = 0;
                }
            }

            // Console.WriteLine($"Output set has {output.RowCount} rows");
            return output;
        }
    }
}
