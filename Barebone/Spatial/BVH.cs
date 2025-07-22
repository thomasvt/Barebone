using BareBone;
using Barebone.Geometry;
using Barebone.Pools;

namespace Barebone.Spatial
{
    /// <summary>
    /// Bounding Volume Hierarchy. Spatial tree for items that are allowed to overlap.
    /// </summary>
    public class BVH<TItem> : IDisposable
    {
        public record struct ItemPair(TItem A, TItem B);
        public record struct RayCastHit(TItem Item, float Distance);

        private class Node : Poolable
        {
            public Aabb Aabb;
            public TItem? Item;
            public ulong ItemCategoryMask;
            public Node? Left, Right;
            public bool IsLeaf => Left == null;

            protected internal override void Construct()
            {
            }

            protected internal override void Destruct()
            {
            }
        }

        private Node? _root;

        public void Dispose()
        {
            Clear();
        }

        public void Add(in TItem item, in Aabb aabb, in ulong categoryMask = ulong.MaxValue)
        {
            var node = Pool.RentWithoutConstruct<Node>();
            node.Item = item;
            node.ItemCategoryMask = categoryMask;
            node.Aabb = aabb;
            node.Left = null;
            node.Right = null;
            _root = Add(_root, in node);
        }

        private Node Add(Node? parent, in Node node)
        {
            if (parent == null)
                return node;

            if (parent.IsLeaf)
            {
                var newParent = Pool.RentWithoutConstruct<Node>();
                newParent.Aabb = parent.Aabb.Union(node.Aabb);
                newParent.Item = default;
                newParent.ItemCategoryMask = parent.ItemCategoryMask | node.ItemCategoryMask;
                newParent.Left = parent;
                newParent.Right = node;
                return newParent;
            }

            var leftSize = parent.Left!.Aabb.Union(node.Aabb).GetArea();
            var rightSize = parent.Right!.Aabb.Union(node.Aabb).GetArea();
            if (leftSize <= rightSize)
            {
                parent.Left = Add(parent.Left, node);

            }
            else
            {
                parent.Right = Add(parent.Right, node);
            }
            parent.Aabb = parent.Left.Aabb.Union(parent.Right.Aabb);
            parent.ItemCategoryMask = parent.Left.ItemCategoryMask | parent.Right.ItemCategoryMask;

            return parent;
        }

        /// <summary>
        /// Clears 'resultBuffer' and then adds all items intersecting 'bounds' to it.
        /// </summary>
        ///
        public void Query(in Aabb bounds, BBList<TItem> resultBuffer)
        {
            resultBuffer.Clear();
            if (_root == null) return;
            var node = _root;
            if (!node.Aabb.Intersects(in bounds)) return;
            Query(in bounds, node, resultBuffer);
        }

        private static void Query(in Aabb bounds, Node node, BBList<TItem> resultBuffer)
        {
            if (node.IsLeaf)
            {
                resultBuffer.Add(node.Item!);
            }
            else
            {
                if (node.Left!.Aabb.Intersects(in bounds))
                    Query(in bounds, node.Left, resultBuffer);
                if (node.Right!.Aabb.Intersects(in bounds))
                    Query(in bounds, node.Right, resultBuffer);
            }
        }

        public void GetOverlappingPairs(in BBList<ItemPair> resultBuffer, in ulong categoryFilterMask)
        {
            resultBuffer.Clear();
            if (_root == null || _root.IsLeaf) return;

            GetOverlappingPairs(_root.Left!, _root.Right!, resultBuffer, in categoryFilterMask);
        }


