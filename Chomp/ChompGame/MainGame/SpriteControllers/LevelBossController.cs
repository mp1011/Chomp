using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame.SpriteControllers
{
    class LevelBossController : EnemyController
    {
        private SpriteDefinition _jawSpriteDefinition;

        private TileModule _tileModule;
        private GameByteGridPoint _position;
        private WorldSprite _player;
        private GameByte _jawSpriteIndex;

        private enum Phase : byte
        {
            Init = 0,
            Test = 1
        }

        private GameByteEnum<Phase> _phase;

        public LevelBossController(ChompGameModule gameModule,
            WorldSprite player,
            SystemMemoryBuilder memoryBuilder) 
            : base(SpriteType.LevelBoss, gameModule, memoryBuilder)
        {
            _player = player;
            _tileModule = gameModule.TileModule;
            _position = gameModule.LevelBossPosition;
            _phase = new GameByteEnum<Phase>(memoryBuilder.AddByte());
            _jawSpriteIndex = memoryBuilder.AddByte();
            _jawSpriteDefinition = new SpriteDefinition(SpriteType.BossJaw, memoryBuilder.Memory);
        }

        protected override void BeforeInitializeSprite()
        {
            _position.X = 100;
            _position.Y = 16;

            WorldSprite.Y = 80;
            WorldSprite.X = 16;
            _hitPoints.Value = 4;
        }

        protected override void UpdateBehavior()
        {
            if (_phase.Value == Phase.Init)
            {
                //F = 15
                //16 = ~

                _tileModule.NameTable.SetFromString(0, 15,
                    @"13335
                            9$BBD
                            @#BBA
                            00$D0");


                WorldSprite.X = 24;
                WorldSprite.Y = 80;
                _phase.Value = Phase.Test;
                Motion.XAcceleration = 10;
                Motion.YAcceleration = 10;

                _jawSpriteIndex.Value = _spritesModule.GetFreeSpriteIndex();
                var jawSprite = _spritesModule.GetSprite(_jawSpriteIndex);
                jawSprite.Tile = _jawSpriteDefinition.Tile;
                jawSprite.SizeX = _jawSpriteDefinition.SizeX;
                jawSprite.SizeY = _jawSpriteDefinition.SizeY;
                jawSprite.Tile2Offset = 0;
                jawSprite.Visible = true;
            }
            else if (_phase.Value == Phase.Test)
            {
                _movingSpriteController.Update();
                if (_levelTimer.IsMod(32))
                {
                    Motion.TargetTowardsExact(WorldSprite, new Point(64, 90), 10);
                }

                _position.X = (byte)(WorldSprite.X - 8 - _tileModule.Scroll.X);
                _position.Y = (byte)(WorldSprite.Y - 66);

                var sprite = GetSprite();
                var jawSprite = _spritesModule.GetSprite(_jawSpriteIndex);
                jawSprite.X = (byte)(sprite.X - 7);
                jawSprite.Y = (byte)(sprite.Y + 8);
            }
        }
    }
}
