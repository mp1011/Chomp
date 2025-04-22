using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class Boss7BulletController : BossBulletController
    {
        private WorldScroller _scroller;
        private NibbleEnum<BulletMode> _mode;
        private HighNibble _stateTimer;
        private WorldSprite _player;
        private ExtendedPoint _initialPosition;

        public BulletMode Mode
        {
            get => _mode.Value;
            set => _mode.Value = value;
        }

        public enum BulletMode : byte
        {
            Normal,
            RandomAimed,
        }
       
        public Boss7BulletController(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            WorldSprite player,
            SpriteType spriteType = SpriteType.BossBullet) : base(gameModule, memoryBuilder, true, spriteType)
        {
            _player = player;
            _scroller = gameModule.WorldScroller;
            _mode = new NibbleEnum<BulletMode>(new LowNibble(memoryBuilder));
            memoryBuilder.AddByte();

            _initialPosition = memoryBuilder.AddExtendedPoint(
                new GameBit(memoryBuilder.CurrentAddress - 1, Bit.Bit4, memoryBuilder.Memory),
                new GameBit(memoryBuilder.CurrentAddress - 1, Bit.Bit5, memoryBuilder.Memory));
        }


        protected override void UpdateActive() 
        {
            if(_mode.Value == BulletMode.RandomAimed)
            {
                if(_state.Value == 0)
                {
                    _initialPosition.X = WorldSprite.X - _scroller.ViewPane.Left;
                    _initialPosition.Y = WorldSprite.Y;
                }
                else
                {
                    WorldSprite.X = _scroller.ViewPane.Left + _initialPosition.X;
                    WorldSprite.Y = _initialPosition.Y;
                }

                WorldSprite.Visible = !WorldSprite.Visible;

                if(_levelTimer.IsMod(16))
                    _state.Value++;

                if(_state.Value == 8)
                {
                    _state.Value = 0;
                    _mode.Value = BulletMode.Normal;

                    AcceleratedMotion.YAcceleration = 6;
                    AcceleratedMotion.XAcceleration = 6;
                    AcceleratedMotion.TargetTowards(WorldSprite, _player.Bounds.Center, 80);
                    WorldSprite.Visible = true;
                }

            }
            else 
                base.UpdateActive();
        }
    }
}
