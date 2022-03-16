﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;
using Engines = JankSQL.Engines;
using System.Collections.Generic;

namespace Tests
{
    public class InsertDeleteTests
    {
        internal string mode = "base";
        internal Engines.IEngine engine;

        [TestMethod, Timeout(2000)]
        public void TestDelete()
        {
            // delete one row
            var ecDelete = Parser.ParseSQLFileFromString("DELETE FROM [mytable] where  keycolumn = 2;");

            ExecuteResult resultDelete = ecDelete.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultDelete.ExecuteStatus, resultDelete.ErrorMessage);
            Assert.IsNull(resultDelete.ResultSet, resultDelete.ErrorMessage);


            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM [mytable];");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            Assert.IsNotNull(resultSelect.ResultSet, resultSelect.ErrorMessage);
            resultSelect.ResultSet.Dump();
            Assert.AreEqual(2, resultSelect.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(4, resultSelect.ResultSet.ColumnCount, "column count mismatch");


            int keyColIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("keycolumn"));
            List<int> keys = new();
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                keys.Add(resultSelect.ResultSet.Row(i)[keyColIndex].AsInteger());
            }

            Assert.IsTrue(keys.Contains(1), "Wrong row deleted");
            Assert.IsTrue(keys.Contains(3), "Wrong row deleted");
            Assert.IsFalse(keys.Contains(2), "expected row not deleted");
        }

        [TestMethod, Timeout(2000)]
        public void TestInsertThree()
        {
            // create a table
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100), AnotherOne INTEGER);");

            Assert.IsNotNull(ecCreate);
            Assert.AreEqual(0, ecCreate.TotalErrors);

            ExecuteResult resultsCreate = ecCreate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsCreate.ExecuteStatus, resultsCreate.ErrorMessage);
            Assert.IsNull(resultsCreate.ResultSet);

            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO TransientTestTable (SomeInteger, SomeString, AnotherOne) VALUES(1, 'moe', 100), (2, 'larry', 200), (3, 'curly', 300);");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsInsert.ExecuteStatus, resultsCreate.ErrorMessage);
            Assert.IsNotNull(resultsInsert.ResultSet);

            // select them back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM TransientTestTable;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            Assert.IsNotNull(resultSelect.ResultSet, resultSelect.ErrorMessage);
            resultSelect.ResultSet.Dump();
            Assert.AreEqual(3, resultSelect.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, resultSelect.ResultSet.ColumnCount, "column count mismatch");

            int someIntegerIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("someinteger"));
            int anotherIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("anotherone"));
            List<int> someIntegers = new();
            List<int> moreIntegers = new();
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                someIntegers.Add(resultSelect.ResultSet.Row(i)[someIntegerIndex].AsInteger());
                moreIntegers.Add(resultSelect.ResultSet.Row(i)[anotherIndex].AsInteger());
            }

            Assert.IsTrue(someIntegers.Contains(1));
            Assert.IsTrue(someIntegers.Contains(2));
            Assert.IsTrue(someIntegers.Contains(3));

            Assert.IsTrue(moreIntegers.Contains(300));
            Assert.IsTrue(moreIntegers.Contains(200));
            Assert.IsTrue(moreIntegers.Contains(100));
        }


        [TestMethod, Timeout(2000)]
        public void TestInsertOne()
        {
            // create a table
            var ecCreate = Parser.ParseSQLFileFromString("CREATE TABLE TransientTestTable (SomeInteger INTEGER, SomeString VARCHAR(100), AnotherOne INTEGER);");

            Assert.IsNotNull(ecCreate);
            Assert.AreEqual(0, ecCreate.TotalErrors);

            ExecuteResult resultsCreate = ecCreate.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsCreate.ExecuteStatus, resultsCreate.ErrorMessage);
            Assert.IsNull(resultsCreate.ResultSet);

            // insert some rows
            var ecInsert = Parser.ParseSQLFileFromString("INSERT INTO TransientTestTable (SomeInteger, SomeString, AnotherOne) VALUES(1, 'moe', 100);");

            Assert.IsNotNull(ecInsert);
            Assert.AreEqual(0, ecInsert.TotalErrors);

            ExecuteResult resultsInsert = ecInsert.ExecuteSingle(engine);
            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, resultsInsert.ExecuteStatus, resultsCreate.ErrorMessage);
            Assert.IsNotNull(resultsInsert.ResultSet);

            // select them back
            var ecSelect = Parser.ParseSQLFileFromString("SELECT * FROM TransientTestTable;");

            ExecuteResult resultSelect = ecSelect.ExecuteSingle(engine);
            Assert.IsNotNull(resultSelect.ResultSet, resultSelect.ErrorMessage);
            resultSelect.ResultSet.Dump();
            Assert.AreEqual(1, resultSelect.ResultSet.RowCount, "row count mismatch");
            Assert.AreEqual(3, resultSelect.ResultSet.ColumnCount, "column count mismatch");

            int someIntegerIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("someinteger"));
            int anotherIndex = resultSelect.ResultSet.ColumnIndex(FullColumnName.FromColumnName("anotherone"));
            List<int> someIntegers = new();
            List<int> moreIntegers = new();
            for (int i = 0; i < resultSelect.ResultSet.RowCount; i++)
            {
                someIntegers.Add(resultSelect.ResultSet.Row(i)[someIntegerIndex].AsInteger());
                moreIntegers.Add(resultSelect.ResultSet.Row(i)[anotherIndex].AsInteger());
            }

            Assert.IsTrue(someIntegers.Contains(1));
            Assert.IsTrue(moreIntegers.Contains(100));
        }

    }
}