namespace Barebone.Game.Scene
{
    public record struct ActorId(uint Value); // must be 32bit because used by Physics in a 32bit field.

    public abstract class Actor
    {
        public ActorId ActorId { get; internal set; }

        public virtual void OnSpawn(in IBBApi bb) { }

        public virtual void Update(in IBBApi bb) { }
        public virtual void OnDespawn(in IBBApi bb) { }
        public virtual void Draw(in IBBApi bb) { }

        public override string ToString()
        {
            return $"{GetType().Name} #{ActorId.Value}";
        }
    }
}
