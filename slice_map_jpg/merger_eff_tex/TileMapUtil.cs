using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class TileMapUtil {

        private const int SIZE = 256;

        public static void SliceBitmap(string imgPath, string saveDir) {
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
