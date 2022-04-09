namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Engines = JankSQL.Engines;

    [TestClass]
    public class ExecuteCSVTests : ExecuteTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = "CSV";

            string tempPath = Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "XYZZY");
            engine = Engines.DynamicCSVEngine.OpenObliterate(tempPath);

            TestHelpers.InjectTableMyTable(engine);
            TestHelpers.InjectTableTen(engine);
        }
    }
}
