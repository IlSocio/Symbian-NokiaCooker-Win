using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;


namespace FuzzyByte
{
    public static class AssemblyInfo
    {

        public static string Title(Assembly assembly)
        {
            string result = string.Empty;
            if (assembly != null)
            {
                object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                    result = ((AssemblyTitleAttribute)customAttributes[0]).Title;
            }
            return result;
        }

        public static string Description(Assembly assembly)
        {
            string result = string.Empty;
            if (assembly != null)
            {
                object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                    result = ((AssemblyDescriptionAttribute)customAttributes[0]).Description;
            }
            return result;
        }

        public static string Company(Assembly assembly)
        {
            string result = string.Empty;
            if (assembly != null)
            {
                object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                    result = ((AssemblyCompanyAttribute)customAttributes[0]).Company;
            }
            return result;
        }

        public static string Product(Assembly assembly)
        {
            string result = string.Empty;
            if (assembly != null)
            {
                object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                    result = ((AssemblyProductAttribute)customAttributes[0]).Product;
            }
            return result;
        }

        public static string Copyright(Assembly assembly)
        {
            string result = string.Empty;
            if (assembly != null)
            {
                object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                    result = ((AssemblyCopyrightAttribute)customAttributes[0]).Copyright;
            }
            return result;
        }

        public static string Trademark(Assembly assembly)
        {
            string result = string.Empty;
            if (assembly != null)
            {
                object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyTrademarkAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                    result = ((AssemblyTrademarkAttribute)customAttributes[0]).Trademark;
            }
            return result;
        }

        public static string AssemblyVersion(Assembly assembly)
        {
            if (assembly == null)
                return string.Empty;
            return assembly.GetName().Version.ToString();
        }

        public static string FileVersion(Assembly assembly)
        {
            if (assembly == null)
                return string.Empty;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileVersion;
        }

        public static string Guid(Assembly assembly)
        {
            string result = string.Empty;
            if (assembly != null)
            {
                object[] customAttributes = assembly.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                    result = ((System.Runtime.InteropServices.GuidAttribute)customAttributes[0]).Value;
            }
            return result;
        }

        public static string FileName(Assembly assembly)
        {
            if (assembly == null)
                return string.Empty;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.OriginalFilename;
        }

        public static string FilePath(Assembly assembly)
        {
            if (assembly == null)
                return string.Empty;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.FileName;
        }
    }
}
