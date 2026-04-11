namespace Barebone.Game.Scene
{
    public interface IScene
    {
        void Add(Actor actor);
        void Remove(Actor actor);
        void Clear();
    }
}
