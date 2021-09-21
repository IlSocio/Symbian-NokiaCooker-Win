using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using FuzzyByte.Utils;


namespace Firmware
{
        public class MyRofsDir
        {
            public TRofsDir thisDir;
            public TRofsEntry thisDirEntry;
            public List<MyRofsDir> dirs = new List<MyRofsDir>();
            public List<TRofsEntry> files = new List<TRofsEntry>();
            public List<TContent> contents = new List<TContent>();

            public MyRofsDir(TRofsDir dir)
            {
                thisDir = dir;
                thisDirEntry = null;
            }
        }


        public class RofsImage : AbstractImage
        {
            private Process rofsBuild = null;
            public TRofsHeader header = new TRofsHeader();
            public TRofsDir rootDir = new TRofsDir();
            public MyRofsDir tree = null;
            //private List<string> _filesHidden = new List<string>();     // path remoto
            private Dictionary<string, TRofsEntry> _filesToCORE = new Dictionary<string, TRofsEntry>();     // path remoto minuscono + Entry
            private Dictionary<string, string> _newFolders = new Dictionary<string, string>();  // path remoto minuscolo + path remoto case-sensitive
            private Dictionary<string, string> _newFiles = new Dictionary<string, string>();    // fullpath remoto + fullpath locale
            private bool iMustRebuild = false;
            private string iCfwSignature = "";

            public RofsImage()
            {
                DateTime now = DateTime.Now.Date;
                iCfwSignature = now.Year + "-" + now.Month.ToString().PadLeft(2, '0') + "-" + now.Day.ToString().PadLeft(2, '0');
                iCfwSignature = "Created with NokiaCooker\\n" + iCfwSignature;
            }

            private List<string> GetDirsList(MyRofsDir aDir, string path)
            {
                List<string> result = new List<string>();
                if (aDir == null)
                    return result;

                string name = "";
                if (aDir.thisDirEntry != null)
                {
                    name = aDir.thisDirEntry.iName + Path.DirectorySeparatorChar;
                    result.Add(path + name);
                }
                foreach (MyRofsDir subDir in aDir.dirs)
                {
                    result.AddRange(GetDirsList(subDir, path + name));
                }
                return result;
            }

            public override List<string> GetDirsList()
            {
                return GetDirsList(tree, "\\");
            }

            public override void ClearImage()
            {
                iMustRebuild = true;
                //_filesHidden.Clear();
                _newFiles.Clear();
                _newFolders.Clear();
                _filesToCORE.Clear();
                // TODO: Importante 1
                // TODO: popolare _filesToCORE della ROFx con: filename + indirizzo su CORE
                // TODO: Cancellare tutti i contenuti (i files verso il CORE saranno reinseriti al termine)
            }

            public override void AddFile(string fullFile, string fullDestPath)
            {
                //                fullFile = fullFile.ToLower();
                //                fullDestPath = fullDestPath.ToLower();
                string filename = Path.GetFileName(fullFile);
                _newFiles.Add(fullDestPath+filename, fullFile);
                // "\\private\\100012a5\DBS_101FD685_BrowserBookmarks.db"
                // "C:\ProgettiInCorso\NokiaCooker\FwExplorer\bin\Debug\Files\private\100012a5\DBS_101FD685_BrowserBookmarks.db"

                // TODO: se il file e' gia' presente in _filesToCORE allora lo rimuove da li

                // TODO: Cosa succede se c'e' un file nella ROFX che ne sostituisce uno del CORE e decido di eliminarlo?
                // TODO: Lo imposto come "hidden".
                // 
                // TODO: Ma se il file nella ROFX non sostituiva uno del CORE ma era un nuovo file???
                // TODO: Lo devo eliminare.
                //
                // TODO: Per risolvere potrei marcare com "hidden" qualsiasi file che e' stato rimosso... NO ---> :(
                // TODO: Se al RofsBuild.exe viene chiesto di fare l'hide nella ROFX di un file non presente nel CORE, non viene inserita nessuna entry.
                //
                // TODO: La cosa piu' sicura da fare e' trovare una ROFX che abbia almeno 1 file che sostituisce quello del CORE.
                // TODO: Credo che dagli attributi del file si possa capire se e' un file che sostituisce uno del CORE oppure se e' un file nuovo.
                // TODO: Cosi' che posso capire se devo eliminarlo oppure marcarlo come "hidden"
            }

