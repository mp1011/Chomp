using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using System;

namespace ChompGame.Helpers
{
    public class CollisionInfo
    {
        public int XCorrection { get; set; }
        public int YCorrection { get; set; }
        public bool IsOnGround { get; set; }
    }

    class CollisionDetector 
    {
        private readonly TileModule _tileModule;
        private readonly Specs _specs;

        public CollisionDetector(TileModule tileModule, Specs specs)
        {
            _specs = specs;
            _tileModule = tileModule;
        }

        public bool CheckCollision(MovingSprite s1, MovingSprite s2)
        {
            return s1.Bounds.Intersects(s2.Bounds);
        }

        public CollisionInfo DetectCollisions(MovingSprite actor)
        {
            var collisionInfo = new CollisionInfo();

            var topLeftTile = actor.TopLeft
                .Divide(_specs.TileWidth)
                .Add(-2, -2);


            var bottomRightTile = actor.BottomRight
                                        .Divide(_specs.TileWidth)
                                        .Add(2, 2);

            _tileModule.NameTable.ForEach(topLeftTile, bottomRightTile, (x, y, t) =>
            {
                if (t == 0)
                    return;

                var tileBounds = new InMemoryByteRectangle(
                    x * _specs.TileWidth, 
                    y * _specs.TileHeight, 
                    _specs.TileWidth, 
                    _specs.TileHeight);

                var tileLeft = _tileModule.NameTable[
                    (x - 1).NMod(_tileModule.NameTable.Width), 
                    y.NMod(_tileModule.NameTable.Height)];
                
                var tileRight = _tileModule.NameTable[
                    (x + 1).NMod(_tileModule.NameTable.Width), 
                    y.NMod(_tileModule.NameTable.Height)];

                bool checkLeftCollision = actor.XSpeed > 0 && tileLeft == 0;
                bool checkRightCollision = actor.XSpeed < 0 && tileRight == 0;

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

            _tileModule.NameTable.ForEach(topLeftTile, bottomRightTile, (x, y, t) =>
            {
                if (t == 0)
                    return;

                var tileAbove = _tileModule.NameTable[x, y - 1];
                var tileBelow = _tileModule.NameTable[x, y + 1];
                var tileLeft = _tileModule.NameTable[x - 1, y];
                var tileRight = _tileModule.NameTable[x + 1, y];

                var tileBounds = new InMemoryByteRectangle(
                    x * _specs.TileWidth,
                    y * _specs.TileHeight,
                    _specs.TileWidth,
                    _specs.TileHeight);

                bool checkAbove = actor.YSpeed >= 0 && tileAbove == 0;
                bool checkBelow = actor.YSpeed < 0 && tileBelow == 0;

                CheckCollisionCorrectionY(actor, collisionInfo, collisionInfo, tileBounds,
                    tileLeft != 0, tileRight != 0, checkAbove, checkBelow);
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

        private void CheckCollisionCorrectionX(MovingSprite actor, CollisionInfo correction,
            ByteRectangle bounds, bool checkLeftCollision, bool checkRightCollision)
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

        private void CheckCollisionCorrectionY(MovingSprite actor, CollisionInfo correction, CollisionInfo collisionInfo,
            ByteRectangle bounds, bool leftSolid, bool rightSolid, bool checkAbove, bool checkBelow)
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

        private void CheckTouchingGround(MovingSprite actor, ByteRectangle collidingTile, CollisionInfo collisionInfo)
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
