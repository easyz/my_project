using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace psd {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
      
        [STAThread]
        static void Main() {

            string[] DIR = new string[] {
                "..\\libs\\"
            };
            System.Func<string, string> findDll = s => {
                s = s.Split(',')[0].Trim();
                foreach (string value in DIR) {
                    string path = value + s + ".dll";
                    if (File.Exists(path)) {
                        return path;
                    }
                }
                return null;
            };
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                string path = findDll(args.Name);
                if (path != null) {
                    Assembly assembly = Assembly.LoadFrom(path);
                    return assembly;
                }
                return null;
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
