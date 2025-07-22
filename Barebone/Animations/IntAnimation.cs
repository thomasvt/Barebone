namespace BareBone.Animations
{
    public class IntAnimation : Animation<int>
    {
        public int From { get; set; }

        /// <summary>
        /// Inclusive
        /// </summary>
        public int To { get; set; }

        protected override int GetValue(float factor)
        {
            if (factor == 1f) return To;
            return (int)(From * (1 - factor) + To * factor);
        }
    }
}
