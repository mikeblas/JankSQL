namespace JankSQL.Engines
{
    using System.Collections;
    using CSharpTest.Net.Collections;

    using JankSQL.Expressions;

    public class IndexAccessor : IEnumerable, IEnumerable<RowWithBookmark>
    {
        private readonly IndexDefinition def;
        private readonly BPlusTree<Tuple, Tuple> indexTree;
        private readonly ExpressionComparisonOperator[]? comparisons;
        private readonly Expression[]? expressions;

        internal IndexAccessor(IndexDefinition indexDefinition, BPlusTree<Tuple, Tuple> index)
        {
            def = indexDefinition;
            indexTree = index;
        }

        internal IndexAccessor(IndexDefinition indexDefinition, BPlusTree<Tuple, Tuple> index, IEnumerable<ExpressionComparisonOperator> comparisons, IEnumerable<Expression> expressions)
        {
            def = indexDefinition;
            indexTree = index;

            this.expressions = expressions.ToArray();
            this.comparisons = comparisons.ToArray();

            if (this.expressions.Length != this.comparisons.Length)
                throw new ArgumentException("comparisons and expressions must be the same length");
        }

        public IndexDefinition IndexDefinition
        {
            get { return def; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnum();
        }

        IEnumerator<RowWithBookmark> IEnumerable<RowWithBookmark>.GetEnumerator()
        {
            return GetEnum();
        }


        internal void Dump()
        {
            Console.WriteLine($"=====");
            Console.WriteLine($"index {def.IndexName}");
            string s = string.Join(",", def.ColumnInfos.Select(x => $"[{x.columnName}, {(x.isDescending ? "DESC" : "ASC")}]"));
            Console.WriteLine($"   {s}");

            foreach (var r in this)
                Console.WriteLine($"   {r.RowData} ==> {r.Bookmark}");
        }

        private IEnumerator<RowWithBookmark> GetEnum()
        {
            if (expressions == null)
                return new BTreeIndexRowEnumerator(indexTree, def);
            else
            {
                if (comparisons == null)
                    throw new InternalErrorException("expected expressions and comparisons");
                return new BTreeIndexRowEnumerator(indexTree, def, comparisons, expressions);
            }
        }
    }
}

