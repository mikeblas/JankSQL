namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;

    abstract public class EngineIndexTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
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
            Assert.That(t, Is.Not.Null);

            var idx = t!.Index("evenIndex");
            Assert.That(idx, Is.Not.Null);

            idx!.Dump();

            int oddCount = 0;
            int evenCount = 0;
            int polarityIndex = idx.IndexDefinition.ColumnIndex("is_even");
            foreach (var indexRow in idx)
            {
                int p = indexRow.RowData[polarityIndex].AsInteger();
                if (p == 0)
                {
                    oddCount += 1;
                    Assert.That(evenCount, Is.Zero, "Odds must come before any even");
                }
                else if (p == 1)
                {
                    evenCount += 1;
                    Assert.That(oddCount, Is.EqualTo(5), "Evens must come after all odds");
                }
                else
                    Assert.Fail($"Didn't expect is_even value {p}");
            }

            Assert.That(oddCount, Is.EqualTo(5));
            Assert.That(evenCount, Is.EqualTo(5));
        }

        [Test]
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
            Assert.That(t, Is.Not.Null);

            var idx = t!.Index("evenIndex");
            Assert.That(idx, Is.Not.Null);
            idx!.Dump();

            int oddCount = 0;
            int evenCount = 0;
            int polarityIndex = idx.IndexDefinition.ColumnIndex("is_even");
            foreach (var indexRow in idx)
            {
                int p = indexRow.RowData[polarityIndex].AsInteger();
                if (p == 0)
                {
                    oddCount += 1;
                    Assert.That(evenCount, Is.EqualTo(5), "Odds must come before any even");
                }
                else if (p == 1)
                {
                    evenCount += 1;
                    Assert.That(oddCount, Is.Zero, "Evens must come before any even");
                }
                else
                    Assert.Fail($"Didn't expect is_even value {p}");
            }

            Assert.That(oddCount, Is.EqualTo(5));
            Assert.That(evenCount, Is.EqualTo(5));
        }


        [Test]
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
            Assert.That(t, Is.Not.Null);

            Tuple newRow = Tuple.CreateEmpty(3);
            newRow[0] = ExpressionOperand.IntegerFromInt(11);
            newRow[1] = ExpressionOperand.VARCHARFromString("eleven");
            newRow[2] = ExpressionOperand.IntegerFromInt(0);

            t!.InsertRow(newRow);

            var idx = t.Index("evenIndex");
            Assert.That(idx, Is.Not.Null);
            idx!.Dump();

            int oddCount = 0;
            int evenCount = 0;
            int polarityIndex = idx.IndexDefinition.ColumnIndex("is_even");
            foreach (var indexRow in idx)
            {
                int p = indexRow.RowData[polarityIndex].AsInteger();
                if (p == 0)
                {
                    oddCount += 1;
                    Assert.That(evenCount, Is.Zero, "Odds must come before any even");
                }
                else if (p == 1)
                {
                    evenCount += 1;
                    Assert.That(oddCount, Is.EqualTo(6), "Evens must come after all odds");
                }
                else
                    Assert.Fail($"Didn't expect is_even value {p}");
            }

            Assert.That(oddCount, Is.EqualTo(6));
            Assert.That(evenCount, Is.EqualTo(5));
        }


        [Test]
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
            Assert.That(t, Is.Not.Null);

            Tuple newRow = Tuple.CreateEmpty(3);
            newRow[0] = ExpressionOperand.IntegerFromInt(11);
            newRow[1] = ExpressionOperand.VARCHARFromString("eleven");
            newRow[2] = ExpressionOperand.IntegerFromInt(0);

            t!.InsertRow(newRow);

            var idx = t.Index("evenIndex");
            Assert.That(idx, Is.Not.Null);
            idx!.Dump();

            int oddCount = 0;
            int evenCount = 0;
            int polarityIndex = idx.IndexDefinition.ColumnIndex("is_even");
            foreach (var indexRow in idx)
            {
                int p = indexRow.RowData[polarityIndex].AsInteger();
                if (p == 0)
                {
                    oddCount += 1;
                    Assert.That(evenCount, Is.EqualTo(5), "Odds must come before all evens");
                }
                else if (p == 1)
                {
                    evenCount += 1;
                    Assert.That(oddCount, Is.Zero, "Evens must come before all odds");
                }
                else
                    Assert.Fail($"Didn't expect is_even value {p}");
            }

            Assert.That(oddCount, Is.EqualTo(6));
            Assert.That(evenCount, Is.EqualTo(5));
        }


        [Test]
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
            Assert.That(t, Is.Not.Null);

            Tuple newRow = Tuple.CreateEmpty(3);
            newRow[0] = ExpressionOperand.IntegerFromInt(11);
            newRow[1] = ExpressionOperand.VARCHARFromString("eleven");
            newRow[2] = ExpressionOperand.IntegerFromInt(0);

            t!.InsertRow(newRow);

            var idx = t.Index("evenIndex");
            Assert.That(idx, Is.Not.Null);
            idx!.Dump();

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
                    Assert.That(evenCount, Is.Zero, "Odds must come before any even");
                }
                else if (p == 1)
                {
                    evenCount += 1;
                    Assert.That(oddCount, Is.EqualTo(6), "Evens must come after all odds");
                }
                else
                    Assert.Fail($"Didn't expect is_even value {p}");

                string thisName = indexRow.RowData[nameIndex].AsString();
                if (lastName != null)
                {
                    int diff = lastName.CompareTo(thisName);
                    if (lastP == p)
                        Assert.That(diff, Is.LessThan(0));
                    else
                        Assert.That(diff, Is.GreaterThan(0));
                }
                lastName = thisName;
                lastP = p;
            }

            Assert.That(oddCount, Is.EqualTo(6));
            Assert.That(evenCount, Is.EqualTo(5));
        }


        [Test]
        public void TestFailCreateUniqueIndex()
        {
            // create a unique index on a test table, expecting failure
            List<(string columnName, bool isDescending)> columnInfos = new()
            {
                ("is_even", false),
            };

            Assert.Throws<ExecutionException>(() => engine.CreateIndex(FullTableName.FromTableName("ten"), "evenIndex", true, columnInfos));
        }


        [Test]
        public void TestFailCreateUniqueTwoIndex()
        {
            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("ten"));
            Assert.That(t, Is.Not.Null);

            // ... and add two-key duplicate row to our test table
            Tuple newRow = Tuple.CreateEmpty(3);
            newRow[0] = ExpressionOperand.IntegerFromInt(0);
            newRow[1] = ExpressionOperand.VARCHARFromString("zero");
            newRow[2] = ExpressionOperand.IntegerFromInt(1);

            t!.InsertRow(newRow);

            // create a unique index on a test table, expecting failure
            List<(string columnName, bool isDescending)> columnInfos = new()
            {
                ("is_even", false),
                ("number_name", false),
            };

            Assert.Throws<ExecutionException>(() => engine.CreateIndex(FullTableName.FromTableName("ten"), "evenIndex", true, columnInfos));
        }


        [Test]
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
            Assert.That(t, Is.Not.Null);

            var idx = t!.Index("evenNameIndex");
            Assert.That(idx, Is.Not.Null);
            Console.WriteLine("Here");
            idx!.Dump();

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
                    Assert.That(evenCount, Is.Zero, "Odds must come before any even");
                }
                else if (p == 1)
                {
                    evenCount += 1;
                    Assert.That(oddCount, Is.EqualTo(5), "Evens must come after all odds");
                }
                else
                    Assert.Fail($"Didn't expect is_even value {p}");

                string thisName = indexRow.RowData[nameIndex].AsString();
                if (lastName != null)
                {
                    int diff = lastName.CompareTo(thisName);
                    if (lastP == p)
                        Assert.That(diff, Is.LessThan(0));
                    else
                        Assert.That(diff, Is.GreaterThan(0));
                }
                lastName = thisName;
                lastP = p;
            }

            Assert.That(oddCount, Is.EqualTo(5));
            Assert.That(evenCount, Is.EqualTo(5));
        }


        [Test]
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
            Assert.That(t, Is.Not.Null);

            var idx = t!.Index("evenNameIndex");
            Assert.That(idx, Is.Not.Null);
            Console.WriteLine("Here");
            idx!.Dump();

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
                    Assert.That(evenCount, Is.Zero, "Odds must come before any even");
                }
                else if (p == 1)
                {
                    evenCount += 1;
                    Assert.That(oddCount, Is.EqualTo(5), "Evens must come after all odds");
                }
                else
                    Assert.Fail($"Didn't expect is_even value {p}");

                string thisName = indexRow.RowData[nameIndex].AsString();
                if (lastName != null)
                {
                    int diff = lastName.CompareTo(thisName);
                    if (lastP == p)
                        Assert.That(diff, Is.LessThan(0));
                    else
                        Assert.That(diff, Is.GreaterThan(0));
                }
                lastName = thisName;
                lastP = p;
            }

            Assert.That(oddCount, Is.EqualTo(5));
            Assert.That(evenCount, Is.EqualTo(5));
        }


        [Test]
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
            Assert.Throws<ExecutionException>(() => engine.CreateIndex(FullTableName.FromTableName("ten"), "evenNameIndex", false, columnInfos));
        }
    }
}

