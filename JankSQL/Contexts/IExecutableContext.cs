namespace JankSQL.Contexts
{
    public interface IExecutableContext
    {
        ExecuteResult Execute(Engines.IEngine engine);

        void Dump();
    }
}
