using System;
using System.Collections.Generic;
using System.Drawing;

namespace merger_tex {
    public class Texture {
        public Bitmap bitmap;
        public string name;

        public Texture() {
        }

        public Texture(int newWidth, int newHeight) {
            bitmap = new Bitmap(newWidth, newHeight);
        }

        public int width {
            get { return bitmap.Width; }
        }

        public int height {
            get { return bitmap.Height; }
        }

        public Color[] GetPixels() {
            int w = bitmap.Width;
            int h = bitmap.Height;
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    pixels[j * w + i] = bitmap.GetPixel(i, j);
                }
            }
            return pixels;
        }

        public void SetPixels(Color[] pixels) {
            int w = bitmap.Width;
            int h = bitmap.Height;
            for (int i = 0; i < w; i++) {
                for (int j = 0; j < h; j++) {
                    bitmap.SetPixel(i, j, pixels[j * w + i]);
                }
            }
        }

        internal void SetPixels(int x, int y, int setW, int setH, Color[] colors) {
            for (int i = 0; i < setW; i++) {
                for (int j = 0; j < setH; j++) {
                    bitmap.SetPixel(x + i, y + j, colors[j * setW + i]);
                }
            }
        }

        public void Resize(int newW, int newH) {
            Bitmap newBitmap = new Bitmap(newW, newH);
            for (int i = 0; i < newW; i++) {
                for (int j = 0; j < newH; j++) {
                    if (i < bitmap.Width && j < bitmap.Height) {
                        newBitmap.SetPixel(i, j, bitmap.GetPixel(i, j));        
                    }
                }
            }
            bitmap = newBitmap;
        }
    }

    public class AtlasMaker {

        public class SpriteEntry {
            public Texture tex;
            public bool temporaryTexture = false;

            public string name = "Sprite";
            public int x = 0;
            public int y = 0;
            public int width = 0;
            public int height = 0;

            public int borderLeft = 0;
            public int borderRight = 0;
            public int borderTop = 0;
            public int borderBottom = 0;

            public int paddingLeft = 0;
            public int paddingRight = 0;
            public int paddingTop = 0;
            public int paddingBottom = 0;
            
            public bool hasBorder { get { return (borderLeft | borderRight | borderTop | borderBottom) != 0; } }
            
            public bool hasPadding { get { return (paddingLeft | paddingRight | paddingTop | paddingBottom) != 0; } }
            
            public void SetRect(int x, int y, int width, int height) {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }
            
            public void SetPadding(int left, int bottom, int right, int top) {
                paddingLeft = left;
                paddingBottom = bottom;
                paddingRight = right;
                paddingTop = top;
            }
            
            public void SetBorder(int left, int bottom, int right, int top) {
                borderLeft = left;
                borderBottom = bottom;
                borderRight = right;
                borderTop = top;
            }

            public override string ToString() {
                return $"Tex: {tex}, TemporaryTexture: {temporaryTexture}, Name: {name}, X: {x}, Y: {y}, Width: {width}, Height: {height}, BorderLeft: {borderLeft}, BorderRight: {borderRight}, BorderTop: {borderTop}, BorderBottom: {borderBottom}, PaddingLeft: {paddingLeft}, PaddingRight: {paddingRight}, PaddingTop: {paddingTop}, PaddingBottom: {paddingBottom}, hasBorder: {hasBorder}, hasPadding: {hasPadding}";
            }

            /// <summary>
            /// Copy all values of the specified sprite data.
            /// </summary>

//            public void CopyFrom(UISpriteData sd) {
//                name = sd.name;
//
//                x = sd.x;
//                y = sd.y;
//                width = sd.width;
//                height = sd.height;
//
//                borderLeft = sd.borderLeft;
//                borderRight = sd.borderRight;
//                borderTop = sd.borderTop;
//                borderBottom = sd.borderBottom;
//
//                paddingLeft = sd.paddingLeft;
//                paddingRight = sd.paddingRight;
//                paddingTop = sd.paddingTop;
//                paddingBottom = sd.paddingBottom;
//            }
//
//            /// <summary>
//            /// Copy the border information from the specified sprite.
//            /// </summary>
//
//            public void CopyBorderFrom(UISpriteData sd) {
//                borderLeft = sd.borderLeft;
//                borderRight = sd.borderRight;
//                borderTop = sd.borderTop;
//                borderBottom = sd.borderBottom;
//            }
        }

        public static Bitmap Trimming(Bitmap bitmap) {
            Texture oldTex = new Texture();
            oldTex.bitmap = bitmap;

            Color[] pixels = oldTex.GetPixels();

            int xmin = oldTex.bitmap.Width;
            int xmax = 0;
            int ymin = oldTex.bitmap.Height;
            int ymax = 0;
            int oldWidth = oldTex.bitmap.Width;
            int oldHeight = oldTex.bitmap.Height;

            for (int y = 0, yw = oldHeight; y < yw; ++y) {
                for (int x = 0, xw = oldWidth; x < xw; ++x) {
                    Color c = pixels[y * xw + x];

                    if (c.A != 0) {
                        if (y < ymin) ymin = y;
                        if (y > ymax) ymax = y;
                        if (x < xmin) xmin = x;
                        if (x > xmax) xmax = x;
                    }
                }
            }

            int newWidth = (xmax - xmin) + 1;
            int newHeight = (ymax - ymin) + 1;

            if (newWidth > 0 && newHeight > 0) {
                SpriteEntry sprite = new SpriteEntry();
                sprite.x = 0;
                sprite.y = 0;
                sprite.width = oldTex.width;
                sprite.height = oldTex.height;

                if (newWidth == oldWidth && newHeight == oldHeight) {
                    sprite.tex = oldTex;
                    sprite.name = oldTex.name;
                    sprite.temporaryTexture = false;
                } else {
                    Color[] newPixels = new Color[newWidth * newHeight];

                    for (int y = 0; y < newHeight; ++y) {
                        for (int x = 0; x < newWidth; ++x) {
                            int newIndex = y * newWidth + x;
                            int oldIndex = (ymin + y) * oldWidth + (xmin + x);
                            newPixels[newIndex] = pixels[oldIndex];
                        }
                    }

                    // Create a new texture
                    sprite.temporaryTexture = true;
                    sprite.name = oldTex.name;
                    sprite.tex = new Texture(newWidth, newHeight);
                    sprite.tex.SetPixels(newPixels);

                    return sprite.tex.bitmap;
                }
            }

            return bitmap;
        }

        public static List<SpriteEntry> CreateSprites(List<Texture> textures) {
            List<SpriteEntry> list = new List<SpriteEntry>();
            foreach (Texture oldTex in textures) {
                Color[] pixels = oldTex.GetPixels();

                int xmin = oldTex.bitmap.Width;
                int xmax = 0;
                int ymin = oldTex.bitmap.Height;
                int ymax = 0;
                int oldWidth = oldTex.bitmap.Width;
                int oldHeight = oldTex.bitmap.Height;

                for (int y = 0, yw = oldHeight; y < yw; ++y) {
                    for (int x = 0, xw = oldWidth; x < xw; ++x) {
                        Color c = pixels[y * xw + x];

                        if (c.A != 0) {
                            if (y < ymin) ymin = y;
                            if (y > ymax) ymax = y;
                            if (x < xmin) xmin = x;
                            if (x > xmax) xmax = x;
                        }
                    }
                }

                int newWidth = (xmax - xmin) + 1;
                int newHeight = (ymax - ymin) + 1;

                if (newWidth > 0 && newHeight > 0) {
                    SpriteEntry sprite = new SpriteEntry();
                    sprite.x = 0;
                    sprite.y = 0;
                    sprite.width = oldTex.width;
                    sprite.height = oldTex.height;

                    if (newWidth == oldWidth && newHeight == oldHeight) {
                        sprite.tex = oldTex;
                        sprite.name = oldTex.name;
                        sprite.temporaryTexture = false;
                    }
                    else {
                        Color[] newPixels = new Color[newWidth*newHeight];

                        for (int y = 0; y < newHeight; ++y) {
                            for (int x = 0; x < newWidth; ++x) {
                                int newIndex = y*newWidth + x;
                                int oldIndex = (ymin + y)*oldWidth + (xmin + x);
                                newPixels[newIndex] = pixels[oldIndex];
                            }
                        }

                        // Create a new texture
                        sprite.temporaryTexture = true;
                        sprite.name = oldTex.name;
                        sprite.tex = new Texture(newWidth, newHeight);
                        sprite.tex.SetPixels(newPixels);
//                        sprite.tex.Apply();

                        // Remember the padding offset
                        sprite.SetPadding(xmin, ymin, oldWidth - newWidth - xmin, oldHeight - newHeight - ymin);
                    }
                    list.Add(sprite);
                } else {
                    int PADDING = 2;
                    SpriteEntry sprite = new SpriteEntry();
                    sprite.x = 0;
                    sprite.y = 0;
                    sprite.width = PADDING;
                    sprite.height = PADDING;
                    sprite.name = oldTex.name;
                    sprite.tex = new Texture(2, 2);
                    sprite.SetPadding(RoundToInt((xmin - PADDING) * 0.5f),RoundToInt((ymin - PADDING) * 0.5f),0,0);
                    list.Add(sprite);
                }
            }
            return list;
        }

        public static bool PackTextures(Texture tex, List<SpriteEntry> sprites) {
            Texture[] textures = new Texture[sprites.Count];
            Rect[] rects;

		    int maxSize = 1024;
            int padding = 2;

            Logger.Log("<<< start merger");
            sprites.Sort(Compare);
            for (int i = 0; i < sprites.Count; ++i) textures[i] = sprites[i].tex;
            rects = UITexturePacker.PackTextures(tex, textures, 4, 4, padding, maxSize);
            Logger.Log("<<< end merger");
 

            for (int i = 0; i < sprites.Count; ++i) {
                Rect rect = ConvertToPixels(rects[i], tex.width, tex.height, true);

//                 Make sure that we don't shrink the textures
                if ((int)Math.Round((double) rect.width) != textures[i].width) return false;

                SpriteEntry se = sprites[i];
                se.x = (int) Math.Round((double) rect.x);
                se.y = (int) Math.Round((double)rect.y);
                se.width =(int) Math.Round((double)rect.width);
                se.height = (int) Math.Round((double)rect.height);
            }
            return true;
        }

        static int Compare(SpriteEntry a, SpriteEntry b) {
            // A is null b is not b is greater so put it at the front of the list
            if (a == null && b != null) return 1;

            // A is not null b is null a is greater so put it at the front of the list
            if (a != null && b == null) return -1;

            // Get the total pixels used for each sprite
            int aPixels = a.width * a.height;
            int bPixels = b.width * b.height;

            if (aPixels > bPixels) return -1;
            else if (aPixels < bPixels) return 1;
            return 0;
        }

        public static int RoundToInt(float f) {
            return (int)Math.Round((double)f);
        }

        static public Rect ConvertToPixels(Rect rect, int width, int height, bool round) {
            Rect final = rect;

            if (round) {
                final.xMin = RoundToInt(rect.xMin * width);
                final.xMax = RoundToInt(rect.xMax * width);
                final.yMin = RoundToInt((1f - rect.yMax) * height);
                final.yMax = RoundToInt((1f - rect.yMin) * height);
            } else {
                final.xMin = rect.xMin * width;
                final.xMax = rect.xMax * width;
                final.yMin = (1f - rect.yMax) * height;
                final.yMax = (1f - rect.yMin) * height;
            }
            return final;
        }
    }
}