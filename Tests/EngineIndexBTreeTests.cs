
namespace Tests
{
    using NUnit.Framework;
    using Engines = JankSQL.Engines;

    [TestFixture]
    public class EngineIndexBTreeTests : EngineIndexTests
    {
        [SetUp]
        public void ClassInitialize()
        {
            mode = "BTree";
            Console.WriteLine($"Test mode is {mode}");

            engine = Engines.BTreeEngine.CreateInMemory();
            TestHelpers.InjectTableMyTable(engine);
            TestHelpers.InjectTableTen(engine);
            TestHelpers.InjectTableStates(engine);
            TestHelpers.InjectTableThree(engine);
        }
    }
}
