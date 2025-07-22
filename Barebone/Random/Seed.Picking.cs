using System.Numerics;
using System.Runtime.CompilerServices;
using Barebone.Geometry;

namespace BareBone.Random
{
    /// <summary>
    /// A collection of stable methods that generate data for game procgen based on a <see cref="Seed"/>.
    /// Point here is that the methods are stable, meaning that the same seed will always generate the same result.
    /// </summary>
    public static class SeedExtensions
    {

        /// <summary>
        /// Gets the vector on the unit circle (length is 1), within given angle constraints, that the hash represents.
        /// </summary>
        public static Vector2 PickUnitVector2(this Seed seed, float minAngle, float maxAngle)
        {
            var a = PickFloat(seed, minAngle, maxAngle);
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        }

        /// <summary>
        /// Gets the vector on the unit circle (length is 1), that the hash represents.
        /// </summary>
        public static Vector2 PickUnitVector2(this Seed seed)
        {
            var a = PickFloat(seed) * MathF.Tau;
            return new Vector2(MathF.Cos(a), MathF.Sin(a));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PickFloat(this Seed seed, float min, float max)
        {
            var t = PickFloat(seed);
            return min + t * (max - min);
        }

        /// <summary>
        /// returns [0, 1]
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PickFloat(this Seed seed)
        {
            return seed.Value / (float)uint.MaxValue;
            // took this from .NET's Random.XoshiroImpl.NextSingle() where they do it from an uint64.
            // See Random.NextSingle and go to its impl to find the XoshiroImpl code.
            // return (hash._hash >> 8) * (1.0f / (1u << 24));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PickInt(this Seed seed, int maxIncl)
        {
            return (int)(seed.Value % (uint)(maxIncl+1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PickInt(this Seed seed, int min, int maxIncl)
        {
            return min + (int)(seed.Value % (uint)(maxIncl - min + 1));
        }

        /// <summary>
        /// Returns the item from the collection that the seed represents.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Pick<T>(this Seed seed, IReadOnlyList<T> list)
        {
            var count = (uint)list.Count;
            if (count == 0) throw new ArgumentException("Cannot pick an item from an empty collection.");
            return list[(int)(seed.Value % count)];
        }

        /// <summary>
        /// Removes and returns the item from the list.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PickAndRemove<T>(this Seed seed, IList<T> list)
        {
            var l = (uint)list.Count;
            var idx = (int)(seed.Value % l);
            var item = list[idx];
            list.RemoveAt(idx);
            return item;
        }

        /// <summary>
        /// Picks a stable item using the seed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Pick<T>(this Seed seed, ReadOnlySpan<T> items)
        {
            var l = (uint)items.Length;
            return items[(int)(seed.Value % l)];
        }

        /// <summary>
        /// Returns a stable vector based on the seed.
        /// </summary>
        public static Vector2I PickVector2I(this Seed seed, AabbI range)
        {
            var count = range.Size.X * range.Size.Y;
            var pick1d = PickInt(seed, count-1);
            var pick2d = range.MinCorner + new Vector2I(pick1d % range.Size.X, pick1d / range.Size.X);
            return pick2d;
        }

        /// <summary>
        /// Returns a stable item from the ChanceSet based on the seed.
        /// </summary>
        public static T Pick<T>(this Seed seed, ChanceSet<T> set)
        {
            return set.Pick(seed.PickFloat());
        }
    }
}
