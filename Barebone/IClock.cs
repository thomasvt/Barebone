namespace Barebone
{
    public interface IClock
    {
        float Now { get; }
        float DeltaT { get; }
        long FrameNumber { get; }
        void Update(float time, float deltaT);

        /// <summary>
        /// Call this every frame to get a `true` only every so many frames, but with a varying offset based on 'seed'.
        /// This helps you distribute operations over all available frames:
        /// different seeds will make this method return true on a different frame, but with the requested interval.
        /// </summary>
        bool IsMyTurn(long seed, long frameInterval);
    }
}
