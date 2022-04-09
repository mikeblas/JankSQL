namespace JankShell
{
    using System.Text;

    using JankSQL;

    internal class ShellProgram
    {
        public static void Main()
        {
            var engine = JankSQL.Engines.BTreeEngine.CreateInMemory();

            StringBuilder command = new ();
            string? line;

            while (true)
            {
                Console.Write("janksh> ");
                line = Console.ReadLine();
                if (line == null)
                    break;
                line = line.Trim();
                if (line.Equals("go", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"Commmand is {command}");
 
                    var ec = Parser.QuietParseSQLFileFromString(command.ToString());

                    ExecuteResult results = ec.ExecuteSingle(engine);
                    if (results.ExecuteStatus != ExecuteStatus.SUCCESSFUL)
                        Console.WriteLine($"JankSh: ERROR: {results.ErrorMessage}");
                    
                    else
                    {
                        if (results.ResultSet == null)
                            Console.WriteLine("JankSh: No result set");
                        else
                        {
                            bool first = true;
                            foreach (FullColumnName str in results.ResultSet.GetColumnNames())
                            {
                                if (!first)
                                    Console.Write(",");
                                first = false;
                                Console.Write($"{str}");
                            }
                            Console.WriteLine();

                            for (int i = 0; i < results.ResultSet.RowCount; i++)
                            {
                                var row = results.ResultSet.Row(i);
                                Console.WriteLine($"{string.Join(',', row)}");
                            }
                        }
                    }

                    command.Clear();
                }
                else
                {
                    command.Append(line);
                }
            }
        }
    }
}
