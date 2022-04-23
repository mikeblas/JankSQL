namespace Tests
{
    using NUnit.Framework;

    using JankSQL;
    using Engines = JankSQL.Engines;
    using System.Diagnostics;


    abstract public class TransactPersistTests
    {
        internal string mode = "base";
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        internal Engines.IEngine engine;

        abstract public void ClassInitialize();

        [Test]
        public void TestInsertRollbackVisible()
        {
            // insert a new row
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO Ten VALUES (11, 'Eleven', 0);");
            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultInsert);


            // select it all back, expecting 11
            var ecSelect1 = Parser.ParseSQLFileFromString("SELECT number_name FROM ten ORDER BY number_name ASC;");

            ExecuteResult resultSelect1 = ecSelect1.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect1, 1, 11);
            resultSelect1.ResultSet.Dump();

            // rollback
            engine.Rollback();

            // select it all back, expecting 10 now
            var ecSelect2 = Parser.ParseSQLFileFromString("SELECT number_name FROM ten ORDER BY number_name ASC;");

            ExecuteResult resultSelect2 = ecSelect2.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect2, 1, 10);
            resultSelect1.ResultSet.Dump();
        }


        [Test]
        public void TestInsertCommitVisible()
        {
            // insert a new row
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO Ten VALUES (11, 'Eleven', 0);");
            ExecuteResult resultInsert = ecInsert.ExecuteSingle(engine);
            JankAssert.SuccessfulNoResultSet(resultInsert);


            // select it all back, expecting 11
            var ecSelect1 = Parser.ParseSQLFileFromString("SELECT number_name FROM ten ORDER BY number_name ASC;");

            ExecuteResult resultSelect1 = ecSelect1.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect1, 1, 11);
            resultSelect1.ResultSet.Dump();

            // commit
            engine.Commit();

            // select it all back, still expecting 11
            var ecSelect2 = Parser.ParseSQLFileFromString("SELECT number_name FROM ten ORDER BY number_name ASC;");

            ExecuteResult resultSelect2 = ecSelect2.ExecuteSingle(engine);
            JankAssert.RowsetExistsWithShape(resultSelect2, 1, 11);
            resultSelect1.ResultSet.Dump();
        }
    }
}
