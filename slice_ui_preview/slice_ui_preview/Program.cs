using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace slice_ui_preview {
    class Program {
        static void Main(string[] args) {
            while (true) {
                string str = Console.ReadLine();
                Console.WriteLine(str);

                if (Directory.Exists(str)) {
                    SliceDir(str);
                } else if (File.Exists(str)) {
                    SliceFile(str);
                } else {
                    Console.WriteLine("Not Found File " + str );
                    continue;
                }
                break;
            }

            Console.WriteLine("Finish!!!");
            Console.ReadKey();
        }

        static void SliceDir(string dir) {
            string[] allFile = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
            foreach (var s in allFile) {
                SliceFile(s);
            }
        }

        static void SliceFile(string filePath) {
            if (!File.Exists(filePath)) {
                return;
            }
            if (!filePath.EndsWith(".png") && !filePath.EndsWith(".jpg")) {
                return;
            }
            try {
                Image image = Image.FromFile(filePath);
                if (image.Width != 720 && image.Height != 1280) {
                    Console.WriteLine("Image Not Size 720 x 1280");
                    return;
                }

                string curDir = Directory.GetCurrentDirectory();
                string newPath;
                if (filePath.StartsWith(curDir)) {
                    newPath = filePath.Replace(curDir, curDir + "\\__out__\\");
                } else {
                    newPath = curDir + "\\__out__" + filePath.Split(':')[1];
                }
                if (!Directory.Exists(Path.GetDirectoryName(newPath))) {
                    Directory.CreateDirectory(Path.GetDirectoryName(newPath));
                }

                Bitmap mImage = new Bitmap(image);

                int startx = 20;
                int starty = 104;
                Bitmap bitmap = new Bitmap(680, 940, PixelFormat.Format24bppRgb);
                bitmap.SetResolution(72, 72);
                for (int j = 0; j < bitmap.Height; ++j) {
                    for (int i = 0; i < bitmap.Width; ++i) {
                        bitmap.SetPixel(i, j, mImage.GetPixel(startx + i, starty + j)); 
                    }
                }
                bitmap.Save(newPath);
                Console.WriteLine("切图 => " + filePath + ", " + newPath);

            } catch (Exception e) {
                Console.WriteLine(e); 
            }
        }
    }
}