            public override void CreateNewFolder(string newFolderName, string fullDestPath)
            {
                //newFolderName = newFolderName.ToLower();
                //fullDestPath = fullDestPath.ToLower();
                DirectoryInfo dirInfo = new DirectoryInfo(newFolderName);
                string foldName = dirInfo.Name;
                fullDestPath += foldName;
                _newFolders.Add(fullDestPath.ToLower(), fullDestPath);
            }

            public override void DeleteFileOrFolder(string fullDestPath)
            {
                // TODO:
            }

            private void FixHeader(BinaryWriter bw, uint adjustment, Int64 time)
            {
                header.iIdentifier[3] = (byte)'x';
                header.iTime = time;
                header._iAdjustment = adjustment;
                header.iDirFileEntriesOffset += adjustment;
                header.iDirTreeOffset += adjustment;
                bw.BaseStream.Seek(0, SeekOrigin.Begin);
                header.Write(bw);
            }


            private void FixRoot(BinaryWriter bw)
            {
                bw.BaseStream.Seek(header.iDirTreeOffset - header._iAdjustment, SeekOrigin.Begin);
                rootDir.iFileBlockAddress += header._iAdjustment;
                foreach (TRofsEntry entry in rootDir.iSubDir)
                {
                    if (entry.iFileAddress != 0xFFFFFFFF && entry.iFileAddress != 0)
                        entry.iFileAddress += header._iAdjustment;
                }
                rootDir.Write(bw);
            }


            private void RestoreCoreEntries(BinaryWriter bw, string fullRemotePath, MyRofsDir dirTree)
            {
                foreach (TRofsEntry entry in dirTree.files)
                {
                    string key = (fullRemotePath + entry.iName).ToLower();

                    // This check is needed in order to overwrite some CORE entries
                    // if (entry.iFileAddress != 0 || entry.iFileAddress != 0xFFFFFFFF)
                    //    continue;

                    if (_filesToCORE.ContainsKey(key))
                    {
                        TRofsEntry coreEntry = _filesToCORE[key];
                        entry.FixToCore(bw, coreEntry);
                    }
                }

                foreach (MyRofsDir aDir in dirTree.dirs)
                {
                    RestoreCoreEntries(bw, fullRemotePath + aDir.thisDirEntry.iName.ToLower() + Path.DirectorySeparatorChar, aDir);
                }
            }


            private void FixTree(BinaryWriter bw, MyRofsDir dirTree)
            {
                int i=0;
                foreach (TRofsEntry entry in dirTree.files)
                {
                    entry.Fix(bw, header._iAdjustment);
                    Debug.WriteLine("Fixing: " + entry.iName + " NewAddr:" + BytesUtils.ToHex(entry.iFileAddress));
                    /*                    TContent cont = dirTree.contents[i];
                                        cont.Write(bw, entry, header._iAdjustment);*/
                    i++;
                }
                foreach (MyRofsDir aDir in dirTree.dirs)
                {
                    aDir.thisDir.Fix(bw, header._iAdjustment);  // <-- questo sistema gia' le sottodirectory...
                    //aDir.thisDirEntry.Fix(bw, header._iAdjustment);
                    Debug.WriteLine("*** Fixing: \\" + aDir.thisDirEntry.iName + "\\ NewAddr:" + BytesUtils.ToHex(aDir.thisDir.iFileBlockAddress) + " " + BytesUtils.ToHex(aDir.thisDirEntry.iFileAddress));
                    FixTree(bw, aDir);
                }
            }

/*
#if DEBUG
            private void GetDirEntries(string fullRemotePath, MyRofsDir dirTree, SortedDictionary<long, string> entries)
            {                
                foreach (MyRofsDir aDir in dirTree.dirs)
                {
                    entries.Add(aDir.thisDir._offset, fullRemotePath + aDir.thisDirEntry.iName);
                    GetDirEntries(fullRemotePath + aDir.thisDirEntry.iName + Path.DirectorySeparatorChar, aDir, entries);
                }
            }

            private void GetFileEntries(string fullRemotePath, MyRofsDir dirTree, SortedDictionary<long, TRofsEntry> entries)
            {
                foreach (TRofsEntry entry in dirTree.files)
                {
                    entries.Add(entry._offset, entry);
                }

                foreach (MyRofsDir aDir in dirTree.dirs)
                {
                    GetFileEntries(fullRemotePath + aDir.thisDirEntry.iName.ToLower() + Path.DirectorySeparatorChar, aDir, entries);
                }
            }
#endif*/


