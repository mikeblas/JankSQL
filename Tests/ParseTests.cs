
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

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectStarMyTable()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectExpressionAddition()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5 FROM [mytable];");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectExpressionParenthesis()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 2*(6+4) FROM [mytable];");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectExpressionSquareRoot()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT SQRT(2) FROM [mytable];");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectExpressionPower()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER(5, 3) FROM [mytable];");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectExpressionPowerExpressionParams()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT POWER((10/2), 15/5) FROM [mytable];");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectExpressionPowerExpressionParamsCasedSpaced()
        {
            var ec = Parser.ParseSQLFileFromString("select power((10/2), 15/5) \n\n\n FROM mytable\n\n\n;");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectList()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [city_name], [population] FROM [mytable];");

            JankAssert.SuccessfulParse(ec);
        }


        [Test]
        public void TestSelectListExpressionDivide()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT [city_name] / [population] FROM [mytable];");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectListExpressionDivideCased()
        {
            var ec = Parser.ParseSQLFileFromString("select city_name / POPulation from MyTable;");

            JankAssert.SuccessfulParse(ec);
        }


        [Test]
        public void TestSelectExpressionTwoExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 3+5, 92 * 6 FROM [mytable];");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectExpressionTwoExpressionsCased()
        {
            var ec = Parser.ParseSQLFileFromString("seLEct 3+5, 92 * 6 from mytable;");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectExpressionThreeExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 355/113, 867-5309, (123 + 456 - 111) / 3 FROM [mytable];");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectSyntaxError()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT FROM WHERE;");

            Assert.That(ec, Is.Not.Null);
            Assert.That(ec.TotalErrors, Is.GreaterThan(0), "expected an error");
        }

        [Test]
        public void TestSelectExpressionSyntaxError()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT 35 * / 4;");

            Assert.That(ec, Is.Not.Null);
            Assert.That(ec.TotalErrors, Is.GreaterThan(0), "expected an error");
        }

        [Test]
        public void TestSelectFromSelect()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM MyTable);");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestSelectFromSelectAlias()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM MyTable) AS SomeAlais;");

            JankAssert.SuccessfulParse(ec);
        }


        [Test]
        public void TestSelectFromSelectAliasJoinTable()
        {
            //REVIEW: why is this an error?
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM MyTable) AS SomeAlais JOIN Ten ON Ten.key = SomeAlias.Key;");

            Assert.That(ec, Is.Not.Null);
            Assert.That(ec.TotalErrors, Is.GreaterThan(0), "expected an error");
        }


        [Test]
        public void TestSelectFromSelectAliasJoinSelectAlias()
        {
            // var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM MyTable) AS SomeAlias JOIN (SELECT * FROM Ten) ON OtherAlias.number_id = SomeAlias.keycolumn;");
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM (SELECT * FROM MyTable) AS SomeAlias JOIN (SELECT * FROM Ten) AS OtherAlias ON OtherAlias.number_id = SomeAlias.keycolumn;");

            JankAssert.SuccessfulParse(ec);
        }

        [Test]
        public void TestTruncateTableSyntaxError()
        {
            var ec = Parser.ParseSQLFileFromString("TRUNCATE TABLE;");

            Assert.That(ec, Is.Not.Null);
            Assert.That(ec.TotalErrors, Is.GreaterThan(0), "expected an error");
        }
    }
}

