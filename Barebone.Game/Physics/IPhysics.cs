using System.Numerics;
using Barebone.Game.Scene;
using Barebone.Geometry;

namespace Barebone.Game.Physics
{
    public interface IPhysics
    {
        /// <summary>
        /// Makes this Actor participate in the physics simulation of the game.
        /// </summary>
        void MakePhysical(in Actor actor, in BodyType bodyType, in Vector2 position, in Vector2? velocity = null, in float angle = 0f);

        ShapeId AttachCircle(in Actor actor, in float radius, in Vector2? center = null);
        ShapeId AttachPolygon(in Actor actor, in Polygon8 polygon);
        ShapeId AttachCapsule(in Actor actor, in Vector2 center1, in Vector2 center2, in float radius);
        void GetTransform(in Actor actor, out Vector2 position, out float angle);
        Vector2 GetGravity();
        void SetGravity(in Vector2 gravity);
    }
}
