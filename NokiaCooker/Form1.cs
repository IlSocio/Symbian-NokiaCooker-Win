#define USE_BACKGOUND_WORKER

using System.Security.Cryptography;
using System.Resources;
using System.Security.Principal;
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
using FuzzyByte.OS;
using FuzzyByte.Forms;
using FuzzyByte.IO;

using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Security.AccessControl;
using System.Reflection;
using System.Xml;
using System.Runtime.InteropServices;
using System.Threading;
using FuzzyByte.Utils;
using NAppUpdate.Framework;
using NAppUpdate.Framework.Sources;
using NAppUpdate.Framework.Common;
using FuzzyByte.NokiaCooker.Properties;
//using System.IO.Pipes;



namespace FuzzyByte.NokiaCooker
{

    public partial class Form1 : Form
    {
//        private NamedPipeServerStream serverPipe;

        private List<FileElem> allFiles = new List<FileElem>();
        private RecentList recent;
        private Process plugin = null;
        private AppIcons imgListTreeView = new AppIcons();    // Evita il flickering della TreeView ogni volta che viene aggiunta una immagine.
        private AppIcons imgListFileView = new AppIcons();
        private AppIcons imgListPlugins = new AppIcons();
        private FwFile fwFile = null;
        private string TmpImages = "Images";
        private string TmpFiles = "Files";
        private string TrashFiles = "Trashcan";
        private string PluginsPath = "Plugins";
        //private string SandboxPath = "Sandbox";
        private bool iNeedsRebuild = false;
        private readonly string CHANGED_MARKER = "(*) ";
        private readonly string ROFS_IMG = "ROFS.ROM";              // se c'e' il VPL nominalo ROFS_2crc.ROM e ROFS_3crc.ROM
        private readonly string ROFX_IMG = "ROFX.ROM";              // se c'e' il VPL nominalo ROFS_2crc.ROM e ROFS_3crc.ROM
        private readonly string CORE_ROFS1_IMG = "ROFS1.ROM";
        private readonly string FAT_IMG = "FAT.IMA";
        private readonly string MMC_IMG = "FAT2.IMA";
        private readonly string UNKN_IMG = "UNKN.BIN";

        // CORE <FileSubType>Mcu</FileSubType>
        // ROFS2 <FileSubType>Ppm</FileSubType>
        // ROFS3 <FileSubType>Ppm</FileSubType>
        // UDA <FileSubType>Content</FileSubType>
        // MMC <FileSubType>MemoryCardContent</FileSubType>

        private AbstractImage iImageFile = null;
        private UInt64 iEstimatedSize = 0;
        private bool iFileSystemChangedRecently = false;
        private bool iNeedsRecomputeSize = false;
        private ToolStripButton tsBtnCancel = null;
        private string iFileArg="";


