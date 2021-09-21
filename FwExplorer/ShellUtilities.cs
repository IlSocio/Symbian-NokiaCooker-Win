/*
 * [File purpose]
 * Author: Phillip Piper
 * Date: 1 May 2007 7:44 PM
 * 
 * CHANGE LOG:
 * 2009-07-08  JPP  Don't cache the image collections
 * 1 May 2007  JPP  Initial Version
 */

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace FuzzyByte.Forms
{

	/// <summary>
	/// ShellUtilities contains routines to interact with the Windows Shell.
	/// </summary>
	public static class ShellUtilities
	{
        /// <summary>
        /// Execute the default verb on the file or directory identified by the given path.
        /// For documents, this will open them with their normal application. For executables,
        /// this will cause them to run.
        /// </summary>
        /// <param name="path">The file or directory to be executed</param>
        /// <returns>Values &lt; 31 indicate some sort of error. See ShellExecute() documentation for specifics.</returns>
        /// <remarks>The same effect can be achieved by <code>System.Diagnostics.Process.Start(path)</code>.</remarks>
        public static int Execute(string path)
        {
            return ShellUtilities.Execute(path, "");
        }

        /// <summary>
        /// Execute the given operation on the file or directory identified by the given path.
        /// Example operations are "edit", "print", "explore".
        /// </summary>
        /// <param name="path">The file or directory to be operated on</param>
        /// <param name="operation">What operation should be performed</param>
        /// <returns>Values &lt; 31 indicate some sort of error. See ShellExecute() documentation for specifics.</returns>
        public static int Execute(string path, string operation)
        {
            IntPtr result = ShellUtilities.ShellExecute(0, operation, path, "", "", SW_SHOWNORMAL);
            return result.ToInt32();
        }


        public static Icon GetIconForFile(string FileName, int flags)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            SHGetFileInfo(FileName, 0, out shinfo, Marshal.SizeOf(shinfo), SHGFI_ICON | flags);
            Icon aIcon;
            try
            {
                aIcon = Icon.FromHandle(shinfo.hIcon);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException(String.Format("File \"{0}\" doesn not exist!", FileName), ex);
            }
            return aIcon;
        }

        /// <summary>
        /// Get the string that describes the file's type.
        /// </summary>
        /// <param name="path">The file or directory whose type is to be fetched</param>
        /// <returns>A string describing the type of the file, or an empty string if something goes wrong.</returns>
        public static String GetFileType(string path)
        {
            SHFILEINFO shfi = new SHFILEINFO();
            int flags = SHGFI_TYPENAME;
            IntPtr result = ShellUtilities.SHGetFileInfo(path, 0, out shfi, Marshal.SizeOf(shfi), flags);
            if (result.ToInt32() == 0)
                return String.Empty;
            else
                return shfi.szTypeName;
        }


        #region Native methods

        private const int SHGFI_ICON               = 0x00100;     // get icon
        private const int SHGFI_DISPLAYNAME        = 0x00200;     // get display name
        private const int SHGFI_TYPENAME           = 0x00400;     // get type name
        private const int SHGFI_ATTRIBUTES         = 0x00800;     // get attributes
        private const int SHGFI_ICONLOCATION       = 0x01000;     // get icon location
        private const int SHGFI_EXETYPE            = 0x02000;     // return exe type
        private const int SHGFI_SYSICONINDEX       = 0x04000;     // get system icon index
        private const int SHGFI_LINKOVERLAY        = 0x08000;     // put a link overlay on icon
        private const int SHGFI_SELECTED           = 0x10000;     // show icon in selected state
        private const int SHGFI_ATTR_SPECIFIED     = 0x20000;     // get only specified attributes
        public const int SHGFI_LARGEICON = 0x00000;     // get large icon
        public const int SHGFI_SMALLICON = 0x00001;     // get small icon
        public const int SHGFI_OPENICON           = 0x00002;     // get open icon
        private const int SHGFI_SHELLICONSIZE      = 0x00004;     // get shell size icon
        private const int SHGFI_PIDL               = 0x00008;     // pszPath is a pidl
        private const int SHGFI_USEFILEATTRIBUTES  = 0x00010;     // use passed dwFileAttribute
		//if (_WIN32_IE >= 0x0500)
        private const int SHGFI_ADDOVERLAYS        = 0x00020;     // apply the appropriate overlays
        private const int SHGFI_OVERLAYINDEX       = 0x00040;     // Get the index of the overlay

        private const int FILE_ATTRIBUTE_NORMAL    = 0x00080;     // Normal file
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x00010;     // Directory

        private const int MAX_PATH = 260;


       [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SHFILEINFO
		{
			public IntPtr hIcon; 
			public int    iIcon; 
			public int    dwAttributes; 
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_PATH)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
			public string szTypeName; 
		}

        private const int SW_SHOWNORMAL = 1;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr ShellExecute(int hwnd, string lpOperation, string lpFile, 
            string lpParameters, string lpDirectory, int nShowCmd);

        // [DllImport("shell32.dll")]
        // private static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes, out SHFILEINFO psfi, int cbSizeFileInfo, int uFlags);
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, int dwFileAttributes,
            out SHFILEINFO psfi, int cbFileInfo, int uFlags);


        #endregion
    }
}
