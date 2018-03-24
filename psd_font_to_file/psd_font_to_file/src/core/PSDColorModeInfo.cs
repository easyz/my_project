using System.IO;

namespace PsdParser
{
    public sealed class PSDColorModeInfo
    {
        public int size;
        public byte[] data;

        public void load(BinaryReader br)
        {
            this.size = EndianReverser.getInt32(br);
            if (this.size <= 0)
                return;
            this.data = new byte[this.size];
            this.data = br.ReadBytes(this.size);
        }

        public void save(BinaryWriter bw)
        {
            bw.Write(EndianReverser.convert(this.size));
            if (this.size <= 0)
                return;
            bw.Write(this.data);
        }
    }
}
