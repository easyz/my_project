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
            }


            return true;
        }

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