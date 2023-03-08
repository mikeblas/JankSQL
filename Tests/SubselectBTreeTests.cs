namespace Tests
{
    using NUnit.Framework;

    using Engines = JankSQL.Engines;
    [TestFixture]
    public class SubselectBTreeTests : SubselectTests
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


        [TearDown]
        public void ClassShutdown()
        {
            if (engine != null)
                engine.Dispose();
        }

    }
}
