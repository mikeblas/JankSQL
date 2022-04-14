﻿namespace JankSQL.Operators
{
    using JankSQL.Contexts;
    using JankSQL.Expressions;

    internal class Update : IComponentOutput
    {
        private readonly IComponentOutput myInput;
        private readonly Engines.IEngineTable engineTable;
        private readonly Expression? predicateExpression;
        private readonly List<ExpressionOperandBookmark> bookmarksToDelete = new ();
        private readonly List<Tuple> rowsToInsert = new ();
        private readonly List<SetOperation> setList;

        private int rowsAffected;

        internal Update(Engines.IEngineTable targetTable, IComponentOutput input, Expression? predicateExpression, List<SetOperation> setList)
        {
            myInput = input;
            engineTable = targetTable;
            this.predicateExpression = predicateExpression;
            this.setList = setList;
        }

        internal int RowsAffected
        {
            get { return rowsAffected; }
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }

        public ResultSet GetRows(Engines.IEngine engine, IRowValueAccessor? outerAccessor, int max)
        {
            ResultSet batch = myInput.GetRows(engine, outerAccessor, 5);
            ResultSet rsOutput = ResultSet.NewWithShape(batch);

            if (batch.IsEOF)
            {
                // last one was received, so let's do the updating work now
                DoUpdateWork();
                rsOutput.MarkEOF();
                return rsOutput;
            }

            for (int i = 0; i < batch.RowCount; i++)
            {
                bool predicatePassed = true;
                if (predicateExpression != null)
                {
                    var accessor = new CombinedValueAccessor(new ResultSetValueAccessor(batch, i), outerAccessor);
                    ExpressionOperand result = predicateExpression.Evaluate(accessor, engine);
                    predicatePassed = result.IsTrue();
                }

                if (!predicatePassed)
                    continue;

                // meets the predicate, so delete it
                int bookmarkIndex = batch.ColumnIndex(FullColumnName.FromColumnName("bookmark_key"));
                ExpressionOperandBookmark? bookmark = batch.Row(i)[bookmarkIndex] as ExpressionOperandBookmark;
                if (bookmark == null)
                    throw new InternalErrorException("Expected bookmark column at index {bookmarkIndex}");
                bookmarksToDelete.Add(bookmark);

                // and build a replacement row
                Tuple modified = Tuple.CreateEmpty(batch.ColumnCount - 1);
                List<FullColumnName> outputColumns = new ();

                for (int col = 0, j = 0; j < batch.ColumnCount; j++)
                {
                    if (j == bookmarkIndex)
                        continue;
                    modified[col] = batch.Row(i)[j];
                    outputColumns.Add(batch.GetColumnName(j));
                    col++;
                }

                foreach (var set in setList)
                {
                    var accessor = new CombinedValueAccessor(new ResultSetValueAccessor(batch, i), outerAccessor);
                    set.Execute(engine, new TemporaryRowValueAccessor(modified, batch.GetColumnNames()), accessor);
                }

                rowsToInsert.Add(modified);
            }

            return rsOutput;
        }


        protected void DoUpdateWork()
        {
            // first, delete everything we want
            engineTable.DeleteRows(bookmarksToDelete);

            // then, insert the modified rows
            foreach (var row in rowsToInsert)
                engineTable.InsertRow(row);

            rowsAffected = bookmarksToDelete.Count;
        }
    }
}

