﻿namespace Tests
{
    using NUnit.Framework;
    using Engines = JankSQL.Engines;

    [TestFixture]
    public class ExecuteBTreeTests : ExecuteTests
    {
        [SetUp]
        public void ClassInitialize()
        {
            mode = "BTree";
            Console.WriteLine($"Test mode is {mode}");

            engine = Engines.BTreeEngine.CreateInMemory();
            TestHelpers.InjectTableMyTable(engine);
            TestHelpers.InjectTableTen(engine);
            TestHelpers.InjectTableEvents(engine);
        }

        [TearDown]
        public void ClassShutdown()
        {
            if (engine != null)
                engine.Dispose();
        }
    }
}

