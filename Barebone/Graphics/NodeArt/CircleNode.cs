using System.Numerics;
using Barebone.Graphics.NodeArt.Core;
using Path = Barebone.Graphics.NodeArt.Core.Path;

namespace Barebone.Graphics.NodeArt
{
    public class CircleNode : ArtNode
    {
        public NodeParameter<float> Radius { get; }
        public NodeParameter<float> MinAngle { get; }
        public NodeParameter<float> MaxAngle { get; }
        public NodeParameter<int> SegmentCount { get; }
        public NodeParameter<float> Rotation { get; }

        public CircleNode()
        {
            Radius = DefineParameter(nameof(Radius), 1f);
            MinAngle = DefineParameter(nameof(MinAngle), 0f);
            MaxAngle = DefineParameter(nameof(MaxAngle), MathF.Tau);
            SegmentCount = DefineParameter(nameof(SegmentCount), 16);
            Rotation = DefineParameter(nameof(Rotation), 0f);
        }

        protected override void CookInternal(in GeometrySet output)
        {
            if (Radius.Value <= 0f || SegmentCount.Value < 3)
                throw new Exception("Circle must have Radius > 0 and SegmentCount > 2");

            var segmentCount = SegmentCount.Value;
            var radius = Radius.Value;
            var angleStep = (MaxAngle.Value - MinAngle.Value) / segmentCount;

            // generate points:
            output.PointSet.SetSize(segmentCount);
            // note that we do CW generation here, because we render polygons in CW order
            var angle = MaxAngle.Value + Rotation.Value;
            for (var i = 0; i < segmentCount; i++)
            {
                angle -= angleStep;
                var p = new Point(i, new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius);
                output.PointSet.SetItem(i, p);
            }

            // generate segments:
            output.SegmentSet.SetSize(segmentCount);
            for (var i = 0; i < segmentCount; i++)
            {
                var b = (i + 1) % segmentCount;
                output.SegmentSet.SetItem(i, new Segment(i, SegmentType.Line, i, b, -1, -1));
            }

            // generate path:
            output.PathSet.SetSize(1);
            output.PathSet.SetItem(0, new Path(0, 0, segmentCount-1));

            // generate shape:
            output.ShapeSet.SetSize(1);
            output.ShapeSet.SetItem(0, new Shape(0, 0));
        }
    }
}
