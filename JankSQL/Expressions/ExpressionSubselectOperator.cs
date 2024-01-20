namespace JankSQL.Expressions
{
    using JankSQL.Contexts;
    using JankSQL.Engines;
    using System.Collections.Generic;

    internal class ExpressionSubselectOperator : ExpressionNode
    {
        private readonly SelectContext selectContext;
        private List<FullColumnName>? outerBindableColumns;

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
            if (outerBindableColumns == null)
                throw new InternalErrorException("ExpressionSubselectOperator was not bound before evaluation");
            if (accessor == null)
                throw new ExecutionException($"Not in a row context to evaluate {this}");

            selectContext.Reset();
            BindResult bindResult = selectContext.Bind(engine, outerBindableColumns, bindValues);
            if (!bindResult.IsSuccessful)
                throw new InternalErrorException($"Could not rebind subselect in IN clause: {bindResult.ErrorMessage}");
            ExecuteResult result = selectContext.Execute(engine, accessor, bindValues);

            if (result.ResultSet.ColumnCount != 1)
                throw new SemanticErrorException($"sub-select returned {result.ResultSet.ColumnCount} columns, must only return 1 column");

            if (result.ResultSet.RowCount == 0)
                stack.Push(ExpressionOperand.NullLiteral());
            else if (result.ResultSet.RowCount == 1)
                stack.Push(result.ResultSet.Row(0)[0]);
            else
                throw new NotImplementedException($"don't know how to cope with {result.ResultSet.RowCount} rows in sub-select");
        }

        internal override BindResult Bind(IEngine engine, IList<FullColumnName> columns, IList<FullColumnName> outerColumnNames, IDictionary<string, ExpressionOperand> bindValues)
        {
            outerBindableColumns = new (outerColumnNames);
            outerBindableColumns.AddRange(columns);
            return selectContext.Bind(engine, outerBindableColumns, bindValues);
        }
    }
}

