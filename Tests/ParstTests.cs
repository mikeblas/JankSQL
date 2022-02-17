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
            var listener = Parser.ParseSQLFile("SELECT * FROM [sys_tables];");

            Assert.IsNotNull(listener);
        }

        [TestMethod]
        public void TestSelectStarMyTyable()
        {
            var listener = Parser.ParseSQLFile("SELECT * FROM [mytable];");

            Assert.IsNotNull(listener);
        }

        [TestMethod]
        public void TestSelectExpressionAddition()
        {
            var listener = Parser.ParseSQLFile("SELECT 3+5 FROM [mytable];");

            Assert.IsNotNull(listener);
        }


        [TestMethod]
        public void TestSelectExpressionParenthesis()
        {
            var listener = Parser.ParseSQLFile("SELECT 2*(6+4) FROM [mytable];");

            Assert.IsNotNull(listener);
        }



        [TestMethod]
        public void TestSelectExpressionSquareRoot()
        {
            var listener = Parser.ParseSQLFile("SELECT SQRT(2) FROM [mytable];");

            Assert.IsNotNull(listener);
        }

        [TestMethod]
        public void TestSelectExpressionPower()
        {
            var listener = Parser.ParseSQLFile("SELECT POWER(5, 3) FROM [mytable];");

            Assert.IsNotNull(listener);
        }

        [TestMethod]
        public void TestSelectExpressionPowerExpressionParams()
        {
            var listener = Parser.ParseSQLFile("SELECT POWER((10/2), 15/5) FROM [mytable];");

            Assert.IsNotNull(listener);
        }

    }
}

