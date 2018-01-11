using System;
using System.Diagnostics;
using psd_vs_pj;

public static class Logger {

    static Logger() {
//        packTex.Logger.Print = (i, s) => {
//            if (i == 0) {
//                FormLogDialog.Output(s);
//            } else if (i == 1) {
//                FormLogDialog.OutputYellow(s);
//            } else if (i == 2) {
//                FormLogDialog.OutputRed(s);
//            } else {
//                FormLogDialog.Output(s);
//            }
//        };
    }

    public static void Log(params object[] format) {
        string outStr = string.Empty;
        if (format != null) {
            outStr = string.Join("\t", format);
        }
        outStr = "[I] " + outStr;
        Console.WriteLine(outStr);
        FormLogDialog.Output(outStr);
    }

    public static void Log(object format) {
        string outStr = "[I] " + format;
        Console.WriteLine(outStr);
        FormLogDialog.Output(outStr);
    }

    public static void Warn(object format) {
        string outStr = "[W] " + format;
        Console.WriteLine(outStr);
        FormLogDialog.OutputYellow(outStr);
    }

    public static void LogError(string format) {
        string outStr = "[E] " + format;
        Console.WriteLine(outStr);
        FormLogDialog.OutputRed(outStr);
    }
}
