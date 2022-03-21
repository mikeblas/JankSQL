namespace JankSQL
{
    using Antlr4.Runtime.Misc;

    public partial class JankListener : TSqlParserBaseListener
    {
        public override void ExitCase_expression([NotNull] TSqlParser.Case_expressionContext context)
        {
            base.ExitCase_expression(context);

            var elseMsg = context.elseExpr.GetText();
            Console.WriteLine($"Case ELSE expression: {elseMsg}");
        }
    }
}