            public override void OpenImage(string aFileName)
            {
                base.OpenImage(aFileName);

                iMustRebuild = false;
                if (rofsBuild == null)
                    rofsBuild = CreateBuildRofsProcess(Path.GetDirectoryName(aFileName) + Path.DirectorySeparatorChar);

                BinaryReader br = new BinaryReader(new FileStream(aFileName, FileMode.Open));
                header.Read(br);                            // HEADER

                string type = ""+(char)header.iIdentifier[0]+(char)header.iIdentifier[1]+(char)header.iIdentifier[2]+(char)header.iIdentifier[3];
                //LogEvent("Signature: "+type+"\n");

                long dirTreeOffset = header.iDirTreeOffset - header._iAdjustment;
                Debug.Assert(br.BaseStream.Position == dirTreeOffset);
                br.BaseStream.Seek(dirTreeOffset, SeekOrigin.Begin);
                rootDir.Read(br);                           // ROOT DIR                
                tree = BuildDirTree(br, rootDir);           // DIR TREE

                long fileListOffset = header.iDirFileEntriesOffset - header._iAdjustment;
                Debug.Assert(fileListOffset == dirTreeOffset + header.iDirTreeSize);
                //Debug.Assert(br.BaseStream.Position == fileListOffset);
                br.BaseStream.Seek(fileListOffset, SeekOrigin.Begin);
                ReadFileTree(br, tree);                     // FILE TREE

                long contentOffset = fileListOffset + header.iDirFileEntriesSize;
                //Debug.Assert(br.BaseStream.Position == contentOffset);
                br.BaseStream.Seek(contentOffset, SeekOrigin.Begin);
                ReadFileContentTree(br, tree);              // CONTENTS 

                br.Close();
            }

            

          /* private string CreateOBY()
            {
                if (_newFiles.Count <= 0)
                    return "";
                string obyPath = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar;
                string obyName = "files.oby";
                string obyFile = obyPath + obyName;
                StreamWriter sw = new StreamWriter(new FileStream(obyFile, FileMode.Create));

                string fname = Encoding.ASCII.GetString(header.iIdentifier) + ".ROM";
                sw.WriteLine("rofsname=" + fname); // usa nome ROFX.ROM
                sw.WriteLine("rofssize=0x"+BytesUtils.ToHex(header.iMaxImageSize));


                SortedDictionary<long, string> dirs = new SortedDictionary<long, string>();
                GetDirEntries("\\", tree, dirs);

                foreach (long offset in dirs.Keys)
                {
                    string part_path = dirs[offset];

                    string remotePath = "";
                    foreach (string key in _newFiles.Keys)
                    {
                        if (remotePath == "")
                        {
                            string dirName = Path.GetDirectoryName(key);
                            if (dirName.ToLower() == part_path.ToLower())
                            {
                                remotePath = key;
                            }
                        }
                    }

                    if (remotePath != "")
                    {
                        string local_path = _newFiles[remotePath];
                        sw.WriteLine("data=\"" + local_path + "\"  \"" + remotePath + "\"");
                        _newFiles.Remove(remotePath);
                    }
                    else
                    {
                    }
                }

                // Remaining Files...
                foreach (string remote_path in _newFiles.Keys)
                {
                    string local_path = _newFiles[remote_path];
                    sw.WriteLine("data=\"" + local_path + "\"  \"" + remote_path + "\"");
                }

                sw.Close();
                return obyName;
            }
            
            */

