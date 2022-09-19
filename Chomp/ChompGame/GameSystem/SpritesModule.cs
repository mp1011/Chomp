using ChompGame.Data;
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

        public override void OnStartup()
        {
        }

        public Sprite GetSprite(int index)
        {
            return new Sprite(_sprite0Address + GameSystem.Specs.BytesPerSprite * index, GameSystem.Memory, Specs, Scroll);
        }

        public Sprite GetScanlineSprite(int index)
        {
            return new Sprite(_sprite0Address + ScanlineSprites[index], GameSystem.Memory, GameSystem.Specs, Scroll);
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            base.BuildMemory(builder);
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
                byte row = (byte)(ScreenPoint.Y - sprite.Y); //todo, scroll

                patternTableTilePoint.Index = sprite.Tile;

                if (row >= Specs.TileHeight)
                {
                    patternTableTilePoint.Y++;
                    patternTableTilePoint.X += (byte)(sprite.Tile2Offset-1);
                    row = (byte)(row - Specs.TileHeight);
                }

                patternTablePoint.X = (byte)(patternTableTilePoint.X * Specs.TileWidth);
                patternTablePoint.Y = (byte)(patternTableTilePoint.Y * Specs.TileHeight + row);

                for (int col = 0; col < Specs.TileWidth; col++)
                {
                    var pixel = _coreGraphicsModule.PatternTable[patternTablePoint.Index];
                    if (pixel != 0)
                    {
                        int drawCol; 
                        if(sprite.FlipX)
                            drawCol = (sprite.X + sprite.Width - col - 1) - Scroll.X;
                        else 
                            drawCol= (sprite.X + col) - Scroll.X;

                        if (drawCol >= 0 && drawCol < Specs.ScreenWidth)
                            _coreGraphicsModule.ScanlineDrawBuffer[drawCol] = pixel;
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

            for(byte spriteIndex = 0; spriteIndex < Specs.MaxSprites; spriteIndex++)
            {
                var sprite = GetSprite(spriteIndex);
                if(sprite.Tile == 0 
                    || !sprite.IntersectsScanline(ScreenPoint.Y))
                {
                    continue;
                }

                ScanlineSprites[scanlineSpriteIndex] = (byte)(sprite.Address - _sprite0Address);
                scanlineSpriteIndex++;
                if (scanlineSpriteIndex == Specs.SpritesPerScanline)
                    break;
            }

            if(scanlineSpriteIndex > 1)
            {
                throw new System.NotImplementedException("need to sort by X");
            }

            if(scanlineSpriteIndex < Specs.SpritesPerScanline)
            {
                ScanlineSprites[scanlineSpriteIndex] = 255;
            }
        }
    }
}
