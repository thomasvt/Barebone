using System.Numerics;

namespace Barebone.AI.Goap
{
    /// <summary>
    /// Describes the world to a GOAP agent as a set of up to 64 Yes/No flags.
    /// Each flag can represent an attribute or a bucket-membership.
    /// </summary>
    public struct WorldState
    {
        public WorldState(params ValueTuple<int, bool>[] flags)
        {
            foreach (var flag in flags)
            {
                Set(flag.Item1, flag.Item2);
            }
        }

        /// <summary>
        /// Defines which bits in Flags are in use.
        /// </summary>
        private ulong _inUse;

        public int FlagCount => BitOperations.PopCount(_inUse);
        /// <summary>
        /// Yes No flags representing various attributes or bucket-membership describing the world.
        /// </summary>
        public ulong Flags { get; private set; }

        public void Set(int idx, bool value)
        {
            if (idx is < 0 or >= 64)
                throw new IndexOutOfRangeException("WorldState flag index must be between 0 and 63.");

            // update Flags:
            Flags &= ~(1ul << idx); // keep all except the bit at idx
            if (value)
                Flags |= 1ul << idx; // set the bit at idx to 1

            // mark idx as used:
            _inUse |= 1ul << idx;
        }

        /// <summary>
        /// Checks if all flags that are InUse on `specification` have the same value as in this WorldState.
        /// </summary>
        public bool Satisfies(WorldState specification)
        {
            // We assume there are no bugs in Set() and don't filter specification's _flags by its _inUse mask.
            return specification.Flags == (Flags & specification._inUse) // all used flags must match
                   && (specification._inUse & ~_inUse) == 0; // all InUse flags in specification must also be InUse in this WorldState
        }

        /// <summary>
        /// Returns the number of flags 'specification' that are not satisfied by this WorldState. 
        /// </summary>
        public int GetUnsatisfiedFlagCount(WorldState specification)
        {
            var sameFlags = ~(Flags ^ specification.Flags);
            var satisfiedFlags = sameFlags 
                                     & specification._inUse // do they matter to the specification?
                                     & _inUse; // prevent flags that are not InUse in this WorldState from accidentally having the right value
            var satisfiedCount = BitOperations.PopCount(satisfiedFlags);
            return specification.FlagCount - satisfiedCount;
        }

        /// <summary>
        /// Copies the flag-values in `effects` to this WorldState.
        /// </summary>
        public void ApplyEffects(WorldState effects)
        {
            var maskEffects = effects._inUse;
            var maskRemainder = ~maskEffects;
            Flags = (maskRemainder & Flags) | (maskEffects & effects.Flags);
        }
    }
}
