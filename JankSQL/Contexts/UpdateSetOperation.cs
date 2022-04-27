namespace JankSQL.Contexts
{
    using JankSQL.Expressions;

    internal enum UpdateSetOperator
    {
        ASSIGN,
        ADD_ASSIGN,
        SUB_ASSIGN,
        MUL_ASSIGN,
        DIV_ASSIGN,
        MOD_ASSIGN,
    }

    internal class UpdateSetOperation
    {
        private readonly FullColumnName fcn;
        private readonly Expression expression;
        private readonly UpdateSetOperator op;

        internal UpdateSetOperation(FullColumnName fcn, UpdateSetOperator op, Expression expression)
        {
            this.fcn = fcn;
            this.op = op;
            this.expression = expression;
        }

        public override string ToString()
        {
            return $"{fcn} {op} {expression}";
        }

        internal void Execute(Engines.IEngine engine, IRowValueAccessor outputaccessor, IRowValueAccessor inputAccessor, Dictionary<string, ExpressionOperand> bindValues)
        {
            if (op != UpdateSetOperator.ASSIGN)
                throw new NotImplementedException();

            ExpressionOperand val = expression.Evaluate(inputAccessor, engine, bindValues);
            outputaccessor.SetValue(fcn, val);
        }
    }
}
