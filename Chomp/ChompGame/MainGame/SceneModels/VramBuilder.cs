using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SceneModels
{
    class VramBuilder
    {
        private readonly NBitPlane _masterPatternTable, _vramPatternTable;
        private readonly SpriteTileTable _spriteTileTable;
        private readonly SystemMemory _memory;
        private readonly Specs _specs;
                
        private bool _enemy1Used, _extra1Used;
        public VramBuilder(
            NBitPlane masterPatternTable,
            NBitPlane vramPatternTable,
            SpriteTileTable spriteTileTable,
            SystemMemory memory,
            Specs specs)
        {
            _specs = specs;
            _masterPatternTable = masterPatternTable;
            _vramPatternTable = vramPatternTable;
            _spriteTileTable = spriteTileTable;
            _memory = memory;
        }

        public byte AddSprite(int x, int y, int width, int height, Point? destination = null)
        {
            Point spriteDestination = destination ?? CalcSpriteDestination(width, height);
            _masterPatternTable.CopyTilesTo(
                   destination: _vramPatternTable,
                   source: new InMemoryByteRectangle(x, y, width, height),
                   destinationPoint: new Point(spriteDestination.X, spriteDestination.Y),
                   _specs,
                   _memory);

            var index = (byte)((spriteDestination.Y * _specs.PatternTableTilesAcross)
                + spriteDestination.X);

            return (byte)(index);
        }

        private Point CalcSpriteDestination(int width, int height)
        {
            Point destination = new Point(0, 1);

            while(true)
            {
                if (HasSpace(destination, width, height))
                    return destination;

                destination.X++;
                if(destination.X == _specs.PatternTableTilesAcross)
                {
                    destination.Y++;
                    destination.X = 0;
                }

                if (destination.Y == _specs.PatternTableTilesDown)
                    return Point.Zero;
            }

        }

        private bool HasSpace(Point potentialDestination, int width, int height)
        {
            bool isClear = true;
            var topLeft = new Point(potentialDestination.X * _specs.TileWidth,
                potentialDestination.Y * _specs.TileHeight);

            var bottomRight = new Point((potentialDestination.X + width) * _specs.TileWidth,
              (potentialDestination.Y + height) * _specs.TileHeight);


            _vramPatternTable.ForEach(topLeft, bottomRight, (x, y, b) =>
            {
                if (b != 0)
                    isClear = false;
            });

            return isClear;
        }

        public byte AddEnemySprite(int x, int y, int width, int height)
        {
            var result = AddSprite(_enemy1Used ? SpriteTileIndex.Enemy2 : SpriteTileIndex.Enemy1, x, y, width, height);
            _enemy1Used = true;
            return result;
        }

        public byte AddExtraSprite(int x, int y, int width, int height)
        {
            var result = AddSprite(_extra1Used ? SpriteTileIndex.Extra2 : SpriteTileIndex.Extra1, x, y, width, height);
            _extra1Used = true;
            return result;
        }

        public byte AddExplosionSprite()
        {
            return AddSprite(SpriteTileIndex.Explosion, 6,5,2,1);
        }

        public byte AddSprite(SpriteTileIndex spriteIndex, int x, int y, int width, int height, Point? destination=null)
        {
            var tile = AddSprite(x, y, width, height, destination);
            _spriteTileTable.SetTile(spriteIndex, tile);
            return tile;
        }

        public void AddStatusBarTiles()
        {
            //row 0 - top status bar text
            _masterPatternTable.CopyTilesTo(
                destination: _vramPatternTable,
                source: new InMemoryByteRectangle(4, 3, 7, 1),
                destinationPoint: new Point(1, 5),
                _specs,
                _memory);

            //row 1 - bottom status bar text
            _masterPatternTable.CopyTilesTo(
                destination: _vramPatternTable,
                source: new InMemoryByteRectangle(5, 4, 8, 1),
                destinationPoint: new Point(0, 6),
                _specs,
                _memory);

            // row 2 - more text
            _masterPatternTable.CopyTilesTo(
                destination: _vramPatternTable,
                source: new InMemoryByteRectangle(13, 4, 2, 1),
                destinationPoint: new Point(6, 7),
                _specs,
                _memory);

            // row 2 - health guage, filled tile
            _masterPatternTable.CopyTilesTo(
                destination: _vramPatternTable,
                source: new InMemoryByteRectangle(0, 4, 5, 1),
                destinationPoint: new Point(1, 7),
                _specs,
                _memory);
        }

        public void AddBossBodyTiles()
        {
            _masterPatternTable.CopyTilesTo(
               destination: _vramPatternTable,
               source: new InMemoryByteRectangle(8, 9, 8, 2),
               destinationPoint: new Point(0, 3),
               _specs,
               _memory);

            _masterPatternTable.CopyTilesTo(
               destination: _vramPatternTable,
               source: new InMemoryByteRectangle(5, 11, 7, 1),
               destinationPoint: new Point(1, 2),
               _specs,
               _memory);
        }

        public void AddBossSprites(Level currentLevel)
        {
            switch(currentLevel)
            {
                case Level.Level1_11_Boss:
                case Level.Level3_20_Midboss:
                case Level.Level4_31_Midboss:
                case Level.Level5_22_MidBoss:
                case Level.Level6_17_Midboss:
                    AddSprite(SpriteTileIndex.Enemy1, 8, 1, 4, 2);
                    AddSprite(SpriteTileIndex.Extra2, 7, 1, 1, 1);                    
                    AddSprite(SpriteTileIndex.Extra1, 12, 2, 1, 1);
                    AddSprite(SpriteTileIndex.Explosion, 5, 0, 2, 1);
                    return;

                case Level.Level1_17_Boss:
                    AddSprite(SpriteTileIndex.Enemy1, 11, 12, 2, 2); //eye
                    AddSprite(SpriteTileIndex.Enemy2, 14, 11, 2, 1); //jaw
                    AddSprite(SpriteTileIndex.Extra1, 4, 0, 1, 1);
                    AddSprite(SpriteTileIndex.Explosion, 5, 0, 2, 1);
                    AddSprite(SpriteTileIndex.Extra2, 5, 0, 1, 1);
                    return;
                case Level.Level3_26_Boss:
                    AddSprite(SpriteTileIndex.Enemy1, 11, 12, 2, 2); //eye
                    AddSprite(SpriteTileIndex.Enemy2, 14, 11, 2, 1); //jaw
                    AddSprite(SpriteTileIndex.Extra1, 12, 2, 1, 1); // bullet
                    AddSprite(SpriteTileIndex.Explosion, 5, 0, 2, 1);
                    return;

                case Level.Level2_2_Fly2:
                    AddSprite(SpriteTileIndex.Enemy1, 8, 1, 4, 2);
                    AddSprite(SpriteTileIndex.Extra1, 12, 2, 1, 1);
                    AddSprite(SpriteTileIndex.Extra2, 7, 1, 1, 1);
                    return;

                case Level.Level2_12_Boss:
                    AddSprite(SpriteTileIndex.Enemy1, 11, 12, 2, 2); //eye
                    AddSprite(SpriteTileIndex.Enemy2, 7, 1, 1, 1); //arm
                    AddSprite(SpriteTileIndex.Extra1, 12, 2, 1, 1);
                    AddSprite(SpriteTileIndex.Explosion, 5, 0, 2, 1);
                    AddSprite(SpriteTileIndex.Extra2, 10, 7, 1, 1);
                    return;
                case Level.Level4_40_Boss:
                    AddSprite(SpriteTileIndex.Enemy1, 13, 13, 2, 2); //horn
                    AddSprite(SpriteTileIndex.Enemy2, 15, 14, 1, 2); //leg
                    AddSprite(SpriteTileIndex.Extra1, 12, 2, 1, 1); // bullet
                    AddSprite(SpriteTileIndex.Explosion, 5, 0, 2, 1);
                    return;
                case Level.Level5_27_Boss:
                    AddSprite(SpriteTileIndex.Enemy1, 11, 12, 2, 2); //eye
                    AddSprite(SpriteTileIndex.Enemy2, 13, 13, 2, 2); //horn
                    AddSprite(SpriteTileIndex.Extra1, 12, 2, 1, 1); // bullet
                    AddSprite(SpriteTileIndex.Explosion, 5, 0, 2, 1);
                    return;
                case Level.Level6_37_Boss:
                    AddSprite(SpriteTileIndex.Enemy1, 11, 12, 2, 2); //eye
                    AddSprite(SpriteTileIndex.Enemy2, 15, 14, 1, 2); //leg
                    AddSprite(SpriteTileIndex.Extra1, 12, 2, 1, 1); // bullet
                    AddSprite(SpriteTileIndex.Explosion, 5, 0, 2, 1);
                    return;

                default:
                    AddSprite(SpriteTileIndex.Enemy1, 8, 1, 4, 2);
                    AddSprite(SpriteTileIndex.Extra1, 12, 2, 1, 1);
                    AddSprite(SpriteTileIndex.Explosion, 5, 0, 2, 1);
                    return;
            }
           
        }
    }
}
