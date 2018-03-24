using System;
using System.IO;

namespace PsdParser
{
    public sealed class PSDLayerResource
    {
        public PSDTypeToolObject typeToolObj = new PSDTypeToolObject();
        public PSDTypeToolObject2 typeToolObj2 = new PSDTypeToolObject2();
        
        public int id;
        public string name;
        public bool drop;
        public int groupStatus;

        public void load(BinaryReader br)
        {
            string str1 = PSDUtil.readAscii(br, 4);
            string str2 = PSDUtil.readAscii(br, 4);
            int int32 = EndianReverser.getInt32(br);
            if (str1 != "8BIM")
                throw new SystemException(string.Format("Wrong signature {0}", str1));
            long position = br.BaseStream.Position;
            if (str2 == "levl" || str2 == "curv" || (str2 == "brit" || str2 == "blnc") || (str2 == "blwh" || str2 == "hue " || (str2 == "hue2" || str2 == "selc")) || (str2 == "mixr" || str2 == "grdm" || (str2 == "phfl" || str2 == "expA") || (str2 == "thrs" || str2 == "nvrt" || str2 == "post")))
                this.drop = true;
            else if (str2 != "SoCo" && str2 != "PtFl" && (str2 != "GdFl" && str2 != "lrFX") && (str2 != "lfx2" && str2 != "tySh"))
            {
                if (str2 == "TySh")
                    typeToolObj2.load(br);
                else if (str2 == "luni")
                    name = PSDUtil.readUnicodeString(br);
                else if (str2 == "lyid")
                    id = EndianReverser.getInt32(br);
                else if (str2 == "lsct")
                {
                    drop = true;
                    groupStatus = EndianReverser.getInt32(br);
                }
            }
            br.BaseStream.Position = position;
            br.BaseStream.Position += int32;
            if (int32 % 2 == 0)
                return;
            ++br.BaseStream.Position;
        }
    }
}
