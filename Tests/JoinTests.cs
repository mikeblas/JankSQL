namespace Tests
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

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 6, 24);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestCrossJoinDerived()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM [mytable] CROSS JOIN [states]);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 6, 24);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestCrossJoinOrdered()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] CROSS JOIN [states] ORDER BY state_name;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 6, 24);
            result.ResultSet.Dump();

            int nameIndex = result.ResultSet.ColumnIndex(FullColumnName.FromColumnName("state_name"));
            string previous = result.ResultSet.Row(0)[nameIndex].AsString();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                string current = result.ResultSet.Row(i)[nameIndex].AsString();
                Assert.IsTrue(previous.CompareTo(current) <= 0, $"expected {previous} <= {current}");
                previous = current;
            }
        }


        [Test]
        public void TestDoubleCrossJoin()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM [three] " +
                "CROSS JOIN [ten] " + 
                "CROSS JOIN [mytable];");

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

            ExecuteResult result = ec.ExecuteSingle(engine);
            ec.Dump();
            JankAssert.RowsetExistsWithShape(result, 9, 63);
            result.ResultSet.Dump();
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

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 9, 63);
            Assert.IsNotNull(result.ResultSet, result.ErrorMessage);
            result.ResultSet.Dump();

            int nameIndex = result.ResultSet.ColumnIndex(FullColumnName.FromColumnName("number_name"));
            string previous = result.ResultSet.Row(0)[nameIndex].AsString();

            for (int i = 1; i < result.ResultSet.RowCount; i++)
            {
                string current = result.ResultSet.Row(i)[nameIndex].AsString();
                Assert.IsTrue(previous.CompareTo(current) >= 0, $"expected {previous} <= {current}");
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

            ExecuteResult result = ec.ExecuteSingle(engine);

            Assert.IsNotNull(ec);
            Assert.IsNotNull(result.ErrorMessage);

            Assert.AreEqual(0, ec.TotalErrors);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [Test]
        public void TestFailCrossJoinSameName()
        {
            var ec = Parser.ParseSQLFileFromString(
                "    SELECT * " +
                "      FROM [THREE] " +
                "CROSS JOIN [three] ");

            ExecuteResult result = ec.ExecuteSingle(engine);

            Assert.IsNotNull(ec);

            Assert.AreEqual(0, ec.TotalErrors);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
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

            ExecuteResult result = ec.ExecuteSingle(engine);

            Assert.IsNotNull(ec);
            Assert.IsNotNull(result.ErrorMessage);

            Assert.AreEqual(0, ec.TotalErrors);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [Test]
        public void TestEquiJoin()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT * " +
                "  FROM [mytable] " +
                "  JOIN [states] ON [mytable].[state_code] = [states].[state_code]");

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

            ExecuteResult result = ec.ExecuteSingle(engine);

            Assert.IsNotNull(ec);
            Assert.IsNotNull(result.ErrorMessage);

            Assert.AreEqual(0, ec.TotalErrors);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
        }


        [Test]
        public void TestEquiInnerJoin()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] INNER JOIN [states] ON [mytable].[state_code] = [states].[state_code]");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 6, 3);
            result.ResultSet.Dump();
        }

        [Test]
        public void TestFailEquiInnerJoinBadName()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] INNER JOIN [bogusname] ON [mytable].[state_code] = [states].[state_code]");

            ExecuteResult result = ec.ExecuteSingle(engine);

            Assert.IsNotNull(ec);
            Assert.IsNotNull(result.ErrorMessage);

            Assert.AreEqual(0, ec.TotalErrors);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [Test]
        public void TestDerivedJoinDerivedOn()
        {
            var ec = Parser.ParseSQLFileFromString(
              "SELECT * " +
              "  FROM (SELECT * FROM MyTable) AS SomeAlias " +
              "  JOIN (SELECT * FROM Ten) AS OtherAlias " +
              "    ON OtherAlias.number_id = SomeAlias.keycolumn;");

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

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 10);
            result.ResultSet.Dump();

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                int left = result.ResultSet.Row(i)[0].AsInteger();
                if (left >= 1 && left <= 3)
                {
                    Assert.IsFalse(result.ResultSet.Row(i)[1].RepresentsNull);
                    int right = result.ResultSet.Row(i)[0].AsInteger();
                    Assert.AreEqual(left, right);
                }
                else if (left == 0 || (left > 3 && left <= 9))
                    Assert.IsTrue(result.ResultSet.Row(i)[1].RepresentsNull);
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

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 2, 3);
            result.ResultSet.Dump();

            for (int i = 0; i < result.ResultSet.RowCount; i++)
            {
                int left = result.ResultSet.Row(i)[0].AsInteger();
                if (left >= 1 && left <= 3)
                {
                    Assert.IsFalse(result.ResultSet.Row(i)[1].RepresentsNull);
                    int right = result.ResultSet.Row(i)[0].AsInteger();
                    Assert.AreEqual(left, right);
                }
                else
                    Assert.Fail($"Unexpected left column value {left}");
            }
        }
    }
}
