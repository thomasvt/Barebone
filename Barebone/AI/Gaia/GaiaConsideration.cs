namespace Barebone.AI.Gaia
{
    public record struct AIConsiderationWeights(float Addend, float Multiplier);

    public abstract class GaiaConsideration
    {
        internal void Calculate()
        {
            ConsiderationWeights = CalculateWeights();
        }

        protected abstract AIConsiderationWeights CalculateWeights();

        public AIConsiderationWeights ConsiderationWeights { get; private set; } = default;
    }

    public class Tuning(float addend, float multiplier) : GaiaConsideration
    {
        public Tuning(float multiplier, int rank) : this(1, multiplier)
        {}

        protected override AIConsiderationWeights CalculateWeights()
        {
            return new(addend, multiplier);
        }
    }

    /// <summary>
    /// Debug utility. Always veto's the option by setting Multiplier to 0. 
    /// </summary>
    public class Veto() : GaiaConsideration
    {
        protected override AIConsiderationWeights CalculateWeights()
        {
            return new(0, 0);
        }
    }
}
