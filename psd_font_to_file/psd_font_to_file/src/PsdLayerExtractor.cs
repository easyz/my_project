using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using LitJson;
using PsdParser;
using Encoder = System.Drawing.Imaging.Encoder;

namespace psd {
    public class PsdLayerExtractor {
        public class Layer
        {
            public bool canLoadLayer = true;
            public PsdParser.PSDLayer psdLayer;
            public List<Layer> children = new List<Layer>();

            public bool isContainer
            {
                get { return this.psdLayer.groupStarted; }
            }
            public bool isTextLayer
            {
                get { return this.psdLayer.isTextLayer; }
            }
            public bool isImageLayer
            {
                get { return this.psdLayer.isImageLayer; }
            }

            public string name
            {
                get { return this.psdLayer.name; }
            }

            public string text
            {
                get;
                internal set;
            }

            public Layer()
            {
            }

            public Layer(PsdParser.PSDLayer psdLayer)
            {
                this.psdLayer = psdLayer;
                if (this.psdLayer.isTextLayer)
                    this.text = this.psdLayer.text.Replace('\r', '\n');
            }

            public void LoadData(BinaryReader br, int bpp)
            {
                var channelCount = this.psdLayer.channels.Length;
                for (var k = 0; k < channelCount; ++k)
                {
                    var channel = this.psdLayer.channels[k];
                    if (this.canLoadLayer && this.psdLayer.isImageLayer)
                        channel.loadData(br, bpp);
                }
            }
        }

        public class ImageFilePath
        {
            public string filePath;
            public string imageMd5;

            public ImageFilePath()
            {
            }
            public ImageFilePath(string info)
            {
                var arr = info.Split('=');

                this.filePath = arr[0];
                if (arr.Length > 1)
                    this.imageMd5 = arr[1];
            }
            public ImageFilePath(string filePath, string imageMd5)
            {
                this.filePath = filePath;
                this.imageMd5 = imageMd5;
            }

            public override string ToString()
            {
                return this.filePath + "=" + this.imageMd5;
            }
        }

        public PsdParser.PSD Psd { get; private set; }

        public string PsdFilePath { get; private set; }

        public string PsdMd5 { get; private set; }

        public Layer mRoot { get; private set; }

        public List<ImageFilePath> ImageFilePathes { get; private set; }

        public List<ImageFilePath> LastImageFilePathes { get; private set; }

        public PsdLayerExtractor(string info) {
            /*! 
             * 
             *  0: path
             *  1: md5
             *  2: add font boolean
             *  3: image file infos
            */

            var arr = info.Split(';');
            var filePath = arr[0];

            PsdFilePath = filePath;

            Psd = new PsdParser.PSD();
            Psd.loadHeader(filePath);

            PsdMd5 = this.CalcMd5();

            mRoot = new Layer();
            var psdLayers = Psd.layerInfo.layers;

            LoadPsdLayers(mRoot, psdLayers, psdLayers.Length - 1);
            mRoot.children.Reverse();

//            for (int i = mRoot.children.Count - 1; i >= 0; --i) {
//                if (mRoot.children[i].name != "panel") {
//                    mRoot.children.RemoveAt(i);
//                }
//            }

            if (mRoot.children.Count == 0) {
                Logger.Warn("导出psd文件错误 [" + filePath + "] 没有找panel组，或者panel组内容为空!!!!");
            }
        }

        public string CalcMd5()
        {
            using (var stream = File.OpenRead(this.PsdFilePath))
            {
                return CalcMd5Imple(stream);
            }
        }

        private string CalcMd5Imple(Stream s)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var bytes = md5.ComputeHash(s);
                var result = new StringBuilder(bytes.Length * 2);
                for (var i = 0; i < bytes.Length; ++i)
                    result.Append(bytes[i].ToString("x2"));

