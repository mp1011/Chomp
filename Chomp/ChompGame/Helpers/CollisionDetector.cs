using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame;
using ChompGame.MainGame.SceneModels;
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
        public int DynamicTileX { get; set; }
        public int DynamicTileY { get; set; }
        public bool LeftLedge { get; set; }
        public bool RightLedge { get; set; }
        public byte LedgeHeight { get; set; }
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
            get => _realMap[x, y - _statusBarRows];
            set => _realMap[x, y - _statusBarRows] = value;
        }
        public int Width => _realMap.Width;

        public int Height => _realMap.Height + _statusBarRows;

        public int Bytes => _realMap.Bytes;

        public byte ValueFromChar(char s) => _realMap.ValueFromChar(s);
    }

    class CollisionDetector 
    {
        private readonly Specs _specs;
        private BitPlaneForCollision _levelTileMap;
       
        public CollisionDetector(Specs specs)
        {
            _specs = specs;
        }

        public void Initialize(SceneDefinition sceneDefinition, NBitPlane levelTileMap)
        {
            _levelTileMap = new BitPlaneForCollision(levelTileMap);
        }

        public bool CheckCollision(MovingSprite s1, MovingSprite s2)
        {
            return s1.Bounds.Intersects(s2.Bounds);
        }

        public CollisionInfo DetectCollisions(MovingWorldSprite actor)
        {
            int collidableTileBeginIndex = Constants.CollidableTileBeginIndex;
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
                if (t < collidableTileBeginIndex)
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
                        || tileAbove >= collidableTileBeginIndex)
                    {
                        return;
                    }

                    if(actorBounds.Bottom == tileBounds.Top
                        && actorBounds.Right >= tileBounds.X 
                        && actorBounds.X <= tileBounds.Right 
                        && actor.YSpeed >= 0)
                    {
                        collisionInfo.IsOnGround = true;
                        if (t == Constants.DestructibleBlockTile)
                        {
                            collisionInfo.DynamicBlockCollision = true;
                            collisionInfo.DynamicTileX = x;
                            collisionInfo.DynamicTileY = y - Constants.StatusBarTiles;
                        }
                        else if( t == Constants.CoinTile)
                        {
                            collisionInfo.IsOnGround = false;
                            collisionInfo.DynamicTileX = x;
                            collisionInfo.DynamicTileY = y - Constants.StatusBarTiles;
                        }
                        else
                        {
                            collisionInfo.TileX = x;
                            collisionInfo.TileY = y - Constants.StatusBarTiles;
                        }
                    }

                    if (collisionInfo.IsOnGround)
                        CheckLedge(x, y, collisionInfo);

                    return;
                }

                if (t == Constants.DestructibleBlockTile || t == Constants.CoinTile)
                {
                    collisionInfo.DynamicBlockCollision = true;
                    collisionInfo.DynamicTileX = x;
                    collisionInfo.DynamicTileY = y - Constants.StatusBarTiles;
                }
                else
                {
                    collisionInfo.TileX = x;
                    collisionInfo.TileY = y - Constants.StatusBarTiles;
                }

                if (t == Constants.CoinTile)
                    return;

                bool checkLeftCollision = actor.XSpeed > 0 && tileLeft < collidableTileBeginIndex;
                bool checkRightCollision = actor.XSpeed < 0 && tileRight < collidableTileBeginIndex;
                bool checkAbove = actor.YSpeed >= 0 && tileAbove < collidableTileBeginIndex;
                bool checkBelow = actor.YSpeed < 0 && tileBelow < collidableTileBeginIndex;

                int leftMove = actorBounds.Right - tileBounds.Left;
                int rightMove = tileBounds.Right - actorBounds.Left;
                int upMove = actorBounds.Bottom - tileBounds.Top;
                int downMove = tileBounds.Bottom - actorBounds.Top;

                if (leftMove > 0 && checkLeftCollision && actor.XSpeed > 0)
                    collisionInfo.XCorrection = -(leftMove);
                else if (rightMove > 0 && checkRightCollision && actor.XSpeed < 0)
                    collisionInfo.XCorrection = rightMove;
                else if (upMove > 0 && checkAbove && actor.YSpeed > 0)
                    collisionInfo.YCorrection = -upMove;
                else if (downMove > 0 && checkBelow && actor.YSpeed < 0)
                    collisionInfo.YCorrection = downMove;

                if(upMove > 0)
                    CheckLedge(x, y, collisionInfo);
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

        public void CheckLedge(int x, int y, CollisionInfo collisionInfo)
        {
            var thisTile = _levelTileMap[x, y];
            var leftTile = _levelTileMap[x - 1, y];
            var rightTile = _levelTileMap[x + 1, y];

            int ledgeX = x;
            int ledgeY = y;

            if (thisTile >= Constants.CollidableTileBeginIndex && leftTile < Constants.CollidableTileBeginIndex)
            {
                ledgeX = x - 1;
                collisionInfo.LeftLedge = true;
            }


            if (thisTile >= Constants.CollidableTileBeginIndex && rightTile < Constants.CollidableTileBeginIndex)
            {
                ledgeX = x + 1;
                collisionInfo.RightLedge = true;
            }

            if (!collisionInfo.LeftLedge && !collisionInfo.RightLedge)
                collisionInfo.LedgeHeight = 0;
            else
            {
                while (_levelTileMap[ledgeX, ledgeY] < Constants.CollidableTileBeginIndex
                    && ledgeY < _levelTileMap.Height)
                {
                    ledgeY++;
                }
            }

            if (ledgeY == _levelTileMap.Height)
                collisionInfo.LedgeHeight = 255;
            else 
                collisionInfo.LedgeHeight = (byte)(ledgeY - y);

            //if(collisionInfo.LeftLedge)            
            //    GameDebug.DebugLog($"Left Edge Detected - Height = {collisionInfo.LedgeHeight}");
            //else if (collisionInfo.RightLedge)
            //    GameDebug.DebugLog($"Right Edge Detected - Height = {collisionInfo.LedgeHeight}");
        }

        public CollisionInfo DetectCollisions2(MovingWorldSprite actor)
        {
            int solidTileBeginIndex = 8;
            var collisionInfo = new CollisionInfo();

            var topLeftTile = actor.TopLeft
                .Divide(_specs.TileWidth)
                .Add(-2, -2);


            var bottomRightTile = actor.BottomRight
                                        .Divide(_specs.TileWidth)
                                        .Add(2, 2);

            _levelTileMap.ForEach(topLeftTile, bottomRightTile, (x, y, t) =>
            {
                if (t < solidTileBeginIndex)
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

                bool checkLeftCollision = actor.XSpeed > 0 && tileLeft < solidTileBeginIndex;
                bool checkRightCollision = actor.XSpeed < 0 && tileRight < solidTileBeginIndex;

                CheckCollisionCorrectionX(actor, collisionInfo, tileBounds, checkLeftCollision, checkRightCollision);
            });

            //foreach (var block in _actorManager.GetActors(ActorType.Gizmo))
            //{
            //    if (!block.Visible)
            //        continue;

            //    bool checkLeftCollision = actor.MotionVector.X > 0;
            //    bool checkRightCollision = actor.MotionVector.X < 0;
            //    CheckCollisionCorrectionX(actor, correction, block.WorldPosition, checkLeftCollision, checkRightCollision);
            //}

            if (collisionInfo.XCorrection != 0)
            {
                actor.X += collisionInfo.XCorrection;
               // collisionInfo += new CollisionInfo(XCorrection: correction.X);
            }

            _levelTileMap.ForEach(topLeftTile, bottomRightTile, (x, y, t) =>
            {
                if (t < solidTileBeginIndex)
                    return;

                var tileBounds = new Rectangle(
                   x * _specs.TileWidth,
                   y * _specs.TileHeight,
                   _specs.TileWidth,
                   _specs.TileHeight);

                if (actor.Bounds.Intersects(tileBounds))
                    return;

                var tileAbove = _levelTileMap[
                    x.NMod(_levelTileMap.Width), 
                    (y - 1).NMod(_levelTileMap.Height)];

                var tileBelow = _levelTileMap[
                    x.NMod(_levelTileMap.Width), 
                    (y + 1).NMod(_levelTileMap.Height)];

                var tileLeft = _levelTileMap[
                    (x - 1).NMod(_levelTileMap.Width), 
                    y.NMod(_levelTileMap.Height)];

                var tileRight = _levelTileMap[
                    (x + 1).NMod(_levelTileMap.Width), 
                    y.NMod(_levelTileMap.Height)];

                bool checkAbove = actor.YSpeed >= 0 && tileAbove < solidTileBeginIndex;
                bool checkBelow = actor.YSpeed < 0 && tileBelow < solidTileBeginIndex;

         
            });

            //foreach (var block in _actorManager.GetActors(ActorType.Gizmo))
            //{
            //    if (!block.Visible)
            //        continue;

            //    bool checkAbove = actor.MotionVector.Y >= 0;
            //    bool checkBelow = actor.MotionVector.Y < 0;

            //    collisionInfo = CheckCollisionCorrectionY(actor, correction, collisionInfo, block.WorldPosition,
            //    false, false, checkAbove, checkBelow);

            //    var correctedBottom = actor.WorldPosition.BottomPixel + correction.Y;

            //    if (collisionInfo.IsOnGround && correctedBottom == block.WorldPosition.TopPixel)
            //    {
            //        correction.CarryMotion = block.MotionVector;
            //    }
            //}

            if (collisionInfo.YCorrection != 0)
            {
                actor.Y = actor.Y + collisionInfo.YCorrection;
            }

            //if (correction.CarryMotion != null)
            //{
            //    actor.WorldPosition.X.Add(correction.CarryMotion.X);
            //    actor.WorldPosition.Y.Add(correction.CarryMotion.Y);
            //}
            if (actor.Y > 32)
                actor.Y += 0;

            return collisionInfo;
        }

        private void CheckCollisionCorrectionX(MovingWorldSprite actor, CollisionInfo correction,
            Rectangle bounds, bool checkLeftCollision, bool checkRightCollision)
        {
            var xTemp = 0;
            if (bounds.Intersects(actor.Bounds))
            {
                var leftDifference = bounds.X - actor.BottomRight.X;
                var rightDifference = bounds.Right - actor.X;

                if (checkLeftCollision && Math.Abs(leftDifference) <= Math.Abs(rightDifference))
                {
                    xTemp = leftDifference;
                    if (xTemp > 0)
                        xTemp = 0;
                    if (xTemp < correction.XCorrection)
                        correction.XCorrection = xTemp;
                }
                else if (checkRightCollision && Math.Abs(leftDifference) >= Math.Abs(rightDifference))
                {
                    xTemp = rightDifference;
                    if (xTemp < 0)
                        xTemp = 0;
                    if (xTemp > correction.XCorrection)
                        correction.XCorrection = xTemp;
                }
            }
        }

        private void CheckCollisionCorrectionY(MovingWorldSprite actor, CollisionInfo correction, CollisionInfo collisionInfo,
            Rectangle bounds, bool leftSolid, bool rightSolid, bool checkAbove, bool checkBelow)
        {

            var yTemp = 0;

            if (bounds.Intersects(actor.Bounds))
            {
                var topDifference = bounds.Y - actor.BottomRight.Y;
                var bottomDifference = bounds.Bottom - actor.Y;

                if (checkAbove && (Math.Abs(topDifference) <= Math.Abs(bottomDifference)))
                {
                    yTemp = topDifference;

                    if (yTemp > 0)
                        yTemp = 0;
                    if (yTemp < correction.YCorrection)
                    {
                        correction.YCorrection = yTemp;
                        CheckTouchingGround(actor, bounds, correction);
                    }
                }
                else if (checkBelow && (Math.Abs(topDifference) >= Math.Abs(bottomDifference)))
                {
                    yTemp = bottomDifference;
                    if (yTemp < 0)
                        yTemp = 0;
                    if (yTemp > correction.YCorrection)
                        correction.YCorrection = yTemp;
                }
            }

            if (checkAbove)
            {
                CheckTouchingGround(actor, bounds, correction);                
                //if (groundCollision.IsOnGround && (!leftSolid || !rightSolid))
                //    collisionInfo += CheckOnLedge(actor, bounds, leftSolid, rightSolid);
            }
        }

        //private CollisionInfo CheckOnLedge(Actor actor, Rectangle tile, bool leftTileSolid, bool rightTileSolid)
        //{
        //    if (actor.MotionVector.X > 0
        //        && !rightTileSolid
        //        && tile.Bottom > actor.WorldPosition.Bottom()
        //        && actor.WorldPosition.Right() > tile.Center.X)
        //    {
        //        return new CollisionInfo(IsFacingLedge: true);
        //    }
        //    else if (actor.MotionVector.X < 0
        //       && !leftTileSolid
        //       && tile.Bottom > actor.WorldPosition.Bottom()
        //       && actor.WorldPosition.Left() < tile.Center.X)
        //    {
        //        return new CollisionInfo(IsFacingLedge: true);
        //    }
        //    else
        //        return new CollisionInfo();
        //}

        private void CheckTouchingGround(MovingWorldSprite actor, Rectangle collidingTile, CollisionInfo collisionInfo)
        {
            var correctedBottom = actor.BottomRight.Y + collisionInfo.YCorrection;

            if (actor.YSpeed >= 0
                && correctedBottom == collidingTile.Y
                && actor.BottomRight.X > collidingTile.X
                && actor.X <= collidingTile.Right)
            {
                collisionInfo.IsOnGround = true;
            }
        }
    }
}
