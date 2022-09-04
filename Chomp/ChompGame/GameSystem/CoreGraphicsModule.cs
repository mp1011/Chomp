using ChompGame.Data;
using ChompGame.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace ChompGame.GameSystem
{
    public class CoreGraphicsModule : Module
    {
        private Color[] _screenData;
        public GameByteGridPoint ScreenPoint { get; private set; }
        public GameByte CurrentColorIndex { get; private set; }
        public NBitPlane PatternTable { get; private set; }      
        public ScanlineDrawBuffer ScanlineDrawBuffer { get; private set; }

        public Palette GetPalette(byte index) => new Palette(Specs, _graphicsMemoryBegin + (Specs.BytesPerPalette * index), GameSystem.Memory);

        private int _graphicsMemoryBegin;

        public CoreGraphicsModule(MainSystem gameSystem) : base(gameSystem) 
        {
            _screenData = new Color[gameSystem.Specs.ScreenWidth * gameSystem.Specs.ScreenHeight];
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            _graphicsMemoryBegin = builder.CurrentAddress;
            builder.AddBytes(Specs.NumPalettes * Specs.BytesPerPalette);
            CurrentColorIndex = builder.AddByte();
            PatternTable = builder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            
            if(Specs.ScreenWidth==256 && Specs.ScreenHeight==256)
                ScreenPoint = builder.AddFullGridPoint();
            else 
                ScreenPoint = builder.AddGridPoint((byte)Specs.ScreenWidth, (byte)Specs.ScreenHeight, Specs.ScreenPointMask);

            switch(Specs.BitsPerPixel)
            {
                case 2:
                    ScanlineDrawBuffer = new TwoBitPixelScanlineDrawBuffer(builder, Specs);
                    break;
                default:
                    throw new Exception("Unsupported bits per pixel");
            }
        }

        public void WriteTileToScanlineBuffer(int startIndex, PatternTablePoint patternTablePoint)
        {
            for(int i = 0; i < Specs.TileWidth; i++)
            {
                ScanlineDrawBuffer[startIndex + i] = PatternTable[patternTablePoint.PixelIndex];
                patternTablePoint.PixelIndex++;
            }
        }

        public override void OnStartup()
        {
            var palette = GetPalette(0);
            palette.SetColor(0, 0);
            palette.SetColor(1, 1);
            palette.SetColor(2, 2);
            palette.SetColor(3, 3);

        }

        public void DrawFrame(SpriteBatch spriteBatch, Texture2D canvas)
        {
            ScreenPoint.Reset();
            GameSystem.OnHBlank();

            byte nextPaletteIndex = 0;
            byte paletteIndex = 0;
            var palette = GetPalette(0);

            int scanlineColumn = 0;

            for (int i = 0; i < _screenData.Length; i++)
            {
                nextPaletteIndex = GameSystem.GetPalette(i);
                if(nextPaletteIndex != paletteIndex)
                {
                    paletteIndex = nextPaletteIndex;
                    palette = GetPalette(nextPaletteIndex);
                }

                var color = palette[ScanlineDrawBuffer[scanlineColumn]];
                _screenData[i] = color;
                scanlineColumn++;

                if (ScreenPoint.Next())
                {
                    scanlineColumn = 0;
                    GameSystem.OnHBlank();
                }
            }

            GameSystem.OnVBlank();
            canvas.SetData(_screenData);
            spriteBatch.Draw(canvas, Vector2.Zero, Color.White);
        }

    }
}
