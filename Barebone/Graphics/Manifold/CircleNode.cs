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
            var radius = Radius.Value;
            var angleStep = (MaxAngle.Value - MinAngle.Value) / SegmentCount.Value;

            for (var angle = MinAngle.Value + Rotation.Value; angle <= MaxAngle.Value + Rotation.Value; angle += angleStep)
            {
                var p = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
                output.Points.Points.Add(new Point(p));
            }
        }
    }
}
