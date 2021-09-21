
using FuzzyByte.Forms;
using FuzzyByte;


namespace FuzzyByte.NokiaCooker
{
    partial class Form1
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Liberare le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            XPTable.Models.DataSourceColumnBinder dataSourceColumnBinder1 = new XPTable.Models.DataSourceColumnBinder();
            XPTable.Renderers.DragDropRenderer dragDropRenderer1 = new XPTable.Renderers.DragDropRenderer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel4 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel5 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel3 = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFATMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importROFSMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importUnknMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nokiaCookerBETA01MarcoBellinoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeViewAdv1 = new TreeViewAdv();
            this.label1 = new System.Windows.Forms.Label();
            this.tableFileList = new XPTable.Models.Table();
            this.columnModel1 = new XPTable.Models.ColumnModel();
            this.colFilename = new XPTable.Models.ImageColumn();
            this.colSize = new XPTable.Models.NumberColumn();
            this.colDate = new XPTable.Models.DateTimeColumn();
            this.colType = new XPTable.Models.TextColumn();
            this.colExt = new XPTable.Models.TextColumn();
            this.tableModel1 = new XPTable.Models.TableModel();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.lblFolders = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblFiles = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblSize = new System.Windows.Forms.ToolStripStatusLabel();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.openFilebw = new System.ComponentModel.BackgroundWorker();
            this.bwRepack = new System.ComponentModel.BackgroundWorker();
            this.button1 = new System.Windows.Forms.Button();
            this.openROFS = new System.Windows.Forms.OpenFileDialog();
            this.openFAT = new System.Windows.Forms.OpenFileDialog();
            this.openUnkn = new System.Windows.Forms.OpenFileDialog();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnDelete = new System.Windows.Forms.ToolStripButton();
            this.btnDonate = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnResize = new System.Windows.Forms.ToolStripButton();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.textAreaAdv1 = new TextAreaAdv();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tableFileList)).BeginInit();
            this.statusStrip2.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel4,
            this.toolStripStatusLabel5,
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel2,
            this.toolStripStatusLabel3});
            this.statusStrip1.Location = new System.Drawing.Point(0, 467);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(860, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel4
            // 
            this.toolStripStatusLabel4.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.toolStripStatusLabel4.Name = "toolStripStatusLabel4";
            this.toolStripStatusLabel4.Size = new System.Drawing.Size(108, 17);
            this.toolStripStatusLabel4.Text = "Other useful stuff:";
            // 
            // toolStripStatusLabel5
            // 
            this.toolStripStatusLabel5.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel5.IsLink = true;
            this.toolStripStatusLabel5.Name = "toolStripStatusLabel5";
            this.toolStripStatusLabel5.Size = new System.Drawing.Size(56, 17);
            this.toolStripStatusLabel5.Text = "NaviFirm";
            this.toolStripStatusLabel5.Click += new System.EventHandler(this.toolStripStatusLabel5_Click);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel1.IsLink = true;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(86, 17);
            this.toolStripStatusLabel1.Text = "ROMPatcher+";
            this.toolStripStatusLabel1.Click += new System.EventHandler(this.toolStripStatusLabel1_Click);
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel2.IsLink = true;
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(67, 17);
            this.toolStripStatusLabel2.Text = "SISXplorer";
            this.toolStripStatusLabel2.Click += new System.EventHandler(this.toolStripStatusLabel2_Click);
            // 
            // toolStripStatusLabel3
            // 
            this.toolStripStatusLabel3.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.toolStripStatusLabel3.IsLink = true;
            this.toolStripStatusLabel3.Name = "toolStripStatusLabel3";
            this.toolStripStatusLabel3.Size = new System.Drawing.Size(119, 17);
            this.toolStripStatusLabel3.Text = "Guardian PLATINUM";
            this.toolStripStatusLabel3.Click += new System.EventHandler(this.toolStripStatusLabel3_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.advancedToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(860, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openMenuItem,
            this.closeMenuItem,
            this.saveMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openMenuItem
            // 
            this.openMenuItem.Image = global::FuzzyByte.NokiaCooker.Properties.Resources.OpenHH;
            this.openMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
            this.openMenuItem.Name = "openMenuItem";
            this.openMenuItem.Size = new System.Drawing.Size(111, 22);
            this.openMenuItem.Text = "Open";
            this.openMenuItem.Click += new System.EventHandler(this.openMenuItem_Click);
            // 
            // closeMenuItem
            // 
            this.closeMenuItem.Enabled = false;
            this.closeMenuItem.Name = "closeMenuItem";
            this.closeMenuItem.Size = new System.Drawing.Size(111, 22);
            this.closeMenuItem.Text = "Close";
            this.closeMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // saveMenuItem
            // 
            this.saveMenuItem.Enabled = false;
            this.saveMenuItem.Image = global::FuzzyByte.NokiaCooker.Properties.Resources.SaveHH;
            this.saveMenuItem.ImageTransparentColor = System.Drawing.Color.Black;
            this.saveMenuItem.Name = "saveMenuItem";
            this.saveMenuItem.Size = new System.Drawing.Size(111, 22);
            this.saveMenuItem.Text = "Save";
            this.saveMenuItem.Click += new System.EventHandler(this.saveMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(111, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // advancedToolStripMenuItem
            // 
            this.advancedToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importFATMenuItem,
            this.importROFSMenuItem,
            this.importUnknMenuItem});
            this.advancedToolStripMenuItem.Name = "advancedToolStripMenuItem";
            this.advancedToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.advancedToolStripMenuItem.Text = "Advanced";
            // 
            // importFATMenuItem
            // 
            this.importFATMenuItem.Enabled = false;
            this.importFATMenuItem.Name = "importFATMenuItem";
            this.importFATMenuItem.Size = new System.Drawing.Size(256, 22);
            this.importFATMenuItem.Text = "Repack using fat data: FAT.IMA";
            this.importFATMenuItem.Click += new System.EventHandler(this.importFATIMAToolStripMenuItem_Click);
            // 
            // importROFSMenuItem
            // 
            this.importROFSMenuItem.Enabled = false;
            this.importROFSMenuItem.Name = "importROFSMenuItem";
            this.importROFSMenuItem.Size = new System.Drawing.Size(256, 22);
            this.importROFSMenuItem.Text = "Repack using rofs data: ROFS.ROM";
            this.importROFSMenuItem.Click += new System.EventHandler(this.importROFSROMToolStripMenuItem_Click);
            // 
            // importUnknMenuItem
            // 
            this.importUnknMenuItem.Enabled = false;
            this.importUnknMenuItem.Name = "importUnknMenuItem";
            this.importUnknMenuItem.Size = new System.Drawing.Size(256, 22);
            this.importUnknMenuItem.Text = "Repack using raw data: UNKN.BIN";
            this.importUnknMenuItem.Click += new System.EventHandler(this.repackUnknMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nokiaCookerBETA01MarcoBellinoToolStripMenuItem});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // nokiaCookerBETA01MarcoBellinoToolStripMenuItem
            // 
            this.nokiaCookerBETA01MarcoBellinoToolStripMenuItem.Name = "nokiaCookerBETA01MarcoBellinoToolStripMenuItem";
            this.nokiaCookerBETA01MarcoBellinoToolStripMenuItem.Size = new System.Drawing.Size(225, 22);
            this.nokiaCookerBETA01MarcoBellinoToolStripMenuItem.Text = "NokiaCooker by Marco Bellino";
            this.nokiaCookerBETA01MarcoBellinoToolStripMenuItem.Click += new System.EventHandler(this.nokiaCookerBETA01MarcoBellinoToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeViewAdv1);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tableFileList);
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip2);
            this.splitContainer1.Size = new System.Drawing.Size(860, 240);
            this.splitContainer1.SplitterDistance = 240;
            this.splitContainer1.TabIndex = 6;
            // 
            // treeViewAdv1
            // 
            this.treeViewAdv1.AllowDrop = true;
            this.treeViewAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewAdv1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
            this.treeViewAdv1.HideSelection = false;
            this.treeViewAdv1.HotTracking = true;
            this.treeViewAdv1.Location = new System.Drawing.Point(0, 20);
            this.treeViewAdv1.Name = "treeViewAdv1";
            this.treeViewAdv1.ShowRootLines = false;
            this.treeViewAdv1.Size = new System.Drawing.Size(240, 220);
            this.treeViewAdv1.TabIndex = 1;
            this.treeViewAdv1.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewAdv1_DragDrop);
            this.treeViewAdv1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewAdv1_AfterSelect);
            this.treeViewAdv1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.treeViewAdv1_PreviewKeyDown);
            this.treeViewAdv1.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewAdv1_DragEnter);
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(240, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Custom Firmwares";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableFileList
            // 
            this.tableFileList.AllowDrop = true;
            this.tableFileList.AllowRMBSelection = true;
            this.tableFileList.BorderColor = System.Drawing.Color.Black;
            this.tableFileList.ColumnModel = this.columnModel1;
            this.tableFileList.DataMember = null;
            this.tableFileList.DataSourceColumnBinder = dataSourceColumnBinder1;
            this.tableFileList.Dock = System.Windows.Forms.DockStyle.Fill;
            dragDropRenderer1.ForeColor = System.Drawing.Color.Red;
            this.tableFileList.DragDropRenderer = dragDropRenderer1;
            this.tableFileList.GridColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.tableFileList.GridLines = XPTable.Models.GridLines.Columns;
            this.tableFileList.Location = new System.Drawing.Point(0, 0);
            this.tableFileList.MultiSelect = true;
            this.tableFileList.Name = "tableFileList";
            this.tableFileList.NoItemsText = "Drag & Drop files over this Area to add them to the Cooked Firmware.";
            this.tableFileList.Size = new System.Drawing.Size(616, 218);
            this.tableFileList.TabIndex = 0;
            this.tableFileList.TableModel = this.tableModel1;
            this.tableFileList.Text = "table1";
            this.tableFileList.UnfocusedBorderColor = System.Drawing.Color.Black;
            this.tableFileList.DragDrop += new System.Windows.Forms.DragEventHandler(this.tableFileList_DragDrop);
            this.tableFileList.DragEnter += new System.Windows.Forms.DragEventHandler(this.tableFileList_DragEnter);
            this.tableFileList.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.tableFileList_PreviewKeyDown);
            this.tableFileList.DoubleClick += new System.EventHandler(this.tableFileList_DoubleClick);
            // 
            // columnModel1
            // 
            this.columnModel1.Columns.AddRange(new XPTable.Models.Column[] {
            this.colFilename,
            this.colSize,
            this.colDate,
            this.colType,
            this.colExt});
            // 
            // colFilename
            // 
            this.colFilename.IsTextTrimmed = false;
            this.colFilename.Text = "Filename";
            this.colFilename.Width = 250;
            // 
            // colSize
            // 
            this.colSize.Alignment = XPTable.Models.ColumnAlignment.Right;
            this.colSize.Editable = false;
            this.colSize.Format = "";
            this.colSize.IsTextTrimmed = false;
            this.colSize.Text = "Size";
            this.colSize.Width = 100;
            // 
            // colDate
            // 
            this.colDate.Editable = false;
            this.colDate.IsTextTrimmed = false;
            this.colDate.ShowDropDownButton = false;
            this.colDate.Text = "Last Modified";
            this.colDate.Visible = false;
            this.colDate.Width = 130;
            // 
            // colType
            // 
            this.colType.IsTextTrimmed = false;
            this.colType.Text = "Type";
            this.colType.Width = 180;
            // 
            // colExt
            // 
            this.colExt.IsTextTrimmed = false;
            this.colExt.Text = "Extension";
            this.colExt.Width = 70;
            // 
            // tableModel1
            // 
            this.tableModel1.RowHeight = 17;
            // 
            // statusStrip2
            // 
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblFolders,
            this.lblFiles,
            this.lblSize});
            this.statusStrip2.Location = new System.Drawing.Point(0, 218);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Size = new System.Drawing.Size(616, 22);
            this.statusStrip2.SizingGrip = false;
            this.statusStrip2.TabIndex = 10;
            this.statusStrip2.Text = "statusStrip2";
            // 
            // lblFolders
            // 
            this.lblFolders.Name = "lblFolders";
            this.lblFolders.Size = new System.Drawing.Size(61, 17);
            this.lblFolders.Text = "Folders: 10";
            // 
            // lblFiles
            // 
            this.lblFiles.Name = "lblFiles";
            this.lblFiles.Size = new System.Drawing.Size(47, 17);
            this.lblFiles.Text = "Files: 20";
            // 
            // lblSize
            // 
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(45, 17);
            this.lblSize.Text = "Size: 11";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Firmware files (*.fpsx; *.v??)|*.fpsx;*.v??|All files (*.*)|*.*";
            this.openFileDialog1.RestoreDirectory = true;
            this.openFileDialog1.Title = "Select Firmware File";
            // 
            // openFilebw
            // 
            this.openFilebw.WorkerReportsProgress = true;
            this.openFilebw.WorkerSupportsCancellation = true;
            this.openFilebw.DoWork += new System.ComponentModel.DoWorkEventHandler(this.openFilebw_DoWork);
            this.openFilebw.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.openFilebw_RunWorkerCompleted);
            this.openFilebw.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.openFilebw_ProgressChanged);
            // 
            // bwRepack
            // 
            this.bwRepack.WorkerReportsProgress = true;
            this.bwRepack.WorkerSupportsCancellation = true;
            this.bwRepack.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwRepack_DoWork);
            this.bwRepack.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwRepack_RunWorkerCompleted);
            this.bwRepack.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bwRepack_ProgressChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(363, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // openROFS
            // 
            this.openROFS.Filter = "ROFS-ROFX files (*.rom)|*.rom|All files (*.*)|*.*";
            this.openROFS.RestoreDirectory = true;
            this.openROFS.Title = "Select ROFS-ROFX File";
            // 
            // openFAT
            // 
            this.openFAT.Filter = "FAT files (*.ima)|*.ima|All files (*.*)|*.*";
            this.openFAT.RestoreDirectory = true;
            this.openFAT.Title = "Select FAT File";
            // 
            // openUnkn
            // 
            this.openUnkn.Filter = "RAW data file (*.bin;*.raw)|*.bin;*.raw|All files (*.*)|*.*";
            this.openUnkn.RestoreDirectory = true;
            this.openUnkn.Title = "Select RAW data file";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 52);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 52);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpen,
            this.btnSave,
            this.toolStripSeparator1,
            this.btnDelete,
            this.toolStripSeparator2,
            this.btnDonate,
            this.toolStripSeparator3,
            this.btnResize});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(860, 52);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnOpen
            // 
            this.btnOpen.Image = global::FuzzyByte.NokiaCooker.Properties.Resources.OpenHH;
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Black;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(84, 49);
            this.btnOpen.Text = "Open Firmware";
            this.btnOpen.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnOpen.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // btnSave
            // 
            this.btnSave.Enabled = false;
            this.btnSave.Image = global::FuzzyByte.NokiaCooker.Properties.Resources.SaveHH;
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Black;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(82, 49);
            this.btnSave.Text = "Save Firmware";
            this.btnSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnSave.Click += new System.EventHandler(this.toolStripButton1_Click_1);
            // 
            // btnDelete
            // 
            this.btnDelete.Enabled = false;
            this.btnDelete.Image = global::FuzzyByte.NokiaCooker.Properties.Resources.Cancel24;
            this.btnDelete.ImageTransparentColor = System.Drawing.Color.Black;
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(117, 49);
            this.btnDelete.Text = "Delete Files or Folders";
            this.btnDelete.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnDonate
            // 
            this.btnDonate.Image = global::FuzzyByte.NokiaCooker.Properties.Resources.Donate;
            this.btnDonate.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnDonate.ImageTransparentColor = System.Drawing.Color.Black;
            this.btnDonate.Name = "btnDonate";
            this.btnDonate.Size = new System.Drawing.Size(112, 49);
            this.btnDonate.Text = "Support NokiaCooker";
            this.btnDonate.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnDonate.ToolTipText = "Make a Donation to Support the Development of this Project";
            this.btnDonate.Click += new System.EventHandler(this.btnDonate_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 52);
            // 
            // btnResize
            // 
            this.btnResize.Image = global::FuzzyByte.NokiaCooker.Properties.Resources.chk_off;
            this.btnResize.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnResize.ImageTransparentColor = System.Drawing.Color.Black;
            this.btnResize.Name = "btnResize";
            this.btnResize.Size = new System.Drawing.Size(152, 49);
            this.btnResize.Text = "Allow Resizing of ROFS/ROFx";
            this.btnResize.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnResize.Click += new System.EventHandler(this.btnResize_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 101);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.textAreaAdv1);
            this.splitContainer2.Size = new System.Drawing.Size(860, 366);
            this.splitContainer2.SplitterDistance = 240;
            this.splitContainer2.TabIndex = 8;
            // 
            // textAreaAdv1
            // 
            this.textAreaAdv1.BackColorText = System.Drawing.SystemColors.Info;
            this.textAreaAdv1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textAreaAdv1.Location = new System.Drawing.Point(0, 0);
            this.textAreaAdv1.Name = "textAreaAdv1";
            this.textAreaAdv1.Size = new System.Drawing.Size(860, 122);
            this.textAreaAdv1.TabIndex = 1;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(444, 4);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(88, 17);
            this.checkBox1.TabIndex = 9;
            this.checkBox1.Text = "don\'t unpack";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // toolStrip2
            // 
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel2,
            this.toolStripSeparator4});
            this.toolStrip2.Location = new System.Drawing.Point(0, 76);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(860, 25);
            this.toolStrip2.TabIndex = 10;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(44, 22);
            this.toolStripLabel2.Text = "Plugins:";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(860, 489);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.statusStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "NokiaCooker BETA 0.6.6";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tableFileList)).EndInit();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel4;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel5;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem closeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nokiaCookerBETA01MarcoBellinoToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker openFilebw;
        private System.ComponentModel.BackgroundWorker bwRepack;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ToolStripMenuItem advancedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importFATMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importROFSMenuItem;
        private System.Windows.Forms.OpenFileDialog openROFS;
        private TreeViewAdv treeViewAdv1;
        private System.Windows.Forms.Label label1;
        private XPTable.Models.Table tableFileList;
        private XPTable.Models.ColumnModel columnModel1;
        private XPTable.Models.TableModel tableModel1;
        private XPTable.Models.ImageColumn colFilename;
        private XPTable.Models.NumberColumn colSize;
        private XPTable.Models.DateTimeColumn colDate;
        private System.Windows.Forms.StatusStrip statusStrip2;
        private System.Windows.Forms.ToolStripStatusLabel lblFolders;
        private System.Windows.Forms.ToolStripStatusLabel lblFiles;
        private System.Windows.Forms.ToolStripStatusLabel lblSize;
        private XPTable.Models.TextColumn colType;
        private System.Windows.Forms.OpenFileDialog openFAT;
        private System.Windows.Forms.ToolStripMenuItem importUnknMenuItem;
        private System.Windows.Forms.OpenFileDialog openUnkn;
        private XPTable.Models.TextColumn colExt;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnDelete;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnDonate;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private TextAreaAdv textAreaAdv1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnResize;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel3;
    }
}

