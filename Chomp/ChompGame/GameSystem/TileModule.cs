using ChompGame.Data;
using ChompGame.ROM;
using System.Linq;

namespace ChompGame.GameSystem
{
    public class TileModule : Module, IHBlankHandler, IVBlankHandler
    {
        private CoreGraphicsModule _coreGraphicsModule => GameSystem.CoreGraphicsModule;

        public GameByteGridPoint Scroll { get; private set; }
        public GameByteGridPoint PatternTablePoint => _coreGraphicsModule.PatternTablePoint;
        public GameByte DrawInstructionAddressOffset => _coreGraphicsModule.DrawInstructionAddressOffset;
        public GameByte DrawHoldCounter => _coreGraphicsModule.DrawHoldCounter;
        public GameByteGridPoint ScreenPoint => _coreGraphicsModule.ScreenPoint;
        public NBitPlane NameTable { get; private set; }
        public GameByte SpritesAddress { get; private set; }
        public Sprite[] Sprites { get; private set; }

        public TileModule(MainSystem gameSystem) : base(gameSystem) 
        { 
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            Scroll = builder.AddGridPoint(
                (byte)(Specs.NameTableWidth * Specs.TileWidth),
                (byte)(Specs.NameTableHeight * Specs.TileHeight), 
                Specs.ScrollXMask, 
                Specs.ScrollYMask);
            
            NameTable = builder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);

            SpritesAddress = builder.AddByte();
            Sprites = builder.AddSprite(Specs.MaxSprites);
        }

        public override void OnStartup()
        {
            var nameTableLoader = new DiskNBitPlaneLoader();
            nameTableLoader.Load(
               new DiskFile(ContentFolder.NameTables, "test.nt"),
               NameTable);
        }

        private Sprite[] GetScanlineSprites()
        {
            return Sprites.Where(p => p.Tile > 0 && p.IntersectsScanline(ScreenPoint.Y))
                .OrderBy(p => p.X)
                .Take(Specs.SpritesPerScanline)
                .ToArray();
        }

        public void OnHBlank()
        {
            var scanlineSprites = GetScanlineSprites();
            int nextScanlineSpriteIndex = 0;

            DrawInstructionAddressOffset.Value = 0;
            PatternTablePoint.Reset();

            var nameTablePoint = new ByteGridPoint(Specs.NameTableWidth, Specs.NameTableHeight);
            nameTablePoint.X = (byte)(Scroll.X / Specs.TileHeight);
            nameTablePoint.Y = (byte)((ScreenPoint.Y+Scroll.Y) / Specs.TileHeight);

            int row = (ScreenPoint.Y + Scroll.Y) % Specs.TileHeight;
            int col = (ScreenPoint.X + Scroll.X) % Specs.TileWidth;
            var tilePoint = new ByteGridPoint(Specs.PatternTableTilesAcross, Specs.PatternTableTilesDown);
            var nextPatternTablePoint = new ByteGridPoint(Specs.PatternTableWidth, Specs.PatternTableHeight);
 
            int colsRemaining = Specs.ScreenWidth / Specs.TileWidth;
            if (col != 0)
                colsRemaining++;

            int screenColumn = 0;

            while(colsRemaining-- > 0)
            {
                if (nextScanlineSpriteIndex < scanlineSprites.Length
                    && screenColumn == scanlineSprites[nextScanlineSpriteIndex].X)
                {
                    tilePoint.Index = scanlineSprites[nextScanlineSpriteIndex].Tile;
                    var spriteRow = ScreenPoint.Y-scanlineSprites[nextScanlineSpriteIndex].Y;
                    if (spriteRow >= 0 && spriteRow < Specs.TileHeight)
                    {
                        nextPatternTablePoint.X = (byte)((tilePoint.X * Specs.TileWidth) + col);
                        nextPatternTablePoint.Y = (byte)((tilePoint.Y * Specs.TileHeight) + row);
                    }
                    nextScanlineSpriteIndex++;
                }
                else
                {
                    tilePoint.Index = NameTable[nameTablePoint.Index];
                    nextPatternTablePoint.X = (byte)((tilePoint.X * Specs.TileWidth) + col);
                    nextPatternTablePoint.Y = (byte)((tilePoint.Y * Specs.TileHeight) + row);
                }

                PatternTablePoint.Advance(_coreGraphicsModule.AddMoveBrushToCommand(
                    destination: nextPatternTablePoint.Index,
                    currentOffset: PatternTablePoint.Index));

                var hold = Specs.TileWidth - col;

                if(nextScanlineSpriteIndex < scanlineSprites.Length
                    && screenColumn + hold > scanlineSprites[nextScanlineSpriteIndex].X)
                {
                    hold = scanlineSprites[nextScanlineSpriteIndex].X - screenColumn;
                }

                screenColumn += hold;
                PatternTablePoint.Advance(_coreGraphicsModule.AddDrawHoldCommand(hold));
                col = 0;

                nameTablePoint.NextColumn();
            }

            PatternTablePoint.X = 0;
            PatternTablePoint.Y = 0;
            DrawHoldCounter.Value = 0;
            DrawInstructionAddressOffset.Value = 0;
        }



        public void OnVBlank()
        {
            PatternTablePoint.X = 0;
            PatternTablePoint.Y = 0;
            DrawHoldCounter.Value = 0;
            DrawInstructionAddressOffset.Value = 0;
        }
    }
}
