using Microsoft.VisualStudio.TestTools.UnitTesting;

using JankSQL;

namespace Tests
{
    [TestClass]
    public class ExecuteCSVTests : ExecuteTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = 1;
        }

    }
}
