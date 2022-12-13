using System.Diagnostics;
using System.Drawing.Imaging;
using AllColors;
using AllColors.FirstRGBGen;
using AllColors.Thrice;

// Faster!
Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

// 1080 x 2340 is Pixel4a


//64 512 512 256 256 5 0 11111111 rnd one
args = new string[] { "64", "512", "512", "256", "256", "5", "0", "11111111", "rnd", "one" };
//Prog.Main(new string[] { "64", "512", "512", "256", "256", "5", "0", "11111111", "rnd", "one" });
//Program2.Run(args);


ImageGenerator generator = new ImageGenerator(ColorSpace.BestFit(1080, 2340));
int? seed = 147;
var directBitmap = generator.Generate(seed);
//string imagePath = $@"c:\temp\image_{options.ColorCount}_{options.Width}_{options.Height}_{seed}.bmp";


/*var cs = ColorSpace.BestFit(1080, 2340);
args[0] = cs.ColorDepth.ToString();
args[1] = cs.Width.ToString();
args[2] = cs.Height.ToString();
args[3] = cs.MidPoint.X.ToString();
args[4] = cs.MidPoint.Y.ToString();

var options = PixelGenOptions.ParseArgs(args);
options.Shuffler = new Shuffler(147);
//options.Sorter = new HueComparer(0);
var directBitmap = PixelGen.Run(options!);
*/

string imagePath = $@"c:\temp\image_{DateTime.Now:yyyyMMddHHmmss}.bmp";

directBitmap!.Bitmap.Save(imagePath, ImageFormat.Bmp);

Process.Start(new ProcessStartInfo()
{
    FileName = imagePath,
    UseShellExecute = true
});


directBitmap.Dispose();

//Debugger.Break();

Console.WriteLine("Press Enter to close");
Console.ReadLine();