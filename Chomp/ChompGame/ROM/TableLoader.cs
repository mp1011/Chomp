using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.Helpers;
using System.IO;

namespace ChompGame.ROM
{
    public enum ContentFolder
    {
        PatternTables,
        NameTables
    }

    public class DiskFile 
    {
        public DiskFile(ContentFolder contentFolder, string file)
        {
            ContentFolder = contentFolder;
            File = file;
        }

        public ContentFolder ContentFolder { get; set; }
        public string File { get; set; }

        public FileInfo GetFile()
        {
            var folder = FolderHelper.GetContentFolder()
                                     .GetChild(ContentFolder.ToString());

            return folder.GetFile(File);
        }
    }

    public interface ITableLoader<TKey, TPlane,TData> where TPlane : IGrid<TData> 
    {
        void Load(TKey key, TPlane plane);
    }

    public abstract class DiskTableLoader<TPlane, TData> 
        : ITableLoader<DiskFile, TPlane, TData>
        where TPlane : IGrid<TData>
    {
        public void Load(DiskFile key, TPlane plane)
        {
            var text = GetFileContents(key);
            LoadFromString(text, plane);
        }

        protected abstract void LoadFromString(string text, TPlane plane);

        private string GetFileContents(DiskFile file)
        {
            return File.ReadAllText(file.GetFile().FullName);
        }
    }

    public class DiskBitPlaneLoader : DiskTableLoader<BitPlane, bool>
    {
        protected override void LoadFromString(string text, BitPlane plane)
        {
            plane.SetFromString(text);
        }
    }

    public class DiskNBitPlaneLoader : DiskTableLoader<NBitPlane, byte>
    {
        protected override void LoadFromString(string text, NBitPlane plane)
        {
            plane.SetFromString(text);
        }
    }
}
