namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;

    abstract public class CastTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        [Test]
        public void TestCastStringtoDecimal()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST('33.3' AS DECIMAL)");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.AreEqual(33.3, result.ResultSet.Row(0)[0].AsDouble(), 0.000001);
            Assert.AreEqual(ExpressionOperandType.DECIMAL, result.ResultSet.Row(0)[0].NodeType);
        }

        [Test]
        public void TestCastStringtoInteger()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST('33' AS INTEGER)");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.AreEqual(33, result.ResultSet.Row(0)[0].AsInteger());
            Assert.AreEqual(ExpressionOperandType.INTEGER, result.ResultSet.Row(0)[0].NodeType);
        }


        [Test]
        public void TestCastDecimalToVARCHAR()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST(33.3 AS VARCHAR(30))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "33.3");
        }

        [Test]
        public void TestCastDecimalToNVARCHAR()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST(33.3 AS NVARCHAR(30))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "33.3");
        }

        [Test]
        public void TestCastIntegerToNVARCHAR()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST(33 AS NVARCHAR(30))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "33");
        }


        [Test]
        public void TestCastIntegerExpressionToVARCHAR()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST(3 * 11 AS VARCHAR(30))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            JankAssert.ValueMatchesString(result.ResultSet, 0, 0, "33");
        }

        [Test]
        public void TestFailCastStringtoInteger()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST('33.3' AS INTEGER)");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNotNull(result.ErrorMessage);

            // throws exception since no ResultSet is available
            Assert.Throws<InvalidOperationException>(() => { var x = result.ResultSet; });
        }
    }
}

