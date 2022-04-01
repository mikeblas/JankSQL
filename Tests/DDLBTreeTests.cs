﻿namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Engines = JankSQL.Engines;

    [TestClass]
    public class DDLBTreeTests : DDLTests
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
