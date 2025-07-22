using System.Buffers;

namespace Barebone.Architecture.Ecs;

internal static class ArrayExt
{
    /// <summary>
    /// Replaces 'array' with a larger one from ArrayPool, Copies 'itemsToCopy' items from the old to the new one, and returns the old one to ArrayPool. 
    /// </summary>
    internal static void GrowPoolArray<T>(ref T[] array, int newCapacity, int itemsToCopy)
    {
        var old = array;
        array = ArrayPool<T>.Shared.Rent(newCapacity);
        Array.Copy(old, array, itemsToCopy);
        ArrayPool<T>.Shared.Return(old);
    }
}