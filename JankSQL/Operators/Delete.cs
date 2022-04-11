namespace JankSQL.Operators
{
    using JankSQL.Expressions;

    internal class Delete : IComponentOutput
    {
        private readonly IComponentOutput myInput;
        private readonly Engines.IEngineTable engineSource;
        private readonly List<ExpressionOperandBookmark> bookmarksToDelete = new ();
        private readonly Expression? predicateExpression;

        internal Delete(Engines.IEngineTable targetTable, IComponentOutput input, Expression? predicateExpression)
        {
            myInput = input;
            engineSource = targetTable;
            this.predicateExpression = predicateExpression;
        }

        public ResultSet GetRows(int max)
        {
            ResultSet batch = myInput.GetRows(5);
            ResultSet rsOutput = ResultSet.NewWithShape(batch);

            if (batch.IsEOF)
            {
                // last one was received, so let's delete now
                engineSource.DeleteRows(bookmarksToDelete);
                rsOutput.MarkEOF();
                return rsOutput;
            }

            for (int i = 0; i < batch.RowCount; i++)
            {
                bool predicatePassed = true;
                if (predicateExpression != null)
                {
                    ExpressionOperand result = predicateExpression.Evaluate(new ResultSetValueAccessor(batch, i));
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
