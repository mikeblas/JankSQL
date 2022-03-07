
namespace JankSQL
{
    internal class Insert : IComponentOutput
    {
        IComponentOutput myInput;
        Engines.IEngineDestination engineDestination;
        

        internal Insert(Engines.IEngineDestination dest, IComponentOutput input)
        {
            myInput = input;
            engineDestination = dest;
        }

        public ResultSet GetRows(int max)
        {
            ResultSet rsInput = myInput.GetRows(max);

            for (int i = 0; i < rsInput.RowCount; i++)
            {
                ExpressionOperand[] row = rsInput.Row(i);
                engineDestination.InsertRow(row);
            }

            return rsInput;
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}
