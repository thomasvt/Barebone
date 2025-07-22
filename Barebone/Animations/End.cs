namespace BareBone.Animations
{
    /// <summary>
    /// Marks the end of a Storyboard. Only necessary if you want the storyboard to take longer than the last functional item on it.
    /// </summary>
    public class End : IStoryboardItem
    {
        public End(float time)
        {
            Time = time;
        }

        float IStoryboardItem.EndTime => Time;
        float IStoryboardItem.BeginTime => Time;
        public bool IsFinished { get; private set; }

        public void Update(in float previousUpdateTime, in float time)
        {
            IsFinished = time >= Time;
        }

        public void Reset()
        {
            IsFinished = false;
        }

        public float Time { get; set; }
    }
}
