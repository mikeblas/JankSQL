﻿namespace JankSQL.Engines
{
    public class RowWithBookmark
    {
        private readonly ExpressionOperand[] rowData;
        private readonly ExpressionOperandBookmark bookmark;

        public RowWithBookmark(ExpressionOperand[] rowData, ExpressionOperandBookmark bookmark)
        {
            this.rowData = rowData;
            this.bookmark = bookmark;
        }

        public ExpressionOperand[] RowData
        {
            get { return rowData; }
        }

        public ExpressionOperandBookmark Bookmark
        {
            get { return bookmark; }
        }
    }
}
