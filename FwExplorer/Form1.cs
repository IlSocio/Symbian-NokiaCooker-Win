#define USE_BACKGOUND_WORKER

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using Firmware;
using System.IO;
using System.Diagnostics;
using FuzzyByte;
using FuzzyByte.Forms;

using XPTable.Models;
using XPTable;
using XPTable.Renderers;
using XPTable.Sorting;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Security.AccessControl;
using System.Reflection;


namespace FuzzyByte.NokiaCooker
{

    public partial class Form1 : Form
    {
        private Process plugin = null;
        private bool iAllowResizing = false;
        private SystemFiles imgListSystem = new SystemFiles();
        private SystemFiles imgListPlugins = new SystemFiles();
        private FwFile fwFile = null;
        private string TmpImages = "Images";
        private string TmpFiles = "Files" ;
        private string PluginsPath =  "Plugins" ;
        private string SandboxPath = "Sandbox";
        private readonly string ROFS_IMG = "ROFS.ROM";
        private readonly string ROFX_IMG = "ROFX.ROM";
        private readonly string CORE_IMG = "CORE.ROM";
        private readonly string FAT_IMG = "FAT.IMA";
        private readonly string UNKN_IMG = "UNKN.BIN";


        public Form1()
        {
            InitializeComponent();
            
            colSize.Renderer = new TextCellRenderer();
            colDate.Renderer = new TextCellRenderer();
            colDate.ShowDropDownButton = false;

            tableModel1.Table.DragDropRenderer = null;

            lblFolders.Text = lblFiles.Text = lblSize.Text = "";

            TmpFiles = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + TmpFiles + Path.DirectorySeparatorChar;
            TmpImages = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + TmpImages + Path.DirectorySeparatorChar;
            PluginsPath = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + PluginsPath + Path.DirectorySeparatorChar;
            SandboxPath = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + SandboxPath + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(TmpFiles);
            Directory.CreateDirectory(TmpImages);
            Directory.CreateDirectory(PluginsPath);

            imgListSystem.SmallIconsImageList.Images.Add(NokiaCooker.Properties.Resources.cellphone);
            imgListSystem.LargeIconsImageList.Images.Add(NokiaCooker.Properties.Resources.cellphone);
            imgListSystem.GetIconIndex(Environment.SystemDirectory);
            imgListSystem.GetIconIndex(Environment.SystemDirectory, 2);

            // TODO: Selezione tramite quadrato
            // TODO: Check updates...
            // TODO: Recents
            treeViewAdv1.ImageList = imgListSystem.SmallIconsImageList;

            fwFile = new FwFile();
            fwFile.Log += new FuzzyByte.LogHandler(fwFile_Log);

#if !DEBUG
            button1.Visible = false;
            checkBox1.Visible = false;
#endif
        }

        private void GUI_SetSaveEnabled(bool state)
        {
            importFATMenuItem.Enabled = state;
            importROFSMenuItem.Enabled = state;
            importUnknMenuItem.Enabled = state;
            btnSave.Enabled = state;
            btnDelete.Enabled = state;
            saveMenuItem.Enabled = state;
            closeMenuItem.Enabled = state;
        }

        private void GUI_SetAllEnabled(bool state)
        {
            menuStrip1.Enabled = state;
            toolStrip1.Enabled = state;
            toolStrip2.Enabled = state;
            treeViewAdv1.Enabled = state;
            if (state)
                Cursor = Cursors.Default;
            else
                Cursor = Cursors.WaitCursor;
            tableFileList.Enabled = state;
            textAreaAdv1.Cursor = Cursor;
        }

        private string FormatFileSize(long size)
        {
            return FormatFileSize(size, 1);
        }

        private string FormatFileSize(long size, int level)
        {
            int[] limits = new int[] { 1024 * 1024 * 1024, 1024 * 1024, 1024 };
            string[] units = new string[] { "GB", "MB", "KB" };
            Array.Resize(ref units, level);
            Array.Resize(ref limits, level);

            for (int i = 0; i < limits.Length; i++)
            {
                if (size >= limits[i])
                    return String.Format("{0:#,##0.##} " + units[i], ((double)size / limits[i]));
            }

            return String.Format("{0} bytes", size); ;
        }


        private Row CreateNewRow(FileSystemInfo fileInfo)
        {
            int index = imgListSystem.GetIconIndex(fileInfo.FullName);
            long fileSize = -1;
            if (fileInfo is FileInfo)
            {
                FileInfo finfo = fileInfo as FileInfo;
                fileSize = finfo.Length;
            }
            string type = imgListSystem.GetFileType(fileInfo.FullName); // ShellUtilities.GetFileType(fileInfo.FullName);
            //string type = "";
            return CreateNewRow(fileInfo.Name, imgListSystem.SmallIconsImageList.Images[index], fileSize, fileInfo.LastWriteTime, type);
        }

        private Row CreateNewRow(string filename, Image img, long fileSize, DateTime date, string type)
        {
            Cell cell0 = new Cell(filename, img);
            Cell cell1 = new Cell("", fileSize);
            Cell cell2 = new Cell(date.ToShortDateString() +" "+ date.ToShortTimeString(), DateTime.MinValue);
            Cell cell3 = new Cell(type);
            Cell cell4 = new Cell(Path.GetExtension(filename));

            if (fileSize >= 0)  // File...
            {
                cell0.Data = filename;
                cell1.Text = FormatFileSize(fileSize);
                cell2.Data = date;
            }

            Cell[] cells = new Cell[] { cell0, cell1, cell2, cell3, cell4};
            XPTable.Models.Row aRow = new XPTable.Models.Row(cells);
            return aRow;
        }


        private void GUI_PopulateFileListFromPath(string path)
        {

            tableModel1.Rows.Clear();
            tableModel1.Selections.Clear();
            DirectoryInfo pathInfo = new DirectoryInfo(path);
            if (!pathInfo.Exists)
                return;
            treeViewAdv1.BeginUpdate();

            Cursor.Current = Cursors.WaitCursor;

            int i = 0;
            int totFolders = 0;
            int totFiles = 0;
            long totSize = 0;
            tableFileList.SuspendLayout();
            foreach (FileSystemInfo fileInfo in pathInfo.GetFileSystemInfos())
            {
                Row aRow = CreateNewRow(fileInfo);
                tableModel1.Rows.Add(aRow);
                i++;
                if (fileInfo is FileInfo)
                {
                    totFiles++;
                    totSize += (fileInfo as FileInfo).Length;
                } else totFolders++;

                if (i % 200 == 0)
                {
                    tableFileList.ResumeLayout();
                    tableFileList.Refresh();
                    tableFileList.SuspendLayout();
                }
            }
            tableFileList.ResumeLayout();

            SortOrder order = SortOrder.None;
            int sortCol = tableModel1.Table.SortingColumn;
            if (sortCol >= 0)
                order = columnModel1.Columns[sortCol].SortOrder;
            tableModel1.Table.Sort(tableModel1.Table.SortingColumn, order);

            lblFolders.Text = "";
            if (totFolders > 0)
                lblFolders.Text = "Folders: " + totFolders;

            lblFiles.Text = "";
            lblSize.Text = "";
            if (totFiles > 0)
            {
                lblFiles.Text += "Files: " + totFiles;
                lblSize.Text += "Size: " + FormatFileSize(totSize, 3);
            }

            Cursor.Current = Cursors.Default;
            treeViewAdv1.EndUpdate();
        }