        #region Check User Credentials
        public bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                isAdmin = false;
                MessageBox.Show(ex.Message);
            }
            return isAdmin;
        }
        #endregion


        /*
        private void WaitForConnectionCallBack(IAsyncResult result)
        {
            serverPipe.EndWaitForConnection(result);
            byte[] buffer = new byte[2000];

            // Read the incoming message
            serverPipe.Read(buffer, 0, 2000);

            // Convert byte buffer to string
            iFileArg = Encoding.UTF8.GetString(buffer, 0, buffer.Length);            

            serverPipe.Close();
            serverPipe = new NamedPipeServerStream("NokiaCookerPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            serverPipe.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), null);

            ProcessNokiaCookerArgs();
        }*/

        public Form1(string[] args)
        {
//            serverPipe = new NamedPipeServerStream("NokiaCookerPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
//            serverPipe.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), null);

            InitializeComponent();

            if (args.Length > 0)
                iFileArg = args[0];
            timerCompute.Start();

            //            lblFolders.Text = lblFiles.Text = lblSize.Text = lblPartSize.Text = lblEstimatedSize.Text =  "";

            TrashFiles = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + TrashFiles + Path.DirectorySeparatorChar;
            TmpFiles = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + TmpFiles + Path.DirectorySeparatorChar;
            TmpImages = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + TmpImages + Path.DirectorySeparatorChar;
            PluginsPath = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + PluginsPath + Path.DirectorySeparatorChar;
            //SandboxPath = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + SandboxPath + Path.DirectorySeparatorChar;
            Directory.CreateDirectory(TrashFiles);
            Directory.CreateDirectory(TmpFiles);
            Directory.CreateDirectory(TmpImages);
            Directory.CreateDirectory(PluginsPath);

            fileSystemWatcher1.Path = TmpFiles;
            fileSystemWatcher1.EnableRaisingEvents = false;
            /*DirBytesUtils.CreateNew(TmpFiles, true);
            DirBytesUtils.CreateNew(TmpImages, true);    Non va bene perche' cancella la dir esistente e la ricrea (cancella rofsbuild.exe)
            DirBytesUtils.CreateNew(PluginsPath, true);*/

            imgListTreeView.SmallIconsImageList.Images.Add(NokiaCooker.Properties.Resources.cellphone);
            imgListTreeView.LargeIconsImageList.Images.Add(NokiaCooker.Properties.Resources.cellphone);
            imgListTreeView.GetIconIndex(Environment.SystemDirectory);
            imgListTreeView.GetIconIndex(Environment.SystemDirectory, 2);

            treeViewAdv1.ImageList = imgListTreeView.SmallIconsImageList;

            textAreaAdv1.LinkClicked += new LinkClickedEventHandler(textAreaAdv1_LinkClicked);

            gridFiles1.SetImageList(imgListFileView);

            fwFile = new FwFile();
            fwFile.Log += new LogHandler(fwFile_Log);
            
#if !DEBUG
            button3.Visible = false;
            button2.Visible = false;
            button1.Visible = false;
            checkBox1.Visible = false;
#endif
            //            AllowDragAndDrop();
        }


        private bool CancelBtnVisible
        {
            get
            {
                return tsBtnCancel.Visible;
            }
            set
            {
                tsBtnCancel.Visible = value;
                lblFolders.Visible = lblFiles.Visible = lblSize.Visible = toolStripStatusLabel8.Visible = !value;
            }
        }

        private bool GUI_FwChanged
        {
            get
            {
                if (treeViewAdv1 == null)
                    return false;
                if (treeViewAdv1.Nodes.Count == 0)
                    return false;
                return (treeViewAdv1.Nodes[0].Text.StartsWith(CHANGED_MARKER));
            }
            set
            {
                if (treeViewAdv1 == null)
                    return;
                if (treeViewAdv1.Nodes.Count == 0)
                    return;
                bool currVal = GUI_FwChanged;
                if (!value && currVal)
                {
                    treeViewAdv1.Nodes[0].Text = treeViewAdv1.Nodes[0].Text.Remove(0, CHANGED_MARKER.Length);
                    return;
                }
                if (value && !currVal)
                {
                    treeViewAdv1.Nodes[0].Text = CHANGED_MARKER + treeViewAdv1.Nodes[0].Text;
                }
            }
        }/**/

        private void GUI_SetToolbarEnabled(bool state)
        {
            importFATMenuItem.Enabled = state;
            importROFSMenuItem.Enabled = state;
            importUnknMenuItem.Enabled = state;
            extractRomMenuItem.Enabled = state;
            btnUnlockRofs.Enabled = state;
            btnRofs1Increase.Enabled = state;
            btnSave.Enabled = state;
            btnExplore.Enabled = state;
            btnDelete.Enabled = state;
            saveMenuItem.Enabled = state;
            closeMenuItem.Enabled = state;
            contextFileView.Enabled = state;
            contextTreeView.Enabled = state;

            bool isCORE = (fwFile != null && fwFile.FwType == TFwType.CORE);
            btnUnlockRofs.Enabled = state & isCORE;
            btnRofs1Increase.Enabled = state & isCORE;
            extractRomMenuItem.Enabled = state & isCORE;
        }

        private void GUI_SetAllEnabled(bool state)
        {
            contextFileView.Enabled = state;
            contextTreeView.Enabled = state;
            menuStrip1.Enabled = state;
            toolbar.Enabled = state;
            toolPlugins.Enabled = state;
            treeViewAdv1.Enabled = state;
            if (state)
                Cursor = Cursors.Default;
            else
                Cursor = Cursors.WaitCursor;
            gridFiles1.Enabled = state;
            textAreaAdv1.Cursor = Cursor;
        }

        /*        private string FormatFileSize(long size)
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
                }*/


        private void GUI_PopulateFileListFromPath(string path)
        {
            Cursor.Current = Cursors.WaitCursor;
            FolderContents cont = new FolderContents(0, 0, 0);
            FileSystemInfo[] infos = Dirs.GetFileSystemInfo(path, cont);
            lblFolders.Text = "";
            if (cont.iFolders > 0)
                lblFolders.Text = "Folders: " + cont.iFolders;

            lblFiles.Text = "";
            lblSize.Text = "";
            if (cont.iFiles > 0)
            {
                lblFiles.Text += "Files: " + cont.iFiles;
                lblSize.Text += "Size: " + BytesUtils.GetSizeDescr(cont.iSize);
            }
            gridFiles1.PopulateFileList(infos); // TODO: Async...
            Cursor.Current = Cursors.Default;
        }

        // flag run in sandbox

        private bool GUI_AddPluginButton(string exeFile)
        {
            if (!File.Exists(exeFile))
                return false;
            string tooltip = "";
            try
            {
                // if it is an assembly gets the description
                System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFrom(exeFile);
                tooltip = AssemblyData.Description(assembly);
            }
            catch (Exception ex)
            {
                // Check if it is an Exe PE
                FileStream fs = File.OpenRead(exeFile);
                byte b1 = (byte)fs.ReadByte();
                byte b2 = (byte)fs.ReadByte();
                fs.Close();
                if (b1 != 0x4D || b2 != 0x5A)
                    return false;
            }
    
            ToolStripButton button = new ToolStripButton();
            button.ImageIndex = imgListPlugins.GetIconIndex(exeFile);
            button.Text = Path.GetFileNameWithoutExtension(exeFile);
            button.ToolTipText = tooltip;
            button.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            button.Click += new EventHandler(button_Click);
            button.Tag = exeFile;

            button.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (Properties.Settings.Default.ToolPlugin_Text)
                button.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;

            toolPlugins.Items.Add(button);
            ToolStripSeparator separ = new ToolStripSeparator();
            toolPlugins.Items.Add(separ);

            ToolStripMenuItem toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem1.Tag = exeFile;
            //            toolStripMenuItem1.Checked = true;
            //            toolStripMenuItem1.CheckOnClick = true;
            //            toolStripMenuItem1.CheckState = System.Windows.Forms.CheckState.Checked;
            toolStripMenuItem1.Text = button.Text;
            toolStripMenuItem1.ImageIndex = button.ImageIndex;
            toolStripMenuItem1.Click += new EventHandler(toolStripMenuItem1_Click);
//            removePluginsToolStripMenuItem.DropDown.ImageList = imgListPlugins.SmallIconsImageList;
            removePluginsToolStripMenuItem.DropDown.ImageList = imgListPlugins.LargeIconsImageList;
            removePluginsToolStripMenuItem.DropDownItems.Add(toolStripMenuItem1);
            return true;
        }

        void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string exeFile = item.Tag as string;
            exeFile = exeFile.ToLower().Trim();
            NokiaCooker.Properties.Settings.Default.Plugins.Remove(exeFile);
            NokiaCooker.Properties.Settings.Default.Save();
            GUI_UpdatePluginsToolbar();
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
                if (!NokiaCooker.Properties.Settings.Default.Plugins.Contains(key))
                {
                    NokiaCooker.Properties.Settings.Default.Plugins.Add(key);
                    NokiaCooker.Properties.Settings.Default.Save();
                }
            }
        }


        private void GUI_UpdatePluginsToolbar()
        {
            //ScanForIntegratedPlugins(PluginsPath);
            // NokiaCooker.Properties.Settings.Default.Plugins.Clear();
            //NokiaCooker.Properties.Settings.Default.Plugins.Add("plugins\\EPICBIZNUS\\navifirm.exe");
            //NokiaCooker.Properties.Settings.Default.Save();

            // Keeps only Label "Plugins:" and Separator
            while (toolPlugins.Items.Count > 2)
                toolPlugins.Items.RemoveAt(2);

            removePluginsToolStripMenuItem.DropDownItems.Clear();

            // Scan della configurazione e aggiunta dei bottoni...
            Size size = new Size(16, 16);
            if (Properties.Settings.Default.ToolPlugin_Big)
            {
                size = new Size(32, 32);
            }
            toolPlugins.ImageScalingSize = size;
            toolPlugins.Width = this.Width;

            int i = 0;
            while (i < NokiaCooker.Properties.Settings.Default.Plugins.Count)
            {
                string exeName = NokiaCooker.Properties.Settings.Default.Plugins[i];
                if (GUI_AddPluginButton(exeName))
                {
                    i++;
                }
                else
                {
                    // removes the entry
                    NokiaCooker.Properties.Settings.Default.Plugins.RemoveAt(i);
                    NokiaCooker.Properties.Settings.Default.Save();
                }
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

                this.TopMost = true;
                this.Focus();
                this.BringToFront();
                this.TopMost = false;
            }
        }


        private string AddParam(string arguments, string param)
        {
            const string quote = "\"";
            if (arguments != string.Empty)
                arguments += " ";
            if (param == string.Empty)
                param = " ";
            arguments += " " + quote + param + quote;
            arguments = arguments.Replace('\\', '/');
            // Necessario usare / invece dello \ perche' crea casini quando si trova a passare \" 
            return arguments;
        }

        private string GetParamsString()
        {
            string arguments = "";
            string fullElemPath = GUI_FocusedFullListViewPath;
            if (!File.Exists(fullElemPath))
                fullElemPath = "";
            arguments = AddParam(arguments, fullElemPath);
            arguments = AddParam(arguments, TmpFiles);
            arguments = AddParam(arguments, Path.GetFileName(fwFile.filename));
            string param4 = "";
            switch (fwFile.FwType)
            {
                case TFwType.CORE:
                    param4 = "CORE";             // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.ROFS:
                    param4 = "ROFS";             // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.ROFX:
                    param4 = "ROFX";             // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.UDA:
                    param4 = "UDA";              // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
                case TFwType.UNKNOWN:
                    param4 = "UNKNOWN";          // UDA, ROFS, ROFX, CORE, UNKNOWN
                    break;
            }
            arguments = AddParam(arguments, param4);
            return arguments;
        }


        // Launch plugin
        void button_Click(object sender, EventArgs e)
        {
            ToolStripButton btn = (sender as ToolStripButton);
            string exePath = btn.Tag.ToString();

            plugin = CreateProcess(exePath);
            plugin.Exited += new EventHandler(rCompExe_Exited);

            string arguments = GetParamsString();

            this.Enabled = false;
            plugin.StartInfo.Arguments = arguments;
            plugin.Start();
        }


        private void GUI_UpdateRecentList()
        {
            recentFilesToolStripMenuItem.DropDownItems.Clear();
            foreach (string recentFile in recent.GetRecentList())
            {
                ToolStripMenuItem recentItem = new System.Windows.Forms.ToolStripMenuItem();
                recentItem.Text = recentFile;
                recentItem.Click += new EventHandler(recentItem_Click);
                recentFilesToolStripMenuItem.DropDownItems.Add(recentItem);
                //                ToolStripItem itm = new ToolStripItem();                
            }
        }

        void recentItem_Click(object sender, EventArgs e)
        {
            string s = sender.ToString();
            GUI_OpenFirmwareFile(s);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);
       
        static uint TOKEN_QUERY = 0x0008;

        WindowsIdentity GetWindowsIdentityForProcess(Process proc)
        {
            WindowsIdentity wi = null;
            IntPtr ph = IntPtr.Zero;
            try
            {
                bool res = OpenProcessToken(proc.Handle, TOKEN_QUERY, out ph);
                wi = new WindowsIdentity(ph);
                // Console.WriteLine(proc.ProcessName + " owned by " + wi.Name);
            }
            catch (Exception xcp)
            {
                // Console.WriteLine(proc.ProcessName + ": " + xcp.Message);
            }
            finally
            {
                if (ph != IntPtr.Zero) { CloseHandle(ph); }
            }
            return wi;
        }


        private void Form1_Load(object sender, EventArgs Add)
        {
            tsBtnCancel = new ToolStripButton("Cancel Operation");
            tsBtnCancel.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            tsBtnCancel.Image = Properties.Resources.Cancel24;
            tsBtnCancel.TextImageRelation = TextImageRelation.TextBeforeImage;
            tsBtnCancel.Click += new EventHandler(tsBtnCancel_Click);
            statusStrip2.Items.Insert(0, tsBtnCancel);
            CancelBtnVisible = false;

#if DEBUG
            ToolStripButton tsBtnRefresh = new ToolStripButton("Refresh Current Size >");
            tsBtnRefresh.Click += new EventHandler(tsBtnRefresh_Click);
            statusStrip2.Items.Insert(6, tsBtnRefresh);
#endif

            //toolStrip2.ImageList = imgListPlugins.SmallIconsImageList;
            toolPlugins.ImageList = imgListPlugins.LargeIconsImageList;

            if (NokiaCooker.Properties.Settings.Default.InitializePlugins)
            {
                ScanForIntegratedPlugins(PluginsPath);
                NokiaCooker.Properties.Settings.Default.InitializePlugins = false;
                NokiaCooker.Properties.Settings.Default.Save();
            }
            removePluginsToolStripMenuItem.DropDown.Closing += new ToolStripDropDownClosingEventHandler(DropDown_Closing);

            // Initialize ToolBars context menus
            menuitemPluginShowText.Checked = Properties.Settings.Default.ToolPlugin_Text;
            menuitemPluginUseBigIcons.Checked = Properties.Settings.Default.ToolPlugin_Big;
            showTextToolStripMenuItem.Checked = Properties.Settings.Default.ToolBar_Text;
            showLargeIconsToolStripMenuItem.Checked = Properties.Settings.Default.ToolBar_Big;

            //LaunchBrowser("www.symbian-toys.com/nokiacooker.aspx");
            UpdateManager updManager = UpdateManager.Instance;
            updManager.UpdateSource = new NAppUpdate.Framework.Sources.SimpleWebSource();
            updManager.Config.TempFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NokiaCooker\\Updates");
            updManager.ReinstateIfRestarted();
            IUpdateSource feedSource = new SimpleWebSource("http://www.symbian-toys.com/updates/nokiacooker_feed_update.xml");
            // Only check for updates if we haven't done so already
            if (updManager.State != UpdateManager.UpdateProcessState.NotChecked)
                return;
            // throw new FuzzyByteException("Update process has already initialized; current state: " + updManager.State.ToString());
            updManager.BeginCheckForUpdates(feedSource, OnCheckUpdatesCompleted, null);
        }

        private void OnCheckUpdatesCompleted(IAsyncResult asyncResult)
        {
            try
            {
                ((UpdateProcessAsyncResult)asyncResult).EndInvoke();
            }
            catch (Exception ex)
            {
                // Notes.ShowWarning("Check updates failed. Check the feed and try again.");
                return;
            }

            // Get a local pointer to the UpdateManager instance
            UpdateManager updManager = UpdateManager.Instance;
            if (updManager.UpdatesAvailable > 0)
            {
#if !DEBUG
                updManager.BeginPrepareUpdates(OnPrepareUpdatesCompleted, null);
                return;
#endif
            }
        }

        private void OnPrepareUpdatesCompleted(IAsyncResult asyncResult)
        {
            try
            {
                ((UpdateProcessAsyncResult)asyncResult).EndInvoke();
            }
            catch (Exception ex)
            {
                // Notes.ShowWarning("Updates preperation failed. Check the feed and try again.");
                return;
            }

            // Get a local pointer to the UpdateManager instance
            UpdateManager updManager = UpdateManager.Instance;
            Notes.ShowInfo("NokiaCooker will be closed and updated to the latest version\n"+
                    "If you experience any issues during the automatic update, try to launch NokiaCooker as Administrator,\notherwise, you can download manually latest version from the official website.");
#if DEBUG
            updManager.ApplyUpdates(true, true, true);
#else
            updManager.ApplyUpdates(true, false, false);
#endif
        }


        void textAreaAdv1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            LaunchBrowser(e.LinkText);            
        }

        void tsBtnCancel_Click(object sender, EventArgs e)
        {
            bwFilesOperation.CancelAsync();
        }

        void tsBtnRefresh_Click(object sender, EventArgs e)
        {
            GUI_SetComputedLabel(iEstimatedSize, fwFile.PartitionSize, false);
            if (!bwComputeSize.IsBusy)
            {
                bwComputeSize.RunWorkerAsync();
            }
        }


        void DropDown_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                e.Cancel = true;
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
            if (bwComputeSize.IsBusy)
            {
                Notes.ShowInfo("Computation of estimated size is in progress.\nPlease try again later.");
                return;
            }
            /*if (lblEstimatedSize.BackColor == Colo iEstimatedSize > fwFile.PartitionSize)
            {
                GUI.ShowWarning("Data exceeds the PartitionSize, can't repack.\nPlease remove "+ BytesUtils.GetSizeDescr(iEstimatedSize- fwFile.PartitionSize));
                return;
            }*/
            textAreaAdv1.DetectUrls = false;
