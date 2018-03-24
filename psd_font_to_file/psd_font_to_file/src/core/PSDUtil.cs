using System;
using System.IO;
using System.Text;

namespace PsdParser
{
  public sealed class PSDUtil
  {
    public static string readAscii(BinaryReader br, int length)
    {
      return Encoding.ASCII.GetString(br.ReadBytes(length));
    }

    public static string readPascalString(BinaryReader br, int modLength)
    {
      byte num = br.ReadByte();
      string str = "";
      if ((int) num == 0)
      {
        br.BaseStream.Position += (long) (modLength - 1);
      }
      else
      {
        str = Encoding.UTF8.GetString(br.ReadBytes((int) num));
        for (int index = (int) num + 1; index % modLength != 0; ++index)
          ++br.BaseStream.Position;
      }
      return str;
    }

    public static string readUnicodeString(BinaryReader br)
    {
      int num1 = 4;
      int int32 = EndianReverser.getInt32(br);
      string str = "";
      if (int32 == 0)
      {
        br.BaseStream.Position += (long) (num1 - 1);
      }
      else
      {
        byte[] bytes = br.ReadBytes(int32 * 2);
        for (int index1 = 0; index1 < int32; ++index1)
        {
          int index2 = index1 * 2;
          byte num2 = bytes[index2];
          bytes[index2] = bytes[index2 + 1];
          bytes[index2 + 1] = num2;
        }
        str = Encoding.Unicode.GetString(bytes);
        for (int index = int32 + 1; index % num1 != 0; ++index)
          ++br.BaseStream.Position;
      }
      return str;
    }

    public static void decodeRLE(byte[] src, byte[] dst, int packedLength, int unpackedLength)
    {
      int index1 = 0;
      int num1 = 0;
      int num2 = unpackedLength;
      int num3 = packedLength;
      while (num2 > 0 && num3 > 0)
      {
        int num4 = (int) src[index1++];
        --num3;
        if (num4 != 128)
        {
          if (num4 > 128)
            num4 -= 256;
          if (num4 < 0)
          {
            int num5 = 1 - num4;
            if (num3 == 0)
              throw new Exception("Input buffer exhausted in replicate");
            if (num5 > num2)
              throw new Exception(string.Format("Overrun in packbits replicate of {0} chars", (object) (num5 - num2)));
            byte num6 = src[index1];
            for (; num5 > 0 && num2 != 0; --num5)
            {
              dst[num1++] = num6;
              --num2;
            }
            if (num2 > 0)
            {
              ++index1;
              --num3;
            }
          }
          else
          {
            for (int index2 = num4 + 1; index2 > 0; --index2)
            {
              if (num3 == 0)
                throw new Exception("Input buffer exhausted in copy");
              if (num2 == 0)
                throw new Exception("Output buffer exhausted in copy");
              dst[num1++] = src[index1++];
              --num2;
              --num3;
            }
          }
        }
      }
      if (num2 <= 0)
        return;
      for (int index2 = 0; index2 < num3; ++index2)
        dst[num1++] = (byte) 0;
    }
  }
}
