using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;

namespace Tests
{
    [TestClass]
    class BareSelectTests
    {
        [TestMethod]
        public void TestBareAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5;");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(1, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }

        [TestMethod]
        public void TestBareAdditionWhere()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 WHERE 1=1;");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(1, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }


        [TestMethod]
        public void TestBareAdditionWhereNot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 WHERE 1=0;");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(0, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }
    }
}
