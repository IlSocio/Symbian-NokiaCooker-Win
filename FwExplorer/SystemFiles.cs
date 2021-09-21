using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using FuzzyByte;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;


namespace FuzzyByte.Forms
{
	public class SystemFiles : IDisposable
	{
		#region Fields                
        private Hashtable ht = new Hashtable();
		private ImageList _smallImageList = new ImageList();
		private ImageList _largeImageList = new ImageList();
		private bool _disposed = false;
		#endregion

		#region Properties
		/// <summary>
		/// Gets System.Windows.Forms.ImageList with small icons in. Assign this property to SmallImageList of ListView, TreeView etc.
		/// </summary>
		public ImageList SmallIconsImageList
		{
			get { return _smallImageList; }
		}

		/// <summary>
		/// Gets System.Windows.Forms.ImageList with large icons in. Assign this property to LargeImageList of ListView, TreeView etc.
		/// </summary>
		public ImageList LargeIconsImageList
		{
			get { return _largeImageList; }
		}

		/// <summary>
		/// Gets number of icons were loaded
		/// </summary>
		public int Count
		{
			get { return _smallImageList.Images.Count; }
		}
		#endregion

		#region Constructor/Destructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public SystemFiles()
			: base()
		{
			_smallImageList.ColorDepth = ColorDepth.Depth32Bit;
			_smallImageList.ImageSize = SystemInformation.SmallIconSize;

			_largeImageList.ColorDepth = ColorDepth.Depth32Bit;
			_largeImageList.ImageSize = SystemInformation.IconSize;
		}

		private void CleanUp(bool disposing)
		{
			if (!this._disposed)
			{
				if (disposing)
				{
					_smallImageList.Dispose();
					_largeImageList.Dispose();
				}
			}
			_disposed = true;
		}

		/// <summary>
		/// Performs resource cleaning
		/// </summary>
		public void Dispose()
		{
			CleanUp(true);
			GC.SuppressFinalize(this);
		}

		~SystemFiles()
		{
			CleanUp(false);
		}
		#endregion

		#region Public Methods

        public string GetFileType(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            string res = ht[ext] as string;
            if (res == null)
            {
                res = ShellUtilities.GetFileType(path);
                ht.Add(ext, res);
            }
            return res;
        }

        public int GetIconIndex(string FileName)
        {
            return GetIconIndex(FileName, 0);
        }
		/// <summary>
		/// Returns index of an icon based on FileName. Note: File should exists!
		/// </summary>
		/// <param name="FileName">Name of an existing File or Directory</param>
		/// <returns>Index of an Icon</returns>
		public int GetIconIndex(string FileName, int flags)
		{
			FileInfo info = new FileInfo(FileName);

			string ext = info.Extension;
			if (String.IsNullOrEmpty(ext))
			{
				if ((info.Attributes & FileAttributes.Directory) != 0)
					ext = "5EEB255733234c4dBECF9A128E896A1E"; // for directories
				else
					ext = "F9EB930C78D2477c80A51945D505E9C4"; // for files without extension
			}
			else
				if (ext.Equals(".exe", StringComparison.InvariantCultureIgnoreCase) ||
					ext.Equals(".lnk", StringComparison.InvariantCultureIgnoreCase))
					ext = info.Name;

            ext = flags + ext;
			if (_smallImageList.Images.ContainsKey(ext))
			{
				return _smallImageList.Images.IndexOfKey(ext);
			}
			else
			{
                Icon smallIcon = ShellUtilities.GetIconForFile(FileName, ShellUtilities.SHGFI_SMALLICON | flags);
				_smallImageList.Images.Add(ext, smallIcon);

                Icon largeIcon = ShellUtilities.GetIconForFile(FileName, ShellUtilities.SHGFI_LARGEICON | flags);
				_largeImageList.Images.Add(ext, largeIcon);

				return _smallImageList.Images.Count - 1;
			}
		}
		#endregion
	}
}
