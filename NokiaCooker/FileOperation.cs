using System;
using System.Collections.Generic;
using System.Text;

namespace FuzzyByte.NokiaCooker
{
    public enum EFileOperationType
    {
        Copy,
        Delete
    }

    public abstract class FileOperation
    {
        public readonly EFileOperationType iId;

        public FileOperation(EFileOperationType aType)
        {
            iId = aType;
        }
    }

    public class FileDeleteOperation : FileOperation
    {
        public string[] iItems;

        public FileDeleteOperation(string item)
            : base(EFileOperationType.Delete)
        {
            iItems = new string[1];
            iItems[0] = item;
        }
        public FileDeleteOperation(string[] items)
            : base(EFileOperationType.Delete)
        {
            iItems = items;
        }
    }

    public class FileCopyOperation : FileOperation
    {
        public string[] iItems;
        public string iDestPath;

        public FileCopyOperation(string[] items, string destPath) : base(EFileOperationType.Copy)
        {
            iItems = items;
            iDestPath = destPath;
        }
    }
}
