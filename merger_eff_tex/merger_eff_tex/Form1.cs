using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using merger_tex;
using Microsoft.VisualBasic;
using psd_vs_pj;
using Color = System.Drawing.Color;

namespace psd {
    internal partial class Form1 : Form {

        private HashSet<string> m_FilterSet = new HashSet<string>();
        private string[] m_DirFiles = new string[0];            

        private Data data;

        private IFileTool m_FileTool;

        public Form1() {
            InitializeComponent();

            m_FileTool = new MergerTex();

            textBox1.Enter += searchNameInput_Enter;
            textBox1.Leave += searchNameInput_Leave;

            base.Load += new EventHandler(this.Form1_Load);
            base.Shown += new EventHandler(this.form1_Activated);
        }

        private void form1_Activated(object sender, EventArgs e) {
            this.data = new Data();
            switch (this.data.Init())
            {
                case Data.InitResult.resNoExe:
                    MessageBox.Show("初始化失败, 程序即将退出\nerror:找不到执行程序 excel2lua.exe", "警告", MessageBoxButtons.OK);
                    break;

                case Data.InitResult.resNoToolPath:
                    MessageBox.Show("初始化失败, 程序即将退出\nerror:未指定工具目录", "警告", MessageBoxButtons.OK);
                    break;

//                case Data.InitResult.resNoPathConfig:
//                    MessageBox.Show("初始化失败, 程序即将退出\nerror:找不到路径配置文件 " + Data.pathConfigFileName, "警告", MessageBoxButtons.OK);
//                    break;

                case Data.InitResult.resSuccess:
                    this.Init();
                    return;
            }
            base.Close();
        }

        private void Form1_Load(object sender, EventArgs e) {
        }

        private string GetTablePath(bool log = false) {
            if (!Directory.Exists(data.tablePath)) {
                if (log) {
                    Logger.Warn(data.tablePath + " not found!!!");
                }
                return null;
            }
            return data.tablePath;
        }

        private void Init() {
            DisplayConfigSection();
            RefreshDirFiles();
            RefreshTableList();

            m_FilterFileText.Text = data.filter;
        }

        private void RefreshTableList() {
            List<string> allSearchFiles = new List<string>(m_DirFiles); 
            string searchText = null;

            if ((textBox1.ForeColor == Color.Black) && !string.IsNullOrEmpty(textBox1.Text)) {
                searchText = textBox1.Text;
            }
            for (int i = allSearchFiles.Count - 1; i >= 0; i--) {
                string filePath = allSearchFiles[i];
                string extension = Path.GetExtension(filePath);
                if (m_FilterSet.Count > 0 && (string.IsNullOrEmpty(extension) || !m_FilterSet.Contains(extension.Substring(1)))) {
                    allSearchFiles.RemoveAt(i);
                    continue;
                }
                if (!string.IsNullOrEmpty(searchText) && (filePath.IndexOf(searchText, StringComparison.Ordinal) == -1)) {
                    allSearchFiles.RemoveAt(i);
                    continue;
                }
            }
            string[] items = allSearchFiles.ToArray();
            tableListBox.Items.Clear();
            tableListBox.Items.AddRange(items);
        }

//        public static List<string> GetFiles(string dir) {
//            List<string> allSearchFiles = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly).ToList();
//            return allSearchFiles;
//        }

        private void RefreshDirFiles() {
            m_DirFiles = new string[0];
            if (string.IsNullOrEmpty(data.tablePath)) {
                data.tablePath = AppDomain.CurrentDomain.BaseDirectory;
            }
            tablePath_textBox.Text = data.tablePath;
            table_out_textBox.Text = data.tableOutPath;
            if (string.IsNullOrEmpty(GetTablePath(true))) {
                return;
            }
            int dirStrLength = data.tablePath.Length + 1;
//            List<string> allSearchFiles = Directory.GetFiles(data.tablePath, "*", SearchOption.AllDirectories).ToList();
            List<string> allSearchFiles = m_FileTool.GetFiles(data.tablePath);
//            List<string> allSearchFiles = GetFiles(data.tablePath);
            for (int i = allSearchFiles.Count - 1; i >= 0; i--) {
                if (allSearchFiles[i].IndexOf("~$", StringComparison.Ordinal) != -1) {
                    allSearchFiles.RemoveAt(i);
                    continue;
                }
                allSearchFiles[i] = allSearchFiles[i].Substring(dirStrLength);
            }
            m_DirFiles = allSearchFiles.ToArray();
        }

        private void searchNameInput_Enter(object sender, EventArgs e) {
            textBox1.ForeColor = Color.Black;
            textBox1.Text = string.Empty;
        }

