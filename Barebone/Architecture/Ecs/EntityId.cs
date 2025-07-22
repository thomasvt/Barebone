namespace Barebone.Architecture.Ecs;

public record struct EntityId(long Value)
{
    public override string ToString()
    {
        return $"Entity:{Value}";
    }

    public static EntityId Null = new(0) ;
}
