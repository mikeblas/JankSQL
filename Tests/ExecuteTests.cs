using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;


namespace Tests
{
    [TestClass]
    public class ExecuteTests
    {
        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            System.Environment.CurrentDirectory = "C:\\Projects\\JankSQL";
        }


        [TestMethod]
        public void TestSelectExpressionPowerExpressionParams()
        {
            var listener = Parser.ParseSQLFile("SELECT POWER((10/2), 15/5) FROM [mytable];");

            listener.Execute();
        }
    }
}
