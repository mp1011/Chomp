using ChompGame.Data;
using ChompGame.Graphics;
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

        public GameByte CurrentPaletteAddress { get; private set; }

        public Palette GetCurrentPalette() => new Palette(Specs, _graphicsMemoryBegin + CurrentPaletteAddress.Value, GameSystem.Memory);

        private int _graphicsMemoryBegin;

        public CoreGraphicsModule(MainSystem gameSystem) : base(gameSystem) 
        {
            _screenData = new Color[gameSystem.Specs.ScreenWidth * gameSystem.Specs.ScreenHeight];
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            _graphicsMemoryBegin = builder.CurrentAddress;
            builder.AddBytes(8); //todo, adjust based on color needs

            CurrentPaletteAddress = builder.AddByte();
            CurrentColorIndex = builder.AddByte();
            PatternTable = builder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            
            if(Specs.ScreenWidth==256 && Specs.ScreenHeight==256)
                ScreenPoint = builder.AddFullGridPoint();
            else 
                ScreenPoint = builder.AddGridPoint((byte)Specs.ScreenWidth, (byte)Specs.ScreenHeight, Specs.ScreenPointMask);

            ScanlineDrawCommands = Enumerable.Range(0, Specs.ScanlineDrawPlanes)
                .Select(p => new ScanlineDrawCommands(builder, PatternTable, Specs))
                .ToArray();
        }

        public override void OnStartup()
        {
            var palette = GetCurrentPalette();
            palette.SetColor(0, 0);
            palette.SetColor(1, 1);
            palette.SetColor(2, 2);
            palette.SetColor(3, 3);

        }

        public void DrawFrame(SpriteBatch spriteBatch, Texture2D canvas)
        {
            ScreenPoint.Reset();
            GameSystem.OnHBlank();
            byte colorIndex = 0;
            byte planeColor = 0;

            var palette = GetCurrentPalette();

            for (int i = 0; i < _screenData.Length; i++)
            {
                //todo, optimize/generalize
                if (ScanlineDrawCommands.Length == 2)
                {
                    colorIndex = ScanlineDrawCommands[1].Update();
                    planeColor = ScanlineDrawCommands[0].Update();
                }
                else
                {
                    colorIndex = ScanlineDrawCommands[0].Update();
                }

                if (colorIndex == 0)
                    colorIndex = planeColor;

                _screenData[i] = palette[colorIndex];
               
                if (ScreenPoint.Next())
                    GameSystem.OnHBlank();
            }

            GameSystem.OnVBlank();
            canvas.SetData(_screenData);
            spriteBatch.Draw(canvas, Vector2.Zero, Color.White);
        }

    }
}
