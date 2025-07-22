namespace BareBone.Animations;

public abstract class Animation<T> : IStoryboardItem
{
    public void Update(in float previousUpdateTime, in float time)
    {
        var duration = EndTime - BeginTime;
        if (duration <= 0) throw new Exception("An animation should have a duration greater than zero.");

        var pct = MathF.Max(0, MathF.Min(1f, (time - BeginTime) / duration)); // coerce within [0,1]
            
        OnUpdate?.Invoke(GetValue(pct));
    }

    public void Reset()
    {
    }

    protected abstract T GetValue(float factor);

    public float EndTime { get; set; }
    public float BeginTime { get; set; }
    public Action<T>? OnUpdate { get; set; }
}