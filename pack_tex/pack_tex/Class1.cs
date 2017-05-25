using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace packTex {
    public class MergerTexUtil {

        const string FORMAT_STR = @"{""mc"":{
	""$name$"":{
		""frameRate"":$frameRate$,
		""frames"":[
$frames$
		]
}},
""res"":{
$res$
}}";

        private struct CopyImgData {
            public string mName;
            public string mCopyName;
        }

        class FrameData {
            public int x;
            public int y;
            public int w;
            public int h;
            public int offX;
            public int offY;
            public int sourceW;
            public int sourceH;
        }

        class FrameJson {
            public string file;
            public Dictionary<string, FrameData> frames;
        }

        class ClipData {
            public int x;
            public int y;
            public int w;
            public int h;
        }

        class ClipFrame {
            public string res;
            public int x;
            public int y;
        }

        class ClipMc {
            public int frameRate;
            public ClipFrame[] frames;
        }

        private string m_WorkPath;
        public MergerTexUtil(string workPath) {
            this.m_WorkPath = workPath;
        }

        public bool HandleFiles(string[] frameFiles, string outPath, string fileName, int frameRate = 12, bool checkRepeat = true) {
            Logger.Log("frameRate = " + frameRate);
            List<CopyImgData> copyFrameFiles = new List<CopyImgData>(frameFiles.Length);
            Dictionary<string, string> tempDict = new Dictionary<string, string>();

            List<string> list = new List<string>(frameFiles.Length);
            for (int i = 0; i < frameFiles.Length; i++) {
                string frameFile = frameFiles[i];
                string frameName = Path.GetFileNameWithoutExtension(frameFile);
                int id;
                if (!int.TryParse(frameName, out id)) {
                    Logger.LogError(frameFile + " 命名错误");
                    return false;
                }
                string md5 = checkRepeat ? i.ToString() : GetMD5HashFromFile(frameFile);
                if (tempDict.ContainsKey(md5)) {
                    copyFrameFiles.Add(new CopyImgData() {
                        mName = frameName,
                        mCopyName = tempDict[md5]
                    });
                } else {
                    tempDict[md5] = frameName;
                    copyFrameFiles.Add(new CopyImgData() {
                        mName = frameName,
                        mCopyName = null,
                    });
                    Logger.Log("parser " + frameFile);
                    list.Add("\"" + frameFile + "\"");
                }
            }

            copyFrameFiles.Sort((lhs, rhs) => {
                int lhsID;
                int.TryParse(lhs.mName, out lhsID);

                int rhsID;
                int.TryParse(rhs.mName, out rhsID);

                return lhsID - rhsID;
            });

            string allFrameFileStr = string.Join(" ", list.ToArray());

            string tempDir = Directory.GetCurrentDirectory() + "\\temp";
            string tempFileName = tempDir + "\\temp";
            if (!Directory.Exists(tempDir)) {
                Directory.CreateDirectory(tempDir);
            } else {
                string[] files = Directory.GetFiles(tempDir);
                foreach (string file in files) {
                    File.Delete(file);
                }
            }
            RunTP(allFrameFileStr, tempFileName);

            string jsonFile = tempFileName + ".json";
            string pngFile = tempFileName + ".png";
            if (!File.Exists(jsonFile) || !File.Exists(pngFile)) {
                Logger.Warn("文件错误 => " + allFrameFileStr);
                return false;
            }

            FrameJson jsonData = LitJson.JsonMapper.ToObject<FrameJson>(File.ReadAllText(jsonFile));

            List<string> framesList = new List<string>();
            List<string> resList = new List<string>();
            foreach (CopyImgData copyImgData in copyFrameFiles) {
                string realName = string.IsNullOrEmpty(copyImgData.mCopyName)
                    ? copyImgData.mName
                    : copyImgData.mCopyName;
                FrameData data = jsonData.frames[realName];
                if (data == null) {
                    Logger.LogError("foreach list error => " + copyImgData.mName);
                    continue;
                }
                framesList.Add("\t\t\t{\"res\":\"$name$\",\"x\":$x$,\"y\":$y$}"
                    .Replace("$name$", realName)
                    .Replace("$x$", -Math.Round(data.sourceW * 0.5 - data.offX) + "")
                    .Replace("$y$", -Math.Round(data.sourceH * 0.5 - data.offY) + ""));
                if (string.IsNullOrEmpty(copyImgData.mCopyName)) {
                    resList.Add("\t\"$name$\":{\"x\":$x$,\"y\":$y$,\"w\":$w$,\"h\":$h$}"
                        .Replace("$name$", realName)
                        .Replace("$x$", data.x + "")
                        .Replace("$y$", data.y + "")
                        .Replace("$w$", data.w + "")
                        .Replace("$h$", data.h + ""));
                }

            }

            string name = fileName;
                name = name.Replace("(" + frameRate + ")", string.Empty).Trim();
            string pngPath = outPath + "\\" + name + ".png";
            string jsonPath = outPath + "\\" + name + ".json";
            if (!Directory.Exists(Path.GetDirectoryName(pngPath))) {
                Directory.CreateDirectory(Path.GetDirectoryName(pngPath));
            }
            File.Copy(pngFile, pngPath, true);

            string content =
                FORMAT_STR.Replace("$name$", name)
                    .Replace("$frameRate$", frameRate + "")
                    .Replace("$frames$", string.Join(",\n", framesList.ToArray()))
                    .Replace("$res$", string.Join(",\n", resList.ToArray()));
            File.WriteAllText(jsonPath, content);

            Logger.Log("输出 >>> " + pngPath);
            Logger.Log("输出 >>> " + jsonPath);

            return true;
        }

        public bool Handle(string handleFile, string outPath, int frameRate = 12, bool checkRepeat = true) {

//            Logger.Log("frameRate = " + frameRate);
            string[] frameFiles = Directory.GetFiles(handleFile, "*", SearchOption.TopDirectoryOnly);
            return HandleFiles(frameFiles, outPath, Path.GetFileNameWithoutExtension(handleFile), frameRate, checkRepeat);
//            List<CopyImgData> copyFrameFiles = new List<CopyImgData>(frameFiles.Length);
//            Dictionary<string, string> tempDict = new Dictionary<string, string>();
//
//            List<string> list = new List<string>(frameFiles.Length);
//            for (int i = 0; i < frameFiles.Length; i++) {
//                string frameFile = frameFiles[i];
//                string frameName = Path.GetFileNameWithoutExtension(frameFile);
//                int id;
//                if (!int.TryParse(frameName, out id)) {
//                    Logger.LogError(frameFile + " 命名错误");
//                    return false;
//                }
//                string md5 = checkRepeat ? i.ToString() : GetMD5HashFromFile(frameFile);
//                if (tempDict.ContainsKey(md5)) {
//                    copyFrameFiles.Add(new CopyImgData() {
//                        mName = frameName,
//                        mCopyName = tempDict[md5]
//                    });
//                } else {
//                    tempDict[md5] = frameName;
//                    copyFrameFiles.Add(new CopyImgData() {
//                        mName = frameName,
//                        mCopyName = null,
//                    });
//                    Logger.Log("parser " + frameFile);
//                    list.Add("\"" + frameFile + "\"");
//                }
//            }
//
//            copyFrameFiles.Sort((lhs, rhs) => {
//                int lhsID;
//                int.TryParse(lhs.mName, out lhsID);
//
//                int rhsID;
//                int.TryParse(rhs.mName, out rhsID);
//
//                return lhsID - rhsID;
//            });
//
//            string allFrameFileStr = string.Join(" ", list.ToArray());
//
//            string tempDir = Directory.GetCurrentDirectory() + "\\temp";
//            string tempFileName = tempDir + "\\temp";
//            if (!Directory.Exists(tempDir)) {
//                Directory.CreateDirectory(tempDir);
//            } else {
//                string[] files = Directory.GetFiles(tempDir);
//                foreach (string file in files) {
//                    File.Delete(file);
//                }
//            }
//            RunTP(allFrameFileStr, tempFileName);
//
//            string jsonFile = tempFileName + ".json";
//            string pngFile = tempFileName + ".png";
//            if (!File.Exists(jsonFile) || !File.Exists(pngFile)) {
//                Logger.Warn("文件错误 => " + allFrameFileStr);
//                return false;
//            }
//
//            FrameJson jsonData = LitJson.JsonMapper.ToObject<FrameJson>(File.ReadAllText(jsonFile));
//
//            List<string> framesList = new List<string>();
//            List<string> resList = new List<string>();
//            foreach (CopyImgData copyImgData in copyFrameFiles) {
//                string realName = string.IsNullOrEmpty(copyImgData.mCopyName)
//                    ? copyImgData.mName
//                    : copyImgData.mCopyName;
//                FrameData data = jsonData.frames[realName];
//                if (data == null) {
//                    Logger.LogError("foreach list error => " + copyImgData.mName);
//                    continue;
//                }
//                framesList.Add("\t\t\t{\"res\":\"$name$\",\"x\":$x$,\"y\":$y$}"
//                    .Replace("$name$", realName)
//                    .Replace("$x$", -Math.Round(data.sourceW * 0.5 - data.offX) + "")
//                    .Replace("$y$", -Math.Round(data.sourceH * 0.5 - data.offY) + ""));
//                if (string.IsNullOrEmpty(copyImgData.mCopyName)) {
//                    resList.Add("\t\"$name$\":{\"x\":$x$,\"y\":$y$,\"w\":$w$,\"h\":$h$}"
//                        .Replace("$name$", realName)
//                        .Replace("$x$", data.x + "")
//                        .Replace("$y$", data.y + "")
//                        .Replace("$w$", data.w + "")
//                        .Replace("$h$", data.h + ""));
//                }
//
//            }
//
//            string name = Path.GetFileNameWithoutExtension(handleFile);
//            name = name.Replace("(" + frameRate + ")", string.Empty).Trim();
//            string pngPath = outPath + "\\" + name + ".png";
//            string jsonPath = outPath + "\\" + name + ".json";
//            if (!Directory.Exists(Path.GetDirectoryName(pngPath))) {
//                Directory.CreateDirectory(Path.GetDirectoryName(pngPath));
//            }
//            File.Copy(pngFile, pngPath, true);
//
//            string content =
//                FORMAT_STR.Replace("$name$", name)
//                    .Replace("$frameRate$", frameRate + "")
//                    .Replace("$frames$", string.Join(",\n", framesList.ToArray()))
//                    .Replace("$res$", string.Join(",\n", resList.ToArray()));
//            File.WriteAllText(jsonPath, content);
//
//            Logger.Log("输出 >>> " + pngPath);
//            Logger.Log("输出 >>> " + jsonPath);

            return true;
        }

        //    public List<string> GetFiles(string dir) {
        //        List<string> allSearchFiles = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly).ToList();
        //        return allSearchFiles;
        //    }

        void RunTP(string inFiles, string outPath) {
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            string path = GetAssemblyPath();

            exep.StartInfo.FileName = this.GetFileByLib("texture_merger\\TextureMerger.exe");
            exep.StartInfo.Arguments = string.Format("-p {0} -o {1}.json", inFiles, outPath);
            //            exep.StartInfo.CreateNoWindow = true;
            exep.StartInfo.CreateNoWindow = false;
            exep.StartInfo.UseShellExecute = false;
            exep.Start();
            exep.WaitForExit();//关键，等待外部程序退出后才能往下执行
        }

        private static string GetAssemblyPath() {
            string _CodeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;

            _CodeBase = _CodeBase.Substring(8, _CodeBase.Length - 8);    // 8是file:// 的长度  

            string[] arrSection = _CodeBase.Split(new char[] { '/' });

            string _FolderPath = "";
            for (int i = 0; i < arrSection.Length - 1; i++) {
                _FolderPath += arrSection[i] + "/";
            }

            return _FolderPath;
        }

        public static string GetMD5HashFromFile(string fileName) {
            try {
                FileStream file = new FileStream(fileName, FileMode.Open);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++) {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            } catch (Exception ex) {
                throw new Exception("GetMD5HashFromFile() fail, error:" + ex.Message);
            }
        }

        //    public const string LIB_DIR = "..\\libs\\";
        const string LIB_DIR = "..\\";

        string GetFileByLib(string fileName) {
            if (string.IsNullOrEmpty(this.m_WorkPath)) {
                return Path.Combine(LIB_DIR, fileName);
            }
            return Path.Combine(this.m_WorkPath, fileName);
        }
    }
}