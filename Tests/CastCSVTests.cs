namespace Tests
{
    using Engines = JankSQL.Engines;

    using NUnit.Framework;

    [TestFixture]
    public class CastCSVTests : CastTests
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
