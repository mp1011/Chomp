using ChompGame.Data;
using ChompGame.Graphics;
using ChompGame.ROM;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace ChompGame.GameSystem
{
    public class CoreGraphicsModule : Module
    {
        private Color[] _screenData;
        public GameByteGridPoint ScreenPoint { get; private set; }
        public GameByte CurrentColorIndex { get; private set; }
        public ScanlineDrawCommands[] ScanlineDrawCommands { get; private set; }
        public NBitPlane PatternTable { get; private set; }
      
        public CoreGraphicsModule(MainSystem gameSystem) : base(gameSystem) 
        {
            _screenData = new Color[gameSystem.Specs.ScreenWidth * gameSystem.Specs.ScreenHeight];
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            CurrentColorIndex = builder.AddByte();
            PatternTable = builder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            ScreenPoint = builder.AddGridPoint((byte)Specs.ScreenWidth, (byte)Specs.ScreenHeight, Specs.ScreenPointMask);

            ScanlineDrawCommands = Enumerable.Range(0, Specs.ScanlineDrawPlanes)
                .Select(p => new ScanlineDrawCommands(builder, PatternTable, Specs))
                .ToArray();
        }

        public override void OnStartup()
        {
            var patternTableLoader = new DiskNBitPlaneLoader();
            patternTableLoader.Load(
                new DiskFile(ContentFolder.PatternTables, "test_4color.pt"),
                PatternTable);
        }

        private bool UpdatePlane(int scanlinePlane, int pixelIndex)
        {
            var colorIndex = ScanlineDrawCommands[scanlinePlane].Update();
            if (colorIndex > 0)
                _screenData[pixelIndex] = Specs.SystemColors[colorIndex];

            return colorIndex != 0;
        }

        public void DrawFrame(SpriteBatch spriteBatch, Texture2D canvas)
        {
            ScreenPoint.Reset();
            GameSystem.OnHBlank();

            bool drawn;

            for (int i = 0; i < _screenData.Length; i++)
            {
                switch(Specs.ScanlineDrawPlanes)
                {
                    case 1:
                        drawn = UpdatePlane(0, i);
                        break;
                    case 2:
                        drawn = UpdatePlane(1, i) || UpdatePlane(0, i);                        
                        break;
                    case 3:
                        drawn = UpdatePlane(2, i) || UpdatePlane(1, i) || UpdatePlane(0, i);
                        break;
                    case 4:
                        drawn = UpdatePlane(3, i) || UpdatePlane(2, i) || UpdatePlane(1, i) || UpdatePlane(0, i);
                        break;
                    default: throw new System.Exception("Unsupported number of scanline planes");
                }

                if (!drawn)
                    _screenData[i] = Specs.SystemColors[0];

                if (ScreenPoint.Next())
                    GameSystem.OnHBlank();
            }

            GameSystem.OnVBlank();
            canvas.SetData(_screenData);
            spriteBatch.Draw(canvas, Vector2.Zero, Color.White);
        }
    }
}
