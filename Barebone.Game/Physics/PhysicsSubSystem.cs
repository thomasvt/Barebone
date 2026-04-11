using System.Numerics;
using Barebone.Game.Scene;
using Barebone.Geometry;
using Barebone.Pools;
using Box2dNet.Interop;

namespace Barebone.Game.Physics
{
    public enum BodyType
    {
        /// <summary>
        /// Doesn't move, but dynamic bodies collide with it.
        /// </summary>
        Static,
        /// <summary>
        /// This is for bodies animated from code. They don't react to collisions. Only dynamic bodies collide with these.
        /// </summary>
        Kinematic,
        /// <summary>
        /// This body moves according to forces you applied to it from code, or from colliding with bodies of all other body types.
        /// </summary>
        Dynamic
    }

    public record struct ShapeId(uint Value);

    internal class PhysicsSubSystem : IPhysics, IDisposable
    {
        private uint _nextShapeId;
        private b2WorldId _b2WorldId;
        private readonly Dictionary<ActorId, b2BodyId> _bodyIdsByActorId = new();

        public PhysicsSubSystem()
        {
            var def = B2Api.b2DefaultWorldDef();
            def.gravity = Vector2.Zero;
            _b2WorldId = B2Api.b2CreateWorld(def);
        }

        public void Update(double deltaT, int subStepCount)
        {
            B2Api.b2World_Step(_b2WorldId, (float)deltaT, subStepCount);
        }

        public Vector2 GetGravity()
        {
            return B2Api.b2World_GetGravity(_b2WorldId);
        }

        public void SetGravity(in Vector2 gravity)
        {
            B2Api.b2World_SetGravity(_b2WorldId, gravity);
        }

        public void MakePhysical(in Actor actor, in BodyType bodyType, in Vector2 position, in Vector2? velocity = null, in float angle = 0f)
        {
            if (_bodyIdsByActorId.ContainsKey(actor.ActorId))
                throw new Exception("That actor already is physical.");

            var def = B2Api.b2DefaultBodyDef();
            def.type = bodyType switch
            {
                BodyType.Dynamic => b2BodyType.b2_dynamicBody,
                BodyType.Kinematic => b2BodyType.b2_kinematicBody,
                BodyType.Static => b2BodyType.b2_staticBody,
                _ => throw new ArgumentOutOfRangeException(nameof(bodyType), bodyType, null)
            };
            def.position = position;
            def.linearVelocity = velocity ?? Vector2.Zero;
            def.rotation = b2Rot.FromAngle(angle);
            def.userData = (nint)actor.ActorId.Value;

            var bodyId = B2Api.b2CreateBody(_b2WorldId, def);
            _bodyIdsByActorId.Add(actor.ActorId, bodyId);
        }

        public ShapeId AttachCircle(in Actor actor, in float radius, in Vector2? center = null)
        {
            if (!_bodyIdsByActorId.TryGetValue(actor.ActorId, out var b2BodyId))
                throw new InvalidOperationException($"Actor {actor} is not physical. Call MakePhysical first.");

            var shapeId = new ShapeId(++_nextShapeId);

            var def = B2Api.b2DefaultShapeDef();
            def.userData = (nint)shapeId.Value;

            B2Api.b2CreateCircleShape(b2BodyId, def, new b2Circle(center ?? Vector2.Zero, radius));

            return shapeId;
        }

        public ShapeId AttachPolygon(in Actor actor, in Polygon8 polygon)
        {
            if (!_bodyIdsByActorId.TryGetValue(actor.ActorId, out var b2BodyId))
                throw new InvalidOperationException($"Actor {actor} is not physical. Call MakePhysical first.");

            var shapeId = new ShapeId(++_nextShapeId);

            var def = B2Api.b2DefaultShapeDef();
            def.userData = (nint)shapeId.Value;

            var points = Pool.RentArray<Vector2>(polygon.Count);
            polygon.FillArray(points);
            var b2Polygon = B2Api.b2MakePolygon(B2Api.b2ComputeHull(points, polygon.Count), 0f);
            Pool.ReturnArray(points);

            B2Api.b2CreatePolygonShape(b2BodyId, def, b2Polygon);

            return shapeId;
        }

        public ShapeId AttachCapsule(in Actor actor, in Vector2 center1, in Vector2 center2, in float radius)
        {
            if (!_bodyIdsByActorId.TryGetValue(actor.ActorId, out var b2BodyId))
                throw new InvalidOperationException($"Actor {actor} is not physical. Call MakePhysical first.");

            var shapeId = new ShapeId(++_nextShapeId);

            var def = B2Api.b2DefaultShapeDef();
            def.userData = (nint)shapeId.Value;

            B2Api.b2CreateCapsuleShape(b2BodyId, def, new b2Capsule(center1, center2, radius));

            return shapeId;
        }

        public void GetTransform(in Actor actor, out Vector2 position, out float angle)
        {
            if (!_bodyIdsByActorId.TryGetValue(actor.ActorId, out var b2BodyId))
                throw new InvalidOperationException($"Actor {actor} is not physical. Call MakePhysical first.");

            var transform = B2Api.b2Body_GetTransform(b2BodyId);
            position = transform.p;
            angle = transform.q.GetAngle();
        }

        public void Clear()
        {
            B2Api.b2DestroyWorld(_b2WorldId);
            _b2WorldId = B2Api.b2CreateWorld(B2Api.b2DefaultWorldDef());
        }

        public void Dispose()
        {
            B2Api.b2DestroyWorld(_b2WorldId);
        }

        public void DestroyIfExists(ActorId actorId)
        {
            if (_bodyIdsByActorId.TryGetValue(actorId, out var b2BodyId))
                B2Api.b2DestroyBody(b2BodyId);
        }
    }
}
