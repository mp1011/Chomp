using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Graphics;
using ChompGame.MainGame;
using System.Collections.Generic;
using System.Linq;

namespace ChompGame.GameSystem
{
    public class SpritesModule : ScanlineGraphicsModule
    {
        public SpritesModule(MainSystem mainSystem) : base(mainSystem)
        {
        }

        private int _sprite0Address;

        public GameByteArray ScanlineSprites { get; private set; }

        public ScanlineSpritePixelPriority ScanlineSpritePixelPriority { get; private set; }

        public override void OnStartup()
        {
        }

        public Sprite GetSprite(int index)
        {
            return new Sprite(_sprite0Address + Sprite.ByteLength * index, GameSystem.Memory, Specs, Scroll);
        }

        public byte GetFreeSpriteIndex()
        {
            for(byte b = 0; b < Specs.MaxSprites; b++)
            {
                if (GetSprite(b).Tile == 0)
                    return b;
            }

            return 255;
        }

        public Sprite GetScanlineSprite(int index)
        {
            return new Sprite(_sprite0Address + ScanlineSprites[index], GameSystem.Memory, GameSystem.Specs, Scroll);
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            base.BuildMemory(builder);

            ScanlineSpritePixelPriority = new ScanlineSpritePixelPriority(builder, Specs);

            _sprite0Address = builder.CurrentAddress;
            builder.AddSprite(Specs.MaxSprites, this);
            ScanlineSprites = new GameByteArray(builder.CurrentAddress, builder.Memory);
            builder.AddBytes(Specs.SpritesPerScanline);
            builder.AddByte(255);
        }

        public override void OnHBlank()
        {
            FillScanlineSprites();

            var patternTableTilePoint = new ByteGridPoint(
                Specs.PatternTableTilesAcross,
                Specs.PatternTableTilesDown);

            var patternTablePoint = new ByteGridPoint(
                Specs.PatternTableWidth,
                Specs.PatternTableHeight);

            for (int i = 0; ScanlineSprites[i] != 255; i++)
            {
                var sprite = new Sprite(_sprite0Address + ScanlineSprites[i], GameSystem.Memory, GameSystem.Specs, Scroll);
                byte row = (byte)(ScreenPoint.Y + Scroll.Y - sprite.Y); //todo, scroll

                //todo, avoid hard coding sprite start
                patternTableTilePoint.Index = Constants.SpriteStartIndex + sprite.Tile;

                if (row >= Specs.TileHeight)
                {
                    patternTableTilePoint.Y++;
                    patternTableTilePoint.X += (byte)(sprite.Tile2Offset-1);
                    row = (byte)(row - Specs.TileHeight);
                }

                patternTablePoint.X = (byte)(patternTableTilePoint.X * Specs.TileWidth);
                patternTablePoint.Y = (byte)(patternTableTilePoint.Y * Specs.TileHeight + row);

                for (int col = 0; col < sprite.Width; col++)
                {
                    int adjCol = sprite.FlipX ? sprite.Width - (col + 1) : col;

                    var pixel = _coreGraphicsModule.PatternTable[patternTablePoint.Index];
                    if (pixel != 0)
                    {
                        ScanlineSpritePixelPriority.Set(i, adjCol, true);

                        int drawCol = (sprite.X + adjCol) - Scroll.X;                          
                        drawCol = drawCol.NMod(Specs.NameTablePixelWidth);

                        if (drawCol >= 0 && drawCol < Specs.ScreenWidth)
                        {                  
                            _coreGraphicsModule.ScanlineDrawBuffer[drawCol] = pixel;
                        }
                    }
                    else
                    {
                        ScanlineSpritePixelPriority.Set(i, adjCol, false);
                    }

                    patternTablePoint.X++;
                }      
            }
        }


        public override void OnVBlank()
        {
        }

        private void FillScanlineSprites()
        {
            int scanlineSpriteIndex = 0;

            List<Sprite> scanlineSprites = new List<Sprite>();

            for (byte spriteIndex = 0; spriteIndex < Specs.MaxSprites; spriteIndex++)
            {
                var sprite = GetSprite(spriteIndex);
                if (!sprite.Visible
                    || !sprite.IntersectsScanline((byte)(ScreenPoint.Y + Scroll.Y)))
                {
                    continue;
                }

                scanlineSprites.Add(sprite);
            }

            foreach(var orderedSprite in scanlineSprites
                .OrderBy(p=>p.X))
            {
                ScanlineSprites[scanlineSpriteIndex] = (byte)(orderedSprite.Address - _sprite0Address);
                scanlineSpriteIndex++;
                if (scanlineSpriteIndex == Specs.SpritesPerScanline)
                    break;
            }
                
            if(scanlineSpriteIndex < Specs.SpritesPerScanline)
            {
                ScanlineSprites[scanlineSpriteIndex] = 255;
            }
        }
    }
}
