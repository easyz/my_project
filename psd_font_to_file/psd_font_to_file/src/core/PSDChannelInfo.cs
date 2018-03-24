using System;
using System.IO;

namespace PsdParser
{
    public sealed class PSDChannelInfo
    {
        public short id;
        public uint size;
        public long dataStartPosition;
        public short compressionType;
        public int width;
        public int height;
        public byte[] data;

        public bool maskChannel
        {
            get
            {
                return (int)this.id == -2;
            }
        }

        public void loadHeader(BinaryReader br)
        {
            this.id = EndianReverser.getInt16(br);
            this.size = EndianReverser.getUInt32(br);
        }

        public void saveHeader(BinaryWriter bw)
        {
            bw.Write(EndianReverser.convert(this.id));
            bw.Write(EndianReverser.convert(this.size));
        }

        public void saveData(BinaryWriter bw)
        {
            int num = (int)this.size;
        }

        public void loadData(BinaryReader br, int bps)
        {
            if (this.size <= 2U)
                return;
            br.BaseStream.Position = this.dataStartPosition;
            this.compressionType = EndianReverser.getInt16(br);
            switch (this.compressionType)
            {
                case 0:
                    this.readData(br, bps, (int)this.compressionType, (short[])null);
                    break;
                case 1:
                    short[] rlePackLengths = new short[this.height * 2];
                    for (int index = 0; index < this.height; ++index)
                        rlePackLengths[index] = EndianReverser.getInt16(br);
                    this.readData(br, bps, (int)this.compressionType, rlePackLengths);
                    break;
                default:
                    throw new SystemException(string.Format("Unsupport compression type {0}", (object)this.compressionType));
            }
        }

        private void readData(BinaryReader br, int bps, int compressionType, short[] rlePackLengths)
        {
            int unpackedLength = bps != 1 ? this.width * bps >> 3 : this.width + 7 >> 3;
            this.data = new byte[unpackedLength * this.height];
            switch (compressionType)
            {
                case 0:
                    br.Read(this.data, 0, this.data.Length);
                    break;
                case 1:
                    for (int index1 = 0; index1 < this.height; ++index1)
                    {
                        byte[] numArray = new byte[(int)rlePackLengths[index1]];
                        byte[] dst = new byte[unpackedLength];
                        br.Read(numArray, 0, (int)rlePackLengths[index1]);
                        PSDUtil.decodeRLE(numArray, dst, (int)rlePackLengths[index1], unpackedLength);
                        for (int index2 = 0; index2 < unpackedLength; ++index2)
                            this.data[index1 * unpackedLength + index2] = dst[index2];
                    }
                    break;
            }
            switch (bps)
            {
                case 1:
                    byte[] numArray1 = this.data;
                    byte[] numArray2 = new byte[this.width * this.height];
                    int num1 = this.height;
                    int num2 = this.width;
                    uint num3 = 0;
                    int index3 = 0;
                    int num4 = 0;
                    for (int index1 = 0; index1 < num1 * (num2 + 7 >> 3); ++index1)
                    {
                        byte num5 = 128;
                        for (int index2 = 0; index2 < 8 && (long)num3 < (long)num2; ++index2)
                        {
                            numArray2[num4++] = ((int)numArray1[index3] & (int)num5) != 0 ? (byte)0 : (byte)1;
                            num5 >>= 1;
                            ++num3;
                        }
                        if ((long)num3 >= (long)num2)
                            num3 = 0U;
                        ++index3;
                    }
                    this.data = numArray2;
                    break;
                case 16:
                    byte[] numArray3 = this.data;
                    byte[] numArray4 = new byte[this.width * this.height];
                    for (int index1 = 0; index1 < this.data.Length; ++index1)
                        this.data[index1] = numArray3[index1 * 2];
                    this.data = numArray4;
                    break;
            }
        }

        private enum PSDChannelID
        {
            PSD_CHANNEL_MASK = -2,
            PSD_CHANNEL_ALPHA = -1,
            PSD_CHANNEL_RED = 0,
            PSD_CHANNEL_GREEN = 1,
            PSD_CHANNEL_BLUE = 2,
        }
    }
}