        private void searchNameInput_Leave(object sender, EventArgs e) {
            if ((textBox1.Text == null) || (textBox1.Text == "")) {
                textBox1.ForeColor = Color.Gray;
                textBox1.Text = "请输入名字查询";
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            if (textBox1.ForeColor == Color.Gray) {
                return;
            }
            Thread.Sleep(50);
            RefreshTableList();
        }

        private void m_FilterFileText_TextChanged(object sender, EventArgs e) {
            Thread.Sleep(50);
            string[] str = m_FilterFileText.Text.Split(';');
            if (str.Length == 1 && string.IsNullOrEmpty(str[0])) {
                m_FilterSet.Clear();
            } else {
                m_FilterSet = new HashSet<string>(str);
            }
            if (m_FilterFileText.Text != data.filter) {
                data.filter = m_FilterFileText.Text;    
            }
            RefreshTableList();
        }

        private void button3_Click(object sender, EventArgs e) {
            fdialog.SelectedPath = data.PathFormat(this.data.tablePath);
            if (this.fdialog.ShowDialog() == DialogResult.OK) {
                this.tablePath_textBox.Text = this.fdialog.SelectedPath;
                this.data.tablePath = this.fdialog.SelectedPath;
                this.RefreshTableList();
            }
        }

        private void button4_Click(object sender, EventArgs e) {

        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e) {
            RefreshDirFiles();
            RefreshTableList();
        }

        private void fileSystemWatcher1_Changed(object sender, RenamedEventArgs e) {
            RefreshDirFiles();
            RefreshTableList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string name = Interaction.InputBox("请输入配置名:", "创建新配置", "", -1, -1);
            if (name.Length > 0x20)
            {
                MessageBox.Show("太长了");
            }
            else if (name.Length > 0)
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog
                {
                    Description = "请指定新配置的工具目录",
                    ShowNewFolderButton = false
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = dialog.SelectedPath;
                    this.data.config.AddNewConfig(name, selectedPath);
                    this.RefreshAll();
                }
            }
        }

        public void RefreshAll()
        {
            if (this.data.Init() != Data.InitResult.resSuccess) {
                MessageBox.Show("重载数据时出错", "警告", MessageBoxButtons.OK);
            }
            DisplayConfigSection();
            RefreshDirFiles();
            RefreshTableList();

            m_FilterFileText.Text = data.filter;
        }

        private void DisplayConfigSection()
        {
            comboBox_configs.Items.Clear();
            comboBox_configs.Items.AddRange(this.data.config.GetConfigNames());
            comboBox_configs.SelectedIndex = this.data.configId;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            string dir = tablePath_textBox.Text + "\\";
            HashSet<string> handleFiles = new HashSet<string>();
            foreach (object item in tableListBox.CheckedItems) {
                handleFiles.Add(dir + item);
            }
            HandleFiles(handleFiles.ToArray());
        }

        private void HandleFiles(string[] handleFiles) {
            if (handleFiles.Length == 0) {
                MessageBox.Show("没有选中的导出项");
                return;
            }

            if (string.IsNullOrEmpty(data.tableOutPath)) {
                MessageBox.Show("没有设置资源输出路径");
                return;
            }
            FormLogDialog dialog = new FormLogDialog();
            dialog.Shown += (o, args) => {
                if (m_FileTool.Handle(handleFiles, data.tableOutPath)) {
                    dialog.btn.Enabled = true;
                }
            };
            dialog.ShowDialog(this);
        }

        private void comboBox_configs_SelectedIndexChanged(object sender, EventArgs e) {
            if (comboBox_configs.SelectedIndex == data.configId) {
                return;
            }
            data.config.SetCurrentConfigId(comboBox_configs.SelectedIndex);
            RefreshAll();
        }

        private void button5_Click(object sender, EventArgs e) {
            RefreshAll();
        }

        private void button6_Click(object sender, EventArgs e) {
            fdialog.SelectedPath = data.PathFormat(this.data.tableOutPath);
            if (this.fdialog.ShowDialog() == DialogResult.OK) {
                this.table_out_textBox.Text = this.fdialog.SelectedPath;
                this.data.tableOutPath = this.fdialog.SelectedPath;
                this.RefreshTableList();
            }
        }

        private void button7_Click(object sender, EventArgs e) {
            string dir = tablePath_textBox.Text + "\\";
            HashSet<string> handleFiles = new HashSet<string>();
            foreach (object item in tableListBox.Items) {
                handleFiles.Add(dir + item);
            }
            HandleFiles(handleFiles.ToArray());
        }


        //         private void button47_Click(object sender,EventArgs e)
        //       
        //{

        //System.Windows.Forms.DataVisualization.Charting.Chart ch =(System.Windows.Forms.DataVisualization.Charting.Chart)this.panel21.Controls[0];
        //           
        //ch.SaveImage(System.Windows.Forms.Application.StartupPath +
        //"\\ChartImg\\ChartTempFile.jpg",
        //System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Jpeg);
        ////临时文件
        //Image image =
        //Image.FromFile(System.Windows.Forms.Application.StartupPath +
        //"\\ChartImg\\ChartTempFile.jpg");
        //           
        //SaveFileDialog savedialog = new SaveFileDialog();
        //           
        //savedialog.Filter = "Jpg 图片|*.jpg|Bmp 图片|*.bmp|Gif 图片|*.gif|Png
        //图片|*.png|Wmf  图片|*.wmf";
        //           
        //savedialog.FilterIndex = 0;
        //           
        //savedialog.RestoreDirectory = true;
        //           
        //savedialog.FileName =
        //System.DateTime.Now.ToString("yyyyMMddHHmmss") + "-";
        //           
        //if (savedialog.ShowDialog() == DialogResult.OK)
        //           
        //{
        //               
        //image.Save(savedialog.FileName,
        //System.Drawing.Imaging.ImageFormat.Jpeg);
        //               
        //MessageBox.Show(this, "图片保存成功！", "信息提示");
        //           
        //}
    }
}