            private string CreateOBY()
            {
                string obyPath = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar;
                string obyName = "files.oby";
                string obyFile = obyPath + obyName;
                StreamWriter sw = new StreamWriter(new FileStream(obyFile, FileMode.Create));

                string fname = Path.GetFileName(filename);
                sw.WriteLine("rofsname=" + fname); // usa nome ROFX.ROM
                sw.WriteLine("rofssize=0x"+BytesUtils.ToHex(header.iMaxImageSize));

                if (_newFiles.Count <= 0)
                {
                    sw.WriteLine("hide=thisisjustadummyentrybyNokiaCooker");
                    sw.Close();
                    return obyName;
                }

                List<ObyEntry> entries = new List<ObyEntry>();
                Get_ObyEntries(tree, "\\", entries);
                entries.Sort();

                foreach (ObyEntry obyEntry in entries)
                { 
                    string remote_path = obyEntry.remotePath;
                    Debug.WriteLine("Offs:" + BytesUtils.ToHex((UInt32)obyEntry.offset) + " " + obyEntry.remotePath + "  Content: " + BytesUtils.ToHex(obyEntry.fileAddress));

                    if (remote_path.EndsWith("\\"))
                        continue;

                    string exAttrib = "";
                    switch (obyEntry.attExtra)
                    {
                        case 0xFF:
                            break;
                        case 0xFD:
                            exAttrib = "exAttrib=U";
                            break;
                        default:
                            throw new FwException("Unknown exAttrib, Please Contact: m.bellino@symbian-toys.com");
                    }

                    if (_newFiles.ContainsKey(remote_path))
                    {
                        string local_path = _newFiles[remote_path];
                        string pathToRemove = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar;
                        DirectoryInfo dirInfo = new DirectoryInfo(pathToRemove);
                        pathToRemove = dirInfo.Parent.FullName;
                        local_path = local_path.Replace(pathToRemove, "..");
                        //if (local_path.EndsWith(".dll") || local_path.EndsWith(".exe") )
                        //    sw.WriteLine("file=\"" + local_path + "\"  \"" + remote_path + "\"" + exAttrib);
                        //else
                        sw.WriteLine("data=\"" + local_path + "\"  \"" + remote_path + "\" " + exAttrib);
                        _newFiles.Remove(remote_path);
                        // E' nei nuovi file quindi lo sovrascrive...
                        // continue;
                    }

                    if (obyEntry.fileAddress == 0xFFFFFFFF || obyEntry.fileAddress == 0x0)
                    {
                        // HIDDEN IN ROFS or HIDDEN IN ROFX
                        sw.WriteLine("hide=\"" + remote_path + "\"");
                    }

                    if (obyEntry.fileAddress < header._iAdjustment)
                    {
                        // CORE ENTRY
                        sw.WriteLine("hide=\"" + remote_path + "\"");
                    }
                }
                // Adds Remaining Files...
                foreach (string remote_path in _newFiles.Keys)
                {
                    string local_path = _newFiles[remote_path];
                    string pathToRemove = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar;
                    DirectoryInfo dirInfo = new DirectoryInfo(pathToRemove);
                    pathToRemove = dirInfo.Parent.FullName;
                    local_path = local_path.Replace(pathToRemove, "..");
                    sw.WriteLine("data=\"" + local_path + "\"  \"" + remote_path + "\"");
                }

                sw.Close();
                return obyName;
            }

/*            private string CreateOBY()
            {
                if (_newFiles.Count <= 0)
                    return "";
                string obyPath = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar;
                string obyName = "files.oby";
                string obyFile = obyPath + obyName;
                StreamWriter sw = new StreamWriter(new FileStream(obyFile, FileMode.Create));

                string fname = Encoding.ASCII.GetString(header.iIdentifier) + ".ROM";
                sw.WriteLine("rofsname=" + fname); // usa nome ROFX.ROM
                sw.WriteLine("rofssize=0x"+BytesUtils.ToHex(header.iMaxImageSize));
                foreach (string remote_path in _filesToCORE.Keys)
                {
                    sw.WriteLine("hide=\"" + remote_path + "\"");
                }
                foreach (string remote_path in _filesHidden)
                {
                    sw.WriteLine("hide=\"" + remote_path + "\"");
                }

                foreach (string remote_path in _newFiles.Keys)
                {
                    string local_path = _newFiles[remote_path];
                    sw.WriteLine("data=\"" + local_path + "\"  \"" + remote_path + "\"");
                }
                sw.Close();
                return obyName;
            }*/

