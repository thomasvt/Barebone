using System.Numerics;
using Barebone.Game.Scene;
using Barebone.Geometry;

namespace Barebone.Game.Physics
{
    public interface IPhysics
    {
        void SetGravity(in Vector2 gravity);
        Vector2 GetGravity();


        /// <summary>
        /// Makes this Actor participate in the physics simulation of the game.
        /// </summary>
        BodyId CreateBody(in BodyType bodyType, in Vector2 position, in Vector2? velocity = null, in float angle = 0f);

        ShapeId AttachCircle(in BodyId bodyId, in float radius, in Vector2? center = null);
        ShapeId AttachPolygon(in BodyId bodyId, in Polygon8 polygon);
        ShapeId AttachCapsule(in BodyId bodyId, in Vector2 center1, in Vector2 center2, in float radius);
        void GetTransform(in BodyId bodyId, out Vector2 position, out float angle);
        void DestroyBody(BodyId bodyId);
    }
}
