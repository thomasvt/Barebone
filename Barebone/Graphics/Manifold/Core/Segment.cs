namespace Barebone.Graphics.Manifold.Core
{
    public enum SegmentType
    {
        Line,
        QuadraticBezier,
        CubicBezier
    }

    public record struct Segment(SegmentType Type, int Point0, int Point1, int ControlPoint0, int ControlPoint1);
}
