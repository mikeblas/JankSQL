namespace JankSQL.Expressions.Functions
{
    internal class FunctionDateAdd : ExpressionFunction
    {
#pragma warning disable SA1509 // Opening braces should not be preceded by blank line
#pragma warning disable SA1001 // Commas should be spaced correctly
        private static readonly Dictionary<string, DatePart> PartMap = new (StringComparer.CurrentCultureIgnoreCase)
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

            { "dayofyear",DatePart.DAYOFYEAR },
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

        private readonly DatePart datePart;

        internal FunctionDateAdd(string datePartName)
            : base("DATEADD")
        {
            if (!PartMap.TryGetValue(datePartName, out datePart))
                throw new SemanticErrorException($"Unknown date part {datePartName}");

            if (datePart == DatePart.MICROSECOND || datePart == DatePart.NANOSECOND || datePart == DatePart.QUARTER)
                throw new SemanticErrorException($"Unsupported date part {datePartName}");
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
                _ => throw new InternalErrorException($"Can't handle datepart {datePart}"),
            };

            ExpressionOperand result = ExpressionOperand.DateTimeFromDateTime(ret);
            stack.Push(result);
        }
    }
}
