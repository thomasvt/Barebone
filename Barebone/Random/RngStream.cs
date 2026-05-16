using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using Barebone.Geometry;

namespace Barebone.Random
{
    /// <summary>
    /// Deterministic, hierarchical RNG stream based on seeds. Recursively use method Child(x) with fixed arguments to get deterministic sub-streams for child-aspects of the game.
    /// </summary>
    public record struct RngStream(ulong Seed)
    {
        private ulong _state;

        /// <summary>
        /// Request a stream for a child of the current item, e.g. a particular child-entity or aspect of it. The same <c>id</c> will always yield the same sub-stream.
        /// The current stream's state has no influence on the obtained child stream.
        /// </summary>
        [Pure]
        public RngStream Child(ulong id) => new(Splitmix64(Seed ^ Splitmix64(id)));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong NextU64()
        {
            _state = Splitmix64(_state);
            return _state;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint NextU32() => (uint)(NextU64() >> 32);

        /// <summary>Returns a random float within [0, 1). 24-bit mantissa precision (top 24 bits of a u64).</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat() => (NextU64() >> 40) * (1.0f / 16_777_216f);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat(float max) => NextFloat() * max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat(float min, float max) => min + NextFloat() * (max - min);

        /// <summary>Inclusive integer range [min, maxIncl]. Modulo bias is acceptable for game RNG.</summary>
        public int NextInt(int min, int maxIncl)
        {
            if (maxIncl < min) throw new ArgumentException($"maxIncl ({maxIncl}) < min ({min})", nameof(maxIncl));
            var span = (uint)(maxIncl - min + 1);
            return min + (int)(NextU32() % span);
        }

        /// <summary>
        /// Returns true if a roll with given chance [0,1) succeeds.
        /// </summary>
        public bool NextChance(float chance) => NextFloat() < chance;

        /// <summary>
        /// Returns a random vector within a bounding box.
        /// </summary>
        public Vector2 NextVector2(Aabb aabb) => new Vector2(NextFloat(aabb.MinCorner.X, aabb.MaxCorner.X), NextFloat(aabb.MinCorner.Y, aabb.MaxCorner.Y));

        /// <summary>
        /// Returns a unit vector in a random direction.
        /// </summary>
        public Vector2 NextUnitVector2() => NextFloat(0, Angles._360).AngleToVector2();

        public Vector2I NextVector2I(AabbI aabb) => new Vector2I(NextInt(aabb.MinCorner.X, aabb.MaxCornerIncl.X), NextInt(aabb.MinCorner.Y, aabb.MaxCornerIncl.Y));

        /// <summary>splitmix64 finalizer (Stafford's variant 13). Strong avalanche; cheap; deterministic.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ulong Splitmix64(ulong x)
        {
            x ^= x >> 30;
            x *= 0xBF58476D1CE4E5B9UL;
            x ^= x >> 27;
            x *= 0x94D049BB133111EBUL;
            x ^= x >> 31;
            return x;
        }
    }
}
