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

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            if (accessor == null)
                throw new ExecutionException($"Not in a row context to evaluate {this}");

            selectContext.Reset();
            ExecuteResult result = selectContext.Execute(engine, accessor, bindValues);

            if (result.ResultSet.ColumnCount != 1)
                throw new SemanticErrorException($"subselect returned {result.ResultSet.ColumnCount} columns, must only return 1 column");

            if (result.ResultSet.RowCount == 0)
                stack.Push(ExpressionOperand.NullLiteral());
            else if (result.ResultSet.RowCount == 1)
                stack.Push(result.ResultSet.Row(0)[0]);
            else
                throw new NotImplementedException($"don't know how to cope with {result.ResultSet.RowCount} rows in subselect");
        }
    }
}

