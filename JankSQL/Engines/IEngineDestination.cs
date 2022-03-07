namespace JankSQL.Engines
{
    public interface IEngineDestination
    {
        void InsertRow(ExpressionOperand[] row);

        int DeleteRows(List<int> rowIndexesToDelete);
    }
}

