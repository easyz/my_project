using System;
using System.Collections.Generic;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using LitJson;
using packTex;

namespace merger_tex {
    public class MergerTex: IFileTool {

//        const string FORMAT_STR = @"{""mc"":{
//	""$name$"":{
//		""frameRate"":$frameRate$,
//		""frames"":[
//$frames$
//		]
//}},
//""res"":{
//$res$
//}}";

//        private struct CopyImgData {
//            public string mName;
//            public string mCopyName;
//        }
//
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
//
//        class ClipData {
//            public int x;
//            public int y;
//            public int w;
//            public int h;
//        }
//
//        class ClipFrame {
//            public string res;
//            public int x;
//            public int y;
//        }
//
//        class ClipMc {
//            public int frameRate;
//            public ClipFrame[] frames;
//        }

        public bool Handle(string[] handleFiles, string outPath) {

            foreach (string handleFile in handleFiles) {
                Logger.Log(">>> start <<< " + handleFile);
                Match match = Regex.Match(Path.GetFileNameWithoutExtension(handleFile), @".*\((\d+)\)");
                if (!match.Success) {
                    Logger.LogError(handleFile + " 没有设置动画帧数");
                    continue;
                }
                string frameRate = match.Groups[1].Value;

                new MergerTexUtil(InitDllPath()).Handle(handleFile, outPath, int.Parse(frameRate));

                /*
                Logger.Log("frameRate = " + frameRate);
//                Dictionary<string, Rect> sizeDict = new Dictionary<string, Rect>();
                string[] frameFiles = Directory.GetFiles(handleFile, "*", SearchOption.TopDirectoryOnly);
                List<CopyImgData> copyFrameFiles = new List<CopyImgData>(frameFiles.Length);
                Dictionary<string, string> tempDict = new Dictionary<string, string>();

                List<string> list = new List<string>(frameFiles.Length);
                foreach (string frameFile in frameFiles) {
                    string frameName = Path.GetFileNameWithoutExtension(frameFile);
                    int id;
                    if (!int.TryParse(frameName, out id)) {
                        Logger.LogError(frameFile + " 命名错误");
                        return false;
                    }
                    string md5 = Util.GetMD5HashFromFile(frameFile);
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

//                        Image image = Image.FromFile(frameFile);
//                        Bitmap bitmap = new Bitmap(image, image.Width, image.Height);
//                        Texture texture = new Texture();
//                        texture.bitmap = bitmap;
//                        texture.name = frameName;
//                        sizeDict[texture.name] = new Rect(0, 0, image.Width, image.Height);
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
                    Logger.LogError("文件错误 => " + allFrameFileStr);
                    return false;
                }

                FrameJson jsonData = LitJson.JsonMapper.ToObject<FrameJson>(File.ReadAllText(jsonFile));
                foreach (KeyValuePair<string, FrameData> VARIABLE in jsonData.frames) {
                    FrameData data = VARIABLE.Value;
                    int halfW = (int) Math.Round(data.sourceW * 0.5);
                    int halfH = (int) Math.Round(data.sourceH * 0.5);
                    int fx = -(halfW - data.offX);
                    int fy = -(halfH - data.offY);

                }


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

                string name = Path.GetFileNameWithoutExtension(handleFile);
                name = name.Replace("(" + frameRate + ")", string.Empty).Trim();
                string pngPath = outPath + "\\" + name + ".png";
                string jsonPath = outPath + "\\" + name + ".json";
                if (!Directory.Exists(Path.GetDirectoryName(pngPath))) {
                    Directory.CreateDirectory(Path.GetDirectoryName(pngPath));
                }
                File.Copy(pngFile, pngPath, true);
                
                string content =
                    FORMAT_STR.Replace("$name$", name)
                        .Replace("$frameRate$", frameRate)
                        .Replace("$frames$", string.Join(",\n", framesList.ToArray()))
                        .Replace("$res$", string.Join(",\n", resList.ToArray()));
                File.WriteAllText(jsonPath, content);
                
                Logger.Log("输出 >>> " + pngPath);
                Logger.Log("输出 >>> " + jsonPath);*/


                //                Console.WriteLine(allFrameFileStr);

                //                List<AtlasMaker.SpriteEntry> spriteEntries = AtlasMaker.CreateSprites(list);
                //                Texture tex = new Texture(4, 4);
                //                AtlasMaker.PackTextures(tex, spriteEntries);
                //
                //                List<string> resList = new List<string>();
                //                List<string> framesList = new List<string>();
                //                foreach (CopyImgData copyImgData in copyFrameFiles) {
                //                    string realName = string.IsNullOrEmpty(copyImgData.mCopyName)
                //                        ? copyImgData.mName
                //                        : copyImgData.mCopyName;
                //                    AtlasMaker.SpriteEntry spriteEntry = spriteEntries.Find(entry => entry.name == realName);
                //                    if (spriteEntry == null) {
                //                        Logger.LogError("foreach list error => " + copyImgData.mName);
                //                        continue;
                //                    }
                //                    Rect oldSize = sizeDict[spriteEntry.name];
                //                    framesList.Add("\t\t\t{\"res\":\"$name$\",\"x\":$x$,\"y\":$y$}"
                //                        .Replace("$name$", realName)
                //                        .Replace("$x$", spriteEntry.paddingLeft - oldSize.width * 0.5f + "")
                //                        .Replace("$y$", spriteEntry.paddingBottom - oldSize.height * 0.5f + ""));
                //                    if (string.IsNullOrEmpty(copyImgData.mCopyName)) {
                //                        resList.Add("\t\"$name$\":{\"x\":$x$,\"y\":$y$,\"w\":$w$,\"h\":$h$}"
                //                            .Replace("$name$", spriteEntry.name)
                //                            .Replace("$x$", spriteEntry.x.ToString())
                //                            .Replace("$y$", tex.height - spriteEntry.height - spriteEntry.y + "")
                //                            .Replace("$w$", spriteEntry.width.ToString())
                //                            .Replace("$h$", spriteEntry.height.ToString())
                //                            );
                //                    }
                //                }
                //
                //                string name = Path.GetFileNameWithoutExtension(handleFile);
                //                name = name.Replace("(" + frameRate + ")", string.Empty).Trim();
                //                string pngPath = outPath + "\\" + name + ".png";
                //                string jsonPath = outPath + "\\" + name + ".json";
                //                AtlasMaker.Trimming(tex.bitmap).Save(pngPath);
                //
                //                string content =
                //                    FORMAT_STR.Replace("$name$", name)
                //                        .Replace("$frameRate$", frameRate)
                //                        .Replace("$frames$", string.Join(",\n", framesList.ToArray()))
                //                        .Replace("$res$", string.Join(",\n", resList.ToArray()));
                //                File.WriteAllText(jsonPath, content);
                //
                //                Logger.Log("输出 >>> " + pngPath);
                //                Logger.Log("输出 >>> " + jsonPath);
            }


            return true;
        }

