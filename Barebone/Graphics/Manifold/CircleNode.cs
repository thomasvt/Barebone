using System.Numerics;
using Barebone.Graphics.Manifold.Core;

namespace Barebone.Graphics.Manifold
{
    public class CircleNode : Node
    {
        public Parameter<float> Radius { get; }
        public Parameter<float> MinAngle { get; }
        public Parameter<float> MaxAngle { get; }
        public Parameter<int> SegmentCount { get; }
        public Parameter<float> Rotation { get; }

        public CircleNode()
        {
            Radius = NewParameter(1f);
            MinAngle = NewParameter(0f);
            MaxAngle = NewParameter(MathF.Tau);
            SegmentCount = NewParameter(16);
            Rotation = NewParameter(0f);
        }

        public override void Cook(in Core.Geometry output)
        {
            if (Radius.Value <= 0f || SegmentCount.Value < 3)
                throw new Exception("Circle must have Radius > 0 and SegmentCount > 2");

            var segmentCount = SegmentCount.Value;
            var radius = Radius.Value;
            var angleStep = (MaxAngle.Value - MinAngle.Value) / segmentCount;

            // generate points:
            int? firstPointIdx = null;
            // note we do CW generation here, because we render polygons in CW order
            for (var angle = MaxAngle.Value + Rotation.Value; angle >= MinAngle.Value + Rotation.Value; angle -= angleStep)
            {
                var p = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
                var idx = output.PointSet.AddPoint(p);
                firstPointIdx ??= idx;
            }

            // generate segments:
            int? firstSegmentIdx = null;
            var pointOffset = firstPointIdx!.Value;
            for (var i = 0; i < segmentCount; i++)
            {
                var b = (i + 1) % segmentCount;
                var idx = output.SegmentSet.AddSegment(pointOffset + i, pointOffset + b);
                firstSegmentIdx ??= idx;
            }

            // generate path:
            var pathIdx = output.PathSet.AddPath(firstSegmentIdx!.Value, firstSegmentIdx!.Value + segmentCount-1);

            // generate shape:
            output.ShapeSet.AddShape(pathIdx);
        }
    }
}
