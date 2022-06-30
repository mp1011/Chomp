using ChompGame.Data;
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
            return new Sprite(_sprite0Address + Sprite.Bytes * index, GameSystem.Memory, Specs);
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

            for (int i = 0; i < ScanlineSprites.Length && ScanlineSprites[i] != 255; i++)
            {
                var sprite = new Sprite(_sprite0Address + ScanlineSprites[i], GameSystem.Memory, GameSystem.Specs);
                commands.AddDrawCommand(false, (byte)(sprite.X - currentRow));

                tilePoint.Index = sprite.Tile;

                int row = (ScreenPoint.Y - sprite.Y) % Specs.TileHeight;

                nextPatternTablePoint.X = (byte)(tilePoint.X * Specs.TileWidth);
                nextPatternTablePoint.Y = (byte)((tilePoint.Y * Specs.TileHeight) + row);

                commands.AddTileMoveCommand(nextPatternTablePoint.Index, PatternTablePoint.Index);
                commands.AddDrawCommand(true, (byte)Specs.TileWidth);

                currentRow = sprite.X + Specs.TileWidth;
                nextPatternTablePoint.Advance(Specs.TileWidth);
                commands.AddTileMoveCommand(0, nextPatternTablePoint.Index);
                PatternTablePoint.Index = 0;
            }

            commands.AddDrawCommand(false, (byte)(Specs.ScreenWidth - currentRow));

            if (DrawInstructionAddressOffset.Value == 0)
                commands.AddDrawCommand(false, (byte)Specs.ScreenWidth);

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
