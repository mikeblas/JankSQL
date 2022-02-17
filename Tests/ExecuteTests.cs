using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;


namespace Tests
{
    [TestClass]
    public class ExecuteTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            System.Environment.CurrentDirectory = @"C:\Projects\JankSQL";
        }


        [TestMethod]
        public void TestSelectExpressionPowerExpressionParams()
        {
            var ec = Parser.ParseSQLFile("SELECT POWER((10/2), 15/5) FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount);
            Assert.AreEqual(1, set.ColumnCount);
        }

        [TestMethod]
        public void TestSelectExpressionTwoExpressions()
        {
            var ec = Parser.ParseSQLFile("SELECT 3+5, 92 * 6 FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount);
            Assert.AreEqual(2, set.ColumnCount);
        }

        [TestMethod]
        public void TestSelectExpressionThreeExpressions()
        {
            var ec = Parser.ParseSQLFile("SELECT 355/113, 867-5309, (123 + 456 - 111) / 3 FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount);
            Assert.AreEqual(3, set.ColumnCount);
        }

        [TestMethod]
        public void TestSelectStar()
        {
            var ec = Parser.ParseSQLFile("SELECT * FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount);
            Assert.AreEqual(4, set.ColumnCount);
        }


        [TestMethod]
        public void TestSelectList()
        {
            var ec = Parser.ParseSQLFile("SELECT [city_name], [population] FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount);
            Assert.AreEqual(2, set.ColumnCount);
        }


        [TestMethod]
        public void TestSelectListExpressionDivide()
        {
            var ec = Parser.ParseSQLFile("SELECT [population] / [keycolumn] FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount);
            Assert.AreEqual(1, set.ColumnCount);
        }
    }
}