            private Process CreateBuildRofsProcess(string folder)
            {
                string fname = folder + "RofsBuild.exe";
#if DEBUG
                if (!File.Exists(fname))
                    return null;
#endif
                Process rCompExe = new Process();
                rCompExe.StartInfo.UseShellExecute = false;
                rCompExe.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                rCompExe.StartInfo.RedirectStandardOutput = true;
                rCompExe.StartInfo.RedirectStandardError = true;
                rCompExe.StartInfo.CreateNoWindow = true;
                rCompExe.StartInfo.FileName = fname;
                rCompExe.StartInfo.WorkingDirectory = Path.GetDirectoryName(rCompExe.StartInfo.FileName);
                if (!System.IO.File.Exists(rCompExe.StartInfo.FileName))
                    throw new RofsException("RofsBuild.exe not found: " + rCompExe.StartInfo.FileName);
                return rCompExe;
            }

            private void LaunchRofsBuild(string arguments)
            {
                rofsBuild.StartInfo.Arguments = arguments;
                rofsBuild.Start();
                string msg = rofsBuild.StandardError.ReadToEnd();
                rofsBuild.WaitForExit();
                if (rofsBuild.ExitCode != 0)
                    throw new RofsException("RofsBuild Error: " + rofsBuild.ExitCode + "  " + msg);
                if (msg != "")
                    LogEvent("RofsBuild: "+ msg, EventType.Warning);
            }


            // TODO: Crea ROFS inserendo dei files vuoti al posto dei riferimenti al CORE.
            // TODO: Saranno create delle Entry con Length=0 iFileAddress=xxxxxx
            // TODO: aggiorna iFileAddress con il valore del CORE

            private class ObyEntry : IComparable
            {
                public string remotePath;
                public UInt32 fileAddress;
                public Int64 offset;
                public byte attExtra;

                public ObyEntry(string aRemotePath, Int64 aOffset, UInt32 aFileAddress)
                {
                    remotePath = aRemotePath;
                    offset = aOffset;
                    fileAddress = aFileAddress;
                    attExtra = 0xFF;
                }

                public bool IsHidden
                {
                    get
                    {
                        return (fileAddress == 0xFFFFFFFF || fileAddress == 0);
                    }
                }


                #region IComparable Membri di

                public int CompareTo(object obj)
                {
                    ObyEntry data = obj as ObyEntry;
                    if (!IsHidden && data.IsHidden)
                        return -1;
                    if (IsHidden && !data.IsHidden)
                        return 1;
                    if (IsHidden && data.IsHidden)
                        return offset.CompareTo(data.offset);
                    return fileAddress.CompareTo(data.fileAddress);
                }

                #endregion
            }


            private void Get_ObyEntries(MyRofsDir aDir, string currPath, List<ObyEntry> entries)
            {
                foreach (TRofsEntry entry in aDir.files)
                {
                    string remotePath = (currPath + entry.iName);
                    ObyEntry newEntry = new ObyEntry(remotePath, entry._offset, entry.iFileAddress);
                    entries.Add(newEntry);
                    newEntry.attExtra = entry.iAttExtra;
                }
                foreach (MyRofsDir aSubDir in aDir.dirs)
                {
                    string remotePath = (currPath + aSubDir.thisDirEntry.iName + "\\");
                    ObyEntry newEntry = new ObyEntry(remotePath, aSubDir.thisDirEntry._offset, aSubDir.thisDirEntry.iFileAddress);
                    entries.Add(newEntry);
                    Get_ObyEntries(aSubDir, currPath + aSubDir.thisDirEntry.iName + "\\", entries);
                }
            }


        /*    private void Get_Files_With_Addr(MyRofsDir aDir, string currPath, List<string> files, long addr)
            {
                foreach (TRofsEntry entry in aDir.files)
                {
                    if (entry.iFileAddress == addr)
                    {
                        //******** string remotePath = (currPath + entry.iName).ToLower();
                        string remotePath = (currPath + entry.iName);
                        files.Add(remotePath);
                    }
                }
                foreach (MyRofsDir aSubDir in aDir.dirs)
                {
                    Get_Files_With_Addr(aSubDir, currPath + aSubDir.thisDirEntry.iName + "\\", files, addr);
                }
            }


            private void Get_HiddenInROFS_Files(MyRofsDir aDir, string currPath, List<string> files)
            {
                Get_Files_With_Addr(aDir, currPath, files, 0xFFFFFFFF);
            }


            private void Get_HiddenInROFx_Files(MyRofsDir aDir, string currPath, List<string> files)
            {
                Get_Files_With_Addr(aDir, currPath, files, 0);
            }

            */
            private void Get_CORE_Entries(MyRofsDir aDir, string currPath, Dictionary<string, TRofsEntry> files)
            {
                foreach (TRofsEntry entry in aDir.files)
                {
                    if (entry.iFileAddress < header._iAdjustment)
                    {
                        string remotePath = (currPath + entry.iName).ToLower();
                        files.Add(remotePath, entry);
                    }
                }
                foreach (MyRofsDir aSubDir in aDir.dirs)
                {
                    Get_CORE_Entries(aSubDir, currPath + aSubDir.thisDirEntry.iName + "\\", files);
                }
            }
            

/*            public void Fix(string filename, uint adjust)
            {
                OpenImage(filename);
                BinaryWriter bw = new BinaryWriter(new FileStream(filename, FileMode.Open));
                FixHeader(bw, header._iAdjustment, time);
                FixRoot(bw);
                FixTree(bw, tree);
                bw.Close();
            }*/


