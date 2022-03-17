using Microsoft.VisualStudio.TestTools.UnitTesting;

using Engines = JankSQL.Engines;

namespace Tests
{
    [TestClass]
    public class JoinCBTreeTests : JoinTests
    {
        [TestInitialize]
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
