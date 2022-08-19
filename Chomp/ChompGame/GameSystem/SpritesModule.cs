using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.Graphics;
using System.Linq;

namespace ChompGame.GameSystem
{
    public class SpritesModule : ScanlineGraphicsModule
    {
        public SpritesModule(MainSystem mainSystem) : base(mainSystem)
        {
        }

        private int _sprite0Address;

        public Sprite[] Sprites { get; private set; }
        public GameByte[] ScanlineSprites { get; private set; }



        public override void OnStartup()
        {
        }

        public Sprite GetSprite(int index)
        {
            return new Sprite(_sprite0Address + GameSystem.Specs.BytesPerSprite * index, GameSystem.Memory, Specs, Scroll);
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            base.BuildMemory(builder);
            _sprite0Address = builder.CurrentAddress;
            Sprites = builder.AddSprite(Specs.MaxSprites, this);
            ScanlineSprites = builder.AddBytes(Specs.SpritesPerScanline);
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

            for (int i = 0; i < ScanlineSprites.Length && ScanlineSprites[i] != 255; i++)
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
            var scanlineSprites = Sprites.Where(p => p.Tile > 0 && p.IntersectsScanline(ScreenPoint.Y))
                .OrderBy(p => p.X)
                .Take(Specs.SpritesPerScanline)
                .ToArray();

            int sprite0Address = Sprites[0].Address;

            for (int i = 0; i < Specs.SpritesPerScanline; i++)
            {
                if (i < scanlineSprites.Length)
                    ScanlineSprites[i].Value = (byte)(scanlineSprites[i].Address - sprite0Address);
                else
                    ScanlineSprites[i].Value = 255;
            }
        }
    }
}
