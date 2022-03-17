using Microsoft.VisualStudio.TestTools.UnitTesting;

using Engines = JankSQL.Engines;


namespace Tests
{
    [TestClass]

    public class ExecuteBTreeTests : ExecuteTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = "BTree";
            Console.WriteLine($"Test mode is {mode}");

            engine = Engines.BTreeEngine.CreateInMemory();
            TestHelpers.InjectTableMyTable(engine);
        }

    }
}

