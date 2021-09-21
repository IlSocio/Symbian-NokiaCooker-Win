using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using FuzzyByte.Utils;


namespace Firmware
{
    public abstract class AbstractImage : Logged
    {
        public string filename = "";

        public abstract void ClearImage();
        public abstract void AddFile(string fullFile, string fullDestPath);
        public abstract void CreateNewFolder(string newFolderName, string fullDestPath);
        public abstract void DeleteFileOrFolder(string fullDestPath);
        public abstract void CloseImage();
        public abstract void Extract(string path);
        public abstract List<string> GetDirsList();


        public virtual void RebuildImage(string newFile, string allFilesPath)
        {
            DateTime started = DateTime.Now;
            LogEvent("Rebuild IMAGE in Progress. Please wait...\t");

            OpenImage(newFile);
            ClearImage();
            DoAddAllFilesAndFolders(allFilesPath, "");
            CloseImage();
            /*            try
                        {
                            OpenImage(newFile);
                            ClearImage();
                            DoAddAllFilesAndFolders(allFilesPath, "");
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            CloseImage();
                        }*/

            TimeSpan span = DateTime.Now - started;
            LogEvent("Took: " + (int)span.TotalSeconds + " Seconds\n");
            
            LogEvent("IMAGE rebuilded Successfully!\n");
        }

        public virtual void OpenImage(string aFileName)
        {
            filename = aFileName;
        }

/*        public virtual void AddAllFilesAndFolders(string fullPath, string fullDestPath)
        {
            DoAddAllFilesAndFolders(fullPath, fullDestPath);
        }*/

        private void DoAddAllFilesAndFolders(string fullPath, string fullDestPath)
        {
            if (!fullPath.EndsWith("" + Path.DirectorySeparatorChar))
                fullPath += Path.DirectorySeparatorChar;
            if (!fullDestPath.EndsWith("" + Path.DirectorySeparatorChar))
                fullDestPath += Path.DirectorySeparatorChar;

            //Debug.WriteLine(newDir.Filename);

            string[] files = Directory.GetFiles(fullPath);
            foreach (string aFile in files)
            {
                string filename = Path.GetFileName(aFile);
                AddFile(aFile, fullDestPath);
            }

            string[] dirs = Directory.GetDirectories(fullPath);

            foreach (string aDir in dirs)
            {
                CreateNewFolder(aDir, fullDestPath);
                string[] splittedPath = aDir.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
                string newDirName = splittedPath[splittedPath.Length - 1];
                DoAddAllFilesAndFolders(aDir, fullDestPath + newDirName);
            }
        }
    }

    public class NullImage : AbstractImage
    {
//        public override void AddAllFilesAndFolders(string fullPath, string fullDestPath) { }
        public override void ClearImage() { }
        public override void AddFile(string fullFile, string fullDestPath) { }
        public override void CreateNewFolder(string newFolderName, string fullDestPath) { }
        public override void DeleteFileOrFolder(string fullDestPath) { }
        public override void OpenImage(string aFileName) { }
        public override void CloseImage() { }
        public override void Extract(string path) { }
        public override List<string> GetDirsList()
        { 
            return new List<string>(); 
        }

        public override void RebuildImage(string newFile, string allFilesPath)        
        {
            // Non posso lanciare l'eccezione perche' viene richiamata anche quando non c'e' stato nessun cambiamento del fw.
            // throw new FwException("Can't rebuild an unknown IMAGE!");
        }
    }
}