            private string GetCustomerSwLocalPath()
            {
                string fname = "\\resource\\versions\\customersw.txt";
                foreach (string remote_path in _newFiles.Keys)
                {
                    if (remote_path.ToLower().Contains(fname))
                        return _newFiles[remote_path];
                }
                return "";
            }


            public override void CloseImage()
            {
//                if (_newFiles.Count > 0 || _newFolders.Count > 0)
                if (iMustRebuild)
                {
                    /*_filesHidden.Clear();
                    Get_HiddenInROFS_Files(tree, "\\", _filesHidden);
                    Get_HiddenInROFx_Files(tree, "\\", _filesHidden);
                    */
                    _filesToCORE.Clear();
                    if (header.iIdentifier[3] == 'x')
                    {
                        Get_CORE_Entries(tree, "\\", _filesToCORE); 
                    }

                    // Temporaneo repack "grezzo"...

                    // Get the filename for \\resource\customersw.txt
                    // Replace the content with NokiaCooker signature
                    string oldText = "";
                    FileAttributes oldAttrib = FileAttributes.Normal;
                    string fname = GetCustomerSwLocalPath();
                    if (fname != "")
                    {
                        oldText = File.ReadAllText(fname);
                        oldAttrib = File.GetAttributes(fname);
                        File.SetAttributes(fname, FileAttributes.Normal);
                        File.WriteAllText(fname, iCfwSignature, Encoding.Unicode);
                    }

                    try
                    {
                        string oby_fname = CreateOBY();
                        LaunchRofsBuild(" -cache -loglevel2 -logfile=test.log " + oby_fname);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        // Restore file attrib and content
                        if (fname != "")
                        {
                            File.WriteAllText(fname, oldText, Encoding.Unicode);
                            File.SetAttributes(fname, oldAttrib);
                        }
                    }


                    if (header.iIdentifier[3] == 'x')
                    {
                        uint back_adjust = header._iAdjustment;
                        string back_filename = filename;
                        Int64 back_time = header.iTime;
                        _newFiles.Clear();
                        _newFolders.Clear();
                        filename = "";
                        header = new TRofsHeader();
                        rootDir = new TRofsDir(); ;
                        tree = null;

                        OpenImage(back_filename);
                        BinaryWriter bw = new BinaryWriter(new FileStream(back_filename, FileMode.Open));
                        FixHeader(bw, back_adjust, back_time);
                        FixRoot(bw);
                        FixTree(bw, tree);

                        // Fix degli indirizzi delle varie entry _filesToCORE
                        RestoreCoreEntries(bw, "" + Path.DirectorySeparatorChar, tree);
                        bw.Close();
                    }
                    else
                    {
                        // Just to add the NC signature...
                        BinaryReader br = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                        header.Read(br);
                        br.Close();
                        BinaryWriter bw = new BinaryWriter(new FileStream(filename, FileMode.Open, FileAccess.Write, FileShare.ReadWrite));
                        header.Write(bw);
                        bw.Close();
                    }
                }
                _newFiles.Clear();
                _newFolders.Clear();
                filename = "";
                header = new TRofsHeader();
                rootDir = new TRofsDir();;
                tree = null;            
            }

