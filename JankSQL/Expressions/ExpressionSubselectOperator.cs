namespace JankSQL.Expressions
{
    using JankSQL.Contexts;

    internal class ExpressionSubselectOperator : ExpressionNode
    {
        private readonly SelectContext selectContext;

        internal ExpressionSubselectOperator(SelectContext selectContext)
        {
            this.selectContext = selectContext;
        }

        public override string ToString()
        {
            return "SUBSELECT";
        }

        internal ExpressionOperand Evaluate(Engines.IEngine engine, IRowValueAccessor accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            selectContext.Reset();
            ExecuteResult result = selectContext.Execute(engine, accessor, bindValues);

            if (result.ResultSet.ColumnCount != 1)
                throw new SemanticErrorException($"subselect returned {result.ResultSet.ColumnCount} columns, must only return 1 column");

            if (result.ResultSet.RowCount == 0)
                return ExpressionOperand.NullLiteral();

            if (result.ResultSet.RowCount == 1)
                return result.ResultSet.Row(0)[0];

            throw new NotImplementedException($"don't know how to cope with {result.ResultSet.RowCount} rows in subselect");
        }
    }
}

