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

            string s = String.Join(",", idx.IndexDefinition.ColumnInfos.Select(x => $"[{x.columnName}, {(x.isDescending ? "DESC" : "ASC")}]"));
            Console.WriteLine(s);

            foreach (var r in idx)
            {
                Console.WriteLine($"{r.RowData} ==> {r.Bookmark}");
            }


            int oddCount = 0;
            int evenCount = 0;
            int polarityIndex = idx.IndexDefinition.ColumnIndex("is_even");
            foreach (var indexRow in idx)
            {
                int p = indexRow.RowData[polarityIndex].AsInteger();
                if (p == 0)
                {
                    oddCount += 1;
                    Assert.AreEqual(0, evenCount, "Odds must come before any even");
                }
                else if (p == 1)
                {
                    evenCount += 1;
                    Assert.AreEqual(5, oddCount, "Evens must come after all odds");
                }
                else
                    Assert.Fail($"Didn't expect is_even value {p}");
            }

            Assert.AreEqual(5, evenCount);
            Assert.AreEqual(5, oddCount);
        }


        [TestMethod]
        public void TestCreateInsertIndex()
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

            Tuple newRow = Tuple.CreateEmpty(4);
            newRow[0] = ExpressionOperand.IntegerFromInt(11);
            newRow[1] = ExpressionOperand.NVARCHARFromString("eleven");
            newRow[2] = ExpressionOperand.IntegerFromInt(0);

            t.InsertRow(newRow);

            var idx = t.Index("evenIndex");
            Assert.IsNotNull(idx);

            string s = String.Join(",", idx.IndexDefinition.ColumnInfos.Select(x => $"[{x.columnName}, {(x.isDescending ? "DESC" : "ASC")}]"));
            Console.WriteLine(s);

            foreach (var r in idx)
            {
                Console.WriteLine($"{r.RowData} ==> {r.Bookmark}");
            }


            int oddCount = 0;
            int evenCount = 0;
            int polarityIndex = idx.IndexDefinition.ColumnIndex("is_even");
            foreach (var indexRow in idx)
            {
                int p = indexRow.RowData[polarityIndex].AsInteger();
                if (p == 0)
                {
                    oddCount += 1;
                    Assert.AreEqual(0, evenCount, "Odds must come before any even");
                }
                else if (p == 1)
                {
                    evenCount += 1;
                    Assert.AreEqual(6, oddCount, "Evens must come after all odds");
                }
                else
                    Assert.Fail($"Didn't expect is_even value {p}");
            }

            Assert.AreEqual(6, oddCount);
            Assert.AreEqual(5, evenCount);
        }
    }
}

