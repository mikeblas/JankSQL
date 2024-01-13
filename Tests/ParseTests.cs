
namespace Tests
{
    using NUnit.Framework;

    using JankSQL;

    [TestFixture]
    public class ParseTests
    {
        [Test]
        public void TestSelectStarSysTables()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [sys_tables];");

            Assert.IsNotNull(ec);
        }

        [Test]
        public void TestSelectStarMyTable()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectExpressionAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 FROM [mytable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectExpressionParenthesis()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 2*(6+4) FROM [mytable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectExpressionSquareRoot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SQRT(2) FROM [mytable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectExpressionPower()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(5, 3) FROM [mytable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectExpressionPowerExpressionParams()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER((10/2), 15/5) FROM [mytable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectExpressionPowerExpressionParamsCasedSpaced()
        {
            var ec = Parser.ParseSQLFileFromString("select power((10/2), 15/5) \n\n\n FROM mytable\n\n\n;");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectList()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [city_name], [population] FROM [mytable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }


        [Test]
        public void TestSelectListExpressionDivide()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [city_name] / [population] FROM [mytable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectListExpressionDivideCased()
        {
            var ec = Parser.ParseSQLFileFromString("select city_name / POPulation from MyTable;");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }


        [Test]
        public void TestSelectExpressionTwoExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5, 92 * 6 FROM [mytable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectExpressionTwoExpressionsCased()
        {
            var ec = Parser.ParseSQLFileFromString("seLEct 3+5, 92 * 6 from mytable;");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectExpressionThreeExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 355/113, 867-5309, (123 + 456 - 111) / 3 FROM [mytable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectSyntaxError()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT FROM WHERE;");

            Assert.IsNotNull(ec);
            Assert.AreNotEqual(0, ec.TotalErrors, "expected an error");
        }

        [Test]
        public void TestSelectExpressionSyntaxError()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 35 * / 4;");

            Assert.IsNotNull(ec);
            Assert.AreNotEqual(0, ec.TotalErrors, "expected an error");
        }

        [Test]
        public void TestSelectFromSelect()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM MyTable);");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestSelectFromSelectAlias()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM MyTable) AS SomeAlais;");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }


        [Test]
        public void TestSelectFromSelectAliasJoinTable()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM MyTable) AS SomeAlais JOIN Ten ON Ten.key = SomeAlias.Key;");

            Assert.IsNotNull(ec);
            Assert.AreNotEqual(0, ec.TotalErrors, "expected an error");
        }


        [Test]
        public void TestSelectFromSelectAliasJoinSelectAlias()
        {
            // var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM MyTable) AS SomeAlias");
            // var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM MyTable) AS SomeAlias JOIN (SELECT * FROM Ten) ON OtherAlias.number_id = SomeAlias.keycolumn;");
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM MyTable) AS SomeAlias JOIN (SELECT * FROM Ten) AS OtherAlias ON OtherAlias.number_id = SomeAlias.keycolumn;");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors, "expected no errors");
        }

        [Test]
        public void TestTruncateTableSyntaxError()
        {
            var ec = Parser.ParseSQLFileFromString("TRUNCATE TABLE;");

            Assert.IsNotNull(ec);
            Assert.AreNotEqual(0, ec.TotalErrors, "expected an error");
        }
    }
}

