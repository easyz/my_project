using System;
using System.Collections.Generic;
//using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using LitJson;

namespace merger_tex {
    public class MergerTex: IFileTool {

        public bool Handle(string[] handleFiles, string outPath) {

            foreach (string handleFile in handleFiles) {
                Logger.Log(">>> start <<< " + handleFile);
//                Match match = Regex.Match(Path.GetFileNameWithoutExtension(handleFile), @".*\((\d+)\)");
//                if (!match.Success) {
//                    Logger.LogError(handleFile + " 没有设置动画帧数");
//                    continue;
//                }
//                string frameRate = match.Groups[1].Value;

//                new MergerTexUtil(InitDllPath()).Handle(handleFile, outPath, int.Parse(frameRate));
                string outDir = Path.Combine(outPath, Path.GetFileNameWithoutExtension(handleFile), "image");
                if (!Directory.Exists(outDir)) {
                    Directory.CreateDirectory(outDir);
                }
                TileMapUtil.SliceBitmap(handleFile, outDir);
            }


            return true;
        }

        public List<string> GetFiles(string dir) {
            List<string> allSearchFiles = Directory.GetFiles(dir, "*", SearchOption.TopDirectoryOnly).ToList();
            return allSearchFiles;
        }

        public static string InitDllPath() {
            if (Util.IsDebug()) {
                return "..\\..\\..\\..\\libs\\";
            }
            return "..\\libs\\";
        }
    }
}