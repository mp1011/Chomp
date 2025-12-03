using ChompGame.Extensions;
using System.IO;
using System.Reflection;

namespace ChompGame.Helpers
{
    public static class FolderHelper
    {
        private static readonly DirectoryInfo _binFolder;

        static FolderHelper()
        {
            var file = new FileInfo(Assembly.GetExecutingAssembly().Location);
            _binFolder = file.Directory;
        }

        public static DirectoryInfo GetContentFolder()
        {
            var projectFolder = _binFolder.GetAncestor("ChompGame.Dev");
            return projectFolder.GetChild("Content");
        }
    }
}
