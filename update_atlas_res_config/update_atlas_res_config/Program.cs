using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitJson;

namespace update_atlas_res_config {
    class Program {
        static void Main(string[] args) {
//            string resjsonPath = @"E:\lycq\client\project\resource\default.res.json";
            string resjsonPath = @"resource\default.res.json";

            JsonData jsonData = JsonMapper.ToObject(File.ReadAllText(resjsonPath));

            JsonData newResjsonData = new JsonData();
            JsonData resJsonData = jsonData["resources"];
            for (int i = resJsonData.Count - 1; i >= 0; --i) {
                JsonData VARIABLE = resJsonData[i];
                string url = VARIABLE["url"].ToString();
                if (url.StartsWith("assets/atlas2_ui") || url.StartsWith("assets/atlas_ui") || url.StartsWith("assets/atlas_font")) {
                    Console.WriteLine("recycle " + url);
                } else {
                    newResjsonData.Add(VARIABLE);
                }

            }

            System.Action<string, bool> action = (path, suffix) => {
                string dir = Path.Combine(Path.GetDirectoryName(resjsonPath), path);
                string path2 = path.Replace("\\", "/");
                string[] allUIFiels = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);

                string[] uiFiles = Array.ConvertAll(allUIFiels, input => input.Replace(dir, string.Empty).Replace("\\", "/"));
                foreach (string uiFile in uiFiles) {
                    JsonData uiJsonData = new JsonData();
                    string name = Path.GetFileNameWithoutExtension(uiFile);
                    if (suffix) {
                        name = name + "_" + uiFile.Substring(uiFile.LastIndexOf("."));
                    }
                    string url = path2 + uiFile;
                    uiJsonData["name"] = name;
                    uiJsonData["type"] = "image";
                    uiJsonData["url"] = url;

                    string slice = ParseSliceArea(Path.Combine(dir, uiFile.Replace("/", "\\")), name.Split('@').ToList());
                    if (!string.IsNullOrEmpty(slice)) {
                        uiJsonData["scale9grid"] = slice;
                    }

                    Console.WriteLine("找到资源 " + url);
                    newResjsonData.Add(uiJsonData);
                }
            };
            action("assets\\atlas2_ui\\", false);
            action("assets\\atlas_ui\\", false);
            action("assets\\atlas_font\\", true);

//            Console.WriteLine(string.Join("\n", Array.ConvertAll(allUIFiels, input=>input.Replace(dir, string.Empty)) ));
            jsonData["resources"] = newResjsonData;

            File.WriteAllText(resjsonPath, jsonData.ToJson());

            Console.ReadKey();
        }

        private static string ParseSliceArea(string imgPath, List<string> name_cmdAndVals) {
            for (int i = 1; i < name_cmdAndVals.Count; i++) {
                string nameCmdAndVal = name_cmdAndVals[i];
                string[] array = nameCmdAndVal.Split('_');
                if (array.Length > 0) {
                    List<string> list = new List<string>();
                    foreach (string s in array) {
                        int num;
                        if (int.TryParse(s, out num)) {
                            list.Add(s);
                        } else {
                            list.Clear();
                            break;
                        }
                    }
                    if (list.Count > 0) {
                        string[] values = list.ToArray();

                        int left = int.Parse(values[0]);
                        int top = values.Length < 2 ? left : int.Parse(values[1]);
                        int right = values.Length < 3 ? left : int.Parse(values[2]);
                        int bottom = values.Length < 4 ? top : int.Parse(values[3]);

                        using (Image image = Image.FromFile(imgPath)) {
                            int imgwidht = image.Width;
                            int imgheight = image.Height;

                            int x = left;
                            int y = top;
                            int width = imgwidht - right - left;
                            int height = imgheight - bottom - top;

                            return string.Format("{0},{1},{2},{3}", x, y, width, height);
                        }
                    }
                }
            }
            return null;
        }
    }
}
