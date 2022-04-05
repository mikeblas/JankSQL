
namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using JankSQL;
    using Engines = JankSQL.Engines;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

    public class ExecuteWhereTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereGreater()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] > 30000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 2);
            result.ResultSet.Dump();
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereGreaterEqual()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM  ten WHERE number_id >= 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 5 );
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereLess()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] < 30000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 1);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereLessEqual()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM ten WHERE number_id <= 5;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 6);
            result.ResultSet.Dump();
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereEqualNone()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 30000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 0);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereEqualSome()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 25000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 1);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereBetween()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM ten WHERE number_id BETWEEN 3 AND 6;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 4);
            result.ResultSet.Dump();
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNotBetween()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM ten WHERE number_id NOT BETWEEN 3 AND 6;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 6);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereExpressionBetween()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM ten WHERE 10 * number_id BETWEEN 30 AND 60;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 4);
            result.ResultSet.Dump();
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereExpressionNotBetween()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM ten WHERE 10 * number_id NOT BETWEEN 30 AND 60;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 6);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereExpressionBetweenExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM ten WHERE 10 * number_id BETWEEN 3 * 10 AND 6 * 10;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 4);
            result.ResultSet.Dump();
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereExpressionNotBetweenExpressions()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM ten WHERE 10 * number_id NOT BETWEEN 3 * 10 AND 6 * 10;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 3, 6);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereEqualsMathA()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 12500 * 2;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 1);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereEqualsMathB()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] * 2 = 50000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 1);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereBangEqual()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] != 37000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 2);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereLessGreaterNotEqual()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] <> 37000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 2);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereOR()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 37000 OR [keycolumn] = 1;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 2);
            result.ResultSet.Dump();
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereAND()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 25000 AND [keycolumn] = 1;");

            ec.Dump();

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 1);
            result.ResultSet.Dump();
        }


        [TestMethod]
        public void TestSelectWhereAliasAND()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] AS Gangster WHERE gangster.population = 25000 AND [keycolumn] = 1;");

            ec.Dump();

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 1);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereComplexAND()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE [population] = 25000 AND [keycolumn] = 5-4;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 1);
            result.ResultSet.Dump();
        }

        // 
        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTParens()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT ([population] = 37000);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 2);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTCompoundParens()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT ([population] = 37000 OR [keycolumn] = 1);");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 1);
            result.ResultSet.Dump();
        }


        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTMultiParens()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT(NOT(NOT ([population] = 37000)));");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 2);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOT()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT [population] = 37000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 2);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTMulti()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT NOT NOT NOT NOT [population] = 37000;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 2);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTCompound()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT [population] = 37000 OR [keycolumn] = 1;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 2);
            result.ResultSet.Dump();
        }

        [TestMethod, Timeout(1000)]
        public void TestSelectWhereNOTCompound3Rows()
        {
            var ec = Parser.ParseSQLFileFromString("SELECT * FROM [mytable] WHERE NOT [population] = 37000 OR [keycolumn] = 2;");

            ExecuteResult result = ec.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(result, 4, 3);
            result.ResultSet.Dump();
        }

    }
}
