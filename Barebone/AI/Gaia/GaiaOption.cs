namespace Barebone.AI.Gaia
{
    public class GaiaOption
    {
        public readonly GaiaConsideration[] Considerations;
        public readonly GaiaAction Action;

        public GaiaOption(int rank, GaiaConsideration[] considerations, GaiaAction action)
        : this(rank, action.GetType().Name, considerations, action)
        {
        }

        public GaiaOption(int rank, string name, GaiaConsideration[] considerations, GaiaAction action)
        {
            if (considerations.Length == 0) throw new Exception("An AIOption must have at least 1 AIConsideration.");
            Considerations = considerations;
            Action = action;
            Rank = rank;
            Name = name;
        }

        internal void Calculate()
        {
            var isFirst = true;
            var addendsSum = 0f;
            var multiplierProduct = 0f;
            foreach (var consideration in Considerations)
            {
                consideration.CalculateWeight();

                // keep track of:
                // * max(rank)
                // * sum of all addends: ∑(addends)
                // * product of all multipliers: Π(multipliers)
                if (isFirst)
                {
                    addendsSum = consideration.Weights.Addend;
                    multiplierProduct = consideration.Weights.Multiplier;
                    isFirst = false;
                }
                else
                {
                    addendsSum += consideration.Weights.Addend;
                    multiplierProduct *= consideration.Weights.Multiplier;
                }
            }

            var combinedWeight = addendsSum * multiplierProduct;

            Weight = combinedWeight;
        }

        public int Rank { get; }
        public string Name { get; }

        /// <summary>
        /// [0..1]. Utility of this option. Higher weight = higher prio.
        /// </summary>
        internal float Weight { get; private set; }
    }
}
