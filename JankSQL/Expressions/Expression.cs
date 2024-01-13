namespace JankSQL
{
    using System.Diagnostics;
    using JankSQL.Expressions;

    [DebuggerDisplay("Expression = {ToString()}")]
    public class Expression : List<ExpressionNode>, IEquatable<Expression>
    {
        internal Expression()
        {
            ContainsAggregate = false;
        }

        internal bool ContainsAggregate { get; set; }

        public override string ToString()
        {
            return string.Join(", ", this);
        }

        public object Clone()
        {
            var clone = new Expression();

            foreach (var node in this)
                clone.Add(node);

            return clone;
        }

        public virtual bool Equals(Expression? other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            if (ReferenceEquals(this, other))
                return true;

            if (this.Count != other.Count)
                return false;

            for (int i = 0; i < this.Count; i++)
            {
                if (!this[i].Equals(other[i]))
                    return false;
            }

            return true;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Expression);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Returns a FullColumnName describing this column if it is a simple column value.
        /// Any additional expression work will cause a null return.
        /// </summary>
        /// <returns>A FullColumnName for columns, null otherwise.</returns>
        public FullColumnName? GetExpressionColumnName()
        {
            // must be exactly 1 expression
            if (this.Count != 1)
                return null;

            // and that expression must be an ExpressionOperandFromColumn
            if (this[0] is not ExpressionOperandFromColumn fc)
                return null;

            return fc.ColumnName;
        }

        internal ExpressionOperand Evaluate(IRowValueAccessor? accessor, Engines.IEngine engine, Dictionary<string, ExpressionOperand> bindValues)
        {
            Stack<ExpressionOperand> stack = new ();

            do
            {
                foreach (ExpressionNode n in this)
                    n.Evaluate(engine, accessor, stack, bindValues);
            }
            while (stack.Count > 1);

            ExpressionOperand result = (ExpressionOperand)stack.Pop();

            // Console.WriteLine($"Evaluated {this} ==> [{result}]");

            return result;
        }

        internal ExpressionOperand EvaluateContained()
        {
            Stack<ExpressionOperand> stack = new ();

            do
            {
                foreach (ExpressionNode n in this)
                    n.Evaluate(null, null, stack, null);
            }
            while (stack.Count > 1);

            ExpressionOperand result = (ExpressionOperand)stack.Pop();

            // Console.WriteLine($"EvaluateContained {this} ==> [{result}]");

            return result;
        }

    }
}
