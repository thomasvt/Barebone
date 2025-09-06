using System.Numerics;
using System.Runtime.CompilerServices;
using Barebone.Geometry;

namespace BareBone.Random
{
    /// <summary>
    /// Random impl based on .NET's random that is stable on all platforms (same results everywhere).
    /// </summary>
    public class StableRandom(int seed)
    {
        private CompatPrng _prng = new(seed); // mutable struct; do not make this readonly

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt() => _prng.InternalSample();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int maxValue) => (int)(_prng.Sample() * maxValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int NextInt(int minValue, int maxValue)
        {
            var range = maxValue - minValue;
            return (int)(_prng.Sample() * range) + minValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte NextByte() => (byte)_prng.InternalSample();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte NextByte(byte maxValue) => (byte)(_prng.Sample() * maxValue);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte NextByte(byte minValue, byte maxValue)
        {
            var range = maxValue - minValue;
            return (byte)(_prng.Sample() * range + minValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long NextInt64()
        {
            while (true)
            {
                // Get top 63 bits to get a value in the range [0, long.MaxValue], but try again
                // if the value is actually long.MaxValue, as the method is defined to return a value
                // in the range [0, long.MaxValue).
                ulong result = NextUInt64() >> 1;
                if (result != long.MaxValue)
                {
                    return (long)result;
                }
            }
        }

        /// <summary>Produces a value in the range [0, ulong.MaxValue].</summary>
        private ulong NextUInt64() =>
             ((ulong)(uint)NextInt(1 << 22)) |
            (((ulong)(uint)NextInt(1 << 22)) << 22) |
            (((ulong)(uint)NextInt(1 << 20)) << 44);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble() => _prng.Sample();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat(float max) => (float)_prng.Sample() * max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat(float min, float max) => (float)_prng.Sample() * (max - min) + min;

        /// <summary>
        /// A random float between 0 and 1
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat() => (float)_prng.Sample();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(byte[] buffer) => _prng.NextBytes(buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NextBytes(Span<byte> buffer) => _prng.NextBytes(buffer);

        /// <summary>
        /// Picks a random item from the array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pick<T>(IReadOnlyList<T> items)
        {
            return items[NextInt(0, items.Count)];
        }

        /// <summary>
        /// Picks a random item from the array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pick<T>(ChanceSet<T> set)
        {
            return set.Pick(NextFloat());
        }

        /// <summary>
        /// Returns a vector in a random direction with random length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 NextVector(float minLength, float maxLength)
        {
            return NextFloat(0, Angles._360).AngleToVector2(NextFloat(minLength, maxLength));
        }

        /// <summary>
        /// Returns a vector in a random direction of the given length.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 NextVector(float length = 1f)
        {
            return NextFloat(0, Angles._360).AngleToVector2(length);
        }

        /// <summary>
        /// Returns a vector with random length, in a random direction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 NextVector(Aabb range)
        {
            return new(NextFloat(range.Left, range.Right), NextFloat(range.Bottom, range.Top));
        }
    }

    /// <summary>
    /// The algorithm is based on a modified version
    /// of Knuth's subtractive random number generator algorithm.  See https://github.com/dotnet/runtime/issues/23198
    /// for a discussion of some of the modifications / discrepancies.
    /// </summary>
    internal struct CompatPrng
    {
        private readonly int[] _seedArray;
        private int _inext;
        private int _inextp;

        public CompatPrng(int seed)
        {
            // Initialize seed array.
            int[] seedArray = new int[56];

            int subtraction = (seed == int.MinValue) ? int.MaxValue : Math.Abs(seed);
            int mj = 161803398 - subtraction; // magic number based on Phi (golden ratio)
            seedArray[55] = mj;
            int mk = 1;

            int ii = 0;
            for (int i = 1; i < 55; i++)
            {
                // The range [1..55] is special (Knuth) and so we're wasting the 0'th position.
                if ((ii += 21) >= 55)
                {
                    ii -= 55;
                }

                seedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0)
                {
                    mk += int.MaxValue;
                }

                mj = seedArray[ii];
            }

            for (int k = 1; k < 5; k++)
            {
                for (int i = 1; i < 56; i++)
                {
                    int n = i + 30;
                    if (n >= 55)
                    {
                        n -= 55;
                    }

                    seedArray[i] -= seedArray[1 + n];
                    if (seedArray[i] < 0)
                    {
                        seedArray[i] += int.MaxValue;
                    }
                }
            }

            _seedArray = seedArray;
            _inext = 0;
            _inextp = 21;
        }

        internal double Sample() =>
            // Including the division at the end gives us significantly improved random number distribution.
            InternalSample() * (1.0 / int.MaxValue);

        internal void NextBytes(Span<byte> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)InternalSample();
            }
        }

        internal int InternalSample()
        {
            int locINext = _inext;
            if (++locINext >= 56)
            {
                locINext = 1;
            }

            int locINextp = _inextp;
            if (++locINextp >= 56)
            {
                locINextp = 1;
            }

            int[] seedArray = _seedArray;
            int retVal = seedArray[locINext] - seedArray[locINextp];

            if (retVal == int.MaxValue)
            {
                retVal--;
            }
            if (retVal < 0)
            {
                retVal += int.MaxValue;
            }

            seedArray[locINext] = retVal;
            _inext = locINext;
            _inextp = locINextp;

            return retVal;
        }

        internal double GetSampleForLargeRange()
        {
            // The distribution of the double returned by Sample is not good enough for a large range.
            // If we use Sample for a range [int.MinValue..int.MaxValue), we will end up getting even numbers only.
            int result = InternalSample();

            // We can't use addition here: the distribution will be bad if we do that.
            if (InternalSample() % 2 == 0) // decide the sign based on second sample
            {
                result = -result;
            }

            double d = result;
            d += int.MaxValue - 1; // get a number in range [0..2*int.MaxValue-1)
            d /= 2u * int.MaxValue - 1;
            return d;
        }
    }
}
