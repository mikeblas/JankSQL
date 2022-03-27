namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using JankSQL;
    using Engines = JankSQL.Engines;

    public class EngineIndexTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;

        [TestMethod]
        public void TestCreateIndex()
        {
            // create a non-unique index on a test table
            List<(string columnName, bool isDescending)> columnInfos = new()
            {
                ("is_even", false)
            };

            engine.CreateIndex(FullTableName.FromTableName("ten"), "evenIndex", false, columnInfos);

            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("ten"));
            Assert.IsNotNull(t);

            var idx = t.Index("evenIndex");
            Assert.IsNotNull(idx);

            foreach (var columnInfo in idx.IndexDefinition.ColumnInfos)
            {
                Console.Write($"{columnInfo.columnName}, ");
            }
            Console.WriteLine();

            foreach (var r in idx)
            {
                Console.WriteLine($"{r.RowData} ==> {r.Bookmark}");
            }
        }
    }
}
