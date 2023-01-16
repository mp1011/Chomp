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

        public CollisionInfo DetectCollisions(WorldSprite actor)
        {
            int solidTileBeginIndex = 8;
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

                var tileAbove = _levelTileMap[
                   x.NMod(_levelTileMap.Width),
                   (y - 1).NMod(_levelTileMap.Height)];

                var tileBelow = _levelTileMap[
                    x.NMod(_levelTileMap.Width),
                    (y + 1).NMod(_levelTileMap.Height)];

                if (!tileBounds.Intersects(actorBounds))
                {
                    if (collisionInfo.IsOnGround
                        || tileAbove >= solidTileBeginIndex)
                    {
                        return;
                    }

                    if(actorBounds.Bottom == tileBounds.Top
                        && actorBounds.Right >= tileBounds.X 
                        && actorBounds.X <= tileBounds.Right 
                        && actor.YSpeed >= 0)
                    {
                        collisionInfo.IsOnGround = true;
                    }

                    return;
                }

                bool checkLeftCollision = actor.XSpeed > 0 && tileLeft < solidTileBeginIndex;
                bool checkRightCollision = actor.XSpeed < 0 && tileRight < solidTileBeginIndex;
                bool checkAbove = actor.YSpeed >= 0 && tileAbove < solidTileBeginIndex;
                bool checkBelow = actor.YSpeed < 0 && tileBelow < solidTileBeginIndex;

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
            });


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


        public CollisionInfo DetectCollisions2(WorldSprite actor)
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

        private void CheckCollisionCorrectionX(WorldSprite actor, CollisionInfo correction,
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

        private void CheckCollisionCorrectionY(WorldSprite actor, CollisionInfo correction, CollisionInfo collisionInfo,
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

        private void CheckTouchingGround(WorldSprite actor, Rectangle collidingTile, CollisionInfo collisionInfo)
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
