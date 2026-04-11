namespace Barebone.Game.Debugging
{
    public interface IDebug
    {
        void IncreaseGameSpeed();
        void DecreaseGameSpeed();
        void Clear();
        void WriteLine(object? msg = null);
        void Write(object msg);
        float GameSpeed { get; }
    }
}
