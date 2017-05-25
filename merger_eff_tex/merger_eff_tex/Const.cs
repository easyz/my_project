using System.IO;

public static class Const {
    public const string LIB_DIR = "..\\libs\\";

    public static string GetFileByLib(string fileName) {
        return Path.Combine(Const.LIB_DIR, fileName);
    }
}