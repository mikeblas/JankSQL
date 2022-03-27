namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using JankSQL;
    using Engines = JankSQL.Engines;

    public class AggregateTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;


        [TestMethod, Timeout(1000)]
        public void TestTwoSumGroupByOutput()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT is_even, MIN(number_name), MAX(number_name) FROM ten GROUP BY is_even");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, result.ResultSet.ColumnCount, "column count mismatch");

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                string lowestName = result.ResultSet.Row(i)[1].AsString();
                string highestName = result.ResultSet.Row(i)[2].AsString();
                int is_even = result.ResultSet.Row(i)[0].AsInteger();

                if (is_even == 0)
                {
                    Assert.AreEqual("five", lowestName);
                    Assert.AreEqual("three", highestName);
                }
                else if (is_even == 1)
                {
                    Assert.AreEqual("eight", lowestName);
                    Assert.AreEqual("zero", highestName);
                }
                else
                    Assert.Fail($"Bogus value for is_even: {is_even}");

            }
        }


        [TestMethod, Timeout(1000)]
        public void TestTwoSumGroupByNoOutput()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT MIN(number_name), MAX(number_name) FROM ten GROUP BY is_even");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(2, result.ResultSet.ColumnCount, "column count mismatch");

            bool matchedEven = false;
            bool matchedOdd = false;

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                string lowestName = result.ResultSet.Row(i)[0].AsString();
                string highestName = result.ResultSet.Row(i)[1].AsString();

                if (lowestName.Equals("five") && highestName.Equals("three"))
                    matchedOdd = true;
                else if (lowestName.Equals("eight") && highestName.Equals("zero"))
                    matchedEven = true;
                else
                    Assert.Fail("Bogus row found: {lowestName}, {highestName}");
            }

            Assert.IsTrue(matchedEven && matchedOdd);
        }

        [TestMethod]
        public void SimpleSum()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SUM(number_id) FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.AreEqual(45, result.ResultSet.Row(0)[0].AsInteger());
        }

        [TestMethod]
        public void SimpleSumCount()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SUM(number_id), COUNT(number_id) FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(2, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.AreEqual(45, result.ResultSet.Row(0)[0].AsInteger());
            Assert.AreEqual(10, result.ResultSet.Row(0)[1].AsInteger());
        }

        [TestMethod]
        public void OneExpressionSumCount()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 23 * SUM(number_id), COUNT(number_id) FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(2, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.AreEqual(45 * 23, result.ResultSet.Row(0)[0].AsInteger());
            Assert.AreEqual(10, result.ResultSet.Row(0)[1].AsInteger());
        }


        [TestMethod]
        public void TwoExpressionSumCount()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 10* SUM(number_id), COUNT(number_id) * 100 FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(2, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.AreEqual(450, result.ResultSet.Row(0)[0].AsInteger());
            Assert.AreEqual(1000, result.ResultSet.Row(0)[1].AsInteger());
        }


        [TestMethod]
        public void TwoSumExpressionCountExpression()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SUM(number_id * 10), COUNT(number_id * 100) FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(2, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.AreEqual(450, result.ResultSet.Row(0)[0].AsInteger());
            Assert.AreEqual(10, result.ResultSet.Row(0)[1].AsInteger());
        }



        [TestMethod, Timeout(1000)]
        public void TestTwoSumGroupByOutputGrouped()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT is_even, SUM(number_id * 10), COUNT(number_id * 100) FROM ten GROUP BY is_even");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(2, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, result.ResultSet.ColumnCount, "column count mismatch");

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                int sumColumn = result.ResultSet.Row(i)[1].AsInteger();
                int countColumn = result.ResultSet.Row(i)[2].AsInteger();
                int is_even = result.ResultSet.Row(i)[0].AsInteger();

                if (is_even == 0)
                {
                    Assert.AreEqual(5, countColumn);
                    Assert.AreEqual(250, sumColumn);
                }
                else if (is_even == 1)
                {
                    Assert.AreEqual(5, countColumn);
                    Assert.AreEqual(200, sumColumn);
                }
                else
                    Assert.Fail($"Bogus value for is_even: {is_even}");

            }
        }


        [TestMethod]
        public void IntegerSimpleAverage()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT AVG(number_id) FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            // it's really 4.5, but all integers, so ...
            Assert.AreEqual(4, result.ResultSet.Row(0)[0].AsInteger());
        }

        [TestMethod]
        public void DecimalSimpleAverage()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT AVG(population) FROM myTable;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            Assert.AreEqual(3854000, result.ResultSet.Row(0)[0].AsDouble(), 0.0001);
        }

        [Ignore]
        [TestMethod]
        public void IntegerCastAverage()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT AVG(CAST(number_id AS FLOAT)) FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();
            Assert.AreEqual(1, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(1, result.ResultSet.ColumnCount, "column count mismatch");

            // it's really 4.5, but all integers, so ...
            Assert.AreEqual(4, result.ResultSet.Row(0)[0].AsInteger());
        }

        [TestMethod]
        public void NotCoveredGroupingSelect()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_name, MIN(number_name), MAX(number_name) FROM ten GROUP BY is_even");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNull(result.ResultSet, "Expected error not caught");
            Assert.IsNotNull(result.ErrorMessage);
       }
    }
}

