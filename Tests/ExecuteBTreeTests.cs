using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;

namespace Tests
{
    [TestClass]
    [Ignore]
    public class ExecuteBTreeTests : ExecuteTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = "BTree";
        }

    }
}

