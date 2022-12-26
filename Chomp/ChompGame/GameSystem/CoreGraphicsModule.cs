using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Graphics;
using ChompGame.MainGame;
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
        public NBitPlane PatternTable { get; private set; }      
        public ScanlineDrawBuffer ScanlineDrawBuffer { get; private set; }
        public Palette GetBackgroundPalette(byte index) => GetPalette(index);

        public Palette GetBackgroundPalette(GameByteGridPoint screenPoint)
        {
            int attrX = (screenPoint.X + _tileModule.Scroll.X) / (Specs.TileWidth * Specs.AttributeTableBlockSize);
            int attrY = (screenPoint.Y + _tileModule.Scroll.Y) / (Specs.TileHeight * Specs.AttributeTableBlockSize);

            byte index = _tileModule.AttributeTable[attrX, attrY];
            return GetPalette(index);
        }

        public Palette GetSpritePalette(byte index) => GetPalette((byte)(2 + index));

        private Palette GetPalette(byte index) => new Palette(Specs, _graphicsMemoryBegin + (Specs.BytesPerPalette * index), GameSystem.Memory);

        private int _graphicsMemoryBegin;

        public CoreGraphicsModule(MainSystem gameSystem) : base(gameSystem) 
        {
            _screenData = new Color[gameSystem.Specs.ScreenWidth * gameSystem.Specs.ScreenHeight];
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            _graphicsMemoryBegin = builder.CurrentAddress;
            builder.AddBytes(Specs.NumPalettes * Specs.BytesPerPalette);
            PatternTable = builder.AddNBitPlane(Specs.PatternTablePlanes, Specs.PatternTableWidth, Specs.PatternTableHeight);
            
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

            int scanlineColumn = 0;
            var palette = GetBackgroundPalette(ScreenPoint);

            for (int i = 0; i < _screenData.Length; i++)
            {
                //todo, don't need to compute this every pixel
                palette = GetBackgroundPalette(ScreenPoint);

                var color = palette[ScanlineDrawBuffer[scanlineColumn]];
                _screenData[i] = color;
                scanlineColumn++;

                if (ScreenPoint.Next())
                {
                    DrawSprites(i - scanlineColumn + 1);
                    scanlineColumn = 0;
                    GameSystem.OnHBlank();
                }
            }

            GameSystem.OnVBlank();
            canvas.SetData(_screenData);
            spriteBatch.Draw(canvas, Vector2.Zero, Color.White);
        }

        private void DrawSprites(int columnStart)
        {
            int scanlineSpriteIndex = 0;
            while(_spritesModule.ScanlineSprites[scanlineSpriteIndex] != 255)
            {
                Sprite sprite = _spritesModule.GetScanlineSprite(scanlineSpriteIndex);
                var palette = GetSpritePalette(sprite.Palette);

                for(int x = 0; x < sprite.Width; x++)
                {
                    int scanlineColumn = ((sprite.X - _spritesModule.Scroll.X) + x).NMod(Specs.NameTablePixelWidth);

                    if (scanlineColumn >= Specs.ScreenWidth)
                        continue;

                    if (_spritesModule.ScanlineSpritePixelPriority.Get(scanlineSpriteIndex, x))
                    {                        
                        var color = palette[ScanlineDrawBuffer[scanlineColumn]];
                        _screenData[columnStart + scanlineColumn] = color;                        
                    }
                }
                
                scanlineSpriteIndex++;
            }
        }

    }
}
