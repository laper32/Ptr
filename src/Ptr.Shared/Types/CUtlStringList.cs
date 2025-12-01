// ReSharper disable ConvertToAutoPropertyWhenPossible
// ReSharper disable UnusedMember.Global

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sharp.Shared.Types.Tier;

namespace Ptr.Shared.Types;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct CUtlStringList(int growSize = 0, int initSize = 0) : IDisposable
{
    private int _size;
    private CUtlMemory<CUtlString> _memory = new(growSize, initSize);

    public void Dispose()
    {
        _memory.Dispose();
    }

    public ref CUtlString this[long index] => ref _memory[index];

    public IEnumerator<CUtlString> GetEnumerator()
    {
        for (var i = 0; i < _size; i++)
        {
            yield return this[i];
        }
    }

    private void GrowVector(int num)
    {
        if (_size + num > _memory.AllocationCount)
        {
            _memory.Grow(_size + num - _memory.AllocationCount);
        }

        _size += num;
    }

    public void Add(string item)
    {
        Add(new CUtlString(item));
    }

    public void Add(CUtlString item)
    {
        Add(ref item);
    }

    public void Add(ref readonly CUtlString item)
    {
        Insert(_size, in item);
    }

    private void Insert(int index, CUtlString item)
    {
        Insert(index, ref item);
    }

    private void Insert(int index, ref readonly CUtlString item)
    {
        if (index < 0 || index > _size)
        {
            throw new IndexOutOfRangeException();
        }

        GrowVector(1);
        ShiftElementsRight(index);
        _memory[index] = item;
    }

    private void ShiftElementsRight(int index, int num = 1)
    {
        if (_size == 0)
        {
            throw new Exception();
        }

        if (num == 0)
        {
            throw new Exception();
        }

        var numToMove = _size - index - num;

        if (numToMove > 0 && num > 0)
        {
            NativeMemory.Copy(Unsafe.AsPointer(ref _memory[index]),
                Unsafe.AsPointer(ref _memory[index + num]),
                (nuint)(numToMove * sizeof(CUtlString)));
        }
    }


    public int Count => _size;
}