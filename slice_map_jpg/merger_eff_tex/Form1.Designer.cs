namespace psd {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.m_FilterFileText = new System.Windows.Forms.TextBox();
            this.tableListBox = new System.Windows.Forms.CheckedListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBox_configs = new System.Windows.Forms.ComboBox();
            this.tablePath_textBox = new System.Windows.Forms.TextBox();
            this.fdialog = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button6 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.table_out_textBox = new System.Windows.Forms.TextBox();
            this.button7 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.button5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.m_FilterFileText);
            this.groupBox1.Controls.Add(this.tableListBox);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(304, 500);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "文件列表";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 12;
            this.label4.Text = "图集目录列表";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(233, 32);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(63, 20);
            this.button5.TabIndex = 11;
            this.button5.Text = "刷新目录";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(113, 12);
            this.label3.TabIndex = 10;
            this.label3.Text = "过滤文件，分号分隔";
            this.label3.Visible = false;
            // 
            // m_FilterFileText
            // 
            this.m_FilterFileText.Location = new System.Drawing.Point(8, 32);
            this.m_FilterFileText.Name = "m_FilterFileText";
            this.m_FilterFileText.Size = new System.Drawing.Size(219, 21);
            this.m_FilterFileText.TabIndex = 9;
            this.m_FilterFileText.Visible = false;
            this.m_FilterFileText.TextChanged += new System.EventHandler(this.m_FilterFileText_TextChanged);
            // 
            // tableListBox
            // 
            this.tableListBox.FormattingEnabled = true;
            this.tableListBox.Location = new System.Drawing.Point(8, 86);
            this.tableListBox.Name = "tableListBox";
            this.tableListBox.Size = new System.Drawing.Size(289, 404);
            this.tableListBox.Sorted = true;
            this.tableListBox.TabIndex = 1;
            // 
            // textBox1
            // 
            this.textBox1.AccessibleName = "";
            this.textBox1.Location = new System.Drawing.Point(8, 59);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(289, 21);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "请输入名字查询";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.comboBox_configs);
            this.groupBox2.Controls.Add(this.tablePath_textBox);
            this.groupBox2.Location = new System.Drawing.Point(322, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(432, 106);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "目录";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(393, 68);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(28, 23);
            this.button3.TabIndex = 7;
            this.button3.Text = "…";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(261, 18);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "删除配置";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "文件路径";
            // 
            // button1
            // 
            this.button1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.button1.Location = new System.Drawing.Point(180, 18);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "增加配置";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBox_configs
            // 
            this.comboBox_configs.FormattingEnabled = true;
            this.comboBox_configs.Location = new System.Drawing.Point(7, 21);
            this.comboBox_configs.Name = "comboBox_configs";
            this.comboBox_configs.Size = new System.Drawing.Size(143, 20);
            this.comboBox_configs.TabIndex = 1;
            this.comboBox_configs.SelectedIndexChanged += new System.EventHandler(this.comboBox_configs_SelectedIndexChanged);
            // 
            // tablePath_textBox
            // 
            this.tablePath_textBox.Location = new System.Drawing.Point(7, 68);
            this.tablePath_textBox.Name = "tablePath_textBox";
            this.tablePath_textBox.ReadOnly = true;
            this.tablePath_textBox.Size = new System.Drawing.Size(379, 21);
            this.tablePath_textBox.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button7);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.button6);
            this.groupBox3.Controls.Add(this.button4);
            this.groupBox3.Controls.Add(this.table_out_textBox);
            this.groupBox3.Location = new System.Drawing.Point(322, 124);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(432, 388);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "工具";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 281);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "资源输出路径";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(394, 296);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(28, 23);
            this.button6.TabIndex = 9;
            this.button6.Text = "…";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button4.Location = new System.Drawing.Point(8, 336);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(160, 42);
            this.button4.TabIndex = 0;
            this.button4.Text = "导出选中项";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click_1);
            // 
            // table_out_textBox
            // 
            this.table_out_textBox.Location = new System.Drawing.Point(8, 296);
            this.table_out_textBox.Name = "table_out_textBox";
            this.table_out_textBox.ReadOnly = true;
            this.table_out_textBox.Size = new System.Drawing.Size(379, 21);
            this.table_out_textBox.TabIndex = 8;
            // 
            // button7
            // 
            this.button7.Font = new System.Drawing.Font("宋体", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button7.Location = new System.Drawing.Point(214, 336);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(160, 42);
            this.button7.TabIndex = 10;
            this.button7.Text = "导出全部";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 524);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.ForeColor = System.Drawing.SystemColors.GrayText;
            this.Name = "Form1";
            this.Text = "导出特效帧动画";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox_configs;
        private System.Windows.Forms.TextBox tablePath_textBox;
        private System.Windows.Forms.CheckedListBox tableListBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox m_FilterFileText;
        private System.Windows.Forms.FolderBrowserDialog fdialog;
//        private System.IO.FileSystemWatcher tableListWatcher;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.TextBox table_out_textBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button7;
    }
}

