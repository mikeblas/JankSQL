namespace JankSQL.Expressions
{
    using Antlr4.Runtime;
    using JankSQL.Expressions.Functions;

    internal abstract class ExpressionFunction : ExpressionNode, IEquatable<ExpressionFunction>
    {
        // here's a dictionary of functions by string name to the classes which work them;
        // this is initialized from FunctionTypeDict by the static constructor.
        // internal functions are always unaccented English, so we use OrdinalIgnoreCase here
        private static readonly Dictionary<string, Func<ExpressionFunction>> FunctionNameDict = new (StringComparer.OrdinalIgnoreCase);

        // here's a dictionary of functions by string name to the classes which work them
        private static readonly Dictionary<Type, Func<ExpressionFunction>> FunctionTypeDict = new ()
        {
            { typeof(TSqlParser.CASTContext),       () => new FunctionCast() },
            { typeof(TSqlParser.DATEADDContext),    () => new FunctionDateAdd() },
            { typeof(TSqlParser.DATEDIFFContext),   () => new FunctionDateDiff() },
            { typeof(TSqlParser.GETDATEContext),    () => new FunctionGetDate() },
            { typeof(TSqlParser.IIFContext),        () => new FunctionIIF() },
            { typeof(TSqlParser.ISNULLContext),     () => new FunctionIsNull() },
            { typeof(TSqlParser.LENContext),        () => new FunctionLEN() },
            { typeof(TSqlParser.PIContext),         () => new FunctionPI() },
            { typeof(TSqlParser.POWERContext),      () => new FunctionPOWER() },
            { typeof(TSqlParser.SQRTContext),       () => new FunctionSQRT() },
        };

        private readonly string name;

        static ExpressionFunction()
        {
            foreach (var func in FunctionTypeDict)
            {
                var f = func.Value.Invoke();
                FunctionNameDict.Add(f.name, func.Value);
            }
        }

        internal ExpressionFunction(string name)
        {
            this.name = name;
        }

        internal abstract int ExpectedParameters { get; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ExpressionOperator);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override string ToString()
        {
            return $"{name}()";
        }

        public bool Equals(ExpressionFunction? other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return name.Equals(other.name, StringComparison.OrdinalIgnoreCase);
        }

        internal static ExpressionFunction? FromFunctionName(string str)
        {
            if (FunctionNameDict.TryGetValue(str, out Func<ExpressionFunction>? value))
            {
                var r = value.Invoke();
                return r;
            }

            return null;
        }

        /// <summary>
        /// Find an ExpressionFunction-derived object that will implement the language function, given
        /// the parser type of that function.
        /// </summary>
        /// <param name="t">Parser type of the desired function.</param>
        /// <returns>ExpressionFunction object, null if not known.</returns>
        internal static ExpressionFunction? FromFunctionType(Type t)
        {
            if (!FunctionTypeDict.TryGetValue(t, out Func<ExpressionFunction>? value))
                return null;

            var r = value.Invoke();
            return r;
        }

        internal abstract void SetFromBuiltInFunctionsContext(IList<ParserRuleContext> stack, TSqlParser.Built_in_functionsContext bifContext);
    }
}
