using System.IO;

namespace PsdParser
{
    public sealed class PSDTypeToolObject
    {
        public double[] transforms = new double[6];
        public PSDTypeToolObject.Font font = new PSDTypeToolObject.Font();
        public PSDTypeToolObject.Style style = new PSDTypeToolObject.Style();
        public PSDTypeToolObject.Text text = new PSDTypeToolObject.Text();
        public PSDTypeToolObject.Color color = new PSDTypeToolObject.Color();
        public short version;

        public void load(BinaryReader br)
        {
            this.version = EndianReverser.getInt16(br);
            for (int index = 0; index < this.transforms.Length; ++index)
                this.transforms[index] = EndianReverser.getDouble(br);
            this.font.load(br);
            this.style.load(br);
            this.text.load(br);
            this.color.load(br);
        }

        public sealed class Font
        {
            public short version;
            public short faceCount;
            public PSDTypeToolObject.Font.Face[] faces;

            public void load(BinaryReader br)
            {
                this.version = EndianReverser.getInt16(br);
                this.faceCount = EndianReverser.getInt16(br);
                this.faces = new PSDTypeToolObject.Font.Face[(int)this.faceCount];
                for (int index = 0; index < (int)this.faceCount; ++index)
                {
                    this.faces[index] = new PSDTypeToolObject.Font.Face();
                    this.faces[index].load(br);
                }
            }

            public sealed class Face
            {
                public short mark;
                public int type;
                public string name;
                public string familyName;
                public string styleName;
                public short script;
                public int designAxes;
                public int designVector;

                public void load(BinaryReader br)
                {
                    this.mark = EndianReverser.getInt16(br);
                    this.type = EndianReverser.getInt32(br);
                    this.name = PSDUtil.readPascalString(br, 4);
                    this.familyName = PSDUtil.readPascalString(br, 4);
                    this.styleName = PSDUtil.readPascalString(br, 4);
                    this.script = EndianReverser.getInt16(br);
                    this.designAxes = EndianReverser.getInt32(br);
                    this.designVector = EndianReverser.getInt32(br);
                }
            }
        }

        public sealed class Style
        {
            public short infoCount;
            public PSDTypeToolObject.Style.Info[] infos;

            public void load(BinaryReader br)
            {
                this.infoCount = EndianReverser.getInt16(br);
                this.infos = new PSDTypeToolObject.Style.Info[(int)this.infoCount];
                for (int index = 0; index < (int)this.infoCount; ++index)
                {
                    this.infos[index] = new PSDTypeToolObject.Style.Info();
                    this.infos[index].load(br);
                }
            }

            public sealed class Info
            {
                public short mark;
                public short faceMark;
                public int size;
                public int tracking;
                public int kerning;
                public int leading;
                public int baseShift;
                public byte autoKern;
                public byte ver1_5specific;
                public byte rotateDirection;

                public void load(BinaryReader br)
                {
                    this.mark = EndianReverser.getInt16(br);
                    this.faceMark = EndianReverser.getInt16(br);
                    this.size = EndianReverser.getInt32(br);
                    this.tracking = EndianReverser.getInt32(br);
                    this.kerning = EndianReverser.getInt32(br);
                    this.leading = EndianReverser.getInt32(br);
                    this.baseShift = EndianReverser.getInt32(br);
                    this.autoKern = br.ReadByte();
                    this.ver1_5specific = br.ReadByte();
                    this.rotateDirection = br.ReadByte();
                }
            }
        }

        public sealed class Text
        {
            public short type;
            public int scaling;
            public int count;
            public int horzPlacement;
            public int vertPlacement;
            public int selStart;
            public int selEnd;
            public short lineCount;
            public PSDTypeToolObject.Text.Line[] lines;

            public void load(BinaryReader br)
            {
                this.type = EndianReverser.getInt16(br);
                this.scaling = EndianReverser.getInt32(br);
                this.count = EndianReverser.getInt32(br);
                this.horzPlacement = EndianReverser.getInt32(br);
                this.vertPlacement = EndianReverser.getInt32(br);
                this.selStart = EndianReverser.getInt32(br);
                this.selEnd = EndianReverser.getInt32(br);
                this.lineCount = EndianReverser.getInt16(br);
                this.lines = new PSDTypeToolObject.Text.Line[(int)this.lineCount];
                for (int index = 0; index < (int)this.lineCount; ++index)
                {
                    this.lines[index] = new PSDTypeToolObject.Text.Line();
                    this.lines[index].load(br);
                }
            }

            public sealed class Line
            {
                public int count;
                public short orientation;
                public short align;
                public short ch;
                public short style;

                public void load(BinaryReader br)
                {
                    this.count = EndianReverser.getInt32(br);
                    this.orientation = EndianReverser.getInt16(br);
                    this.align = EndianReverser.getInt16(br);
                    this.ch = EndianReverser.getInt16(br);
                    this.style = EndianReverser.getInt16(br);
                }
            }
        }

        public sealed class Color
        {
            public short[] components = new short[4];
            public short space;
            public byte antiAlias;

            public void load(BinaryReader br)
            {
                this.space = EndianReverser.getInt16(br);
                for (int index = 0; index < this.components.Length; ++index)
                    this.components[index] = EndianReverser.getInt16(br);
                this.antiAlias = br.ReadByte();
            }
        }
    }
}
