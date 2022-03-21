namespace JankSQL.Operators
{
    internal class Delete : IComponentOutput
    {
        readonly IComponentOutput myInput;
        readonly Engines.IEngineTable engineSource;
        readonly List<Expression> predicateExpressions;
        readonly List<ExpressionOperandBookmark> bookmarksToDelete = new ();

        internal Delete(Engines.IEngineTable targetTable, IComponentOutput input, List<Expression> predicateExpressions)
        {
            myInput = input;
            engineSource = targetTable;
            this.predicateExpressions = predicateExpressions;
        }

        public ResultSet? GetRows(int max)
        {
            ResultSet? batch = myInput.GetRows(5);
            if (batch == null)
            {
                // last one was received, so let's delete now
                engineSource.DeleteRows(bookmarksToDelete);
                return null;
            }

            ResultSet rsOutput = ResultSet.NewWithShape(batch);

            for (int i = 0; i < batch.RowCount; i++)
            {
                bool predicatePassed = true;
                foreach (var p in predicateExpressions)
                {
                    ExpressionOperand result = p.Evaluate(new ResultSetValueAccessor(batch, i));

                    if (!result.IsTrue())
                    {
                        predicatePassed = false;
                        break;
                    }
                }

                if (!predicatePassed)
                    continue;

                // meets the predicate, so delete it
                int bookmarkIndex = batch.ColumnIndex(FullColumnName.FromColumnName("bookmark_key"));
                bookmarksToDelete.Add((ExpressionOperandBookmark) batch.Row(i)[bookmarkIndex]);
            }

            return rsOutput;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}
