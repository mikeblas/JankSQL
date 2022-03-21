﻿
using JankSQL.Contexts;

namespace JankSQL
{
    internal class Update : IComponentOutput
    {
        readonly IComponentOutput myInput;
        readonly Engines.IEngineTable engineTable;
        readonly List<Expression> predicateExpressions;
        readonly List<ExpressionOperandBookmark> bookmarksToDelete = new();
        readonly List<ExpressionOperand[]> rowsToInsert = new();
        readonly List<SetOperation> setList;

        internal Update(Engines.IEngineTable targetTable, IComponentOutput input, List<Expression> predicateExpressions, List<SetOperation> setList)
        {
            myInput = input;
            this.engineTable = targetTable;
            this.predicateExpressions = predicateExpressions;
            this.setList = setList;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }

        public ResultSet? GetRows(int max)
        {
            ResultSet? batch = myInput.GetRows(5);
            if (batch == null)
            {
                // last one was received, so let's do the updating work now
                DoUpdateWork();
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
                int bookmarkIndex = batch.ColumnIndex(FullColumnName.FromColumnName("bookmark_key"));
                int bookmark = batch.Row(i)[bookmarkIndex].AsInteger();
                bookmarksToDelete.Add(ExpressionOperandBookmark.FromInteger(bookmark));

                // and build a replacement row
                ExpressionOperand[] modified = new ExpressionOperand[batch.ColumnCount-1];
                List<FullColumnName> outputColumns = new();

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
                    set.Execute(new TemporaryRowValueAccessor(modified, batch.GetColumnNames()), new RowsetValueAccessor(batch, i));
                }
                rowsToInsert.Add(modified);
            }

            return rsOutput;
        }


        void DoUpdateWork()
        {
            // first, delete everything we want
            engineTable.DeleteRows(bookmarksToDelete);

            // then, insert the modified rows
            foreach (var row in rowsToInsert)
            {
                engineTable.InsertRow(row);
            }
        }
    }
}

