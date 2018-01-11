using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using LitJson;

namespace convert_tilemap_game {
    class Program {

        private const string JSON_CONFIG = @"{
	// 地图数据根目录
	""tilemap_dir"": ""E:\\lycq\\resource\\tilemap\\map_data"",

	// 输出路径
	""out_dir"": ""E:\\lycq\\config\\map_data"",

	// 是否对地图进行切片
	// ""slice"" true,

	// 需要解析的地图数据
	""convert_file"": [""map001""]
}
";

        private const int SIZE = 256;
        private const string CONFIG_NAME = "convert_tilemap_conf.json";

        static void Main(string[] args) {
            Start(args);
            Console.ReadKey();
        }

        static void Start(string[] args) {
            string path = null;
            if (args.Length == 0) {
                path = CONFIG_NAME;
            } else {
                path = args[0];
            }

            if (!File.Exists(path)) {
                File.WriteAllText(CONFIG_NAME, JSON_CONFIG);
                Console.WriteLine("自动生成配置文件，请填写相关数据 >> " + CONFIG_NAME);
                return;
//                if (args.Length == 0) {
//                    Console.WriteLine("请指定配置文件 [" + CONFIG_ANME + "] 路径");
//                } else {
//                    Console.WriteLine(path + " 文件不存在");
//                }
//                return;
            }

            bool autoFind = false;
            if (args.Length > 1) {
                bool.TryParse(args[1], out autoFind);
            }

            bool autoSlice = false;
            if (args.Length > 2) {
                bool.TryParse(args[2], out autoSlice);
            }

//            try {
                JsonData jsonData = JsonMapper.ToObject(File.ReadAllText(path));
                string baseDir = Directory.GetCurrentDirectory() + "\\" + jsonData["tilemap_dir"].ToString();
                string outDir = Directory.GetCurrentDirectory() + "\\" + jsonData["out_dir"].ToString();
                Console.WriteLine(baseDir);
                Console.WriteLine(outDir);
                if (!Directory.Exists(outDir)) {
                    Directory.CreateDirectory(outDir);
                }
                bool slice = autoSlice || jsonData.Keys.Contains("slice") && (bool)jsonData["slice"];

                string[] convertFiles = new string[0];
                if (autoFind) {
                    string[] files = Directory.GetFiles(baseDir, "*.tmx", SearchOption.TopDirectoryOnly);
                    List<string> list = new List<string>();
                    foreach (string file in files) {
                        list.Add(Path.GetFileNameWithoutExtension(file)); 
                    }
                    convertFiles = list.ToArray();
                } else {
                    JsonData files = jsonData["convert_file"];
                    List<string> list = new List<string>();
                    for (int i = 0; i < files.Count; i++) {
                        list.Add(files[i].ToString());
                    }
                    convertFiles = list.ToArray();
                }
                for (int i = 0; i < convertFiles.Length; i++) {
                    string name = convertFiles[i].Trim();
                    string convertPath = Path.Combine(baseDir, name + ".tmx");
                    if (!File.Exists(convertPath)) {
                        Console.WriteLine(convertPath + " 文件不存在");
                        continue;
                    }
                    XmlDocument doc = new XmlDocument();
                    doc.Load(convertPath);

                    string imgPath;
//                    byte[] bytes = Parser(doc.DocumentElement, out imgPath);
                    string bytes = Parser(doc.DocumentElement, baseDir, out imgPath);
                    if (bytes != null) {
                        string pdir = Path.Combine(outDir, name);
                        if (!Directory.Exists(pdir)) {
                            Directory.CreateDirectory(pdir);
                        }
//                        File.WriteAllBytes(Path.Combine(pdir, "mdata.txt"), bytes);
                        File.WriteAllText(Path.Combine(pdir, "mdata.txt"), bytes);
                        Console.WriteLine("Convert Suc >> " + convertPath);

                        if (slice) {
                            string pidir = Path.Combine(pdir, "image");
                            if (!Directory.Exists(pidir)) {
                                Directory.CreateDirectory(pidir);
                            }
                            SliceBitmap(Path.Combine(baseDir, imgPath), pidir);
                        }
                    } else {
                        Console.WriteLine("Convert Error >> " + convertPath);
                    }
                }

//            } catch (Exception e) {
//                Console.WriteLine(e);
//            }

            //            XmlDocument doc = new XmlDocument();
            //            doc.Load("C:\\Users\\Administrator\\Desktop\\00\\00.tmx");
            //
            //            Parser(doc.DocumentElement);
            Console.WriteLine(">> 任务结束");
        }


