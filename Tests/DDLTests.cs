using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            ExecuteResult[] results = ec.Execute();

            Assert.IsNotNull(ec);
        }
    }
}


