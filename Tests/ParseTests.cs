using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;

namespace Tests
{
    [TestClass]
    public class ParseTests
    {
        [TestMethod]
        public void TestSelectStarSysTables()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [sys_tables];");

            Assert.IsNotNull(ec);
        }

        [TestMethod]
        public void TestSelectStarMyTyable()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");

            Assert.IsNotNull(ec);
        }

        [TestMethod]
        public void TestSelectExpressionAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 FROM [mytable];");

            Assert.IsNotNull(ec);
        }


        [TestMethod]
        public void TestSelectExpressionParenthesis()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 2*(6+4) FROM [mytable];");

            Assert.IsNotNull(ec);
        }



        [TestMethod]
        public void TestSelectExpressionSquareRoot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SQRT(2) FROM [mytable];");

            Assert.IsNotNull(ec);
        }

        [TestMethod]
        public void TestSelectExpressionPower()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(5, 3) FROM [mytable];");

            Assert.IsNotNull(ec);
        }

        [TestMethod]
        public void TestSelectExpressionPowerExpressionParams()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER((10/2), 15/5) FROM [mytable];");

            Assert.IsNotNull(ec);
        }

        [TestMethod]
        public void TestSelectExpressionPowerExpressionParamsCasedSpaced()
        {
            var ec = Parser.ParseSQLFileFromString("select power((10/2), 15/5) \n\n\n FROM mytable\n\n\n;");

            Assert.IsNotNull(ec);
        }

        [TestMethod]
        public void TestSelectList()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [city_name], [population] FROM [mytable];");

            Assert.IsNotNull(ec);
        }


        [TestMethod]
        public void TestSelectListExpressionDivide()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [city_name] / [population] FROM [mytable];");

            Assert.IsNotNull(ec);
        }

        [TestMethod]
        public void TestSelectListExpressionDivideCased()
        {
            var ec = Parser.ParseSQLFileFromString("select city_name / POPulation from MyTable;");

            Assert.IsNotNull(ec);
        }


        [TestMethod]
        public void TestSelectExpressionTwoExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5, 92 * 6 FROM [mytable];");

            Assert.IsNotNull(ec);
        }

        [TestMethod]
        public void TestSelectExpressionTwoExpressionsCased()
        {
            var ec = Parser.ParseSQLFileFromString("seLEct 3+5, 92 * 6 from mytable;");

            Assert.IsNotNull(ec);
        }

        [TestMethod]
        public void TestSelectExpressionThreeExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 355/113, 867-5309, (123 + 456 - 111) / 3 FROM [mytable];");

            Assert.IsNotNull(ec);
        }

        [TestMethod]
        public void TestSelectSyntaxError()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT FROM WHERE;");

            Assert.IsNotNull(ec);
            Assert.AreNotEqual(0, ec.TotalErrors, "expected an error");
        }

        [TestMethod]
        public void TestSelectExpressionSyntaxError()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 35 * / 4;");

            Assert.IsNotNull(ec);
            Assert.AreNotEqual(0, ec.TotalErrors, "expected an error");
        }



        [TestMethod]
        public void TestTruncateTableSyntaxError()
        {
            var ec = Parser.ParseSQLFileFromString("TRUNCATE TABLE;");

            Assert.IsNotNull(ec);
            Assert.AreNotEqual(0, ec.TotalErrors, "expected an error");
        }
    }
}
