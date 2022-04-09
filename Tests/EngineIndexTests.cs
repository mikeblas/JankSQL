namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using JankSQL;
    using Engines = JankSQL.Engines;

    public class EngineIndexTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
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

            idx.Dump();

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
        public void TestCreateDescIndex()
        {
            // create a non-unique index on a test table
            List<(string columnName, bool isDescending)> columnInfos = new()
            {
                ("is_even", true)
            };

            engine.CreateIndex(FullTableName.FromTableName("ten"), "evenIndex", false, columnInfos);

            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("ten"));
            Assert.IsNotNull(t);

            var idx = t.Index("evenIndex");
            Assert.IsNotNull(idx);
            idx.Dump();

            int oddCount = 0;
            int evenCount = 0;
            int polarityIndex = idx.IndexDefinition.ColumnIndex("is_even");
            foreach (var indexRow in idx)
            {
                int p = indexRow.RowData[polarityIndex].AsInteger();
                if (p == 0)
                {
                    oddCount += 1;
                    Assert.AreEqual(5, evenCount, "Odds must come before all evens");
                }
                else if (p == 1)
                {
                    evenCount += 1;
                    Assert.AreEqual(0, oddCount, "Evens must come before all odds");
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

            Tuple newRow = Tuple.CreateEmpty(3);
            newRow[0] = ExpressionOperand.IntegerFromInt(11);
            newRow[1] = ExpressionOperand.VARCHARFromString("eleven");
            newRow[2] = ExpressionOperand.IntegerFromInt(0);

            t.InsertRow(newRow);

            var idx = t.Index("evenIndex");
            Assert.IsNotNull(idx);
            idx.Dump();

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


        [TestMethod]
        public void TestCreateInsertDescIndex()
        {
            // create a non-unique index on a test table
            List<(string columnName, bool isDescending)> columnInfos = new()
            {
                ("is_even", true)
            };

            engine.CreateIndex(FullTableName.FromTableName("ten"), "evenIndex", false, columnInfos);

            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("ten"));
            Assert.IsNotNull(t);

            Tuple newRow = Tuple.CreateEmpty(3);
            newRow[0] = ExpressionOperand.IntegerFromInt(11);
            newRow[1] = ExpressionOperand.VARCHARFromString("eleven");
            newRow[2] = ExpressionOperand.IntegerFromInt(0);

            t.InsertRow(newRow);

            var idx = t.Index("evenIndex");
            Assert.IsNotNull(idx);
            idx.Dump();

            int oddCount = 0;
            int evenCount = 0;
            int polarityIndex = idx.IndexDefinition.ColumnIndex("is_even");
            foreach (var indexRow in idx)
            {
                int p = indexRow.RowData[polarityIndex].AsInteger();
                if (p == 0)
                {
                    oddCount += 1;
                    Assert.AreEqual(5, evenCount, "Odds must come before all evens");
                }
                else if (p == 1)
                {
                    evenCount += 1;
                    Assert.AreEqual(0, oddCount, "Evens must come before all odds");
                }
                else
                    Assert.Fail($"Didn't expect is_even value {p}");
            }

            Assert.AreEqual(6, oddCount);
            Assert.AreEqual(5, evenCount);
        }


        [TestMethod]
        public void TestCreateInsertTwoIndex()
        {
            // create a non-unique index on a test table
            List<(string columnName, bool isDescending)> columnInfos = new()
            {
                ("is_even", false),
                ("number_name", false),
            };

            engine.CreateIndex(FullTableName.FromTableName("ten"), "evenIndex", false, columnInfos);

            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("ten"));
            Assert.IsNotNull(t);

            Tuple newRow = Tuple.CreateEmpty(3);
            newRow[0] = ExpressionOperand.IntegerFromInt(11);
            newRow[1] = ExpressionOperand.VARCHARFromString("eleven");
            newRow[2] = ExpressionOperand.IntegerFromInt(0);

            t.InsertRow(newRow);

            var idx = t.Index("evenIndex");
            Assert.IsNotNull(idx);
            idx.Dump();

            int oddCount = 0;
            int evenCount = 0;
            string? lastName = null;
            int lastP = -1;
            int polarityIndex = idx.IndexDefinition.ColumnIndex("is_even");
            int nameIndex = idx.IndexDefinition.ColumnIndex("number_name");
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

                string thisName = indexRow.RowData[nameIndex].AsString();
                if (lastName != null)
                {
                    int diff = lastName.CompareTo(thisName);
                    if (lastP == p)
                        Assert.IsTrue(diff < 0);
                    else
                        Assert.IsTrue(diff > 0);
                }
                lastName = thisName;
                lastP = p;
            }

            Assert.AreEqual(6, oddCount);
            Assert.AreEqual(5, evenCount);
        }


        [TestMethod]
        [ExpectedException(typeof(ExecutionException), "Expected error from duplicate key")]
        public void TestFailCreateUniqueIndex()
        {
            // create a unique index on a test table, expecting failure
            List<(string columnName, bool isDescending)> columnInfos = new()
            {
                ("is_even", false),
            };

            engine.CreateIndex(FullTableName.FromTableName("ten"), "evenIndex", true, columnInfos);
        }


        [TestMethod]
        [ExpectedException(typeof(ExecutionException), "Expected error from duplicate key")]
        public void TestFailCreateUniqueTwoIndex()
        {
            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("ten"));
            Assert.IsNotNull(t);

            // ... and add two-key duplicate row to our test table
            Tuple newRow = Tuple.CreateEmpty(3);
            newRow[0] = ExpressionOperand.IntegerFromInt(0);
            newRow[1] = ExpressionOperand.VARCHARFromString("zero");
            newRow[2] = ExpressionOperand.IntegerFromInt(1);

            t.InsertRow(newRow);

            // create a unique index on a test table, expecting failure
            List<(string columnName, bool isDescending)> columnInfos = new()
            {
                ("is_even", false),
                ("number_name", false),
            };

            engine.CreateIndex(FullTableName.FromTableName("ten"), "evenIndex", true, columnInfos);
        }


        [TestMethod]
        public void TestCreateUniqueTwoIndex()
        {
            // create a non-unique index on a test table
            List<(string columnName, bool isDescending)> columnInfos = new()
            {
                ("is_even", false),
                ("number_name", false),
            };

            engine.CreateIndex(FullTableName.FromTableName("ten"), "evenNameIndex", true, columnInfos);

            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("ten"));
            Assert.IsNotNull(t);

            var idx = t.Index("evenNameIndex");
            Assert.IsNotNull(idx);
            Console.WriteLine("Here");
            idx.Dump();

            int oddCount = 0;
            int evenCount = 0;
            string? lastName = null;
            int lastP = -1;
            int polarityIndex = idx.IndexDefinition.ColumnIndex("is_even");
            int nameIndex = idx.IndexDefinition.ColumnIndex("number_name");
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

                string thisName = indexRow.RowData[nameIndex].AsString();
                if (lastName != null)
                {
                    int diff = lastName.CompareTo(thisName);
                    if (lastP == p)
                        Assert.IsTrue(diff < 0);
                    else
                        Assert.IsTrue(diff > 0);
                }
                lastName = thisName;
                lastP = p;
            }

            Assert.AreEqual(5, oddCount);
            Assert.AreEqual(5, evenCount);
        }


        [TestMethod]
        public void TestCreateTwoIndex()
        {
            // create a non-unique index on a test table
            List<(string columnName, bool isDescending)> columnInfos = new()
            {
                ("is_even", false),
                ("number_name", false),
            };

            engine.CreateIndex(FullTableName.FromTableName("ten"), "evenNameIndex", false, columnInfos);

            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("ten"));
            Assert.IsNotNull(t);

            var idx = t.Index("evenNameIndex");
            Assert.IsNotNull(idx);
            Console.WriteLine("Here");
            idx.Dump();

            int oddCount = 0;
            int evenCount = 0;
            string? lastName = null;
            int lastP = -1;
            int polarityIndex = idx.IndexDefinition.ColumnIndex("is_even");
            int nameIndex = idx.IndexDefinition.ColumnIndex("number_name");
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

                string thisName = indexRow.RowData[nameIndex].AsString();
                if (lastName != null)
                {
                    int diff = lastName.CompareTo(thisName);
                    if (lastP == p)
                        Assert.IsTrue(diff < 0);
                    else
                        Assert.IsTrue(diff > 0);
                }
                lastName = thisName;
                lastP = p;
            }

            Assert.AreEqual(5, oddCount);
            Assert.AreEqual(5, evenCount);
        }


        [TestMethod]
        [ExpectedException(typeof(ExecutionException), "Expected error from duplicate key")]
        public void TestFailCreateSameNameTwoIndex()
        {
            // create a non-unique index on a test table
            List<(string columnName, bool isDescending)> columnInfos = new()
            {
                ("is_even", false),
                ("number_name", false),
            };

            // create it once
            try
            {
                engine.CreateIndex(FullTableName.FromTableName("ten"), "evenNameIndex", false, columnInfos);
            }
            catch (Exception ex)
            {
                Assert.Fail($"unexpected exception {ex}");
            }

            // create it again, should fail
            engine.CreateIndex(FullTableName.FromTableName("ten"), "evenNameIndex", false, columnInfos);
        }
    }
}

