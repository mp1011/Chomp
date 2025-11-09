using ChompGame.Data;
using ChompGame.MainGame;
using ChompGame.MainGame.SceneModels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ChompGame.GameSystem
{
    internal class TileCopier
    {
        private readonly NBitPlane _masterPatternTable; private NBitPlane masterPatternTable => _masterPatternTable;
        private readonly ChompGameModule _gameModule;
        private Specs Specs => _gameModule.GameSystem.Specs;
        private MainSystem GameSystem => _gameModule.GameSystem;

        private Specs _specs => Specs; private Specs specs => Specs;

        private SystemMemory _memory => GameSystem.Memory; private SystemMemory memory => GameSystem.Memory;

        public TileCopier(NBitPlane masterPatternTable, ChompGameModule gameModule)
        {
            _gameModule = gameModule;
            _masterPatternTable = masterPatternTable;
        }

        public void AnalyzeTileUsage()
        {
            BitPlaneCopyStats.Sources.Clear();

            foreach(var method in typeof(TileCopier).GetMethods().Where(p=>p.Name.Contains("Copy")))
            {
                try
                {
                    method.Invoke(this, null);
                }
                catch
                {

                }
            }

            foreach(Level level in Enum.GetValues(typeof(Level)))
            {
                _gameModule.CurrentLevel = level;
                var sceneDef = new SceneDefinition(level, _memory, _specs);
                var lb = new LevelBuilder(_gameModule, sceneDef);
                lb.SetupVRAMPatternTable(GameSystem.CoreGraphicsModule.BackgroundPatternTable, GameSystem.CoreGraphicsModule.SpritePatternTable, _memory);
            }

            List<Point> unused = new List<Point>();
            for (int y = 0; y < _masterPatternTable.Height/ _specs.TileHeight; y++)
            {
                for(int x =0; x< _masterPatternTable.Width / _specs.TileWidth; x++)
                {
                    var usedIn = BitPlaneCopyStats.Sources.Where(p => p.Contains((byte)x, (byte)y)).ToArray();
                    if (usedIn.Length == 0)
                    {
                        unused.Add(new Point(x, y));
                    }    
                }
            }

            unused = unused.Distinct().ToList();

            GameDebug.NoOp();
        }

        public void CopyTilesForTitle()
        {
            // text line1
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(4, 3, 7, 1),
                destinationPoint: new Point(1, 0),
                _gameModule.Specs,
                GameSystem.Memory);

            // text line2
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(11, 3, 4, 1),
                destinationPoint: new Point(0, 1),
                _gameModule.Specs,
                GameSystem.Memory);

            // text line2
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(8, 5, 4, 1),
                destinationPoint: new Point(4, 1),
                _gameModule.Specs,
                GameSystem.Memory);

            // text line3
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(6, 4, 8, 1),
                destinationPoint: new Point(0, 2),
                _gameModule.Specs,
                GameSystem.Memory);

            // star
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(5, 6, 1, 1),
                destinationPoint: new Point(0, 3),
                _gameModule.Specs,
                GameSystem.Memory);

            // solid tile
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(5, 5, 1, 1),
                destinationPoint: new Point(1, 3),
                _gameModule.Specs,
                GameSystem.Memory);

            // M mid
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(4, 6, 1, 1),
                destinationPoint: new Point(2, 3),
                _gameModule.Specs,
                GameSystem.Memory);

            // Other Text
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(0, 9, 4, 1),
                destinationPoint: new Point(3, 3),
                _gameModule.Specs,
                GameSystem.Memory);

            // gem sprite
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.SpritePatternTable,
                source: new InMemoryByteRectangle(15, 0, 1, 1),
                destinationPoint: new Point(1, 0),
                _gameModule.Specs,
                GameSystem.Memory);

            // chomp sprite
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.SpritePatternTable,
                source: new InMemoryByteRectangle(8, 1, 4, 2),
                destinationPoint: new Point(2, 0),
                _gameModule.Specs,
                GameSystem.Memory);
        }

        public void CopyTilesForLevelCard()
        {
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(7, 3, 1, 1),
                destinationPoint: new Point(1, 0),
                _gameModule.Specs,
                GameSystem.Memory);

            _masterPatternTable.CopyTilesTo(
               destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
               source: new InMemoryByteRectangle(8, 5, 4, 1),
               destinationPoint: new Point(2, 0),
               _gameModule.Specs,
               GameSystem.Memory);

            _masterPatternTable.CopyTilesTo(
              destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
              source: new InMemoryByteRectangle(6, 4, 8, 1),
              destinationPoint: new Point(0, 1),
              _gameModule.Specs,
              GameSystem.Memory);
        }
    
        public void CopyTilesForGameOver()
        {
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(4, 3, 7, 1),
                destinationPoint: new Point(1, 0),
                Specs,
                GameSystem.Memory);

            _masterPatternTable.CopyTilesTo(
               destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
               source: new InMemoryByteRectangle(11, 3, 5, 1),
               destinationPoint: new Point(0, 1),
               Specs,
               GameSystem.Memory);

        }

        public void CopyTilesForSprite(int srcX, int srcY, int srcWidth, int srcHeight, int destX, int destY, NBitPlane target)
        {
            _masterPatternTable.CopyTilesTo(
            destination: target,
            source: new InMemoryByteRectangle(srcX, srcY, srcWidth, srcHeight),
            destinationPoint: new Point(destX, destY),
            _specs,
            _memory);
        }

        public void CopyTilesForBossTheme(ByteRectangleBase FloorTiles)
        {
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: FloorTiles,
                destinationPoint: new Point(0, 1),
                _specs,
                _memory);
        }

        public void CopyTilesForStatusBar()
        {
            //row 0 - top status bar text
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(4, 3, 7, 1),
                destinationPoint: new Point(1, 5),
                _specs,
                _memory);

            //row 1 - bottom status bar text
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(5, 4, 8, 1),
                destinationPoint: new Point(0, 6),
                _specs,
                _memory);

            // row 2 - more text
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(13, 4, 2, 1),
                destinationPoint: new Point(6, 7),
                _specs,
                _memory);

            // row 2 - health guage, filled tile
            _masterPatternTable.CopyTilesTo(
                destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
                source: new InMemoryByteRectangle(0, 4, 5, 1),
                destinationPoint: new Point(1, 7),
                _specs,
                _memory);
        }

        public void CopyTilesForBossBody(bool finalBoss)
        {
            _masterPatternTable.CopyTilesTo(
               destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
               source: new InMemoryByteRectangle(8, 9, 8, 2),
               destinationPoint: new Point(0, 3),
               _specs,
               _memory);

            _masterPatternTable.CopyTilesTo(
               destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
               source: new InMemoryByteRectangle(5, 11, 7, 1),
               destinationPoint: new Point(1, 2),
               _specs,
               _memory);

            if (finalBoss)
            {
                _masterPatternTable.CopyTilesTo(
                     destination: GameSystem.CoreGraphicsModule.BackgroundPatternTable,
               source: new InMemoryByteRectangle(11, 8, 1, 1),
               destinationPoint: new Point(1, 2),
               _specs,
               _memory);
            }

        }

        public void CopyTilesForMistTheme()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;

            //mist
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(15, 2, 1, 1),
                destinationPoint: new Point(1, 0),
                specs,
                memory);

            //mist
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(15, 4, 1, 1),
                destinationPoint: new Point(2, 0),
                specs,
                memory);

            //solid
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(5, 7, 1, 1),
                destinationPoint: new Point(6, 0),
                specs,
                memory);

            //bg
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 12, 8, 1),
                destinationPoint: new Point(0, 1),
                specs,
                memory);

            //brick
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(2, 8, 1, 1),
                destinationPoint: new Point(7, 2),
                specs,
                memory);

            //brick l
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(7, 9, 1, 1),
                destinationPoint: new Point(1, 3),
                specs,
                memory);

            //brick r
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(7, 10, 1, 1),
                destinationPoint: new Point(2, 3),
                specs,
                memory);
        }

        public void CopyTilesForCityTheme()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 7, 8, 1),
                destinationPoint: new Point(0, 3),
                specs,
                memory);

            _masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(0, 8, 8, 1),
               destinationPoint: new Point(0, 4),
               specs,
               memory);

            _masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(8, 8, 4, 1),
              destinationPoint: new Point(1, 0),
              specs,
              memory);

            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 14, 8, 1),
                destinationPoint: new Point(0, 1),
                specs,
                memory);

            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(10, 10, 1, 1),
                destinationPoint: new Point(4, 0),
                specs,
                memory);
        }

        public void CopyTilesForCityTrainTheme()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 7, 8, 1),
                destinationPoint: new Point(0, 3),
                specs,
                memory);

            _masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(8, 7, 4, 1),
               destinationPoint: new Point(0, 1),
               specs,
               memory);

            _masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(8, 12, 3, 1),
               destinationPoint: new Point(4, 1),
               specs,
               memory);

            _masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(13, 12, 3, 1),
               destinationPoint: new Point(0, 2),
               specs,
               memory);

            _masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(0, 14, 2, 1),
               destinationPoint: new Point(1, 0),
               specs,
               memory);

            _masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(13, 10, 2, 1),
              destinationPoint: new Point(3, 0),
              specs,
              memory);
        }

        public void CopyTilesForDesertInteriorTheme()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(8, 14, 2, 1),
                destinationPoint: new Point(0, 1),
                specs,
                memory);

            _masterPatternTable.CopyTilesTo(
                 destination: vramPatternTable,
                 source: new InMemoryByteRectangle(2, 12, 6, 1),
                 destinationPoint: new Point(2, 1),
                 specs,
                 memory);


            _masterPatternTable.CopyTilesTo(
                 destination: vramPatternTable,
                 source: new InMemoryByteRectangle(10, 14, 3, 1),
                 destinationPoint: new Point(1, 0),
                 specs,
                 memory);
        }

        public void CopyTilesForPlainsTheme()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;
            //tile row 1
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 5, 6, 1),
                destinationPoint: new Point(1, 0),
                specs,
                memory);

            //tile row 2
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 12, 8, 1),
                destinationPoint: new Point(0, 1),
                specs,
                memory);
        }

        public void CopyTilesForForestTheme()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;
            //trees1
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(9, 8, 2, 1),
                destinationPoint: new Point(1, 0),
                specs,
                memory);

            //trees2
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(11, 15, 3, 1),
                destinationPoint: new Point(3, 0),
                specs,
                memory);

            //solid
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(5, 7, 1, 1),
                destinationPoint: new Point(6, 0),
                specs,
                memory);

            //trunk
            _masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(6, 12, 1, 1),
               destinationPoint: new Point(7, 0),
               specs,
               memory);

            //bg
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 12, 8, 1),
                destinationPoint: new Point(0, 1),
                specs,
                memory);
        }

        public void CopyTilesForTechBaseTheme()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;
            //bg
            _masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(0, 10, 3, 1),
              destinationPoint: new Point(1, 0),
              specs,
              memory);

            _masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(0, 11, 3, 1),
             destinationPoint: new Point(4, 0),
             specs,
             memory);

            _masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(5, 7, 1, 1),
               destinationPoint: new Point(7, 0),
               specs,
               memory);

            //bg extra
            _masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(6, 11, 1, 1),
             destinationPoint: new Point(0, 3),
             specs,
             memory);

            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(15, 10, 1, 1),
                destinationPoint: new Point(1, 3),
                specs,
                memory);

            _masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(3, 11, 2, 1),
              destinationPoint: new Point(2, 3),
              specs,
              memory);


            // fg
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(5, 7, 1, 1),
                destinationPoint: new Point(0, 1),
                specs,
                memory);

            _masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(5, 7, 1, 1),
              destinationPoint: new Point(1, 1),
              specs,
              memory);

            _masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(2, 12, 3, 1),
             destinationPoint: new Point(2, 1),
             specs,
             memory);

            _masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(5, 12, 1, 1),
               destinationPoint: new Point(5, 1),
               specs,
               memory);

            _masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(8, 12, 2, 1),
             destinationPoint: new Point(6, 1),
             specs,
             memory);
        }

        public void CopyTilesForMistAutoscrollTheme()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;
            //mist
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(15, 2, 1, 1),
                destinationPoint: new Point(1, 0),
                specs,
                memory);

            //mist
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(15, 4, 1, 1),
                destinationPoint: new Point(2, 0),
                specs,
                memory);

            //solid
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(5, 7, 1, 1),
                destinationPoint: new Point(6, 0),
                specs,
                memory);

            //bg
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 12, 8, 1),
                destinationPoint: new Point(0, 1),
                specs,
                memory);

            //brick
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(2, 8, 1, 1),
                destinationPoint: new Point(3, 0),
                specs,
                memory);

            //brick l
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(7, 9, 1, 1),
                destinationPoint: new Point(4, 0),
                specs,
                memory);

            //brick r
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(7, 10, 1, 1),
                destinationPoint: new Point(5, 0),
                specs,
                memory);

            // star 
            _masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(5, 6, 1, 1),
                destinationPoint: new Point(6, 0),
                specs,
                memory);
        }
    
        public void CopyTilesForCityInterior()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;
            // "sky"
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(4, 4, 1, 1),
                destinationPoint: new Point(7, 0),
                _specs,
                memory);

            //bg buildings
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 7, 8, 1),
                destinationPoint: new Point(0, 3),
                _specs,
                memory);

            // fg buildings
            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(0, 8, 8, 1),
               destinationPoint: new Point(0, 4),
               _specs,
               memory);

            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(8, 8, 4, 1),
              destinationPoint: new Point(1, 0),
              _specs,
              memory);

            //fg
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 14, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);
        }

        public void CopyTilesForDesertTheme()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;

            // sand
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 13, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);

            // pyramid
            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(11, 11, 3, 1),
               destinationPoint: new Point(0, 3),
               _specs,
               memory);

            // pyramid2
            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(6, 15, 2, 1),
               destinationPoint: new Point(3, 3),
               _specs,
               memory);

            masterPatternTable.CopyTilesTo(
               destination: vramPatternTable,
               source: new InMemoryByteRectangle(5, 5, 1, 1),
               destinationPoint: new Point(5, 3),
               _specs,
               memory);

            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(9, 13, 2, 1),
             destinationPoint: new Point(6, 3),
             _specs,
             memory);
        }
    
        public void CopyTilesForDesertRainTheme()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;

            // sand
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 13, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);

            // rain
            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(8, 15, 4, 1),
              destinationPoint: new Point(1, 0),
              _specs,
              memory);
        }
    
        public void CopyTilesForFinalArea()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;

            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(0, 15, 6, 1),
              destinationPoint: new Point(0, 1),
              _specs,
              memory);

            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(14, 15, 1, 1),
             destinationPoint: new Point(6, 1),
             _specs,
             memory);


            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(15, 13, 1, 1),
             destinationPoint: new Point(7, 1),
             _specs,
             memory);

            // bricks
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 14, 2, 1),
                destinationPoint: new Point(1, 0),
                _specs,
                memory);

            // pipe
            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(6, 0, 1, 1),
              destinationPoint: new Point(3, 0),
              _specs,
              memory);

            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(6, 6, 1, 1),
             destinationPoint: new Point(4, 0),
             _specs,
             memory);

        }

        public void CopyTilesForGlitchCore()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;

            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(0, 0, 7, 1),
              destinationPoint: new Point(1, 0),
              _specs,
              memory);

            masterPatternTable.CopyTilesTo(
             destination: vramPatternTable,
             source: new InMemoryByteRectangle(2, 1, 8, 2),
             destinationPoint: new Point(0, 1),
             _specs,
             memory);


        }
    
        public void CopyTilesForOceanAutoscroll()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;

            //tile row 1
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(2, 5, 3, 1),
                destinationPoint: new Point(0, 3),
                _specs,
                memory);

            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(0, 6, 4, 1),
              destinationPoint: new Point(3, 3),
              _specs,
              memory);

            //tile row 2
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 13, 8, 1),
                destinationPoint: new Point(0, 4),
                _specs,
                memory);
        }
    
        public void CopyTilesForOceanTheme()
        {
            var vramPatternTable = GameSystem.CoreGraphicsModule.BackgroundPatternTable;

            //tile row 1
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(2, 5, 3, 1),
                destinationPoint: new Point(1, 0),
                _specs,
                memory);

            masterPatternTable.CopyTilesTo(
              destination: vramPatternTable,
              source: new InMemoryByteRectangle(0, 6, 4, 1),
              destinationPoint: new Point(4, 0),
              _specs,
              memory);

            //tile row 2
            masterPatternTable.CopyTilesTo(
                destination: vramPatternTable,
                source: new InMemoryByteRectangle(0, 13, 8, 1),
                destinationPoint: new Point(0, 1),
                _specs,
                memory);
        }
    }
}
