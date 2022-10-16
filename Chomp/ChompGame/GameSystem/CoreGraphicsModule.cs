using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ChompGame.GameSystem
{
    public class CoreGraphicsModule : Module
    {

        private SpritesModule _spritesModule;
        private TileModule _tileModule;

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

            _spritesModule = GameSystem.GetModule<SpritesModule>();
            _tileModule = GameSystem.GetModule<TileModule>();
        }

        public void DrawFrame(SpriteBatch spriteBatch, Texture2D canvas)
        {
            ScreenPoint.Reset();
            GameSystem.OnHBlank();

            bool drawingSprite = false;
            byte nextPaletteIndex = _tileModule.BackgroundPaletteIndex;
            byte paletteIndex = nextPaletteIndex;
            var palette = GetPalette(nextPaletteIndex);

            int scanlineColumn = 0;

            byte spriteColumn = 0;
            int scanlineSpriteIndex = 0;

            Sprite nextSprite = _spritesModule.GetScanlineSprite(scanlineSpriteIndex);

            for (int i = 0; i < _screenData.Length; i++)
            {
                if(_spritesModule.ScanlineSprites[scanlineSpriteIndex] != 255)
                {
                    if ((ScreenPoint.X + _spritesModule.Scroll.X).NMod(Specs.NameTablePixelWidth) == nextSprite.X.NMod(Specs.NameTablePixelWidth))
                    {
                        nextPaletteIndex = nextSprite.Palette;
                        drawingSprite = true;
                        spriteColumn = 0;
                    }
                    else if ((ScreenPoint.X + _spritesModule.Scroll.X).NMod(Specs.NameTablePixelWidth) == nextSprite.Right.NMod(Specs.NameTablePixelWidth))
                    {
                        nextPaletteIndex = _tileModule.BackgroundPaletteIndex;
                        scanlineSpriteIndex++;
                        nextSprite = _spritesModule.GetScanlineSprite(scanlineSpriteIndex);
                        drawingSprite = false;
                    }
                }

                if(drawingSprite)
                {
                    bool pixelPriority = _spritesModule.ScanlineSpritePixelPriority.Get(scanlineSpriteIndex, spriteColumn);
                    if(pixelPriority)
                        nextPaletteIndex = nextSprite.Palette;
                    else
                        nextPaletteIndex = _tileModule.BackgroundPaletteIndex;
                }

                if (nextPaletteIndex != paletteIndex)
                {
                    paletteIndex = nextPaletteIndex;
                    palette = GetPalette(nextPaletteIndex);
                }

                var color = palette[ScanlineDrawBuffer[scanlineColumn]];
                _screenData[i] = color;

                scanlineColumn++;

                if (drawingSprite)
                    spriteColumn++;

                if (ScreenPoint.Next())
                {
                    scanlineColumn = 0;
                    GameSystem.OnHBlank();

                    scanlineSpriteIndex = 0;
                    nextSprite = _spritesModule.GetScanlineSprite(scanlineSpriteIndex);
                }
            }

            GameSystem.OnVBlank();
            canvas.SetData(_screenData);
            spriteBatch.Draw(canvas, Vector2.Zero, Color.White);
        }

    }
}
