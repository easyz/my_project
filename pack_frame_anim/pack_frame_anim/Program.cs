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

        private const string CONFIG_NAME = "lybconf.json";

        static void Main(string[] args) {

            MainInit.Init();
            if (!File.Exists(CONFIG_NAME)) {
                Console.WriteLine("not config file => " + CONFIG_NAME);
                return;
            }
            JsonData jsonData = LitJson.JsonMapper.ToObject(File.ReadAllText(CONFIG_NAME));
            JsonData animJsonData = jsonData["anim"];
            string[] animTypes = animJsonData.Keys.ToArray();
            List<string> temp = new List<string>();
            foreach (string key in animTypes) {
                temp.Add(key + "、" + animJsonData[key]["name"]);
            }
            Console.WriteLine("需要要打包的类型:\n" + string.Join("\n", temp.ToArray()));

            JsonData exportData = null;
            while (true) {
                string keyInfo = Console.ReadKey().KeyChar.ToString();
                if (animTypes.Contains(keyInfo)) {
                    exportData = animJsonData[keyInfo];
                    Console.WriteLine("开始打包" + exportData["name"]);
                } else {
                    Console.WriteLine();
                    Console.WriteLine("请输入正确的指令，当前指令 => " + keyInfo);
                    continue;
                }
                break;
            }

            string baseDir = Directory.GetCurrentDirectory() + jsonData["baseDir"];
            string outBaseDir = Directory.GetCurrentDirectory() + jsonData["outBaseDir"];

            if (!Directory.Exists(baseDir)) {
                Console.WriteLine("没有找到目录 => " + baseDir);
                Console.ReadKey();
                return;
            }

            if (!Directory.Exists(outBaseDir)) {
                Directory.CreateDirectory(outBaseDir);
            }

            string[] allDir = Directory.GetDirectories(baseDir + exportData["path"] + "\\", "*", SearchOption.TopDirectoryOnly);
            foreach (string s in allDir) {
                SingleFile(s + "\\", outBaseDir + exportData["outPath"] + "\\", Path.GetFileNameWithoutExtension(s), exportData["action"]);
            }

            Console.WriteLine("Finish!!!");
            Console.ReadKey();

        }

        static void SingleFile(string dir, string outDir, string name, JsonData actionDatas) {

            HashSet<string> allFiels = new HashSet<string>();
            foreach (string VARIABLE in Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly)) {
                allFiels.Add(Path.GetFileNameWithoutExtension(VARIABLE));
            }

            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            Dictionary<string, int> frameDict = new Dictionary<string, int>();

            for (int forward = 0; forward < 5; forward++) {
                string[] keys = actionDatas.Keys.ToArray();
                foreach (string key in keys) {
                    JsonData actData = actionDatas[key];
                    List<string> list = new List<string>();
                    for (int i = (int) actData[1]; i <= (int) actData[2]; i++) {
                        string fileName = forward + i.ToString("D4");
                        if (!allFiels.Contains(fileName)) {
                            Logger.LogError("不存在文件 => " + fileName + " 忽略动作类型 " + key);
                            goto NEXT;
                        }
                        list.Add(dir + fileName + ".png");
                    }
                    string dictName = name + "_" + forward + key;
                    dict.Add(dictName, list);
                    frameDict.Add(dictName, (int)actData[0]);
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
