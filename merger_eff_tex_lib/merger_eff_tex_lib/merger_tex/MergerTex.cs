using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using merger_eff_tex_lib;
using packTex;

namespace merger_tex {
    public class MergerTex: IFileTool {

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

        public bool HandleFrame(string[] frameFiles, string outPath, string outName, int frameRate) {
            if (!Directory.Exists(outPath)) {
                Directory.CreateDirectory(outPath);
            }

//            Dictionary<string, Rect> sizeDict = new Dictionary<string, Rect>();
//            Dictionary<string, string> tempDict = new Dictionary<string, string>();
//            List<CopyImgData> copyFrameFiles = new List<CopyImgData>(frameFiles.Length);


            MergerTexUtil obj = new packTex.MergerTexUtil(MainInit.InitDllPath());
            return obj.HandleFiles(frameFiles, outPath, outName, frameRate);

            /*
            List<Texture> list = new List<Texture>(frameFiles.Length);
            foreach (string frameFile in frameFiles) {
                string frameName = Path.GetFileNameWithoutExtension(frameFile);


//                new packTex.MergerTexUtil(MainInit.InitDllPath()).Handle(handleFiles, outDir)

//                int id;
//                if (!int.TryParse(frameName, out id)) {
//                    Logger.LogError(frameFile + " 命名错误");
//                    return false;
//                }
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
//            copyFrameFiles.Sort((lhs, rhs) => {
//                int lhsID;
//                int.TryParse(lhs.mName, out lhsID);
//
//                int rhsID;
//                int.TryParse(rhs.mName, out rhsID);
//
//                return lhsID - rhsID;
//            });
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

            string name = outName;
            name = name.Replace("(" + frameRate + ")", string.Empty).Trim();
            string pngPath = outPath + "\\" + name + ".png";
            string jsonPath = outPath + "\\" + name + ".json";
            Bitmap outbitmap = AtlasMaker.Trimming(tex.bitmap);
            outbitmap.SetResolution(72, 72);
            outbitmap.Save(pngPath);

            string content =
                FORMAT_STR.Replace("$name$", name)
                    .Replace("$frameRate$", frameRate.ToString())
                    .Replace("$frames$", string.Join(",\n", framesList.ToArray()))
                    .Replace("$res$", string.Join(",\n", resList.ToArray()));
            File.WriteAllText(jsonPath, content);

            Logger.Log("输出 >>> " + pngPath);
            Logger.Log("输出 >>> " + jsonPath);

            return true;*/
        }

        public bool Handle(string[] handleFiles, string outPath) {
            return false;
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
        }*/

        public List<string> GetFiles(string dir) {
            List<string> allSearchFiles = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly).ToList();
            return allSearchFiles;
        }
    }
}