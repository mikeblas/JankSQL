namespace JankSQL.Operators
{
    internal interface IComponentOutput
    {
        ResultSet? GetRows(int max);

        void Rewind();
    }
}
