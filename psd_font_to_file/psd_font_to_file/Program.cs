using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using LitJson;
using psd;

namespace psd_font_to_file {
    class Program {
        public Program() {
        }

        static void Main(string[] args) {
//            if (args.Length != 1) {
//                Console.WriteLine("not args => " + args.Length);
//                Console.ReadKey();
//                return;
//            }
//            string path = args[0];
            string path = "C:\\Users\\yiz\\Desktop\\222.psd";
            Console.WriteLine(path);

            string outTmpDir = Path.GetDirectoryName(path) + "\\";
            string outTmpPath = outTmpDir + "psd_tmp_file";
            if (!Directory.Exists(outTmpPath)) {
                Directory.CreateDirectory(outTmpPath);
            }
            string fileName = Path.GetFileNameWithoutExtension(path);
             PsdLayerExtractor psdLayerExtractor = new PsdLayerExtractor(path);
            Run(path);
            string text = psdLayerExtractor.GetSingleText().Replace("\0", "");
            Console.WriteLine(text);
            string imgPath = path.Replace(".psd", ".png");
            while (true) {
                if (File.Exists(imgPath)) {
                    break;
                }
                Console.WriteLine("not found file => " + imgPath);
                Thread.Sleep(1000);
            }

            Bitmap b = new Bitmap(imgPath);
            int left = 0;
            for (int i = 0; i < b.Width; i++) {
                for (int j = 0; j < b.Height; j++) {
                    Color color = b.GetPixel(i, j);
                    if (HasColor(color)) {
                        left = i;
                        goto NEXT1;
                    }
                }
            }
            NEXT1:
            int right = 0;
            for (int i = b.Width - 1; i >= 0; i--) {
                for (int j = b.Height - 1; j >= 0; j--) {
                    Color color = b.GetPixel(i, j);
                    if (HasColor(color)) {
                        right = i;
                        goto NEXT2;
                    }
                }
            }
            NEXT2:
            int top = 0;
            for (int j = 0; j < b.Height; j++) {
                for (int i = 0; i < b.Width; i++) {
                    Color color = b.GetPixel(i, j);
                    if (HasColor(color)) {
                        top = j;
                        goto NEXT3;
                    }
                }
            }
            NEXT3:
            int bottom = 0;
            for (int j = b.Height - 1; j >= 0; j--) {
                for (int i = b.Width - 1; i >= 0; i--) {
                    Color color = b.GetPixel(i, j);
                    if (HasColor(color)) {
                        bottom = j;
                        goto NEXT4;
                    }
                }
            }
            NEXT4:
            Console.WriteLine(left + "," + top + "," + right + "," + bottom);
            List<MyRect> list = Parse2(b, left, top, right, bottom);
            if (list.Count != text.Length) {
                Console.WriteLine("[ERROR] --------------------------- list.Count != text.Length " + list.Count + "  ;  " + text.Length);
            } else {
                HashSet<string> set = new HashSet<string>();
                List<OutRect> outList = new List<OutRect>();
                for (int i = 0, len = list.Count; i < len; i++) {
                    string str = text.Substring(i, 1);
                    if (set.Add(str)) {
                        outList.Add(new OutRect() {
                            rect = list[i],
                            str = str,
                        });
                    }
                }
                string outDir = outTmpPath + "/slice";
                List<string> filePath= new List<string>();
                foreach (OutRect rect in outList) {
                    string outFilePath = SliceBitmap(b, rect.rect, rect.str, outDir);
                    filePath.Add(outFilePath);
                }
                b.Dispose();
                if (File.Exists(imgPath)) {
                    File.Delete(imgPath);
                }
                string outFile = outTmpPath + "/" + fileName;
                RunTP(outDir, outFile);
                JsonData jsonData = LitJson.JsonMapper.ToObject(File.ReadAllText(outFile + ".json", Encoding.Default));
                JsonData outData = new JsonData();
                outData["file"] = fileName + ".png";
                JsonData outFrames = new JsonData();
                outData["frames"] = outFrames;
                foreach (JsonData VARIABLE in jsonData["frames"]) {
                    JsonData outJson = new JsonData();
                    outJson["x"] = VARIABLE["frame"]["x"];
                    outJson["y"] = VARIABLE["frame"]["y"];
                    outJson["w"] = VARIABLE["frame"]["w"];
                    outJson["h"] = VARIABLE["frame"]["h"];
                    outJson["offX"] = VARIABLE["spriteSourceSize"]["x"];
                    outJson["offY"] = VARIABLE["spriteSourceSize"]["y"];
                    outJson["sourceW"] = VARIABLE["sourceSize"]["w"];
                    outJson["sourceH"] = VARIABLE["sourceSize"]["h"];
                    outFrames[LanChange(((string) VARIABLE["filename"]).Split('.')[0])] = outJson;
                }
                string outjsonstring = JsonMapper.ToJson(outData);
//                byte[] buffer = Encoding.GetEncoding("gbk").GetBytes(JsonMapper.ToJson(outData));
//                string outstring = Encoding.UTF8.GetString(buffer);
                string outstring = LanChange(outjsonstring);
//                string outstring = outjsonstring;
                File.WriteAllText(outTmpDir + fileName + ".fnt", outstring, Encoding.UTF8);
                File.Copy(outFile + ".png", outTmpDir + fileName + ".png");
            }
            Directory.Delete(outTmpPath, true);
            Console.WriteLine("Finish!!!");
            Console.ReadKey();
        }
        static string LanChange(string str) {
            Encoding utf8;
            Encoding gb2312;
            utf8 = Encoding.GetEncoding("UTF-8");
            gb2312 = Encoding.GetEncoding("GB2312");
            byte[] gb = gb2312.GetBytes(str);
            gb = Encoding.Convert(gb2312, utf8, gb);
            return utf8.GetString(gb);
        }
        static string SliceBitmap(Bitmap b, MyRect rect, string fName, string outDir) {
            if (!Directory.Exists(outDir)) {
                Directory.CreateDirectory(outDir);
            }
            int w = rect.right - rect.left + 1;
            int h = rect.bottom - rect.top + 1;
            Bitmap outBitmap = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(outBitmap)) {
                //清空画布并以透明背景色填充
                g.Clear(Color.Transparent);
                //在指定位置并且按指定大小绘制原图片的指定部分
                g.DrawImage(b, new Rectangle(0, 0, w, h), rect.left, rect.top, w, h, GraphicsUnit.Pixel);
                string path = outDir + "/" + fName + ".png";
                outBitmap.Save(path);
                outBitmap.Dispose();
                return path;
            }
            return null;
        }

