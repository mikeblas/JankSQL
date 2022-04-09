
namespace Tests
{
    using NUnit.Framework;

    using Engines = JankSQL.Engines;

    [TestFixture]
    public class InsertDeleteBTreeTests : InsertDeleteTests
    {
        [SetUp]
        public void ClassInitialize()
        {
            mode = "BTree";

            engine = Engines.BTreeEngine.CreateInMemory();

            TestHelpers.InjectTableMyTable(engine);
        }
    }
}
