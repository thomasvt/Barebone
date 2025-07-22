namespace Barebone;

/// <summary>
/// Time service holding current game time, frame's delta time etc.
/// </summary>
public class Clock
{
    public float Time { get; private set; }
    public float DeltaT { get; private set; }

    public long FrameNumber { get; private set; }

    public void Update(float time, float deltaT)
    {
        Time = time;
        DeltaT = deltaT;
        FrameNumber++;
    }

    /// <summary>
    /// Call this every frame to get a `true` only every so many frames, but with a varying offset based on 'seed'.
    /// This helps you distribute operations over all available frames:
    /// different seeds will make this method return true on a different frame, but with the requested interval.
    /// </summary>
    public bool IsMyTurn(long seed, long frameInterval)
    {
        return (seed % frameInterval) == (FrameNumber % frameInterval);
    }
}