        /*
        public bool Handle(string[] handleFiles, string outPath) {

            foreach (string handleFile in handleFiles) {
                Logger.Log(">>> start <<< " + handleFile);
                Match match = Regex.Match(Path.GetFileNameWithoutExtension(handleFile), @".*\((\d+)\)");
                if (!match.Success) {
                    Logger.LogError(handleFile + " 没有设置动画帧数");
                    continue;
                }
                string frameRate = match.Groups[1].Value;
                Logger.Log("frameRate = " + frameRate); 
                Dictionary<string, Rect> sizeDict = new Dictionary<string, Rect>();
                string[] frameFiles = Directory.GetFiles(handleFile, "*", SearchOption.TopDirectoryOnly);
                List<CopyImgData> copyFrameFiles = new List<CopyImgData>(frameFiles.Length);
                Dictionary<string, string> tempDict = new Dictionary<string, string>();

                List<Texture> list = new List<Texture>(frameFiles.Length);
                foreach (string frameFile in frameFiles) {
                    string frameName = Path.GetFileNameWithoutExtension(frameFile);
                    int id;
                    if (!int.TryParse(frameName, out id)) {
                        Logger.LogError(frameFile + " 命名错误");
                        return false;
                    }
                    string md5 = Util.GetMD5HashFromFile(frameFile);
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

                        Image image = Image.FromFile(frameFile);
                        Bitmap bitmap = new Bitmap(image, image.Width, image.Height);
                        Texture texture = new Texture();
                        texture.bitmap = bitmap;
                        texture.name = frameName;
                        sizeDict[texture.name] = new Rect(0, 0, image.Width, image.Height);
                        Logger.Log("parser " + frameFile);
                        list.Add(texture);
                    }
                }
                copyFrameFiles.Sort((lhs, rhs) => {
                    int lhsID;
                    int.TryParse(lhs.mName, out lhsID);

                    int rhsID;
                    int.TryParse(rhs.mName, out rhsID);

                    return lhsID - rhsID;
                });
                List<AtlasMaker.SpriteEntry> spriteEntries = AtlasMaker.CreateSprites(list);
                Texture tex = new Texture(4, 4);
                AtlasMaker.PackTextures(tex, spriteEntries);

                List<string> resList = new List<string>();
                List<string> framesList = new List<string>();
                foreach (CopyImgData copyImgData in copyFrameFiles) {
                    string realName = string.IsNullOrEmpty(copyImgData.mCopyName)
                        ? copyImgData.mName
                        : copyImgData.mCopyName;
                    AtlasMaker.SpriteEntry spriteEntry = spriteEntries.Find(entry => entry.name == realName);
                    if (spriteEntry == null) {
                        Logger.LogError("foreach list error => " + copyImgData.mName);
                        continue;
                    }
                    Rect oldSize = sizeDict[spriteEntry.name];
                    framesList.Add("\t\t\t{\"res\":\"$name$\",\"x\":$x$,\"y\":$y$}"
                        .Replace("$name$", realName)
                        .Replace("$x$", spriteEntry.paddingLeft - oldSize.width * 0.5f + "")
                        .Replace("$y$", spriteEntry.paddingBottom - oldSize.height * 0.5f + ""));
                    if (string.IsNullOrEmpty(copyImgData.mCopyName)) {
                        resList.Add("\t\"$name$\":{\"x\":$x$,\"y\":$y$,\"w\":$w$,\"h\":$h$}"
                            .Replace("$name$", spriteEntry.name)
                            .Replace("$x$", spriteEntry.x.ToString())
                            .Replace("$y$", tex.height - spriteEntry.height - spriteEntry.y + "")
                            .Replace("$w$", spriteEntry.width.ToString())
                            .Replace("$h$", spriteEntry.height.ToString())
                            );
                    }
                }

                string name = Path.GetFileNameWithoutExtension(handleFile);
                name = name.Replace("(" + frameRate + ")", string.Empty).Trim();
                string pngPath = outPath + "\\" + name + ".png";
                string jsonPath = outPath + "\\" + name + ".json";
                AtlasMaker.Trimming(tex.bitmap).Save(pngPath);

                string content =
                    FORMAT_STR.Replace("$name$", name)
                        .Replace("$frameRate$", frameRate)
                        .Replace("$frames$", string.Join(",\n", framesList.ToArray()))
                        .Replace("$res$", string.Join(",\n", resList.ToArray()));
                File.WriteAllText(jsonPath, content);

                Logger.Log("输出 >>> " + pngPath);
                Logger.Log("输出 >>> " + jsonPath);
            }
            

            return true;
        }
        */
        public List<string> GetFiles(string dir) {
            List<string> allSearchFiles = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly).ToList();
            return allSearchFiles;
        }

        public static void RunTP(string inFiles, string outPath) {
            System.Diagnostics.Process exep = new System.Diagnostics.Process();
            exep.StartInfo.FileName = Const.GetFileByLib("texture_merger\\TextureMerger.exe");
            exep.StartInfo.Arguments = string.Format("-p {0} -o {1}.json", inFiles, outPath);
//            exep.StartInfo.CreateNoWindow = true;
            exep.StartInfo.CreateNoWindow = false;
            exep.StartInfo.UseShellExecute = false;
            exep.Start();
            exep.WaitForExit();//关键，等待外部程序退出后才能往下执行
        }

        public static string InitDllPath() {
            if (Util.IsDebug()) {
                return "..\\..\\..\\..\\libs\\";
            }
            return "..\\libs\\";
        }
    }
}