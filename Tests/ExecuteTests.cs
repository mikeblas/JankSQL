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


        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionPowerExpressionParams()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER((10/2), 15/5) FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionTwoExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5, 92 * 6 FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(2, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionThreeExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 355/113, 867-5309, (123 + 456 - 111) / 3 FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(3, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectStar()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(4, set.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectList()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [city_name], [population] FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(2, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestCompoundSelectList()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [city_name], [population]*2, [population] FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(3, set.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestCompoundSelectListQualified()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [mytable].[city_name], [mytable].[population], [population]*2 FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(3, set.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectListExpressionDivide()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [population] / [keycolumn] FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectListExpressionDivideQualifiedAliased()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [population] / [mytable].[keycolumn] AS [TheRatio] FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionAdditionAliased()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 AS [MySum] FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionParenthesis()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 2*(6+4) FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }



        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionSquareRoot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SQRT(2) FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectExpressionPower()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(5, 3) FROM [mytable];");

            ResultSet set = ec.Execute();
            set.Dump();
            Assert.AreEqual(3, set.RowCount, "row count mismatch");
            Assert.AreEqual(1, set.ColumnCount, "column count mismatch");
        }
    }
}


