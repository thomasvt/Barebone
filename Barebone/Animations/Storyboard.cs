using System.Collections;

namespace BareBone.Animations
{
    /// <summary>
    /// Like a dopesheet of animations and events on a timeline with which game state can be altered and animated.
    /// </summary>
    public class Storyboard : ICollection<IStoryboardItem>
    {
        private static Func<float> _getTimeFunc = null!;

        private readonly List<IStoryboardItem> _items;
        private float _startTime;
        private bool _isFinished;
        private float _previousTime;

        public Storyboard()
        {
#if DEBUG
            if (_getTimeFunc == null) throw new Exception($"Call {nameof(Storyboard)}.{nameof(SetTimeService)} before using Storyboards.");
#endif
            _isFinished = true;
            _items = new();
        }

        public IReadOnlyCollection<IStoryboardItem> Items => _items;

        public void Start()
        {
            foreach (var item in Items)
                item.Reset();

            _previousTime = float.MinValue;
            _isFinished = false;
            _startTime = _getTimeFunc();
            OnStart?.Invoke();
        }

        public void Stop()
        {
            _isFinished = true;
        }

        public void Update()
        {
            if (_isFinished)
                return;

            var time = _getTimeFunc() - _startTime;

            var allItemsFinished = true;
            foreach (var item in Items)
            {
                if (time >= item.BeginTime && _previousTime < item.EndTime) // we check previoustime, because we want a final Execute() when the time is stepping OVER the EndTime.
                    item.Update(_previousTime, time);

                if (item.EndTime > time)
                    allItemsFinished = false;
            }

            if (allItemsFinished)
            {
                _isFinished = true;
                OnFinished?.Invoke();
            }

            _previousTime = time;
        }

        /// <summary>
        /// Invoked when the last running animation inside the storyboard finishes. Not when you call Stop().
        /// </summary>
        public Action? OnFinished { get; set; }

        /// <summary>
        /// Invoked from Storyboard.Start()
        /// </summary>
        public Action? OnStart { get; set; }

        public bool IsFinished => _isFinished;

        public IEnumerator<IStoryboardItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(IStoryboardItem item)
        {
            _items.Add(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        bool ICollection<IStoryboardItem>.Contains(IStoryboardItem item)
        {
            throw new NotImplementedException();
        }

        void ICollection<IStoryboardItem>.CopyTo(IStoryboardItem[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(IStoryboardItem item)
        {
            return _items.Remove(item);
        }

        public int Count => _items.Count;

        public bool IsReadOnly => !_isFinished;

        public static void SetTimeService(Func<float> func)
        {
            _getTimeFunc = func;
        }
    }
}
