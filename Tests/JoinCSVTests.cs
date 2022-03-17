using Microsoft.VisualStudio.TestTools.UnitTesting;

using Engines = JankSQL.Engines;

namespace Tests
{
    [TestClass]
    public class JoinCSVTests : JoinTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = "CSV";
            Console.WriteLine($"Test mode is {mode}");

            string tempPath = Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "XYZZY");
            engine = Engines.DynamicCSVEngine.OpenObliterate(tempPath);

            TestHelpers.InjectTableMyTable(engine);

            TestHelpers.InjectTableTen(engine);

            TestHelpers.InjectTableStates(engine);

            TestHelpers.InjectTableThree(engine);
        }
    }
}
