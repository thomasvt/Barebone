using System.Numerics;
using Barebone.Graphics.NodeArt.Core;

namespace Barebone.Graphics.NodeArt
{
    public class CircleNode : NaNode
    {
        public NaParameter<float> Radius { get; }
        public NaParameter<float> MinAngle { get; }
        public NaParameter<float> MaxAngle { get; }
        public NaParameter<int> SegmentCount { get; }
        public NaParameter<float> Rotation { get; }

        public CircleNode()
        {
            Radius = DefineParameter(1f);
            MinAngle = DefineParameter(0f);
            MaxAngle = DefineParameter(MathF.Tau);
            SegmentCount = DefineParameter(16);
            Rotation = DefineParameter(0f);
        }

        protected override void Cook(in Core.NaGeometry output)
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
