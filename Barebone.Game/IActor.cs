namespace Barebone.Game
{
    public interface IActor
    {
        void Update(in IBBApi bb);
        void Draw(in IBBApi bb);
    }
}
