namespace BareBone.Animations
{
    /// <summary>
    /// Single point on a <see cref="Storyboard"/> timeline when some logic is invoked.
    /// </summary>
    public class Event(float triggerTime, Action action) : IStoryboardItem
    {
        float IStoryboardItem.EndTime => TriggerTime;
        float IStoryboardItem.BeginTime => TriggerTime;
        public bool IsFinished { get; private set; }

        public void Update(in float previousUpdateTime, in float time)
        {
            if (!IsFinished && previousUpdateTime < TriggerTime && time >= TriggerTime)
            {
                IsFinished = true; // prevent triggering more than once.
                Action?.Invoke();
            }
        }

        public void Reset()
        {
            IsFinished = false;
        }

        /// <summary>
        /// When should this event trigger?
        /// </summary>
        public float TriggerTime { get; set; } = triggerTime;

        /// <summary>
        /// What to do when it triggers.
        /// </summary>
        public Action? Action { get; set; } = action;
    }
}
