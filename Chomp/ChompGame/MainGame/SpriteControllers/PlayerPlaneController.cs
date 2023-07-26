using ChompGame.Data;
using ChompGame.Data.Memory;

namespace ChompGame.MainGame.SpriteControllers
{
    class PlayerPlaneController : PlayerController
    {
        private GameByte _headSpriteIndex;

        public PlayerPlaneController(ChompGameModule gameModule, SystemMemoryBuilder memoryBuilder) 
            : base(gameModule, memoryBuilder, SpriteModels.SpriteType.Plane, SpriteModels.SpriteTileIndex.Plane)
        {
            _headSpriteIndex = memoryBuilder.AddByte();
            Palette = 0;
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
            head.Palette = 1;
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
            CheckInput();
            _motionController.Update();
            CheckBounds();

            WorldSprite.UpdateSprite();
            PositionHeadSprite();
        }
    }
}