            public override void Extract(string path)
            {
                DateTime started = DateTime.Now;
                LogEvent("Unpack ROF" + ((char)header.iIdentifier[3]) + ". Please wait...\t");

                Extract(path, tree);

                TimeSpan span = DateTime.Now - started;
                LogEvent("Took: " + (int)span.TotalSeconds + " Seconds\n");
                LogEvent("ROF" + ((char)header.iIdentifier[3]) + " unpacked Successfully!\n");
            }

            private void Extract(string path, MyRofsDir aDir)
            {
                //return;
                if (!path.EndsWith("" + Path.DirectorySeparatorChar))
                    path = path + Path.DirectorySeparatorChar;

                if (aDir.thisDirEntry != null)
                {
                    if (aDir.thisDirEntry.iName.Contains(":"))
                    {
                        LogEvent("Invalid Dir Entry: " + aDir.thisDirEntry.iName, EventType.Warning);
                        aDir.thisDirEntry.iName = aDir.thisDirEntry.iName.Replace(":", "");
                    }

                    path += aDir.thisDirEntry.iName + Path.DirectorySeparatorChar;
                }
                //Debug.WriteLine("EXTRACT: " + path);
                Directory.CreateDirectory(path);
                // LogEvent("Directory: "+ path, EventType.Debug);
                

                // TODO: L'immagine estratta e' piu' grande dell'immagine ripaccata...
                // TODO: Fix il salvataggio dell'immagine nel FPSX...

                int i = 0;
                foreach (TRofsEntry aFile in aDir.files)
                {
                    bool extract = true;
                    string flag = "";
                    if (aFile.iFileAddress < header._iAdjustment)
                    {
                        extract = false;                        // E71   ROFX   rm346_510.21.009_prd  
                        flag = " ROFS1-CORE Reference ";        // La ROFS2 fa riferimento ai file della ROFS1
                        // In questo caso non e' possibile modificare la ROFS1 perche' questi riferimenti (presenti nella ROFS2) non verranno aggiornati.
                    }
                    if (aFile.iFileAddress == 0)
                    {
                        extract = false;
                        flag = " HIDDEN in ROFX ";
                    }
                    if (aFile.iFileAddress == 0xFFFFFFFF)
                    {
                        extract = false;
                        flag = " HIDDEN in ROFS ";  // Caso Atipico
                    }

                   // if (flag != "")
                    //    LogEvent(flag + "File: " + aFile.iName + " Attr:" + aFile.iAtt + " AttExtra:" + aFile.iAttExtra + " Addr:" + aFile.iFileAddress + " Size:" + aFile.iFileSize, EventType.Debug);
                    if (extract)
                    {                        
                        string destPath = Path.GetDirectoryName(path + aFile.iName);
                        // Debug.Assert(Directory.Exists(destPath));
                        if (Directory.Exists(destPath))
                        {
                            File.WriteAllBytes(path + aFile.iName, aDir.contents[i].data);
                        }
                        else
                        {
                            LogEvent("File Not Extracted: " + aFile.iName, EventType.Warning);
                        }
                    }
                    i++;
                }

                foreach (MyRofsDir aSubDir in aDir.dirs)
                {
                    Extract(path, aSubDir);
                }
            }


/*            private void WriteAll(string filename)
            {
                BinaryWriter bw = new BinaryWriter(new FileStream(filename, FileMode.Create));
                header.Write(bw);

                long dirTreeOffset = header.iDirTreeOffset - header._iAdjustment;
                bw.BaseStream.Seek(dirTreeOffset, SeekOrigin.Begin);
                rootDir.Write(bw);
                WriteDirTree(bw, tree);


                long fileListOffset = header.iDirFileEntriesOffset - header._iAdjustment;
                bw.BaseStream.Seek(fileListOffset, SeekOrigin.Begin);
                WriteFileTree(bw, tree);


                long contentOffset = fileListOffset + header.iDirFileEntriesSize;
                bw.BaseStream.Seek(contentOffset, SeekOrigin.Begin);
                WriteFileContentTree(bw, tree);
                // TODO: Update all the indexes... 
                // 1) identificare gli indici da aggiornare... 
                // 2) identificare l'ordine in cui aggiornarli... 
                // 3) capire come farlo...

                bw.Close();
            }*/


