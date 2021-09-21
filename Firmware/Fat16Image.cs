using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FuzzyByte.Utils;
using FuzzyByte.IO;


namespace Firmware
{

    public class Fat16Image : AbstractImage
    {
        public Fat16 fat16 = new Fat16();

        public Fat16Image()
        {
            
        }

        private List<string> GetDirsList(List<Entry> childs, string path)
        {
            List<string> result = new List<string>();
            foreach (Entry entry in childs)
            {
                if (entry.Filename == ".")
                    continue;
                if (entry.Filename == "..")
                    continue;
                if (entry.IsDeleted)
                    continue;
                if (entry.IsDirectory)
                {
                    List<Entry> subs = fat16.ReadDirectoryContentFromImage(entry);
                    result.Add(path + entry.Filename);
                    result.AddRange(GetDirsList(subs, path + entry.Filename + Path.DirectorySeparatorChar));
                }
                else
                {
                    //result.Add(path + entry.Filename);
                }
            }
            return result;
        }

        public override List<string> GetDirsList()
        {
            return GetDirsList(fat16.rootDir, "\\");
        }

        public override void OpenImage(string fname)
        {
            base.OpenImage(fname);
            fat16.OpenImage(fname);
            System.Diagnostics.Debug.WriteLine("Max FAT size: "+ fat16.Maxsize + " -- " + BytesUtils.GetSizeDescr(fat16.Maxsize));
        }

        public override void CloseImage()
        {
            fat16.CloseImage();
        }

        public override void ClearImage()
        {
            fat16.ClearImage();
        }

        public override void Extract(string path)
        {
            DateTime started = DateTime.Now;
            LogEvent("Unpack FAT. Please wait...\t");
            fat16.ExtractAllFromImage(fat16.rootDir, path);
            TimeSpan span = DateTime.Now - started;
            LogEvent("Took: " + (int)span.TotalSeconds + " Seconds\n");
            LogEvent("FAT unpacked Successfully!\n");
        }

        public void ResizeFat(UInt32 newSize)
        {                
            fat16.Resize(newSize);
        }

        private Entry FindFileOrDir(string fullDest, ref Entry parentEntry)
        {
            parentEntry = null;
            string[] destDirs = Dirs.SplitPath(fullDest);
            if (destDirs.Length == 0)
            {
                // Root Folder
                return null;
            }
            List<Entry> childs = fat16.rootDir;
            for (int i = 0; i < destDirs.Length; i++)
            {
                Entry entry = fat16.FindFileOrDir(destDirs[i], childs);
                if (entry == null)
                    return null;
                if (i == destDirs.Length - 1)
                    return entry;
                if (!entry.IsDirectory)
                    return null;
                childs = fat16.ReadDirectoryContentFromImage(entry);
                Entry aEntry = childs[2];
                string aaa = aEntry.Filename;
                parentEntry = entry;
            }
            return null;
        }


        public override void AddFile(string fullFile, string fullDestPath)
        {
            if (!fullDestPath.EndsWith("" + Path.DirectorySeparatorChar))
                fullDestPath += Path.DirectorySeparatorChar;

            // Find Parent dir...
            Entry parent = null;
            Entry entry = FindFileOrDir(fullDestPath, ref parent);
            if (entry == null && parent != null)
                throw new Fat16Exception("Destination Path Doesn't Exists");

            string filename = Path.GetFileName(fullFile);
            Entry newEntry = fat16.AddFileEntry(filename, File.ReadAllBytes(fullFile), entry);
        }


        public override void CreateNewFolder(string newFolderName, string fullDestPath)
        {
            if (!newFolderName.EndsWith("" + Path.DirectorySeparatorChar))
                newFolderName += Path.DirectorySeparatorChar;
            if (!fullDestPath.EndsWith("" + Path.DirectorySeparatorChar))
                fullDestPath += Path.DirectorySeparatorChar;

            // Find dir...
            Entry parent = null;
            Entry entryDir = FindFileOrDir(fullDestPath, ref parent);
            if (entryDir == null && parent != null)
            {
                throw new Fat16Exception("Destination Path Doesn't Exists");
            }
            // Crea new Dir in Root oppure in Path...

          //  System.Diagnostics.Debug.WriteLine("Create Folder: " + newFolderName);
            // Create new Dir
            DirectoryInfo dirInfo = new DirectoryInfo(newFolderName);
            string newDirName = dirInfo.Name;
/*            string[] splittedPath = BytesUtils.SplitPath(newFolderName);
            if (splittedPath.Length == 0)
                throw new Fat16Exception("Invalid Folder Name");

            string newDirName = splittedPath[splittedPath.Length - 1];*/
            Entry newDir = fat16.AddDirEntry(newDirName, entryDir);
            if (newDir == null)
                throw new Fat16Exception("Folder Already Exists");
        }


        public override void DeleteFileOrFolder(string fullDestPath)
        {
            if (!fullDestPath.EndsWith("" + Path.DirectorySeparatorChar))
                fullDestPath += Path.DirectorySeparatorChar;

            // Find Parent dir...
            Entry parent = null;
            Entry entry = FindFileOrDir(fullDestPath, ref parent);
            if (entry == null)
                throw new Fat16Exception("Destination Path Doesn't Exists");

            fat16.DeleteEntryFromImage(parent, entry);
        }
    }
}
