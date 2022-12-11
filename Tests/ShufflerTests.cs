using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace AllColors.Tests;

public class ShufflerTests
{
    private const int Depth = 10;

    [Fact]
    public void ShuffleCopyWorks()
    {
        var allColors = ARGB.AllRGBs;
        var shuffler = new Thrice.Shuffler(null);
        var copy = shuffler.ShuffleCopy(allColors);

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
        var allColors = ARGB.AllRGBs;

        HashSet<ARGB[]> colorSequences = new(ARGBComparer.Instance);

        for (var i = 0; i < Depth; i++)
        {
            // Create a shuffler with no seed
            var shuffler = new Thrice.Shuffler(null);

            var shuffled = shuffler.ShuffleCopy(allColors);

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

        var allColors = ARGB.AllRGBs;

        foreach (int seed in seeds)
        {
            HashSet<ARGB[]> colorSequences = new(ARGBComparer.Instance);

            for (var d = 0; d < Depth; d++)
            {
                // Create a shuffler with no seed
                var shuffler = new Thrice.Shuffler(seed);

                var shuffled = shuffler.ShuffleCopy(allColors);

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