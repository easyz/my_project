using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace psd_vs_pj {
    public partial class FormLogDialog : Form {

        private static FormLogDialog m_Instance;

        public FormLogDialog() {
            m_Instance = this;
            InitializeComponent();
            
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            SizeGripStyle = SizeGripStyle.Hide;
            btn.Enabled = false;
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);
            m_Instance = null;
        }

        private void tbxLog_TextChanged(object sender, EventArgs e) {

        }

        public static void AppendTextColorful(RichTextBox rtBox, string text, Color color, bool addNewLine = true) {
            if (addNewLine) {
                text += Environment.NewLine;
            }
            rtBox.SelectionStart = rtBox.TextLength;
            rtBox.SelectionLength = 0;
            rtBox.SelectionColor = color;
            rtBox.AppendText(text);
            rtBox.SelectionColor = rtBox.ForeColor;
        }

        public static void OutputRed(string log) {
            Output(log, Color.Red);     
        }

        public static void OutputYellow(string log) {
            Output(log, Color.Yellow);     
        }

        public static void Output(string log) {
            Output(log, Color.Black);     
        }

        private static void Output(string log, Color color) {
            if (m_Instance == null) {
                return;
            }
            log = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), log);
            AppendTextColorful(m_Instance.tbxLog, log, color);
            m_Instance.Write(log);
        }

        public void Write(string msg) {
            string logPath = Path.GetDirectoryName(Application.ExecutablePath);
            StreamWriter sw = File.AppendText(logPath + "/log.txt");
            sw.WriteLine(msg);
            sw.Close();
            sw.Dispose();
        }

        public Button btn { get { return button1; } }

        private void button1_Click(object sender, EventArgs e) {
            Close();
        }
    }
}
