
namespace JankSQL
{
    internal class Delete : IComponentOutput
    {
        IComponentOutput myInput;
        Engines.IEngineDestination engineDestination;
        List<Expression> predicateExpressions;
        List<int> bookmarksToDelete = new();

        internal Delete(Engines.IEngineDestination targetTable, IComponentOutput input, List<Expression> predicateExpressions)
        {
            myInput = input;
            engineDestination = targetTable;
            this.predicateExpressions = predicateExpressions;
        }

        public ResultSet GetRows(int max)
        {
            ResultSet batch = myInput.GetRows(5);
            if (batch == null)
            {
                // last one was received, so let's delete now
                engineDestination.DeleteRows(bookmarksToDelete);
                return null;
            }

            ResultSet rsOutput = ResultSet.NewWithShape(batch);

            for (int i = 0; i < batch.RowCount; i++)
            {
                bool predicatePassed = true;
                foreach (var p in predicateExpressions)
                {
                    ExpressionOperand result = p.Evaluate(new RowsetValueAccessor(batch, i));

                    if (!result.IsTrue())
                    {
                        predicatePassed = false;
                        break;
                    }
                }

                if (!predicatePassed)
                    continue;

                // meets the predicate, so delete it
                int bookmarkIndex = batch.ColumnIndex(FullColumnName.FromColumnName("bookmark"));
                int bookmark = batch.Row(i)[bookmarkIndex].AsInteger();
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
