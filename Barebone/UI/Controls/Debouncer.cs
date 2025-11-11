namespace Barebone.UI.Controls
{
    public class Debouncer(TimeSpan delay) : IDisposable
    {
        private Timer? _timer;

        public void Debounce(Action action)
        {
            // Reset the timer each time this method is called.
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer?.Dispose();

            _timer = new Timer(_ => action(), null, delay, Timeout.InfiniteTimeSpan);
        }

        public void Dispose() => _timer?.Dispose();
    }
}
