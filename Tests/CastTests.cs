
namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using JankSQL;
    using Engines = JankSQL.Engines;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

    public class CastTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;


        [TestMethod]
        public void TestCastStringtoDecimal()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST('33.3' AS DECIMAL)");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.AreEqual(33.3, result.ResultSet.Row(0)[0].AsDouble(), 0.000001);
            Assert.AreEqual(ExpressionOperandType.DECIMAL, result.ResultSet.Row(0)[0].NodeType);
        }

        [TestMethod]
        public void TestCastStringtoInteger()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST('33' AS INTEGER)");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.AreEqual(33, result.ResultSet.Row(0)[0].AsInteger());
            Assert.AreEqual(ExpressionOperandType.INTEGER, result.ResultSet.Row(0)[0].NodeType);
        }


        [TestMethod]
        public void TestCastDecimalToVARCHAR()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST(33.3 AS VARCHAR(30))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.AreEqual("33.3", result.ResultSet.Row(0)[0].AsString());
            Assert.AreEqual(ExpressionOperandType.VARCHAR, result.ResultSet.Row(0)[0].NodeType);
        }

        [TestMethod]
        public void TestCastDecimalToNVARCHAR()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST(33.3 AS NVARCHAR(30))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.AreEqual("33.3", result.ResultSet.Row(0)[0].AsString());
            Assert.AreEqual(ExpressionOperandType.NVARCHAR, result.ResultSet.Row(0)[0].NodeType);
        }

        [TestMethod]
        public void TestCastIntegerToNVARCHAR()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST(33 AS NVARCHAR(30))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.AreEqual("33", result.ResultSet.Row(0)[0].AsString());
            Assert.AreEqual(ExpressionOperandType.NVARCHAR, result.ResultSet.Row(0)[0].NodeType);
        }


        [TestMethod]
        public void TestCastIntegerExpressionToVARCHAR()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST(3 * 11 AS VARCHAR(30))");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 1, 1);
            result.ResultSet.Dump();

            Assert.AreEqual("33", result.ResultSet.Row(0)[0].AsString());
            Assert.AreEqual(ExpressionOperandType.VARCHAR, result.ResultSet.Row(0)[0].NodeType);
        }

        [TestMethod]
        public void TestFailCastStringtoInteger()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT CAST('33.3' AS INTEGER)");

            ExecuteResult result = ec.ExecuteSingle(engine);
            Assert.IsNull(result.ResultSet);
            Assert.IsNotNull(result.ErrorMessage);
        }
    }
}

