
namespace JankSQL
{
    internal class DeleteContext : IExecutableContext
    {
        FullTableName tableName;
        PredicateContext predicateContext;

        internal DeleteContext(FullTableName tableName)
        {
            this.tableName = tableName;
        }

        internal PredicateContext PredicateContext { get { return predicateContext; } set { predicateContext = value; } }

        public void Dump()
        {
            Console.WriteLine($"DELETE FROM {tableName}");

            if (predicateContext.PredicateExpressionListCount == 0)
            {
                Console.WriteLine("   no predicates");
            }
            else
            {
                for (int i = 0; i < predicateContext.PredicateExpressionListCount; i++)
                {
                    Console.WriteLine($"    {predicateContext.PredicateExpressions[i]}");
                }
            }
        }

        public ExecuteResult Execute()
        {
            ExecuteResult results = new ExecuteResult();

            // get systables
            Engines.DynamicCSV sysTables = new Engines.DynamicCSV("sys_tables.csv", "sys_tables");
            sysTables.Load();

            // get the file name for our table
            string? effectiveTableFileName = Engines.DynamicCSV.FileFromSysTables(sysTables, tableName.TableName);

            if (effectiveTableFileName == null)
            {
                throw new ExecutionException($"Table {tableName} does not exist");
            }
            else
            {
                // found the source table, so load it
                Engines.DynamicCSV table = new Engines.DynamicCSV(effectiveTableFileName, tableName.TableName);

                List<int> bookmarksToDelete = new List<int>();

                TableSource source = new TableSource(table);

                while (true)
                {
                    ResultSet batch = source.GetRows(5);
                    if (batch.RowCount == 0)
                        break;

                    for (int i = 0; i < batch.RowCount; i++)
                    {
                        bool predicatePassed = true;
                        foreach (var p in predicateContext.PredicateExpressions)
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
                }

                table.DeleteRows(bookmarksToDelete);
            }

            return results;
        }
    }
}
