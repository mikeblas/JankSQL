﻿using Microsoft.VisualStudio.TestTools.UnitTesting;

using Engines = JankSQL.Engines;

namespace Tests
{
    [TestClass]
    public class CastCSVTests : CastTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = "CSV";

            string tempPath = Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "XYZZY");
            engine = Engines.DynamicCSVEngine.OpenObliterate(tempPath);

            TestHelpers.InjectTableMyTable(engine);
            TestHelpers.InjectTableTen(engine);
        }
    }
}