            private MyRofsDir BuildDirTree(BinaryReader br, TRofsDir dir)
            {
                MyRofsDir newEntry = new MyRofsDir(dir);
                foreach (TRofsEntry entry in dir.iSubDir)
                {
                    //Debug.WriteLine("BuildDirTree: DIR: " + entry.iName + " Addr:"+ BytesUtils.ToHex(entry.iFileAddress));
                    long subDirAddr = entry.iFileAddress - header._iAdjustment;
                    // Debug.Assert(subDirAddr == br.BaseStream.Position);
                    // A quanto pare e' indispensabile leggere i dati tramite seek... 
                    br.BaseStream.Seek(subDirAddr, SeekOrigin.Begin);

                    TRofsDir subDirContent = new TRofsDir();
                    subDirContent.Read(br);

                    MyRofsDir subDirTree = BuildDirTree(br, subDirContent);
                    subDirTree.thisDirEntry = entry;
                    newEntry.dirs.Add(subDirTree);
                }
                return newEntry;
            }

/*            private void WriteDirTree(BinaryWriter bw, MyRofsDir myDir)
            {
                foreach (MyRofsDir entry in myDir.dirs)
                {
                    uint dirAddress = entry.thisDirEntry.iFileAddress - header._iAdjustment;
                    bw.BaseStream.Seek(dirAddress, SeekOrigin.Begin);
                    long oriPos = bw.BaseStream.Position;
                    entry.thisDir.Write(bw);
                    WriteDirTree(bw, entry);
                    int pad = (int)(bw.BaseStream.Position - oriPos) % 2;
                    for (int i = 0; i < pad; i++)
                        bw.Write(entry.thisDir.padding);
                }
            }*/


            private void ReadFileTree(BinaryReader br, MyRofsDir myDir)
            {
 /*               if (myDir.thisDirEntry != null)
                {
                    if (myDir.thisDirEntry.iName == "css")
                    {
                    }
//                    Debug.WriteLine("Process: " + myDir.thisDirEntry.iName);
                }*/
                uint fileBlockAddress = myDir.thisDir.iFileBlockAddress - header._iAdjustment; // ROFx
                if (fileBlockAddress > 0)
                {
                    // Debug.Assert(br.BaseStream.Position == fileBlockAddress);
                    // A quanto pare e' indispensabile leggere i dati tramite seek... 
                    br.BaseStream.Seek(fileBlockAddress, SeekOrigin.Begin);
                    long startPos = br.BaseStream.Position;
                    while (br.BaseStream.Position < myDir.thisDir.iFileBlockSize + startPos)
                    {
                        TRofsEntry entry = new TRofsEntry();
                        entry.Read(br);
                        myDir.files.Add(entry);
 //                       Debug.WriteLine("Entry: "+ entry.iName);
                    }
                }
                foreach (MyRofsDir aDir in myDir.dirs)
                {
                    ReadFileTree(br, aDir);
                }
            }

/*            private void WriteFileTree(BinaryWriter bw, MyRofsDir myDir)
            {
                long fileBlockAddr = myDir.thisDir.iFileBlockAddress - header._iAdjustment;
                bw.BaseStream.Seek(fileBlockAddr, SeekOrigin.Begin);
                foreach (TRofsEntry entry in myDir.files)
                {
                    entry.Write(bw);
                }
                foreach (MyRofsDir entry in myDir.dirs)
                {
                    WriteFileTree(bw, entry);
                }
            }*/


            private void ReadFileContentTree(BinaryReader br, MyRofsDir myDir)
            {
                foreach (TRofsEntry fileEntry in myDir.files)
                {
                    if (fileEntry.iName.Contains("atches.zip"))
                    {
                    }
                    TContent cont = new TContent(); // 25162692
                    cont.Read(br, fileEntry, header._iAdjustment);
                    myDir.contents.Add(cont);
                }
                foreach (MyRofsDir aDir in myDir.dirs)
                {
                    ReadFileContentTree(br, aDir);
                }
            }

/*            private void WriteFileContentTree(BinaryWriter bw, MyRofsDir myDir)
            {
                int i = 0;
                foreach (TRofsEntry fileEntry in myDir.files)
                {
                    TContent cont = myDir.contents[i];
                    cont.Write(bw, fileEntry, header._iAdjustment);
                    i++;
                }
                foreach (MyRofsDir aDir in myDir.dirs)
                {
                    WriteFileContentTree(bw, aDir);
                }
            }*/
        }
}
