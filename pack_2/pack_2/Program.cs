using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LitJson;
using packTex;

namespace pack_2 {
    class Program {
       
        private const string CONFIG_NAME = "lybconf.json";

        static void Main(string[] args) {
            if (File.Exists(CONFIG_NAME)) {
                JsonData jsonData = LitJson.JsonMapper.ToObject(File.ReadAllText(CONFIG_NAME));
                string outPath = (string) jsonData["out_path"];
                
                List<string> fileList = new List<string>();
                for (int i = 0, len = jsonData["dir"].Count; i < len; ++i) {
                    JsonData data = jsonData["dir"][i];
                    string[] files = Directory.GetFiles((string) data["path"], "*.png");
                    HashSet<string> set = new HashSet<string>();
                    for (int j = 0, jlen = data["exclude_name"].Count; j < jlen; ++j) {
                        set.Add((string) data["exclude_name"][j]);
                    }
                    foreach (string s in files) {
                        string fileName = Path.GetFileNameWithoutExtension(s);
                        if (set.Contains(fileName)) {
                            continue;
                        }
                        fileList.Add(s);
                    }
                }
                for (int i = 0, len = jsonData["file_path"].Count; i < len; ++i) {
                    string path = (string) jsonData["file_path"][i];
                    fileList.Add(path);
                }
                ClearDir(outPath);
                for (int i = 0, len = fileList.Count; i < len; ++i) {
                    string path = fileList[i];
                    string jsonPath = path.Replace(".png", ".json");
                    string outName = Path.GetFileNameWithoutExtension(path);
                    SliceBitmap(path, jsonPath, outPath, outName);
                    Console.WriteLine("out => " + path);
                }
                Console.WriteLine("finish !!!");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("not config !!!");
            Console.ReadKey();
        }

        const int SIZE = 32;

        static void ClearDir(string dir) {
            if (Directory.Exists(dir)) {
                foreach (var VARIABLE in Directory.GetFiles(dir)) {
                    File.Delete(VARIABLE);
                }
                foreach (var VARIABLE in Directory.GetDirectories(dir)) {
                    Directory.Delete(VARIABLE); 
                }
            } else {
                Directory.CreateDirectory(dir);
            }
        }

        static void SliceBitmap(string imgPath, string imgJsonPath, string saveDir, string saveName) {

            string tempDir = ".\\td\\" + saveName + "\\";
            ClearDir(tempDir);

            Image img = Image.FromFile(imgPath);
            Bitmap mImage = new Bitmap(img);

            int width = (int)Math.Ceiling(mImage.Width * 1.0f / SIZE);
            int height = (int)Math.Ceiling(mImage.Height * 1.0f / SIZE);

            List<string> list = new List<string>();
            for (int j = 0; j < height; j++) {
                for (int i = 0; i < width; i++) {
                    list.Add(CreateChunkBitmap(mImage, tempDir, i, j));
                }
            }

            img.Dispose();

            string outDir = saveDir;
            string outName = saveName;
            string jsonPath = outDir + "\\" + outName + ".json";
            MergerTexUtil obj = new packTex.MergerTexUtil(MainInit.InitDllPath());
            obj.HandleFiles(tempDir, outDir, outName, 5, true, true);
            JsonData outJsonData = LitJson.JsonMapper.ToObject(File.ReadAllText(imgJsonPath));
            if (File.Exists(jsonPath)) {
                JsonData jsonData = LitJson.JsonMapper.ToObject(File.ReadAllText(jsonPath));

                var frames = jsonData["mc"][outName]["frames"];
                Dictionary<string, Point> frameDict = new Dictionary<string, Point>();
                for (int i = 0; i < frames.Count; ++i) {
                    frameDict[(string) frames[i]["res"]] = new Point((int) frames[i]["x"], (int) frames[i]["y"]);
                }
                Dictionary<string, object> newJsonData = new Dictionary<string, object>();
                newJsonData["name"] = outName;
                newJsonData["swidth"] = mImage.Width;
                newJsonData["sheight"] = mImage.Height;
                Image newImg = Image.FromFile(outDir + "\\" + outName + ".png");
                newJsonData["width"] = newImg.Width;
                newJsonData["height"] = newImg.Height;
                newImg.Dispose();
                newJsonData["size"] = SIZE;
                Dictionary<string, int[]> dict = new Dictionary<string, int[]>();
                for (int j = 0; j < height; j++) {
                    for (int i = 0; i < width; i++) {
                        string thunkName = GetTileName(i, j);
                        JsonData resData = jsonData["res"][thunkName];
                        Point p;
                        frameDict.TryGetValue(thunkName, out p);
                        int cx = SIZE >> 1;
                        int cy = SIZE >> 1;
                        dict[thunkName] = new int[] {(int) resData["x"] + 1, (int) resData["y"] + 1, (int) resData["w"] - 2, (int) resData["h"] - 2, cx + p.X + 1, cy + p.Y + 1};
                    }
                }
                newJsonData["data"] = dict;
                ResMesh(outJsonData, newJsonData);
//                File.WriteAllText(jsonPath + "22222222222.json", JsonMapper.ToJson(newJsonData));
//                File.WriteAllText(jsonPath + "1111111111.json", JsonMapper.ToJson(outJsonData));
                File.WriteAllText(jsonPath, JsonMapper.ToJson(outJsonData));
            } else {
                Console.WriteLine("[ERROR] NOT JSON CONFIG !!!!!!!!");
            }
        }

        static void CaclIndexAndOffset(int v, out int v1, out int v2) {
            v1 = (int) Math.Floor(v * 1.0/ SIZE);
            v2 = v - v1 * SIZE;
        }

        static void ResMesh(JsonData jsonData, Dictionary<string, object> jsonObj) {
            Dictionary<string, int[]> datas = (Dictionary<string, int[]>) jsonObj["data"];
            int width = (int) jsonObj["width"];
            int height = (int) jsonObj["height"];
            var res = jsonData["res"];
            jsonData["width"] = width;
            jsonData["height"] = height;
            var collection = res.Keys;

            foreach (string key in collection) {

                List<int> uvs = new List<int>();
                List<int> vertices  = new List<int>();
                List<int> indices = new List<int>();

                int slen = 0;
                System.Action<int, int, int, int, int, int, int, int> addTar = (x1, y1, mx2, my2, ux1, uy1, uw, uh) => {
                    var ux2 = ux1 + uw;
                    var uy2 = uy1 + uh;
                    var len = slen * 4;
                    slen += 1;
                    uvs.AddRange(new int[] {ux1, uy1, ux2, uy1, ux2, uy2, ux1, uy2});
        			vertices.AddRange(new int[] {x1,y1, mx2,y1, mx2,my2, x1,my2});
        			indices.AddRange(new int[] {len, len + 1, len + 2, len + 2, len + 3, len});
                };

                var resData = res[key];
                int x = (int) resData["x"];
                int y = (int) resData["y"];
                int w = (int) resData["w"];
                int h = (int) resData["h"];
                int x2 = x + w;
                int y2 = y + h;

                int xIndex, xOffset;
                int yIndex, yOffset;
                int x2Index, x2Offset;
                int y2Index, y2Offset;
                CaclIndexAndOffset(x, out xIndex, out xOffset);
                CaclIndexAndOffset(y, out yIndex, out yOffset);
                CaclIndexAndOffset(x2, out x2Index, out x2Offset);
                CaclIndexAndOffset(y2, out y2Index, out y2Offset);

                for (int j = yIndex; j <= y2Index; ++j) {
                    for (int i = xIndex; i <= x2Index; ++i) {
					    string imgName = j + "_" + i;
                        int[] data = datas[imgName];
					    int chunkW = data[2];
					    var chunkH = data[3];
                        if (chunkW >= 2 && chunkH >= 2) {
                            int chunkX1 = i * SIZE + data[4];
                            int chunkY1 = j * SIZE + data[5];
                            int chunkX2 = chunkX1 + chunkW;
                            int chunkY2 = chunkY1 + chunkH;

                            if (x <= chunkX2 && x2 >= chunkX1 && y <= chunkY2 && y2 >= chunkY1) {
                                int _x1 = Math.Max(chunkX1, x);
                                int _y1 = Math.Max(chunkY1, y);
                                int _x2 = Math.Min(chunkX2, x2);
                                int _y2 = Math.Min(chunkY2, y2);
                                addTar(_x1 - x, _y1 - y, _x2 - x, _y2 - y, ((_x1 - chunkX1) + data[0]), ((_y1 - chunkY1) + data[1]), _x2 - _x1, _y2 - _y1);
                            }
                        }
                    }
                }

                JsonData uvData = new JsonData();
                uvData.SetJsonType(JsonType.Array);
                foreach (int uv in uvs) {
                    uvData.Add(uv);
                }
                resData["m1"] = uvData;
                JsonData verData = new JsonData();
                verData.SetJsonType(JsonType.Array);
                foreach (int uv in vertices) {
                    verData.Add(uv);
                }
                resData["m2"] = verData;
                JsonData inData = new JsonData();
                inData.SetJsonType(JsonType.Array);
                foreach (int uv in indices) {
                    inData.Add(uv);
                }
                resData["m3"] = inData;
            }
//            return new List<int>[] {uvs, vertices, indices};
        }

//        static void CaleTar(Dictionary<string, object> jsonObj) {
//            int size = (int) jsonObj["size"];
//            int width = (int) jsonObj["width"];
//            int height = (int) jsonObj["height"];
//            var wLen = Math.Ceiling((int)jsonObj["swidth"] * 0.1f/ (int)jsonObj["size"]);
//			var hLen = Math.Ceiling((int)jsonObj["sheight"] * 0.1f/ (int)jsonObj["size"]);
//            Dictionary<string, int[]> datas = (Dictionary<string, int[]>) jsonObj["data"];
//				for (int j = 0; j < hLen; ++j) {
//					for (int i = 0; i < wLen; ++i) {
//					    string imgName = j + "_" + i;
//					    int[] data = datas[imgName];
//					    int imgWidth = data[2];
//					    var imgHeight = data[3];
//
//						if (imgWidth != 1 && imgHeight != 1) {
//						    addTar(i * size + data[4], j * size + data[5], imgWidth, imgHeight,
//						        data[0] / width, data[1] / height, imgWidth / width, imgHeight / height);
//						}
//						
//					}
//				}
//        }

        static string CreateChunkBitmap(Bitmap image, string saveDir, int chunkX, int chunkY) {
            int w = (chunkX + 1) * SIZE;
            int h = (chunkY + 1) * SIZE;
            if (w >= image.Width) {
                w = image.Width - chunkX * SIZE;
            } else {
                w = SIZE;
            }
            if (h >= image.Height) {
                h = image.Height - chunkY * SIZE;
            } else {
                h = SIZE;
            }
            Bitmap bitmap = new Bitmap(w + 2, h + 2, PixelFormat.Format32bppArgb);
            bitmap.SetResolution(72, 72);

            for (int j = 0; j < h; j++) {
                for (int i = 0; i < w; i++) {
                    int x = chunkX * SIZE + i;
                    int y = chunkY * SIZE + j;
                        bitmap.SetPixel(i + 1, j + 1, image.GetPixel(x, y));
                }
            }
            int chunkStartX = chunkX * SIZE;
            int chunkStartY = chunkY * SIZE;
            bitmap.SetPixel(0, 0, image.GetPixel(chunkStartX, chunkStartY));
            bitmap.SetPixel(w + 1, 0, image.GetPixel(chunkStartX + w - 1, chunkStartY));
            for (int i = 0; i < w; i++) {
                int x = chunkStartX + i;
                bitmap.SetPixel(i + 1, 0, image.GetPixel(x, chunkStartY + 0));
                bitmap.SetPixel(i + 1, h + 1, image.GetPixel(x, chunkStartY + h - 1));
            }
            bitmap.SetPixel(0, h + 1, image.GetPixel(chunkStartX, chunkStartY + h - 1));
            bitmap.SetPixel(w + 1, h + 1, image.GetPixel(chunkStartX + w - 1, chunkStartY + h - 1));
            for (int j = 0; j < h; j++) {
                int y = chunkStartY + j;
                bitmap.SetPixel(0, j + 1, image.GetPixel(chunkStartX + 0, y));
                bitmap.SetPixel(w + 1, j + 1, image.GetPixel(chunkStartX + w - 1, y));
            }


            string savePath = Path.Combine(saveDir, GetTileName(chunkX, chunkY) + ".png");
//            Console.WriteLine(savePath);
            
            bitmap.Save(savePath, ImageFormat.Png);
            return savePath;
        }

        static string GetTileName(int x, int y) {
            return y + "_" + x;
        }
    }
}


    class MainInit {
        public static void Init() {
            string[] DIR = new string[] {
                "..\\libs\\"
            };
            System.Func<string, string> findDll = s => {
                s = s.Split(',')[0].Trim();
                foreach (string value in DIR) {
                    string path = value + s + ".dll";
                    if (File.Exists(path)) {
                        return path;
                    }
                }
                return null;
            };
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                string path = findDll(args.Name);
                if (path != null) {
                    Assembly assembly = Assembly.LoadFrom(path);
                    return assembly;
                }
                return null;
            };
        }

        public static string InitDllPath() {
            if (File.Exists("FLAG")) {
                return "..\\..\\..\\..\\libs\\";
            }
            return "..\\libs\\";
        }
    }