        // flag run in sandbox

        private void GUI_AddPluginButton(string exeFile)
        {
            string tooltip = "";
            try
            {
                System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFrom(exeFile);
                tooltip = AssemblyInfo.Description(assembly);
            }
            catch (Exception ex)
            {
            }
            ToolStripButton button = new ToolStripButton();
            button.ImageIndex = imgListPlugins.GetIconIndex(exeFile);
            button.Text = Path.GetFileNameWithoutExtension(exeFile);
            button.ToolTipText = tooltip;
            button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            button.Click += new EventHandler(button_Click);
            button.Tag = exeFile;

            toolStrip2.Items.Add(button);
            ToolStripSeparator separ = new ToolStripSeparator();
            toolStrip2.Items.Add(separ);
        }


        private void ScanForIntegratedPlugins(string path)
        {
            if (!Directory.Exists(path))
                return;
            foreach (string exeName in Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories))
            {
                // Non carica i plugin che non hanno una loro directory...
                string plugDir = Path.GetDirectoryName(exeName) + Path.DirectorySeparatorChar;
                if (plugDir.ToLower() == PluginsPath.ToLower())
                    continue;
                string key = exeName.Trim().ToLower();
                foreach (string s in NokiaCooker.Properties.Settings.Default.Plugins)
                {
                }
                NokiaCooker.Properties.Settings.Default.Reload();
                if (!NokiaCooker.Properties.Settings.Default.Plugins.Contains(key))
                {
                    NokiaCooker.Properties.Settings.Default.Plugins.Add(key);
                    NokiaCooker.Properties.Settings.Default.Save();
                }
            }
        }


        private void GUI_UpdatePluginsToolbar()
        {
            ScanForIntegratedPlugins(PluginsPath);
            toolStrip2.Items.Clear();
            // Scan della configurazione e aggiunta dei bottoni...
            foreach (string exeName in NokiaCooker.Properties.Settings.Default.Plugins)
            {
                GUI_AddPluginButton(exeName);
            }
        }


/*      private void GUI_LoadUntrustedPlugins()
        {            

            foreach (string pluginName in Directory.GetFiles(UntrustedPluginsPath, "*.exe", SearchOption.AllDirectories))
            {
                // Check if it is an Exe PE
                FileStream fs = File.OpenRead(pluginName);
                byte b1 = (byte)fs.ReadByte();
                byte b2 = (byte)fs.ReadByte();
                fs.Close();
                if (b1 != 0x4D || b2 != 0x5A)
                    continue;

                // Non carica i plugin che non hanno una loro directory...
                string plugDir = Path.GetDirectoryName(pluginName) + Path.DirectorySeparatorChar;
                if (plugDir.ToLower() == UntrustedPluginsPath.ToLower())
                    continue;

                ToolStripButton button = new ToolStripButton();
                button.ImageIndex = imgListPlugins.GetIconIndex(pluginName);
                button.Text = Path.GetFileNameWithoutExtension(pluginName);
                button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
                button.Click += new EventHandler(buttonUntrust_Click);
                button.Tag = pluginName;

                toolStrip3.Items.Add(button);
                ToolStripSeparator separ = new ToolStripSeparator();
                toolStrip3.Items.Add(separ);
            }
        }*/

        private Process CreateProcess(string exeName)
        {
            Process rCompExe = new Process();
            rCompExe.StartInfo.UseShellExecute = true;
            /*rCompExe.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            rCompExe.StartInfo.RedirectStandardOutput = true;
            rCompExe.StartInfo.RedirectStandardError = true;
            rCompExe.StartInfo.CreateNoWindow = false;*/
            rCompExe.StartInfo.FileName = exeName;
            rCompExe.EnableRaisingEvents = true;
            rCompExe.StartInfo.WorkingDirectory = Path.GetDirectoryName(rCompExe.StartInfo.FileName);
            return rCompExe;
        }


        private delegate void ExitedDelegate(object sender, EventArgs e);

        void rCompExe_Exited(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                MethodInvoker d = delegate { rCompExe_Exited(sender, e); };
                this.Invoke(d);
/*                ExitedDelegate d = new ExitedDelegate(rCompExe_Exited);
                this.Invoke(d, new object[] { sender, e });*/
            }
            else
            {
                plugin.Close();
                this.Enabled = true;
            }
        }

        
        void button_Click(object sender, EventArgs e)
        {
            ToolStripButton btn = (sender as ToolStripButton);
            string exePath = btn.Tag.ToString();

            plugin = CreateProcess(exePath);
            plugin.Exited += new EventHandler(rCompExe_Exited);
            // Launch plugin
            string arguments = "\"" + TmpFiles + "\" \"" + Path.GetFileName(fwFile.filename) + "\" ";
            switch (fwFile.FwType)
            {
                case TFwType.CORE:
                    arguments += "CORE";             // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.ROFS:
                    arguments += "ROFS";             // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.ROFX:
                    arguments += "ROFX";             // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.UDA:
                    arguments += "UDA";              // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.UNKNOWN:
                    arguments += "UNKNOWN";          // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
            }

            this.Enabled = false;
            plugin.StartInfo.Arguments = arguments;
            plugin.Start();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
#if !DEBUG
            DateTime now = DateTime.Now;
            if (now.Month != 11)
            {
                GUI.ShowWarning("This BETA version is too much old! Please download the new version from the official website http://www.symbian-toys.com/");
                Application.Exit();
            }
#endif
            toolStrip2.ImageList = imgListPlugins.SmallIconsImageList;

            GUI_UpdatePluginsToolbar();
        }


        private string GetDeveloperPath(string plugExe)
        {
            string plugDir = Path.GetDirectoryName(plugExe) + Path.DirectorySeparatorChar;
            plugDir = plugDir.Substring(PluginsPath.Length);
            string[] dirs = plugDir.Split(Path.DirectorySeparatorChar);
            return PluginsPath + dirs[0] + Path.DirectorySeparatorChar;
        }

