using System.Diagnostics;
using System.Drawing.Imaging;
using AllColors.Thrice;

// 1080 x 2340 is Pixel4a




ImageOptions options = ImageOptions.BestRectangle(24);

ImageGenerator generator = new ImageGenerator(options);
int? seed = 147;
var directBitmap = generator.Generate(seed);

string imagePath = $@"c:\temp\image_{options.ColorCount}_{options.Width}_{options.Height}_{seed}.bmp";

directBitmap.Bitmap.Save(imagePath, ImageFormat.Bmp);

Process.Start(new ProcessStartInfo()
{
    FileName = imagePath,
    UseShellExecute = true
});


directBitmap.Dispose();

Debugger.Break();