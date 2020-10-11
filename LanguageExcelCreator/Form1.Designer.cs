namespace LanguageExcelCreator
{
    partial class englishImport
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.createButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.sourceBox1 = new System.Windows.Forms.TextBox();
            this.sourceButton1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.destBox = new System.Windows.Forms.TextBox();
            this.destButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.sourceBox2 = new System.Windows.Forms.TextBox();
            this.sourceButton2 = new System.Windows.Forms.Button();
            this.tip = new System.Windows.Forms.Label();
            this.importButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.englishBox1 = new System.Windows.Forms.TextBox();
            this.englishButton1 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.englishBox2 = new System.Windows.Forms.TextBox();
            this.englishButton2 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // createButton
            // 
            this.createButton.AccessibleName = "";
            this.createButton.Location = new System.Drawing.Point(194, 156);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(157, 40);
            this.createButton.TabIndex = 0;
            this.createButton.Text = "生成";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.OnClickCreate);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "Master路径:";
            // 
            // sourceBox1
            // 
            this.sourceBox1.Location = new System.Drawing.Point(81, 20);
            this.sourceBox1.Name = "sourceBox1";
            this.sourceBox1.Size = new System.Drawing.Size(362, 21);
            this.sourceBox1.TabIndex = 2;
            // 
            // sourceButton1
            // 
            this.sourceButton1.Location = new System.Drawing.Point(449, 18);
            this.sourceButton1.Name = "sourceButton1";
            this.sourceButton1.Size = new System.Drawing.Size(75, 23);
            this.sourceButton1.TabIndex = 3;
            this.sourceButton1.Text = "选择";
            this.sourceButton1.UseVisualStyleBackColor = true;
            this.sourceButton1.Click += new System.EventHandler(this.OnClickSelectPath);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 95);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "生成路径:";
            // 
            // destBox
            // 
            this.destBox.Location = new System.Drawing.Point(81, 92);
            this.destBox.Name = "destBox";
            this.destBox.Size = new System.Drawing.Size(362, 21);
            this.destBox.TabIndex = 5;
            // 
            // destButton
            // 
            this.destButton.Location = new System.Drawing.Point(449, 90);
            this.destButton.Name = "destButton";
            this.destButton.Size = new System.Drawing.Size(75, 23);
            this.destButton.TabIndex = 6;
            this.destButton.Text = "选择";
            this.destButton.UseVisualStyleBackColor = true;
            this.destButton.Click += new System.EventHandler(this.OnClickSelectPath);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(22, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "Slg路径:";
            // 
            // sourceBox2
            // 
            this.sourceBox2.Location = new System.Drawing.Point(81, 55);
            this.sourceBox2.Name = "sourceBox2";
            this.sourceBox2.Size = new System.Drawing.Size(362, 21);
            this.sourceBox2.TabIndex = 8;
            // 
            // sourceButton2
            // 
            this.sourceButton2.Location = new System.Drawing.Point(449, 55);
            this.sourceButton2.Name = "sourceButton2";
            this.sourceButton2.Size = new System.Drawing.Size(75, 23);
            this.sourceButton2.TabIndex = 9;
            this.sourceButton2.Text = "选择";
            this.sourceButton2.UseVisualStyleBackColor = true;
            this.sourceButton2.Click += new System.EventHandler(this.OnClickSelectPath);
            // 
            // tip
            // 
            this.tip.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tip.Font = new System.Drawing.Font("宋体", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tip.Location = new System.Drawing.Point(22, 273);
            this.tip.Name = "tip";
            this.tip.Size = new System.Drawing.Size(514, 23);
            this.tip.TabIndex = 10;
            this.tip.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // importButton
            // 
            this.importButton.AccessibleName = "";
            this.importButton.Location = new System.Drawing.Point(178, 146);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(157, 40);
            this.importButton.TabIndex = 11;
            this.importButton.Text = "英文表一键导入";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.OnClickImportEnglish);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 25);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 12;
            this.label4.Text = "M英文路径:";
            // 
            // englishBox1
            // 
            this.englishBox1.Location = new System.Drawing.Point(79, 20);
            this.englishBox1.Name = "englishBox1";
            this.englishBox1.Size = new System.Drawing.Size(362, 21);
            this.englishBox1.TabIndex = 13;
            // 
            // englishButton1
            // 
            this.englishButton1.Location = new System.Drawing.Point(447, 20);
            this.englishButton1.Name = "englishButton1";
            this.englishButton1.Size = new System.Drawing.Size(75, 23);
            this.englishButton1.TabIndex = 14;
            this.englishButton1.Text = "选择";
            this.englishButton1.UseVisualStyleBackColor = true;
            this.englishButton1.Click += new System.EventHandler(this.OnClickSelectPath);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 15;
            this.label5.Text = "S英文路径:";
            // 
            // englishBox2
            // 
            this.englishBox2.Location = new System.Drawing.Point(79, 56);
            this.englishBox2.Name = "englishBox2";
            this.englishBox2.Size = new System.Drawing.Size(362, 21);
            this.englishBox2.TabIndex = 16;
            // 
            // englishButton2
            // 
            this.englishButton2.Location = new System.Drawing.Point(447, 55);
            this.englishButton2.Name = "englishButton2";
            this.englishButton2.Size = new System.Drawing.Size(75, 23);
            this.englishButton2.TabIndex = 17;
            this.englishButton2.Text = "选择";
            this.englishButton2.UseVisualStyleBackColor = true;
            this.englishButton2.Click += new System.EventHandler(this.OnClickSelectPath);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(1, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(540, 248);
            this.tabControl1.TabIndex = 18;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.createButton);
            this.tabPage1.Controls.Add(this.sourceBox2);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.sourceBox1);
            this.tabPage1.Controls.Add(this.sourceButton1);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.destBox);
            this.tabPage1.Controls.Add(this.destButton);
            this.tabPage1.Controls.Add(this.sourceButton2);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(532, 222);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "翻译表生成";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.englishButton2);
            this.tabPage2.Controls.Add(this.englishBox2);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.englishButton1);
            this.tabPage2.Controls.Add(this.englishBox1);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.importButton);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(532, 222);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "英文数据一键导入";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // englishImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 312);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.tip);
            this.Name = "englishImport";
            this.Text = "Form1";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox sourceBox1;
        private System.Windows.Forms.Button sourceButton1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox destBox;
        private System.Windows.Forms.Button destButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox sourceBox2;
        private System.Windows.Forms.Button sourceButton2;
        private System.Windows.Forms.Label tip;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox englishBox1;
        private System.Windows.Forms.Button englishButton1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox englishBox2;
        private System.Windows.Forms.Button englishButton2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
    }
}