/*
        void button_Click(object sender, EventArgs e)
        {
            ToolStripButton btn = (sender as ToolStripButton);
            //string exePath = @"C:\ProgettiInCorso\NokiaCooker\TestPlugins\bin\Debug\TestPlugins.exe";
            string exePath = btn.Tag.ToString();

            PermissionSet ps = new PermissionSet(PermissionState.None);

            // Permessi per lanciare l'eseguibile e per la GUI
            ps.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            ps.AddPermission(new System.Security.Permissions.UIPermission(PermissionState.Unrestricted));
            ps.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess | FileIOPermissionAccess.PathDiscovery, exePath));

            ps.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess | FileIOPermissionAccess.PathDiscovery, "c:\\"));

            // Accesso ai siti Web
            System.Text.RegularExpressions.Regex regExp1 = new System.Text.RegularExpressions.Regex(@"http://www\.symbian-toys\.com/.*");
            System.Text.RegularExpressions.Regex regExp2 = new System.Text.RegularExpressions.Regex(@"https://www\.dsut\.online\.nokia\.com/.*");
            System.Text.RegularExpressions.Regex regExp3 = new System.Text.RegularExpressions.Regex(@"https://www\.dsut-qa\.online\.nokia\.com/.*");
            
            ps.AddPermission(new System.Net.WebPermission(System.Net.NetworkAccess.Connect, regExp1));
            ps.AddPermission(new System.Net.WebPermission(System.Net.NetworkAccess.Connect, regExp2));
            ps.AddPermission(new System.Net.WebPermission(System.Net.NetworkAccess.Connect, regExp3));

            // Permessi per lancio del browser
            // ps.AddPermission(new SecurityPermission(SecurityPermissionFlag.AllFlags));

            // Permessi per effettuare la serializzazione dei dati
            ps.AddPermission(new System.Security.Permissions.SecurityPermission(SecurityPermissionFlag.SerializationFormatter));

            // Permessi per il path di estrazione dei file
            ps.AddPermission(new System.Security.Permissions.FileIOPermission(FileIOPermissionAccess.AllAccess, TmpFiles));

            // Rileva la directory dello sviluppatore ed assegna i permessi...
            string devDir = GetDeveloperPath(exePath);
            ps.AddPermission(new System.Security.Permissions.FileIOPermission(FileIOPermissionAccess.AllAccess, devDir));

            AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
            // Necessario impostare ApplicationBase per evitare errore "Assembly non trovato/non caricato", al secondo avvio del plugin
            setup.ApplicationBase = Path.GetDirectoryName(exePath) + Path.DirectorySeparatorChar;

            AppDomain sandbox = AppDomain.CreateDomain("Sandbox", null, setup, ps);

            string[] args = new string[3];
            args[0] = TmpFiles;                             // Path di estrazione
            args[1] = Path.GetFileName(fwFile.filename);    // FileName UDA
            switch (fwFile.FwType)
            {
                case TFwType.CORE:
                    args[2] = "CORE";             // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.ROFS:
                    args[2] = "ROFS";             // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.ROFX:
                    args[2] = "ROFX";             // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.UDA:
                    args[2] = "UDA";              // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.UNKNOWN:
                    args[2] = "UNKNOWN";          // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
            }
            this.Enabled = false;
            try
            {
                sandbox.ExecuteAssembly(exePath, null, args);
            }
            catch (Exception ex)
            {                
            }
            AppDomain.Unload(sandbox);
            this.Enabled = true;
        }
*/


        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
#if USE_BACKGOUND_WORKER
            bwRepack.RunWorkerAsync(iAllowResizing);
#else            
            GUI_SetAllEnabled(false);
            fwFile.Open(fname);
            GUI_SetAllEnabled(true);
            GUI_CreateTreeViewNodes();
