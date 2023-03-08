namespace Tests
{
    using NUnit.Framework;

    using Engines = JankSQL.Engines;

    [TestFixture]
    public class ExecuteWhereCSVTests : ExecuteWhereTests
    {
        [SetUp]
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
