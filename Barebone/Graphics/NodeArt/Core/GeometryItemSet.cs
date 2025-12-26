using Barebone.Pools;

namespace Barebone.Graphics.NodeArt.Core;

public class GeometryItemSet<T> : Poolable where T : struct
{
    private BBList<T> _items = null!;
    public Span<T> Items => _items.AsSpan();

    private AttributeSet _attributes = null!;

    protected internal override void Construct()
    {
        _items = Pool.Rent<BBList<T>>();
        _attributes = Pool.Rent<AttributeSet>();
    }

    protected internal override void Destruct()
    {
        _items.Return();
        _attributes.Return();
    }

    public void SetItem(int idx, T item)
    {
        if (idx < 0 || idx > _items.Count)
            throw new ArgumentOutOfRangeException(nameof(idx));

        _items.InternalArray[idx] = item;
    }

    public void CloneInto(GeometryItemSet<T> dest)
    {
        dest._items.Clear();
        dest.SetSize(_items.Count);
        Items.CopyTo(dest.Items);
        _attributes.CloneInto(dest._attributes);
    }

    public void SetSize(int count)
    {
        _items.SetFixedCount(count, true);
        _attributes.SetItemCount(count);
    }

    public AttributeArray<TAttr> GetOrCreateAttributeArray<TAttr>(string name) where TAttr : struct
    {
        return _attributes.GetOrCreate<TAttr>(name, _items.Count);
    }

    public AttributeArray<TAttr>? GetAttributeArrayOrNull<TAttr>(string name) where TAttr : struct
    {
        if (_attributes.TryGet<TAttr>(name, out var arr))
            return arr;
        return null;
    }
}