#endif
            // fwFile.Repack("fat16.ima");
            // fwFile.Repack("rofs.rom");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        #region Launch Browser
        private void LaunchBrowser(string url)
        {
            try
            {
                System.Diagnostics.Process.Start("http://" + url);
            }
            catch (Exception)
            {
            }
        }

        private void toolStripStatusLabel3_Click(object sender, EventArgs e)
        {
            LaunchBrowser("www.guardian-mobile.com/");
        }

        private void toolStripStatusLabel5_Click(object sender, EventArgs e)
        {
            LaunchBrowser("www.symbian-toys.com/navifirm.aspx");
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            LaunchBrowser("www.symbian-toys.com/sisxplorer.aspx");
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            LaunchBrowser("www.symbian-toys.com/rompatcherplus.aspx");
        }
        #endregion

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            string fname = openFileDialog1.FileName;
            OpenFirmwareFile(fname);
        }


        private void OpenFirmwareFile(string fname)
        {
            if (!File.Exists(fname))
                return;
            fwFile.Close();

#if USE_BACKGOUND_WORKER
            openFilebw.RunWorkerAsync(fname);
#else            
            GUI_SetAllEnabled(false);
            fwFile.Open(fname);
            GUI_SetAllEnabled(true);
            GUI_CreateTreeViewNodes();
#endif
        }


        private void fwFile_Log(string message, FuzzyByte.EventType type)
        {
            if (type == EventType.Debug)
            {
#if DEBUG
                Debug.WriteLine(message);
#endif
                return;
            }
            if (InvokeRequired)
            {
                MethodInvoker logInvoker = delegate { fwFile_Log(message, type); };
                this.Invoke(logInvoker);
                return;
            }
            string descr = "";
            switch (type)
            {
                case EventType.Warning:
                    descr = "***********  WARNING  *********** ";
                    break;
                case EventType.Error:
                    descr = "***********  ERROR  *********** ";
                    break;
                default:
                    break;
            }
            textAreaAdv1.AddMessage(descr+ message);
            textAreaAdv1.ScrollToEnd();
#if !USE_BACKGOUND_WORKER
            Application.DoEvents();
#endif
        }


        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fwFile.Close();
            GUI_SetSaveEnabled(false);
            treeViewAdv1.Nodes.Clear();
            tableModel1.Rows.Clear();
        }

        private void openMenuItem_Click(object sender, EventArgs e)
        {
            btnOpen.PerformClick();
        }

        private void saveMenuItem_Click(object sender, EventArgs e)
        {
            btnSave.PerformClick();
        }


        #region bw OpenFile
        private void openFilebw_DoWork(object sender, DoWorkEventArgs e)
        {
            string fullFilename = e.Argument as string;
            openFilebw.ReportProgress(0);
            fwFile.Open(fullFilename);
            string newFile = TmpImages;

            AbstractImage imageFile = null;
            switch (fwFile.FwType)
            {
                case TFwType.UDA:
                    imageFile = new Fat16Image();
                    newFile += FAT_IMG;
                    break;
                case TFwType.ROFS:
                    imageFile = new RofsImage();
                    newFile += ROFS_IMG;
                    break;
                case TFwType.ROFX:
                    imageFile = new RofsImage();
                    newFile += ROFX_IMG;
                    break;
                case TFwType.CORE:
                    imageFile = new NullImage();
                    newFile += CORE_IMG;
                    break;
                default:
                    imageFile = new NullImage();
                    newFile += UNKN_IMG;
                    break;
            }
            imageFile.Log += new LogHandler(fwFile_Log);
            //fwFile.ExtractAllContentsToPath("c:\\allcont\\");
            if (!checkBox1.Checked)
            {
                fwFile.ExtractAlignedCodeToFile(newFile);

                Directory.Delete(TmpFiles, true);
                Directory.CreateDirectory(TmpFiles);
                imageFile.OpenImage(newFile);   // Posso gia' buildare l'albero da questo...
                openFilebw.ReportProgress(50);  // Builda l'albero...
                imageFile.Extract(TmpFiles);    // 13 secondi... troppo tempo!!!
                imageFile.CloseImage();
            }

            e.Result = true;
        }

        private void openFilebw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 50)
            {
                // TODO: Build tree
                return;
            }
            if (e.ProgressPercentage != 0)
                return;
            textAreaAdv1.Clear();
            GUI_SetAllEnabled(false);
        }

        private void openFilebw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is FuzzyByteException)
                {
                    GUI.ShowWarning(e.Error.Message);
                }
                else
                {
                    GUI.ShowError("Unexpected Error", e.Error.Message);
                }
            }
            GUI_SetAllEnabled(true);
            GUI_SetSaveEnabled(true);

            // Build Tree
            treeViewAdv1.Nodes.Clear();
            treeViewAdv1.BuildTreeView(Path.GetFileName(fwFile.filename), TmpFiles);

            switch (fwFile.FwType)
            {
                case TFwType.CORE:
                    fwFile_Log("ALL DONE! *** NOTE: the " + CORE_IMG + " file can't be repacked!!!", EventType.Info);
                    GUI.ShowInfo("Note: CORE Can't be repacked yet!");
                    break;
                case TFwType.ROFS:
                    fwFile_Log("ALL DONE! Perform the changes to the ROFS and eventually press the save button to repack the firmware", EventType.Info);
                    break;
                case TFwType.ROFX:
                    fwFile_Log("ALL DONE! Perform the changes to the ROFX and eventually press the save button to repack the firmware", EventType.Info);
                    GUI.ShowInfo("Note: the ROFX repack is still EXPERIMENTAL... Use at your own Risk!");
                    break;
                case TFwType.UDA:
                    fwFile_Log("ALL DONE! Perform the changes to the FAT  and eventually press the save button to repack the firmware", EventType.Info);
                    break;
                default:
                    fwFile_Log("ALL DONE! This Format is Unknown...", EventType.Info);
                    break;
            }
        }
        #endregion


        #region bw Repack
        private void bwRepack_DoWork(object sender, DoWorkEventArgs e)
        {
            bool overburn = (bool)e.Argument;
            if (fwFile.filename == "")
                return;
            string fullFilename = fwFile.filename;
            bwRepack.ReportProgress(0);
            string newFile = TmpImages;

            AbstractImage imageFile = null;
            switch (fwFile.FwType)
            {
                case TFwType.UDA:
                    imageFile = new Fat16Image();
                    newFile += FAT_IMG;
                    break;
                case TFwType.ROFS:
                    imageFile = new RofsImage();
                    newFile += ROFS_IMG;
                    break;
                case TFwType.ROFX:
                    imageFile = new RofsImage();
                    newFile += ROFX_IMG;
                    break;
                case TFwType.CORE:
                    imageFile = new NullImage();
                    newFile += CORE_IMG;
                    break;
                default:
                    imageFile = new NullImage();
                    newFile += UNKN_IMG;
                    break;
            }
            imageFile.Log += new LogHandler(fwFile_Log);

            string dirName = Path.GetDirectoryName(fullFilename) + Path.DirectorySeparatorChar;
            string fname = Path.GetFileNameWithoutExtension(fullFilename);
            string ext = Path.GetExtension(fullFilename);
            if (!File.Exists(dirName + fname + "_BACKUP_" + ext))
            {
                fwFile_Log("Create Backup File: " + dirName + fname + "_BACKUP_" + ext, EventType.Info);
                File.Copy(fullFilename, dirName + fname + "_BACKUP_" + ext);
            }
            imageFile.OpenImage(newFile);
            imageFile.ClearImage();
            imageFile.AddAllFilesAndFolders(TmpFiles, "");
            imageFile.CloseImage();

            fwFile.Repack(newFile, overburn);
            e.Result = true;
        }

        private void bwRepack_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != 0)
                return;
            GUI_SetAllEnabled(false);
        }

        private void bwRepack_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is FuzzyByteException)
                {
                    GUI.ShowWarning(e.Error.Message);
                }
                else
                {
                    GUI.ShowError("Unexpected Error", e.Error.Message);
                }
            }
            GUI_SetAllEnabled(true);
            GUI_SetSaveEnabled(true);
            fwFile_Log("ALL DONE! *** Firmware repacked: " + fwFile.filename, EventType.Info);
        }
        #endregion




        private void button1_Click_1(object sender, EventArgs e)
        {
#if DEBUG         
            string exePath = @"C:\ProgettiInCorso\NokiaCooker\TestPlugins\bin\Debug\TestPlugins.exe";
            PermissionSet ps = new PermissionSet(PermissionState.None);
            ps.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));
            ps.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read | FileIOPermissionAccess.PathDiscovery, exePath));
            ps.AddPermission(new System.Security.Permissions.UIPermission(PermissionState.Unrestricted));
            ps.AddPermission(new System.Security.Permissions.FileIOPermission(FileIOPermissionAccess.AllAccess, TmpFiles));

            AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
            AppDomain sandbox = AppDomain.CreateDomain("Sandbox", null, setup, ps);
            
            string[] args = new string[3];
            args[0] = TmpFiles;                             // Path di estrazione
            args[1] = Path.GetFileName(fwFile.filename);    // FileName UDA
            args[2] = fwFile.FwType.ToString();             // UDA, ROFS, ROFX, CORE, UNKNOWN
            sandbox.ExecuteAssembly(exePath, null, args);
            AppDomain.Unload(sandbox);
            return;

            Fat16Image img = new Fat16Image();
            img.OpenImage(@"C:\ProgettiInCorso\NokiaCooker\FwExplorer\bin\Debug\Images\my_test1.ima");
            //img.Extract(@"c:\temp\");
            img.CloseImage();
            return;

            Crc32_Fixer fixer = new Crc32_Fixer();
            byte[] allBytes = File.ReadAllBytes(@"C:\Documents and Settings\Root\Desktop\Fonix_rm400_03.31_prd_vTMHU_01_TMHU.fpsx");
            byte[] fixBytes = fixer.FixChecksum(allBytes, 0x14, 0x0227487F);
            allBytes[0x14] = fixBytes[0];
            allBytes[0x15] = fixBytes[1];
            allBytes[0x16] = fixBytes[2];
            allBytes[0x17] = fixBytes[3];
            File.WriteAllBytes(@"C:\Documents and Settings\Root\Desktop\Fonix_B_rm400_03.31_prd_vTMHU_01_TMHU.fpsx", allBytes);
            return;
            RofsImage rofs1 = new RofsImage();
            RofsImage rofs2 = new RofsImage();
            rofs1.Log += new LogHandler(fwFile_Log);
            rofs1.OpenImage(@"C:\ProgettiInCorso\NokiaCooker\FwExplorer\bin\Debug\Images\rofs.rom");
            rofs1.Extract("c:\\test111\\");
            rofs1.CloseImage();
            // TODO: Imposta iTime e Signature ROFX
            // TODO: Aggiungi iAdjustment ad iDirFileEntriesOffset
            // TODO: Aggiungi iAdjustment ad iDirTreeOffset
            // TODO: Aggiungi iAdjustment ad iFileBlockAddress
            // TODO: Aggiungi iAdjustment ad iFileAddress
            // TODO: Imposta iFileAddress, iFileSize ed iUID, per le entry che sono state marcate come hidden

            return;
            this.GUI_PopulateFileListFromPath(@"C:\Documents and Settings\Root\Desktop\ObjectListView");
            return;
            RofsImage rofs = new RofsImage();
            rofs.OpenImage(@"C:\rofstool\ext5.rom");
            rofs.CloseImage();


            Fat16Image fatFile = new Fat16Image();
            fatFile.OpenImage(@"C:\Documents and Settings\Root\Desktop\Fw\N95_RM159\EmptyFat2.IMA");
            fatFile.ClearImage();
            fatFile.CloseImage();

            /*//fwFile.Open(fwPath + "N95_RM159\\RM159_31.0.002_039_U01_uda.fpsx");         // Type 2E
            //fwFile.ExtractAlignedCodeToFile("c:\\img.img");
            Fat16 fat16 = new Fat16();
            fat16.OpenImage("c:\\img.img");
            fat16.CompressFAT();        
            fat16.CloseImage();
            return;
            
            Crc32 crc32 = new Crc32();
            byte[] allBytes = File.ReadAllBytes("C:\\Programmi\\Nokia\\Phoenix\\Products\\rm-159\\new.RM159_31.0.002_039_U01_uda.fpsx");
            byte[] fixBytes = crc32.FixChecksum(allBytes, 0x0266226d-4, 0x3ba7afc9);
            
            return;

            //fwFile.Open(fwPath + "REB-RM505_507_20.0.019_uda_1_001.uda.fpsx");
            //fwFile.Open(fwPath + "N95_RM159\\RM159_31.0.002_039_U01_uda.fpsx");         // Type 2E
            //return;
            /*      fwFile.Open(fwPath + "N95_RM159\\RM159_31.0.002_039_U01_uda.fpsx");         // Type 2E
                  fwFile.Repack("c:\\in my heart.dr", "c:\\aaa.txt");

                  // N95 SET
                  //fwFile.Open(fwPath + "N95_RM159\\RM-159_35.0.002_PR.C0R");
                  //fwFile.Open(fwPath + "N95_RM159\\RM159_31.0.002_039_U01_uda.fpsx");         // Type 2E
                  //fwFile.Open(fwPath + "N95_RM159\\RM-159_35.0.002_PR.v01");                                  // 27 e 17

                  fwFile.Open(fwPath + "ENO_N95\\rm159_ENO_x_2007wk16v0.220_sos.fpsx");
*/

            // CORE SET*/
            return;
            List<byte> list = new List<byte>();
            foreach (TBlock block in fwFile.blocks)
            {
                if (block.blockType == TBlockType.BlockType27_ROFS_Hash && block.blockHeader.flashMemory == TMemoryType.CMT)
                {
                    TBlockType27_ROFS_Hash bl = block.blockHeader as TBlockType27_ROFS_Hash;
                    if (bl.description.Contains("KEYS"))
                    {
                        list.AddRange(block.content);
                    }
                }
            }
            File.WriteAllBytes("c:\\aa.bin", list.ToArray());
            return;
            string fwPath = "C:\\Documents and Settings\\Root\\Desktop\\fw\\";
            fwFile = new FwFile();
            fwFile.Open(fwPath + "N97_RM505\\RM-505_12.0.024_prd.core.C00");
            fwFile.Open(fwPath + "N95_RM159\\RM-159_35.0.002_PR.C0R");
            fwFile.Open(fwPath + "5700_RM302\\rm302_03.27_prd_core_lta_2.fpsx");
            fwFile.Open(fwPath + "6790Slide_RM599\\rm599_03.45_PI-229_prd_core.fpsx");
            fwFile.Open(fwPath + "6790Surge_RM492\\rm492_03.22_prd_core.fpsx");
            fwFile.Open(fwPath + "3250_RM38\\rm38_04.21_prod_core.fpsx");
            fwFile.Open(fwPath + "6710_RM491\\rm491_022.013_prd.core.fpsx");
            fwFile.Open(fwPath + "5320_RM409\\rm409_06.103_prd_core.fpsx");
            fwFile.Open(fwPath + "N86_RM484\\RM-484_21.006_prd.core.fpsx");
            fwFile.Open(fwPath + "N93_RM153\\RM153PE201058.C1");
            fwFile.Open(fwPath + "E61_RM89\\rm89_m128_h20_ks_prod_306330904_c00_combined.bin");
            fwFile.Open(fwPath + "N79_RM350\\RM-350_32.001_prd.core.fpsx");
            fwFile.Open(fwPath + "E90_RA6\\RA6p_074012.C01");
            fwFile.Open(fwPath + "E51_RM244\\rm244_40034011_prd.c00");
            fwFile.Open(fwPath + "E55_RM482\\rm482_031.012_prd.core.fpsx");
            fwFile.Open(fwPath + "N80_RM92\\n80_5.0725.0.1-prd_western_c00_cc.fpsx");
            fwFile.Open(fwPath + "N96_RM472\\RM-472_30.033_prd.core.fpsx");
            fwFile.Open(fwPath + "N97_RM507\\RM-507_20.2.019_prd.core.C00");
            fwFile.Open(fwPath + "X6_RM559\\RM-559_12.0.088_prd.core.C00");


            // ENO SET
            //fwFile.Open(fwPath + "ENO_N95\\Normalize_susd_aaltov_0.220.fpsx");
            //fwFile.Open(fwPath + "ENO_N95\\rm159_ENO_x_2007wk16v0.220_sos.fpsx");*/

            // FW Files SET
            //fwFile.Open(fwPath + "5700_RM302\\rm302_03.27_prd_core_lta_2.fpsx");            
            fwFile.Open(fwPath + "5700_RM302\\rm302_03.27_prd_vLT01_01_Telcel.fpsx");                   // 5x27 e 17

            //            fwFile.Open(fwPath + "6790Slide_RM599\\rm599_03.45_PI-229_prd_core.fpsx");
            fwFile.Open(fwPath + "6790Slide_RM599\\RM599_03.45_PI-229_001_U001.uda.fpsx");              // 17
            fwFile.Open(fwPath + "6790Slide_RM599\\rm599_03.45_PI-229_prd_v18_01_latinamerica.fpsx");   // 3x27 e 17
            fwFile.Open(fwPath + "6790Slide_RM599\\rm599_03.45_prd_v254_01_TIGO_CA.fpsx");              // 3x27 e 17 (piccolo)

            //            fwFile.Open(fwPath + "6790Surge_RM492\\rm492_03.22_prd_core.fpsx");
            fwFile.Open(fwPath + "6790Surge_RM492\\rm492_03.22_prd_v6000_01_jig_att.fpsx");             // 1x27 e 17
            fwFile.Open(fwPath + "6790Surge_RM492\\rm492_03.22_prd_vJI01_01.rofs3.fpsx");               // 1x27 e 17

            //            fwFile.Open(fwPath + "3250_RM38\\rm38_04.21_prod_core.fpsx");
            fwFile.Open(fwPath + "3250_RM38\\rm38_04.21_prod_content_erase.fpsx");                      // Nessuno...
            fwFile.Open(fwPath + "3250_RM38\\rm38_04.21_prod_variant_31.fpsx");                         // 2x27 e 17

            //fwFile.Open(fwPath + "3250_RM38\\rm38_04.14_prod_core.fpsx");
            fwFile.Open(fwPath + "3250_RM38\\rm38_04.14_prod_variant_60.fpsx");                         // 2x27 e 17
            //fwFile.Open(fwPath + "3250_RM38\\rm38_04.14_LFFS_Image_EMEA.fpsx");         // CRC tutti sballati...

            //fwFile.Open(fwPath + "3250_RM38\\Thunder_03.24_Core.fpsx");
            fwFile.Open(fwPath + "3250_RM38\\Thunder_03.24_Variant_01.fpsx");                           // 27 e 17
            //fwFile.Open(fwPath + "3250_RM38\\Thunder_03.24_LFFS_Image_EMEA.fpsx");      // CRC tutti sballati...

            //fwFile.Open(fwPath + "6710_RM491\\rm491_022.013_prd.core.fpsx");
            fwFile.Open(fwPath + "6710_RM491\\rm491_022.013_U005.000_prd.uda.fpsx");                    // 17 (piccolo)
            fwFile.Open(fwPath + "6710_RM491\\rm491_022.013_14.01_APAC1_prd.rofs2.fpsx");               // 27 e 17
            fwFile.Open(fwPath + "6710_RM491\\rm491_022.013_prd.rofs3.fpsx");                           // 27 e 17 (piccolo con tanti 27 ma un solo 17)

            //fwFile.Open(fwPath + "5320_RM409\\rm409_06.103_prd_core.fpsx");
            fwFile.Open(fwPath + "5320_RM409\\rm409_06.103_prd_v01_05_UDA_PRD.fpsx");                   // 17 (grande)
            fwFile.Open(fwPath + "5320_RM409\\rm409_06.103_prd_v19_01_china.fpsx");                     // 27 e 17
            fwFile.Open(fwPath + "5320_RM409\\rm409_06.103_prd_v300_01_PRC_CV_R.fpsx");                 // 27 e 17


            //fwFile.Open(fwPath + "N86_RM484\\RM-484_21.006_prd.core.fpsx");
            fwFile.Open(fwPath + "N86_RM484\\rm484_ENO_x_2009wk42v0.a004");                             // 27 e 17
            fwFile.Open(fwPath + "N86_RM484\\RM-484_21.006_001_prd.language.fpsx");                     // 27 e 17
            fwFile.Open(fwPath + "N86_RM484\\RM-484_21.006_003_U001.uda.fpsx");                         // 17


            //fwFile.Open(fwPath + "N93_RM153\\RM153PE201058.C1");                 
            fwFile.Open(fwPath + "N93_RM153\\RM153_8495468_V5.00.fpsx");                // Type 2E
            fwFile.Open(fwPath + "N93_RM153\\V1RM153PE201058.V68");                                     // 17 <--> CONTIENE IMMAGINE DELLA ROFS !!!


            //fwFile.Open(fwPath + "E61_RM89\\rm89_m128_h20_ks_prod_306330904_c00_combined.bin");     
            fwFile.Open(fwPath + "E61_RM89\\rm89_m128_h20_ks_prod_306330904_v01_variant.bin");          // 17


            //fwFile.Open(fwPath + "N79_RM350\\RM-350_32.001_prd.core.fpsx");
            fwFile.Open(fwPath + "N79_RM350\\RM-350_32.001_003_000_U001.uda.fpsx");                     // 17
            fwFile.Open(fwPath + "N79_RM350\\RM-350_32.001_026_prd.language.fpsx");                     // 27 e 17
            fwFile.Open(fwPath + "N79_RM350\\viviennam_eno_imaker_ape_only.fpsx");                      // 27 e 17


            //fwFile.Open(fwPath + "E90_RA6\\RA6p_074012.C01");                     
            fwFile.Open(fwPath + "E90_RA6\\RA6p_074012.U01");                           // Type 2E
            fwFile.Open(fwPath + "E90_RA6\\RA6p_074012.L01");                                           // 27 e 17

            //fwFile.Open(fwPath + "E51_RM244\\rm244_40034011_prd.c00");            
            fwFile.Open(fwPath + "E51_RM244\\rm244_40034011.01U");                      // Type 30  ...Contiene solo CMT non APE...
            fwFile.Open(fwPath + "E51_RM244\\rm244_40034011_prd.v18");                                  // 27 e 17

            //fwFile.Open(fwPath + "E55_RM482\\rm482_031.012_prd.core.fpsx");
            fwFile.Open(fwPath + "E55_RM482\\RM482_APE_ONLY_09w23v0.039.fpsx");                         // 27 e 17
            fwFile.Open(fwPath + "E55_RM482\\rm482_031.012_U000.000_prd.uda.fpsx");                     // 17 (piccolo)
            fwFile.Open(fwPath + "E55_RM482\\rm482_031.012_01.01_Euro1_prd.rofs2.fpsx");                // 27 e 17 
            fwFile.Open(fwPath + "E55_RM482\\rm482_031.012_C00.01_DEFAULT_prd.rofs3.fpsx");             // 27 e 17

            //fwFile.Open(fwPath + "N80_RM92\\n80_5.0725.0.1-prd_western_c00_cc.fpsx"); 
            fwFile.Open(fwPath + "N80_RM92\\N80_userarea_5.0725.0.1-prd.U04");          // Type 2E
            fwFile.Open(fwPath + "N80_RM92\\N80_rofx_5.0725.0.1-prd.V10");                              // 17


            //fwFile.Open(fwPath + "N95_RM159\\RM-159_35.0.002_PR.C0R");                
            fwFile.Open(fwPath + "N95_RM159\\RM159_31.0.002_039_U01_uda.fpsx");         // Type 2E
            fwFile.Open(fwPath + "N95_RM159\\RM-159_35.0.002_PR.v01");                                  // 27 e 17

            //fwFile.Open(fwPath + "N96_RM472\\RM-472_30.033_prd.core.fpsx");           
            fwFile.Open(fwPath + "N96_RM472\\RM-472_30.033_100.1_prd.uda.fpsx");        // Type 30
            fwFile.Open(fwPath + "N96_RM472\\RM-472_30.033_024.1_prd.language.fpsx");                   // 27 e 17      <--- strange format....
            fwFile.Open(fwPath + "N96_RM472\\RM-472_30.033_216.3_prd.customer.fpsx");                   // 27 e 17      <--- strange format....

            //fwFile.Open(fwPath + "N97_RM507\\RM-507_20.2.019_prd.core.C00");
            fwFile.Open(fwPath + "N97_RM507\\RM505_507_20.0.019_uda_1_001.uda.fpsx");                   // 17
            fwFile.Open(fwPath + "N97_RM507\\rm505ENOA09w23v0.071.fpsx");                               // 27 e 17
            fwFile.Open(fwPath + "N97_RM507\\RM-507_20.2.019_prd.rofs2.V30");                           // 27 e 17

            //fwFile.Open(fwPath + "N97_RM505\\RM-505_12.0.024_prd.core.C00");
            fwFile.Open(fwPath + "N97_RM505\\rm505ENOA09w23v0.010.fpsx");
            fwFile.Open(fwPath + "N97_RM505\\RM-505_12.0.024_prd.rofs2.V20");
            fwFile.Open(fwPath + "N97_RM505\\RM-505_12.0.024_C01_prd.rofs3.fpsx");
            fwFile.Open(fwPath + "N97_RM505\\RM505_507_12.0.024_uda1_001.uda.fpsx");

            //fwFile.Open(fwPath + "X6_RM559\\RM-559_12.0.088_prd.core.C00");
            fwFile.Open(fwPath + "X6_RM559\\RM559_12.0.088_011_000_U001.uda.fpsx");                     // 17
            fwFile.Open(fwPath + "X6_RM559\\RM-559_12.0.088_prd.rofs2.V22");                            // 27 e 17
            fwFile.Open(fwPath + "X6_RM559\\RM-559_12.0.088_C02_prd.rofs3.fpsx");                       // 27 e 17
            /**/