        static string Parser(XmlElement xmlElement, string baseDir, out string inImgPath) {
            inImgPath = null;
            if (xmlElement == null || xmlElement.Name != "map") {
                Console.WriteLine("XML文件格式错误 => " + (xmlElement == null ? "NULL" : xmlElement.Name));
                return null;
            }

            int mapWidth = 0;
            int mapHeight = 0;
            int tileWidht = 0;
            int tileHeight = 0;
            foreach (XmlAttribute attribute in xmlElement.Attributes) {
                switch (attribute.Name) {
                    case "width":
                        mapWidth = Convert.ToInt32(attribute.Value);
                        break;
                    case "height":
                        mapHeight = Convert.ToInt32(attribute.Value);
                        break;
                    case "tilewidth":
                        tileWidht = Convert.ToInt32(attribute.Value);
                        break;
                    case "tileheight":
                        tileHeight = Convert.ToInt32(attribute.Value);
                        break;
                }
            }

            if (mapWidth == 0 || mapHeight == 0 || tileWidht != 64 || tileHeight != 64) {
                Console.WriteLine("参数错误 => " + mapWidth + " " + mapHeight + " " + tileWidht + " " + tileHeight);
                return null;
            }

            Dictionary<string, List<XmlNode>> dict = new Dictionary<string, List<XmlNode>>();
            foreach (XmlNode xmlNode in xmlElement.ChildNodes) {
                if (dict.ContainsKey(xmlNode.Name)) {
                    dict[xmlNode.Name].Add(xmlNode);
                } else {
                    dict[xmlNode.Name] = new List<XmlNode>() { xmlNode };
                }
            }

            if (!dict.ContainsKey("imagelayer")) {
                Console.WriteLine("没有找到图像层");
                return null;
            }


            XmlNode bgXmlNode = null;
            foreach (XmlNode xmlNode in dict["imagelayer"]) {
                foreach (XmlAttribute attribute in xmlNode.Attributes) {
                    if (attribute.Value == "背景") {
                        bgXmlNode = xmlNode;
                        break;
                    }
                }
            }

            if (bgXmlNode == null) {
                Console.WriteLine("没有找到背景层");
                return null;
            }

            string imgPath = string.Empty;
//            int imgWidth = 0;
//            int imgHeight = 0;
            foreach (XmlAttribute attribute in bgXmlNode.ChildNodes[0].Attributes) {
                switch (attribute.Name) {
                    case "source":
                        imgPath = attribute.Value;
                        break;
//                    case "width":
//                        imgWidth = Convert.ToInt32(attribute.Value);
//                        break;
//                    case "height":
//                        imgHeight = Convert.ToInt32(attribute.Value);
//                        break;
                }
            }

            if (string.IsNullOrEmpty(imgPath)) {
                Console.WriteLine("图片参数错误");
                return null;
            }

            inImgPath = imgPath;

            Image img = Image.FromFile(Path.Combine(baseDir, imgPath));
            Bitmap mImage = new Bitmap(img);

            int width = mImage.Width;
            int height = mImage.Height;
            int imgWidth = width;
            int imgHeight = height;
            mImage.Dispose();

            Layer waklableLayer = new Layer();
            Layer hiddentLayer = new Layer();
            Layer layer = new Layer();
            Layer curLayer = layer;

            foreach (XmlNode xmlNode in dict["layer"]) {
                foreach (XmlAttribute attribute in xmlNode.Attributes) {
                    switch (attribute.Name) {
                        case "name":
                            if (attribute.Value == "碰撞（红色）") {
                                curLayer = waklableLayer;
                            } else if (attribute.Value == "遮挡（蓝色）") {
                                curLayer = hiddentLayer;
                            } else {
                                curLayer = layer;
                                Console.WriteLine("未定义的层 " + attribute.Value);
                            }
                            curLayer.name = attribute.Value;
                            break;
                        case "width":
                            curLayer.widht = Convert.ToInt32(attribute.Value);
                            break;
                        case "height":
                            curLayer.height = Convert.ToInt32(attribute.Value);
                            break;
                    }
                }
                curLayer.values = Array.ConvertAll(xmlNode.ChildNodes[0].InnerText.Split(','), Convert.ToInt32);
            }

            int realWidth = (int)Math.Ceiling(imgWidth * 1.0f / 64.0f);
            int realHeight = (int)Math.Ceiling(imgHeight * 1.0f / 64.0f);

            int[] array = new int[realWidth * realHeight];
            ParserLayer(waklableLayer, array, 0, realWidth, realHeight);
            ParserLayer(hiddentLayer, array, 1, realWidth, realHeight);

            MapData mapData = new MapData();
            mapData.width = imgWidth;
            mapData.height = imgHeight;
            mapData.realWidth = realWidth;
            mapData.realHeight = realHeight;
            List<int> list = new List<int>();
            for (int j = 0; j < realHeight; j++) {
                for (int i = 0; i < realWidth; i++) {
                    list.Add((byte)array[j * realWidth + i]);
                }
            }
            mapData.datas = list.ToArray();

//            List<TempData> groups = 
            mapData.groups = Array.ConvertAll<TempData, Dictionary<string, ObjGroupData>>(ParserMonPoint(dict).ToArray(), input => input.data);
            mapData.pkgroups = Array.ConvertAll<TempData, Dictionary<string, ObjGroupData>>(ParserPkPoint(dict).ToArray(), input => input.data);
            TempData tempData = ParserBossPoint(dict);
            if (tempData != null) {
                mapData.bossgroups = tempData.data;
            } else {
                mapData.bossgroups = new Dictionary<string, ObjGroupData>();
            }

            return LitJson.JsonMapper.ToJson(mapData);
        }

