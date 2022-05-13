namespace JankSQL.Engines
{
    using System.Collections;
    using CSharpTest.Net.Collections;
    using JankSQL.Expressions;

    internal class BTreeIndexRowEnumerator : IEnumerator<RowWithBookmark>
    {
        private readonly IEnumerator<KeyValuePair<Tuple, Tuple>> treeEnumerator;
        private readonly IndexDefinition def;

        private readonly ExpressionComparisonOperator[]? comparisons;
        private readonly Expression[]? expressions;

        internal BTreeIndexRowEnumerator(BPlusTree<Tuple, Tuple> tree, IndexDefinition indexDefinition)
        {
            treeEnumerator = tree.GetEnumerator();
            def = indexDefinition;
        }

        internal BTreeIndexRowEnumerator(BPlusTree<Tuple, Tuple> tree, IndexDefinition indexDefinition, ExpressionComparisonOperator[] comparisons, Expression[] expressions)
        {
            def = indexDefinition;

            this.expressions = expressions.ToArray();
            this.comparisons = comparisons.ToArray();

            Tuple startKey = Tuple.CreateEmpty(this.expressions.Length + (def.IsUnique ? 0 : 1));

            // compute the matching values for the expressions
            for (int i = 0; i < this.expressions.Length; i++)
                startKey[i] = this.expressions[i].EvaluateContained();

            // add a bookmark to the key
            if (!def.IsUnique)
                startKey[this.expressions.Length] = ExpressionOperand.IntegerFromInt(0);

            treeEnumerator = (IEnumerator<KeyValuePair<Tuple, Tuple>>)tree.EnumerateFrom(startKey);
        }

        public RowWithBookmark Current
        {
            get
            {
                ExpressionOperandBookmark bookmarkResult = new (treeEnumerator.Current.Value);

                // if it's not unique, don't generate the uniquifier
                int keyLen = treeEnumerator.Current.Key.Count - (def.IsUnique ? 0 : 1);
                Tuple keyCopy = Tuple.CreatePartialCopy(keyLen, treeEnumerator.Current.Key);
                return new RowWithBookmark(keyCopy, bookmarkResult);
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose()
        {
            treeEnumerator.Dispose();
        }

        public bool MoveNext()
        {
            bool ret = treeEnumerator.MoveNext();

            if (expressions != null && ret)
            {
                // see if this still matches the key
                bool fullMatch = true;

                for (int i = 0; i < expressions.Length; i++)
                {
                    ExpressionOperand x = this.expressions[i].Evaluate(null, null, null);
                    if (!comparisons![i].DirectEvaluate(treeEnumerator.Current.Key[i], x))
                    {
                        fullMatch = false;
                        break;
                    }
                }

                ret = fullMatch;
            }

            return ret;
        }

        public void Reset()
        {
            treeEnumerator.Reset();
        }
    }
}
