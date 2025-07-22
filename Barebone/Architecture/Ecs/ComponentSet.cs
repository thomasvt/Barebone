using System.Buffers;
using System.Runtime.CompilerServices;

namespace Barebone.Architecture.Ecs;

internal interface IComponentSet
{
    abstract void Clear();
    void GrowCapacity(int newCapacity, int itemsToCopy);
    void Copy(int sourceIdx, IComponentSet destinationComponentSet, int destIdx);
    void Copy(int sourceIdx, int destIdx);
    void Dispose();
    void ClearItem(int idx);
}

internal class ComponentSet<TCompo> : IComponentSet
{
    private TCompo[] _items = ArrayPool<TCompo>.Shared.Rent(EntitySet.InitialCapacity);

    public void GrowCapacity(int newCapacity, int itemsToCopy)
    {
        ArrayExt.GrowPoolArray(ref _items, newCapacity, itemsToCopy);
    }

    public void Copy(int sourceIdx, IComponentSet destinationComponentSet, int destIdx)
    {
        var destination = (ComponentSet<TCompo>)destinationComponentSet;
        destination.Set(destIdx, _items[sourceIdx]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Copy(int sourceIdx, int destIdx)
    {
        _items[destIdx] = _items[sourceIdx];
    }

    public void Dispose()
    {
        ArrayPool<TCompo>.Shared.Return(_items);
    }

    public void ClearItem(int idx)
    {
        _items[idx] = default!;
    }

    public void Clear()
    {
        Array.Clear(_items);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int idx, in TCompo value)
    {
        _items[idx] = value;
    }

    /// <summary>
    /// Gets a ref to the component at given index.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TCompo Get(int idx)
    {
        return ref _items[idx];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<TCompo> AsSpan(int count) => _items.AsSpan(0, count);
}