                return result.ToString();
            }
        }

        private int LoadPsdLayers(Layer parent, PsdParser.PSDLayer[] psdLayers, int i)
        {
            while (i >= 0)
            {
                var psdLayer = psdLayers[i--];
                if (psdLayer.groupStarted)
                {
                    var newParent = new Layer(psdLayer);
                    parent.children.Add(newParent);
                    i = this.LoadPsdLayers(newParent, psdLayers, i);

                    if (psdLayer.name.Contains("@ignore"))
                        parent.children.Remove(newParent);
                }
                else if (psdLayer.groupEnded)
                {
                    parent.children.Reverse();
                    break;
                }
                else if (!psdLayer.drop && !psdLayer.name.Contains("@ignore"))
                {
                    parent.children.Add(new Layer(psdLayer));
                }
            }
            return i;
        }

        public void SaveLayersToPNGs(string outPath = null) {
            var psdFilePath = this.PsdFilePath;
            string prePath;
            if (string.IsNullOrEmpty(outPath)) {
                prePath = psdFilePath.Substring(0, psdFilePath.Length - 4) + "_layers";
            } else {
                prePath = outPath + "\\" + Path.GetFileNameWithoutExtension(psdFilePath) + "_layers";
            }

            string[] dirs = prePath.Split('\\');
            if (dirs.Length < 2) {
                Console.WriteLine(prePath + " 目录层级过小");
                return;
            }

            if (Directory.Exists(prePath)) {
                string[] files = Directory.GetFiles(prePath, "*", SearchOption.TopDirectoryOnly);
                foreach (string file in files) {
                    if (File.Exists(file)) {
                        File.Delete(file);
                    }
                }
                string[] childdirs = Directory.GetDirectories(prePath, "*", SearchOption.TopDirectoryOnly);
                foreach (string childdir in childdirs) {
                    if (Directory.Exists(childdir)) {
                        Directory.Delete(childdir, true);
                    }
                }

//                Directory.Delete(prePath, true);
            }
            Directory.CreateDirectory(prePath);

            Logger.Log("Saving layers from '" + psdFilePath + "' save path '" + prePath + "'");

            this.ImageFilePathes = new List<ImageFilePath>();
            if (this.LastImageFilePathes == null)
                this.LastImageFilePathes = new List<ImageFilePath>();

             SaveLayersToPNGs_imple(prePath, this.mRoot.children);

            List<string> list = new List<string>(dirs);
            string rootName = list[list.Count - 2];
            string layerName = list[list.Count - 1];
            list.RemoveRange(list.Count - 2, 2);
            // 配置文件路径
            string configDirPath = string.Join("\\", list.ToArray());

//            SaveLayersConfig(configDirPath, rootName, layerName, saveFiles);
        }

        public string GetSingleText() {
            return this.mRoot.children[0].text;
        }
//        private void SaveLayersConfig(string configDirPath, string rootName, string layerName, SaveImgData[] saveFiles)
//        {
//            string configPath = configDirPath + "\\" + "default.res.json";
//
//            JsonData allData = new JsonData();
//            JsonData saveData = new JsonData();
//            if (File.Exists(configPath)) {
//                JsonData jsonData = JsonMapper.ToObject(File.ReadAllText(configPath));
//                var dictionary = jsonData as IDictionary;
//                if (dictionary.Contains("groups")) {
//                    allData["groups"] = jsonData["groups"];
//                }
//                JsonData resData = jsonData["resources"];
//                foreach (JsonData obj in resData) {
//                    string url = (string) obj["url"];
//                    if (!url.StartsWith(rootName + "/" + layerName)) {
//                        saveData.Add(obj);
//                    }
//                }
//            }
//
//            foreach (SaveImgData saveFile in saveFiles) {
//                JsonData jsondata = new JsonData();
//                jsondata["url"] = saveFile.path.Replace(configDirPath + "\\", string.Empty).Replace("\\", "/");
//                jsondata["type"] = "image";
//                jsondata["name"] = Path.GetFileName(saveFile.path).Replace(".", "_");
//                if (saveFile.slice != null) {
//                    jsondata["scale9grid"] = string.Format("{0},{1},{2},{3}", saveFile.slice.left, saveFile.slice.top, saveFile.slice.sliceWidth, saveFile.slice.sliceHeigth);
//                }
//                saveData.Add(jsondata);
//            }
//
//            allData["resources"] = saveData;
//
//            File.WriteAllText(configPath, allData.ToJson());
//        }

