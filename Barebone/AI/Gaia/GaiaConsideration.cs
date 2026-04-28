
namespace Barebone.AI.Gaia
{
    public record struct Weights(float Addend, float Multiplier)
    {
        /// <summary>
        /// Prevents the option this Consideration is attached to from being picked.
        /// </summary>
        public static Weights Veto = new(0, 0);
    }

    public abstract class GaiaConsideration
    {
        internal void Calculate()
        {
            ConsiderationWeights = CalculateWeights();
        }

        protected abstract Weights CalculateWeights();

        public Weights ConsiderationWeights { get; private set; } = default;
    }

    public class Tuning(float addend, float multiplier) : GaiaConsideration
    {
        public Tuning(float multiplier, int rank) : this(1, multiplier)
        {}

        protected override Weights CalculateWeights()
        {
            return new(addend, multiplier);
        }
    }

    /// <summary>
    /// Debug utility. Always veto's the option by setting Multiplier to 0. 
    /// </summary>
    public class Veto() : GaiaConsideration
    {
        protected override Weights CalculateWeights()
        {
            return new(0, 0);
        }
    }
}
