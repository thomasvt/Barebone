using Barebone.Graphics.Manifold.Core;

namespace Barebone.Graphics.Manifold
{
    public class PointWrangleNode : Node
    {
        public delegate Point PointWrangleFunction(ref readonly Point point);

        public Parameter<PointWrangleFunction?> Function { get; set; }
        public Parameter<Node?> Input { get; set; }

        public PointWrangleNode()
        {
            Function = NewParameter((PointWrangleFunction?)null);
            Input = NewParameter<Node?>(null);
        }

        public override void Cook(in Core.Geometry output)
        {
            if (Input.Value == null) throw new Exception("Node has no Input.");

            var input = Input.Value.GetResult();
            var func = Function.Value;

            if (func == null)
            {
                input.CopyTo(output);
                return;
            }

            foreach (ref readonly var p in input.Points.Points.AsSpan())
            {
                output.Points.Points.Add(func(in p));
            }
        }
    }
}
