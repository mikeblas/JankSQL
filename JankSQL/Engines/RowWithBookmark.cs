namespace JankSQL.Engines
{
    public class RowWithBookmark
    {
        private readonly Tuple rowData;
        private readonly ExpressionOperandBookmark bookmark;

        public RowWithBookmark(Tuple rowData, ExpressionOperandBookmark bookmark)
        {
            this.rowData = rowData;
            this.bookmark = bookmark;
        }

        public Tuple RowData
        {
            get { return rowData; }
        }

        public ExpressionOperandBookmark Bookmark
        {
            get { return bookmark; }
        }
    }
}
