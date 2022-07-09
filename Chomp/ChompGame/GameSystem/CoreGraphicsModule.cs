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

        public GameByte CurrentPaletteIndex { get; private set; }

        public Palette GetCurrentPalette() => new Palette(Specs, _graphicsMemoryBegin + (Specs.BytesPerPalette * CurrentPaletteIndex.Value), GameSystem.Memory);

        private int _graphicsMemoryBegin;

        public CoreGraphicsModule(MainSystem gameSystem) : base(gameSystem) 
        {
            _screenData = new Color[gameSystem.Specs.ScreenWidth * gameSystem.Specs.ScreenHeight];
        }

        public Palette SetCurrentPalette(int index)
        {
            CurrentPaletteIndex.Value = (byte)index;
            return GetCurrentPalette();
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            _graphicsMemoryBegin = builder.CurrentAddress;
            builder.AddBytes(Specs.NumPalettes * Specs.BytesPerPalette);

            CurrentPaletteIndex = builder.AddByte();
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
            byte color0=0, color1=0;

            for (int i = 0; i < _screenData.Length; i++)
            {
                //todo, optimize/generalize
                if (ScanlineDrawCommands.Length == 2)
                {
                    color1 = ScanlineDrawCommands[1].Update();
                    color0 = ScanlineDrawCommands[0].Update();
                }
                else
                {
                    color0 = ScanlineDrawCommands[0].Update();
                }

                if (color1 != 0)
                {
                    CurrentPaletteIndex.Value = ScanlineDrawCommands[1].PaletteIndex.Value;
                    var palette = GetCurrentPalette();
                    _screenData[i] = palette[color1];
                }
                else
                {
                    CurrentPaletteIndex.Value = ScanlineDrawCommands[0].PaletteIndex.Value;
                    var palette = GetCurrentPalette();
                    _screenData[i] = palette[color0];
                }
               
                if (ScreenPoint.Next())
                    GameSystem.OnHBlank();
            }

            GameSystem.OnVBlank();
            canvas.SetData(_screenData);
            spriteBatch.Draw(canvas, Vector2.Zero, Color.White);
        }

    }
}
