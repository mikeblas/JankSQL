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
                    JankAssert.ValueMatchesString(result.ResultSet, 1, i, "Even");
                else
                    JankAssert.ValueMatchesString(result.ResultSet, 1, i, "Odd");
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
                    Assert.That(second, Is.EqualTo("ni"));
                else if (num == 1)
                    Assert.That(second, Is.EqualTo("ichi"));
                else
                    Assert.That(second, Is.EqualTo("unknown"));
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
                    JankAssert.ValueMatchesString(result.ResultSet, 1, i, "ni");
                else if (num == 1)
                    JankAssert.ValueMatchesString(result.ResultSet, 1, i, "ichi");
                else
                    JankAssert.ValueMatchesString(result.ResultSet, 1, i, "unknown");
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
                    JankAssert.ValueMatchesString(result.ResultSet, 1, i, "ComputedEven");
                else
                    JankAssert.ValueMatchesString(result.ResultSet, 1, i, "ComputedOdd");
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
                    Assert.That(secondColumn, Is.EqualTo(25 * 33));
                else if (num == 1)
                    Assert.That(secondColumn, Is.EqualTo(25 * 627));
                else
                    Assert.That(secondColumn, Is.EqualTo(25 * (867 - 5309)));
            }
        }
    }
}

