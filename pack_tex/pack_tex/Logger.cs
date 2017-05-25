using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;

namespace packTex {

    public static class Logger {

        public static Action<int, string> Print;

        public static void Log(params object[] format) {
            string outStr = string.Empty;
            if (format != null) {
                outStr = string.Join("\t", format);
            }
            outStr = "[I] " + outStr;
            if (Print != null) {
                Print(0, outStr);
                return;
            }
            Console.WriteLine(outStr);
        }

        public static void Log(object format) {
            string outStr = "[I] " + format;
            if (Print != null) {
                Print(0, outStr);
                return;
            }
            Console.WriteLine(outStr);
        }

        public static void Warn(object format) {
            string outStr = "[W] " + format;
            Console.WriteLine(outStr);
            if (Print != null) {
                Print(1, outStr);
                return;
            }
        }

        public static void LogError(string format) {
            string outStr = "[E] " + format;
            if (Print != null) {
                Print(2, outStr);
                return;
            }
            Console.WriteLine(outStr);
        }
    }
}