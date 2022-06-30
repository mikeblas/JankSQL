namespace JankSQL.Operators
{
    using System.Collections.Generic;

    using JankSQL.Contexts;
    using JankSQL.Engines;
    using JankSQL.Expressions;

    internal class Union : IComponentOutput
    {
        private IComponentOutput leftInput;
        private IComponentOutput rightInput;
        private UnionType unionType;

        public Union(UnionType unionType, IComponentOutput leftInput, IComponentOutput rightInput)
        {
            this.leftInput = leftInput;
            this.rightInput = rightInput;
            this.unionType = unionType;
        }

        public ResultSet GetRows(IEngine engine, IRowValueAccessor? outerAccessor, int max, Dictionary<string, ExpressionOperand> bindValues)
        {
            throw new NotImplementedException();
        }

        public void Rewind()
        {
            throw new NotImplementedException();
        }
    }
}


