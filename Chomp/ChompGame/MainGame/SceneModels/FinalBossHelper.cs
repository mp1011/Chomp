using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteControllers;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame.SceneModels
{
    class FinalBossHelper
    {
        private readonly ChompGameModule _gameModule;
        private readonly NBitPlane _masterPatternTable;
        private readonly SceneSpriteControllers _sceneSpriteControllers;
        private readonly SpriteTileTable _spriteTileTable;
        private readonly CoreGraphicsModule _graphicsModule;
        private readonly SystemMemory _memory;
        private readonly GameShort _addr;

        public FinalBossHelper(
            ChompGameModule gameModule,
            SystemMemoryBuilder memoryBuilder,
            NBitPlane masterPatternTable, 
            SceneSpriteControllers sceneSpriteControllers,
            SpriteTileTable spriteTileTable,
            CoreGraphicsModule graphicsModule,
            SystemMemory memory)
        {
            _gameModule = gameModule;
            _masterPatternTable = masterPatternTable;
            _sceneSpriteControllers = sceneSpriteControllers;
            _spriteTileTable = spriteTileTable;
            _graphicsModule = graphicsModule;
            _memory = memory;
            _addr = memoryBuilder.AddShort();
            _addr.Value = (ushort)memoryBuilder.CurrentAddress;
        }

        public ICollidableSpriteControllerPool SetEnemy(EnemyIndex enemy)
        {
            switch(enemy)
            {
                case EnemyIndex.Lizard:
                    CopyEnemyVram(2, 0);
                    var enemies = SetSpriteControllers(SpriteType.Lizard);
                    PlaceEnemies(enemies);
                    return enemies;
                case EnemyIndex.Bird:
                    CopyEnemyVram(8, 0);
                    enemies = SetSpriteControllers(SpriteType.Bird);
                    PlaceEnemies(enemies);
                    return enemies;
                case EnemyIndex.Ogre:
                    CopyEnemyVram(12, 0);
                    enemies = SetSpriteControllers(SpriteType.Ogre);
                    PlaceEnemies(enemies);
                    return enemies;
                default:
                    return null;
            }
        }

        public void SetPhase2Sprite()
        {
            CopyEnemyVram(8, 9);
        }

        private void PlaceEnemies(ICollidableSpriteControllerPool enemies)
        {
            PlaceEnemy(enemies,24, 96);
            PlaceEnemy(enemies, 96, 96);
            PlaceEnemy(enemies, 34, 112);
            PlaceEnemy(enemies, 106, 112);

        }

        private void PlaceEnemy(ICollidableSpriteControllerPool enemies, int x, int y)
        {

            var enemy = enemies.TryAddNew();
            enemy.WorldSprite.X = x;
            enemy.WorldSprite.Y = y;
            enemy.WorldSprite.UpdateSprite();
        }

        private ICollidableSpriteControllerPool SetSpriteControllers(SpriteType spriteType)
        {
            _gameModule.GameRAM.CurrentAddress = (int)_addr.Value;
            var memoryBuilder = new SystemMemoryBuilder(_memory, _gameModule.Specs, _gameModule.GameRAM);

            WorldSprite player = _sceneSpriteControllers.Player.WorldSprite;

            ICollidableSpriteControllerPool enemy=null, extra=null;
            switch(spriteType)
            {
                case SpriteType.Lizard:

                    extra = new EnemyOrBulletSpriteControllerPool<BulletController>(
                                     6,
                                     _gameModule.SpritesModule,
                                     () => new BulletController(_gameModule, memoryBuilder, SpriteType.LizardBullet));

                    enemy = new EnemyOrBulletSpriteControllerPool<LizardEnemyController>(
                        6,
                        _gameModule.SpritesModule,
                        () => new LizardEnemyController(extra, SpriteTileIndex.Enemy2, _gameModule, player, memoryBuilder));
                    break;
                case SpriteType.Bird:
                    // needed as the number of pools is fixed 
                    extra = new EnemyOrBulletSpriteControllerPool<BulletController>(1, _gameModule.SpritesModule,() => new BulletController(_gameModule, memoryBuilder, SpriteType.LizardBullet));

                    enemy = new EnemyOrBulletSpriteControllerPool<BirdEnemyController>(
                    6,
                    _gameModule.SpritesModule,
                    () => new BirdEnemyController(player, _gameModule, memoryBuilder, SpriteTileIndex.Enemy2));
                    break;
                case SpriteType.Ogre:
                    extra = new EnemyOrBulletSpriteControllerPool<OgreBulletController>(
                                     6,
                                     _gameModule.SpritesModule,
                                     () => new OgreBulletController(_gameModule, memoryBuilder, SpriteTileIndex.Extra2));

                    enemy = new EnemyOrBulletSpriteControllerPool<OgreController>(
                        6,
                        _gameModule.SpritesModule,
                        () => new OgreController(extra, SpriteTileIndex.Enemy2, _gameModule, memoryBuilder, player));
                    break;
            }

            _sceneSpriteControllers.ReplaceEnemyController2(enemy, extra);
            return enemy;

        }

        private void CopyEnemyVram(int x, int y)
        {
            var tileIndex = _spriteTileTable.GetTile(SpriteTileIndex.Enemy2);
            int destY = tileIndex / _graphicsModule.Specs.PatternTableTilesAcross;
            int destX = tileIndex % _graphicsModule.Specs.PatternTableTilesAcross;

            _masterPatternTable.CopyTilesTo(
                   destination: _graphicsModule.SpritePatternTable,
                   source: new InMemoryByteRectangle(x, y, 4, 2),
                   destinationPoint: new Point(destX, destY),
                   _graphicsModule.Specs,
                   _memory);
        }
    }
}
