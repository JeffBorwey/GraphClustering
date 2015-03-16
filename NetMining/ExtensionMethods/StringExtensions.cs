using System.Net.NetworkInformation;

namespace NetMining.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string GetFolder(this string filename)
        {
            int lastIndex = filename.LastIndexOf('\\');
            if (lastIndex == -1)
                return ".\\";
            return filename.Substring(0, filename.LastIndexOf('\\'));
        }

        public static string GetShortFilename(this string filename)
        {
            int lastIndex = filename.LastIndexOf('\\');
            if (lastIndex == -1)
                return filename;
            return filename.Substring(filename.LastIndexOf('\\') + 1);
        }

        public static string GetFilenameNoExtension(this string filename)
        {
            int lastIndex = filename.LastIndexOf('.');
            if (lastIndex == -1)
                return filename;
            return filename.Substring(0, lastIndex);
        }

        public static string GetFileExtension(this string filename)
        {
            int lastIndex = filename.LastIndexOf('.');
            if (lastIndex == -1)
                return "";
            return filename.Substring(filename.LastIndexOf('\\') + 1);
        }
    }
}
