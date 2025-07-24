namespace JankSQL.Operators
{
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class Delete : IOperatorOutput
    {
        private readonly IOperatorOutput myInput;
        private readonly Engines.IEngineTable engineSource;
        private readonly List<ExpressionOperandBookmark> bookmarksToDelete = new ();
        private readonly Expression? predicateExpression;

        private int rowsAffected;

        internal Delete(Engines.IEngineTable targetTable, IOperatorOutput input, Expression? predicateExpression)
        {
            myInput = input;
            engineSource = targetTable;
            this.predicateExpression = predicateExpression;
        }

        internal int RowsAffected
        {
            get { return rowsAffected; }
        }

        public FullColumnName[] GetOutputColumnNames()
        {
            return myInput.GetOutputColumnNames();
        }

        public BindResult Bind(IEngine engine, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            BindResult br = myInput.Bind(engine, outerColumnNames, bindValues);
            if (!br.IsSuccessful)
                return br;

            FullColumnName[] inputColumnNames = myInput.GetOutputColumnNames();
            if (predicateExpression != null)
            {
                br = predicateExpression.Bind(engine, inputColumnNames, outerColumnNames, bindValues);
                if (!br.IsSuccessful)
                    return br;
            }
            return BindResult.Success();
        }


        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max, IDictionary<string, ExpressionOperand> bindValues)
        {
            ResultSet batch = myInput.GetRows(engine, outerAccessor, 5, bindValues);
            ResultSet rsOutput = ResultSet.NewWithShape(batch);

            if (batch.IsEOF)
            {
                // last one was received, so let's delete now
                rowsAffected = engineSource.DeleteRows(bookmarksToDelete);
                rsOutput.MarkEOF();
                return rsOutput;
            }

            for (int i = 0; i < batch.RowCount; i++)
            {
                bool predicatePassed = true;
                if (predicateExpression != null)
                {
                    var accessor = new CombinedValueAccessor(new ResultSetValueAccessor(batch, i), outerAccessor);
                    ExpressionOperand result = predicateExpression.Evaluate(accessor, engine, bindValues);
                    predicatePassed = result.IsTrue();
                }

                if (!predicatePassed)
                    continue;

                // meets the predicate, so delete it
                int bookmarkIndex = batch.ColumnIndex(FullColumnName.FromColumnName("bookmark_key"));
                if (batch.Row(i)[bookmarkIndex] is not ExpressionOperandBookmark bookmark)
                    throw new InternalErrorException("Expected bookmark column at index {bookmarkIndex}");
                bookmarksToDelete.Add(bookmark);
            }

            return rsOutput;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}
