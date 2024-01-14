namespace JankSQL.Engines
{
    using JankSQL.Expressions;

    public class RowWithBookmark
    {
        public RowWithBookmark(Tuple rowData, ExpressionOperandBookmark bookmark)
        {
            this.RowData = rowData;
            this.Bookmark = bookmark;
        }

        public Tuple RowData { get; }

        public ExpressionOperandBookmark Bookmark { get; }
    }
}
