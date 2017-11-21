using System.IO;

public class ZLibHelper {

    public static void CopyStream(System.IO.Stream input, System.IO.Stream output) {
        byte[] buffer = new byte[2000];
        int len;
        while ((len = input.Read(buffer, 0, 2000)) > 0) {
            output.Write(buffer, 0, len);
        }
        output.Flush();
    }

    public static byte[] Compress(byte[] stream) {
        MemoryStream inMs = new MemoryStream(stream);
        MemoryStream outMs = new MemoryStream();

        zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outMs, zlib.zlibConst.Z_DEFAULT_COMPRESSION);
        try {
            CopyStream(inMs, outZStream);
        } finally {
            outZStream.Close();
            outMs.Close();
            inMs.Close();
        }
        return outMs.ToArray();
    }

    private static void compressFile(string inFile, string outFile) {

        System.IO.FileStream outFileStream = new System.IO.FileStream(outFile, System.IO.FileMode.Create);
        zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outFileStream, zlib.zlibConst.Z_DEFAULT_COMPRESSION);
        System.IO.FileStream inFileStream = new System.IO.FileStream(inFile, System.IO.FileMode.Open);
        try {
            CopyStream(inFileStream, outZStream);
        } finally {
            outZStream.Close();
            outFileStream.Close();
            inFileStream.Close();
        }
    }
    private static void decompressFile(string inFile, string outFile) {
        System.IO.FileStream outFileStream = new System.IO.FileStream(outFile, System.IO.FileMode.Create);
        zlib.ZOutputStream outZStream = new zlib.ZOutputStream(outFileStream);
        System.IO.FileStream inFileStream = new System.IO.FileStream(inFile, System.IO.FileMode.Open);
        try {
            CopyStream(inFileStream, outZStream);
        } finally {
            outZStream.Close();
            outFileStream.Close();
            inFileStream.Close();
        }
    }
}