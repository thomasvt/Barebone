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
        void Set(string name, int value);
        int? Get(string name);
        int Inc(string name, int amount = 1);
    }
}