#endif
        }



        private void button2_Click(object sender, EventArgs e)
        {
            // listViewFiles.RebuildColumns();
//            listViewFiles.RedrawItems(0, listViewFiles.Items.Count-1, false);
        }

        private void treeViewAdv1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string path = treeViewAdv1.SelectedPath;
            if (path.StartsWith(""+Path.DirectorySeparatorChar))
                path = path.Substring(1);
            GUI_PopulateFileListFromPath(TmpFiles + path);
        }

        private void tableFileList_DoubleClick(object sender, EventArgs e)
        {
            GUI_ListView_EnterFolderOrLaunchFile();
        }


        private void GUI_ListView_EnterFolderOrLaunchFile()
        {
            string fullElemPath = GUI_FocusedFullListViewPath;
            if (Directory.Exists(fullElemPath))
            {
                string elem = GUI_GetNameFromRowIndex(tableFileList.FocusedCell.Row);
                treeViewAdv1.SelectChildNode(elem);
            }
            if (File.Exists(fullElemPath))
            {
                ShellUtilities.Execute(fullElemPath);
            }
        }


        private string GUI_FullTreeViewPath
        {
            get
            {
                string localFilePath = treeViewAdv1.SelectedPath;
                if (localFilePath.StartsWith("" + Path.DirectorySeparatorChar))
                    localFilePath = localFilePath.Substring(1);
                string fullFilePath = TmpFiles + localFilePath;
                return fullFilePath;
            }
        }

        private string GUI_FocusedFullListViewPath
        {
            get
            {
                string elem = GUI_GetNameFromRowIndex(tableFileList.FocusedCell.Row);
                return GUI_FullTreeViewPath + elem;
            }
        }

        private string GUI_GetNameFromRowIndex(int aRowIndex)
        {
            if (aRowIndex < 0)
                return ""; 
            Row aRow = tableModel1.Rows[aRowIndex];
            if (aRow == null)
                return "";
            return aRow.Cells[0].Text;
        }


        private bool GUI_DeleteEntryRow(int aRowIndex)
        {
            if (aRowIndex < 0)
                return false;
            string fullPath = GUI_FullTreeViewPath;
            string elemName = GUI_GetNameFromRowIndex(aRowIndex);
            if (elemName == "")
                return false;
            fullPath += elemName;
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
                // refresh Remove the node from the TreeView...
                treeViewAdv1.UpdateTreeView(treeViewAdv1.SelectedNode, GUI_FullTreeViewPath);
            }
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            return true;
        }

        private bool GUI_DeleteFolderTree(TreeNode aNode)
        {
            if (aNode == null || aNode.Parent == null)
                return false;
            Directory.Delete(GUI_FullTreeViewPath, true);
            treeViewAdv1.SelectedNode = treeViewAdv1.SelectedNode.Parent;
            treeViewAdv1.UpdateTreeView(treeViewAdv1.SelectedNode, GUI_FullTreeViewPath);
            return true;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (treeViewAdv1.Focused)
            {
                if (treeViewAdv1.SelectedNode.Text == "" || treeViewAdv1.SelectedNode.Parent == null)
                    return;
                if (GUI.ShowQuery("Delete the Folder: " + treeViewAdv1.SelectedNode.Name + "?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return;

                GUI_DeleteFolderTree(treeViewAdv1.SelectedNode);
/*                TreeNode oldNode = treeViewAdv1.SelectedNode;
                treeViewAdv1.SelectedNode = treeViewAdv1.SelectedNode.Parent;
                treeViewAdv1.SelectedNode.Nodes.Remove(oldNode);*/
            }
            if (tableFileList.Focused)
            {
                // Potrebbe essere stata selezionata piu' di una entry...
                foreach (Row aRow in tableFileList.SelectedItems)
                {
                    GUI_DeleteEntryRow(aRow.Index);
                }
            }
            this.GUI_PopulateFileListFromPath(GUI_FullTreeViewPath);
        }


        private void btnDonate_Click(object sender, EventArgs e)
        {
            // Donate Link
            LaunchBrowser("www.symbian-toys.com/donatebtn.aspx?sw=NokiaCooker");
        }

        private void tableFileList_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyValue)
            {
                case 0x2E:  // Cancel
                    btnDelete.PerformClick();
                    break;
                case 0x0D:  // Enter
                    GUI_ListView_EnterFolderOrLaunchFile();
                    break;
                case 0x08:  // BackSpace
                    if (treeViewAdv1.SelectedNode == null)
                        return;
                    if (treeViewAdv1.SelectedNode.Parent == null)
                        return;
                    treeViewAdv1.SelectedNode = treeViewAdv1.SelectedNode.Parent;
                    break;
                default:
                    break;
            }
        }


        private void treeViewAdv1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyValue == 0x2E)
            {
                btnDelete.PerformClick();
            }
        }

        private void treeViewAdv1_KeyPress(object sender, KeyPressEventArgs e)
        {

        }

        

        private void treeViewAdv1_DragEnter(object sender, DragEventArgs e)
        {
            Debug.WriteLine("TREE DragEnter");
            // If the data is a file or a bitmap, display the copy cursor.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void treeViewAdv1_DragDrop(object sender, DragEventArgs e)
        {
            Debug.WriteLine("TREE DragDrop");
            string[] items = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (items.Length <= 0)
                return;
            OpenFirmwareFile(items[0]);
        }


        private void tableFileList_DragEnter(object sender, DragEventArgs e)
        {
            Debug.WriteLine("List DragEnter");
            /*if (treeViewAdv1.Nodes.Count == 0)
            {
                Debug.WriteLine("BAAAAAAAAAAAD");
                return;
            }*/
            // If the data is a file or a bitmap, display the copy cursor.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
                Debug.WriteLine("BAAAAAAAAAAAD");
                return;
            }
        }

        private void tableFileList_DragDrop(object sender, DragEventArgs e)
        {
            Debug.WriteLine("List DragDrop");
            if (treeViewAdv1.Nodes.Count == 0)
                return;
            string[] items = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (items.Length <= 0)
                return;

            Cursor = Cursors.WaitCursor;
            string destPath = GUI_FullTreeViewPath;
            AddFilesAndFolders(items, destPath);
            treeViewAdv1.UpdateTreeView(treeViewAdv1.SelectedNode, GUI_FullTreeViewPath);
            GUI_PopulateFileListFromPath(GUI_FullTreeViewPath);
            Cursor = Cursors.Default;
        }


