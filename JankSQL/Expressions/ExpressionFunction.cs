﻿namespace JankSQL.Expressions
{
    using JankSQL.Expressions.Functions;

    internal abstract class ExpressionFunction : ExpressionNode, IEquatable<ExpressionFunction>
    {
        // here's a dictionary of functions by string name to the classes which work them
        private static readonly Dictionary<string, Func<ExpressionFunction>> FunctionDict = new (StringComparer.InvariantCultureIgnoreCase)
        {
            { "PI", () => new FunctionPI() },
            { "SQRT", () => new FunctionSQRT() },
            { "POWER", () => new FunctionPOWER() },
        };

        private readonly string str;

        internal ExpressionFunction(string str)
        {
            this.str = str;
        }

        internal abstract int ExpectedParameters { get; }


        public override bool Equals(object? obj)
        {
            return Equals(obj as ExpressionOperator);
        }

        public override int GetHashCode()
        {
            return str.GetHashCode();
        }

        public override string ToString()
        {
            return $"{str}()";
        }

        public bool Equals(ExpressionFunction? other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            if (ReferenceEquals(this, other))
                return true;

            return str.Equals(other.str, StringComparison.OrdinalIgnoreCase);
        }

        internal static ExpressionFunction? FromFunctionName(string str)
        {
            if (!FunctionDict.ContainsKey(str))
                return null;

            var r = FunctionDict[str].Invoke();
            return r;
        }

        internal abstract ExpressionOperand Evaluate(Stack<ExpressionOperand> stack);
    }
}

