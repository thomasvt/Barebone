using System.Buffers;
using System.Numerics;
using Barebone.Architecture.Ecs;
using Barebone.Geometry;
using Box2dNet.Interop;

namespace Barebone.Box2d.Ecs
{
    public struct PolygonShapeCompo
    {
        public b2ShapeId B2ShapeId;

        public Polygon8 Polygon;
        public float CornerRadius;
        public ulong CategoryMask;
        public ulong InteractionMask;
        public EntityId? RigidBodyId;

        public static void OnAdd(in EcsScene ecsScene, in EntityId id, ref PolygonShapeCompo s)
        {
            var b2Body = ecsScene.GetComponentRef<RigidBodyCompo>(s.RigidBodyId ?? id);
            var def = B2Api.b2DefaultShapeDef();
            def.filter.categoryBits = s.CategoryMask;
            def.filter.maskBits = s.InteractionMask;

            var arr = ArrayPool<Vector2>.Shared.Rent(s.Polygon.Count);
            s.Polygon.FillArray(arr);
            var hull = B2Api.b2ComputeHull(arr, s.Polygon.Count);
            var polygon = B2Api.b2MakePolygon(hull, s.CornerRadius);
            ArrayPool<Vector2>.Shared.Return(arr);

            s.B2ShapeId = B2Api.b2CreatePolygonShape(b2Body.B2BodyId, def, polygon);
        }

        public static void OnRemove(ref PolygonShapeCompo s)
        {
            B2Api.b2DestroyShape(s.B2ShapeId, true);
            s.B2ShapeId = default;
        }

        public static void Register(EcsScene scene)
        {
            scene.RegisterComponent<PolygonShapeCompo>();
            scene.SubscribeOnAdd((in EntityId id, ref PolygonShapeCompo s) => OnAdd(scene, id, ref s));
            scene.SubscribeOnRemove((in EntityId id, ref PolygonShapeCompo s) => OnRemove(ref s));
        }
    }
}
