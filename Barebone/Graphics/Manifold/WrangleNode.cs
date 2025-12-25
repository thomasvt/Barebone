using Barebone.Graphics.Manifold.Core;

namespace Barebone.Graphics.Manifold
{
    public class PointWrangleNode : Node
    {
        public delegate void PointWrangleDelegate(ref Point point);

        public Parameter<PointWrangleDelegate?> Delegate { get; set; }
        public Parameter<Node?> Input { get; set; }

        public PointWrangleNode()
        {
            Delegate = NewParameter((PointWrangleDelegate?)null);
            Input = NewParameter<Node?>(null);
        }

        public override void Cook(in Core.Geometry output)
        {
            if (Input.Value == null) throw new Exception("Node has no Input.");

            var input = Input.Value.GetResult();
            var wrangle = Delegate.Value;

            input.CloneTo(output);

            if (wrangle == null)
                return;

            foreach (ref var p in output.PointSet.Points.AsSpan())
                wrangle(ref p);
        }
    }
}
