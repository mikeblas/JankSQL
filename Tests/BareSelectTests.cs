using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;

namespace Tests
{
    [TestClass]
    public class BareSelectTests
    {
        [TestMethod, Timeout(1000)]
        public void TestAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5;");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(1, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestNegativeNumber()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -32;");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(1, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestNegativeNumberMultiply()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT -32 * -133;");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(1, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestAdditionWhere()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 WHERE 1=1;");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(1, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestAdditionWhereNot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 WHERE 1=0;");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(0, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestThreeStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT N'hello', 'goodbye', 'Bob''s Burgers';");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(1, set.RowCount, "row count mismatch");
            Assert.AreEqual(3, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000), Ignore]
        public void TestConcatenateTwoStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Hello' + ', world';");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(1, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000), Ignore]
        public void TestConcatenateThreeStrings()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 'Hello' + ', world' + ', good day!';");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(1, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }


    }
}
