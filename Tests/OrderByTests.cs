namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;
    using System.Diagnostics;

    abstract public class OrderByTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestOrderByOneStringDefault()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_name FROM ten ORDER BY number_name;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 10);
            result.ResultSet.Dump();

            string previous = result.ResultSet.Row(0)[0].AsString();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                string current = result.ResultSet.Row(i)[0].AsString();
                Assert.IsTrue(previous.CompareTo(current) <= 0, $"expected {previous} <= {current}");
                previous = current;
            }
        }

        [Test]
        public void TestOrderByOneStringAsc()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_name FROM ten ORDER BY number_name ASC;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 10);
            result.ResultSet.Dump();

            string previous = result.ResultSet.Row(0)[0].AsString();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                string current = result.ResultSet.Row(i)[0].AsString();
                Assert.IsTrue(previous.CompareTo(current) <= 0, $"expected {previous} <= {current}");
                previous = current;
            }
        }

        [Test]
        public void TestOrderByOneStringDesc()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_name FROM ten ORDER BY number_name desc;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 10);
            result.ResultSet.Dump();

            string previous = result.ResultSet.Row(0)[0].AsString();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                string current = result.ResultSet.Row(i)[0].AsString();
                Assert.IsTrue(previous.CompareTo(current) >= 0, $"expected {previous} >= {current}");
                previous = current;
            }
        }


        [Test]
        public void TestOrderByOneIntegerDefault()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_id FROM ten ORDER BY number_id;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 10);
            result.ResultSet.Dump();

            int previous = result.ResultSet.Row(0)[0].AsInteger();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                int current = result.ResultSet.Row(i)[0].AsInteger();
                Assert.IsTrue(previous.CompareTo(current) <= 0, $"expected {previous} <= {current}");
                previous = current;
            }
        }


        [Test]
        public void TestOrderByOneIntegerAsc()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_id FROM ten ORDER BY number_id ASC;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 10);
            result.ResultSet.Dump();

            int previous = result.ResultSet.Row(0)[0].AsInteger();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                int current = result.ResultSet.Row(i)[0].AsInteger();
                Assert.IsTrue(previous.CompareTo(current) <= 0, $"expected {previous} <= {current}");
                previous = current;
            }
        }

        [Test]
        public void TestOrderByOneIntegerFilterAsc()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_id FROM ten WHERE is_even = 1 ORDER BY number_id ASC;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 5);
            result.ResultSet.Dump();

            int previous = result.ResultSet.Row(0)[0].AsInteger();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                int current = result.ResultSet.Row(i)[0].AsInteger();
                Assert.IsTrue(previous.CompareTo(current) <= 0, $"expected {previous} <= {current}");
                previous = current;
            }
        }

        [Test]
        public void TestOrderByOneIntegerDesc()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_id FROM ten ORDER BY number_id DESC;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 10);
            result.ResultSet.Dump();

            int previous = result.ResultSet.Row(0)[0].AsInteger();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                int current = result.ResultSet.Row(i)[0].AsInteger();
                Assert.IsTrue(previous.CompareTo(current) >= 0, $"expected {previous} >= {current}");
                previous = current;
            }
        }


        [Test]
        public void TestOrderByOneIntegerFilterDesc()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_id FROM ten WHERE is_even = 0 ORDER BY number_id DESC;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 5);
            result.ResultSet.Dump();

            int previous = result.ResultSet.Row(0)[0].AsInteger();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                int current = result.ResultSet.Row(i)[0].AsInteger();
                Assert.IsTrue(previous.CompareTo(current) >= 0, $"expected {previous} >= {current}");
                previous = current;
            }
        }


        [Test]
        public void TestOrderByOneIntegerFilteredAllDesc()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_id FROM ten WHERE is_even = 35 ORDER BY number_id DESC;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 0);
            result.ResultSet.Dump();
        }


        [Test]
        public void TestOrderByOneIntegerFilteredAll()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_id FROM ten WHERE is_even = 35 ORDER BY number_id;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 0);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestOrderByManyNIntegers()
        {
            Random random = new ();
            int testRowCount = 10000;

            // create a table
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeKey INTEGER, SomeInteger INTEGER);");

            Assert.IsNotNull(ecCreate);
            Assert.AreEqual(0, ecCreate.TotalErrors);

            ExecuteResult resultsCreate = ecCreate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL_WITH_MESSAGE, resultsCreate.ExecuteStatus, resultsCreate.ErrorMessage);
            Assert.NotNull(resultsCreate.ErrorMessage);

            Stopwatch parsing = new ();
            Stopwatch execution = new ();

            // insert some rows
            int checksum = 0;
            for (int i = 1; i <= testRowCount; i++)
            {
                int r = random.Next();
                checksum += r;
                parsing.Start();
                string statement = $"INSERT INTO TransientTestTable (SomeKey, SomeInteger) VALUES({i}, {r});";
                var ecInsert = Parser.QuietParseSQLFileFromString(statement);
                parsing.Stop();

                Assert.IsNotNull(ecInsert);
                Assert.AreEqual(0, ecInsert.TotalErrors);

                execution.Start();
                ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
                execution.Stop();

                JankAssert.SuccessfulNoResultSet(resultsInsert);
            }

            Console.WriteLine($"parsing:   {parsing.ElapsedMilliseconds}");
            Console.WriteLine($"execution: {execution.ElapsedMilliseconds}");

            // select it out
            var ecSelect = Parser.ParseSQLFileFromString("SELECT SomeKey, SomeInteger FROM TransientTestTable ORDER BY SomeKey;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect, 2, testRowCount);
            // resultsSelect.ResultSet.Dump();

            int testsum = resultSelect.ResultSet.Row(0)[1].AsInteger();
            int previous = resultSelect.ResultSet.Row(0)[0].AsInteger();
            for (int i = 1; i < resultSelect.ResultSet.RowCount; i++)
            {
                int current = resultSelect.ResultSet.Row(i)[0].AsInteger();
                Assert.IsTrue(previous.CompareTo(current) <= 0, $"expected {previous} <= {current}");
                previous = current;

                int x = resultSelect.ResultSet.Row(i)[1].AsInteger();
                testsum += x;
            }

            Assert.AreEqual(checksum, testsum);
        }
    }
}
