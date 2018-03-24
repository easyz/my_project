using System.IO;

namespace PsdParser
{
    public sealed class PSDLayerInfo
    {
        public PSDLayer[] layers;
        public long channelDataStartPosition;
        public long channelDataEndPosition;

        public void loadHeader(BinaryReader br, int bpp)
        {
            int num1 = (int)EndianReverser.getUInt32(br);
            long num2 = br.BaseStream.Position + (long)num1;
            EndianReverser.getInt32(br);
            int length1 = (int)EndianReverser.getInt16(br);
            if (length1 == 0)
            {
                br.BaseStream.Position = num2;
            }
            else
            {
                if (length1 < 0)
                    length1 = -length1;
                this.layers = new PSDLayer[length1];
                for (int index = 0; index < length1; ++index)
                {
                    PSDLayer psdLayer = new PSDLayer();
                    psdLayer.load(br, bpp);
                    this.layers[index] = psdLayer;
                }
                this.channelDataStartPosition = br.BaseStream.Position;
                this.channelDataEndPosition = num2;
                for (int index1 = 0; index1 < this.layers.Length; ++index1)
                {
                    PSDLayer psdLayer = this.layers[index1];
                    int length2 = psdLayer.channels.Length;
                    for (int index2 = 0; index2 < length2; ++index2)
                    {
                        PSDChannelInfo psdChannelInfo = psdLayer.channels[index2];
                        psdChannelInfo.dataStartPosition = br.BaseStream.Position;
                        br.BaseStream.Position += (long)psdChannelInfo.size;
                    }
                }
                br.BaseStream.Position = num2;
            }
        }

        public void loadData(BinaryReader br, int bpp)
        {
            br.BaseStream.Position = this.channelDataStartPosition;
            for (int index1 = 0; index1 < this.layers.Length; ++index1)
            {
                PSDLayer psdLayer = this.layers[index1];
                int length = psdLayer.channels.Length;
                bool flag = !psdLayer.drop && psdLayer.isImageLayer;
                for (int index2 = 0; index2 < length; ++index2)
                {
                    PSDChannelInfo psdChannelInfo = psdLayer.channels[index2];
                    if (flag)
                        psdChannelInfo.loadData(br, bpp);
                    else
                        br.BaseStream.Position += (long)psdChannelInfo.size;
                }
            }
            br.BaseStream.Position = this.channelDataEndPosition;
        }
    }
}
