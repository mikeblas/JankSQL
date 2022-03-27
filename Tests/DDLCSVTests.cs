namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Engines = JankSQL.Engines;

    [TestClass]
    public class DDLCSVTests : DDLTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            mode = "CSV";
            Console.WriteLine($"Test mode is {mode}");

            string tempPath = Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "XYZZY");
            engine = Engines.DynamicCSVEngine.OpenObliterate(tempPath);
        }
    }
}
