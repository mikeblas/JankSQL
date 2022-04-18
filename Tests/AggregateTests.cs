namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;

    abstract public class AggregateTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestMinMaxGroupByOutput()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT is_even, MIN(number_name), MAX(number_name) FROM ten GROUP BY is_even");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 2);
            result.ResultSet.Dump();

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
                Assert.IsFalse(result.ResultSet.Row(0)[1].RepresentsNull);
                Assert.IsFalse(result.ResultSet.Row(0)[2].RepresentsNull);

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

        [Test]
        public void TestMinMaxGroupByOutputNoRows()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT is_even, MIN(number_name), MAX(number_name) FROM ten WHERE 1 = 0 GROUP BY is_even");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 0);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestMinMaxGroupByNoOutput()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT MIN(number_name), MAX(number_name) FROM ten GROUP BY is_even");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 2);
            result.ResultSet.Dump();

            bool matchedEven = false;
            bool matchedOdd = false;

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                string lowestName = result.ResultSet.Row(i)[0].AsString();
                string highestName = result.ResultSet.Row(i)[1].AsString();
                Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
                Assert.IsFalse(result.ResultSet.Row(0)[1].RepresentsNull);

                if (lowestName.Equals("five") && highestName.Equals("three"))
                    matchedOdd = true;
                else if (lowestName.Equals("eight") && highestName.Equals("zero"))
                    matchedEven = true;
                else
                    Assert.Fail("Bogus row found: {lowestName}, {highestName}");
            }

            Assert.IsTrue(matchedEven && matchedOdd);
        }

        [Test]
        public void TestMinMaxGroupByNoOutputNoRows()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT MIN(number_name), MAX(number_name) FROM ten WHERE 1 = 0 GROUP BY is_even");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 0);
            result.ResultSet.Dump();
        }


        [Test]
        public void TestSimpleSum()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SUM(number_id) FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 45);
        }

        [Test]
        public void TestSimpleSumNoRows()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SUM(number_id) FROM ten WHERE 1 = 0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }

        [Test]
        public void TestSimpleSumCount()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SUM(number_id), COUNT(number_id) FROM kiloLeft");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 500500);
            JankAssert.ValueMatchesInteger(result.ResultSet, 1, 0, 1000);
        }

        [Test]
        public void TestSumCountNoRows()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SUM(number_id), COUNT(number_id) FROM ten WHERE 1 = 0");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
            JankAssert.ValueMatchesInteger(result.ResultSet, 1, 0, 0);
        }


        [Test]
        public void TestMinMax()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT MIN(number_id), MAX(number_id) FROM ten ");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 0);
            JankAssert.ValueMatchesInteger(result.ResultSet, 1, 0, 9);
        }


        [Test]
        public void TestMinMaxFiltered()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT MIN(number_id), MAX(number_id) FROM ten WHERE is_even = 1");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 0);
            JankAssert.ValueMatchesInteger(result.ResultSet, 1, 0, 8);
        }

        [Test]
        public void TestMinMaxNoRows()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT MIN(number_id), MAX(number_id) FROM ten WHERE 1 = 0");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
            JankAssert.ValueIsNull(result.ResultSet, 1, 0);
        }

        [Test]
        public void TestOneExpressionSumCount()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 23 * SUM(number_id), COUNT(number_id) FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 45 * 23);
            JankAssert.ValueMatchesInteger(result.ResultSet, 1, 0, 10);
        }

        [Test]
        public void TestOneExpressionSumCountNoRows()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 23 * SUM(number_id), COUNT(number_id) FROM ten WHERE 1 = 0");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
            JankAssert.ValueMatchesInteger(result.ResultSet, 1, 0, 0);
        }

        [Test]
        public void TestTwoExpressionSumCount()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 10* SUM(number_id), COUNT(number_id) * 100 FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 450);
            JankAssert.ValueMatchesInteger(result.ResultSet, 1, 0, 1000);
        }


        [Test]
        public void TestTwoSumExpressionCountExpression()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SUM(number_id * 10), COUNT(number_id * 100) FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 450);
            JankAssert.ValueMatchesInteger(result.ResultSet, 1, 0, 10);
        }

        [Test]
        public void TestTwoSumGroupByOutputGrouped()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT is_even, SUM(number_id * 10), COUNT(number_id * 100) FROM ten GROUP BY is_even");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 2);
            result.ResultSet.Dump();

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                Assert.IsFalse(result.ResultSet.Row(0)[0].RepresentsNull);
                Assert.IsFalse(result.ResultSet.Row(0)[1].RepresentsNull);
                Assert.IsFalse(result.ResultSet.Row(0)[2].RepresentsNull);

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


        [Test]
        public void TestIntegerSimpleAverage()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT AVG(number_id) FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            // it's really 4.5, but all integers, so ...
            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 4);
        }


        [Test]
        public void TestIntegerSimpleAverageNoRows()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT AVG(number_id) FROM ten WHERE 1 = 0");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }

        [Test]
        public void TestDecimalSimpleAverage()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT AVG(population) FROM myTable;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesDecimal(result.ResultSet, 0, 0, 3_854_000, 0.0001);
        }

        [Test]
        public void TestDecimalSimpleAverageNoRows()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT AVG(population) FROM myTable WHERE 1=0;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }


        [Test]
        public void TestDecimalSimpleAverageNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT AVG(population + NULL) FROM myTable;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }


        [Test]
        public void TestDecimalSimpleSumNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SUM(population + NULL) FROM myTable;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }

        [Test]
        public void TestDecimalSimpleCountNull()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT COUNT(population + NULL) FROM myTable;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 0);
        }

        [Test]
        public void TestIntegerCastAverage()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT AVG(CAST(number_id AS DECIMAL)) FROM ten");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            // it's really 4.5, since we cast to decimal
            JankAssert.ValueMatchesDecimal(result.ResultSet, 0, 0, 4.5, 0.00001);
        }


        [Test]
        public void TestIntegerCastAverageNoRows()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT AVG(CAST(number_id AS DECIMAL)) FROM ten WHERE 1 = 0");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueIsNull(result.ResultSet, 0, 0);
        }

        [Test]
        public void TestNotCoveredGroupingSelect()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT number_name, MIN(number_name), MAX(number_name) FROM ten GROUP BY is_even");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ErrorMessage);

            // this will throw, since no result set is available
            Assert.Throws<InvalidOperationException>(() => { var x = result.ResultSet; });
        }
    }
}