        private static TempData CreateTempData(XmlNode xmlNode) {
            Dictionary<string, ObjGroupData> datas = new Dictionary<string, ObjGroupData>();
            for (int i = 0, len = xmlNode.ChildNodes.Count; i < len; ++i) {
                XmlNode node = xmlNode.ChildNodes[i];
                Dictionary<string, string> attr = GetAttr(node);
                ObjGroupData data = new ObjGroupData();
                if (attr.ContainsKey("gid")) {
                    data.type = int.Parse(attr["gid"]);
                    data.x = (int)(Math.Floor((float.Parse(attr["x"]) + float.Parse(attr["width"]) * 0.5) / 64.0));
                    data.y = (int)(Math.Floor((float.Parse(attr["y"]) - float.Parse(attr["height"]) * 0.5) / 64.0));
                } else {
                    data.type = 9999;
                    data.points = GetPolyLine(node);
                    data.x = (int)(Math.Floor((float.Parse(attr["x"])) / 64.0));
                    data.y = (int)(Math.Floor((float.Parse(attr["y"])) / 64.0));
                }
                datas[data.type + ""] = data;
            }
            return new TempData { data = datas};
        }

        private static Point[] GetPolyLine(XmlNode xmlNode) {
            if (!xmlNode.HasChildNodes) {
                return null;
            }
            XmlAttribute attr = xmlNode.ChildNodes[0].Attributes["points"];
            if (attr == null) {
                return null;
            }
            string[] value = attr.Value.Split(' ');
            List<Point> list = new List<Point>();
            foreach (string s in value) {
                string[] pos = s.Split(',');
                Point point = new Point();
                point.x = (int)Math.Floor(float.Parse(pos[0]) / 64.0);
                point.y = (int)Math.Floor(float.Parse(pos[1]) / 64.0);
                list.Add(point);
            }
            return list.ToArray();

//            for (int i = 0, len = xmlNode.ChildNodes.Count; i < len; ++i) {
//                XmlNode node = xmlNode.ChildNodes[i];
//                Dictionary<string, string> attr = GetAttr(node);
//                Point data = new Point();
//                data.x = (int)(Math.Floor((float.Parse(attr["x"]) + float.Parse(attr["width"]) * 0.5) / 64.0));
//                data.y = (int)(Math.Floor((float.Parse(attr["y"]) - float.Parse(attr["height"]) * 0.5) / 64.0));
//                datas[data.type + ""] = data;
//            }
        }

