using ChompGame.Data;
using ChompGame.Graphics;
using System.Linq;

namespace ChompGame.GameSystem
{
    class SpritesModule : ScanlineGraphicsModule
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
            return new Sprite(_sprite0Address + GameSystem.Specs.BytesPerSprite * index, GameSystem.Memory, Specs);
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            _sprite0Address = builder.CurrentAddress;
            Sprites = builder.AddSprite(Specs.MaxSprites);
            ScanlineSprites = builder.AddBytes(Specs.SpritesPerScanline);
        }

        public override void OnHBlank()
        {
            var commands = _coreGraphicsModule.ScanlineDrawCommands[Layer];
            commands.BeginAddDrawInstructions();
            DrawInstructionAddressOffset.Value = 0;
            FillScanlineSprites();

            DrawInstructionAddressOffset.Value = 0;
            PatternTablePoint.Reset();

            var tilePoint = new ByteGridPoint(Specs.PatternTableTilesAcross, Specs.PatternTableTilesDown);
            var nextPatternTablePoint = new ByteGridPoint(Specs.PatternTableWidth, Specs.PatternTableHeight);

            int currentRow = 0;
            int commandCount = 0;

            PaletteSwitch paletteSwitch = null;

            int i = 0;
            for (i = 0; i < ScanlineSprites.Length && ScanlineSprites[i] != 255; i++)
            {
                commands.CurrentPaletteSwitch.Value = 0;
                
                var sprite = new Sprite(_sprite0Address + ScanlineSprites[i], GameSystem.Memory, GameSystem.Specs);

                paletteSwitch = new PaletteSwitch(commands.CurrentPaletteSwitch.Address + 1 + i, GameSystem.Memory);
                paletteSwitch.CommandCount = (byte)commandCount;
                paletteSwitch.Palette = sprite.Palette;

                commandCount += commands.AddDrawCommand(false, (byte)(sprite.X - currentRow));

                tilePoint.Index = sprite.Tile;

                if (sprite.Orientation == Orientation.Horizontal)
                    throw new System.NotImplementedException();
                else
                {
                    if (ScreenPoint.Y >= sprite.Y + Specs.TileHeight)
                        tilePoint.Index += (sprite.Tile2Offset + Specs.PatternTableTilesAcross-1);
                }

                int row = (ScreenPoint.Y - sprite.Y) % Specs.TileHeight;

                nextPatternTablePoint.X = (byte)(tilePoint.X * Specs.TileWidth);
                nextPatternTablePoint.Y = (byte)((tilePoint.Y * Specs.TileHeight) + row);

                commandCount += commands.AddTileMoveCommand(nextPatternTablePoint.Index, PatternTablePoint.Index);
                commandCount += commands.AddDrawCommand(true, (byte)Specs.TileWidth);

                currentRow = sprite.X + Specs.TileWidth;
                nextPatternTablePoint.Advance(Specs.TileWidth);
                commandCount += commands.AddTileMoveCommand(0, nextPatternTablePoint.Index);
                PatternTablePoint.Index = 0;
            }

            paletteSwitch = new PaletteSwitch(commands.CurrentPaletteSwitch.Address + 1 + i, GameSystem.Memory);
            paletteSwitch.CommandCount = 255;
            paletteSwitch.Palette = 0;

            commands.AddDrawCommand(false, (byte)(Specs.ScreenWidth - currentRow));

            if (DrawInstructionAddressOffset.Value == 0)
                commandCount += commands.AddDrawCommand(false, (byte)Specs.ScreenWidth);

            DrawInstructionAddressOffset.Value = 255;
            DrawHoldCounter.Value = 0;
            PatternTablePoint.Reset();

            DrawInstructionAddressOffset.Value = 255;
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
