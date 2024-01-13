namespace JankSQL.Expressions.Functions
{
    using Antlr4.Runtime;

    internal class FunctionDateAdd : ExpressionFunction
    {
#pragma warning disable SA1509 // Opening braces should not be preceded by blank line
#pragma warning disable SA1001 // Commas should be spaced correctly
        // OrdinalIgnoreCase here because names are always unaccented English
        private static readonly Dictionary<string, DatePart> PartMap = new (StringComparer.OrdinalIgnoreCase)
        {
            { "nanosecond", DatePart.NANOSECOND },
            { "ns",    DatePart.NANOSECOND },

            { "microsecond", DatePart.MICROSECOND },
            { "mcs",    DatePart.MICROSECOND },

            { "second", DatePart.SECOND },
            { "s",      DatePart.SECOND },
            { "ss",     DatePart.SECOND },

            { "minute", DatePart.MINUTE },
            { "mi",     DatePart.MINUTE },
            { "n",      DatePart.MINUTE },

            { "hour",   DatePart.HOUR },
            { "hh",     DatePart.HOUR },

            { "day",    DatePart.DAY },
            { "d",      DatePart.DAY },
            { "dd",     DatePart.DAY },

            { "daydreamer",DatePart.DAYOFYEAR },
            { "dy",     DatePart.DAYOFYEAR },
            { "y",      DatePart.DAYOFYEAR },

            { "week",   DatePart.WEEK },
            { "wk",     DatePart.WEEK },
            { "ww",     DatePart.WEEK },

            { "weekday",DatePart.WEEKDAY },
            { "dw",     DatePart.WEEKDAY },
            { "w",      DatePart.WEEKDAY },

            { "month",  DatePart.MONTH },
            { "mm",     DatePart.MONTH },
            { "m",      DatePart.MONTH },

            { "quarter",DatePart.QUARTER },
            { "q",      DatePart.QUARTER },
            { "qq",     DatePart.QUARTER },

            { "year",   DatePart.YEAR },
            { "yy",     DatePart.YEAR },
            { "yyyy",   DatePart.YEAR },
        };
#pragma warning restore SA1001 // Commas should be spaced correctly
#pragma warning restore SA1509 // Opening braces should not be preceded by blank line

        private DatePart datePart;

        internal FunctionDateAdd()
            : base("DATEADD")
        {
        }

        private enum DatePart
        {
            YEAR,
            QUARTER,
            MONTH,
            DAYOFYEAR,
            DAY,
            WEEK,
            WEEKDAY,
            HOUR,
            MINUTE,
            SECOND,
            MILLISECOND,
            MICROSECOND,
            NANOSECOND,
        }

        internal override int ExpectedParameters => 2;

        internal override void Evaluate(Engines.IEngine engine, IRowValueAccessor? accessor, Stack<ExpressionOperand> stack, Dictionary<string, ExpressionOperand> bindValues)
        {
            EvaluateContained(stack);
        }

        internal override void EvaluateContained(Stack<ExpressionOperand> stack)
        {
            ExpressionOperand dateValue = stack.Pop();
            ExpressionOperand number = stack.Pop();
            var delta = number.AsInteger();
            var startDate = dateValue.AsDateTime();

            var ret = datePart switch
            {
                DatePart.DAY or DatePart.DAYOFYEAR or DatePart.WEEKDAY => startDate.AddDays(delta),
                DatePart.YEAR => startDate.AddYears(delta),
                DatePart.MONTH => startDate.AddMonths(delta),
                DatePart.HOUR => startDate.AddHours(delta),
                DatePart.MINUTE => startDate.AddMinutes(delta),
                DatePart.SECOND => startDate.AddSeconds(delta),
                DatePart.MILLISECOND => startDate.AddMilliseconds(delta),
                _ => throw new InternalErrorException($"Can't handle datePart {datePart}"),
            };

            ExpressionOperand result = ExpressionOperand.DateTimeFromDateTime(ret);
            stack.Push(result);
        }

        internal override void SetFromBuiltInFunctionsContext(IList<ParserRuleContext> stack, TSqlParser.Built_in_functionsContext bifContext)
        {
            var c = (TSqlParser.DATEADDContext)bifContext;

            string datePartName = c.dateparts_12().GetText();

            if (!PartMap.TryGetValue(datePartName, out datePart))
                throw new SemanticErrorException($"Unknown date part {datePartName}");
            if (datePart == DatePart.MICROSECOND || datePart == DatePart.NANOSECOND || datePart == DatePart.QUARTER)
                throw new SemanticErrorException($"Unsupported date part {datePartName}");

            stack.Add(c.number);
            stack.Add(c.date);
        }
    }
}
