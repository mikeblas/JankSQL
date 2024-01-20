namespace JankSQL.Operators
{
    using JankSQL.Contexts;
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class Join : IOperatorOutput
    {
        private readonly JoinType joinType;

        //REVIEW: added to the right side only? Is that right?
        private readonly string? derivedTableAlias;
        private ResultSet? outputSet = null;
        private int outputIndex = 0;

        private int leftIndex = -1;
        private int rightIndex = -1;

        private List<FullColumnName>? allColumnNames = null;

        private ResultSet? leftRows = null;
        private ResultSet? rightRows = null;


        internal Join(JoinType joinType, IOperatorOutput leftInput, IOperatorOutput rightInput, List<Expression> predicateExpressions, string? derivedTableAlias)
        {
            this.joinType = joinType;

            // If we have a RIGHT OUTER JOIN, the inputs are switcharooed and we do the same work as a LEFT OUTER JOIN
            // This seems oogy, but I'll fix it as soon as it blows up. I promise! (But really, it seems like any other
            // solution is actually more confusing.)
            if (joinType == JoinType.RIGHT_OUTER_JOIN)
            {
                this.LeftInput = rightInput;
                this.RightInput = leftInput;
            }
            else
            {
                this.LeftInput = leftInput;
                this.RightInput = rightInput;
            }

            PredicateExpressions = predicateExpressions;
            this.derivedTableAlias = derivedTableAlias;
        }

        internal IOperatorOutput LeftInput { get; set; }

        internal IOperatorOutput RightInput { get; set; }

        internal List<Expression> PredicateExpressions { get; set; }

        public void Rewind()
        {
            outputIndex = 0;
            // Console.WriteLine("REWIND!");
        }

        public FullColumnName[] GetOutputColumnNames()
        {
            FullColumnName[] leftColumns = LeftInput.GetOutputColumnNames();
            FullColumnName[] rightColumns = RightInput.GetOutputColumnNames();

            List<FullColumnName> allCols = new ();
            foreach (var fcn in leftColumns)
            {
                //REVIEW: only the right side; is that correct?
                // if (derivedTableAlias != null)
                //  fcn.SetTableName(derivedTableAlias);
                allCols.Add(fcn);
                Console.WriteLine($"Left: {fcn}");
            }

            foreach (var fcn in rightColumns)
            {
                FullColumnName effective = fcn;
                if (derivedTableAlias != null)
                    effective = effective.ApplyTableAlias(derivedTableAlias);
                allCols.Add(effective);
                Console.WriteLine($"Right: {effective}");
            }

            return allCols.ToArray();
        }

        public BindResult Bind(IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            BindResult br = this.LeftInput.Bind(engine, outerColumnNames, bindValues);
            if (!br.IsSuccessful)
                return br;
            br = RightInput.Bind(engine, outerColumnNames, bindValues);
            if (!br.IsSuccessful)
                return br;

            return BindResult.Success();
        }

        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, Dictionary<string, ExpressionOperand> bindValues)
        {
            if (outputSet is null)
                outputSet = ProduceOutputSet(engine, outerAccessor, bindValues);

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
                    throw new InternalErrorException("an expected row set was null");

                allColumnNames = new List<FullColumnName>();
                foreach (var fcn in leftRows.GetColumnNames())
                {
                    //REVIEW: only the right side; is that correct?
                    // if (derivedTableAlias != null)
                    //  fcn.SetTableName(derivedTableAlias);
                    allColumnNames.Add(fcn);
                    Console.WriteLine($"Left: {fcn}");
                }

                foreach (var fcn in rightRows.GetColumnNames())
                {
                    FullColumnName effective = fcn;
                    if (derivedTableAlias != null)
                        effective = effective.ApplyTableAlias(derivedTableAlias);
                    allColumnNames.Add(effective);
                    Console.WriteLine($"Right: {effective}");
                }
            }

            return allColumnNames;
        }

        protected bool FillLeftRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, Dictionary<string, ExpressionOperand> bindValues)
        {
            leftRows = LeftInput.GetRows(engine, outerAccessor, max, bindValues);
            return leftRows != null && leftRows.RowCount > 0;
        }

        protected bool FillRightRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, Dictionary<string, ExpressionOperand> bindValues)
        {
            rightRows = RightInput.GetRows(engine, outerAccessor, max, bindValues);
            return rightRows != null && rightRows.RowCount > 0;
        }

        protected ResultSet ProduceOutputSet(Engines.IEngine engine, IRowValueAccessor? outerAccessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            const int max = 7;

            if (leftRows == null)
            {
                FillLeftRows(engine, outerAccessor, max, bindValues);
                leftIndex = 0;
            }

            if (rightRows == null)
            {
                FillRightRows(engine, outerAccessor, max, bindValues);
                rightIndex = 0;
            }

            ResultSet output = new (GetAllColumnNames());

            bool leftMatched = false;

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
                    ExpressionOperand op = PredicateExpressions[0].Evaluate(new TemporaryRowValueAccessor(totalRow, allColumnNames), engine, bindValues);
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
                else if (joinType == JoinType.LEFT_OUTER_JOIN || joinType == JoinType.RIGHT_OUTER_JOIN)
                {
                    if (matched)
                    {
                        output.AddRow(totalRow);
                        leftMatched = true;
                    }
                }
                else
                {
                    throw new NotImplementedException($"don't know join type {joinType}");
                }

                rightIndex += 1;
                if (rightIndex == rightRows.RowCount)
                {
                    if (!FillRightRows(engine, outerAccessor, max, bindValues))
                    {
                        // we exhausted right, so rewind ...
                        RightInput.Rewind();
                        FillRightRows(engine, outerAccessor, max, bindValues);

                        // was left ever matched? handle LEFT/RIGHT OUTER JOIN accordingly ...
                        if ((joinType == JoinType.LEFT_OUTER_JOIN || joinType == JoinType.RIGHT_OUTER_JOIN) && !leftMatched)
                        {
                            // wipe the right hand rows to NULL, pass the left hand rows out
                            for (int i = leftRows.ColumnCount; i < leftRows.ColumnCount + rightRows.ColumnCount; i++)
                                totalRow[i++] = ExpressionOperand.NullLiteral();
                            output.AddRow(totalRow);
                        }

                        // and now advance the left side
                        leftMatched = false;

                        leftIndex += 1;
                        if (leftIndex == leftRows.RowCount)
                        {
                            // we exhausted the left side, so try to get more
                            if (!FillLeftRows(engine, outerAccessor, max, bindValues))
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
