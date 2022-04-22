namespace JankSQL.Engines
{
    using System.Collections.Immutable;

    using JankSQL.Expressions;

    public class BTreeDiskEngine : BTreeEngine
    {
        private readonly string basePath;
        private readonly string sysTablesPath;
        private readonly string sysColumnsPath;

        protected BTreeDiskEngine(string basePath, string sysTablesPath, string sysColsPath)
        {
            this.basePath = basePath;
            this.sysTablesPath = sysTablesPath;
            this.sysColumnsPath = sysColsPath;
        }

        public static BTreeDiskEngine Open(string basePath, OpenPolicy policy)
        {
            BTreeDiskEngine engine;
            switch (policy)
            {
                case OpenPolicy.ExistingOnly:
                    engine = OpenExistingOnly(basePath);
                    break;

                case OpenPolicy.Always:
                    engine = OpenExistingOnly(basePath);
                    break;

                case OpenPolicy.Obliterate:
                    engine = OpenObliterate(basePath);
                    break;

                default:
                    throw new ArgumentException($"can't handle OpenPolicy {policy}");
            }

            return engine;
        }

        public static BTreeDiskEngine OpenExistingOnly(string basePath)
        {
            if (!Directory.Exists(basePath))
                throw new FileNotFoundException($"CSV database directory {basePath} not found");

            (string sysTablesPath, string sysColsPath) = GetCatalogPaths(basePath);

            if (!File.Exists(sysColsPath))
                throw new FileNotFoundException($"CSV SysColumns file {sysColsPath} not found");

            if (!File.Exists(sysTablesPath))
                throw new FileNotFoundException($"CSV SysTables file {sysTablesPath} not found");

            return new BTreeDiskEngine(basePath, sysTablesPath, sysColsPath);
        }

        public static BTreeDiskEngine OpenAlways(string basePath)
        {
            BTreeDiskEngine? engine = null;
            try
            {
                engine = OpenExistingOnly(basePath);
            }
            catch (FileNotFoundException)
            {
                // it's okay; we're meant to handle this
            }

            if (engine == null)
                engine = OpenObliterate(basePath);

            return engine;
        }

        public static BTreeDiskEngine OpenObliterate(string basePath)
        {
            RemoveDatabase(basePath);
            CreateDatabase(basePath);
            return OpenExistingOnly(basePath);
        }


        public static void RemoveDatabase(string basePath)
        {
            try
            {
                Directory.Delete(basePath, true);
            }
            catch (DirectoryNotFoundException)
            {
                // that's OK -- already gone!
            }
        }

        protected static void CreateDatabase(string basePath)
        {
            Directory.CreateDirectory(basePath);
            CreateSystemCatalog(basePath);
        }

        protected static (string sysTablesPath, string sysColsPath) GetCatalogPaths(string basePath)
        {
            string sysTablesPath = Path.Combine(basePath, "sys_tables.jank");
            string sysColsPath = Path.Combine(basePath, "sys_columns.jank");

            return (sysTablesPath, sysColsPath);
        }

        protected static void CreateSystemCatalog(string basePath)
        {

        }
    }
}
