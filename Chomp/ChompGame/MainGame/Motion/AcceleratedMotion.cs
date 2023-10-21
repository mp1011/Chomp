using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Helpers;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame
{
    class AcceleratedMotion : IMotion
    {
        private GameByte _timer;
        private ByteVector _targetMotion;
        private PrecisionMotion _currentMotion;
        private NibblePoint _acceleration;
        public PrecisionMotion CurrentMotion => _currentMotion;

        public int TargetXSpeed
        {
            get => _targetMotion.X;
            set => _targetMotion.X = value;
        }

        public int TargetYSpeed
        {
            get => _targetMotion.Y;
            set => _targetMotion.Y = value;
        }

        public int XSpeed
        {
            get => _currentMotion.XSpeed;
            set => _currentMotion.XSpeed = value;
        }

        public int YSpeed
        {
            get => _currentMotion.YSpeed;
            set => _currentMotion.YSpeed = value;
        }

        public byte XAcceleration
        {
            get => _acceleration.X;
            set => _acceleration.X = value;
        }

        public byte YAcceleration
        {
            get => _acceleration.Y;
            set => _acceleration.Y = value;
        }


        public AcceleratedMotion(GameByte timer, SystemMemoryBuilder memoryBuilder)
        {
            _timer = timer;
            _targetMotion = new ByteVector(memoryBuilder.AddByte(), memoryBuilder.AddByte());
            _currentMotion = new PrecisionMotion(memoryBuilder);
            _acceleration = new NibblePoint(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddByte();
        }

        public void SetXSpeed(int speed)
        {
            XSpeed = speed;
            TargetXSpeed = speed;
        }

        public void SetYSpeed(int speed)
        {
            YSpeed = speed;
            TargetYSpeed = speed;
        }

        public void Apply(IWithPosition sprite)
        {
            if ((_timer.Value % 4) == 0)
            {
                _currentMotion.XSpeed = UpdateSpeed(_currentMotion.XSpeed, _targetMotion.X, _acceleration.X);
                _currentMotion.YSpeed = UpdateSpeed(_currentMotion.YSpeed, _targetMotion.Y, _acceleration.Y);
            }

            _currentMotion.Apply(sprite);
        }

        private int UpdateSpeed(int current, int target, byte acceleration)
        {
            if (current == target)
                return current;

            int newSpeed = current;

            if(current < target)
            {
                newSpeed = current + acceleration;
                if (newSpeed > target)
                    newSpeed = target;
            }
            else if(current > target)
            {
                newSpeed = current - acceleration;
                if (newSpeed < target)
                    newSpeed = target;
            }

            return newSpeed;
        }

        /// <summary>
        /// Adjust target x and y speed such that object will move toward target
        /// </summary>
        /// <param name="destination"></param>
        public void TargetTowards(WorldSprite source, WorldSprite destination, int speed)
        {
            TargetTowards(source, destination.Bounds.Center, speed);
        }

        public void Turn(int turnAngle, int speed)
        {
            var currentAngle = new Point(XSpeed, YSpeed);
            var newAngleDegrees = (currentAngle.Degrees() + turnAngle).NMod(360);
            var newAngle = GameMathHelper.PointFromAngle(newAngleDegrees, speed);
            SetXSpeed(newAngle.X);
            SetYSpeed(newAngle.Y);
        }

        public void TurnTowards(WorldSprite source, Point destination, int turnAngle, int speed)
        {
            var targetAngle = source.Bounds.Center
                .GetVectorTo(destination, speed);

            var currentAngle = new Point(XSpeed, YSpeed);

            if(XSpeed == 0 && YSpeed == 0)
            {
                SetXSpeed(targetAngle.X);
                SetYSpeed(targetAngle.Y);
                return;
            }

            var angleDifferenceA = (targetAngle.Degrees() - currentAngle.Degrees()).NMod(360);
            var angleDifferenceB = (currentAngle.Degrees() - targetAngle.Degrees()).NMod(360);
            var angleDifference = angleDifferenceA < angleDifferenceB ? angleDifferenceA : angleDifferenceB;

            Point newAngle = Point.Zero;
            if (angleDifference < turnAngle)
            {
                if (targetAngle.X != 0 || targetAngle.Y != 0)
                {
                    SetXSpeed(targetAngle.X);
                    SetYSpeed(targetAngle.Y);
                }
            }
            else if(angleDifference > 0)
            {
                var newAngleDegrees = (currentAngle.Degrees() + turnAngle).NMod(360);
                newAngle = GameMathHelper.PointFromAngle(newAngleDegrees, speed);
              
            }
            else
            {
                var newAngleDegrees = (currentAngle.Degrees() - turnAngle).NMod(360);
                newAngle = GameMathHelper.PointFromAngle(newAngleDegrees, speed);
            }

            if (newAngle.X != 0 || newAngle.Y != 0)
            {
                SetXSpeed(newAngle.X);
                SetYSpeed(newAngle.Y);
            }
        }

        /// <summary>
        /// Adjust target x and y speed such that object will move toward target
        /// </summary>
        /// <param name="destination"></param>
        public void TargetTowards(WorldSprite source, Point destination, int speed)
        {
            var src = source.Bounds.Center;         
            Point targetAngle = src.GetVectorTo(destination, speed);

            TargetXSpeed = targetAngle.X;
            TargetYSpeed = targetAngle.Y;
        }

        /// <summary>
        /// Adjust target x and y speed such that object will exactly land on the target and then stop
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="speed"></param>
        /// <returns>true when the object is at the destination</returns>
        public bool TargetTowardsExact(WorldSprite source, Point destination, int speed)
        {
            TargetTowards(source, destination, speed);

            if (source.Center.DistanceSquared(destination) < 4)
            {
                TargetXSpeed = 0;
                TargetYSpeed = 0;
                XSpeed = 0;
                YSpeed = 0;
                source.Center = destination;
                return true;
            }

            return false;
        }

        public void Stop()
        {
            XSpeed = 0;
            YSpeed = 0;
            TargetXSpeed = 0;
            TargetYSpeed = 0;
        }
    }

}
