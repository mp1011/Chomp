using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.Helpers
{
    public class CollisionInfo
    {
        public int XCorrection { get; set; }
        public int YCorrection { get; set; }
        public bool IsOnGround { get; set; }
        public bool DynamicBlockCollision { get; set; }
        public int TileX { get; set; }
        public int TileY { get; set; }
        public int CoinTileX { get; set; }
        public int CoinTileY { get; set; }
        public int BreakableTileX { get; set; }
        public int BreakableTileY { get; set; }
        public bool LeftLedge { get; set; }
        public bool RightLedge { get; set; }
        public byte LedgeHeight { get; set; }
        public bool HitLeftWall => XCorrection > 0;
        public bool HitRightWall => XCorrection < 0;
    }

    /// <summary>
    /// adds 2 tiles to Y to account for status bar
    /// </summary>
    class BitPlaneForCollision : IGrid<byte>
    {
        private NBitPlane _realMap;
        private const int _statusBarRows = 2;

        public BitPlaneForCollision(NBitPlane realMap)
        {
            _realMap = realMap;
        }

        public byte this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public byte this[int x, int y]
        {
            get => y < 2 ? (byte)0 : _realMap[x, y - _statusBarRows];
            set => _realMap[x, y - _statusBarRows] = value;
        }
        public int Width => _realMap.Width;

        public int Height => _realMap.Height + _statusBarRows;

        public int Bytes => _realMap.Bytes;

        public byte ValueFromChar(char s, int offset) => _realMap.ValueFromChar(s, offset);
    }

    class CollisionDetector 
    {
        private const byte CollidableTileBegin = 8;
        private const byte CollidableTileEnd = 23;

        private readonly Specs _specs;
        private BitPlaneForCollision _levelTileMap;
        private SceneDefinition _currentScene;
        private SpriteTileTable _spriteTileTable;

        public CollisionDetector(Specs specs)
        {
            _specs = specs;
        }

        public void Initialize(SceneDefinition sceneDefinition, NBitPlane levelTileMap, SpriteTileTable spriteTileTable)
        {
            _spriteTileTable = spriteTileTable;
            _levelTileMap = new BitPlaneForCollision(levelTileMap);
            _currentScene = sceneDefinition;
        }

        public bool CheckCollision(MovingSprite s1, MovingSprite s2)
        {
            return s1.Bounds.Intersects(s2.Bounds);
        }

        public static bool IsTileSolid(byte index) => index >= CollidableTileBegin
            && index < CollidableTileEnd;

        public CollisionInfo DetectCollisions(WorldSprite actor, IMotion motion)
        {
            var collisionInfo = new CollisionInfo();

            var topLeftTile = actor.TopLeft
                .Divide(_specs.TileWidth)
                .Add(-2, -2);


            var bottomRightTile = actor.BottomRight
                                        .Divide(_specs.TileWidth)
                                        .Add(2, 2);

            var actorBounds = actor.Bounds;

            _levelTileMap.ForEach(topLeftTile, bottomRightTile, (x, y, t) =>
            {
                if (_currentScene.IsBossScene && y < _levelTileMap.Height - 4)
                    return;

                if (t < CollidableTileBegin || t > CollidableTileEnd)
                    return;

                var tileBounds = new Rectangle(
                    x * _specs.TileWidth,
                    y * _specs.TileHeight,
                    _specs.TileWidth,
                    _specs.TileHeight);
              
                var tileLeft = _levelTileMap[
                    (x - 1).NMod(_levelTileMap.Width),
                    y.NMod(_levelTileMap.Height)];

                var tileRight = _levelTileMap[
                    (x + 1).NMod(_levelTileMap.Width),
                    y.NMod(_levelTileMap.Height)];

                var tileAbove = _levelTileMap[
                   x.NMod(_levelTileMap.Width),
                   (y - 1).NMod(_levelTileMap.Height)];

                var tileBelow = _levelTileMap[
                    x.NMod(_levelTileMap.Width),
                    (y + 1).NMod(_levelTileMap.Height)];

                if (!tileBounds.Intersects(actorBounds))
                {
                    if (collisionInfo.IsOnGround
                        || IsTileSolid(tileAbove))
                    {
                        if (t == _spriteTileTable.DestructibleBlockTile)
                        {
                            collisionInfo.DynamicBlockCollision = true;
                            collisionInfo.BreakableTileX = x;
                            collisionInfo.BreakableTileY = y - Constants.StatusBarTiles;
                        }
                        return;
                    }

                    if(actorBounds.Bottom == tileBounds.Top
                        && actorBounds.Right >= tileBounds.X 
                        && actorBounds.X <= tileBounds.Right 
                        && motion.YSpeed >= 0)
                    {
                        collisionInfo.IsOnGround = true;
                        if (t == _spriteTileTable.DestructibleBlockTile)
                        {
                            collisionInfo.DynamicBlockCollision = true;
                            collisionInfo.BreakableTileX = x;
                            collisionInfo.BreakableTileY = y - Constants.StatusBarTiles;
                        }
                        else if( t == _spriteTileTable.CoinTile)
                        {
                            collisionInfo.IsOnGround = false;
                            collisionInfo.CoinTileX = x;
                            collisionInfo.CoinTileY = y - Constants.StatusBarTiles;
                        }
                        else
                        {
                            collisionInfo.TileX = x;
                            collisionInfo.TileY = y - Constants.StatusBarTiles;
                        }
                    }

                    if (collisionInfo.IsOnGround)
                        CheckLedge(x, y, collisionInfo, actorBounds, tileBounds);

                    return;
                }

                if (t == _spriteTileTable.DestructibleBlockTile)
                {
                    collisionInfo.DynamicBlockCollision = true;
                    collisionInfo.BreakableTileX = x;
                    collisionInfo.BreakableTileY = y - Constants.StatusBarTiles;
                }
                if (t == _spriteTileTable.CoinTile)
                {
                    collisionInfo.DynamicBlockCollision = true;
                    collisionInfo.CoinTileX = x;
                    collisionInfo.CoinTileY = y - Constants.StatusBarTiles;
                }
                else
                {
                    collisionInfo.TileX = x;
                    collisionInfo.TileY = y - Constants.StatusBarTiles;
                }

                if (t == _spriteTileTable.CoinTile)
                    return;

                bool checkLeftCollision = motion.XSpeed > 0 && (!IsTileSolid(tileLeft) || tileLeft == _spriteTileTable.CoinTile);
                bool checkRightCollision = motion.XSpeed < 0 && (!IsTileSolid(tileRight) || tileRight == _spriteTileTable.CoinTile);
                bool checkAbove = motion.YSpeed >= 0 && (!IsTileSolid(tileAbove) || tileAbove == _spriteTileTable.CoinTile);
                bool checkBelow = motion.YSpeed < 0 && (!IsTileSolid(tileBelow) || tileBelow == _spriteTileTable.CoinTile);

                int leftMove = actorBounds.Right - tileBounds.Left;
                int rightMove = tileBounds.Right - actorBounds.Left;
                int upMove = actorBounds.Bottom - tileBounds.Top;
                int downMove = tileBounds.Bottom - actorBounds.Top;

                if (leftMove > 0 && checkLeftCollision && motion.XSpeed > 0)
                    collisionInfo.XCorrection = -(leftMove);
                else if (rightMove > 0 && checkRightCollision && motion.XSpeed < 0)
                    collisionInfo.XCorrection = rightMove;
                else if (upMove > 0 && checkAbove && motion.YSpeed > 0)
                    collisionInfo.YCorrection = -upMove;
                else if (downMove > 0 && checkBelow && motion.YSpeed < 0)
                    collisionInfo.YCorrection = downMove;

                if(collisionInfo.YCorrection < 0)
                    CheckLedge(x, y, collisionInfo, actorBounds, tileBounds);
            });

            int rightEdgeOverlap = (actor.X + actor.Bounds.Width) - _levelTileMap.Width * _specs.TileWidth;
            if(rightEdgeOverlap > 0)
            {
                collisionInfo.XCorrection = -rightEdgeOverlap;
            }

            int leftEdgeOverlap = 0 - actor.X;
            if(leftEdgeOverlap > 0)
            {
                collisionInfo.XCorrection = leftEdgeOverlap;
            }

            if (collisionInfo.XCorrection != 0)
            {
                actor.X += collisionInfo.XCorrection;
            }

            if (collisionInfo.YCorrection != 0)
            {
                actor.Y += collisionInfo.YCorrection;
            }

            return collisionInfo;
        }

        public void CheckLedge(int x, int y, CollisionInfo collisionInfo, Rectangle actorBounds, Rectangle tileBounds)
        {
            var thisTile = _levelTileMap[x, y];
            var leftTile = _levelTileMap[x - 1, y];
            var rightTile = _levelTileMap[x + 1, y];

            int ledgeX = x;
            int ledgeY = y;

            if (IsTileSolid(thisTile) 
                && !IsTileSolid(leftTile)
                && actorBounds.Left <= tileBounds.Left)
            {
                ledgeX = x - 1;
                collisionInfo.LeftLedge = true;
            }


            if (IsTileSolid(thisTile)
                && !IsTileSolid(rightTile)
                && actorBounds.Right >= tileBounds.Right)
            {
                ledgeX = x + 1;
                collisionInfo.RightLedge = true;
            }

            if (!collisionInfo.LeftLedge && !collisionInfo.RightLedge)
                collisionInfo.LedgeHeight = 0;
            else
            {
                while (!IsTileSolid(_levelTileMap[ledgeX, ledgeY])
                    && ledgeY < _levelTileMap.Height)
                {
                    ledgeY++;
                }
            }

            if (ledgeY == _levelTileMap.Height)
                collisionInfo.LedgeHeight = 255;
            else 
                collisionInfo.LedgeHeight = (byte)(ledgeY - y);

            //if (collisionInfo.LeftLedge)
            //    GameDebug.DebugLog($"Left Edge Detected - Height = {collisionInfo.LedgeHeight}");
            //else if (collisionInfo.RightLedge)
            //    GameDebug.DebugLog($"Right Edge Detected - Height = {collisionInfo.LedgeHeight}");
        }
     
        private void CheckTouchingGround(WorldSprite actor, IMotion motion, Rectangle collidingTile, CollisionInfo collisionInfo)
        {
            var correctedBottom = actor.BottomRight.Y + collisionInfo.YCorrection;

            if (motion.YSpeed >= 0
                && correctedBottom == collidingTile.Y
                && actor.BottomRight.X > collidingTile.X
                && actor.X <= collidingTile.Right)
            {
                collisionInfo.IsOnGround = true;
            }
        }
    }
}
