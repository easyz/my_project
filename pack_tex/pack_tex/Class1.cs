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

//        class FrameData {
//            public int x;
//            public int y;
//            public int w;
//            public int h;
//            public int offX;
//            public int offY;
//            public int sourceW;
//            public int sourceH;
//        }
//
//        class FrameJson {
//            public string file;
//            public Dictionary<string, FrameData> frames;
//        }

        class FrameData2 {
            public int x;
            public int y;
            public int w;
            public int h;
        }

        class FrameData3 {
            public int w;
            public int h;
        }

        class FrameData {
            public string filename;
            public FrameData2 frame;
            public bool rotated;
            public bool trimmed;
            public FrameData2 spriteSourceSize;
            public FrameData3 sourceSize;
        }

        class FrameJson {
            public FrameData[] frames;
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

            List<string> list = new List<string>(frameFiles);
            list.Sort((lhs, rhs) => {
                int lhsID;
                int.TryParse(lhs, out lhsID);

                int rhsID;
                int.TryParse(rhs, out rhsID);

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

            foreach (FrameData data in jsonData.frames) {
                string realName = Path.GetFileNameWithoutExtension(data.filename);
                framesList.Add("\t\t\t{\"res\":\"$name$\",\"x\":$x$,\"y\":$y$}"
                    .Replace("$name$", realName)
                    .Replace("$x$", (data.spriteSourceSize.x - (data.sourceSize.w >> 1)).ToString())
                    .Replace("$y$", (data.spriteSourceSize.y - (data.sourceSize.h >> 1)).ToString()));

                    resList.Add("\t\"$name$\":{\"x\":$x$,\"y\":$y$,\"w\":$w$,\"h\":$h$}"
                        .Replace("$name$", realName)
                        .Replace("$x$", data.frame.x + "")
                        .Replace("$y$", data.frame.y + "")
                        .Replace("$w$", data.frame.w + "")
                        .Replace("$h$", data.frame.h + ""));
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

            string[] frameFiles = Directory.GetFiles(handleFile, "*", SearchOption.TopDirectoryOnly);
            return HandleFiles(frameFiles, outPath, Path.GetFileNameWithoutExtension(handleFile), frameRate, checkRepeat);
        }

//        void RunTP(string inFiles, string outPath) {
//            System.Diagnostics.Process exep = new System.Diagnostics.Process();
//            string path = GetAssemblyPath();
//
//            exep.StartInfo.FileName = this.GetFileByLib("texture_merger\\TextureMerger.exe");
//            exep.StartInfo.Arguments = string.Format("-p {0} -o {1}.json", inFiles, outPath);
//            //            exep.StartInfo.CreateNoWindow = true;
//            exep.StartInfo.CreateNoWindow = false;
//            exep.StartInfo.UseShellExecute = false;
//            exep.Start();
//            exep.WaitForExit();//关键，等待外部程序退出后才能往下执行
//        }

        void RunTP(string inFiles, string outPath) {
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            string path = GetAssemblyPath();

            string[] argsArray = new[] {
                "--disable-rotation",
                "--size-constraints AnySize",
                "--max-width 2048",
                "--max-height 2048",
                "--format json-array",
//                "--png-opt-level 1"
            };
            string args = string.Join(" ", argsArray);
            string outImg = outPath + ".png";
            string outJson = outPath + ".json";

            exep.StartInfo.FileName = this.GetFileByLib("texturepack\\bin\\TexturePacker.exe");
            exep.StartInfo.Arguments = string.Format("{0} --data {1} --sheet {2} {3}", args, outJson, outImg, inFiles);
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