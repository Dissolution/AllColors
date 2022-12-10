﻿namespace AllColors.Thrice;

public sealed class Shuffler
{
    private readonly Random _random;

    public Shuffler(int? seed)
    {
        if (seed is null)
        {
            _random =new Random();
        }
        else
        {
            _random = new Random(seed.Value);
        }
    }

    public void Shuffle<T>(ref T[] array)
    {
        for (var i = array.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    public T[] ShuffleCopy<T>(T[] array)
    {
        /* To initialize an array a of n elements to a randomly shuffled copy of source, both 0-based:
           for i from 0 to n − 1 do
           j ← random integer such that 0 ≤ j ≤ i
           if j ≠ i
           a[i] ← a[j]
           a[j] ← source[i]
        */

        int len = array.Length;
        var copy = new T[len];
        for (var i = 0; i < len; i++)
        {
            int j = _random.Next(i + 1);
            if (j != i)
            {
                copy[i] = copy[j];
            }
            copy[j] = array[i];
        }
        return copy;
    }
}