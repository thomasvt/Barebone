using System.Numerics;
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
        BodyId CreateDynamicBody(in Vector2 position, in Vector2? velocity = null, in float angle = 0f, in bool lockRotation = false);
        BodyId CreateStaticBody(in Vector2? position = null, in float angle = 0f);
        /// <summary>
        /// Destroys the given body and its attached shapes.
        /// </summary>
        void DestroyBody(BodyId bodyId);

        ShapeId AttachCircle(in BodyId bodyId, in Vector2? center, in float radius, in float friction);
        ShapeId AttachPolygon(in BodyId bodyId, in Polygon8 polygon, in float friction);
        ShapeId AttachCapsule(in BodyId bodyId, in Vector2 center1, in Vector2 center2, in float radius, in float friction);

        void GetTransform(in BodyId bodyId, out Vector2 position, out float angle);
        Vector2 GetVelocity(in BodyId bodyId);
        void SetVelocity(in BodyId bodyId, in Vector2 velocity);


        /// <summary>
        /// Lets the physical simulation take a step forward.
        /// </summary>
        /// <param name="subStepCount">Substeps to simulate, this increases quality of the simulation but also the calculation cost.</param>
        void Step(float deltaT, int subStepCount);
    }
}
