namespace JankSQL.Engines
{
    public interface IEngineDestination
    {
        void InsertRow(ExpressionOperand[] row);

    }
}

