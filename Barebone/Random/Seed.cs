using System.Numerics;
using System.Runtime.CompilerServices;

namespace BareBone.Random
{
    /// <summary>
    /// Stable/deterministic hash-based seed generation for game procgen. Supposed to be used in a hierarchy of methods
    /// that generates game content, where you use seed.Combine(x) to generate sub-seeds for sub-methods called,
    /// where 'x' is a constant value within the current method, representing a certain feature of the procgen. 
    /// (Based on .NET's <see cref="HashCode"/> which is based on xxHash: https://github.com/Cyan4973/xxHash.)
    /// </summary>
    public readonly struct Seed(uint value) : IEquatable<Seed>
    {
        public readonly uint Value = value;
        public int SValue => (int)value;

        private const uint SeedInternal = 1546519154U;

        private const uint Prime1 = 2654435761U;
        private const uint Prime2 = 2246822519U;
        private const uint Prime3 = 3266489917U;
        private const uint Prime4 = 668265263U;
        private const uint Prime5 = 374761393U;

        /// <summary>
        /// Finds a hashcode for the string. (because <see cref="string"/>.Hashcode() is different each time the app is restarted.)
        /// </summary>
        public static Seed ForString(string s)
        {
            unchecked
            {
                var hash1 = 5381u;
                var hash2 = hash1;

                for (var i = 0; i < s.Length && s[i] != '\0'; i += 2)
                {
                    hash1 = (hash1 << 5) + hash1 ^ s[i];
                    if (i == s.Length - 1 || s[i + 1] == '\0')
                        break;
                    hash2 = (hash2 << 5) + hash2 ^ s[i + 1];
                }

                return new Seed(hash1 + hash2 * 1566083941u);
            }
        }

        public static Seed For(int value1)
        {
            // Provide a way of diffusing bits from something with a limited
            // input hash space. For example, many enums only have a few
            // possible hashes, only using the bottom few bits of the code. Some
            // collections are built on the assumption that hashes are spread
            // over a larger space, so diffusing the bits may help the
            // collection work more efficiently.

            uint hc1 = (uint)value1;

            uint hash = MixEmptyState();
            hash += 4;

            hash = QueueRound(hash, hc1);

            hash = MixFinal(hash);
            return new Seed(hash);
        }

        public static Seed For(int value1, int value2)
        {
            uint hc1 = (uint)value1;
            uint hc2 = (uint)value2;

            uint hash = MixEmptyState();
            hash += 8;

            hash = QueueRound(hash, hc1);
            hash = QueueRound(hash, hc2);

            hash = MixFinal(hash);
            return new Seed(hash);
        }

        public Seed Combine(int value)
        {
            uint hc1 = (uint)value;

            var hash = Value;
            hash += 4;

            hash = QueueRound(hash, hc1);

            hash = MixFinal(hash);
            return new Seed(hash);
        }

        public Seed Combine(int value1, int value2)
        {
            uint hc1 = (uint)value1;
            uint hc2 = (uint)value2;

            var hash = Value;
            hash += 8;

            hash = QueueRound(hash, hc1);
            hash = QueueRound(hash, hc2);

            hash = MixFinal(hash);
            return new Seed(hash);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint QueueRound(uint hash, uint queuedValue)
        {
            return BitOperations.RotateLeft(hash + queuedValue * Prime3, 17) * Prime4;
        }

        private static uint MixEmptyState()
        {
            return SeedInternal + Prime5;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MixFinal(uint hash)
        {
            hash ^= hash >> 15;
            hash *= Prime2;
            hash ^= hash >> 13;
            hash *= Prime3;
            hash ^= hash >> 16;

            return hash;
        }

        private static readonly char[] Digits = new[]
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 
            'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        public override string ToString()
        {
            var result = new char[7];
            var v = Value;
            for (var i = 6; i >= 0; i--)
            {
                result[i] = Digits[v % Digits.Length];
                v = v / (uint)Digits.Length;
            }

            return new string(result).Insert(3, " ");
        }

        public static implicit operator uint(Seed h) => h.Value;
        public static implicit operator Seed(uint h) => new(h);

        public bool Equals(Seed other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            return obj is Seed other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (int)Value;
        }
    }
}