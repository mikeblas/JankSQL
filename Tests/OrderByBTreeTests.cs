namespace Tests
{
    using NUnit.Framework;

    using Engines = JankSQL.Engines;

    [TestFixture]

    public class OrderByBTreeTests : OrderByTests
    {
        [SetUp]
        public override void ClassInitialize()
        {
            mode = "BTree";
            Console.WriteLine($"Test mode is {mode}");

            engine = Engines.BTreeEngine.CreateInMemory();
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

