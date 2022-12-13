using System.Diagnostics;

namespace AllColors;

public interface IChildItem
{
    int ListIndex { get; internal set; }
}

public sealed class ParentList<TChild>
    where TChild : class, IChildItem
{
    private TChild?[] _children;
    private int _endIndex;
    private int _count;

    public TChild?[] Children => _children;
    public int EndIndex => _endIndex;
    public int Count => _count;

    public ParentList(int capacity = 8)
    {
        _children = new TChild?[capacity];
        _endIndex = 0;
        _count = 0;
    }

    public bool Contains(TChild child)
    {
        return child.ListIndex != -1;
    }

    public bool TryAdd(TChild child)
    {
        if (child.ListIndex != -1) return false;
        var freeIndex = _endIndex;
        if (freeIndex >= _children.Length)
        {
            Array.Resize<TChild?>(ref _children, freeIndex * 4);
        }

        // Associate
        child.ListIndex = freeIndex;
        _children[freeIndex] = child;
        _endIndex = freeIndex + 1;
        _count++;
        return true;
    }

    public bool TryRemove(TChild child)
    {
        if (child.ListIndex == -1) return false;
        _children[child.ListIndex] = null;
        child.ListIndex = -1;
        _count--;
        return true;
    }

    public void Compact()
    {
        if (_count == _endIndex) return;

        double waste = ((double)_endIndex / _count);
        if (waste < 1.05d)
            return;

        
        TChild?[] children = _children;

        int freeIndex = 0;

        for (var i = 0; i < _endIndex; i++)
        {
            TChild? child = children[i];
            if (child is null) continue;

            if (child.ListIndex != freeIndex)
            {
                child.ListIndex = freeIndex;
                children[freeIndex] = child;
            }

            freeIndex++;
        }

        Debug.Assert(freeIndex == _count);
        Debug.Write($"ParentList Compact: {_endIndex} => {freeIndex}");

        _endIndex = freeIndex;
    }
}