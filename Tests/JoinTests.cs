namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using JankSQL;
    using Engines = JankSQL.Engines;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

    public class JoinTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;

        [TestMethod, Timeout(1000)]
        public void TestCrossJoin()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] CROSS JOIN [states];");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 6, 24);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
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


        [TestMethod, Timeout(1000)]
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

        [TestMethod, Timeout(1000)]
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


        [TestMethod, Timeout(1000)]
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


        [TestMethod, Timeout(1000)]
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
            Assert.IsNull(result.ResultSet);

            Assert.AreEqual(0, ec.TotalErrors);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod, Timeout(1000)]
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

        [TestMethod, Timeout(1000)]
        public void TestFailEquiJoinBadName()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT * " +
                "  FROM [mytableBADNAME] " +
                "  JOIN [states] ON [mytable].[state_code] = [states].[state_code]");

            ExecuteResult result = ec.ExecuteSingle(engine);

            Assert.IsNotNull(ec);
            Assert.IsNull(result.ResultSet);

            Assert.AreEqual(0, ec.TotalErrors);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
        }


        [TestMethod, Timeout(1000)]
        public void TestEquiInnerJoin()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] INNER JOIN [states] ON [mytable].[state_code] = [states].[state_code]");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 6, 3);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestFailEquiInnerJoinBadName()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] INNER JOIN [bogusname] ON [mytable].[state_code] = [states].[state_code]");

            ExecuteResult result = ec.ExecuteSingle(engine);

            Assert.IsNotNull(ec);
            Assert.IsNull(result.ResultSet);

            Assert.AreEqual(0, ec.TotalErrors);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
        }
    }
}
