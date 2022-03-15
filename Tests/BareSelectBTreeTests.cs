using Microsoft.VisualStudio.TestTools.UnitTesting;

using Engines = JankSQL.Engines;
using System;

namespace Tests
{
    [TestClass]
    public class BareSelectBTreeTests : BareSelectTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = "BTree";
            Console.WriteLine($"Test mode is {mode}");

            engine = Engines.BTreeEngine.CreateInMemory();
        }
    }
}
