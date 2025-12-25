namespace Barebone.Graphics.NodeArt.Core
{
    public enum SegmentType
    {
        Line,
        QuadraticBezier,
        CubicBezier
    }

    public record struct Segment(int Idx, SegmentType Type, int PointIdx0, int PointIdx1, int ControlPointIdx0, int ControlPointIdx1);
}
