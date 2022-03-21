namespace JankSQL.Operators
{
    internal class Insert : IComponentOutput
    {
        private readonly IComponentOutput myInput;
        private readonly Engines.IEngineTable engineTable;

        internal Insert(Engines.IEngineTable destTable, IComponentOutput input)
        {
            myInput = input;
            engineTable = destTable;
        }

        public ResultSet? GetRows(int max)
        {
            ResultSet? rsInput = myInput.GetRows(max);
            if (rsInput == null)
                return null;

            for (int i = 0; i < rsInput.RowCount; i++)
            {
                ExpressionOperand[] row = rsInput.Row(i);
                engineTable.InsertRow(row);
            }

            return rsInput;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}