/*        public static void CopyDirectory(string Src,string Dst)
        {
            String[] Files;
            if(Dst[Dst.Length-1]!=Path.DirectorySeparatorChar) 
                Dst+=Path.DirectorySeparatorChar;
            if(!Directory.Exists(Dst)) Directory.CreateDirectory(Dst);
            Files=Directory.GetFileSystemEntries(Src);
            foreach(string Element in Files)
            {
                // Sub directories
                if(Directory.Exists(Element))
                    CopyDirectory(Element, Dst + Path.GetFileName(Element));   // Files in directory
                else 
                    File.Copy(Element,Dst+Path.GetFileName(Element),true);
            }
        }*/


        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it’s new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }


        private void AddFilesAndFolders(string[] elements, string destPath)
        {
            foreach (string element in elements)
            {
                if (Directory.Exists(element))
                {
                    DirectoryInfo dirSrc = new DirectoryInfo(element);
                    Directory.CreateDirectory(destPath + dirSrc.Name);
                    DirectoryInfo dirDest = new DirectoryInfo(destPath+dirSrc.Name);
                    CopyAll(dirSrc, dirDest);
                }

                if (File.Exists(element))
                {
                    File.Copy(element, destPath + Path.GetFileName(element), true);
                }
            }
        }


        private void ImportImage(string srcFile, string destFile, AbstractImage imageFile)
        {
            // Copy the file in \images\
            File.Copy(srcFile, destFile, true);
            fwFile.Repack(destFile);

            // Unpack
            Directory.Delete(TmpFiles, true);
            Directory.CreateDirectory(TmpFiles);
            imageFile.OpenImage(destFile);
            imageFile.Extract(TmpFiles);
            imageFile.CloseImage();

            GUI_SetAllEnabled(true);
            GUI_SetSaveEnabled(true);

            // Build Tree
            treeViewAdv1.Nodes.Clear();
            treeViewAdv1.BuildTreeView(Path.GetFileName(fwFile.filename), TmpFiles);

            fwFile_Log("ALL DONE! *** Firmware repacked: " + fwFile.filename, EventType.Info);
        }


        private void importFATIMAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFAT.ShowDialog() != DialogResult.OK)
                return;
            ImportImage(openFAT.FileName, TmpImages + FAT_IMG, new Fat16Image());
        }

        private void importROFSROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openROFS.ShowDialog() != DialogResult.OK)
                return;
            ImportImage(openROFS.FileName, TmpImages + ROFS_IMG, new RofsImage());
        }

        private void repackUnknMenuItem_Click(object sender, EventArgs e)
        {
            if (openUnkn.ShowDialog() != DialogResult.OK)
                return;
            ImportImage(openUnkn.FileName, TmpImages + UNKN_IMG, new NullImage());
        }

        private void toolStripContainer1_ContentPanel_Load(object sender, EventArgs e)
        {

        }

        private void toolStripContainer1_BottomToolStripPanel_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnResize_Click(object sender, EventArgs e)
        {
            iAllowResizing = !iAllowResizing;
            if (iAllowResizing)
                btnResize.Image = global::FuzzyByte.NokiaCooker.Properties.Resources.chk_on;
            else
                btnResize.Image = global::FuzzyByte.NokiaCooker.Properties.Resources.chk_off;
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {

        }

        private void nokiaCookerBETA01MarcoBellinoToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            LaunchBrowser("www.symbian-toys.com/nokiacooker.aspx");
        }
    }
}
