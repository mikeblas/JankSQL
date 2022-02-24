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

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(24, set.RowCount, "row count mismatch");
            Assert.AreEqual(6, set.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestEquiJoin()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] JOIN [states] ON [mytable].[state_code] = [states].[state_code]");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(6, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestEquiInnerJoin()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] INNER JOIN [states] ON [mytable].[state_code] = [states].[state_code]");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(6, set.ColumnCount, "column count mismatch");
        }
    }
}