#if USE_BACKGOUND_WORKER
            if (!bwRepack.IsBusy)
                bwRepack.RunWorkerAsync();
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
            const string HTTP = "http://";
            try
            {
                if (url.ToLower().Contains(HTTP))
                    url = url.Remove(0, HTTP.Length);
                System.Diagnostics.Process.Start(HTTP + url);
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
            LaunchBrowser("www.symbian-toys.com/iconhider.aspx");
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            LaunchBrowser("www.symbian-toys.com/rompatcherplus.aspx");
        }
        #endregion

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            //throw new RofsException("BBBB");

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            string fname = openFileDialog1.FileName;
            GUI_OpenFirmwareFile(fname);
        }


        private void GUI_OpenFirmwareFile(string fname)
        {
            if (!GUI_CloseFwFile())
                return;
            if (OpenFirmwareFile(fname))
            {
                //treeViewAdv1.AllowDrop = false;
            }
            GUI_UpdateRecentList();
        }

        private bool OpenFirmwareFile(string fname)
        {
            if (!File.Exists(fname))
                return false;
            recent.UpdateRecentFiles(fname);
            fwFile.Close();
            iNeedsRebuild = false;
            textAreaAdv1.DetectUrls = false;

            removeOldFwFiles();
#if USE_BACKGOUND_WORKER
            if (!bwOpenFile.IsBusy)
                bwOpenFile.RunWorkerAsync(fname);
#else            
            GUI_SetAllEnabled(false);
            fwFile.Open(fname);
            GUI_SetAllEnabled(true);
            GUI_CreateTreeViewNodes();
#endif
            return true;
        }


        private void fwFile_Log(string message, EventType type)
        {
            if (type == EventType.Debug)
            {
#if DEBUG
                Debug.Write(message);
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
            textAreaAdv1.Write(descr + message);
            textAreaAdv1.ScrollToEnd();
#if !USE_BACKGOUND_WORKER
            Application.DoEvents();
#endif
        }


        private bool GUI_CloseFwFile()
        {
            if (GUI_FwChanged)
            {
                if (Notes.ShowQuery("Firmware has not been saved, discard changes and close the firmware?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    return false;
            }

            //treeViewAdv1.AllowDrop = true;
            fwFile.Close();
            lblPartSize.Text = lblEstimatedSize.Text = "";
            Cursor = Cursors.Default;
            GUI_SetAllEnabled(true);
            GUI_SetToolbarEnabled(false);
            treeViewAdv1.MonitorFileSystem = false;
            treeViewAdv1.Nodes.Clear();
            gridFiles1.Clear();
            iNeedsRebuild = false;
            fileSystemWatcher1.EnableRaisingEvents = false;

            removeOldFwFiles();
            return true;
        }


        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GUI_CloseFwFile();
        }

        private void openMenuItem_Click(object sender, EventArgs e)
        {
            btnOpen.PerformClick();
        }

        private void saveMenuItem_Click(object sender, EventArgs e)
        {
            btnSave.PerformClick();
        }


        private void removeOldFwFiles()
        {
            DateTime started = DateTime.Now;
            fwFile_Log("Remove old fw Files. Please wait...\t", EventType.Info);

            // Dirs.DeleteContent(TrashFiles); // nel peggiore dei casi impiega quanto impiegava prima
            string newDir = Dirs.CreateNewDir(TrashFiles + "Tmp");
            Dirs.MoveContent(TmpFiles, newDir); // Move TmpFiles to trashfiles
            Dirs.DeleteContent(TmpFiles);   // nel caso in cui qualche file non sia stato spostato per qualche motivo
            if (!bwDeleteTrashFolder.IsBusy)
                bwDeleteTrashFolder.RunWorkerAsync();

            TimeSpan span = DateTime.Now - started;
            fwFile_Log("Took: " + (int)span.TotalSeconds + " Seconds\n", EventType.Info);
            fwFile_Log("Files successfully removed!\n", EventType.Info);
        }

        #region bw OpenFile
        private void bwOpenFile_DoWork(object sender, DoWorkEventArgs e)
        {
            if (bwComputeSize.IsBusy)
                bwComputeSize.CancelAsync();

            string fullFilename = e.Argument as string;
            fileSystemWatcher1.EnableRaisingEvents = false;
            iEstimatedSize = 0;
            treeViewAdv1.MonitorFileSystem = false;
            bwOpenFile.ReportProgress(-1);
            fwFile.Open(fullFilename);
            fwFile.Read();
            string newFile = TmpImages;

            switch (fwFile.FwType)
            {
                case TFwType.UDA:
                    iImageFile = new Fat16Image();
                    newFile += FAT_IMG;
                    break;
                case TFwType.ROFS:
                    iImageFile = new RofsImage();
                    newFile += ROFS_IMG;
                    break;
                case TFwType.ROFX:
                    iImageFile = new RofsImage();
                    newFile += ROFX_IMG;
                    break;
                case TFwType.CORE:
                    iImageFile = new RofsImage();
                    newFile += CORE_ROFS1_IMG;
                    break;
                default:
                    iImageFile = new NullImage();
                    newFile += UNKN_IMG;
                    break;
            }
            iImageFile.Log += new LogHandler(fwFile_Log);

            if (!checkBox1.Checked)
            {
                fwFile.ExtractAlignedCodeToFile(newFile);

                /*              Sembra funzionare, ma e' trooopo gabuloso
                                FileOperationWrapper.MoveToRecycleBin(TrashFiles);
                                Directory.Move(TmpFiles, TrashFiles);
                                DirBytesUtils.CreateNew(TmpFiles, true);/**/

                // E' un problema perche' potrei aprire un nuovo file prima che il backgroundworker del delete sia stato completato
                //                DirBytesUtils.Delete(TrashFiles);             // nel peggiore dei casi impiega 18 secondi
                //                Directory.Move(TmpFiles, TrashFiles);
                //                DirBytesUtils.CreateNew(TmpFiles, true);

                try
                {
                    iImageFile.OpenImage(newFile);          // Posso gia' buildare l'albero da questo...
                    bwOpenFile.ReportProgress(50);          // Builda l'albero...
                    iImageFile.Extract(TmpFiles);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    iImageFile.CloseImage();
                }
            }
            e.Result = true;
        }

        private void bwOpenFile_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (e.ProgressPercentage == -1)
                {
                    Debug.WriteLine("Initialize");
                    textAreaAdv1.Clear();
                    GUI_SetAllEnabled(false);
                    return;
                }
                if (e.ProgressPercentage == 50)
                {
                    gridFiles1.AllowDrop = false;
                    treeViewAdv1.AllowDrop = false;
                    GUI_SetComputedLabel(iEstimatedSize, fwFile.PartitionSize, false);
                    //                lblEstimatedSize.Text = "Current Size: " + BytesUtils.GetSizeDescr(iEstimatedSize);
                    lblPartSize.Text = "Partition Size: " + BytesUtils.GetSizeDescr(fwFile.PartitionSize);
                    // Build tree
                    List<string> fullList = iImageFile.GetDirsList();
                    fullList.Sort();
                    treeViewAdv1.Nodes.Clear();
                    // string[] dirList = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);
                    treeViewAdv1.BuildTreeView(Path.GetFileName(fwFile.filename), TmpFiles, fullList.ToArray());
                    treeViewAdv1.Enabled = true;
                    //                treeViewAdv1.Update();          /**/
                    gridFiles1.Enabled = true;
                    Cursor = Cursors.Default;
                }
                return;
            }
            catch (FuzzyByteException ex)
            {
                Notes.ShowWarning(ex.Message);
            }
        }


        private void bwOpenFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            gridFiles1.AllowDrop = true;
            treeViewAdv1.AllowDrop = true;
            if (e.Error != null)
            {
                if (e.Error is FuzzyByteException)
                {
                    Notes.ShowWarning(e.Error.Message);
                }
                else
                {
                    Notes.ShowError("Unexpected Error", e.Error.Message);
                }
                GUI_CloseFwFile();
                return;
            }
            fileSystemWatcher1.EnableRaisingEvents = true;
            GUI_SetAllEnabled(true);
            GUI_SetToolbarEnabled(true);

            //GUI_SetComputedLabel(iEstimatedSize, fwFile.PartitionSize, false);
            //lblPartSize.Text = "Partition Size: " + BytesUtils.GetSizeDescr(fwFile.PartitionSize);

            string selPath = treeViewAdv1.SelectedPath;

            // Rebuild Tree
            treeViewAdv1.Nodes.Clear();
            treeViewAdv1.BuildTreeView(Path.GetFileName(fwFile.filename), TmpFiles);
            treeViewAdv1.SelectedPath = selPath;
            treeViewAdv1.MonitorFileSystem = true;
            //treeViewAdv1.Update();

            if (!bwComputeSize.IsBusy && !checkBox1.Checked)
            {
                updateSizeLabelTimer.Start();
                bwComputeSize.RunWorkerAsync();
            }

            switch (fwFile.FwType)
            {
                case TFwType.CORE:
                    fwFile_Log("Perform the changes to the ROFS1 and eventually press the save button to repack the firmware.\n", EventType.Info);
                    //                    GUI.ShowInfo("Note: CORE Can't be repacked yet!");
                    break;
                case TFwType.ROFS:
                    fwFile_Log("Perform the changes to the ROFS and eventually press the save button to repack the firmware.\n", EventType.Info);
                    break;
                case TFwType.ROFX:
                    fwFile_Log("Perform the changes to the ROFX and eventually press the save button to repack the firmware.\n", EventType.Info);
                    // GUI.ShowInfo("Note: the ROFX repack is still EXPERIMENTAL... Use at your own Risk!");
                    break;
                case TFwType.UDA:
                    fwFile_Log("Perform the changes to the FAT  and eventually press the save button to repack the firmware.\n", EventType.Info);
                    break;
                default:
                    fwFile_Log("This Format is Unknown.\n", EventType.Info);
                    break;
            }

            //DUMP_SHA1(); // TODO: Remove after
        }
        #endregion


        #region bw Repack
        private void bwRepack_DoWork(object sender, DoWorkEventArgs e)
        {
            if (fwFile.filename == "")
                return;
            string fullFilename = fwFile.filename;
            fileSystemWatcher1.EnableRaisingEvents = false;
            bwRepack.ReportProgress(-1);    // Initialize GUI
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
                    imageFile = new RofsImage();
                    newFile += CORE_ROFS1_IMG;
                    break;
                default:
                    imageFile = new NullImage();
                    newFile += UNKN_IMG;
                    break;
            }
            // Se non c'e' stato nessun cambiamento istanzia NullImage cosi' da non fare il rebuild dell'immagine...
            if (!iNeedsRebuild)
                imageFile = new NullImage();

            imageFile.Log += new LogHandler(fwFile_Log);

            string dirName = Path.GetDirectoryName(fullFilename) + Path.DirectorySeparatorChar;
            string fname = Path.GetFileNameWithoutExtension(fullFilename);
            string ext = Path.GetExtension(fullFilename);
            if (!File.Exists(dirName + fname + "_BACKUP_" + ext))
            {
                if (FuzzyByte.NokiaCooker.Properties.Settings.Default.MakeBackup)
                {
                    fwFile_Log("Create Backup File: " + dirName + fname + "_BACKUP_" + ext + "\n", EventType.Info);
                    File.Copy(fullFilename, dirName + fname + "_BACKUP_" + ext);
                }
            }

            imageFile.RebuildImage(newFile, TmpFiles);

            /*            imageFile.OpenImage(newFile);
                        imageFile.ClearImage();
                        imageFile.AddAllFilesAndFolders(TmpFiles, "");
                        imageFile.CloseImage();*/

            fwFile.Repack(newFile);
            e.Result = true;
        }

        private void bwRepack_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (e.ProgressPercentage == -1)
                {
                    GUI_SetAllEnabled(false);
                }
            }
            catch (FuzzyByteException ex)
            {
                Notes.ShowWarning(ex.Message);
            }
        }

        private void bwRepack_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                if (e.Error is FuzzyByteException)
                {
                    Notes.ShowWarning(e.Error.Message);
                }
                else
                {
                    Notes.ShowError("Unexpected Error", e.Error.Message);
                }
            }
            else
            {
                GUI_FwChanged = false;
                iNeedsRebuild = false;
            }
            fileSystemWatcher1.EnableRaisingEvents = true;
            GUI_SetAllEnabled(true);
            GUI_SetToolbarEnabled(true);
            //            fwFile_Log("ALL DONE! *** Firmware repacked: " + fwFile.filename + "\n", EventType.Info);
            fwFile_Log("DONE!\n\n", EventType.Info);
            fwFile_Log("******* 26/Nov/2011: Open message to Cookers: NokiaCooker needs YOU! *******\n", EventType.Info);
            fwFile_Log("If you like NokiaCooker and want to help its development, it would be very nice if you'll add a short message like the one below, when you'll publish your CFW on forums, or on your blog:\n\n[quote] This CFW has been created with NokiaCooker. Consider donating for Free and High Quality softwares. http://www.symbian-toys.com/NokiaCooker.aspx [/quote]\n\nThat message will not cost you anything and you'll help a LOT the development of NokiaCooker! :)\n", EventType.Info);
            fwFile_Log("\n******* 30/Oct/2012: Updated News and Concerns *******\nProbabily this will be the last NC release.\n\nAs everyone can see, end-users are not interested in supporting NokiaCooker.\nAbout a year ago, on 26/Nov/2011, I asked to you Cookers to include a short message in your CFW release, hoping it would help NokiaCooker.\n", EventType.Info);
            fwFile_Log("During these days I looked at 20 CFW published recently and all of them were created with NokiaCooker\n", EventType.Info);
            fwFile_Log("But, I noticed that only 10% of them included that \"support message\", while the 90% just ignored it.\nIf things will not change soon, NokiaCooker will be definitely suspended. No further warning messages will follow\n\nA lot of End-Users, Modders and you Cookers, are upset because Symbian is dying, day by day...\nBut, are you really doing all you can to keep it alive?\n", EventType.Info);
            fwFile_Log("\nI did my part and you're supposed to do yours\nSo, let's hope those 10/90 will become soon 90/10 ;) Come on!\n", EventType.Info);
            fwFile_Log("\nThank you for your understanding,\nMarco (aka Il.Socio)\n", EventType.Info);
            textAreaAdv1.DetectUrls = true;
        }
        #endregion


        #region DEBUG STUFF

        private void DUMP_SHA1onContent()
        {
            UInt32 initOffset = 0;
            UInt32 endOffset = 0;
            UInt32 len2 = 0;
            byte[] sha1 = null;
            TGroup group = fwFile.blocksColl.GetRofsOrUdaGroupForFwType(fwFile.FwType);
            if (group == null)
                return;

            Int32 lenDescrBlocks = 0;
            foreach (TBlock aBlock in group.certBlocks)
            {
                Content_Sha1 cnt = new Content_Sha1(aBlock.content);
                Debug.WriteLine(cnt);
                initOffset = cnt.startSha1Offset;
                endOffset = cnt.endSha1Offset;
                sha1 = cnt.hash20_sha1_on_data;
                len2 = cnt.length2;
                lenDescrBlocks += aBlock.content.Length;
            }

            
            SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
            cryptoTransformSHA1.Initialize();
            
            int dataToRead = (int)(endOffset - initOffset + 1); // 0x626cfe + 0xC00 = 0x627900
            foreach (TBlock aBlock in group.codeBlocks)
            {
                if (dataToRead > 0)
                {
                    int lenRead = aBlock.content.Length;
                    if (lenRead >= dataToRead)
                    {
                        lenRead = dataToRead;
                        cryptoTransformSHA1.TransformFinalBlock(aBlock.content, 0, lenRead);
                        dataToRead = 0;
                    }
                    else
                    {
                        byte[] arr = new byte[aBlock.content.Length];
                        dataToRead -= cryptoTransformSHA1.TransformBlock(aBlock.content, 0, lenRead, arr, 0);
                    }
                }
            }

            //cryptoTransformSHA1.TransformFinalBlock(new byte[0], 0, 0);
            string block_sha1 = BytesUtils.ToHex(sha1).Replace(" ", "");
            string comp_sha1 = BytesUtils.ToHex(cryptoTransformSHA1.Hash).Replace(" ", "");


            // DUMP...
            List<byte> list = new List<byte>();
            foreach (TBlock aBlock in group.codeBlocks)
            {
                list.AddRange(aBlock.content);
            }


            string outPath = Path.GetDirectoryName(fwFile.filename) + Path.DirectorySeparatorChar + "SHA1" + Path.DirectorySeparatorChar;
            Dirs.CreateNewDir(outPath, true);
            if (comp_sha1 != block_sha1)
            {
                string fname = outPath + comp_sha1 + "_" + block_sha1 + ".no.bin";
                File.WriteAllBytes(fname, list.ToArray());
            }
            else
            {
                string fname = outPath + comp_sha1 + "_" + block_sha1 + ".ok.bin";
                File.WriteAllBytes(fname, new byte[0]);
            }
            /**/
        }

        private void DUMP_ALL_SHA1onContent()
        {
            string outPath = Path.GetDirectoryName(fwFile.filename) + Path.DirectorySeparatorChar + "SHA1" + Path.DirectorySeparatorChar;            

            foreach (TGroup group in fwFile.blocksColl.GetGroups())
            {
                switch (group.Descr)
                {
                    case "[KEYS]":
                        continue;
                    case "[PASUBTOC]":
                        continue;
                    case "[PAPUBKEYS]":
                        continue;
                    case "[PRIMAPP]":
                        continue;
                    default:
                        break;
                }
                if (group.certBlocks.Count == 0)
                    continue;

                UInt32 initOffset = 0;
                UInt32 endOffset = 0;
                UInt32 len2 = 0;
                string sha1_cnt = "";

                byte[] oldCont = null;
                foreach (TBlock aBlock in group.certBlocks)
                {
                    Debug.WriteLine(aBlock);
                    Content_Sha1 cnt = new Content_Sha1(aBlock.content);
                    Debug.WriteLine(cnt);
                    initOffset = cnt.startSha1Offset;
                    endOffset = cnt.endSha1Offset;
                    len2 = cnt.length2;
                    string new_sha1_cnt = BytesUtils.ToHex(cnt.hash20_sha1_on_data).Replace(" ", "");           // Uguale per tutti i blocchi di questo gruppo... calcolato sui blocchi dati 17
                    if (sha1_cnt != "" && sha1_cnt != new_sha1_cnt)
                        throw new FwException("Err");
                    sha1_cnt = new_sha1_cnt;

                    TBlockHeader27_ROFS_Hash headerHash = aBlock.blockHeader as TBlockHeader27_ROFS_Hash;
                    string rootkey = BytesUtils.ToHex(headerHash.cmt_root_key_hash20_sha1).Replace(" ", "");
                    string signature = BytesUtils.ToHex(cnt.maybe_signature_rsa_sha1).Replace(" ", "");
                    // Forse è la signature che puo' essere verificata usando la rootkey (RootKey) Ho signature differenti, perchè ci sono differenti RootKey
                    // TODO: Dovrei trovare 2 rootkey uguali con 2 contenuti uguali per verificare se ottengo 2 signature uguali
                    string fname = outPath + group.Descr + "_" + rootkey + "_" + sha1_cnt + "_" + signature.Substring(0, 100);
                    //File.WriteAllBytes(fname, aBlock.content);
                }


                SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
                cryptoTransformSHA1.Initialize();
                
                int dataToRead = (int)(endOffset - initOffset + 1); // 0x626cfe + 0xC00 = 0x627900
                foreach (TBlock aBlock in group.codeBlocks)
                {
                    if (dataToRead <= 0)
                        continue;
                    if (dataToRead <= aBlock.content.Length)
                    {
                        // This block does contain enough data to fullfill the request
                        cryptoTransformSHA1.TransformFinalBlock(aBlock.content, 0, dataToRead);
                        dataToRead = 0;
                    }
                    else
                    {
                        // This block doesn't contain enough data to fullfill the request
                        byte[] dummy_out = new byte[aBlock.content.Length];
                        dataToRead -= cryptoTransformSHA1.TransformBlock(aBlock.content, 0, aBlock.content.Length, dummy_out, 0);
                    }
                }
                if (dataToRead < 0)
                {
                }
                if (dataToRead > 0)
                {
                    byte[] arr = new byte[dataToRead];
                   // File.WriteAllBytes(@"k:\\aa.bin", arr);
                    cryptoTransformSHA1.TransformFinalBlock(arr, 0, arr.Length);
                    dataToRead = 0;
                }

                //cryptoTransformSHA1.TransformFinalBlock(new byte[0], 0, 0);
                string comp_sha1 = "000000000000000000000000000";
                if (group.codeBlocks.Count > 0)
                    comp_sha1 = BytesUtils.ToHex(cryptoTransformSHA1.Hash).Replace(" ", "");

                // DUMP...
                List<byte> list = new List<byte>();
                foreach (TBlock aBlock in group.certBlocks)
                {
                    list.AddRange(aBlock.content);
                }
                foreach (TBlock aBlock in group.codeBlocks)
                {
                    list.AddRange(aBlock.content);
                }

                if (comp_sha1 != sha1_cnt)
                {
                    string fname = outPath + "no_" + group.Descr + "_" + comp_sha1 + "_" + sha1_cnt + ".bin";
                    File.WriteAllBytes(fname, list.ToArray());
                }
                else
                {
                    string fname = outPath + "ok_" + group.Descr + "_" + comp_sha1 + "_" + sha1_cnt + ".bin";
                    File.WriteAllBytes(fname, new byte[0]);
                }
            }
        }

        private bool FIND_SHA1(byte[] allBytes2, string hashToFind, out int offs, out int len)
        {
            offs = 0;
            len = 0;
            int i = 0;
            // for (int i = 0; i < allBytes2.Length; i++)
            {
                for (int dataLen = allBytes2.Length - i; dataLen > 0; dataLen--)
                {
                    byte[] newArr = new byte[dataLen];
                    Array.Copy(allBytes2, i, newArr, 0, newArr.Length);
                    string sha1 = Analysis.ComputeSHA1(newArr).Replace(" ", "");
                    if (sha1.Substring(4) == hashToFind.Substring(4))
                    {
                        offs = i;
                        len = dataLen;
                        return true;
                        // e' lo SHA1 calcolato sui dati senza i pacchetti di check.
                        // ed e' calcolato senza il padding finale

                        // 71303 invece di 70F04 (ci sono 0x3FF byte di troppo)

                        // 0x00002CD3 (ci sono 0x3FF byte di troppo rispetto al 0x28D4)
                        // start = 0x537    +   dataLen = 0x28D4    =>   end = 0x2E0B
                        // EndFile = 0x2F37


                        // 0x00002D3B (ci sono 0x3FF byte di troppo rispetto al 0x293C)
                        // start = 0x537    +   dataLen = 0x293C    =>   end = 0x2E73
                        // EndFile = 0x2F37
                    }
                }
                Debug.WriteLine("Done Offset: " + offs);
                Application.DoEvents();
            }
            return false;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            /*byte[] data = File.ReadAllBytes(@"K:\fw\PortC6_5800\SHA1\_toCompute_699E178822112961C02A07111323468C80D8B272.bin");
            int off = 0;
            int len=0;
            FIND_SHA1(data, "699E178822112961C02A07111323468C80D8B272", out off, out len);
            return;/**/

            // Dumpa gli SHA1 del content
            //DUMP_ALL_SHA1onContent();
            // DUMP_SHA1onContent();    
            //return;

            foreach (TGroup aGroup in fwFile.blocksColl.GetGroups())
            {
                if (aGroup.certBlocks.Count == 0)
                    continue;

                foreach (TBlock certBlock in aGroup.certBlocks)
                {
                    TBlockHeader27_ROFS_Hash headerHash = certBlock.blockHeader as TBlockHeader27_ROFS_Hash;

                    string rootkey = BytesUtils.ToHex(headerHash.cmt_root_key_hash20_sha1).Replace(" ", "");
                    
                    // KEYS:    gli SHA1 Header matchano con il contenuto...
                    // "B8C3ADECFC997FCD8081D3DEAF870B8CBC33AE82"
                    // "479C6DDE3942E12C429C1D6ADED803710E52EBD3"
                    // "C70CB07324056BC66A824347F40DB2D5E198D1B2"
                    // "F2D76DFAFD66C7F195F278417DF05888EC3294E3"
                    // "38F312750F686F9FC9B1B3778774A19550F16CA3"
                    // "CAEEBB65D3C48E6DC73B49DC5063A2EE5867D611"

                    // ADA:
                    // "38F312750F686F9FC9B1B3778774A19550F16CA3"
                    // "CAEEBB65D3C48E6DC73B49DC5063A2EE5867D611"
                    // "F2D76DFAFD66C7F195F278417DF05888EC3294E3"
                    // "C70CB07324056BC66A824347F40DB2D5E198D1B2"
                    // "479C6DDE3942E12C429C1D6ADED803710E52EBD3"
                    // "B8C3ADECFC997FCD8081D3DEAF870B8CBC33AE82"
                    Debug.WriteLine(aGroup.Descr + " " + rootkey);
                    switch (aGroup.Descr)
                    {
                        case "[KEYS]":
                            // KEYS
                            byte[] buff_unkn = new byte[4];
                            Array.Copy(certBlock.content, 0, buff_unkn, 0, 4);

                            byte[] buff = new byte[136];    // 0x88 bytes
                            Array.Copy(certBlock.content, 4, buff, 0, 136);
                            string compSha1 = Analysis.ComputeSHA1(buff).Replace(" ", "");

                            // Debug.WriteLine(BytesUtils.ToHex(certBlock._offset) + "  " + BytesUtils.ToHex(buff_unkn).Replace(" ", "") + " " + sha1InHeader);
                            if (compSha1 == rootkey)
                            {
                                //Debug.WriteLine(BytesUtils.ToHex(buff).Replace(" ", ""));
                                //Debug.WriteLine("");
                            }
                            break;
                        case "[PASUBTOC]":
                            break;
                        case "[PAPUBKEYS]":
                            break;
                        case "[PRIMAPP]":
                            break;
                        default:
                            // ADA
                            // SOS UPDAPP Controlla Length2 e Length3
                            // DSP0 Controlla

                            // TODO: controlla contenuto... e verifica meglio la lunghezza.

                            Content_Sha1 cnt = new Content_Sha1(certBlock.content);
                            string sha1InContent = BytesUtils.ToHex(cnt.hash20_sha1_on_data);
                            sha1InContent = sha1InContent.Replace(" ", "");
                            Debug.WriteLine(BytesUtils.ToHex(certBlock._offset) + " " + cnt.ToString() + "  Sha1InContent:" + sha1InContent);
                             
                            break;
                    }/**/
                }
            }
            return;

            FwFile test = new FwFile();
/*            test.Open(@"K:\fw\PortC6_5800\ORI_Port\RM-356_60.0.003_prd.rofs2.v05"); // E' un file .fpsx embedded
            test.Read();
            test.ExtractGroupsToPath(@"K:\fw\PortC6_5800\Groups_Port\");
            test.Close();
            test.Open(@"K:\fw\0591778\ORI_5800\RM-356_52.0.101_prd.rofs2.v01"); // E' un file .fpsx embedded
            test.Read();
            test.ExtractGroupsToPath(@"K:\fw\PortC6_5800\Groups_5800\");
            test.Close();
            test.Open(@"K:\fw\0595425\ORI_C6\RM-612_42.0.004_prd.rofs2.v08"); // E' un file .fpsx embedded
            test.Read();
            test.ExtractGroupsToPath(@"K:\fw\PortC6_5800\Groups_C6\");
            test.Close();/**/

            test.Open(@"K:\fw\PortC6_5800\ORI_Port\RM-356_60.0.003_prd.core.c0r"); // E' un file .fpsx embedded
            test.Read();
            test.ExtractGroupsToPath(@"K:\fw\PortC6_5800\Groups_Port\");
            test.Close();/**/
            test.Open(@"K:\fw\0591778\ORI_5800\RM-356_60.0.003_prd.core.c00"); // E' un file .fpsx embedded
            test.Read();
            test.ExtractGroupsToPath(@"K:\fw\PortC6_5800\Groups_5800\");
            test.Close();/**/
            test.Open(@"K:\fw\0595425\ORI_C6\RM-612_42.0.004_prd.core.c00"); // E' un file .fpsx embedded
            test.Read();
            test.ExtractGroupsToPath(@"K:\fw\PortC6_5800\Groups_C6\");
            test.Close();/**/
            /*
            FwFile test = new FwFile();
            test.Open(@"K:\fw\0591778\fw60_5800_DataFA.bin"); // E' un file .fpsx embedded
            // test.Open(@"K:\fw\0591778\fw52_5800_DataF3.bin"); // Contiene header + un immagine
            test.Read();
            test.ExtractGroupsToPath(@"K:\fw\0591778\fw60_FA\");
            test.Close();
            return;*/
/*            Fat16Image img1 = new Fat16Image();
            img1.OpenImage(@"C:\ProgettiInCorso\NokiaCooker\NokiaCooker\bin\Debug\Images\FullIma.ima");            
            img1.CloseImage();
            return;*/
            //            fwFile.ExtendRofs1();
            // TODO: Retrieves sections and show sizes in the GUI...
            // uda.fwFile.Repartition(0x1000, 0x2000);
            // rofs2.fwFile.Repartition(0x1000, 0x2000);
            // rofs3.fwFile.Repartition(0x1000, 0x2000);

//            fwFile.RepartitionCore(0x10000000, 0x0b000000, 0x00400000, 0x01000000);

            //TLV_Partition_Info_BB5 bb5 = fwFile.header.GetPartitionInfoBb5TLV();
            //bb5.ChangeSystemEnd(0x0D980000);
            /*TGroup keys = fwFile.blocksColl.GetGroupMatchingDescrition("KEYS");
            if (keys != null)
            {
                TBlockType27_ROFS_Hash block_rofs_hash = keys.certBlocks[0].blockHeader as TBlockType27_ROFS_Hash;
                // TODO: Check block_rofs_hash.cmt_root_key_hash20_sha1 
            }
            TGroup grp = fwFile.blocksColl.GetGroupMatchingDescrition("PASUB");
            Content_PaSubToc cnt = new Content_PaSubToc(grp.certBlocks[0].content);
            cnt.ExtractToPath(@"C:\Users\Root\Desktop\Extr\");*/

            //fwFile.ExtractGroupsToPath(@"C:\Users\Root\Desktop\Extr\");
            //fwFile.ExtractRawImageToFile(@"C:\Users\Root\Desktop\Extr\raw.img");
            //8A4EA50765 6D 43 12 F4 DA 94 A6 44 D5 8B 48 438222BB

            /*
            TGroup group = fwFile.blocksColl.GetGroupForFwType(fwFile.FwType);            
            Content_Sha1 blk = new Content_Sha1(group.descrBlock.content);

            //byte[] allBytes2 = group.list[0].content; 
            byte[] allBytes2 = File.ReadAllBytes(@"C:\Users\Root\Desktop\Extr\3477_00000D95_SOS+ROFS3_17.bin");
            string sha1_in_block = BytesUtils.ToHex(blk.hash20_sha1);
            sha1_in_block = sha1_in_block.Replace(" ", "");

            long dataLen1 = blk.length2 - group.descrBlock.content.Length;
            byte[] newArr1 = new byte[dataLen1];
            Array.Copy(allBytes2, 0, newArr1, 0, newArr1.Length);
            string sha1_1 = Analysis.ComputeSHA1(newArr1).Replace(" ", "");
            if (sha1_in_block == sha1_1)
            {
                return;
            }
            for (int i = 0; i < allBytes2.Length; i++)
            {
                for (int dataLen = allBytes2.Length - i; dataLen > 0; dataLen--)
                {
                    byte[] newArr = new byte[dataLen];
                    Array.Copy(allBytes2, i, newArr, 0, newArr.Length);
                    string sha1 = Analysis.ComputeSHA1(newArr).Replace(" ", "");
                    if (sha1 == sha1_in_block)
                    {
                        // e' lo SHA1 calcolato sui dati senza i pacchetti di check.
                        // ed e' calcolato senza il padding finale

                        // 71303 invece di 70F04 (ci sono 0x3FF byte di troppo)

                        // 0x00002CD3 (ci sono 0x3FF byte di troppo rispetto al 0x28D4)
                        // start = 0x537    +   dataLen = 0x28D4    =>   end = 0x2E0B
                        // EndFile = 0x2F37

                        
                        // 0x00002D3B (ci sono 0x3FF byte di troppo rispetto al 0x293C)
                        // start = 0x537    +   dataLen = 0x293C    =>   end = 0x2E73
                        // EndFile = 0x2F37
                    }
                    Application.DoEvents();
                }
            }/**/
            /*            byte[] allBytes2 = File.ReadAllBytes(@"c:\Data1.bin");
                        for (int i=0; i<allBytes2.Length; i++)
                        {
                            byte[] newArr = new byte[allBytes2.Length-i];
                            Array.Copy(allBytes2, i, newArr, 0, newArr.Length);
                            string md5 = Analysis.ComputeMD5(newArr).Replace(" ", "");
                            if (md5.StartsWith("916F75") ||md5.StartsWith("1BE8C8"))
                            {
                            }
                        }*/
            return;

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
        #endregion


        private void button2_Click(object sender, EventArgs e)
        {
            // listViewFiles.RebuildColumns();
            //            listViewFiles.RedrawItems(0, listViewFiles.Items.Count-1, false);
        }

        private void treeViewAdv1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //Debug.WriteLine("TreeView AfterSelect");
            string path = treeViewAdv1.SelectedPath;
            if (path.StartsWith("" + Path.DirectorySeparatorChar))
                path = path.Substring(1);

            treeViewAdv1.SuspendLayout(); // To avoid flicker of the TreeView
            //treeViewAdv1.BeginUpdate(); // To avoid flicker of the TreeView
            GUI_PopulateFileListFromPath(TmpFiles + path);
            //treeViewAdv1.EndUpdate();
            treeViewAdv1.ResumeLayout();
        }

        private void tableFileList_DoubleClick(object sender, EventArgs e)
        {
        }


        private void GUI_ListView_EnterFolderOrLaunchFile()
        {
            string fullElemPath = GUI_FocusedFullListViewPath;
            if (Directory.Exists(fullElemPath))
            {
                string elem = gridFiles1.GetNameFromRowIndex(gridFiles1.FocusedRowIndex);
                treeViewAdv1.SelectedPath = treeViewAdv1.SelectedPath + elem;
                // treeViewAdv1.SelectChildNode(elem);
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
                string elem = gridFiles1.GetNameFromRowIndex(gridFiles1.FocusedRowIndex);
                return GUI_FullTreeViewPath + elem;
            }
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            //throw new Exception("CCCC");

            if (treeViewAdv1.Focused)
            {
                TreeNode aNode = treeViewAdv1.SelectedNode;
                if (aNode == null || aNode.Text == "" || aNode.Parent == null)
                    return;
                //if (GUI.ShowQuery("Delete the Folder: " + treeViewAdv1.SelectedNode.Name + "?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                //    return;

                string destPath = GUI_FullTreeViewPath;

                FileOperation oper = new FileDeleteOperation(destPath);
                if (!bwFilesOperation.IsBusy)
                    bwFilesOperation.RunWorkerAsync(oper);
            }
            if (gridFiles1.Focused)
            {
                // Potrebbe essere stata selezionata piu' di una entry...
                List<string> list = new List<string>();
                string rootPath = GUI_FullTreeViewPath;
                foreach (int aRowIndex in gridFiles1.SelectedIndicies)
                {
                    if (aRowIndex < 0)
                        continue;
                    string elemName = gridFiles1.GetNameFromRowIndex(aRowIndex);
                    if (elemName == "")
                        continue;
                    list.Add(rootPath + elemName);
                }
                FileOperation oper = new FileDeleteOperation(list.ToArray());
                if (!bwFilesOperation.IsBusy)
                    bwFilesOperation.RunWorkerAsync(oper);
            }/**/
        }


        private void btnDonate_Click(object sender, EventArgs e)
        {
            //throw new FwException("CCCC");

            // Donate Link
            LaunchBrowser("www.symbian-toys.com/donatebtn.aspx?sw=NokiaCooker&retpage=NokiaCooker.aspx#donate");
        }



        private void treeViewAdv1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyValue == 0x2E)
            {
                btnDelete.PerformClick();
            }
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
            GUI_OpenFirmwareFile(items[0]);
        }



        #region Import Stuff
        private void ImportImage(string srcFile, string destFile, AbstractImage imageFile)
        {
            // Copy the file in \images\
            try
            {
                File.Copy(srcFile, destFile, true);
                fwFile.Repack(destFile);
                // Unpack
                Dirs.DeleteContent(TmpFiles);
                //Directory.Delete(TmpFiles, true);
                //Directory.CreateDirectory(TmpFiles);
                imageFile.OpenImage(destFile);
                imageFile.Extract(TmpFiles);
                imageFile.CloseImage();
            }
            catch (FuzzyByteException ex)
            {
                Notes.ShowWarning(ex.Message);
            }

            GUI_SetAllEnabled(true);
            GUI_SetToolbarEnabled(true);

            // Build Tree
            treeViewAdv1.Nodes.Clear();
            treeViewAdv1.BuildTreeView(Path.GetFileName(fwFile.filename), TmpFiles);

            fwFile_Log("ALL DONE! *** Firmware repacked: " + fwFile.filename, EventType.Info);
            fwFile_Log("******* 26/Nov/2011: Open message to Cookers: NokiaCooker needs YOU! *******\n", EventType.Info);
            fwFile_Log("If you like NokiaCooker and want to help its development, it would be very nice if you'll add a short message like the one below, when you'll publish your CFW on forums, or on your blog:\n\n[quote] This CFW has been created with NokiaCooker. Consider donating for Free and High Quality softwares. http://www.symbian-toys.com/NokiaCooker.aspx [/quote]\n\nThat message will not cost you anything and you'll help a LOT the development of NokiaCooker! :)\n", EventType.Info);
            fwFile_Log("\n******* 30/Oct/2012: Updated News and Concerns *******\nProbabily this will be the last NC release.\n\nAs everyone can see, end-users are not interested in supporting NokiaCooker.\nAbout a year ago, on 26/Nov/2011, I asked to you Cookers to include a short message in your CFW release, hoping it would help NokiaCooker.\n", EventType.Info);
            fwFile_Log("During these days I looked at 20 CFW published recently and all of them were created with NokiaCooker\n", EventType.Info);
            fwFile_Log("But, I noticed that only 10% of them included that \"support message\", while the 90% just ignored it.\nIf things will not change soon, NokiaCooker will be definitely suspended. No further warning messages will follow\n\nA lot of End-Users, Modders and you Cookers, are upset because Symbian is dying, day by day...\nBut, are you really doing all you can to keep it alive?\n", EventType.Info);
            fwFile_Log("\nI did my part and you're supposed to do yours\nSo, let's hope those 10/90 will become soon 90/10 ;) Come on!\n", EventType.Info);
            textAreaAdv1.DetectUrls = true;
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
        #endregion

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void btnResize_Click(object sender, EventArgs e)
        {
            /*            iAllowResizing = !iAllowResizing;
                        if (iAllowResizing)
                            btnResize.Image = global::FuzzyByte.NokiaCooker.Properties.Resources.chk_on;
                        else
                            btnResize.Image = global::FuzzyByte.NokiaCooker.Properties.Resources.chk_off;*/
        }

        private void nokiaCookerBETA01MarcoBellinoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LaunchBrowser("www.symbian-toys.com/nokiacooker.aspx");
        }

        private void toolStrip2_DragDrop(object sender, DragEventArgs e)
        {
            Debug.WriteLine("TREE DragDrop");
            string[] items = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (items.Length <= 0)
                return;
            foreach (string item in items)
            {
                string key = item.ToLower().Trim();
                if (!FuzzyByte.NokiaCooker.Properties.Settings.Default.Plugins.Contains(key))
                    FuzzyByte.NokiaCooker.Properties.Settings.Default.Plugins.Add(key);
            }
            FuzzyByte.NokiaCooker.Properties.Settings.Default.Save();
            GUI_UpdatePluginsToolbar();
        }

        private void toolStrip2_DragEnter(object sender, DragEventArgs e)
        {
            Debug.WriteLine("ToolStrip DragEnter");
            // If the data is a file or a bitmap, display the copy cursor.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
                return;
            }
        }

        private void addNewPluginToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (openFileDialog2.ShowDialog() != DialogResult.OK)
                return;
            foreach (string aFile in openFileDialog2.FileNames)
            {
                string key = aFile.ToLower().Trim();
                if (!NokiaCooker.Properties.Settings.Default.Plugins.Contains(key))
                {
                    NokiaCooker.Properties.Settings.Default.Plugins.Add(key);
                    NokiaCooker.Properties.Settings.Default.Save();
                }
            }
            GUI_UpdatePluginsToolbar();
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
            toolStripSplitButton1.ShowDropDown();
        }

        private void button2_Click_2(object sender, EventArgs e)
        {
            fwFile.ExtractGroupsToPath("C:\\Users\\Root\\Desktop\\Extr\\");
            //fwFile.ExtractContentTypeToFile("C:\\Users\\Root\\Desktop\\code.bin", TContentType.Code);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            fwFile.DumpToFile();
        }



        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("Changed: " + e.FullPath);
            if (!iNeedsRebuild)
            {
                iNeedsRebuild = true;
                GUI_FwChanged = true;
            }
            iFileSystemChangedRecently = true;
        }


        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("Created: " + e.FullPath);
            if (!iNeedsRebuild)
            {
                iNeedsRebuild = true;
                GUI_FwChanged = true;
            }
            iFileSystemChangedRecently = true;
        }


        private void fileSystemWatcher1_Deleted(object sender, FileSystemEventArgs e)
        {
            Debug.WriteLine("Deleted: " + e.FullPath);
            if (!iNeedsRebuild)
            {
                iNeedsRebuild = true;
                GUI_FwChanged = true;
            }
            iFileSystemChangedRecently = true;
        }

        private void fileSystemWatcher1_Renamed(object sender, RenamedEventArgs e)
        {
            Debug.WriteLine("Renamed:" + e.FullPath);
            if (!iNeedsRebuild)
            {
                iNeedsRebuild = true;
                GUI_FwChanged = true;
            }
            iFileSystemChangedRecently = true;
        }


        private void timerCompute_Tick(object sender, EventArgs e)
        {
            // Serve per gestire i cambiamenti tramite accesso diretto al file-system
            // sia per file che directory, sia per TreeView che per FileView
            if (iFileSystemChangedRecently)
            {
                iFileSystemChangedRecently = false;
                iNeedsRecomputeSize = true;
                GUI_SetComputedLabel(iEstimatedSize, fwFile.PartitionSize, false);
                GUI_SetAllEnabled(false);
                return;
            }
            if (iNeedsRecomputeSize)
            {
                GUI_PopulateFileListFromPath(GUI_FullTreeViewPath);
                GUI_SetAllEnabled(true);
                if (!bwComputeSize.IsBusy)
                {
                    bwComputeSize.RunWorkerAsync();
                }
            }
        }


        private void GUI_SetComputedLabel(UInt64 estimSize, UInt64 partSize, bool isFinalValue)
        {
            /*            if (this.InvokeRequired)
                        {
                            MethodInvoker d = delegate { GUI_SetComputedLabel(estimSize, partSize, isFinalValue); };
                            this.Invoke(d);
                            return;
                        }/**/

            lblEstimatedSize.Text = "Estimated Size: ";
            if (isFinalValue && partSize > 0)
            {
                lblEstimatedSize.Text += BytesUtils.GetSizeDescr(estimSize);

                Int64 diffSize = (Int64)(fwFile.PartitionSize - estimSize);
                if (diffSize >= -500000 && diffSize <= 500000)
                {
                    // lblEstimatedSize.BackColor = Color.Black;
                    // lblEstimatedSize.ForeColor = Color.Yellow;
                    lblEstimatedSize.ForeColor = Color.Black;
                    lblEstimatedSize.BackColor = Color.Yellow;
                    return;
                }
                if (diffSize > 0)
                {
                    lblEstimatedSize.ForeColor = Color.White;
                    lblEstimatedSize.BackColor = Color.Green;
                    // lblEstimatedSize.BackColor = System.Drawing.SystemColors.Control;
                    // lblEstimatedSize.ForeColor = Color.Green;
                }
                else
                {
                    lblEstimatedSize.ForeColor = Color.White;
                    lblEstimatedSize.BackColor = Color.Red;
                    // lblEstimatedSize.BackColor = Color.Black;
                    // lblEstimatedSize.ForeColor = Color.Red;
                }
                return;
            }
            lblEstimatedSize.BackColor = System.Drawing.SystemColors.Control;
            lblEstimatedSize.ForeColor = System.Drawing.SystemColors.ControlText;
            if (partSize > 0)
            {
                lblEstimatedSize.Text += BytesUtils.GetSizeDescr(estimSize);
                statusStrip2.Update();
                return;
            }
        }


        #region BwComputeSize

        private void bwComputeSize_DoWork(object sender, DoWorkEventArgs e)
        {
            allFiles.Clear();
            iEstimatedSize = 0;
            bwComputeSize.ReportProgress(-1);
            DirectoryInfo dirInfo = new DirectoryInfo(TmpFiles);
            iEstimatedSize = ComputeEstimatedSize(dirInfo);
        }

        private void bwComputeSize_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (e.ProgressPercentage == -1)
                {
                    updateSizeLabelTimer.Start();
                }
                //            UInt64 val = (UInt64)e.UserState;
                //            GUI_SetComputedLabel(val, fwFile.PartitionSize, false);
            }
            catch (FuzzyByteException ex)
            {
                Notes.ShowWarning(ex.Message);
            }
        }


        private void bwComputeSize_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //throw new FwException("aaa");
            updateSizeLabelTimer.Stop();
            if (e.Cancelled)
            {
                iEstimatedSize = 0;
                allFiles.Clear();
            }
            GUI_SetComputedLabel(iEstimatedSize, fwFile.PartitionSize, true);
            iNeedsRecomputeSize = false;

            if (!bwDeleteTrashFolder.IsBusy)
                bwDeleteTrashFolder.RunWorkerAsync();
        }

        private UInt64 ComputeEstimatedSize(DirectoryInfo dirInfo)
        {
            UInt64 res = 0;
            foreach (FileInfo fInfo in dirInfo.GetFiles())
            {
                if (bwComputeSize.CancellationPending)
                    return 0;
                allFiles.Add(new FileElem(fInfo.FullName, fInfo.Length));
                res += (UInt64)fInfo.Length;
                iEstimatedSize += (UInt64)fInfo.Length;
            }
            foreach (DirectoryInfo subDirInfo in dirInfo.GetDirectories())
            {
                if (bwComputeSize.CancellationPending)
                    return 0;
                //                bwComputeSize.ReportProgress(50, iEstimatedSize);
                res += ComputeEstimatedSize(subDirInfo);
            }
            return res;
        }


        #endregion


        #region BwFilesOperation
        private void bwFilesOperation_DoWork(object sender, DoWorkEventArgs e)
        {
            bwFilesOperation.ReportProgress(-1);
            fileSystemWatcher1.EnableRaisingEvents = false;
            if (bwComputeSize.IsBusy)
                bwComputeSize.CancelAsync();
            FileCopyOperation copyOper = e.Argument as FileCopyOperation;
            if (copyOper != null)
            {
                AddFilesAndFolders(copyOper.iItems, copyOper.iDestPath);
            }
            FileDeleteOperation delOper = e.Argument as FileDeleteOperation;
            if (delOper != null)
            {
                // rimozione file ricorsiva, con aggiornamento di iEstimatedSize
                RemoveFilesAndFolders(delOper.iItems);
            }
        }

        private void bwFilesOperation_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (e.ProgressPercentage == -1)
                {
                    updateSizeLabelTimer.Start();
                    iNeedsRebuild = true;
                    GUI_FwChanged = true;
                    Cursor = Cursors.WaitCursor;
                    GUI_SetAllEnabled(false);
                    CancelBtnVisible = true;
                    return;
                }
                /*
                            if (e.ProgressPercentage == 50)
                            {
                                if (dlg == null)
                                {
                                    dlg = new DlgFilesOperation();
                                    DialogResult res = dlg.ShowDialog();
                                    if (res == DialogResult.Cancel)
                                        bwFilesOperation.CancelAsync();
                                }
                            }*/
                // Update Estimated Size
                // GUI_SetComputedLabel(iEstimatedSize, fwFile.PartitionSize, false);
            }
            catch (FuzzyByteException ex)
            {
                Notes.ShowWarning(ex.Message);
            }
        }

        private void RemoveFilesAndFolders(string[] elements)
        {
            foreach (string element in elements)
            {
                //if (dlg == null)
                //    bwFilesOperation.ReportProgress(50);
                if (bwFilesOperation.CancellationPending)
                    return;
                if (File.Exists(element))
                {
                    FileElem elem = allFiles.Find(FileElem.ByFileName(element));
                    if (elem != null)
                    {
                        iEstimatedSize -= (UInt64)elem.iSize;
                        allFiles.Remove(elem);
                    }
                    File.SetAttributes(element, FileAttributes.Normal);
                    File.Delete(element);
                }
                else
                    if (Directory.Exists(element))
                    {
                        string[] files = Directory.GetFiles(element);
                        string[] dirs = Directory.GetDirectories(element);
                        RemoveFilesAndFolders(files);
                        RemoveFilesAndFolders(dirs);
                        //                    bwFilesOperation.ReportProgress(50);
                        Directory.Delete(element, true);
                    }
            }
        }

        private void AddFilesAndFolders(string[] elements, string destPath)
        {
            foreach (string element in elements)
            {
                //if (dlg == null)
                //    bwFilesOperation.ReportProgress(50);
                if (bwFilesOperation.CancellationPending)
                    return;
                if (Directory.Exists(element))
                {
                    DirectoryInfo dirSrc = new DirectoryInfo(element);
                    Directory.CreateDirectory(destPath + dirSrc.Name);
                    string[] files = Directory.GetFiles(dirSrc.FullName);
                    string[] dirs = Directory.GetDirectories(dirSrc.FullName);
                    AddFilesAndFolders(files, destPath + dirSrc.Name + Path.DirectorySeparatorChar);
                    AddFilesAndFolders(dirs, destPath + dirSrc.Name + Path.DirectorySeparatorChar);
                    //                    bwFilesOperation.ReportProgress(50);
                }
                else
                    if (File.Exists(element))
                    {
                        string fname = destPath + Path.GetFileName(element);
                        File.Copy(element, fname, true);
                        FileInfo fi = new FileInfo(fname);
                        FileElem elem = allFiles.Find(FileElem.ByFileName(fname));
                        if (elem != null)
                        {
                            // File Overwrite
                            iEstimatedSize -= (UInt64)elem.iSize;
                            elem.iSize = fi.Length;
                            iEstimatedSize += (UInt64)elem.iSize;
                        }
                        else
                        {
                            // Create new File
                            elem = new FileElem(fname, fi.Length);
                            allFiles.Add(elem);
                            iEstimatedSize += (UInt64)elem.iSize;
                        }
                        // bwFilesOperation.ReportProgress(50);
                        // non posso notificarlo ad ogni file, perche' poi il thread GUI non ci sta dietro a dispatchare tutte le notifiche che arrivano.
                    }
            }
        }

        private void bwFilesOperation_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            /*if (dlg != null)
            {
                dlg.Close();
                dlg = null;
            }/**/
            CancelBtnVisible = false;
            if (e.Error != null)
            {
                if (e.Error is FuzzyByteException)
                {
                    Notes.ShowWarning(e.Error.Message);
                }
                else
                {
                    Notes.ShowError("Unexpected Error", e.Error.Message);
                }
            }

            updateSizeLabelTimer.Stop();
            treeViewAdv1.MonitorFileSystem = true;
            // TODO: dismiss copy / delete progress dialog.

            // use e.result to update iEstimatedSize and allfiles[]
            // non va bene perche' FileCopyOperation.Items contiene una directory, non l'elenco dei file

            GUI_PopulateFileListFromPath(GUI_FullTreeViewPath);
            GUI_SetComputedLabel(iEstimatedSize, fwFile.PartitionSize, true);
            // enable again the raising event
            fileSystemWatcher1.EnableRaisingEvents = true;
            Cursor = Cursors.Default;
            GUI_SetAllEnabled(true);
        }
        #endregion


        #region Unlock Dialog
        private Point GetPointForUnlockDlg()
        {
            /*            Rectangle rectBtn = this.RectangleToScreen(this.toolStrip1.bBounds);
                        int x = rectBtn.Left;
                        int y = rectBtn.Top; 
                        return new Point(x, y);/**/
            //+ rectBtn.Height;*/
            /*                        Rectangle rectBtn = btnUnlockRofs.Bounds;
                                    Point pointBtn = this.PointToScreen(rectBtn.Location);
                                    int x = pointBtn.X;
                                    int y = pointBtn.Y; //+ rectBtn.Height;*/
            /*            Rectangle rectBtn = btnUnlockRofs.Bounds;
                        Point pointBtn = this.PointToScreen(rectBtn.Location);
                        int x = pointBtn.X;
                        int y = pointBtn.Y; //+ rectBtn.Height;*/
            /*Point scr = this.PointToScreen(toolStrip2.Location);
            int x = MousePosition.X;
            int y = scr.Y + toolStrip2.Height;*/
            Point scr = PointToScreen(toolPlugins.Location);
            int x = MousePosition.X - 100;
            int y = scr.Y + 10;
            return new Point(x, y);/**/
        }


        private void btnUnlockRofs_Click(object sender, EventArgs e)
        {
            if (fwFile.FwType != TFwType.CORE)
                return;
            TSectionArea sectArea = fwFile.GetSectionArea();
            List<TSectionInfo> rofsSections = sectArea.GetRofsSections();

            List<string> descrList = new List<string>();
            List<byte> valueList = new List<byte>();

            foreach (TSectionInfo section in rofsSections)
            {
                byte value=0;
                if (section.GetProtValue(out value))
                {
                    string s = BytesUtils.ToString(section.descr).ToUpper();
                    descrList.Add(s);
                    valueList.Add(value);
                }
            }
            UnlockRofsDialog unlockDlg = new UnlockRofsDialog(valueList);
            
            // Sposta Dialog
            unlockDlg.StartPosition = FormStartPosition.Manual;
            Size oldSize = unlockDlg.ClientSize;
            unlockDlg.Location = GetPointForUnlockDlg();
            // Necessario perche' altrimenti si perde la dimensione del dialog nel caso usi font di differente DPI.
            unlockDlg.ClientSize = oldSize;

            DialogResult res = unlockDlg.ShowDialog(this);
            if (res != DialogResult.OK)
                return;

            if (!unlockDlg.Changed)
                return;

            GUI_FwChanged = true;

            int i=0;
            foreach (byte aValue in valueList)
            {
                rofsSections[i].SetProtValue(aValue);
                i++;
            }

            TBlock block = fwFile.blocksColl.FindSectionAreaBlock();            
            sectArea.WriteToSectionAreaBlock(block);
/*            FwFile.TSectionInfo rofs1 = null;
            FwFile.TSectionInfo rofs2 = null;
            FwFile.TSectionInfo rofs3 = null;
            int rofs1Val = -1;
            int rofs2Val = -1;
            int rofs3Val = -1;
            foreach (FwFile.TSectionInfo info in sections)
            {
                byte value = 0;
                if (info.GetProtValue("ROFS1", out value))
                {
                    rofs1Val = value;
                    rofs1 = info;
                }
                if (info.GetProtValue("ROFS2", out value))
                {
                    rofs2Val = value;
                    rofs2 = info;
                }
                if (info.GetProtValue("ROFS3", out value))
                {
                    rofs3Val = value;
                    rofs3 = info;
                }
            }

            UnlockRofsDialog unlockDlg = new UnlockRofsDialog(rofs1Val, rofs2Val, rofs3Val);

            // Sposta Dialog
            unlockDlg.StartPosition = FormStartPosition.Manual;
            Size oldSize = unlockDlg.ClientSize;
            unlockDlg.Location = GetPointForUnlockDlg();
            // Necessario perche' altrimenti si perde la dimensione del dialog nel caso usi font di differente DPI.
            unlockDlg.ClientSize = oldSize;

            DialogResult res = unlockDlg.ShowDialog(this);
            if (res != DialogResult.OK)
                return;

            if (unlockDlg.Rofs1Changed && rofs1 != null)
            {
                GUI_FwChanged = true;
                rofs1.SetProtValue("ROFS1", unlockDlg.Rofs1Val);
            }
            if (unlockDlg.Rofs2Changed && rofs2 != null)
            {
                GUI_FwChanged = true;
                rofs2.SetProtValue("ROFS2", unlockDlg.Rofs2Val);
            }
            if (unlockDlg.Rofs3Changed && rofs3 != null)
            {
                GUI_FwChanged = true;
                rofs3.SetProtValue("ROFS3", unlockDlg.Rofs3Val);
            }

            fwFile.UpdateSectionBlock(sections);*/
        }
        #endregion


        private void updateSizeLabelTimer_Tick(object sender, EventArgs e)
        {
            GUI_SetComputedLabel(iEstimatedSize, fwFile.PartitionSize, false);
        }

        private void gridFiles1_DoubleClick(object sender, EventArgs e)
        {
            GUI_ListView_EnterFolderOrLaunchFile();
        }

        private void gridFiles1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
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

        private void gridFiles1_DragEnter(object sender, DragEventArgs e)
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

        private void gridFiles1_DragDrop(object sender, DragEventArgs e)
        {
            Debug.WriteLine("List DragDrop");
            if (treeViewAdv1.Nodes.Count == 0)
                return;
            string[] items = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (items.Length <= 0)
                return;

            Cursor = Cursors.WaitCursor;
            iNeedsRebuild = true;
            GUI_FwChanged = true;
            GUI_SetAllEnabled(false);

            string destPath = GUI_FullTreeViewPath;

            FileOperation oper = new FileCopyOperation(items, destPath);
            if (!bwFilesOperation.IsBusy)
                bwFilesOperation.RunWorkerAsync(oper);
        }

        private void btnExplore_Click(object sender, EventArgs e)
        {
            //throw new FuzzyByteException("AAAA");

            Process.Start(GUI_FullTreeViewPath);
        }

        private void bwDeleteTrashFolder_DoWork(object sender, DoWorkEventArgs e)
        {
            Dirs.DeleteContent(TrashFiles, bwDeleteTrashFolder);
        }

        private void bwDeleteTrashFolder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void bwDeleteTrashFolder_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void removeFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnDelete.PerformClick();
        }

        private void exploreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fullElemPath = GUI_FocusedFullListViewPath;
            if (Directory.Exists(fullElemPath))
            {
                Process.Start(fullElemPath);
            }
            else
            {
                btnExplore.PerformClick();
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridFiles1.SelectAll();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            gridFiles1.Focus();
        }

        private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            treeViewAdv1.Focus();
            btnDelete.PerformClick();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            treeViewAdv1.Focus();
            btnExplore.PerformClick();
        }

        private void btnRofs1Increase_Click(object sender, EventArgs e)
        {
            UInt32 gainedSpace = fwFile.ExtendRofs1();
            if (gainedSpace == 0)
            {
                Notes.ShowInfo("ROFS1 has already been extended to the maximum size!");
                return;
            }

            string newSizeDescr = BytesUtils.GetSizeDescr(fwFile.PartitionSize);
            lblPartSize.Text = "Partition Size: " + newSizeDescr;
            GUI_SetComputedLabel(iEstimatedSize, fwFile.PartitionSize, true);

            GUI_FwChanged = true;
            Notes.ShowInfo("ROFS1 has been extended to: " + newSizeDescr + "\n" + "+" + BytesUtils.GetSizeDescr(gainedSpace));            
        }

        private void GUI_ToolBarChangeSize(bool big)
        {
            Size size = new Size(16, 16);
            if (big)
                size = new Size(32, 32);
            toolbar.ImageScalingSize = size;
            toolbar.Width = this.Width;
            // Necessario perche' l'immagine non e' quadrata di 32x32, quindi non puo' scalare con ImageScalingSize
            if (big)
                btnDonate.Image = Properties.Resources.Donate32;
            else
                btnDonate.Image = Properties.Resources.Donate16;
        }

        private void GUI_ToolBarEnableText(bool enabled)
        {
            ToolStripItemDisplayStyle style = ToolStripItemDisplayStyle.Image;
            if (enabled)
                style = ToolStripItemDisplayStyle.ImageAndText;
            foreach (ToolStripItem aItem in toolbar.Items)
            {
                aItem.DisplayStyle = style;
            }
        }

        private void showLargeIconsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showLargeIconsToolStripMenuItem.Checked = !showLargeIconsToolStripMenuItem.Checked;
            // Update Config
            Properties.Settings.Default.ToolBar_Big = showLargeIconsToolStripMenuItem.Checked;
            Properties.Settings.Default.Save();

            GUI_ToolBarChangeSize(showLargeIconsToolStripMenuItem.Checked);
        }

        private void showTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showTextToolStripMenuItem.Checked = !showTextToolStripMenuItem.Checked;
            // Update Config
            Properties.Settings.Default.ToolBar_Text = showTextToolStripMenuItem.Checked;
            Properties.Settings.Default.Save();

            GUI_ToolBarEnableText(showTextToolStripMenuItem.Checked);
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            menuitemPluginUseBigIcons.Checked = !menuitemPluginUseBigIcons.Checked;
            // Update Config
            Properties.Settings.Default.ToolPlugin_Big = menuitemPluginUseBigIcons.Checked;
            Properties.Settings.Default.Save();
            // Update ToolBar
            GUI_UpdatePluginsToolbar();
        }


        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            menuitemPluginShowText.Checked = !menuitemPluginShowText.Checked;
            // Update Config
            Properties.Settings.Default.ToolPlugin_Text = menuitemPluginShowText.Checked;
            Properties.Settings.Default.Save();
            // Update ToolBar
            GUI_UpdatePluginsToolbar();
        }

        private void expandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeViewAdv1.SelectedNode == null)
                return;
            treeViewAdv1.SelectedNode.ExpandAll();
        }

        private void collapseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeViewAdv1.SelectedNode.Collapse();
            if (treeViewAdv1.Nodes.Count > 0)
                treeViewAdv1.Nodes[0].Expand();
            GUI_PopulateFileListFromPath(GUI_FullTreeViewPath);
        }

        private void toolStripLabel2_Click(object sender, EventArgs e)
        {

        }

        private void removePluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!GUI_CloseFwFile())
            {
                e.Cancel = true;
            }
        }

        private void extractRomMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            TGroup aGroup = fwFile.blocksColl.GetGroupMatchingDescrition("CORE");
            if (aGroup == null)
            {
                Notes.ShowWarning("ROM data not found! Please contact: m.bellino@symbian-toys.com");
                return;
            }
            string filename = saveFileDialog1.FileName;
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            BufferedStream bs = new BufferedStream(fs);
            foreach (TBlock aBlock in aGroup.codeBlocks)
            {
                if (fs.Position == 0)
                {
                    byte[] newArray = new byte[aBlock.content.Length - 0xC00];
                    Array.Copy(aBlock.content, 0xC00, newArray, 0, newArray.Length);
                    fs.Write(newArray, 0, newArray.Length);
                    continue;
                }
                fs.Write(aBlock.content, 0, aBlock.content.Length);
            }
            bs.Close();
            fs.Close();
            Notes.ShowInfo("ROM successfully saved!\nUse ReadImage, or aRomAT by CODeRUS to open the ROM file.");
        }

