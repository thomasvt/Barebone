namespace Barebone.Graphics.Manifold.Core
{
    /// <summary>
    /// We use the convention that Paths must be ordered sequences of segments, so we don't need to store wasteful arrays of segment indices.
    /// </summary>
    public record struct Path(int Idx, int FirstSegmentIdx, int LastSegmentIdx);
}
