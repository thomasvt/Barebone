namespace BareBone
{
    /// <summary>
    /// Exponentially dampened metric.
    /// </summary>
    public class PerformanceCounter(string name, string unit = "", float dampingFactor = 0.01f)
    {
        public string Name { get; } = name;
        public string Unit { get; } = unit;

        public void AddSample(float value)
        {
            Average += (value - Average) * dampingFactor;
        }

        public float Average { get; private set; }

        public override string ToString()
        {
            return $"{Name}: {Average:0.00}{Unit}";
        }
    }
}
