using Microsoft.VisualStudio.TestTools.UnitTesting;

using Engines = JankSQL.Engines;
using System.IO;

namespace Tests
{
    [TestClass]
    public class InsertDeleteBTreeTests : InsertDeleteTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = "BTree";

            string tempPath = Path.GetTempPath();
            engine = Engines.BTreeEngine.CreateInMemory();

            TestHelpers.InjectTableMyTable(engine);
        }
    }
}
