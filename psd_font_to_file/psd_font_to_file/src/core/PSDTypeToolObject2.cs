using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PsdParser
{
    public class PSDTypeToolObject2
    {
        internal double[] transforms = new double[6];
        internal Descriptor textDescriptor = new Descriptor();
        internal Descriptor wrapDescriptor = new Descriptor();
        internal double[] rect = new double[4];
        internal short version;
        internal short textVersion;
        internal int textDescriptorVersion;
        internal short wrapVersion;
        internal int wrapDescriptorVersion;

        public string text
        {
            get
            {
                if (this.textDescriptor.descCount > 0)
                {
                    foreach (Descriptor.Description description in this.textDescriptor.descs)
                    {
                        if (description.ostype == "TEXT")
                            return description.items[0] as string;
                    }
                }
                return (string)null;
            }
        }

        public PSDRect area
        {
            get
            {
                PSDRect psdRect = new PSDRect();
                psdRect.left = psdRect.right = (int)this.transforms[4];
                psdRect.top = psdRect.bottom = (int)this.transforms[5];
                return psdRect;
            }
        }

        public void load(BinaryReader br)
        {
            this.version = EndianReverser.getInt16(br);
            for (int index = 0; index < this.transforms.Length; ++index)
                this.transforms[index] = EndianReverser.getDouble(br);
            this.textVersion = EndianReverser.getInt16(br);
            this.textDescriptorVersion = EndianReverser.getInt32(br);
            this.textDescriptor.load(br);
            this.wrapVersion = EndianReverser.getInt16(br);
            this.wrapDescriptorVersion = EndianReverser.getInt32(br);
            this.wrapDescriptor.load(br);
            for (int index = 0; index < this.rect.Length; ++index)
                this.rect[index] = EndianReverser.getDouble(br);
        }

        internal class BaseDecoder
        {
            public List<object> items = new List<object>();

            public BaseDecoder(BinaryReader br, string key)
            {
                int int32 = EndianReverser.getInt32(br);
                while (int32-- > 0)
                {
                    Decoder.DecodeFunc decoder = Decoder.getDecoder(PSDUtil.readAscii(br, 4));
                    if (decoder != null)
                    {
                        object obj = decoder(br, key);
                        if (obj != null)
                            this.items.Add(obj);
                    }
                }
            }
        }

        internal class Decoder_Reference : BaseDecoder
        {
            public Decoder_Reference(BinaryReader br, string key)
                : base(br, key)
            {
            }
        }

        internal class Decoder_List : BaseDecoder
        {
            public Decoder_List(BinaryReader br, string key)
                : base(br, key)
            {
            }
        }

        internal class Decoder_UnitFloat
        {
            private const string ANGLE = "#Ang";
            private const string DENSITY = "#Rsl";
            private const string DISTANCE = "#Rlt";
            private const string NONE = "#Nne";
            private const string PERCENT = "#Prc";
            private const string PIXELS = "#Pxl";
            public string key;
            public double value;

            public Decoder_UnitFloat(BinaryReader br)
            {
                this.key = PSDUtil.readAscii(br, 4);
                this.value = EndianReverser.getDouble(br);
            }
        }

        internal class Decoder_Enumerate
        {
            public string type;
            public string enumName;

            public Decoder_Enumerate(BinaryReader br)
            {
                this.type = Decoder.readStringOrKey(br);
                this.enumName = Decoder.readStringOrKey(br);
            }
        }

        internal class Decoder_Class
        {
            public string name;
            public string classId;

            public Decoder_Class(BinaryReader br)
            {
                this.name = Decoder.readUnicodeString(br);
                this.classId = Decoder.readStringOrKey(br);
            }
        }

        internal class Decoder_Alias
        {
            public string alias;

            public Decoder_Alias(BinaryReader br)
            {
                int int32 = EndianReverser.getInt32(br);
                this.alias = PSDUtil.readAscii(br, int32);
            }
        }

        public class Decoder_EngineData
        {
            public byte[] pdfData;

            public Decoder_EngineData(BinaryReader br)
            {
                int int32 = EndianReverser.getInt32(br);
                this.pdfData = br.ReadBytes(int32);
            }
        }

        internal class Decoder_Property
        {
            public string name;
            public string classId;
            public string keyId;

            public Decoder_Property(BinaryReader br)
            {
                this.name = Decoder.readUnicodeString(br);
                this.classId = Decoder.readStringOrKey(br);
                this.keyId = Decoder.readStringOrKey(br);
            }
        }

        internal class Decoder_Offset
        {
            public string name;
            public string classId;
            public int offset;

            public Decoder_Offset(BinaryReader br)
            {
                this.name = Decoder.readUnicodeString(br);
                this.classId = Decoder.readStringOrKey(br);
                this.offset = EndianReverser.getInt32(br);
            }
        }

        internal class Decoder_EnumerateReference
        {
            public string name;
            public string classId;
            public string typeId;
            public string enumId;

            public Decoder_EnumerateReference(BinaryReader br)
            {
                this.name = Decoder.readUnicodeString(br);
                this.classId = Decoder.readStringOrKey(br);
                this.typeId = Decoder.readStringOrKey(br);
                this.enumId = Decoder.readStringOrKey(br);
            }
        }

        internal class Decoder_UnknownOSType
        {
            public string message;

            public Decoder_UnknownOSType(string message)
            {
                this.message = message;
            }
        }

        internal class Decoder
        {
            public const string REFERENCE = "obj ";
            public const string DESCRIPTOR = "Objc";
            public const string LIST = "VlLs";
            public const string DOUBLE = "doub";
            public const string UNIT_FLOAT = "UntF";
            public const string STRING = "TEXT";
            public const string ENUMERATED = "enum";
            public const string INTEGER = "long";
            public const string BOOLEAN = "bool";
            public const string GLOBAL_OBJECT = "GlbO";
            public const string CLASS1 = "type";
            public const string CLASS2 = "GlbC";
            public const string ALIAS = "alis";
            public const string RAW_DATA = "tdta";
            public const string PROPERTY = "prop";
            public const string CLASS = "Clss";
            public const string ENUMERATED_REFERENCE = "Enmr";
            public const string OFFSET = "rele";
            public const string IDENTIFIER = "Idnt";
            public const string INDEX = "indx";
            public const string NAME = "name";

            public static string readStringOrKey(BinaryReader br)
            {
                int int32 = EndianReverser.getInt32(br);
                int length = int32 > 0 ? int32 : 4;
                return PSDUtil.readAscii(br, length);
            }

            public static string readUnicodeString(BinaryReader br)
            {
                int int32 = EndianReverser.getInt32(br);
                if (int32 == 0)
                    return "";
                byte[] bytes = br.ReadBytes(int32 * 2);
                for (int index1 = 0; index1 < int32; ++index1)
                {
                    int index2 = index1 * 2;
                    byte num = bytes[index2];
                    bytes[index2] = bytes[index2 + 1];
                    bytes[index2 + 1] = num;
                }
                return Encoding.Unicode.GetString(bytes);
            }

            private static object decode_REFERENCE(BinaryReader br, string key)
            {
                return new Decoder_Reference(br, key);
            }

            private static object decode_DESCRIPTOR(BinaryReader br, string key)
            {
                Descriptor descriptor = new Descriptor();
                descriptor.load(br);
                return (object)descriptor;
            }

            private static object decode_LIST(BinaryReader br, string key)
            {
                return new Decoder_List(br, key);
            }

            private static object decode_DOUBLE(BinaryReader br, string key)
            {
                return (object)EndianReverser.getDouble(br);
            }

            private static object decode_UNIT_FLOAT(BinaryReader br, string key)
            {
                return new Decoder_UnitFloat(br);
            }

            private static object decode_STRING(BinaryReader br, string key)
            {
                return (object)Decoder.readUnicodeString(br);
            }

            private static object decode_ENUMERATED(BinaryReader br, string key)
            {
                return new Decoder_Enumerate(br);
            }

            private static object decode_INTEGER(BinaryReader br, string key)
            {
                return (object)EndianReverser.getInt32(br);
            }

            private static object decode_BOOLEAN(BinaryReader br, string key)
            {
                return (object)(EndianReverser.getBoolean(br));
            }

            private static object decode_GLOBAL_OBJECT(BinaryReader br, string key)
            {
                Descriptor descriptor = new Descriptor();
                descriptor.load(br);
                return (object)descriptor;
            }

            private static object decode_CLASS1(BinaryReader br, string key)
            {
                return new Decoder_Class(br);
            }

            private static object decode_CLASS2(BinaryReader br, string key)
            {
                return new Decoder_Class(br);
            }

            private static object decode_ALIAS(BinaryReader br, string key)
            {
                return new Decoder_Alias(br);
            }

            private static object decode_RAW_DATA(BinaryReader br, string key)
            {
                if (key == "EngineData")
                    return new Decoder_EngineData(br);
                return new Decoder_UnknownOSType("Cannot decode RAW_DATA data");
            }

            private static object decode_PROPERTY(BinaryReader br, string key)
            {
                return new Decoder_Property(br);
            }

            private static object decode_CLASS(BinaryReader br, string key)
            {
                return new Decoder_Class(br);
            }

            private static object decode_ENUMERATED_REFERENCE(BinaryReader br, string key)
            {
                return new Decoder_EnumerateReference(br);
            }

            private static object decode_OFFSET(BinaryReader br, string key)
            {
                return new Decoder_Offset(br);
            }

            private static object decode_IDENTIFIER(BinaryReader br, string key)
            {
                return new Decoder_UnknownOSType("Cannot decode IDENTIFIER data");
            }

            private static object decode_INDEX(BinaryReader br, string key)
            {
                return new Decoder_UnknownOSType("Cannot decode INDEX data");
            }

            private static object decode_NAME(BinaryReader br, string key)
            {
                return new Decoder_UnknownOSType("Cannot decode NAME data");
            }

            public static DecodeFunc getDecoder(string ostype)
            {
                switch (ostype)
                {
                    case REFERENCE:
                        return decode_REFERENCE;
                    case DESCRIPTOR:
                        return decode_DESCRIPTOR;
                    case LIST:
                        return decode_LIST;
                    case DOUBLE:
                        return decode_DOUBLE;
                    case UNIT_FLOAT:
                        return decode_UNIT_FLOAT;
                    case STRING:
                        return decode_STRING;
                    case ENUMERATED:
                        return decode_ENUMERATED;
                    case INTEGER:
                        return decode_INTEGER;
                    case BOOLEAN:
                        return decode_BOOLEAN;
                    case GLOBAL_OBJECT:
                        return decode_GLOBAL_OBJECT;
                    case CLASS1:
                        return decode_CLASS1;
                    case CLASS2:
                        return decode_CLASS2;
                    case ALIAS:
                        return decode_ALIAS;
                    case RAW_DATA:
                        return decode_RAW_DATA;
                    case PROPERTY:
                        return decode_PROPERTY;
                    case CLASS:
                        return decode_CLASS;
                    case ENUMERATED_REFERENCE:
                        return decode_ENUMERATED;
                    case OFFSET:
                        return decode_OFFSET;
                    case IDENTIFIER:
                        return decode_IDENTIFIER;
                    case INDEX:
                        return decode_INDEX;
                    case NAME:
                        return decode_NAME;
                    default:
                        return null;
                }
            }

            public delegate object DecodeFunc(BinaryReader br, string key);
        }

        internal class Descriptor
        {
            public string name;
            public string classId;
            public int descCount;
            public Descriptor.Description[] descs;

            public void load(BinaryReader br)
            {
                this.name = Decoder.readUnicodeString(br);
                this.classId = Decoder.readStringOrKey(br);
                this.descCount = EndianReverser.getInt32(br);
                this.descs = new Descriptor.Description[this.descCount];
                for (int index = 0; index < this.descCount; ++index)
                {
                    this.descs[index] = new Descriptor.Description();
                    this.descs[index].load(br);
                }
            }

            internal class Description
            {
                public List<object> items = new List<object>();
                public string key;
                public string ostype;

                public void load(BinaryReader br)
                {
                    this.key = Decoder.readStringOrKey(br);
                    this.ostype = PSDUtil.readAscii(br, 4);
                    Decoder.DecodeFunc decoder = Decoder.getDecoder(this.ostype);
                    if (decoder == null)
                        return;
                    object obj = decoder(br, this.key);
                    if (obj == null)
                        return;
                    this.items.Add(obj);
                }
            }
        }
    }
}
