using ChompGame.Data;
using ChompGame.GameSystem;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SceneModels
{
    class LevelBuilder
    {
        private Specs _specs;
        private SceneDefinition _sceneDefinition;
        private TileModule _tileModule;

        public LevelBuilder(SceneDefinition sceneDefinition, TileModule tileModule, Specs specs)
        {
            _sceneDefinition = sceneDefinition;
            _specs = specs;
            _tileModule = tileModule;
        }

        private void FillStatusBarTopLine()
        {
            _tileModule.NameTable
                .SetFromString(@"01277734562777777");            
        }

        public void BuildBackgroundNametable()
        {
            FillStatusBarTopLine();

            int layerABegin = 2 + _sceneDefinition.BeginTiles + _sceneDefinition.ParallaxLayerABeginTile;
            int layerBBegin = layerABegin + _sceneDefinition.ParallaxLayerBTiles;
            int layerCBegin = layerBBegin + _sceneDefinition.ParallaxLayerATiles;
         

            //mountain layer 1
            _tileModule.NameTable.SetFromString(0, layerBBegin,
                @"00000500000005000000050000000500
                        34121625341216253412162534121625");

            //mountain layer 2
            _tileModule.NameTable.SetFromString(0, layerCBegin,
              @"00000500001200000050000120000000
                      12341623416621234162341662123434");
        }

        public NBitPlane BuildAttributeTable(SystemMemory memory, int nameTableBytes)
        {
            //fix address
            NBitPlane attributeTable = NBitPlane.Create(
                memory.GetAddress(AddressLabels.FreeRAM) + nameTableBytes, memory, 
                _specs.AttributeTableBitsPerBlock,
               _sceneDefinition.GetLevelTileWidth(_specs) / _specs.AttributeTableBlockSize,
               _sceneDefinition.GetLevelTileHeight(_specs) / _specs.AttributeTableBlockSize);

            return attributeTable;
        }


        public NBitPlane BuildNameTable(SystemMemory memory, int seed)
        {
            NBitPlane nameTable = NBitPlane.Create(memory.GetAddress(AddressLabels.FreeRAM), 
                memory, 
                _specs.NameTableBitPlanes,
                _sceneDefinition.GetLevelTileWidth(_specs),
                _sceneDefinition.GetLevelTileHeight(_specs));

          
            return SetupDefaultTiles(nameTable);
        }

        private NBitPlane SetupDefaultTiles(NBitPlane nameTable)
        {
            return _sceneDefinition.ScrollStyle switch 
            {
                ScrollStyle.Horizontal => _sceneDefinition.LevelShape switch 
                {
                    LevelShape.Flat => AddEdgeTiles(nameTable,
                                                   top: _sceneDefinition.BeginTiles,
                                                   bottom: _sceneDefinition.EndTiles),
                    _ => throw new NotImplementedException(),
                },
                _ => throw new NotImplementedException(),
            };
        }

        private NBitPlane AddEdgeTiles(NBitPlane nameTable, int top = 0, int left = 0, int right = 0, int bottom = 0)
        {
            for(var row = 0; row < top; row++)
            {
                for(var col = 0; col < nameTable.Width; col++)
                {
                    nameTable[row, col] = 9; //todo
                }
            }

            for (var row = nameTable.Height-bottom; row < nameTable.Height; row++)
            {
                for (var col = 0; col < nameTable.Width; col++)
                {
                    nameTable[col, row] = 9; //todo
                }
            }

            if (left != 0 || right != 0)
                throw new NotImplementedException();

            return nameTable;
        }

        public void SetupVRAMPatternTable(
           NBitPlane masterPatternTable,
           NBitPlane vramPatternTable,
           SystemMemory memory)
        {
            //tile row 1
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, (int)_sceneDefinition.TileRow, 8, 1),
                destinationPoint: new Point(0, 0),
                _specs,
                memory);

            //tile row 2
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(8, (int)_sceneDefinition.TileRow, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);

            GridPoint spriteDestination = new ByteGridPoint(_specs.PatternTableTilesAcross, _specs.PatternTableTilesDown);
            spriteDestination.Y = 2;

            if (_sceneDefinition.HasSprite(SpriteLoadFlags.Player))
            {
                //player sprite
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(0, 0, 2, 2),
                  destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                  _specs,
                  memory);

                //bomb sprite
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(5, 1, 1, 1),
                  destinationPoint: new Point(spriteDestination.X+1, spriteDestination.Y),
                  _specs,
                  memory);

                spriteDestination.Advance(2, extraRowSkip: 1);

                //status bar text
                masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: new InMemoryByteRectangle(4, 3, 8, 2),
                    destinationPoint: new Point(0, 5),
                    _specs,
                    memory);

                //status bar text 2
                masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: new InMemoryByteRectangle(12, 4, 2, 1),
                    destinationPoint: new Point(6, 7),
                    _specs,
                    memory);

                //health guage
                masterPatternTable.CopyTilesTo(
                    destination: vramPatternTable,
                    source: new InMemoryByteRectangle(0, 4, 4, 1),
                    destinationPoint: new Point(0, 7),
                    _specs,
                    memory);
            }


            if (_sceneDefinition.HasSprite(SpriteLoadFlags.Lizard))
            {
                //lizard sprite
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(2, 0, 2, 2),
                  destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                  _specs,
                  memory);

                spriteDestination.Advance(2, extraRowSkip: 1);

                //fireball sprite
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(4, 0, 4, 1),
                  destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                  _specs,
                  memory);

                spriteDestination.Advance(4, extraRowSkip: 1);
            }

            if (_sceneDefinition.HasSprite(SpriteLoadFlags.Bird))
            {
                //bird sprite
                masterPatternTable.CopyTilesTo(
                  destination: vramPatternTable,
                  source: new InMemoryByteRectangle(8, 0, 4, 1),
                  destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                  _specs,
                  memory);

                spriteDestination.Advance(4, extraRowSkip: 1);
            }
        }
    }
}
