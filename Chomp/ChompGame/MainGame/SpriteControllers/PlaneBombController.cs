using ChompGame.Data.Memory;
using ChompGame.Extensions;

namespace ChompGame.MainGame.SpriteControllers
{
    class PlaneBombController : BombController
    {
        public PlaneBombController(ChompGameModule gameModule, PlayerController playerController, SystemMemoryBuilder memoryBuilder) 
            : base(gameModule, playerController, memoryBuilder)
        {

        }


        protected override void UpdateActive()
        {
            if (_bombState.Value >= BombState.Explode)
            {
                base.UpdateActive();
            }
            else if (_bombState.Value < BombState.RiseBegin)
            {
                if (!_isThrown.Value)
                {                 
                    if (_levelTimer.IsMod(8))
                        _bombState.Value++;

                    if (_bombState.Value == BombState.RiseBegin)
                        _bombState.Value = BombState.Idle;

                    _motion.SetXSpeed(-5);
                    if (_bombState.Value < BombState.IdleMid)
                        _motion.TargetYSpeed = 10;
                    else
                        _motion.TargetYSpeed = -10;

                    _motion.YAcceleration = 20;
                }
                else
                {
                    if (_motion.YSpeed < 0)
                        _motion.YSpeed = 0;
                    _motion.YAcceleration = 5;
                    _motion.TargetYSpeed = 20;
                }

                _motionController.Update();
            }
            else if(_bombState.Value == BombState.RiseBegin)
            {
                SetCarried();
            }
            else if(_bombState.Value == BombState.RiseEnd)
            {
                WorldSprite.X = _playerController.WorldSprite.X+8;
                WorldSprite.Y = _playerController.WorldSprite.Y;

                _playerController.CheckBombThrow(this);
            }
        }
    }
}
