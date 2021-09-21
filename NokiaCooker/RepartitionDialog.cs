using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using FuzzyByte.Utils;
using Firmware;
using System.Diagnostics;
using FuzzyByte.Forms;
using System.IO;


namespace FuzzyByte.NokiaCooker
{
    public partial class RepartitionDialog : Form
    {
        private List<FwFile> fwFiles = new List<FwFile>();
        private TSizes oriSizes = new TSizes();
        private TSizes minSizes = new TSizes();
        private string fatImageFilename = "";
        private bool altPressed = false;
        private string vplFile = "";

        public event LogHandler Log;
/*        {
            add { fwFile.Log += value; }
            remove { fwFile.Log -= value; }            
        }*/

        protected void LogEvent(string message)
        {
            LogEvent(message, EventType.Info);
        }

        protected void LogEvent(string message, EventType type)
        {            
            if (Log != null)
            {
                Log(message, type);
            }
        }

        [DllImport("uxtheme", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public extern static Int32 SetWindowTheme(IntPtr hWnd,
                      String textSubAppName, String textSubIdList);

        protected override void OnHandleCreated(EventArgs e)
        {
            SetWindowTheme(this.Handle, "", "");
            base.OnHandleCreated(e);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCLBUTTONDOWN = 161;
            const int WM_SYSCOMMAND = 274;
            const int HTCAPTION = 2;
            const int SC_MOVE = 61456;

            if ((m.Msg == WM_SYSCOMMAND) && (m.WParam.ToInt32() == SC_MOVE))
            {
                return;
            }

            if ((m.Msg == WM_NCLBUTTONDOWN) && (m.WParam.ToInt32() == HTCAPTION))
            {
                return;
            }

            base.WndProc(ref m);
        } 


        public RepartitionDialog(string img, string aVplFile)
        {
            InitializeComponent();

            vplFile = aVplFile;
            fatImageFilename = img;
            altPressed = Control.ModifierKeys == Keys.Alt;
            if (altPressed)
            {
#if DEBUG
                panel1.Visible = true;
                this.Width = 720;
                this.Height = 350;
#endif
            }
        }

        public RepartitionDialog(string img)
        {
            InitializeComponent();

            vplFile = "";
            fatImageFilename = img;
            altPressed = Control.ModifierKeys == Keys.Alt;
            if (altPressed)
            {
#if DEBUG
                panel1.Visible = true;
                this.Width = 720;
                this.Height = 350;
#endif
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TSizes newVal = new TSizes();
            newVal.rofs1 = (UInt32)(numRofs1.Value);
            newVal.rofs2 = (UInt32)(numRofs2.Value);
            newVal.rofs3 = (UInt32)(numRofs3.Value);
            newVal.uda = (UInt32)(numUda.Value);

            if ((newVal.rofs1 == oriSizes.rofs1) &&
                (newVal.rofs2 == oriSizes.rofs2) &&
                (newVal.rofs3 == oriSizes.rofs3) &&
                (newVal.uda == oriSizes.uda))
            {
                Notes.ShowInfo("Nothing to do!");
                return;
            }

            if (newVal.uda - oriSizes.uda != 0)
            {
                if (Notes.ShowQuery("UDA will be erased, all UDA content will be lost.\nProceed?", MessageBoxButtons.OKCancel) != DialogResult.OK)
                    return;
            }

            if (newVal.Total != oriSizes.Total)
                throw new FwException("End Sizes are wrong, please contact: m.bellino@symbian-toys.com");
            if (!bwResize.IsBusy)
                bwResize.RunWorkerAsync(newVal);            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void RepartitionDialog_Load(object sender, EventArgs e)
        {            
            lblSpace.Text = "";
        }

        private void RepartitionDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (FwFile aFwFile in fwFiles)
                aFwFile.Close();
        }

        private void RepartitionDialog_Shown(object sender, EventArgs e)
        {
            if (vplFile != "")
            {
                if (!bwOpenFile.IsBusy)
                    bwOpenFile.RunWorkerAsync(vplFile.Trim().ToLower());
                return;
            }
            if (openFileDialogVpl.ShowDialog() != DialogResult.OK)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            if (!bwOpenFile.IsBusy)
                bwOpenFile.RunWorkerAsync(openFileDialogVpl.FileName);
        }


        private void GUI_SetAllEnabled(bool state)
        {
            this.Enabled = state;
            if (state)
                Cursor = Cursors.Default;
            else
                Cursor = Cursors.WaitCursor;
        }


        private void bwOpenFile_DoWork(object sender, DoWorkEventArgs e)
        {            
            string fullFilename = e.Argument as string;
            bwOpenFile.ReportProgress(-1);
            List<VplEntry> files = VplFile.GetAllEntries(fullFilename);

            string path = System.IO.Path.GetDirectoryName(fullFilename) + System.IO.Path.DirectorySeparatorChar;

            // Loads the core file found in the vpl
            FwFile coreFile = null;
            foreach (VplEntry entry in files)
            {
                if (entry.fileType != TFileType.MCU)
                    continue;

                if (!System.IO.File.Exists(path + entry.filename))
                    continue;

                coreFile = new FwFile();
                coreFile.Log += Log;
                coreFile.Open(path + entry.filename, true);
                coreFile.Read();
            }

            if (coreFile == null)
                throw new FwException("CORE File Not Loaded!");

            // Extends ROFS1
            UInt32 gained = coreFile.ExtendRofs1();
            string gainDescr = BytesUtils.GetSizeDescr(gained);
            if (gained > 0)
                LogEvent("Extended ROFS1 and gained " + gainDescr + "\n");

            // Checks the Sections and Retrieves the Sections Sizes
            TSectionArea sectArea = coreFile.GetSectionArea();
            List<TSectionInfo> rofsSections = sectArea.GetRofsSections();
            TSectionInfo userSection = sectArea.GetUserSection();

            if (rofsSections.Count < 3)
                throw new FwException("This fw is not supported yet, please contact: m.bellino@symbian-toys.com");
            if (userSection == null)
                throw new FwException("UserSection is missing, please contact: m.bellino@symbian-toys.com");

            /*
            foreach (TSectionInfo sect in rofsSections)
            {
                if (sect.length < 1024 * 1024)
                    throw new FwException("ROFS Partitions < 1Mb are not allowed");
            }
            if (userSection.length < 1024 * 1024)
                throw new FwException("UDA Partitions < 1Mb are not allowed");
            */

            // Loads ROFS2, ROFS3 and UDA...
            FwFile udaFile = null;
            FwFile rofs2File = null;
            FwFile rofs3File = null;
            foreach (VplEntry entry in files)
            {
                if (entry.fileType == TFileType.MCU)
                    continue;

                if (entry.fileType == TFileType.OTHER)
                    continue;

                if (!System.IO.File.Exists(path + entry.filename))
                    continue;

                FwFile fwFile = new FwFile();
                fwFile.Log += Log;
                fwFile.Open(path + entry.filename);
                fwFile.Read();

                switch (fwFile.FwType)
                {
                    case TFwType.UNKNOWN:
                        {
                            fwFile.Close();
                            continue;
                        }
                    case TFwType.ROFS:
                    case TFwType.ROFX:
                        {
                            if (rofs2File == null)
                            {
                                rofs2File = fwFile;
                            }
                            else
                                if (rofs3File == null)
                                {
                                    rofs3File = fwFile;
                                    UInt32 firstLocA = (rofs2File.blocksColl.GetRofsOrUdaGroupForFwType(fwFile.FwType).FirstLocation);
                                    UInt32 firstLocB = (rofs3File.blocksColl.GetRofsOrUdaGroupForFwType(fwFile.FwType).FirstLocation);
                                    if (firstLocB < firstLocA)
                                    {
                                        // swap ROFS2 <-> ROFS3
                                        FwFile tmp = rofs2File;
                                        rofs2File = rofs3File;
                                        rofs3File = tmp;
                                    }
                                }
                            break;
                        }
                        // TODO: Lock dopo tot tempo? Comunicazione con il server?
                    case TFwType.UDA:
                        {
                            TGroup group = fwFile.blocksColl.GetRofsOrUdaGroupForFwType(fwFile.FwType);
                            // Detect if it is the UDA...
                            TLV_Partition_Info_BB5 parts_bb5 = coreFile.header.GetPartitionArea();
                            TPartition part = parts_bb5.GetPartitionForAddr(group.FirstLocation);
                            if (part != null)
                                udaFile = fwFile;
                            break;
                        }
                    default:
                        break;
                }
            }
            if (rofs2File == null || rofs3File == null)
                throw new FwException("ROFS2 or ROFS3 fw file is Missing!");
            if (udaFile == null)
                throw new FwException("The UDA fw file is Missing!");

            // Retrieve the sizes          
            oriSizes.rofs1 = rofsSections[0].length;
            oriSizes.rofs2 = rofsSections[1].length;
            oriSizes.rofs3 = rofsSections[2].length;
            oriSizes.uda = userSection.length;

            if (oriSizes.rofs1 != coreFile.PartitionSize)
                throw new FwException("Wrong data in CORE partition, Please Contact m.bellino@symbian-toys.com");

            if (oriSizes.rofs2 != rofs2File.PartitionSize + 1)
                throw new FwException("Wrong data in ROFS2 partition, Please Contact m.bellino@symbian-toys.com");

            if (oriSizes.rofs3 != rofs3File.PartitionSize + 1)
                throw new FwException("Wrong data in ROFS3 partition, Please Contact m.bellino@symbian-toys.com");

            if (oriSizes.uda != udaFile.PartitionSize + 1)
                throw new FwException("Wrong data in UDA partition, Please Contact m.bellino@symbian-toys.com");

            /*
            // Arrotonda le dim. minime a 1Mb per eccesso poi aggiunge il resto... forse, cosi' facendo puo' capitare che sfora
            minSizes.rofs1 = RoundToMb((UInt32)(coreFile.ContentLength)) + GetMod(oriSizes.rofs1);
            minSizes.rofs2 = RoundToMb((UInt32)(rofs2File.ContentLength)) + GetMod(oriSizes.rofs2);
            minSizes.rofs3 = RoundToMb((UInt32)(rofs3File.ContentLength)) + GetMod(oriSizes.rofs3);
            minSizes.uda = RoundToMb((UInt32)(udaFile.ContentLength)) + GetMod(oriSizes.uda);*/
            
            minSizes.rofs1 = ComputeMin(coreFile.ContentLength, oriSizes.rofs1);
            minSizes.rofs2 = ComputeMin(rofs2File.ContentLength, oriSizes.rofs2);
            minSizes.rofs3 = ComputeMin(rofs3File.ContentLength, oriSizes.rofs3);
            minSizes.uda = ComputeMin(udaFile.ContentLength, oriSizes.uda);

            if ((minSizes.rofs1 > oriSizes.rofs1) ||
                (minSizes.rofs2 > oriSizes.rofs2) ||
                (minSizes.rofs3 > oriSizes.rofs3) ||
                (minSizes.uda > oriSizes.uda))
                throw new FwException("Min Sizes exceeds current size, Please Contact m.bellino@symbian-toys.com");

            // Adds the Core at the end, so it will be processed after all other files.
            fwFiles.Add(udaFile);
            fwFiles.Add(rofs2File);
            fwFiles.Add(rofs3File);
            fwFiles.Add(coreFile);
        }

        private UInt32 ComputeMin(double contentLen, UInt32 currSize)
        {
            UInt32 intLen = (UInt32)contentLen;
            // 0x00f05000		0x00106000		    0x00205000
            // 0x00f05000		0x00006000		    0x00105000
            // 0x00f05000		0x00000e00		    0x5000
            // 0x00f05000       0x00d00e00          0x00d05000

            const UInt32 UNIT = 0x100000;
            UInt32 mod = (UInt32)(currSize % UNIT);
            UInt32 div = (UInt32)(contentLen / UNIT);
            UInt32 res = mod + div * UNIT;
            if (res < contentLen)
                res += UNIT;
            return res;
        }

        private UInt32 GetMod(double bytes)
        {
            UInt32 res = (UInt32) (bytes % 0x100000);
            return res;
        }

        private UInt32 RoundToValue(double bytes, UInt32 value)
        {
            UInt32 half_value = value / 2 - 1;
            UInt32 mbInt = (UInt32)Math.Round((bytes + half_value) / value);
            return mbInt * value;
        }

        private UInt32 RoundToMb(double bytes)
        {
            UInt32 mbInt = (UInt32)Math.Round((bytes + 0x7FFFF) / 0x100000);
            return mbInt * 0x100000;
        }

        private void bwOpenFile_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
            {
                GUI_SetAllEnabled(false);
                return;
            }
        }

        private void bwOpenFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            GUI_SetAllEnabled(true);

            //m_TotSpace = (UInt32)(rofsSections[0].length / 1024 / 1024 + rofsSections[1].length / 1024 / 1024 + rofsSections[2].length / 1024 / 1024 + userSection.length / 1024 / 1024);
            lblCap.Text = oriSizes.TotalMb + " Mb";

            // NumericUpDown Min
            numRofs1.Minimum = minSizes.rofs1; 
            numRofs2.Minimum = minSizes.rofs2; 
            numRofs3.Minimum = minSizes.rofs3; 
            numUda.Minimum = minSizes.uda;

            // NumericUpDown Max
            numRofs1.Maximum = oriSizes.Total;
            numRofs2.Maximum = oriSizes.Total;
            numRofs3.Maximum = oriSizes.Total;
            numUda.Maximum = oriSizes.Total;

            // NumericUpDown Value
            numRofs1.Value = oriSizes.rofs1; 
            numRofs2.Value = oriSizes.rofs2; 
            numRofs3.Value = oriSizes.rofs3; 
            numUda.Value = oriSizes.uda;

            // Total size
            lblCap.Text = BytesUtils.GetSizeDescr(oriSizes.Total);

            // Mb Decription
            label5.Text = BytesUtils.GetSizeDescr((UInt32)numRofs1.Value);
            label6.Text = BytesUtils.GetSizeDescr((UInt32)numRofs2.Value);
            label7.Text = BytesUtils.GetSizeDescr((UInt32)numRofs3.Value);
            label8.Text = BytesUtils.GetSizeDescr((UInt32)numUda.Value);

            // Min. Size Text and Description
            label15.Text = BytesUtils.GetSizeDescr(minSizes.rofs1);
            label14.Text = BytesUtils.GetSizeDescr(minSizes.rofs2);
            label13.Text = BytesUtils.GetSizeDescr(minSizes.rofs3);
            label12.Text = BytesUtils.GetSizeDescr(minSizes.uda);

            string udaLocked = "";
            numUda.Enabled = fwFiles[3].CanResizeUda();
            if (!numUda.Enabled)
                udaLocked = " - Locked - UDA can't be resized yet for this Fw.";

            LogEvent("Current Partitions.\n");
            LogEvent("ROFS1: " + BytesUtils.GetSizeDescr(oriSizes.rofs1) + "\n");
            LogEvent("ROFS2: " + BytesUtils.GetSizeDescr(oriSizes.rofs2) + "\n");
            LogEvent("ROFS3: " + BytesUtils.GetSizeDescr(oriSizes.rofs3) + "\n");
            LogEvent("UDA: " + BytesUtils.GetSizeDescr(oriSizes.uda) + udaLocked + "\n");

            if (altPressed)
            {
                numUda.Enabled = true;
            }

            if (panel1.Visible)
            {
                TGroup udaGroup = fwFiles[0].blocksColl.GetRofsOrUdaGroupForFwType(TFwType.UDA);
                TGroup rofs2Group = fwFiles[1].blocksColl.GetRofsOrUdaGroupForFwType(TFwType.ROFS);
                TGroup rofs3Group = fwFiles[2].blocksColl.GetRofsOrUdaGroupForFwType(TFwType.ROFS);
                TGroup rofs1Group = fwFiles[3].blocksColl.GetRofsOrUdaGroupForFwType(TFwType.CORE);
                TSectionArea sectAreaInCore = fwFiles[3].GetSectionArea();
                List<TSectionInfo> rofsSections = sectAreaInCore.GetRofsSections();
                TSectionInfo userSection = sectAreaInCore.GetUserSection();

#if DEBUG
                // Partitions
                TLV_Partition_Info_BB5 part = fwFiles[3].header.GetPartitionArea();
                label31.Text = part.ToString();
#endif

                // Erase Area Start End
                UInt32 loLimit;
                UInt32 hiLimit;
                fwFiles[3].header.GetEraseLimitsForAddr(rofs1Group.FirstLocation, out loLimit, out hiLimit);
                label18.Text = BytesUtils.ToHex(loLimit) + " - " + BytesUtils.ToHex(hiLimit);

                fwFiles[1].header.GetEraseLimitsForAddr(rofs2Group.FirstLocation, out loLimit, out hiLimit);
                label19.Text = BytesUtils.ToHex(loLimit) + " - " + BytesUtils.ToHex(hiLimit);

                fwFiles[2].header.GetEraseLimitsForAddr(rofs3Group.FirstLocation, out loLimit, out hiLimit);
                label20.Text = BytesUtils.ToHex(loLimit) + " - " + BytesUtils.ToHex(hiLimit);

                fwFiles[0].header.GetEraseLimitsForAddr(udaGroup.FirstLocation, out loLimit, out hiLimit);
                label21.Text = BytesUtils.ToHex(loLimit) + " - " + BytesUtils.ToHex(hiLimit);

                // Sections Start End
                label25.Text = BytesUtils.ToHex(rofsSections[0].startAddress) + " - " + BytesUtils.ToHex(rofsSections[0].EndAddress);
                label24.Text = BytesUtils.ToHex(rofsSections[1].startAddress) + " - " + BytesUtils.ToHex(rofsSections[1].EndAddress);
                label23.Text = BytesUtils.ToHex(rofsSections[2].startAddress) + " - " + BytesUtils.ToHex(rofsSections[2].EndAddress);
                label22.Text = BytesUtils.ToHex(userSection.startAddress) + " - " + BytesUtils.ToHex(userSection.EndAddress);

                // Blocks Start End
                label29.Text = BytesUtils.ToHex(rofs1Group.FirstLocation) + " - " + BytesUtils.ToHex(rofs1Group.LastLocation);
                label28.Text = BytesUtils.ToHex(rofs2Group.FirstLocation) + " - " + BytesUtils.ToHex(rofs2Group.LastLocation);
                label27.Text = BytesUtils.ToHex(rofs3Group.FirstLocation) + " - " + BytesUtils.ToHex(rofs3Group.LastLocation);
                label26.Text = BytesUtils.ToHex(udaGroup.FirstLocation) + " - " + BytesUtils.ToHex(udaGroup.LastLocation);
            }
        }


        private void NumValueChanged(object sender, EventArgs e)
        {
            // Mb Decription
            label5.Text = BytesUtils.GetSizeDescr((UInt32)numRofs1.Value);
            label6.Text = BytesUtils.GetSizeDescr((UInt32)numRofs2.Value);
            label7.Text = BytesUtils.GetSizeDescr((UInt32)numRofs3.Value);
            label8.Text = BytesUtils.GetSizeDescr((UInt32)numUda.Value);

            UInt32 newTotLen = (UInt32)(numRofs1.Value + numRofs2.Value + numRofs3.Value + numUda.Value);
            Int32 diff = (Int32)(newTotLen - oriSizes.Total);
            if (diff == 0)
                lblSpace.Text = "";
            if (diff > 0)
                lblSpace.Text = "Exceed Space: " + (diff / 1024 / 1024) + " Mb";
            if (diff < 0)
                lblSpace.Text = "Unallocated Space Left: " + (-diff / 1024 / 1024) + " Mb";
            btnResize.Enabled = (diff == 0);
        }

        private void numRofs1_ValueChanged(object sender, EventArgs e)
        {
            NumValueChanged(sender, e);
        }

        private void numRofs2_ValueChanged(object sender, EventArgs e)
        {
            NumValueChanged(sender, e);
        }

        private void numRofs3_ValueChanged(object sender, EventArgs e)
        {
            NumValueChanged(sender, e);
        }

        private void numUda_ValueChanged(object sender, EventArgs e)
        {
            NumValueChanged(sender, e);
        }


        private void bwResize_DoWork(object sender, DoWorkEventArgs e)
        {
            TSizes newSizes = e.Argument as TSizes;
            bwResize.ReportProgress(-1);

            // CORE will be processed after all the others.
            Int32 diffRofs1 = (Int32)(newSizes.rofs1 - oriSizes.rofs1);
            Int32 diffRofs2 = (Int32)(newSizes.rofs2 - oriSizes.rofs2);
            Int32 diffRofs3 = (Int32)(newSizes.rofs3 - oriSizes.rofs3);
            Int32 diffUda = (Int32)(newSizes.uda - oriSizes.uda);
              
            if (diffRofs1 + diffRofs2 + diffRofs3 + diffUda != 0)
                throw new FwException("Something wrong in computing new Size, Please Contact m.bellino@symbian-toys.com");

            if (diffUda != 0)
            {
                fwFiles[0].Repartition(diffRofs1 + diffRofs2 + diffRofs3, 0);                           // UDA
                fwFiles[0].ExtractAlignedCodeToFile(fatImageFilename);

                Fat16Image fat = new Fat16Image();
                fat.Log += this.Log;
                try
                {
                    fat.OpenImage(fatImageFilename);
                    fat.ResizeFat(newSizes.uda);
                }
                finally
                {
                    fat.CloseImage();
                }
            }
            fwFiles[1].Repartition(diffRofs1, diffRofs1 + diffRofs2);                                   // ROFS2
            fwFiles[2].Repartition(diffRofs1 + diffRofs2, diffRofs1+diffRofs2+diffRofs3);               // ROFS3
            fwFiles[3].RepartitionCore(newSizes.rofs1, newSizes.rofs2, newSizes.rofs3, newSizes.uda);   // CORE

            // Save Firmware files...
            if (diffUda != 0)
                fwFiles[0].Repack(fatImageFilename);
//            else
//                fwFiles[0].DumpToFile();
            fwFiles[1].DumpToFile();
            fwFiles[2].DumpToFile();
            fwFiles[3].DumpToFile();

            foreach (FwFile aFwFile in fwFiles)
            {
                aFwFile.Close();
            }

            LogEvent("Firmware Repartitioned!\n");
            LogEvent("ROFS1: " + BytesUtils.GetSizeDescr(newSizes.rofs1) + "\n");
            LogEvent("ROFS2: " + BytesUtils.GetSizeDescr(newSizes.rofs2) + "\n");
            LogEvent("ROFS3: " + BytesUtils.GetSizeDescr(newSizes.rofs3) + "\n");
            LogEvent("UDA: " + BytesUtils.GetSizeDescr(newSizes.uda) + "\n");
        }

        private void bwResize_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == -1)
            {
                GUI_SetAllEnabled(false);
                return;
            }
        }

        private void bwResize_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
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
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }
            GUI_SetAllEnabled(true);
            DialogResult = DialogResult.OK;
            Close();
        }

    }

    class TSizes
    {
        public UInt32 rofs1;
        public UInt32 rofs2;
        public UInt32 rofs3;
        public UInt32 uda;

        public UInt64 Total
        {
            get
            {
                return rofs1 + rofs2 + rofs3 + uda;
            }
        }

        public UInt32 TotalMb
        {
            get
            {
                return rofs1Mb + rofs2Mb + rofs3Mb + udaMb;
            }
        }

        public UInt32 rofs1Mb
        {
            get
            {
                return rofs1 / 1024 / 1024;
            }
        }

        public UInt32 rofs2Mb
        {
            get
            {
                return rofs2 / 1024 / 1024;
            }
        }
        public UInt32 rofs3Mb
        {
            get
            {
                return rofs3 / 1024 / 1024;
            }
        }
        public UInt32 udaMb
        {
            get
            {
                return uda / 1024 / 1024;
            }
        }
    }
}
