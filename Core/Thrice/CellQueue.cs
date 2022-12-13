using System.Buffers;
using System.Diagnostics;

namespace AllColors.Thrice;

public sealed class CellQueue
{
    private ImageCell?[] _cells;
    private int _endIndex;
    private int _count;

    public ImageCell?[] Cells
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _cells;
    }

    public int EndLength
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _endIndex;
    }

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _count;
    }

    public CellQueue(int capacity = 4096)
    {
        _cells = ArrayPool<ImageCell?>.Shared.Rent(capacity);
        _endIndex = 0;
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow()
    {
        var newArray = ArrayPool<ImageCell?>.Shared.Rent(_cells.Length * 2);
        int n = 0;
        ImageCell?[] cells = _cells;
        for (var i = 0; i < _endIndex; i++)
        {
            ImageCell? cell = cells[i];
            if (cell is null) continue;

            cell.ListIndex = n;
            newArray[n++] = cell;
        }

        ArrayPool<ImageCell?>.Shared.Return(cells);
        _cells = newArray;
        _endIndex = n;
        Debug.Assert(_count == _endIndex);
    }

    public void Compact()
    {
        if (_count == _endIndex) return;

//#if RUNTESTS
        double waste = ((double)_endIndex / _count);
        if (waste < 1.05d)
            return;

        Debug.Write($"ImageCellQueue Compact: {_endIndex} => ");
//#endif
        ImageCell?[] cells = _cells;

        int freeIndex = 0;

        for (var i = 0; i < _endIndex; i++)
        {
            ImageCell? cell = cells[i];
            if (cell is null) continue;

            if (cell.ListIndex != freeIndex)
            {
                cell.ListIndex = freeIndex;
                cells[freeIndex] = cell;
            }

            freeIndex++;
        }

        Debug.Assert(freeIndex == _count);
        Debug.WriteLine($"{freeIndex}");

        _endIndex = freeIndex;
    }
    

    public bool TryAdd(ImageCell cell)
    {
        if (cell.ListIndex != -1) return false;
        int i = _endIndex;
        if (i == _cells.Length) Grow();
        cell.ListIndex = i;
        _cells[i] = cell;
        _endIndex = i + 1;
        _count++;
        return true;
    }

    public bool TryRemove(ImageCell cell)
    {
        var index = cell.ListIndex;
        if (index == -1) return false;
        _cells[index] = null;
        cell.ListIndex = -1;
        _count--;
        return true;
    }
}