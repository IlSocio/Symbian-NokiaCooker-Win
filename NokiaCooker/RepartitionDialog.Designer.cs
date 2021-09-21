namespace FuzzyByte.NokiaCooker
{
    partial class RepartitionDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnResize = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.openFileDialogVpl = new System.Windows.Forms.OpenFileDialog();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.bwOpenFile = new System.ComponentModel.BackgroundWorker();
            this.numUda = new System.Windows.Forms.NumericUpDown();
            this.numRofs3 = new System.Windows.Forms.NumericUpDown();
            this.numRofs2 = new System.Windows.Forms.NumericUpDown();
            this.numRofs1 = new System.Windows.Forms.NumericUpDown();
            this.lblSpace = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lblCap = new System.Windows.Forms.Label();
            this.bwResize = new System.ComponentModel.BackgroundWorker();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label31 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numUda)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRofs3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRofs2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRofs1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnResize
            // 
            this.btnResize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnResize.Location = new System.Drawing.Point(12, 227);
            this.btnResize.Name = "btnResize";
            this.btnResize.Size = new System.Drawing.Size(75, 28);
            this.btnResize.TabIndex = 0;
            this.btnResize.Text = "Save";
            this.btnResize.UseVisualStyleBackColor = true;
            this.btnResize.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(207, 227);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 28);
            this.button2.TabIndex = 1;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // openFileDialogVpl
            // 
            this.openFileDialogVpl.Filter = "Firmware files (*.vpl)|*.vpl";
            this.openFileDialogVpl.RestoreDirectory = true;
            this.openFileDialogVpl.Title = "Select Firmware File";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 103);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 17);
            this.label3.TabIndex = 28;
            this.label3.Text = "ROFS3: ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(62, 17);
            this.label2.TabIndex = 26;
            this.label2.Text = "ROFS2: ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 17);
            this.label1.TabIndex = 24;
            this.label1.Text = "ROFS1: ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(37, 131);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 17);
            this.label4.TabIndex = 30;
            this.label4.Text = "UDA: ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(112, 131);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(27, 17);
            this.label8.TabIndex = 38;
            this.label8.Text = "Mb";
            // 
            // bwOpenFile
            // 
            this.bwOpenFile.WorkerReportsProgress = true;
            this.bwOpenFile.WorkerSupportsCancellation = true;
            this.bwOpenFile.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwOpenFile_DoWork);
            this.bwOpenFile.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bwOpenFile_ProgressChanged);
            this.bwOpenFile.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwOpenFile_RunWorkerCompleted);
            // 
            // numUda
            // 
            this.numUda.Hexadecimal = true;
            this.numUda.Increment = new decimal(new int[] {
            1048576,
            0,
            0,
            0});
            this.numUda.Location = new System.Drawing.Point(88, 129);
            this.numUda.Maximum = new decimal(new int[] {
            -1,
            0,
            0,
            0});
            this.numUda.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numUda.Name = "numUda";
            this.numUda.ReadOnly = true;
            this.numUda.Size = new System.Drawing.Size(18, 22);
            this.numUda.TabIndex = 39;
            this.numUda.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numUda.ValueChanged += new System.EventHandler(this.numUda_ValueChanged);
            // 
            // numRofs3
            // 
            this.numRofs3.Hexadecimal = true;
            this.numRofs3.Increment = new decimal(new int[] {
            1048576,
            0,
            0,
            0});
            this.numRofs3.Location = new System.Drawing.Point(88, 101);
            this.numRofs3.Maximum = new decimal(new int[] {
            -1,
            0,
            0,
            0});
            this.numRofs3.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRofs3.Name = "numRofs3";
            this.numRofs3.ReadOnly = true;
            this.numRofs3.Size = new System.Drawing.Size(18, 22);
            this.numRofs3.TabIndex = 40;
            this.numRofs3.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRofs3.ValueChanged += new System.EventHandler(this.numRofs3_ValueChanged);
            // 
            // numRofs2
            // 
            this.numRofs2.Hexadecimal = true;
            this.numRofs2.Increment = new decimal(new int[] {
            1048576,
            0,
            0,
            0});
            this.numRofs2.Location = new System.Drawing.Point(88, 73);
            this.numRofs2.Maximum = new decimal(new int[] {
            -1,
            0,
            0,
            0});
            this.numRofs2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRofs2.Name = "numRofs2";
            this.numRofs2.ReadOnly = true;
            this.numRofs2.Size = new System.Drawing.Size(18, 22);
            this.numRofs2.TabIndex = 41;
            this.numRofs2.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRofs2.ValueChanged += new System.EventHandler(this.numRofs2_ValueChanged);
            // 
            // numRofs1
            // 
            this.numRofs1.Hexadecimal = true;
            this.numRofs1.Increment = new decimal(new int[] {
            1048576,
            0,
            0,
            0});
            this.numRofs1.Location = new System.Drawing.Point(88, 45);
            this.numRofs1.Maximum = new decimal(new int[] {
            -1,
            0,
            0,
            0});
            this.numRofs1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRofs1.Name = "numRofs1";
            this.numRofs1.ReadOnly = true;
            this.numRofs1.Size = new System.Drawing.Size(18, 22);
            this.numRofs1.TabIndex = 42;
            this.numRofs1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRofs1.ValueChanged += new System.EventHandler(this.numRofs1_ValueChanged);
            // 
            // lblSpace
            // 
            this.lblSpace.AutoSize = true;
            this.lblSpace.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblSpace.Location = new System.Drawing.Point(12, 196);
            this.lblSpace.Name = "lblSpace";
            this.lblSpace.Size = new System.Drawing.Size(40, 17);
            this.lblSpace.TabIndex = 44;
            this.lblSpace.Text = "aaaa";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(112, 103);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(27, 17);
            this.label7.TabIndex = 37;
            this.label7.Text = "Mb";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(112, 75);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(27, 17);
            this.label6.TabIndex = 36;
            this.label6.Text = "Mb";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(112, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(27, 17);
            this.label5.TabIndex = 35;
            this.label5.Text = "Mb";
            // 
            // lblCap
            // 
            this.lblCap.AutoSize = true;
            this.lblCap.Location = new System.Drawing.Point(112, 164);
            this.lblCap.Name = "lblCap";
            this.lblCap.Size = new System.Drawing.Size(27, 17);
            this.lblCap.TabIndex = 46;
            this.lblCap.Text = "Mb";
            // 
            // bwResize
            // 
            this.bwResize.WorkerReportsProgress = true;
            this.bwResize.WorkerSupportsCancellation = true;
            this.bwResize.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwResize_DoWork);
            this.bwResize.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bwResize_ProgressChanged);
            this.bwResize.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwResize_RunWorkerCompleted);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(209, 19);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 17);
            this.label10.TabIndex = 47;
            this.label10.Text = "Min. Size";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(85, 19);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(66, 17);
            this.label11.TabIndex = 48;
            this.label11.Text = "New Size";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(209, 131);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(27, 17);
            this.label12.TabIndex = 52;
            this.label12.Text = "Mb";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(209, 103);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(27, 17);
            this.label13.TabIndex = 51;
            this.label13.Text = "Mb";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(209, 75);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(27, 17);
            this.label14.TabIndex = 50;
            this.label14.Text = "Mb";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(209, 47);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(27, 17);
            this.label15.TabIndex = 49;
            this.label15.Text = "Mb";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(37, 164);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(79, 17);
            this.label16.TabIndex = 62;
            this.label16.Text = "Total Size: ";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 7);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(79, 17);
            this.label9.TabIndex = 67;
            this.label9.Text = "Erase Area";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(186, 7);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(62, 17);
            this.label17.TabIndex = 68;
            this.label17.Text = "Sections";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(3, 35);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(54, 17);
            this.label18.TabIndex = 69;
            this.label18.Text = "label18";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(3, 63);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(54, 17);
            this.label19.TabIndex = 70;
            this.label19.Text = "label19";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(3, 91);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(54, 17);
            this.label20.TabIndex = 71;
            this.label20.Text = "label20";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(3, 119);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(54, 17);
            this.label21.TabIndex = 72;
            this.label21.Text = "label21";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(186, 119);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(54, 17);
            this.label22.TabIndex = 76;
            this.label22.Text = "label22";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(186, 91);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(54, 17);
            this.label23.TabIndex = 75;
            this.label23.Text = "label23";
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(186, 63);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(54, 17);
            this.label24.TabIndex = 74;
            this.label24.Text = "label24";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(186, 35);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(54, 17);
            this.label25.TabIndex = 73;
            this.label25.Text = "label25";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(370, 119);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(54, 17);
            this.label26.TabIndex = 81;
            this.label26.Text = "label26";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(370, 91);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(54, 17);
            this.label27.TabIndex = 80;
            this.label27.Text = "label27";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(370, 63);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(54, 17);
            this.label28.TabIndex = 79;
            this.label28.Text = "label28";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(370, 35);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(54, 17);
            this.label29.TabIndex = 78;
            this.label29.Text = "label29";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(370, 7);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(114, 17);
            this.label30.TabIndex = 77;
            this.label30.Text = "Blocks Locations";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.label31);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.label26);
            this.panel1.Controls.Add(this.label17);
            this.panel1.Controls.Add(this.label27);
            this.panel1.Controls.Add(this.label18);
            this.panel1.Controls.Add(this.label28);
            this.panel1.Controls.Add(this.label19);
            this.panel1.Controls.Add(this.label29);
            this.panel1.Controls.Add(this.label20);
            this.panel1.Controls.Add(this.label30);
            this.panel1.Controls.Add(this.label21);
            this.panel1.Controls.Add(this.label22);
            this.panel1.Controls.Add(this.label25);
            this.panel1.Controls.Add(this.label23);
            this.panel1.Controls.Add(this.label24);
            this.panel1.Location = new System.Drawing.Point(183, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(99, 209);
            this.panel1.TabIndex = 82;
            this.panel1.Visible = false;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(3, 152);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(54, 17);
            this.label31.TabIndex = 82;
            this.label31.Text = "label31";
            // 
            // RepartitionDialog
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.ClientSize = new System.Drawing.Size(294, 267);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.lblCap);
            this.Controls.Add(this.lblSpace);
            this.Controls.Add(this.numRofs1);
            this.Controls.Add(this.numRofs2);
            this.Controls.Add(this.numRofs3);
            this.Controls.Add(this.numUda);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnResize);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "RepartitionDialog";
            this.Text = "Partition Manager";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RepartitionDialog_FormClosed);
            this.Load += new System.EventHandler(this.RepartitionDialog_Load);
            this.Shown += new System.EventHandler(this.RepartitionDialog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.numUda)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRofs3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRofs2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRofs1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnResize;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.OpenFileDialog openFileDialogVpl;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label8;
        private System.ComponentModel.BackgroundWorker bwOpenFile;
        private System.Windows.Forms.NumericUpDown numUda;
        private System.Windows.Forms.NumericUpDown numRofs3;
        private System.Windows.Forms.NumericUpDown numRofs2;
        private System.Windows.Forms.NumericUpDown numRofs1;
        private System.Windows.Forms.Label lblSpace;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblCap;
        private System.ComponentModel.BackgroundWorker bwResize;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label31;
    }
}