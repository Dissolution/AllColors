using FluentAssertions;

namespace AllColors.Tests;

public class ShufflerTests
{
    private const int Depth = 10;

    [Fact]
    public void ShuffleCopyWorks()
    {
        var allColors = ColorSpace.AllColors();
        var shuffler = new Shuffler(null);
        var copy = shuffler.ShuffleCopy<ARGB>(allColors);

        // Verify the copy still contains every possible color
        var set = new HashSet<ARGB>();
        foreach (var color in copy)
        {
            set.Add(color).Should().BeTrue();
        }

        // Verify it is different
        ARGBComparer.Instance.Equals(allColors, copy)
            .Should().BeFalse();
    }

    [Fact]
    public void ShufflerCanBeInconsistent()
    {
        var allColors = ColorSpace.AllColors();

        HashSet<ARGB[]> colorSequences = new(ARGBComparer.Instance);

        for (var i = 0; i < Depth; i++)
        {
            // Create a shuffler with no seed
            var shuffler = new Shuffler(null);

            var shuffled = shuffler.ShuffleCopy<ARGB>(allColors);

            // Verify it is different
            ARGBComparer.Instance
                .Equals(allColors, shuffled)
                .Should().BeFalse();

            // Verify we have not seen it
            colorSequences.Add(shuffled).Should().BeTrue();
        }
    }


    [Fact]
    public void ShufflerIsConsistent()
    {
        int[] seeds = new int[4]
        {
            0,
            147,
            int.MinValue,
            int.MaxValue,
        };

        var allColors = ColorSpace.AllColors();

        foreach (int seed in seeds)
        {
            HashSet<ARGB[]> colorSequences = new(ARGBComparer.Instance);

            for (var d = 0; d < Depth; d++)
            {
                // Create a shuffler with no seed
                var shuffler = new Shuffler(seed);

                var shuffled = shuffler.ShuffleCopy<ARGB>(allColors);

                // Verify it is different
                ARGBComparer.Instance
                    .Equals(allColors, shuffled)
                    .Should().BeFalse();

                // Verify we have seen it
                if (colorSequences.Count == 0)
                    colorSequences.Add(shuffled);
                else
                {
                    var added = colorSequences.Add(shuffled);
                    added.Should().BeFalse();
                }
            }
        }
    }
}