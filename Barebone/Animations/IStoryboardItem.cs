namespace BareBone.Animations;

public interface IStoryboardItem
{
    float EndTime { get; }
    float BeginTime { get; }
    void Update(in float previousUpdateTime, in float time);
    void Reset();
}