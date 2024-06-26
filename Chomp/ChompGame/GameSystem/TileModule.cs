﻿using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.GameSystem
{
    public class TileModule : ScanlineGraphicsModule
    {            
        public NBitPlane NameTable { get; private set; }

        public NBitPlane AttributeTable { get; private set; }

        public TileModule(MainSystem gameSystem) : base(gameSystem) 
        { 
        }

        public override void OnStartup()
        {
           
        }

        public override void BuildMemory(SystemMemoryBuilder builder)
        {
            base.BuildMemory(builder);
            NameTable = builder.AddNBitPlane(Specs.NameTableBitPlanes, Specs.NameTableWidth, Specs.NameTableHeight);
            AttributeTable = builder.AddNBitPlane(Specs.AttributeTableBitsPerBlock, Specs.NameTableWidth / Specs.AttributeTableBlockSize,
                Specs.NameTableHeight / Specs.AttributeTableBlockSize);
        }

        public override void OnHBlank()
        {
            var nameTablePoint = new ByteGridPoint(
                Specs.NameTableWidth, 
                Specs.NameTableHeight);

            var patternTableTilePoint = new ByteGridPoint(
                Specs.PatternTableTilesAcross, 
                Specs.PatternTableTilesDown);

            var patternTablePoint = new ByteGridPoint(
                Specs.PatternTableWidth,
                Specs.PatternTableHeight);

            nameTablePoint.X = (byte)(Scroll.X / Specs.TileWidth);
            nameTablePoint.Y = (byte)((ScreenPoint.Y + Scroll.Y) / Specs.TileHeight);

            patternTableTilePoint.Index = NameTable[nameTablePoint.Index];
          
            int col = Scroll.X % Specs.TileWidth;
            int row = (ScreenPoint.Y + Scroll.Y) % Specs.TileHeight; //todo, scroll
            int remainingTilePixels = Specs.TileWidth - col;

            patternTablePoint.X = (byte)(patternTableTilePoint.X * Specs.TileWidth + col);
            patternTablePoint.Y = (byte)(patternTableTilePoint.Y * Specs.TileHeight + row);

            for (int i =0; i < Specs.ScreenWidth; i++)
            {
                _coreGraphicsModule.BackgroundScanlineDrawBuffer[i] = _coreGraphicsModule.BackgroundPatternTable[patternTablePoint.Index];

                remainingTilePixels--;
                if(remainingTilePixels == 0)
                {
                    nameTablePoint.X++;
                    patternTableTilePoint.Index = NameTable[nameTablePoint.Index];
                    remainingTilePixels = Specs.TileWidth;
                    patternTablePoint.X = (byte)(patternTableTilePoint.X * Specs.TileWidth);
                    patternTablePoint.Y = (byte)(patternTableTilePoint.Y * Specs.TileHeight + row);
                }
                else
                {
                    patternTablePoint.X++;
                }
            }          
        }

        public override void OnVBlank()
        {
        }
    }
}
