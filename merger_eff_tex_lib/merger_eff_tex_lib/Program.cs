using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LitJson;
using merger_tex;

namespace merger_eff_tex_lib {
    class Program {
        struct Range {
            public string mState;
            public int mMinID; 
            public int mMaxID;
            public int mFrame;

            public Range(string state, int minId, int maxId, int frame = 12) {
                this.mState = state;
                this.mMinID = minId;
                this.mMaxID = maxId;
                mFrame = frame;
            }
        }


        private static readonly List<Range> CONFIG = new List<Range>() {
            new Range("s", 1, 4),
            new Range("r", 15, 20),
            new Range("a", 24, 30),
            new Range("c", 31, 37),
        };

        private static readonly Dictionary<string, Range> DICT = CONFIG.ToDictionary(range => range.mState);

        const string OUT_DIR = "wing";

        private const string CONFIG_NAME = "conf.json";

        static void AddConfig(JsonData jsonData, string key) {
            if (jsonData.Keys.Contains(key)) {
                CONFIG.Add(new Range(key, (int) jsonData[key][0], (int) jsonData[key][1]));
            } else {
                CONFIG.Add(DICT[key]);
                Console.WriteLine("Not Found Key => " + key);
            }
        }

        public static int TYPE = 2;
        static void Main(string[] args) {

            MainInit.Init();

//            Console.WriteLine(Console.ReadKey());
//            Console.ReadKey();
//            return;

            Console.WriteLine(@"需要要打包的类型:

1、翅膀
2、动作
3、武器
4、怪物
5、英雄

");
            ConsoleKeyInfo keyInfo;
            while (true) {

                keyInfo = Console.ReadKey();
                if (keyInfo.KeyChar == '1') {
                    TYPE = 1;
                    Console.WriteLine("开始打包翅膀");
                } else if (keyInfo.KeyChar == '2') {
                    TYPE = 2;
                    Console.WriteLine("开始打包动作");
                } else if (keyInfo.KeyChar == '3') {
                    TYPE = 3;
                    Console.WriteLine("开始打包武器");
                } else if (keyInfo.KeyChar == '4') {
                    TYPE = 4;
                    Console.WriteLine("开始打包怪物");
                } else if (keyInfo.KeyChar == '5') {
                    TYPE = 5;
                    Console.WriteLine("开始打包英雄");
                } else {
                    Console.WriteLine();
                    Console.WriteLine("请输入正确的指令，当前指令 => " + keyInfo.KeyChar);
                    continue;
                }
                break;
            }

//            Console.WriteLine(TYPE);
//Console.ReadKey();
//            return;

//            string baseDir = @"E:\lycq\resource\总美术上传文件\";
//            string outBaseDir = @"E:\lycq\client\project\resource\assets\movie\";

            string baseDir = Directory.GetCurrentDirectory() + @"\..\..\总美术上传文件\";
            string outBaseDir = Directory.GetCurrentDirectory() + @"\..\..\temp_out_movie\";

            if (!Directory.Exists(baseDir)) {
                Console.WriteLine("没有找到目录 => " + baseDir);
                Console.ReadKey();
                return;
            }

            if (!Directory.Exists(outBaseDir)) {
                Directory.CreateDirectory(outBaseDir);
            }
             
            // 翅膀
            if (TYPE == 1) {
                string[] allDir = Directory.GetDirectories(baseDir + "翅膀\\", "*", SearchOption.TopDirectoryOnly);
                foreach (string s in allDir) {
                    SingleFile(s + "\\", outBaseDir + "wing\\", Path.GetFileNameWithoutExtension(s), null);
                }
            // 服装
            } else if (TYPE == 2) {
                string[] allDir = Directory.GetDirectories(baseDir + "3d模型角色动作\\", "*", SearchOption.TopDirectoryOnly);
                foreach (string s in allDir) {
                    string[] nameDir = Directory.GetDirectories(s, "*", SearchOption.TopDirectoryOnly);
                    foreach (string namePath in nameDir) {
//                        string namePath = nameDir[0];
                        string name = Path.GetFileNameWithoutExtension(namePath);
                        string outDir = outBaseDir + "body\\";
                        SingleFile(namePath + "\\男\\", outDir, name, "0");
                        SingleFile(namePath + "\\女\\", outDir, name, "1");
                    }
                    //                    if (nameDir.Length > 0) {
                    //                        string namePath = nameDir[0];
                    //                        string name = Path.GetFileNameWithoutExtension(namePath);
                    //                        string outDir = @"E:\lycq\client\project\resource\assets\movie\body\";
                    //                        SingleFile(namePath + "\\男\\", outDir, name, "0");
                    //                        SingleFile(namePath + "\\女\\", outDir, name, "1");
                    //                    }

                }
            // 武器
            } else if (TYPE == 3) {

                System.Action<string, string> parser = (s, sex) => {
                    if (!Directory.Exists(s)) {
                        return;
                    }
                    string[] nameDirs = Directory.GetDirectories(s, "*", SearchOption.TopDirectoryOnly);
                    foreach (string nameDir in nameDirs) {
                        string name = Path.GetFileNameWithoutExtension(nameDir);
                        string outDir = outBaseDir + "weapon\\";
                        SingleFile(nameDir + "\\", outDir, name, sex);
                    }
                };

                string[] allDir = Directory.GetDirectories(baseDir + "武器\\", "*", SearchOption.TopDirectoryOnly);
                foreach (string s in allDir) {
                    string dir0 = s + "\\男\\";
                    string dir1 = s + "\\女\\";
                    parser(dir0, "0");
                    parser(dir1, "1");
//                    string[] nameDir = Directory.GetDirectories(s, "*", SearchOption.TopDirectoryOnly);
//                    if (nameDir.Length > 0) {
//                        string namePath = nameDir[0];
//                        string name = Path.GetFileNameWithoutExtension(namePath);
//                        string outDir = @"E:\lycq\client\project\resource\assets\movie\body\";
//                        SingleFile(namePath + "\\男\\", outDir, name, "0");
//                        SingleFile(namePath + "\\女\\", outDir, name, "1");
//                    }

                }
            } else if (TYPE == 4) {
                
                CONFIG.Clear();

                CONFIG.Add(new Range("s", 1, 4, 6));
                CONFIG.Add(new Range("r", 6, 11));
                CONFIG.Add(new Range("a", 13, 17));
                CONFIG.Add(new Range("c", 30, 35));

                string[] allDir = Directory.GetDirectories(baseDir + "怪物\\", "*", SearchOption.TopDirectoryOnly);
                foreach (string s in allDir) {
                    SingleFile(s + "\\", outBaseDir + "monster\\", Path.GetFileNameWithoutExtension(s), null);
                }
            } else if (TYPE == 5) {

                string[] allDir = Directory.GetDirectories(baseDir + "英雄\\", "*", SearchOption.TopDirectoryOnly);
                foreach (string s in allDir) {
                    SingleFile(s + "\\", outBaseDir + "hero\\", Path.GetFileNameWithoutExtension(s), null);
                }

            } else {
                JsonData jsonData = LitJson.JsonMapper.ToObject(File.ReadAllText(CONFIG_NAME));

                ICollection<string> keys = jsonData.Keys;
                if (keys.Contains("frame")) {
                    CONFIG.Clear();

                    JsonData frameData = jsonData["frame"];
                    AddConfig(frameData, "s");
                    AddConfig(frameData, "r");
                    AddConfig(frameData, "a");
                    AddConfig(frameData, "c");
                }

    //            string dir = @"E:\lycq\resource\总美术上传文件\翅膀\男新手翅膀\";
    //            string name = "body001";
    //            string name = "wing00";
    //            string sex = "0";
    //            string sex = null;

                string dir = jsonData["asset_dir"].ToString() + "\\";
                string outDir = jsonData["out_dir"].ToString();
                string name = jsonData["name"].ToString();
                string sex = null;
                if (keys.Contains("sex")) {
                    sex = jsonData["sex"].ToString() == "男" ? "0" : "1";
                }

                SingleFile(dir, outDir, name, sex);
            }
            Console.ReadKey();

        }

        static void SingleFile(string dir, string outDir, string name, string sex) {

//            Console.WriteLine(dir +"   "  + outDir + "   "　+　name + "   " + sex);
//            return;

            HashSet<string> allFiels = new HashSet<string>();
            foreach (string VARIABLE in Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly)) {
                allFiels.Add(Path.GetFileNameWithoutExtension(VARIABLE));
            }

            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            Dictionary<string, int> frameDict = new Dictionary<string, int>();

            for (int forward = 0; forward < 5; forward++) {
                foreach (Range VARIABLE in CONFIG) {
                    List<string> list = new List<string>();
                    for (int i = VARIABLE.mMinID; i <= VARIABLE.mMaxID; i++) {
                        string fileName = forward + i.ToString("D4");
                        if (!allFiels.Contains(fileName)) {
                            Logger.LogError("不存在文件 => " + fileName + " 忽略动作类型 " + VARIABLE.mState);
                            goto NEXT;
                        }
                        list.Add(dir + fileName + ".png");
                    }
                    string dictName;
                    if (string.IsNullOrEmpty(sex)) {
                        dictName = name + "_" + forward + VARIABLE.mState;
                    } else {
                        dictName = name + "_" + sex + "_" + forward + VARIABLE.mState;
                    }
                    dict.Add(dictName, list);
                    frameDict.Add(dictName, VARIABLE.mFrame);
                    NEXT:
                    ;
                }
            }

            foreach (KeyValuePair<string, List<string>> pair in dict) {
                //                Console.WriteLine(pair.Key + "  " + string.Join(", ", pair.Value.ToArray()));
                Handle(pair.Value.ToArray(), outDir, pair.Key, frameDict[pair.Key]);
            }

            Console.WriteLine(">> 输出 " + name);
        }

        static void Handle(string[] handleFiles, string outDir, string outName, int frame) {
            MergerTex fileTool = new MergerTex();
            if (fileTool.HandleFrame(handleFiles, outDir, outName, frame)) {
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
}
