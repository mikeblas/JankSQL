namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using JankSQL.Expressions;
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
            Assert.IsNotNull(t);

            var idx = t!.Index("evenIndex");
            Assert.IsNotNull(idx);

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
            Assert.IsNotNull(t);

            var idx = t!.Index("evenIndex");
            Assert.IsNotNull(idx);
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
            Assert.IsNotNull(t);

            Tuple newRow = Tuple.CreateEmpty(3);
            newRow[0] = ExpressionOperand.IntegerFromInt(11);
            newRow[1] = ExpressionOperand.VARCHARFromString("eleven");
            newRow[2] = ExpressionOperand.IntegerFromInt(0);

            t!.InsertRow(newRow);

            var idx = t.Index("evenIndex");
            Assert.IsNotNull(idx);
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
            Assert.IsNotNull(t);

            Tuple newRow = Tuple.CreateEmpty(3);
            newRow[0] = ExpressionOperand.IntegerFromInt(11);
            newRow[1] = ExpressionOperand.VARCHARFromString("eleven");
            newRow[2] = ExpressionOperand.IntegerFromInt(0);

            t!.InsertRow(newRow);

            var idx = t.Index("evenIndex");
            Assert.IsNotNull(idx);
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
            Assert.IsNotNull(t);

            Tuple newRow = Tuple.CreateEmpty(3);
            newRow[0] = ExpressionOperand.IntegerFromInt(11);
            newRow[1] = ExpressionOperand.VARCHARFromString("eleven");
            newRow[2] = ExpressionOperand.IntegerFromInt(0);

            t!.InsertRow(newRow);

            var idx = t.Index("evenIndex");
            Assert.IsNotNull(idx);
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
            Assert.IsNotNull(t);

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
            Assert.IsNotNull(t);

            var idx = t!.Index("evenNameIndex");
            Assert.IsNotNull(idx);
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
            Assert.IsNotNull(t);

            var idx = t!.Index("evenNameIndex");
            Assert.IsNotNull(idx);
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

        [Test]
        public void TestBestIndexFirstTwo()
        {
            TestHelpers.InjectTableFiveIndex(engine);

            List<(string, bool)> filterColumns = new ();
            filterColumns.Add(("Col1", true));
            filterColumns.Add(("Col2", true));

            var fiveTable = engine.GetEngineTable(FullTableName.FromTableName("fiveindex"));
            Assert.NotNull(fiveTable);

            string? str = fiveTable!.BestIndex(filterColumns);
            Console.WriteLine($"[{string.Join(", ", filterColumns.Select(x => x.Item1))}]: index is {str}");
            Assert.AreEqual("FirstTwo", str);
        }

        [Test]
        public void TestBestIndexJustOne()
        {
            TestHelpers.InjectTableFiveIndex(engine);

            List<(string, bool)> filterColumns = new();
            filterColumns.Add(("Col1", true));
            filterColumns.Add(("Col5", true));

            var fiveTable = engine.GetEngineTable(FullTableName.FromTableName("fiveindex"));
            Assert.NotNull(fiveTable);

            string? str = fiveTable!.BestIndex(filterColumns);
            Console.WriteLine($"[{string.Join(", ", filterColumns.Select(x => x.Item1))}]: index is {str}");
            Assert.AreEqual("JustOne", str);
        }

        [Test]
        public void TestBestIndexNoMatch()
        {
            TestHelpers.InjectTableFiveIndex(engine);

            List<(string, bool)> filterColumns = new();
            filterColumns.Add(("Col3", true));
            filterColumns.Add(("Col2", true));

            var fiveTable = engine.GetEngineTable(FullTableName.FromTableName("fiveindex"));
            Assert.NotNull(fiveTable);

            string? str = fiveTable!.BestIndex(filterColumns);
            Console.WriteLine($"[{string.Join(", ", filterColumns.Select(x => x.Item1))}]: index is {str}");
            Assert.IsNull(str, "Didn't expect a match");
        }

        [Test]
        public void TestBestIndexEqualityBetter()
        {
            TestHelpers.InjectTableFiveIndex(engine);

            List<(string, bool)> filterColumns = new();
            filterColumns.Add(("Col4", true));
            filterColumns.Add(("Col5", true));
            filterColumns.Add(("Col1", false));
            filterColumns.Add(("Col2", false));

            var fiveTable = engine.GetEngineTable(FullTableName.FromTableName("fiveindex"));
            Assert.NotNull(fiveTable);

            string? str = fiveTable!.BestIndex(filterColumns);
            Console.WriteLine($"[{string.Join(", ", filterColumns.Select(x => x.Item1))}]: index is {str}");
            Assert.AreEqual("LastTwo", str);
        }

        [Test]
        public void TestIndexPredicateEqualityAccessor()
        {
            TestHelpers.InjectTableFiveIndexPopulated(engine);

            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("fiveindex"));
            Assert.IsNotNull(t);

            // JustOne has a single column; get it where it equals 3
            var comparisonOperators = new List<ExpressionComparisonOperator>()
            {
                new ExpressionComparisonOperator("=")
            };

            var predicate = new Expression
            {
                ExpressionOperand.IntegerFromInt(3),
            };

            List<Expression> predicates = new ();
            predicates.Add(predicate);

            // the accessor for that should generate 10,000 rows, all with the key of 3
            // and a payload that has a bookmark which goes back to a row in the table
            var idx = t!.PredicateIndex("JustOne", comparisonOperators, predicates);
            int threesFound = 0;
            foreach (var row in idx!)
            {
                threesFound += 1;
                Assert.AreEqual(3, row.RowData[0].AsInteger());

                var wholeRow = t.RowFromBookmark(row.Bookmark);
                Assert.AreEqual(3, wholeRow[0].AsInteger());
                // Console.WriteLine($"{row.RowData} --> {row.Bookmark} --> {wholeRow}");
            }

            // 10000 rows, right?
            Assert.AreEqual(10_000, threesFound);
        }


        [Test]
        public void TestIndexPredicateEqualityTwoAccessor()
        {
            TestHelpers.InjectTableFiveIndexPopulated(engine);

            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("fiveindex"));
            Assert.IsNotNull(t);

            // JustOne has a single column; get it where it equals 3
            var comparisonOperators = new List<ExpressionComparisonOperator>()
            {
                new ExpressionComparisonOperator("="),
                new ExpressionComparisonOperator("=")
            };

            List<Expression> predicates = new()
            {
                new Expression { ExpressionOperand.IntegerFromInt(5) },
                new Expression { ExpressionOperand.IntegerFromInt(3) },
            };

            // the accessor for that should generate 10,000 rows, all with the keys of 5 and 3 on the first two columns
            // and a payload that has a bookmark which goes back to a row in the table
            var idx = t!.PredicateIndex("firsttwo", comparisonOperators, predicates);
            int threesFound = 0;
            foreach (var row in idx!)
            {
                threesFound += 1;
                Assert.AreEqual(5, row.RowData[0].AsInteger());
                Assert.AreEqual(3, row.RowData[1].AsInteger());

                var wholeRow = t.RowFromBookmark(row.Bookmark);
                Assert.AreEqual(5, wholeRow[0].AsInteger());
                Assert.AreEqual(3, wholeRow[1].AsInteger());
                // Console.WriteLine($"{row.RowData} --> {row.Bookmark} --> {wholeRow}");
            }

            // 1000 rows, right?
            Assert.AreEqual(1_000, threesFound);
        }



        [Test]
        public void TestIndexPredicateEqualityGreaterTwoAccessor()
        {
            TestHelpers.InjectTableFiveIndexPopulated(engine);

            // get our table
            Engines.IEngineTable? t = engine.GetEngineTable(FullTableName.FromTableName("fiveindex"));
            Assert.IsNotNull(t);

            // JustOne has a single column; get it where it equals 3
            var comparisonOperators = new List<ExpressionComparisonOperator>()
            {
                new ExpressionComparisonOperator("="),
                new ExpressionComparisonOperator("<")
            };

            List<Expression> predicates = new()
            {
                new Expression { ExpressionOperand.IntegerFromInt(5) },
                new Expression { ExpressionOperand.IntegerFromInt(3) },
            };

            // the accessor for that should generate 10,000 rows, all with the keys of 5 and 3 on the first two columns
            // and a payload that has a bookmark which goes back to a row in the table
            var idx = t!.PredicateIndex("firsttwo", comparisonOperators, predicates);
            int threesFound = 0;

            foreach (var row in idx!)
            {
                threesFound += 1;
                Assert.AreEqual(5, row.RowData[0].AsInteger());
                Assert.That(row.RowData[1].AsInteger(), Is.AnyOf(1, 2));

                var wholeRow = t.RowFromBookmark(row.Bookmark);
                Assert.AreEqual(5, wholeRow[0].AsInteger());
                Assert.That(row.RowData[1].AsInteger(), Is.AnyOf(1, 2));
                // Console.WriteLine($"{row.RowData} --> {row.Bookmark} --> {wholeRow}");
            }

            // 2000 rows, right? (5,1,x,x,x) thru (5,2,x,x,x)
            Assert.AreEqual(2_000, threesFound);
        }
    }
}

