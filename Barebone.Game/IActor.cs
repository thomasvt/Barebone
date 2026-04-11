namespace Barebone.Game
{
    public interface IActor
    {
        void OnSpawn(in IBBApi bb);
        void Update(in IBBApi bb);
        void OnDespawn(in IBBApi bb);
        void Draw(in IBBApi bb);
    }
}
