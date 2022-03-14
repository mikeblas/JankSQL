using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;
using Engines = JankSQL.Engines;

namespace Tests
{
    public class DDLTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;

        [TestMethod]
        public void TestCreateInsertTruncateDropTable()
        {
            // create a table
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100), AnotherOne INTEGER);");

            Assert.IsNotNull(ecCreate);
            Assert.AreEqual(0, ecCreate.TotalErrors);

            ExecuteResult resultsCreate = ecCreate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsCreate.ExecuteStatus);
            Assert.IsNull(resultsCreate.ResultSet);

            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO TransientTestTable (SomeInteger, SomeString, AnotherOne) VALUES(1, 'moe', 100), (2, 'larry', 200), (3, 'curly', 300);");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsInsert.ExecuteStatus);
            Assert.IsNotNull(resultsInsert.ResultSet);

            // truncate the table
            var ec = Parser.ParseSQLFileFromString("TRUNCATE TABLE [TransientTestTable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors);

            ExecuteResult results = ec.ExecuteSingle(engine);

            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, results.ExecuteStatus);
            Assert.IsNull(results.ResultSet);


            // drop the table
            var ecDrop = Parser.ParseSQLFileFromString("DROP TABLE TransientTestTable;");

            Assert.IsNotNull(ecDrop);
            Assert.AreEqual(0, ecDrop.TotalErrors);

            ExecuteResult resultsDrop = ecDrop.ExecuteSingle(engine);

            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsDrop.ExecuteStatus);
            Assert.IsNull(resultsDrop.ResultSet);
        }


        [TestMethod, Timeout(1000)]
        public void TestTruncateTableBadName()
        {
            var ec = Parser.ParseSQLFileFromString("TRUNCATE TABLE [BadTableName];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors);

            ExecuteResult[] results = ec.Execute(engine);
            Assert.AreEqual(1, results.Length, "result count mismatch");

            Assert.AreEqual(ExecuteStatus.FAILED, results[0].ExecuteStatus);
            Assert.IsNotNull(results[0].ErrorMessage);

            Assert.IsNull(results[0].ResultSet);
        }


        [TestMethod, Timeout(1000)]
        public void TestDropTableBadName()
        {
            var ec = Parser.ParseSQLFileFromString("DROP TABLE [BadTableName];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors);

            ExecuteResult results = ec.ExecuteSingle(engine);

            Assert.AreEqual(ExecuteStatus.FAILED, results.ExecuteStatus);
            Assert.IsNotNull(results.ErrorMessage);

            Assert.IsNull(results.ResultSet);
        }


        [TestMethod, Timeout(1000)]
        public void TestCreateDropTable()
        {
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100));");

            Assert.IsNotNull(ecCreate);
            Assert.AreEqual(0, ecCreate.TotalErrors);

            ExecuteResult resultsCreate = ecCreate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsCreate.ExecuteStatus);
            Assert.IsNull(resultsCreate.ResultSet);

            var ecDrop = Parser.ParseSQLFileFromString("DROP TABLE TransientTestTable;");

            Assert.IsNotNull(ecDrop);
            Assert.AreEqual(0, ecDrop.TotalErrors);

            ExecuteResult resultsDrop = ecDrop.ExecuteSingle(engine);

            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsDrop.ExecuteStatus);
            Assert.IsNull(resultsDrop.ResultSet);
        }
    }
}

