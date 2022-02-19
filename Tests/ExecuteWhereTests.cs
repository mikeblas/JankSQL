using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;


namespace Tests
{
    [TestClass]
    public class ExecuteWhereTests
    {

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereGreater()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] > 30000;");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(2, set.RowCount);
            Assert.AreEqual(4, set.ColumnCount);
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereLess()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] < 30000;");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(1, set.RowCount);
            Assert.AreEqual(4, set.ColumnCount);
        }

        public void TestSelectWhereEqual()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 30000;");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount);
            Assert.AreEqual(4, set.ColumnCount);
        }

    }
}
