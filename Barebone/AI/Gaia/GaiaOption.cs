namespace Barebone.AI.Gaia
{
    public class GaiaOption
    {
        private readonly GaiaConsideration[] _considerations;
        public readonly GaiaAction Action;

        public GaiaOption(int rank, string name, GaiaConsideration[] considerations, GaiaAction action)
        {
            if (considerations.Length == 0) throw new Exception("An AIOption must have at least 1 AIConsideration.");
            _considerations = considerations;
            Action = action;
            Rank = rank;
            Name = name;
        }

        internal void Calculate()
        {
            var isFirst = true;
            var addendsSum = 0f;
            var multiplierProduct = 0f;
            foreach (var consideration in _considerations)
            {
                consideration.Calculate();

                // keep track of:
                // * max(rank)
                // * sum of all addends: ∑(addends)
                // * product of all multipliers: Π(multipliers)
                if (isFirst)
                {
                    addendsSum = consideration.ConsiderationWeights.Addend;
                    multiplierProduct = consideration.ConsiderationWeights.Multiplier;
                    isFirst = false;
                }
                else
                {
                    addendsSum += consideration.ConsiderationWeights.Addend;
                    multiplierProduct *= consideration.ConsiderationWeights.Multiplier;
                }
            }

            var combinedWeight = addendsSum * multiplierProduct;

            Score = combinedWeight;
        }

        public int Rank { get; }
        public string Name { get; }

        internal float Score { get; private set; }
    }
}
