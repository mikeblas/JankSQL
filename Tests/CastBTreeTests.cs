namespace Tests
{
    using Engines = JankSQL.Engines;

    using NUnit.Framework;

    [TestFixture]
    public class CastBTreeTests : CastTests
    {
        [SetUp]
        public void ClassInitialize()
        {
            mode = "BTree";
            Console.WriteLine($"Test mode is {mode}");

            engine = Engines.BTreeEngine.CreateInMemory();
            TestHelpers.InjectTableMyTable(engine);
            TestHelpers.InjectTableTen(engine);
        }

    }
}

