using System;
using System.Diagnostics;


public static class Logger {

//    public static void Log(params object[] format) {
//        string outStr = string.Empty;
////        if (format != null) {
////            outStr = string.Join("\t", format);
////        }
//        outStr = "[I] " + outStr;
//        Console.WriteLine(outStr);
//    }

    public static void Log(object format) {
        string outStr = "[I] " + format;
        Console.WriteLine(outStr);
    }

    public static void Warn(object format) {
        string outStr = "[W] " + format;
        Console.WriteLine(outStr);
    }

    public static void LogError(string format) {
        string outStr = "[E] " + format;
        Console.WriteLine(outStr);
    }
}