        private static List<TempData> ParserMonPoint(Dictionary<string, List<XmlNode>> dict) {
            List<TempData> groups = new List<TempData>();
            if (dict.ContainsKey("objectgroup")) {
                foreach (XmlNode xmlNode in dict["objectgroup"]) {
                    int index = 0;
                    foreach (XmlAttribute attribute in xmlNode.Attributes) {
                        switch (attribute.Name) {
                            case "name":
                                Match match = new Regex(@"刷怪点(\d+)").Match(attribute.Value);
                                if (match.Success) {
                                    index = int.Parse(match.Groups[1].Value);
                                } else {
                                    goto NEXT;
                                }
                                break;
                        }
                    }
                    TempData tempData = CreateTempData(xmlNode);
                    tempData.index = index;
                    groups.Add(tempData);
                    NEXT:
                    ;
                }
            }
            groups.Sort((lhs, rhs) => {
                return lhs.index - rhs.index;
            });
            return groups;
        }

        private static TempData ParserBossPoint(Dictionary<string, List<XmlNode>> dict) {
            if (dict.ContainsKey("objectgroup")) {
                foreach (XmlNode xmlNode in dict["objectgroup"]) {
                    foreach (XmlAttribute attribute in xmlNode.Attributes) {
                        switch (attribute.Name) {
                            case "name":
                                if (attribute.Value == "BOSS") {
                                        
                                } else {
                                    goto NEXT;
                                }
                                break;
                        }
                    }
                    TempData tempData = CreateTempData(xmlNode);
                    return tempData;
                    NEXT:
                    ;
                }
            }
            return null;
        }

        private static List<TempData> ParserPkPoint(Dictionary<string, List<XmlNode>> dict) {
            List<TempData> groups = new List<TempData>();
            if (dict.ContainsKey("objectgroup")) {
                foreach (XmlNode xmlNode in dict["objectgroup"]) {
                    int index = 0;
                    foreach (XmlAttribute attribute in xmlNode.Attributes) {
                        switch (attribute.Name) {
                            case "name":
                                Match match = new Regex(@"遭遇点(\d+)").Match(attribute.Value);
                                if (match.Success) {
                                    index = int.Parse(match.Groups[1].Value);
                                } else {
                                    goto NEXT;
                                }
                                break;
                        }
                    }
                    TempData tempData = CreateTempData(xmlNode);
                    tempData.index = index;
                    groups.Add(tempData);
                    NEXT:
                    ;
                }
            }
            groups.Sort((lhs, rhs) => {
                return lhs.index - rhs.index;
            });
            return groups;
        }
        private static Dictionary<string, string> GetAttr(XmlNode xmlNode) {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (XmlAttribute attribute in xmlNode.Attributes) {
                dict[attribute.Name] = attribute.Value;
            }
            return dict;
        }

        class MapData {
            public int width;
            public int height;
            public int realHeight;
            public int realWidth;
            public int[] datas;
            public Dictionary<string, ObjGroupData>[] groups;
            public Dictionary<string, ObjGroupData>[] pkgroups;
            public Dictionary<string, ObjGroupData> bossgroups;
        }

        class ObjGroupData {
            public int x;
            public int y;
            public int type;
            public Point[] points;
        }

        class Point {
            public int x;
            public int y;
        }

        class TempData {
            public int index;
            public Dictionary<string, ObjGroupData> data;
        }

        static void WriteShort(BinaryWriter bw, UInt16 value) {
            byte[] bytes = BitConverter.GetBytes(value);
            bw.Write(bytes[1]);
            bw.Write(bytes[0]);
        }

        static void WriteInt(BinaryWriter bw, int value) {
            byte[] bytes = BitConverter.GetBytes(value);
            bw.Write(bytes[3]);
            bw.Write(bytes[2]);
            bw.Write(bytes[1]);
            bw.Write(bytes[0]);
        }

        static void ParserLayer(Layer layer, int[] array, int offset, int realWidth, int realHeight) {
            for (int i = 0; i < realWidth; i++) {
                for (int j = 0; j < realHeight; j++) {
                    int index = j * layer.widht + i;
                    if (layer.values[index] != 0) {
                        array[j * realWidth + i] |= 1 << offset;
                    }
                }
            }
        }

        class Layer {
            public string name;
            public int widht;
            public int height;
            public int[] values = new int[0];

            public override string ToString() {
                return string.Format("Name: {0}, Widht: {1}, Height: {2}, Values: {3}", name, widht, height, values.Length);
            }
        }

