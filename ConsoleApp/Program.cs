using System.Diagnostics;
using System.Drawing.Imaging;
using AllColors.Thrice;

// 1080 x 2340
ImageOptions options = ImageOptions.BestColors(1080, 2340);

Debugger.Break();



ImageGenerator generator = new ImageGenerator(options, 147);
using var image = generator.Generate();

string imagePath = @"c:\temp\image.bmp";

image.Bitmap.Save(imagePath, ImageFormat.Bmp);

Process.Start(new ProcessStartInfo()
{
    FileName = Directory.GetParent(imagePath)!.FullName,
    UseShellExecute = true
});

//
// foreach (var color in typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static)
//     .Where(prop => prop.PropertyType == typeof(Color))
//     .Select(prop => prop.GetValue(null))
//     .OfType<Color>())
// {
//     int colorInt = color.ToArgb();
//     ARGB colorARGB = new ARGB(color);
//     int argbInt = colorARGB.GetHashCode();
//     Debug.Assert(colorInt == argbInt);
//     Debug.Assert(colorARGB.Alpha == color.A);
//     Debug.Assert(colorARGB.Red == color.R);
//     Debug.Assert(colorARGB.Green == color.G);
//     Debug.Assert(colorARGB.Blue == color.B);
// }

Debugger.Break();