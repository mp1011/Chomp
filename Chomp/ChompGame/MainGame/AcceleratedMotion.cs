using ChompGame.Data;

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

        public void Apply(WorldSprite sprite)
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
    }

}
