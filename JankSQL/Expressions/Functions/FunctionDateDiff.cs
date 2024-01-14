namespace JankSQL.Expressions.Functions
{
    using Antlr4.Runtime;

    internal class FunctionDateDiff : ExpressionFunction
    {
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

        private DatePart datePart;

        internal FunctionDateDiff()
            : base("DATEDIFF")
        {
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

        internal override void SetFromBuiltInFunctionsContext(IList<ParserRuleContext> stack, TSqlParser.Built_in_functionsContext bifContext)
        {
            var c = (TSqlParser.DATEDIFFContext)bifContext;

            string datePartName = c.dateparts_12().GetText();

            if (!PartMap.TryGetValue(datePartName, out datePart))
                throw new SemanticErrorException($"Unknown date part {datePartName}");
            if (datePart == DatePart.MICROSECOND || datePart == DatePart.NANOSECOND || datePart == DatePart.QUARTER)
                throw new SemanticErrorException($"Unsupported date part {datePartName}");

            stack.Add(c.date_first);
            stack.Add(c.date_second);
        }
    }
}
