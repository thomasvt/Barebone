namespace Barebone.Game.Input
{
    internal class ButtonStateTracker<TButton> where TButton: struct
    {
        private readonly HashSet<TButton> _isPressed = new();
        private readonly HashSet<TButton> _justPressed = new();
        private readonly HashSet<TButton> _justReleased = new();

        public void EndFrame()
        {
            _justPressed.Clear();
            _justReleased.Clear();
        }

        public void Down(TButton button)
        {
            if (_isPressed.Add(button)) // don't count as a JustPressed if it's already pressed (this ignores key-repeats done by OS)
                _justPressed.Add(button);
        }

        public void Up(TButton button)
        {
            if (_isPressed.Remove(button)) // don't count as a JustPressed if it's already pressed (this ignores key-repeats done by OS)
                _justReleased.Add(button);
        }

        public bool JustPressed(TButton button)
        {
            return _justPressed.Contains(button);
        }

        public bool JustReleased(TButton button)
        {
            return _justReleased.Contains(button);
        }

        public bool IsPressed(TButton button)
        {
            return _isPressed.Contains(button);
        }
    }
}
