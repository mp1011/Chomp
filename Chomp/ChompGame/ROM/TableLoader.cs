using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;

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

    public abstract class DiskBitmapLoader<TPlane, TData>
         : ITableLoader<DiskFile, TPlane, TData>
         where TPlane : IGrid<TData>
    {
        protected MainSystem _gameSystem;

        protected DiskBitmapLoader(MainSystem gameSystem)
        {
            _gameSystem = gameSystem;
        }

        public void Load(DiskFile file, TPlane plane)
        {
            using (var fs = file.GetFile().OpenRead())
            {
                using (var t = Texture2D.FromStream(_gameSystem.GraphicsDevice, fs))
                {
                    Color[] colorData = new Color[t.Width * t.Height];
                    t.GetData(colorData);
                    LoadFromColorData(colorData, plane);
                }
            }
        }

        public abstract void LoadFromColorData(Color[] colors, TPlane plane);
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

    public class DiskNBitPlaneBitmapLoader : DiskBitmapLoader<NBitPlane, byte>
    {
        public DiskNBitPlaneBitmapLoader(MainSystem gameSystem) : base(gameSystem)
        {
        }

        public override void LoadFromColorData(Color[] colors, NBitPlane plane)
        {
            Color[] indexColors = new Color[] { Color.Black, Color.Red, new Color(0,255,0,255), Color.Blue };

            var indices = colors
                .Select(c => Array.IndexOf(indexColors, c))
                .ToArray();

            if (indices.Any(p => p < 0))
                throw new Exception("Image contains invalid colors");

            for(int i = 0; i < indices.Length;i++)
            {
                plane[i] = (byte)indices[i];
            }
        }
    }
}