/*        private void LaunchBrowser(string url)
        {
            if (!HavePermission(new PermissionSet(PermissionState.Unrestricted)))
                return;
            try
            {
                //System.Diagnostics.Process.Start("iexplore.exe", "http://" + url);                
                System.Diagnostics.Process.Start("http://" + url);
            }
            catch (Exception)
            {
            }
        }*/

        private void toolStripStatusLabel10_Click(object sender, EventArgs e)
        {
            LaunchBrowser("www.guardian-mobile.com/");
        }

        private void btnRepartition_Click(object sender, EventArgs e)
        {
            if (fwFile.filename != "")
            {
                Notes.ShowWarning("Close the current Firmware file first!");
                return;
            }

            // SHIFT KEY is being held down
            RepartitionDialog dlg = new RepartitionDialog(TmpImages + FAT_IMG);
            dlg.Log += new LogHandler(fwFile_Log);
            // Sposta Dialog
            dlg.StartPosition = FormStartPosition.Manual;
            Size oldSize = dlg.ClientSize;
            dlg.Location = GetPointForUnlockDlg();
            // Necessario perche' altrimenti si perde la dimensione del dialog nel caso usi font di differente DPI.
            dlg.ClientSize = oldSize;
            dlg.ShowDialog();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // Initialize ToolBars size and Text    
            GUI_UpdatePluginsToolbar();
            if (!bwDeleteTrashFolder.IsBusy)
                bwDeleteTrashFolder.RunWorkerAsync();

            GUI_SetAllEnabled(true);
            GUI_SetToolbarEnabled(false);

            recent = new RecentList("Symbian-Toys", "NokiaCooker");
            GUI_UpdateRecentList();
            GUI_ToolBarEnableText(Properties.Settings.Default.ToolBar_Text);
            GUI_ToolBarChangeSize(Properties.Settings.Default.ToolBar_Big);

            /*DateTime now = DateTime.Now;
            if (now.Year >= 2014 && now.Month >= 8)
            {
                Notes.ShowWarning("This version is too much old!\nPlease download the new version from the official website\nhttp://www.symbian-toys.com/");
                Application.Exit();
            }/**/

            bool explorerRunAsAdmin = Processes.IsProcessOwnerAdmin("explorer");
            if (IsUserAdministrator() && !explorerRunAsAdmin)
            {
                Notes.ShowInfo("NokiaCooker is running as Administrator:\nthe Drag&Drop will not work properly!\n\nYou should run NokiaCooker as regular user.");
                //Application.Exit();
            }

            ProcessNokiaCookerArgs();
        }

        private void ProcessNokiaCookerArgs()
        {
            if (iFileArg.Length <= 0)
                return;

            if (!btnOpen.Enabled)
                return;

            if (iFileArg.ToLower().EndsWith(".vpl"))
                {
                    if (!GUI_CloseFwFile())
                        return;
                    RepartitionDialog dlg = new RepartitionDialog(TmpImages + FAT_IMG, iFileArg);
                    dlg.Log += new LogHandler(fwFile_Log);
                    // Sposta Dialog
                    dlg.StartPosition = FormStartPosition.Manual;
                    Size oldSize = dlg.ClientSize;
                    dlg.Location = GetPointForUnlockDlg();
                    // Necessario perche' altrimenti si perde la dimensione del dialog nel caso usi font di differente DPI.
                    dlg.ClientSize = oldSize;
                    dlg.ShowDialog();
                }
                else
                {
                    GUI_OpenFirmwareFile(iFileArg);
                }
            
        }
    }


    class FileElem
    {
        public static Predicate<FileElem> ByFileName(string fileName)
        {
            return delegate(FileElem aElem)
            {
                return aElem.iFileName.ToLower() == fileName.ToLower();
            };
        }

        public string iFileName;
        public Int64 iSize;

        public FileElem(string aFname, Int64 aSize)
        {
            iFileName = aFname;
            iSize = aSize;
        }
    }
}
