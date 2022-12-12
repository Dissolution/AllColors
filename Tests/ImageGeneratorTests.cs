using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllColors.Thrice;
using FluentAssertions;

namespace AllColors.Tests;

public class ImageGeneratorTests
{
    [Fact]
    public void OutputIsConsistent()
    {
        var options = ColorSpace.BestRectangle(16);
        int seed = 147;

        int hash = 0;

        for (var i = 0; i < 10; i++)
        {
            var gen = new ImageGenerator(options);

            for (var j = 0; j < 10; j++)
            {
                DirectBitmap data = gen.Generate(seed);

                var dataHash = ARGBComparer.Instance.GetHashCode(data.ARGBSpan);

                if (hash == 0)
                {
                    hash = dataHash;
                }
                else
                {
                    if (dataHash != hash)
                    {

                    }

                    dataHash.Should().Be(hash);
                }

                data.Dispose();
            }
        }
    }
}