using System.Buffers;
using System.Text;

namespace Barebone.Pools
{
    public record MemorySnapshot(Dictionary<IPool, int> Pools)
    {
        public MemorySnapshotDiff GetDiff(MemorySnapshot b)
        {
            var diffsPerPool = new Dictionary<IPool, int>();
            var a = this;
            foreach (var poolA in a.Pools.Keys)    
            {
                if (b.Pools.TryGetValue(poolA, out var poolBRentedCount))
                {
                    // pool found in A and B.
                    diffsPerPool.Add(poolA, poolBRentedCount - a.Pools[poolA]);
                }
                else
                {
                    // pool from A not found in B
                    diffsPerPool.Add(poolA, -poolA.RentedCount); 
                }
            }

            foreach (var poolB in b.Pools.Keys)
            {
                if (!a.Pools.ContainsKey(poolB))
                {
                    // pool not found in A but is so in B.
                    diffsPerPool.Add(poolB, poolB.RentedCount);
                }
            }

            return new MemorySnapshotDiff(diffsPerPool);
        }
    }

    public record MemorySnapshotDiff(Dictionary<IPool, int> RentedCountDiffPerPool)
    {
        public bool IsEmpty => RentedCountDiffPerPool.All(kvp => kvp.Value == 0);

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var kvp in RentedCountDiffPerPool)
            {
                if (kvp.Value == 0) continue;
                sb.AppendLine($"{kvp.Key.Name}: rentcount difference: {kvp.Value}");
            }

            return sb.ToString();
        }
    };

    /// <summary>
    /// The central memory pool for renting and returning all objects needed by the game.
    /// </summary>
    public static class Pool
    {
        /// <summary>
        /// Rents an available instance of T and calls its Rented(). Calls Construct() on it so you can initialize its fields,
        /// as you would in a constructor of a not pooled object. Call Return() when you're done with it.
        /// </summary>
        public static T Rent<T>() where T : Poolable, new() => Pool<T>.Shared.Rent();
        /// <summary>
        /// Returns a previously rented instance to the pool. Calls Destruct() on it so you can reset all fields.
        /// </summary>
        public static void Return<T>(T t) where T : Poolable, new() => Pool<T>.Shared.Return(t);

        /// <summary>
        /// Like Rent, but Construct() is not called on the rented instance. Used for simple situations that don't need the extra call.
        /// </summary>
        public static T RentNoConstruct<T>() where T : Poolable, new() => Pool<T>.Shared.Rent();
        /// <summary>
        /// Like Return, but Destruct() is not called on the returned instance. Used for simple situations that don't need the extra call.
        /// </summary>
        public static void ReturnNoDestruct<T>(T t) where T : Poolable, new() => Pool<T>.Shared.Return(t);

        public static HashSet<IPool> UsedPools = new();
        public static string GetReport()
        {
            var sb = new StringBuilder();
            foreach (var pool in UsedPools)
            {
                sb.AppendLine((pool as IPool)!.GetReport());
            }
            return sb.ToString();
        }

        public static MemorySnapshot GetMemorySnapshot()
        {
            return new MemorySnapshot(UsedPools.ToDictionary(p => p, p => p.RentedCount));
        }

        public static T[] RentArray<T>(int minCapacity)
        {
            return ArrayPool<T>.Shared.Rent(minCapacity);
        }

        public static void ReturnArray<T>(T[] array)
        {
            ArrayPool<T>.Shared.Return(array);
        }
    }

    /// <summary>
    /// Object pool for low GC pressure and memory leak detection in C-like pattern. Use this on objects for which you want no GC pressure and explicit control over the steps in their lifecycle.
    /// </summary>
    public class Pool<T> : IPool where T : Poolable, new()
    {
        public string Name => typeof(T).GetCSharpTypeName();
        private readonly BBList<T> _freeList = new();

        /// <summary>
        /// Preallocates a number of objects for later renting.
        /// </summary>
        public void Reserve(int count)
        {
            var idx = _freeList.Count;
            _freeList.EnsureCapacity(_freeList.Count + count);
            for (var i = 0; i < count; i++)
                _freeList.InternalArray[idx + i] = new T();
                
            _freeList.Count += count;
        }

        public T Rent()
        {
#if DEBUG
            Pool.UsedPools.Add(this); // add if not yet
#endif
            T t;
            if (_freeList.Count == 0)
            {
                t = new T
                {
                    OriginPool = this
                };
            }
            else
            {
                t = _freeList.Pop();
            }
            
#if DEBUG
            if (t.IsRented) throw new Exception($"Instance ('{this}') is already taken.");
#endif
            t.IsRented = true;
            t.Construct();
            RentedCount++;
            return t;
        }

        public T RentNoConstruct()
        {
#if DEBUG
            Pool.UsedPools.Add(this); // add if not yet
#endif
            var t = _freeList.Count == 0
                ? new T()  // no more pooled instances, make one. This instance will later be returned, making the pool size larger indefinitely.
                : _freeList.Pop();
#if DEBUG
            if (t.IsRented) throw new Exception($"Instance ('{this}') is already taken.");
#endif
            t.IsRented = true;
            RentedCount++;
            return t;
        }

        public void ReturnNoDestruct(T t)
        {
#if DEBUG
            if (!t.IsRented) throw new Exception($"Instance ('{this}') was already Returned.");
#endif
            t.IsRented = false;
            _freeList.Add(t);
            RentedCount--;
        }

        public void Return(T t)
        {
#if DEBUG
            if (!t.IsRented) throw new Exception($"Instance ('{this}') was already Returned.");
#endif
            t.Destruct();
            t.IsRented = false;
            _freeList.Add(t);
            RentedCount--;
        }

        public void Return(object o)
        {
            var t = (T)o;
#if DEBUG
            if (!t.IsRented) throw new Exception($"Instance ('{this}') was already Returned.");
#endif
            t.Destruct();
            t.IsRented = false;
            _freeList.Add(t);
            RentedCount--;
        }

        public void ReturnNoDestruct(object o)
        {
            var t = (T)o;
#if DEBUG
            if (!t.IsRented) throw new Exception($"Instance ('{this}') was already Returned.");
#endif
            t.IsRented = false;
            _freeList.Add(t);
            RentedCount--;
        }

        ~Pool()
        {
            if (RentedCount > 0) throw new MemoryLeakException($"Pool<{typeof(T)}> still was renting {RentedCount} instance(s) when it was finalized by GC.");
        }

        public string GetReport()
        {
            return $"Pool {Name}(rented: {RentedCount}, available: {_freeList.Count})";
        }

        public int RentedCount { get; private set; }

        public static Pool<T> Shared = new();
    }
}
