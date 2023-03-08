
namespace Tests
{
    using NUnit.Framework;

    using Engines = JankSQL.Engines;

    [TestFixture]
    public class ExecuteWhereBTreeDiskTests : ExecuteWhereTests
    {
        [SetUp]
        public void ClassInitialize()
        {
            mode = "BTreeDisk";
            Console.WriteLine($"Test mode is {mode}");

            string tempPath = Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "XYZZY");

            engine = Engines.BTreeEngine.OpenDiskBased(tempPath, Engines.OpenPolicy.Obliterate);
            TestHelpers.InjectTableMyTable(engine);
            TestHelpers.InjectTableTen(engine);
        }

        [TearDown]
        public void ClassShutdown()
        {
            if (engine != null)
                engine.Dispose();
        }
    }
}