//        struct SaveImgData {
//            public string path;
//            public PsdLayerRect slice;
//        }

        private void SaveLayersToPNGs_imple(string prePath, List<Layer> layers)
        {
            using (var psdFileStream = new FileStream(this.PsdFilePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                foreach (var layer in layers) {
                    if (!layer.canLoadLayer) {
                        continue;
                    }

                    if (layer.isContainer) {
//                        saveFiles.AddRange(SaveLayersToPNGs_imple(prePath + "/" + layer.name, layer.children));
//                        saveFiles.AddRange(SaveLayersToPNGs_imple(prePath , layer.children));
                        continue;
                    }

                    var fileName = "sdfsdfdsf";
                    if (this.HasUnacceptibleChar(fileName)) {
                        Logger.LogError(fileName + " Contains wrong character '\\ / : * ? \" < > |' not allowed");
                        continue;
                    }

                    var filePath = prePath + "/" + fileName;
                    ImageFilePath newImageFilePath = null;
                    {
                        try {
                            psdFileStream.Position = 0;
                            var br = new BinaryReader(psdFileStream);
                            {
                                layer.LoadData(br, this.Psd.headerInfo.bpp);
                                newImageFilePath = new ImageFilePath(filePath, "pass");
                            }
                        }
                        catch (System.Exception e) {
                            Logger.LogError(e.Message);
                        }
                    }
                    this.ImageFilePathes.Add(newImageFilePath);

                    var data = layer.psdLayer.mergeChannels();
                    if (data == null)
                        continue;

                    Bitmap bitmap;
                    ImageFormat imageFormat;

                        Logger.Log("Saving texture '" + newImageFilePath.filePath + "'");
                        imageFormat = MakeTexture(ref data, out bitmap, layer);

                    //
                    if (bitmap != null) {
                        if (!System.IO.Directory.Exists(prePath))
                            System.IO.Directory.CreateDirectory(prePath);

//                        EncoderParameter encoderParameter = new EncoderParameter(, 100);
//                        EncoderParameters encoderParameters = new EncoderParameters(1);

//                        encoderParameters.Param[0] = encoderParameter;
//                        bitmap.Save(filePath, GetEncoderInfo(imageFormat == ImageFormat.Jpeg ? "image/jpeg" : "image/png"), encoderParameters);

                        bitmap.Save(filePath, imageFormat);
       
                    }
                }
            }
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for(j = 0; j < encoders.Length; ++j)
            {
                if(encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        
        private ImageFormat MakeTexture(ref byte[] data, out Bitmap bitmap, Layer layer) {
            var channelCount = layer.psdLayer.channels.Length;
            var pitch = layer.psdLayer.pitch;
            var w = layer.psdLayer.area.width;
            var h = layer.psdLayer.area.height;

            var format = layer.psdLayer.mImgType == PSDLayer.ImgType.JPG ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb;

            bitmap = new Bitmap(w, h, format);
            bitmap.SetResolution(72, 72);
//            Color[] colors = new Color[data.Length / channelCount];

            var k = 0;
            for (var y = h - 1; y >= 0; --y) {
                for (var x = 0; x < pitch; x += channelCount) {
                    var n = x + y * pitch;
                    byte a, r, g, b;
                    if (channelCount == 4) {
                        b = data[n++];
                        g = data[n++];
                        r = data[n++];
                        a = (byte)System.Math.Round(data[n++] / 255.0f * layer.psdLayer.opacity * 255.0f);
                    } else {
                        b = data[n++];
                        g = data[n++];
                        r = data[n++];
                        a = (byte)System.Math.Round(layer.psdLayer.opacity * 255.0f);
                    }
                    int index = k++;
//                    colors[index] = Color.FromArgb(a, r, g, b);
                    bitmap.SetPixel(index % w, h - 1 - index / w, Color.FromArgb(a, r, g, b));
                }
            }

            return channelCount == 3 ? ImageFormat.Jpeg : ImageFormat.Png;
        }

        public bool HasUnacceptibleChar(string str) {
            return str.IndexOfAny(new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' }) >= 0;
        }

#region 调试信息

        private void PrintLayers(string groupName, List<Layer> layers) {
            foreach (var layer in layers) {
                if (layer.isContainer)
                    this.PrintLayers(layer.name, layer.children);
                else {
                    var preName = groupName + (!string.IsNullOrEmpty(groupName) ? "/" : "");
//                    layer.canLoadLayer = GUILayout.Toggle(layer.canLoadLayer, "", GUILayout.Width(15));
//                    GUILayout.Label(preName + layer.name);
                    Logger.Log(preName + layer.name);
                }
            }
        }

        public void PrintLayers() {
            PrintLayers(string.Empty, mRoot.children);
        }

#endregion
    }
}