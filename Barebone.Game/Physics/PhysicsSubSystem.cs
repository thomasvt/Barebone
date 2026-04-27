using System.Numerics;
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
    public record struct BodyId(uint Value);

    internal class PhysicsSubSystem : IPhysics, IDisposable
    {
        private uint _nextShapeId;
        private uint _nextBodyId;
        private b2WorldId _b2WorldId;
        private readonly Dictionary<BodyId, b2BodyId> _b2BodyIdsByBodyId = new();

        public PhysicsSubSystem()
        {
            var def = B2Api.b2DefaultWorldDef();
            def.gravity = Vector2.Zero;
            _b2WorldId = B2Api.b2CreateWorld(def);
        }

        public Vector2 GetGravity()
        {
            return B2Api.b2World_GetGravity(_b2WorldId);
        }
        
        public void SetGravity(in Vector2 gravity)
        {
            B2Api.b2World_SetGravity(_b2WorldId, gravity);
        }

        private BodyId CreateBody(in BodyType bodyType, in Vector2 position, in Vector2? velocity, in float angle, in bool lockRotation)
        {
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
            def.motionLocks.angularZ = lockRotation;
            def.rotation = b2Rot.FromAngle(angle);

            var bodyId = new BodyId(++_nextBodyId);
            var b2BodyId = B2Api.b2CreateBody(_b2WorldId, def);
            _b2BodyIdsByBodyId.Add(bodyId, b2BodyId);

            return bodyId;
        }

        public BodyId CreateDynamicBody(in Vector2 position, in Vector2? velocity, in float angle, in bool lockRotation)
        {
            return CreateBody(BodyType.Dynamic, position, velocity, angle, lockRotation);
        }

        public BodyId CreateStaticBody(in Vector2? position = null, in float angle = 0)
        {
            return CreateBody(BodyType.Static, position ?? Vector2.Zero, Vector2.Zero, angle, false);
        }

        public ShapeId AttachCircle(in BodyId bodyId, in Vector2? center, in float radius, in float friction)
        {
            var b2BodyId = GetB2BodyIdOrThrow(bodyId);

            var shapeId = new ShapeId(++_nextShapeId);

            var def = B2Api.b2DefaultShapeDef();
            def.material.friction = friction;
            def.userData = (nint)shapeId.Value;

            B2Api.b2CreateCircleShape(b2BodyId, def, new b2Circle(center ?? Vector2.Zero, radius));

            return shapeId;
        }

        public ShapeId AttachPolygon(in BodyId bodyId, in Polygon8 polygon, in float friction)
        {
            var b2BodyId = GetB2BodyIdOrThrow(bodyId);

            var shapeId = new ShapeId(++_nextShapeId);

            var def = B2Api.b2DefaultShapeDef();
            def.material.friction = friction;
            def.userData = (nint)shapeId.Value;

            var points = Pool.RentArray<Vector2>(polygon.Count);
            polygon.FillArray(points);
            var b2Polygon = B2Api.b2MakePolygon(B2Api.b2ComputeHull(points, polygon.Count), 0f);
            Pool.ReturnArray(points);

            B2Api.b2CreatePolygonShape(b2BodyId, def, b2Polygon);

            return shapeId;
        }

        public ShapeId AttachCapsule(in BodyId bodyId, in Vector2 center1, in Vector2 center2, in float radius, in float friction)
        {
            var b2BodyId = GetB2BodyIdOrThrow(bodyId);

            var shapeId = new ShapeId(++_nextShapeId);

            var def = B2Api.b2DefaultShapeDef();
            def.material.friction = friction;
            def.userData = (nint)shapeId.Value;

            B2Api.b2CreateCapsuleShape(b2BodyId, def, new b2Capsule(center1, center2, radius));

            return shapeId;
        }

        public void GetTransform(in BodyId bodyId, out Vector2 position, out float angle)
        {
            var b2BodyId = GetB2BodyIdOrThrow(bodyId);

            var transform = B2Api.b2Body_GetTransform(b2BodyId);
            position = transform.p;
            angle = transform.q.GetAngle();
        }

        public Vector2 GetVelocity(in BodyId bodyId)
        {
            var b2BodyId = GetB2BodyIdOrThrow(bodyId);
            return B2Api.b2Body_GetLinearVelocity(b2BodyId);
        }

        public void SetVelocity(in BodyId bodyId, in Vector2 velocity)
        {
            var b2BodyId = GetB2BodyIdOrThrow(bodyId);

            B2Api.b2Body_SetLinearVelocity(b2BodyId, velocity);
        }

        public void Clear()
        {
            B2Api.b2DestroyWorld(_b2WorldId);
            _b2WorldId = B2Api.b2CreateWorld(B2Api.b2DefaultWorldDef());
        }

        public void DestroyBody(BodyId bodyId)
        {
            if (_b2BodyIdsByBodyId.TryGetValue(bodyId, out var b2BodyId))
            {
                var b2ShapeIds = Pool.RentArray<b2ShapeId>(B2Api.b2Body_GetShapeCount(b2BodyId));
                var count = B2Api.b2Body_GetShapes(b2BodyId, b2ShapeIds, b2ShapeIds.Length);
                foreach (var b2ShapeId in b2ShapeIds.AsSpan(0, count))
                {
                    // we will track shape mapping data soon, so we will have to clean it up too. We can do that here.
                }
                B2Api.b2DestroyBody(b2BodyId); // also destroys attached b2Shapes.
            }
        }

        public void Step(float deltaT, int subStepCount)
        {
            B2Api.b2World_Step(_b2WorldId, deltaT, subStepCount);
        }

        public void Dispose()
        {
            B2Api.b2DestroyWorld(_b2WorldId);
        }

        private b2BodyId GetB2BodyIdOrThrow(BodyId bodyId)
        {
            if (!_b2BodyIdsByBodyId.TryGetValue(bodyId, out var b2BodyId))
                throw new InvalidOperationException($"Unknown BodyId: {bodyId}.");
            return b2BodyId;
        }
    }
}
