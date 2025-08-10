using System.Numerics;
using Barebone.Architecture.Ecs;
using Box2dNet.Interop;

namespace Barebone.Box2d.Ecs
{
    public struct CircleShapeCompo
    {
        public b2ShapeId B2ShapeId;

        public ulong CategoryMask;
        public ulong InteractionMask;
        public EntityId? RigidBodyId;

        public Vector2 CircleCenter;
        public float CircleRadius;
        public bool IsSensor;
        public float Friction;
        public float Restitution;

        public static void OnAdd(in EcsScene ecsScene, in EntityId id, ref CircleShapeCompo s)
        {
            var b2Body = ecsScene.GetComponentRef<RigidBodyCompo>(s.RigidBodyId ?? id);
            var def = B2Api.b2DefaultShapeDef();
            def.filter.categoryBits = s.CategoryMask;
            def.filter.maskBits = s.InteractionMask;
            def.isSensor = s.IsSensor;
            def.enableSensorEvents = true;
            def.material.friction = s.Friction;
            def.material.restitution = s.Restitution;

            s.B2ShapeId = B2Api.b2CreateCircleShape(b2Body.B2BodyId, def, new b2Circle(s.CircleCenter, s.CircleRadius));
        }

        public static void OnRemove(ref CircleShapeCompo s)
        {
            B2Api.b2DestroyShape(s.B2ShapeId, true);
            s.B2ShapeId = default;
        }

        public static void Register(EcsScene scene)
        {
            scene.RegisterComponent<CircleShapeCompo>();
            scene.SubscribeOnAdd((in EntityId id, ref CircleShapeCompo s) => OnAdd(scene, id, ref s));
            scene.SubscribeOnRemove((in EntityId id, ref CircleShapeCompo s) => OnRemove(ref s));
        }
    }
}
