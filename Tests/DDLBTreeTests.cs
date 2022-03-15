﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using Engines = JankSQL.Engines;
using System.IO;
using System;

namespace Tests
{
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