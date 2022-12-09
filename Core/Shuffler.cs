/*
-- To shuffle an array a of n elements (indices 0..n-1):
for i from n−1 downto 1 do
     j ← random integer such that 0 ≤ j ≤ i
     exchange a[j] and a[i]
     */
     
namespace AllColors;

public static class Shuffler
{
    public static void Shuffle<T>(Random random, Span<T> span)
    {
        for (var i = span.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (span[i], span[j]) = (span[j], span[i]);
        }
    }
}