using System;
using System.IO;

namespace ChompGame.Extensions
{
    public static class FileExtensions
    {
        public static DirectoryInfo GetAncestor(this DirectoryInfo directory, string name)
        {
            while(directory != null)
            {
                if (directory.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return directory;

                directory = directory.Parent;
            }

            return null;
        }

        public static DirectoryInfo GetChild(this DirectoryInfo directoryInfo, string name)
        {
            return new DirectoryInfo($"{directoryInfo.FullName}\\{name}");
        }

        public static FileInfo GetFile(this DirectoryInfo directoryInfo, string name)
        {
            return new FileInfo($"{directoryInfo.FullName}\\{name}");
        }
    }
}
