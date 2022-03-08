namespace JankSQL
{
    internal interface IComponentOutput
    {
        ResultSet? GetRows(int max);

        void Rewind();
    }
}
