namespace Tests
{
    using NUnit.Framework;

    using Engines = JankSQL.Engines;

    [TestFixture]
    public class DDLCSVTests : DDLTests
    {
        [SetUp]
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
