using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;

namespace Tests
{
    [TestClass]
    public class ExecuteBTreeTests : ExecuteTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = 2;
        }

    }
}

