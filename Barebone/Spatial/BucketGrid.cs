using System.Diagnostics;
using Barebone.Geometry;

namespace Barebone.Spatial
{
    /// <summary>
    /// A regular grid for spatial querying with a bucket of particles per grid-cell.
    /// </summary>
    public class BucketGrid<T>
    {
        private readonly BBList<T>[] _buckets;

        public BucketGrid(Vector2I gridSize)
        {
            Size = gridSize;
            _buckets = new BBList<T>[gridSize.X * gridSize.Y];
            for (var i = 0; i < _buckets.Length; i++)
                _buckets[i] = new BBList<T>();
        }

        public void Add(Vector2I position, T item)
        {
            if (position.X < 0 || position.X >= Size.X || position.Y < 0 || position.Y >= Size.Y) throw new ArgumentOutOfRangeException(nameof(position));

            var bucket = _buckets[position.X + position.Y * Size.X];
            bucket.Add(item);
        }

        public void Remove(Vector2I position, T item)
        {
            if (position.X < 0 || position.X >= Size.X || position.Y < 0 || position.Y >= Size.Y) throw new ArgumentOutOfRangeException(nameof(position));

            var bucket = _buckets[position.X + position.Y * Size.X];
#if DEBUG
            if (!bucket.SwapRemove(item)) Debugger.Break();
#else
            bucket.SwapRemove(item);
#endif
        }

        /// <summary>
        /// Moves an item from an old to a new cell. Returns false if that new cell is out of range.
        /// In this last case, the item is removed from the old cell but added nowhere and therefore no more in this BucketGrid.
        /// </summary>
        public bool TryMove(T item, Vector2I oldCell, Vector2I newCell)
        {
            if (oldCell.X < 0 || oldCell.X >= Size.X || oldCell.Y < 0 || oldCell.Y >= Size.Y) throw new ArgumentOutOfRangeException(nameof(oldCell));
                
#if DEBUG
            if (!_buckets[oldCell.X + oldCell.Y * Size.X].SwapRemove(item)) Debugger.Break();
#else
            _buckets[oldCell.X + oldCell.Y * Size.X].SwapRemove(item);
#endif
            if (newCell.X < 0 || newCell.X >= Size.X || newCell.Y < 0 || newCell.Y >= Size.Y) return false;

            _buckets[newCell.X + newCell.Y * Size.X].Add(item);

            return true;
        }

        public int Append(Vector2I cell, BBList<T> buffer)
        {
            if (cell.X < 0 || cell.X >= Size.X || cell.Y < 0 || cell.Y >= Size.Y) return 0;

            var bucket = _buckets[cell.X + cell.Y * Size.X];
            if (bucket.Count == 0) return 0;
            buffer.AddBBList(bucket);
            return bucket.Count;
        }

        public int AppendNoCheck(Vector2I cell, BBList<T> buffer)
        {
            var bucket = _buckets[cell.X + cell.Y * Size.X];
            if (bucket.Count == 0) return 0;
            buffer.AddBBList(bucket);
            return bucket.Count;
        }

        /// <summary>
        /// Returns true if the cell is empty or out of bounds.
        /// </summary>
        public bool IsCellEmpty(Vector2I cell)
        {
            return cell.X < 0 || cell.X >= Size.X || cell.Y < 0 || cell.Y >= Size.Y 
                   || _buckets[cell.X + cell.Y * Size.X].Count == 0;
        }

        /// <summary>
        /// Appends the content of the range of buckets to your buffer. Cells that are out of bounds are considered empty, no error is thrown.
        /// </summary>
        public int Append(AabbI cellRange, BBList<T> buffer)
        {
            var count = 0;
            for (var x = cellRange.MinCorner.X; x < cellRange.MaxCornerExcl.X; x++)
                for (var y = cellRange.MinCorner.Y; y < cellRange.MaxCornerExcl.Y; y++)
                {
                    if (x < 0 || x >= Size.X || y < 0 || y >= Size.Y) continue;

                    var bucket = _buckets[x + y * Size.X];
                    buffer.AddBBList(bucket);
                    count += bucket.Count;
                }
            return count;
        }

        /// <summary>
        /// Appends the content of the range of buckets to your buffer. Cells that are out of bounds are considered empty, no error is thrown.
        /// </summary>
        public int AppendNoCheck(AabbI cellRange, BBList<T> buffer)
        {
            var count = 0;
            for (var x = cellRange.MinCorner.X; x < cellRange.MaxCornerExcl.X; x++)
            for (var y = cellRange.MinCorner.Y; y < cellRange.MaxCornerExcl.Y; y++)
            {
                var bucket = _buckets[x + y * Size.X];
                buffer.AddBBList(bucket);
                count += bucket.Count;
            }
            return count;
        }

        public void Clear()
        {
            foreach (var bucker in _buckets)
                bucker.Clear();
        }

        public Vector2I Size { get; private set; }
    }
}
