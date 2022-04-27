namespace JankSQL.Expressions.Functions
{
    internal class FunctionDateDiff : ExpressionFunction
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

            { "month",  DatePart.MONTH },
            { "mm",     DatePart.MONTH },
            { "m",      DatePart.MONTH },

            { "quarter",DatePart.QUARTER },
            { "q",      DatePart.QUARTER },
            { "qq",     DatePart.QUARTER },

            { "year",   DatePart.YEAR },
            { "yy",     DatePart.YEAR },
            { "y",      DatePart.YEAR },
            { "yyyy",   DatePart.YEAR },
        };
#pragma warning restore SA1001 // Commas should be spaced correctly
#pragma warning restore SA1509 // Opening braces should not be preceded by blank line

        private readonly DatePart datePart;

        internal FunctionDateDiff(string datePartName)
            : base("DATEDIFF")
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
            ExpressionOperand dateRight = stack.Pop();
            ExpressionOperand dateLeft = stack.Pop();

            var startDate = dateLeft.AsDateTime();
            var endDate = dateRight.AsDateTime();

            TimeSpan ts = endDate.Subtract(startDate);

            //REVIEW: DateDiff is hard, so this is half-baked. Need to decide how to do it.
            var ret = datePart switch
            {
                DatePart.DAY or DatePart.DAYOFYEAR => (int)ts.TotalDays,
                DatePart.YEAR => endDate.Year - startDate.Year,
                DatePart.MONTH => (endDate.Month + (endDate.Year * 12)) - (startDate.Month + (startDate.Year * 12)),
                DatePart.HOUR => (int)ts.TotalHours,
                DatePart.MINUTE => (int)ts.TotalMinutes,
                DatePart.SECOND => (int)ts.TotalSeconds,
                DatePart.MILLISECOND => (int)ts.TotalMilliseconds,
                _ => throw new InternalErrorException($"Can't handle datepart {datePart}"),
            };

            ExpressionOperand result = ExpressionOperand.IntegerFromInt(ret);
            stack.Push(result);
        }
    }
}
