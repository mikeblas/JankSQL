using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;

namespace Tests
{
    [TestClass]
    public class JoinTests
    {
        [TestMethod, Timeout(1000)]
        public void TestCrossJoin()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] CROSS JOIN [states];");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(24, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(6, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestDoubleCrossJoin()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [three] CROSS JOIN [ten] CROSS JOIN [mytable];");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(90, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(8, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestFilterDoubleCrossJoin()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT * FROM [Three] CROSS JOIN [Ten] CROSS JOIN [MyTable]" +
                " WHERE [three].[number_id] + 10 * [ten].[number_id] > 30;");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(63, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(8, result.ResultSet.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestFilterDoubleCrossJoinBadName()
        {
            var ec = Parser.ParseSQLFileFromString(
                "SELECT * FROM [Three] CROSS JOIN [Ten] CROSS JOIN [MyTable]" +
                " WHERE [three].[BADcolumnName] + 10 * [ten].[number_id] > 30;");

            ExecuteResult result = ec.ExecuteSingle();

            Assert.IsNotNull(ec);
            Assert.IsNull(result.ResultSet);

            Assert.AreEqual(0, ec.TotalErrors);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
        }

        [TestMethod, Timeout(1000)]
        public void TestEquiJoin()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] JOIN [states] ON [mytable].[state_code] = [states].[state_code]");

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(6, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestEquiJoinBadName()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytableBADNAME] JOIN [states] ON [mytable].[state_code] = [states].[state_code]");

            ExecuteResult result = ec.ExecuteSingle();

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

            ExecuteResult result = ec.ExecuteSingle();
            Assert.IsNotNull(result.ResultSet);
            result.ResultSet.Dump();
            Assert.AreEqual(3, result.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(6, result.ResultSet.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestEquiInnerJoinBadName()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] INNER JOIN [bogusname] ON [mytable].[state_code] = [states].[state_code]");

            ExecuteResult result = ec.ExecuteSingle();

            Assert.IsNotNull(ec);
            Assert.IsNull(result.ResultSet);

            Assert.AreEqual(0, ec.TotalErrors);

            Assert.AreEqual(ExecuteStatus.FAILED, result.ExecuteStatus);
            Assert.IsNotNull(result.ErrorMessage);
        }
    }
}
