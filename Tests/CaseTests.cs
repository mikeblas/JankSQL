namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;

    abstract public class CaseTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestSearchedCase2()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id," +
                "       CASE " +
                "            WHEN is_even = 0 THEN 'Odd'" +
                "            When is_even = 1 THEN 'Even'" +
                "       END" +
                "  FROM ten; ");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 10);
            result.ResultSet.Dump();

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                if (result.ResultSet.Row(i)[0].AsInteger() % 2 == 0)
                    Assert.AreEqual("Even", result.ResultSet.Row(i)[1].AsString());
                else
                    Assert.AreEqual("Odd", result.ResultSet.Row(i)[1].AsString());
            }
        }


        [Test]
        public void TestSearchedCase()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id, " +
                "       CASE " +
                "            WHEN number_id = 1 THEN 'ichi' " +
                "            WHEN number_id = 2 THEN 'ni' " +
                "            ELSE 'unknown' " +
                "       END " +
                " FROM ten;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 10);
            result.ResultSet.Dump();

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                int num = result.ResultSet.Row(i)[0].AsInteger();
                string second = result.ResultSet.Row(i)[1].AsString();

                if (num == 2)
                    Assert.AreEqual("ni", second);
                else if (num == 1)
                    Assert.AreEqual("ichi", second);
                else
                    Assert.AreEqual("unknown", second);
            }
        }


        [Test]
        public void TestSimpleCase()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id, " +
                "       CASE number_id" +
                "            WHEN 1 THEN 'ichi' " +
                "            WHEN 2 THEN 'ni' " +
                "            ELSE 'unknown' " +
                "       END " +
                " FROM ten;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 10);
            result.ResultSet.Dump();

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                int num = result.ResultSet.Row(i)[0].AsInteger();

                if (num == 2)
                    Assert.AreEqual("ni", result.ResultSet.Row(i)[1].AsString());
                else if (num == 1)
                    Assert.AreEqual("ichi", result.ResultSet.Row(i)[1].AsString());
                else
                    Assert.AreEqual("unknown", result.ResultSet.Row(i)[1].AsString());
            }
        }

        [Test]
        public void TestSimpleCaseExpression()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id, " +
                "       CASE number_id % 2 " +
                "            WHEN 0 THEN 'ComputedEven' " +
                "            WHEN 1 THEN 'ComputedOdd' " +
                "            ELSE 'unknown' " +
                "       END " +
                " FROM ten;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 10);
            result.ResultSet.Dump();

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                int num = result.ResultSet.Row(i)[0].AsInteger();

                if (num % 2 == 0)
                    Assert.AreEqual("ComputedEven", result.ResultSet.Row(i)[1].AsString());
                else
                    Assert.AreEqual("ComputedOdd", result.ResultSet.Row(i)[1].AsString());
            }
        }


        [Test]
        public void TestExpressionSimpleCaseExpression()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT number_id, " +
                "       25 * CASE number_id " +
                "            WHEN 0 THEN 33 " +
                "            WHEN 1 THEN 627 " +
                "            ELSE 867 - 5309 " +
                "       END " +
                " FROM ten;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 10);
            result.ResultSet.Dump();

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                int num = result.ResultSet.Row(i)[0].AsInteger();
                int secondColumn = result.ResultSet.Row(i)[1].AsInteger();

                if (num == 0)
                    Assert.AreEqual(25 * 33, secondColumn);
                else if (num == 1)
                    Assert.AreEqual(25 * 627, secondColumn);
                else
                    Assert.AreEqual(25 * (867 - 5309), secondColumn);
            }
        }
    }
}

