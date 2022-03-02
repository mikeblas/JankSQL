﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;

namespace Tests
{
    [TestClass]
    public class DDLTests
    {
        [TestMethod]
        public void TestSelectStarSysTables()
        {
            var ec = Parser.ParseSQLFileFromString("TRUNCATE TABLE [TargetTable];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors);

            ExecuteResult[] results = ec.Execute();
            Assert.AreEqual(1, results.Length, "result count mismatch");

            Assert.AreEqual(ExecuteStatus.SUCCESSFUL, results[0].ExecuteStatus);

            Assert.IsNull(results[0].ResultSet);
        }


        [TestMethod]
        public void TestSelectStarSysTablesBadName()
        {
            var ec = Parser.ParseSQLFileFromString("TRUNCATE TABLE [BadTableName];");

            Assert.IsNotNull(ec);
            Assert.AreEqual(0, ec.TotalErrors);

            ExecuteResult[] results = ec.Execute();
            Assert.AreEqual(1, results.Length, "result count mismatch");

            Assert.AreEqual(ExecuteStatus.FAILED, results[0].ExecuteStatus);
            Assert.IsNotNull(results[0].ErrorMessage);

            Assert.IsNull(results[0].ResultSet);
        }
    }
}

