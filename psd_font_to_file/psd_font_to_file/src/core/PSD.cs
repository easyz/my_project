using System;
using System.IO;

namespace PsdParser
{
    public sealed class PSD
    {
        public string filePath;
        public string fileName;
        public PSDHeaderInfo headerInfo;
        public PSDColorModeInfo colorModeInfo;
        public PSDResolutionInfo resolutionInfo;
        public PSDDisplayInfo displayInfo;
        public PSDLayerInfo layerInfo;

        public void loadHeader(string filePath)
        {
            this.filePath = filePath;
            int num = filePath.LastIndexOfAny("/\\".ToCharArray());
            this.fileName = num < 0 ? filePath : filePath.Substring(num + 1);
            Console.WriteLine(File.Exists(filePath));
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader br = new BinaryReader(fileStream))
                {
                    this.readHeader(br);
                    this.readColorMode(br);
                    this.readImageResource(br);
                    this.readLayers(br);
                }
            }
        }

        public void loadData()
        {
            if (this.layerInfo == null)
                throw new SystemException("Load header in the first");
            using (FileStream fileStream = new FileStream(this.filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader br = new BinaryReader((Stream)fileStream))
                    this.layerInfo.loadData(br, (int)this.headerInfo.bpp);
            }
        }

        private void readHeader(BinaryReader br)
        {
            this.headerInfo = new PSDHeaderInfo();
            this.headerInfo.load(br);
            if ((int)this.headerInfo.bpp != 8)
                throw new SystemException("For now, only Support 8 Bit Per Channel");
        }

        private void readColorMode(BinaryReader br)
        {
            this.colorModeInfo = new PSDColorModeInfo();
            this.colorModeInfo.load(br);
        }

        private void readImageResource(BinaryReader br)
        {
            int int32_1 = EndianReverser.getInt32(br);
            long position = br.BaseStream.Position;
            while (br.BaseStream.Position - position < (long)int32_1)
            {
                if (PSDUtil.readAscii(br, 4) == "8BIM")
                {
                    short int16 = EndianReverser.getInt16(br);
                    PSDUtil.readPascalString(br, 2);
                    int int32_2 = EndianReverser.getInt32(br);
                    if (int32_2 > 0)
                    {
                        switch (int16)
                        {
                            case 1005:
                                this.resolutionInfo = new PSDResolutionInfo();
                                this.resolutionInfo.load(br);
                                break;
                            case 1007:
                                this.displayInfo = new PSDDisplayInfo();
                                this.displayInfo.load(br);
                                break;
                            default:
                                br.BaseStream.Position += (long)int32_2;
                                break;
                        }
                        if (int32_2 % 2 != 0)
                            ++br.BaseStream.Position;
                    }
                }
            }
        }

        private void readLayers(BinaryReader br)
        {
            this.layerInfo = new PSDLayerInfo();
            this.layerInfo.loadHeader(br, (int)this.headerInfo.bpp);
        }
    }
}