        static void SliceBitmap(string imgPath, string saveDir) {
            if (Directory.Exists(saveDir)) {
                foreach (var VARIABLE in Directory.GetFiles(saveDir)) {
                    File.Delete(VARIABLE);
                }
                foreach (var VARIABLE in Directory.GetDirectories(saveDir)) {
                    Directory.Delete(VARIABLE); 
                }
            } else {
                Directory.CreateDirectory(saveDir);
            }

            Image img = Image.FromFile(imgPath);
            Bitmap mImage = new Bitmap(img);

            int width = (int)Math.Ceiling(mImage.Width * 1.0f / SIZE);
            int height = (int)Math.Ceiling(mImage.Height * 1.0f / SIZE);

            for (int j = 0; j < height; j++) {
                for (int i = 0; i < width; i++) {
                    CreateChunkBitmap(mImage, saveDir, i, j);
                }
            }

            string smallPath = Path.Combine(Path.GetDirectoryName(saveDir), "small.jpg");
            Image smallImage = PictureProcess(img, 64, 64);
            smallImage.Save(smallPath, ImageFormat.Jpeg);

            Image pkImage = PictureProcess(img, 445, 445);
            pkImage.Save(Path.Combine(Path.GetDirectoryName(saveDir), "pk_preview.jpg"), ImageFormat.Jpeg);

            img.Dispose();
        }

        static void CreateChunkBitmap(Bitmap image, string saveDir, int chunkX, int chunkY) {
            Bitmap bitmap = new Bitmap(256, 256, PixelFormat.Format24bppRgb);
            bitmap.SetResolution(72, 72);

            for (int j = 0; j < SIZE; j++) {
                for (int i = 0; i < SIZE; i++) {
                    int x = chunkX * SIZE + i;
                    int y = chunkY * SIZE + j;
                    if (x >= image.Width) {
                        bitmap.SetPixel(i, j, Color.Black);
                    } else if (y >= image.Height) {
                        bitmap.SetPixel(i, j, Color.Black);
                    } else {
                        bitmap.SetPixel(i, j, image.GetPixel(x, y));
                    }
                }
            }

            string savePath = Path.Combine(saveDir, GetTileName(chunkX, chunkY) + ".jpg");
            Console.WriteLine(savePath);

            var p = GetEncoderInfo("image/jpeg");
            var encoder = new EncoderParameters(1);
            encoder.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 50L);
            bitmap.Save(savePath, p, encoder);
//            bitmap.Save(savePath, ImageFormat.Jpeg);
        }

        static string GetTileName(int x, int y) {
            return y + "_" + x;
        }
        private static ImageCodecInfo GetEncoderInfo(String mimeType) {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j) {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }

        static Image PictureProcess(Image sourceImage, int targetWidth, int targetHeight) {
            int width;//图片最终的宽  
            int height;//图片最终的高  
            try {
                System.Drawing.Imaging.ImageFormat format = sourceImage.RawFormat;
//                Bitmap targetPicture = new Bitmap(targetWidth, targetHeight);
//                targetPicture.SetResolution(72, 72);
//                Graphics g = Graphics.FromImage(targetPicture);
//                g.Clear(Color.White);

                //计算缩放图片的大小  
                if (sourceImage.Width > targetWidth && sourceImage.Height <= targetHeight) {
                    width = targetWidth;
                    height = (width * sourceImage.Height) / sourceImage.Width;
                } else if (sourceImage.Width <= targetWidth && sourceImage.Height > targetHeight) {
                    height = targetHeight;
                    width = (height * sourceImage.Width) / sourceImage.Height;
                } else if (sourceImage.Width <= targetWidth && sourceImage.Height <= targetHeight) {
                    width = sourceImage.Width;
                    height = sourceImage.Height;
                } else {
                    width = targetWidth;
                    height = (width * sourceImage.Height) / sourceImage.Width;
                    if (height > targetHeight) {
                        height = targetHeight;
                        width = (height * sourceImage.Width) / sourceImage.Height;
                    }
                }
//                g.DrawImage(sourceImage, (targetWidth - width) / 2, (targetHeight - height) / 2, width, height);
                Bitmap targetPicture = new Bitmap(width, height);
                targetPicture.SetResolution(72, 72);
                Graphics g = Graphics.FromImage(targetPicture);
                g.DrawImage(sourceImage, 0, 0, width, height);

                return targetPicture;
            } catch (Exception ex) {
                Console.Write(ex);
            }
            return null;
        }
    }
}
