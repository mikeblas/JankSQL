﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Engines = JankSQL.Engines;
using System.IO;

namespace Tests
{
    [TestClass]
    public class ExecuteWhereCSVTests : ExecuteWhereTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = "CSV";


            string tempPath = Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "XYZZY");
            engine = Engines.DynamicCSVEngine.OpenObliterate(tempPath);

            TestHelpers.InjectTableMyTable(engine);
        }
    }
}