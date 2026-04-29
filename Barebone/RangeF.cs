namespace Barebone
{
    /// <summary>
    /// A 1D range with inclusive Min and exclusive Max value.
    /// </summary>
    public record struct RangeF(float Min, float Max)
    {
        public readonly bool Contains(float value)
        {
            return value >= Min && value < Max;
        }

        public float Middle => (Max + Min) * 0.5f;

        public override string ToString()
        {
            return $"{Min}:{Max}";
        }
    }
}
