﻿namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;

    abstract public class JoinTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestCrossJoin()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] CROSS JOIN [states];");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 6, 24);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestCrossJoinDerived()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM [mytable] CROSS JOIN [states]);");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 6, 24);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestCrossJoinOrdered()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] CROSS JOIN [states] ORDER BY state_name;");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 6, 24);
            result.ResultSet.Dump();

            int nameIndex = result.ResultSet.ColumnIndex(FullColumnName.FromColumnName("state_name"));
            string previous = result.ResultSet.Row(0)[nameIndex].AsString();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                string current = result.ResultSet.Row(i)[nameIndex].AsString();
                Assert.That(previous, Is.LessThanOrEqualTo(current), $"expected {previous} <= {current}");
                previous = current;
            }
        }

        [Test]
        public void TestDoubleJoin()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT * " +
                "  FROM three " +
                "  JOIN ten on three.number_id = ten.number_id " +
                "  JOIN mytable on mytable.keycolumn = three.number_id");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 9, 3);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestDoubleCrossJoin()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM [three] " +
                "CROSS JOIN [ten] " +
                "CROSS JOIN [mytable];");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 9, 90);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestFilterDoubleCrossJoin()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM [three] " +
                "CROSS JOIN [ten] " +
                "CROSS JOIN [mytable] " +
                "     WHERE [three].[number_id] + 10 * [ten].[number_id] > 30;");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 9, 63);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestFilterDoubleDerivedCrossJoin()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM [three] " +
                "CROSS JOIN " +
                "     (    SELECT * FROM [ten] " +
                "      CROSS JOIN [mytable]) AS X " +
                "     WHERE [three].[number_id] + 10 * [x].[number_id] > 30;");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            ec.Dump();
            JankAssert.RowsetExistsWithShape(result, 9, 63);
            result.ResultSet.Dump();
        }


        [Test]
        public void TestFilterDoubleDerivedCrossJoinWithBinds()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM [three] " +
                "CROSS JOIN " +
                "     (    SELECT * FROM [ten] " +
                "      CROSS JOIN [mytable]) AS X " +
                "     WHERE [three].[number_id] + 10 * [x].[number_id] > @LowLimit;");
            JankAssert.SuccessfulParse(ec);

            ec.SetBindValue("@LowLimit", ExpressionOperand.IntegerFromInt(30));
            ExecuteResult result = ec.ExecuteSingle(engine);
            ec.Dump();
            JankAssert.RowsetExistsWithShape(result, 9, 63);
            result.ResultSet.Dump();

            ec.SetBindValue("@LowLimit", ExpressionOperand.IntegerFromInt(20));
            ExecuteResult result2 = ec.ExecuteSingle(engine);
            ec.Dump();
            JankAssert.RowsetExistsWithShape(result2, 9, 72);
            result2.ResultSet.Dump();
        }


        [Test]
        public void TestFilterDoubleCrossJoinOrderBy()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM [Three] " +
                "CROSS JOIN [Ten] " +
                "CROSS JOIN [MyTable] " +
                "     WHERE [three].[number_id] + 10 * [ten].[number_id] > 30 " +
                "  ORDER BY number_name DESC");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 9, 63);
            Assert.That(result.ResultSet, Is.Not.Null, result.ErrorMessage);
            result.ResultSet.Dump();

            int nameIndex = result.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_name"));
            string previous = result.ResultSet.Row(0)[nameIndex].AsString();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                string current = result.ResultSet.Row(i)[nameIndex].AsString();
                Assert.That(previous, Is.GreaterThanOrEqualTo(current), $"expected {previous} <= {current}");
                previous = current;
            }
        }

        [Test]
        public void TestFailDoubleCrossJoinSameName()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM [three] " +
                "CROSS JOIN " +
                "     (    SELECT * FROM [ten] " +
                "      CROSS JOIN [mytable]) AS three ");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.FailureWithMessage(result);
        }

        [Test]
        public void TestFailCrossJoinSameName()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM [THREE] " +
                "CROSS JOIN [three] ");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);

            JankAssert.FailureWithMessage(result);
        }

        [Test]
        public void TestFilterDoubleCrossJoinBadName()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM [Three] " +
                "CROSS JOIN [Ten] " +
                "CROSS JOIN [MyTable] " +
                "     WHERE [three].[BADcolumnName] + 10 * [ten].[number_id] > 30;");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.FailureWithMessage(result);
        }

        [Test]
        public void TestEquiJoin()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT * " +
                "  FROM [mytable] " +
                "  JOIN [states] ON [mytable].[state_code] = [states].[state_code]");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 6, 3);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestFailEquiJoinBadName()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT * " +
                "  FROM [mytableBADNAME] " +
                "  JOIN [states] ON [mytable].[state_code] = [states].[state_code]");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);

            JankAssert.FailureWithMessage(result);
        }


        [Test]
        public void TestEquiInnerJoin()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] INNER JOIN [states] ON [mytable].[state_code] = [states].[state_code]");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 6, 3);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestFailEquiInnerJoinBadName()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] INNER JOIN [bogusname] ON [mytable].[state_code] = [states].[state_code]");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);

            JankAssert.FailureWithMessage(result);
        }

        [Test]
        public void TestDerivedJoinDerivedOn()
        {
            var ec = Parser.ParseSQLFileFromString(
              "SELECT * " +
              "  FROM (SELECT * FROM MyTable) AS SomeAlias " +
              "  JOIN (SELECT * FROM Ten) AS OtherAlias " +
              "    ON OtherAlias.number_id = SomeAlias.keycolumn;");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 7, 3);
            result.ResultSet.Dump();
        }


        [Test]
        public void TestDerivedJoinDerivedWhereOn()
        {
            var ec = Parser.ParseSQLFileFromString(
              "SELECT * " +
              "  FROM (SELECT * FROM MyTable) AS SomeAlias " +
              "  JOIN (SELECT * FROM Ten WHERE number_id >= 3) AS OtherAlias " +
              "    ON OtherAlias.number_id = SomeAlias.keycolumn;");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 7, 1);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestDerivedCrossJoinTable()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM (SELECT * FROM myTable) " +
                "CROSS JOIN ten");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 7, 30);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestTableCrossJoinDerived()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM ten " +
                "CROSS JOIN (SELECT * FROM myTable)");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 7, 30);
            result.ResultSet.Dump();
        }


        [Test]
        public void TestLeftOuterJoin()
        {
            var ec = Parser.ParseSQLFileFromString(
                  "SELECT number_id, keycolumn " +
                  "  FROM ten " +
                  " LEFT OUTER JOIN mytable on numbeR_id = keycolumn;");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 10);
            result.ResultSet.Dump();

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                int left = result.ResultSet.Row(i)[0].AsInteger();
                if (left >= 1 && left <= 3)
                {
                    Assert.That(result.ResultSet.Row(i)[1].RepresentsNull, Is.False);
                    int right = result.ResultSet.Row(i)[0].AsInteger();
                    Assert.That(left, Is.EqualTo(right));
                }
                else if (left == 0 || (left > 3 && left <= 9))
                    Assert.That(result.ResultSet.Row(i)[1].RepresentsNull, Is.True);
                else
                    Assert.Fail($"Unexpected left column value {left}");
            }
        }


        [Test]
        public void TestRightOuterJoin()
        {
            var ec = Parser.ParseSQLFileFromString(
                  "SELECT number_id, keycolumn " +
                  "  FROM ten " +
                  " RIGHT OUTER JOIN mytable on numbeR_id = keycolumn;");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 3);
            result.ResultSet.Dump();

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                int left = result.ResultSet.Row(i)[0].AsInteger();
                if (left >= 1 && left <= 3)
                {
                    Assert.That(result.ResultSet.Row(i)[1].RepresentsNull, Is.False);
                    int right = result.ResultSet.Row(i)[0].AsInteger();
                    Assert.That(left, Is.EqualTo(right));
                }
                else
                    Assert.Fail($"Unexpected left column value {left}");
            }
        }

        [Test]
        public void TestKiloJoinOn()
        {
            TestHelpers.InjectTableKiloLeft(engine);
            TestHelpers.InjectTableKiloRight(engine);

            var ec = Parser.ParseSQLFileFromString("SELECT COUNT(1) FROM KiloLeft JOIN KiloRight ON KiloLeft.Number_ID = KiloRight.Number_ID;");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesInteger(result.ResultSet, 0, 0, 1000);
        }

        [Test]
        public void TestSelectJoinAsteriskLeft()
        {
            TestHelpers.InjectTableKiloLeft(engine);
            TestHelpers.InjectTableKiloRight(engine);

            var ec = Parser.ParseSQLFileFromString("SELECT Y.* FROM Ten X CROSS JOIN MyTable Y;");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 30);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestSelectJoinAsteriskRight()
        {
            TestHelpers.InjectTableKiloLeft(engine);
            TestHelpers.InjectTableKiloRight(engine);

            var ec = Parser.ParseSQLFileFromString("SELECT X.* FROM Ten X CROSS JOIN MyTable Y;");
            JankAssert.SuccessfulParse(ec);

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 30);
            result.ResultSet.Dump();
        }
    }
}
