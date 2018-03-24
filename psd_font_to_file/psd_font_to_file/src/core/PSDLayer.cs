using System;
using System.IO;
using System.Text;

namespace PsdParser
{
    public sealed class PSDLayer
    {
        public enum ImgType {
            JPG,
            PNG,
        }

        public ImgType mImgType {
            get { return channels.Length == 4 ? ImgType.PNG : ImgType.JPG; }
        }

        private int groupStatus;
        private byte opacityValue;

        public int id { get; internal set; }

        public string name { get; internal set; }

        public bool drop { get; internal set; }

        public bool groupStarted
        {
            get
            {
                if (!this.groupOpened)
                    return this.groupClosed;
                return true;
            }
        }

        public bool groupOpened
        {
            get
            {
                return this.groupStatus == 1;
            }
        }

        public bool groupClosed
        {
            get
            {
                return this.groupStatus == 2;
            }
        }

        public bool groupEnded
        {
            get
            {
                return this.groupStatus == 3;
            }
        }

        public float opacity
        {
            get
            {
                return (float)this.opacityValue / (float)byte.MaxValue;
            }
        }

        public bool isImageLayer
        {
            get
            {
                if (!this.drop && this.area.width > 0 && this.area.height > 0)
                    return this.channels.Length > 0;
                return false;
            }
        }

        public PSDRect area { get; internal set; }

        public PSDChannelInfo[] channels { get; internal set; }

        public int pitch
        {
            get
            {
                return this.area.width * this.channels.Length;
            }
        }

        public bool isTextLayer
        {
            get
            {
                return !string.IsNullOrEmpty(this.text);
            }
        }

        public string text { get; internal set; }

        public void load(BinaryReader br, int bpp)
        {
            PSDRect psdRect;
            psdRect.top = EndianReverser.getInt32(br);
            psdRect.left = EndianReverser.getInt32(br);
            psdRect.bottom = EndianReverser.getInt32(br);
            psdRect.right = EndianReverser.getInt32(br);
            this.area = psdRect;
            int width = this.area.width;
            int height = this.area.height;
            if (width > 12288 || height > 12288)
                throw new SystemException(string.Format("Too big image (width:{0} height{1})", (object)width, (object)height));
            ushort uint16 = EndianReverser.getUInt16(br);
            if ((int)uint16 > 56)
                throw new SystemException(string.Format("Too many channels {0}", (object)uint16));
            this.channels = new PSDChannelInfo[(int)uint16];
            for (int index = 0; index < (int)uint16; ++index)
            {
                PSDChannelInfo psdChannelInfo = new PSDChannelInfo();
                psdChannelInfo.width = this.area.width;
                psdChannelInfo.height = this.area.height;
                psdChannelInfo.loadHeader(br);
                this.channels[index] = psdChannelInfo;
            }
            string str = PSDUtil.readAscii(br, 4);
            if ("8BIM" != str)
                throw new SystemException(string.Format("Wrong signature {0}", (object)str));
            br.ReadInt32();
            this.opacityValue = br.ReadByte();
            int num1 = (int)br.ReadByte();
            int num2 = (int)br.ReadByte();
            int num3 = (int)br.ReadByte();
            long num4 = (long)EndianReverser.getUInt32(br);
            long position1 = br.BaseStream.Position;
            uint uint32_1 = EndianReverser.getUInt32(br);
            br.BaseStream.Position += (long)uint32_1;
            long num5 = num4 - (br.BaseStream.Position - position1);
            long position2 = br.BaseStream.Position;
            uint uint32_2 = EndianReverser.getUInt32(br);
            br.BaseStream.Position += (long)uint32_2;
            long num6 = num5 - (br.BaseStream.Position - position2);
            long position3 = br.BaseStream.Position;
            this.name = PSDUtil.readPascalString(br, 4);
            long num7 = num6 - (br.BaseStream.Position - position3);
            PSDLayerResource psdLayerResource = new PSDLayerResource();
            while (num7 > 7L)
            {
                long position4 = br.BaseStream.Position;
                psdLayerResource.load(br);
                num7 -= br.BaseStream.Position - position4;
            }
            if (num7 > 0L)
                br.BaseStream.Position += num7;
            this.id = psdLayerResource.id;
            if (!string.IsNullOrEmpty(psdLayerResource.name))
                this.name = psdLayerResource.name;
            this.drop = psdLayerResource.drop;
            this.groupStatus = psdLayerResource.groupStatus;
            this.text = psdLayerResource.typeToolObj2.text;
            /*
            if (psdLayerResource.typeToolObj2.textDescriptor.descCount == 6) {
                PSDTypeToolObject2.Decoder_EngineData data = psdLayerResource.typeToolObj2.textDescriptor.descs[5].items[0] as PSDTypeToolObject2.Decoder_EngineData;
                Logger.Log("-------------" + data);

                StringBuilder sb = new StringBuilder();
                foreach (byte b in data.pdfData) {
                    sb.Append((char) b);
                }
                Logger.Log(sb);


//                File.WriteAllBytes("d:\\test.txt", data.pdfData);

                //Debug.Log(System.Text.Encoding.UTF8.GetString(data.pdfData));
                //Debug.Log(System.Text.Encoding.ASCII.GetString(data.pdfData));
//                string s = System.Text.Encoding.Unicode.GetString(data.pdfData);
//                Debug.Log(s); 
                File.WriteAllText("d:\\test2222.txt", sb.ToString());

            }*/

            if (this.area.width != 0 || this.groupStatus != 0)
                return;
            this.area = psdLayerResource.typeToolObj2.area;
        }

