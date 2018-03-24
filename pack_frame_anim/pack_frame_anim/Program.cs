using System;
using System.Collections;
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
            bool isLow = exportData.Keys.Contains("low");
            if (exportData["action"].Keys.Contains("")) {
                foreach (string s in allDir) {
                    SingleFile2(s + "\\", outBaseDir + exportData["outPath"] + "\\", Path.GetFileNameWithoutExtension(s), exportData["action"][""], isLow);
                }
            } else {
                foreach (string s in allDir) {
                    SingleFile(s + "\\", outBaseDir + exportData["outPath"] + "\\", Path.GetFileNameWithoutExtension(s), exportData["action"], isLow);
                }
            }

            Console.WriteLine("Finish!!!");
            Console.ReadKey();

        }

        static string GetFileName(int id) {
            return id.ToString("D4");
        }

        static List<string> GetFrameList(HashSet<string> allFiels, string dir, int forward, IDictionary jsonData) {
            List<string> all = new List<string>();
            foreach (string key in jsonData.Keys) {
                JsonData actData = (JsonData) jsonData[key];

                List<string> list = new List<string>();
                for (int i = (int) actData[1]; i <= (int) actData[2]; i++) {
                    string fileName = forward + GetFileName(i);
                    if (!allFiels.Contains(fileName)) {
                        if (list.Count < 1) {
                            Logger.LogError("不存在文件 => " + fileName + " 忽略动作类型 " + key);
                        } else {
                            Logger.LogError("不存在文件 => " + fileName + " 忽略动作 " + key + "帧 => " + i);
                        }
//                        list.Clear();
                        goto NEXT;
                    }
                    list.Add(dir + fileName + ".png");
                }
                NEXT:
                all.AddRange(list);
            }
            return all;
        }

        static JsonData GetFrameList2(HashSet<string> allFiels, int forward, JsonData actData) {
            JsonData jsonData = new JsonData();
            jsonData["frameRate"] = 0;

            JsonData list = new JsonData();
            list.SetJsonType(JsonType.Array);
            for (int i = (int) actData[1]; i <= (int) actData[2]; i++) {
                string fileName = forward + GetFileName(i);
                if (!allFiels.Contains(fileName)) {
                    goto NEXT;
                }
                JsonData d = new JsonData();
                d["res"] = fileName;
                d["x"] = 0;
                d["y"] = 0;
                list.Add(d);
            }
            NEXT:
            ;
            if (list.Count > 0) {
                jsonData["frameRate"] = actData[0];
            }
            jsonData["frames"] = list;
            return jsonData;
        }

        static void SingleFile(string dir, string outDir, string name, JsonData actionDatas, bool lowQuality) {

            HashSet<string> allFiels = new HashSet<string>();
            foreach (string VARIABLE in Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly)) {
                allFiels.Add(Path.GetFileNameWithoutExtension(VARIABLE));
            }

            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            Dictionary<string, int> frameDict = new Dictionary<string, int>();
            Dictionary<string, JsonData> subFrame = new Dictionary<string, JsonData>();

            for (int forward = 0; forward < 5; forward++) {
                string[] keys = actionDatas.Keys.ToArray();
                foreach (string key in keys) {
                    JsonData actData = actionDatas[key];
                    IDictionary idict = actData;
                    int rate = 8;
                    JsonData sub = null;
                    if (actData.IsArray) {
                        rate = (int) actData[0];
                        idict = new Dictionary<string, JsonData>() {{key, actData}};
                    } else {

                        foreach (string subKey in actData.Keys) {
                            if (sub == null) {
                                sub = new JsonData();
                            }
                            sub[subKey] = GetFrameList2(allFiels, forward, actData[subKey]);
                        } 
                    }
                    List<string> list = GetFrameList(allFiels, dir, forward, idict);
                    if (list.Count > 0) {
                        string dictName = name + "_" + forward + key;
                        dict.Add(dictName, list);
                        frameDict.Add(dictName, rate);
                        if (sub != null) {
                            subFrame[dictName] = sub;
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, List<string>> pair in dict) {
                //                Console.WriteLine(pair.Key + "  " + string.Join(", ", pair.Value.ToArray()));
                Handle(pair.Value.ToArray(), outDir, pair.Key, frameDict[pair.Key], lowQuality);
                if (subFrame.ContainsKey(pair.Key)) {
                    string jsonPath = outDir + "/" + pair.Key + ".json";
                    if (File.Exists(jsonPath)) {
                        JsonData jsonData = LitJson.JsonMapper.ToObject(File.ReadAllText(jsonPath));
                        JsonData mcJsonData = jsonData["mc"][pair.Key];
                        Dictionary<string, JsonData> jsonDict = new Dictionary<string, JsonData>();
                        foreach (JsonData VARIABLE in mcJsonData["frames"]) {
                            jsonDict[(string) VARIABLE["res"]] = VARIABLE;
                        }
                        JsonData subJsonData = subFrame[pair.Key];
                        foreach (string key in subJsonData.Keys) {
                            foreach (JsonData VARIABLE in subJsonData[key]["frames"]) {
                                VARIABLE["x"] = jsonDict[(string) VARIABLE["res"]]["x"];
                                VARIABLE["y"] = jsonDict[(string) VARIABLE["res"]]["y"];
                            }
                        }
                        jsonData["mc"] = subJsonData;
                        File.WriteAllText(jsonPath, LitJson.JsonMapper.ToJson(jsonData)); 
                    } else {
                        Console.WriteLine("[ERROR] not json file => " + jsonPath);
                    }
                }
            }

            Console.WriteLine(">> 输出 " + name);
        }

        static void SingleFile2(string dir, string outDir, string name, JsonData actData, bool lowQuality) {
            
            HashSet<string> allFiels = new HashSet<string>();
            foreach (string VARIABLE in Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly)) {
                allFiels.Add(Path.GetFileNameWithoutExtension(VARIABLE));
            }

            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            Dictionary<string, int> frameDict = new Dictionary<string, int>();

            List<string> list = new List<string>();
            for (int i = (int) actData[1]; i <= (int) actData[2]; i++) {
                string fileName = "0" + i.ToString("D4");
                if (!allFiels.Contains(fileName)) {
                    Logger.LogError("不存在文件 => " + fileName + " 忽略待机类型 ");
                    return;
                }
                list.Add(dir + fileName + ".png");
            }
            string dictName = name;
            dict.Add(dictName, list);
            frameDict.Add(dictName, (int)actData[0]);

            foreach (KeyValuePair<string, List<string>> pair in dict) {
                Handle(pair.Value.ToArray(), outDir, pair.Key, frameDict[pair.Key], lowQuality);
            }

            Console.WriteLine(">> 输出 " + name);
        }

        static void Handle(string[] handleFiles, string outDir, string outName, int frame, bool low) {
            MergerTex fileTool = new MergerTex();
            if (fileTool.HandleFrame(handleFiles, outDir, outName, frame, low)) {
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
