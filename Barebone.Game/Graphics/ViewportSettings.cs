using Barebone.Game.Core;
using Barebone.Geometry;

namespace Barebone.Game.Graphics
{
    public record struct ViewportSettings(in Vector2I Size, in LogicalScaleMode ScaleMode);
}