        public byte[] mergeChannels()
        {
            if (!this.isImageLayer)
                return (byte[])null;
            int ch = this.channels.Length;
            int length2 = this.channels[0].data.Length;
            byte[] byteArray = new byte[this.area.width * this.area.height * ch];
            int num1 = 0;
            for (int index1 = 0; index1 < length2; ++index1)
            {
                if (ch == 4)
                {
                    byte[] bytes = byteArray;
                    int index2 = num1;
                    int num2 = 1;
                    int num3 = index2 + num2;
                    int num4 = (int)this.channels[3].data[index1];
                    bytes[index2] = (byte)num4;
                    byte[] numArray3 = byteArray;
                    int index3 = num3;
                    int num5 = 1;
                    int num6 = index3 + num5;
                    int num7 = (int)this.channels[2].data[index1];
                    numArray3[index3] = (byte)num7;
                    byte[] numArray4 = byteArray;
                    int index4 = num6;
                    int num8 = 1;
                    int num9 = index4 + num8;
                    int num10 = (int)this.channels[1].data[index1];
                    numArray4[index4] = (byte)num10;
                    byte[] numArray5 = byteArray;
                    int index5 = num9;
                    int num11 = 1;
                    num1 = index5 + num11;
                    int num12 = (int)this.channels[0].data[index1];
                    numArray5[index5] = (byte)num12;
                }
                else if (ch == 3)
                {
                    byte[] bytes = byteArray;
                    int index2 = num1;
                    int num2 = 1;
                    int num3 = index2 + num2;
                    int num4 = (int)this.channels[2].data[index1];
                    bytes[index2] = (byte)num4;
                    byte[] numArray3 = byteArray;
                    int index3 = num3;
                    int num5 = 1;
                    int num6 = index3 + num5;
                    int num7 = (int)this.channels[1].data[index1];
                    numArray3[index3] = (byte)num7;
                    byte[] numArray4 = byteArray;
                    int index4 = num6;
                    int num8 = 1;
                    num1 = index4 + num8;
                    int num9 = (int)this.channels[0].data[index1];
                    numArray4[index4] = (byte)num9;
                }
            }
            return byteArray;
        }
    }
}