        private static void GetOverlappingPairs(in Node a, in Node b, in BBList<ItemPair> resultBuffer, in ulong categoryFilterMask)
        {
            // Test all combinations within A and B themselves:
            if (!a.IsLeaf && (a.ItemCategoryMask & categoryFilterMask) != 0) GetOverlappingPairs(a.Left!, a.Right!, resultBuffer, in categoryFilterMask);
            if (!b.IsLeaf && (b.ItemCategoryMask & categoryFilterMask) != 0) GetOverlappingPairs(b.Left!, b.Right!, resultBuffer, in categoryFilterMask);

            // should we test A against B?
            if ((a.ItemCategoryMask & b.ItemCategoryMask & categoryFilterMask) == 0 
                || !a.Aabb.Intersects(b.Aabb)) return;

            if (a.IsLeaf && b.IsLeaf)
            {
                // Both are leaves, add the pair
                resultBuffer.Add(new(a.Item!, b.Item!));
            }
            else if (a.IsLeaf)
            {
                // test A against all children of B
                GetOverlappingPairs(a, b.Left!, resultBuffer, in categoryFilterMask);
                GetOverlappingPairs(a, b.Right!, resultBuffer, in categoryFilterMask);

            }
            else if (b.IsLeaf)
            {
                // test B against all children of A
                GetOverlappingPairs(a.Left!, b, resultBuffer, in categoryFilterMask);
                GetOverlappingPairs(a.Right!, b, resultBuffer, in categoryFilterMask);
            }
            else
            {
                // Recurse into both to compare all children of a with all children of b
                GetOverlappingPairs(a.Left!, b.Left!, resultBuffer, in categoryFilterMask);
                GetOverlappingPairs(a.Left!, b.Right!, resultBuffer, in categoryFilterMask);
                GetOverlappingPairs(a.Right!, b.Left!, resultBuffer, in categoryFilterMask);
                GetOverlappingPairs(a.Right!, b.Right!, resultBuffer, in categoryFilterMask);
            }
        }

        public delegate bool NarrowPhaseTest(Ray2 ray, in TItem item);

        /// <summary>
        /// Returns the closest item intersecting the ray. Expects Ray.Direction to be normalized.
        /// </summary>
        /// <param name="narrowTest">Narrow phase test invoked by broadphase when the ray hits the AABB of the item. Return true if ray hits the actual item.</param>
        public RayCastHit? RayCastClosest(in Ray2 ray, in float maxDistance, NarrowPhaseTest narrowTest, ulong categoryMask = ulong.MaxValue)
        {
            if (_root == null) return null;

            var todo = Pool.Rent<BBList<Node>>();
            todo.Add(_root!);

            RayCastHit? closest = null;

            while (todo.Count > 0)
            {
                var node = todo.Pop();
                if ((node.ItemCategoryMask & categoryMask) == 0) continue;

                if (node.IsLeaf)
                {
                    var hitDistance = Intersections.RayAabb(in ray, in node.Aabb);
                    var isRayHit = hitDistance.HasValue && hitDistance.Value <= maxDistance;

                    if (!isRayHit || !narrowTest(ray, node.Item!)) continue;

                    if (closest == null || closest.Value.Distance > hitDistance)
                        closest = new RayCastHit(node.Item!, hitDistance!.Value);
                }
                else
                {
                    var isRayHit = node.Aabb.Contains(ray.Origin); // if we already are sure from this Origin test, we can skip the expensive raycast.
                    if (!isRayHit)
                    {
                        var hitDistance = Intersections.RayAabb(in ray, in node.Aabb);
                        isRayHit = hitDistance.HasValue && hitDistance.Value <= maxDistance;
                    }

                    if (!isRayHit) continue;

                    todo.Add(node.Left!);
                    todo.Add(node.Right!);
                }
            }

            Pool.Return(todo);

            return closest;
        }

        public void Clear()
        {
            if (_root == null) return;
            Clear(_root!);
            _root = null;
        }

        public override string ToString()
        {
            var sb = new IndentedStringBuilder("  ");
            ToStringInternal(_root, sb);
            return sb.ToString();
        }

        private void ToStringInternal(Node? node, IndentedStringBuilder sb)
        {
            if (node == null) return;
            sb.Append($"{node.Aabb} ({node.ItemCategoryMask:b}): ");
            if (node.IsLeaf)
            {
                sb.AppendLine(node.Item!.ToString());
            }
            else
            {
                sb.AppendLine();
                sb.Indent();
                ToStringInternal(node.Left, sb);
                ToStringInternal(node.Right, sb);
                sb.Outdent();
            }
        }

        private static void Clear(Node node)
        {
            if (node.Left != null)
            {
                Clear(node.Left);
                node.Left = null;
                Clear(node.Right!);
                node.Right = null;
            }
            Pool.Return(node);
        }


    }
}


