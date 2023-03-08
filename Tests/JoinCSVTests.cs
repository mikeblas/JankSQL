namespace Tests
{
    using NUnit.Framework;

    using Engines = JankSQL.Engines;

    [TestFixture]
    public class JoinCSVTests : JoinTests
    {
        [SetUp]
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
