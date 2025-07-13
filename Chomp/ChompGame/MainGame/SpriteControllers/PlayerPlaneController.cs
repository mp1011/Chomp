using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SceneModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class PlayerPlaneController : PlayerController
    {
        private GameByte _headSpriteIndex;
        private ExitsModule _exitModule;

        protected override bool AlwaysActive => true;
        
        public PlayerPlaneController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder) 
            : base(gameModule, memoryBuilder, SpriteModels.SpriteType.Plane, SpriteModels.SpriteTileIndex.Plane)
        {
            _headSpriteIndex = memoryBuilder.AddByte();
            _exitModule = gameModule.ExitsModule;
            Palette = SpritePalette.Platform;
        }

        protected override void OnSpriteCreated(Sprite sprite)
        {
            _headSpriteIndex.Value = _spritesModule.GetFreeSpriteIndex();
            Sprite head = _spritesModule.GetSprite(_headSpriteIndex.Value);
            head.SizeX = 1;
            head.SizeY = 1;
            head.FlipX = false;
            head.FlipY = false;
            head.Tile = _spriteTileTable.GetTile(SpriteModels.SpriteTileIndex.Player);
            head.Visible = true;
            head.Palette = SpritePalette.Player;
        }

        public override bool CollidesWith(IWithBounds other)
        {
            if (_statusBar.Health == 0)
                return false;

            if (base.CollidesWith(other))
                return true;

            var headSprite = _spritesModule.GetSprite(_headSpriteIndex.Value);
            return other.Bounds.Intersects(headSprite.Bounds);
        }

        private void CheckInput()
        {
            _inputModule.OnLogicUpdate();
            if (_inputModule.Player1.UpKey.IsDown())
            {
                AcceleratedMotion.TargetYSpeed = -_motionController.WalkSpeed;
                AcceleratedMotion.YAcceleration = _motionController.WalkAccel;
            }
            else if (_inputModule.Player1.DownKey.IsDown())
            {
                AcceleratedMotion.TargetYSpeed = _motionController.WalkSpeed;
                AcceleratedMotion.YAcceleration = _motionController.WalkAccel;
            }
            else
            {
                AcceleratedMotion.TargetYSpeed = 0;
                AcceleratedMotion.YAcceleration = _motionController.BrakeAccel;
            }

            if (_inputModule.Player1.LeftKey.IsDown())
            {
                AcceleratedMotion.TargetXSpeed = -_motionController.WalkSpeed;
                AcceleratedMotion.XAcceleration = _motionController.WalkAccel;
            }
            else if (_inputModule.Player1.RightKey.IsDown())
            {
                AcceleratedMotion.TargetXSpeed = _motionController.WalkSpeed;
                AcceleratedMotion.XAcceleration = _motionController.WalkAccel;
            }
            else
            {
                AcceleratedMotion.TargetXSpeed = 0;
                AcceleratedMotion.XAcceleration = _motionController.BrakeAccel;
            }
        }

        public override void OnBossDead()
        {
            _bossDead.Value = true;
        }

        private void PositionHeadSprite()
        {

            var headSprite = _spritesModule.GetSprite(_headSpriteIndex.Value);
            var sprite = GetSprite();
            headSprite.X = (byte)(sprite.X + 2);
            headSprite.Y = (byte)(sprite.Y - 4);
        }

        private void CheckBounds()
        {
            if (WorldSprite.X < -4)
            {
                WorldSprite.X = -4;
                AcceleratedMotion.SetXSpeed(0);
            }
            else if (WorldSprite.X > _specs.ScreenWidth - 4)
            {
                WorldSprite.X = _specs.ScreenWidth - 4;
                AcceleratedMotion.SetXSpeed(0);
            }

            if (WorldSprite.Y < Constants.StatusBarHeight + 4)
            {
                WorldSprite.Y = Constants.StatusBarHeight + 4;
                AcceleratedMotion.SetYSpeed(0);
            }
            else if (WorldSprite.Y > _specs.ScreenHeight - 4)
            {
                WorldSprite.Y = _specs.ScreenHeight - 4;
                AcceleratedMotion.SetYSpeed(0);
            }
        }

        protected override void UpdateActive()
        {
            if (_statusBar.Health == 0)
            {
                AcceleratedMotion.TargetYSpeed = _motionController.WalkSpeed;
                AcceleratedMotion.YAcceleration = _motionController.WalkAccel;
                Motion.XSpeed = 0;
                AcceleratedMotion.TargetXSpeed = 0;
                _motionController.Update();
                PositionHeadSprite();
                return;
            }

            if (_bossDead)
            {
                AcceleratedMotion.TargetYSpeed = 0;
                AcceleratedMotion.YAcceleration = _motionController.BrakeAccel;
                AcceleratedMotion.TargetXSpeed = _motionController.WalkSpeed*2;
                AcceleratedMotion.XAcceleration = _motionController.WalkAccel;

                _motionController.Update();
                WorldSprite.UpdateSprite();
                PositionHeadSprite();

                if(WorldSprite.X > _specs.ScreenWidth)
                {
                    AcceleratedMotion.SetXSpeed(0);
                    _exitModule.GotoNextLevel();
                    WorldSprite.Y = _specs.ScreenHeight;
                }
                return;
            }

            CheckInput();
            _motionController.Update();
            CheckBounds();

            WorldSprite.UpdateSprite();
            PositionHeadSprite();

            CheckAfterHitInvincability();
            var headSprite = _spritesModule.GetSprite(_headSpriteIndex.Value);
            headSprite.Visible = Visible;
        }
    }
}
