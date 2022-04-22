namespace Tests
{
    using NUnit.Framework;
    using Engines = JankSQL.Engines;

    [TestFixture]
    public class QuestionBTreeDiskTests : QuestionTests
    {
        [SetUp]
        public void ClassInitialize()
        {
            mode = "BTreeDisk";
            Console.WriteLine($"Test mode is {mode}");

            string tempPath = Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "XYZZY");

            engine = Engines.BTreeEngine.OpenDiskBased(tempPath, Engines.OpenPolicy.Obliterate);
        }

        [TearDown]
        public void ClassShutdown()
        {
            if (engine != null)
                engine.Dispose();
        }
    }
}

