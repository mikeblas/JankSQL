
# Table Structure

JankSQL uses the [CSharpTest.Net](https://github.com/csharptest/CSharpTest.Net.Collections) B-Tree implementation, which is some amazing software. It implements a simple interface in its `BTree` generic class so that we can make a BTree of keys and values over `BTree<Key, Value>`. The class supports persistence and locking and gives enumerators that look up keys and walks the values available starting at a key.

In JankSQL, The `BTree` class is used with the `Tuple` class that implements a tuple of typed values, each represented with the `ExpressionOperand` class. Tuple represents a set of values, so it's used for both the key and the value. Thus, JankSQL's use of `BTree` is always on `BTree<Tuple, Tuple>`. Tuple has helpers that implement the comparison and persistence interfaces that `BTree` requires. 

## Tables

JankSQL implements a table, then, with a key-value store built on a `BTree<Tuple, Tuple>` object. The value `Tuple` contains all of the columns of the table. The key is a `Tuple` that contains a single integer which is used as a monotonically increasing row ID.

Since the table has no index, any operation against it is a scan. Inserting a new row simply adds a one to the last used key and inserts the row as the value for that key. Deleting a row simply removes the row, and the key number is not re-used.

For now, this approach is quite adequate, but it does mean that a table can't survive more than 2<sup>32</sup> operations because the row ID value will wrap-around. (This is tracked by [Issue #2](https://github.com/mikeblas/JankSQL/issues/2)).

Conceptually, we can consider table's fundamental storage -- sometimes called it "heap", perhaps incorrectly -- to be a map between the row ID and the actual row payload: `BTree<RowID, Tuple>`. It's just that the row ID itself is implemented as a `Tuple`, too.

## Unique Indexes

A unique index in Jank augments the fundamental `BTree` with another access path. Each index is implemented a map from the keys of the index to the row ID. We can consider the table and the first index as an example:

```csharp
BTree<Tuple, Tuple> theTable;	// key: row ID, value: rows
BTree<Tuple, Tuple> firstIndex; // key: index key, value: row ID
```

To find a row, we can look it up by key in `firstIndex` to get a row ID. Then, to get the remaining columns, the row ID is used to probe `theTable` to get that payload.

Any number of indexes can be created, all referencing back to `theTable` via the row ID key.

## Non-unique Indexes

Classically, BTrees implement only unique indexes: keys can't be duplicated. CSharpTest's implementation is no different, so Jank must provide some mechanism for handling duplicate key values in non-unique indexes.

Jank's approach simply appends a unique ID to the key set. If a non-unique index is created with key columns `Col1` and `Col2`, the effective key becomes `(Col1, Col2, uniqueifier)`. A probe for a value into a non-unique index naturally is a scan, since there may be zero, one, or more values matching the key due to its non-unique nature.

Jank is limited again by using a 32-bit integer here, so any non-unique index an have only 2<sup>32</sup> keys with the same value.

## Index maintenance

The addition or removal of a row to the table updates all indexes. Updating a value in an existing row must update the indexes that cover that column.