        static List<MyRect> Parse2(Bitmap b, int left, int top, int right, int bottom) {
            List<MyRect> list = new List<MyRect>();
            int start = left;
            for (int i = left; i <= right; i++) {
                bool hasColor = true;
                for (int j = top; j <= bottom; j++) {
                    hasColor = HasColor(b.GetPixel(i, j));
                    if (hasColor) {
                        break;
                    }
                }
                if (!hasColor) {
                    if (start >= 0) {
                        MyRect rect = new MyRect();
                        rect.left = start;
                        rect.right = i - 1;
                        rect.top = top;
                        rect.bottom = bottom;
                        list.Add(rect);
                    }
                    start = -1;
                } else {
                    if (start < 0) {
                        start = i;
                    }
                }
            }
            {
                if (start >= 0) {
                    MyRect rect = new MyRect();
                    rect.left = start;
                    rect.right = right;
                    rect.top = top;
                    rect.bottom = bottom;
                    list.Add(rect);
                }
                
            }
            return list;
        }

        static bool HasColor(Color color) {
            if (color.A == 0) {
                return false;
            }
            return color.R > 1 || color.G > 1 || color.B > 1;
        }

        static void Run(string filePath) {
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            
//            Directory.SetCurrentDirectory("./psd");
            exep.StartInfo.FileName = "psd2png.exe";
            exep.StartInfo.Arguments = string.Format("-i {0}", filePath);
            //            exep.StartInfo.CreateNoWindow = true;
            exep.StartInfo.CreateNoWindow = false;
            exep.StartInfo.UseShellExecute = false;
            exep.Start();
            exep.WaitForExit();//关键，等待外部程序退出后才能往下执行
//            Directory.SetCurrentDirectory("../");
        }

//        static void RunPack(string filePath) {
//            System.Diagnostics.Process exep = new System.Diagnostics.Process();
//            
//            exep.StartInfo.FileName = "texturepack/bin/TexturePacker.exe";
//            exep.StartInfo.Arguments = string.Format("-i {0}", filePath);
//            //            exep.StartInfo.CreateNoWindow = true;
//            exep.StartInfo.CreateNoWindow = false;
//            exep.StartInfo.UseShellExecute = false;
//            exep.Start();
//            exep.WaitForExit();//关键，等待外部程序退出后才能往下执行
//        }

        static void RunTP(string inFiles, string outPath) {
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            string[] argsArray;
                argsArray = new[] {
                    "--disable-rotation",
                    "--size-constraints AnySize",
                    "--force-squared",
                    "--max-width 2048",
                    "--max-height 2048",
                    "--format json-array",
                };
            string args = string.Join(" ", argsArray);
            string outImg = outPath + ".png";
            string outJson = outPath + ".json";

            exep.StartInfo.FileName = "texturepack/bin/TexturePacker.exe";
            exep.StartInfo.Arguments = string.Format("{0} --data {1} --sheet {2} {3}", args, outJson, outImg, inFiles);
            //            exep.StartInfo.CreateNoWindow = true;
            exep.StartInfo.CreateNoWindow = false;
            exep.StartInfo.UseShellExecute = false;
            exep.Start();
            exep.WaitForExit();//关键，等待外部程序退出后才能往下执行
        }
    }
}

struct MyRect {
    public int left;
    public int right;
    public int top;
    public int bottom;

    public override string ToString() {
        return $"Left: {left}, Right: {right}, Top: {top}, Bottom: {bottom}";
    }
}

struct OutRect {
    public MyRect rect;
    public string str;
}