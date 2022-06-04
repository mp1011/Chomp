using ChompGame.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

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
            var projectFolder = _binFolder.GetAncestor("ChompGame");
            return projectFolder.GetChild("Content");
        }
    }
}
