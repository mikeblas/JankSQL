using Microsoft.VisualStudio.TestTools.UnitTesting;

using Engines = JankSQL.Engines;

namespace Tests
{
    [TestClass]
    public class InsertDeleteBTreeTests : InsertDeleteTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = "BTree";

            engine = Engines.BTreeEngine.CreateInMemory();

            TestHelpers.InjectTableMyTable(engine);
        }
    }
}
