namespace JankSQL
{
    internal static class ParseHelpers
    {
        internal static string GetEffectiveName(string objectName)
        {
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/ranges
            if (objectName[0] != '[' || objectName[^1] != ']')
                return objectName;

            return objectName[1..^1];
        }

        internal static string StringFromIDContext(TSqlParser.Id_Context idContext)
        {
            string? str = PossibleStringFromIDContext(idContext);
#pragma warning disable IDE0270 // Use coalesce expression
            if (str == null)
                throw new ArgumentException("idContext was null");
#pragma warning restore IDE0270 // Use coalesce expression

            return str!;
        }

        internal static string? PossibleStringFromIDContext(TSqlParser.Id_Context idContext)
        {
            if (idContext == null)
                return null;

            if (idContext.ID() != null)
                return idContext.ID().GetText();

            if (idContext.keyword() != null)
                return idContext.keyword().GetText();

            if (idContext.SQUARE_BRACKET_ID() != null)
                return GetEffectiveName(idContext.SQUARE_BRACKET_ID().GetText());

            if (idContext.DOUBLE_QUOTE_ID() != null)
                return idContext.DOUBLE_QUOTE_ID().GetText().Replace("\"", string.Empty);

            throw new InvalidOperationException("Unimaginable id_ context");
        }
    }
}
