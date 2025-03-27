using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.Graphics;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers.Bosses
{
    class Level6BossController : LevelBossController
    {
        private NibbleEnum<Phase> _phase;
        private BossPart _eye1, _eye2, _eye3, _eye4;
        private BossPart _leg1, _leg2, _leg3, _leg4, _leg5, _leg6;
      
        protected override bool AlwaysActive => true;
        protected override bool DestroyWhenFarOutOfBounds => false;
        protected override bool DestroyWhenOutOfBounds => false;

        private GameBit _forceScrollOn;
        private MaskedByte _scrollLock;

        public Level6BossController(ChompGameModule gameModule, WorldSprite player, EnemyOrBulletSpriteControllerPool<BossBulletController> bulletControllers, SystemMemoryBuilder memoryBuilder) : base(gameModule, player, bulletControllers, memoryBuilder)
        {
            _phase = new NibbleEnum<Phase>(new LowNibble(memoryBuilder));           
            memoryBuilder.AddByte();
          
            _eye1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye3 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));
            _eye4 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.LevelBoss, memoryBuilder.Memory));

            _leg1 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg2 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg3 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg4 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg5 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));
            _leg6 = CreatePart(memoryBuilder, new SpriteDefinition(SpriteType.BossLeg, memoryBuilder.Memory));

            _forceScrollOn = new GameBit(_worldScroller.Extra.Address, Bit.Bit7, memoryBuilder.Memory);
            _scrollLock = new MaskedByte(_worldScroller.Extra.Address, Bit.Right6, memoryBuilder.Memory);
        }

        private enum Phase : byte
        {
            BeforeBoss,
            GroundFall,
            ForceScroll,
            BossAppear,
            Reposition,
            ForceScrollChase
        }

        protected override int BossHP => GameDebug.BossTest ? 1 : 3;

        protected override string BossTiles { get; } =
            @"08AAAAAC00000
              8IIIIIIIAC000
              GIIIIIIIII900
              8IIIIIFFFF400
              GIIII90000000
              8IIIIIAAAA900
              GIIIIIIIII400
              03IIIIIF45000
              006FFF4000000";
           
        protected override void UpdatePartPositions()
        {
            _eye1.UpdatePosition(WorldSprite);
            _eye2.UpdatePosition(WorldSprite);
            _eye3.UpdatePosition(WorldSprite);
            _eye4.UpdatePosition(WorldSprite);
            _leg1.UpdatePosition(WorldSprite);
            _leg2.UpdatePosition(WorldSprite);
            _leg3.UpdatePosition(WorldSprite);
            _leg4.UpdatePosition(WorldSprite);
            _leg5.UpdatePosition(WorldSprite);
            _leg6.UpdatePosition(WorldSprite);
        }

        protected override string BlankBossTiles => "0";

        protected override void BeforeInitializeSprite()
        {
            SetPhase(Phase.BeforeBoss);
            base.BeforeInitializeSprite();
        }

        private void SetPhase(Phase p)
        {
      //      GameDebug.Watch1 = new DebugWatch("Boss X", () => WorldSprite.X);
         //   GameDebug.Watch2 = new DebugWatch("Boss Y", () => WorldSprite.Y);
            GameDebug.Watch3 = new DebugWatch("State Timer", () => _stateTimer.Value);

            _phase.Value = p;
            _stateTimer.Value = 0;

            if(p == Phase.BeforeBoss)
            {
                _gameModule.BossBackgroundHandler.ShowCoins = false;
                _paletteModule.BgColor = ColorIndex.Black;
                HideBoss();
                _scrollLock.Value = 6;
            }
            else if(p == Phase.BossAppear)
            {
                _musicModule.CurrentSong = GameSystem.MusicModule.SongName.Nemesis;
                SetBossTiles();              
                SetupBossParts();
                WorldSprite.Visible = false;
                WorldSprite.X = 320;
                WorldSprite.Y = 88;

                _motion.SetYSpeed(0);
                _motion.SetXSpeed(20);
            }    
            else if(p == Phase.ForceScroll)
            {
                _musicModule.CurrentSong = GameSystem.MusicModule.SongName.Nemesis;
                _scrollLock.Value = 45;
                _forceScrollOn.Value = true;
            }
        }

        private void SetupBossParts()
        {
            var eye1Sprite = _eye1.PrepareSprite(SpriteTileIndex.Enemy1);
            eye1Sprite.Tile2Offset = 1;
            eye1Sprite.FlipX = true;
            eye1Sprite.FlipY = false;
            _eye1.XOffset = 0;
            _eye1.YOffset = -8;
            eye1Sprite.Visible = true;

            var eye2Sprite = _eye2.PrepareSprite(SpriteTileIndex.Enemy1);
            eye2Sprite.Tile2Offset = 1;
            eye2Sprite.FlipX = true;
            eye2Sprite.FlipY = false;
            _eye2.XOffset = 9;
            _eye2.YOffset = -5;
            eye2Sprite.Visible = true;

            var eye3Sprite = _eye3.PrepareSprite(SpriteTileIndex.Enemy1);
            eye3Sprite.Tile2Offset = 1;
            eye3Sprite.FlipX = true;
            eye3Sprite.FlipY = true;
            _eye3.XOffset = 0;
            _eye3.YOffset = 8;
            eye1Sprite.Visible = true;

            var eye4Sprite = _eye4.PrepareSprite(SpriteTileIndex.Enemy1);
            eye4Sprite.Tile2Offset = 1;
            eye4Sprite.FlipX = true;
            eye4Sprite.FlipY = true;
            _eye4.XOffset = 8;
            _eye4.YOffset = 4;
            eye4Sprite.Visible = true;


            var leg1Sprite = _leg1.PrepareSprite(SpriteTileIndex.Enemy2);
            leg1Sprite.Tile2Offset = 1;
            leg1Sprite.FlipX = false;
            leg1Sprite.FlipY = false;
            leg1Sprite.Priority = false;
            _leg1.XOffset = 2;
            _leg1.YOffset = 16;
            leg1Sprite.Visible = true;

            var leg2Sprite = _leg2.PrepareSprite(SpriteTileIndex.Enemy2);
            leg2Sprite.Tile2Offset = 1;
            leg2Sprite.FlipX = false;
            leg2Sprite.FlipY = false;
            leg2Sprite.Priority = false;
            _leg2.XOffset = 8;
            _leg2.YOffset = 20;
            leg2Sprite.Visible = true;

            var leg3Sprite = _leg3.PrepareSprite(SpriteTileIndex.Enemy2);
            leg3Sprite.Tile2Offset = 1;
            leg3Sprite.FlipX = false;
            leg3Sprite.FlipY = false;
            leg3Sprite.Priority = false;
            _leg3.XOffset = 16;
            _leg3.YOffset = 21;
            leg3Sprite.Visible = true;

            var leg4Sprite = _leg4.PrepareSprite(SpriteTileIndex.Enemy2);
            leg4Sprite.Tile2Offset = 1;
            leg4Sprite.FlipX = false;
            leg4Sprite.FlipY = true;
            leg4Sprite.Priority = false;
            _leg4.XOffset = 2;
            _leg4.YOffset = -16;
            leg4Sprite.Visible = true;

            var leg5Sprite = _leg5.PrepareSprite(SpriteTileIndex.Enemy2);
            leg5Sprite.Tile2Offset = 1;
            leg5Sprite.FlipX = false;
            leg5Sprite.FlipY = true;
            leg5Sprite.Priority = false;
            _leg5.XOffset = 8;
            _leg5.YOffset = -20;
            leg5Sprite.Visible = true;

            var leg6Sprite = _leg6.PrepareSprite(SpriteTileIndex.Enemy2);
            leg6Sprite.Tile2Offset = 1;
            leg6Sprite.FlipX = false;
            leg6Sprite.FlipY = true;
            leg6Sprite.Priority = false;
            _leg6.XOffset = 16;
            _leg6.YOffset = -18;
            leg6Sprite.Visible = true;

        }

        protected override void UpdateActive()
        {
            if(_phase.Value == Phase.BeforeBoss)
            {
                WorldSprite.Visible = false;
                if (_player.X > 64)
                    SetPhase(Phase.GroundFall);
            }
            else if(_phase.Value == Phase.GroundFall)
            {
                if(_levelTimer.IsMod(16))
                {
                    _worldScroller.ModifyTiles(DestroyNextGround);
                    _stateTimer.Value++;
                }

                if (_stateTimer.Value == 12)
                    SetPhase(Phase.ForceScroll);
            }
            else if(_phase.Value == Phase.ForceScroll)
            {
                if (_levelTimer.IsMod(16))
                    RainBullet();

                if(_levelTimer.IsMod(24))
                {
                    _worldScroller.ModifyTiles(RefreshTilesForForcedScroll);
                    _stateTimer.Value++;
                }

                if (_player.X >= 400)
                    SetPhase(Phase.BossAppear);
            }
            else if(_phase.Value == Phase.BossAppear)
            {
                _motionController.Update();
                FadeIn();

                PositionBoss();

                if (WorldSprite.X > 360)
                {
                    _motion.TargetXSpeed = 0;
                    _motion.XAcceleration = 2;
                }
            }
        }

        private void PositionBoss()
        {
            _position.X = (byte)(WorldSprite.X + 123 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 77);
            UpdatePartPositions();

        }

        private void RainBullet()
        {
            var bullet = _bulletControllers.TryAddNew();
            if (bullet == null)
                return;

            bullet.DestroyOnCollision = true;

            bullet.WorldSprite.X = _player.X + _rng.Generate(4) * 4;
            bullet.WorldSprite.Y = 64;
            _audioService.PlaySound(ChompAudioService.Sound.Fireball);


            bullet.AcceleratedMotion.YAcceleration = 3;
            bullet.AcceleratedMotion.TargetYSpeed = 80; 
        }

        private void RefreshTilesForForcedScroll(NBitPlane tileMap, NBitPlane attributeMap)
        {
            int screenTileLeft = ((_worldScroller.ViewPane.Left / _gameModule.Specs.TileWidth)-0).NMod(tileMap.Width);
            int screenTileRight = (_worldScroller.ViewPane.Right / _gameModule.Specs.TileWidth).NMod(tileMap.Width);

            int column = (screenTileRight + 1).NMod(tileMap.Width);
            int bottom = tileMap.Height - 1;

            int pit = 0;

            while (column != screenTileLeft)
            {
                int tile2 = column.IsMod(2) ? 9 : 8;

                tileMap[column, bottom - 1] = 11;
                tileMap[column, bottom] = (byte)tile2;
                
                column = (column + 1).NMod(tileMap.Width);               
            }

            _audioService.PlaySound(ChompAudioService.Sound.Break);

            tileMap[screenTileLeft, bottom] = 0;
            tileMap[screenTileLeft + 1, bottom] = 0;
            tileMap[screenTileLeft, bottom - 1] = 0;
            tileMap[screenTileLeft + 1, bottom - 1] = 0;

            tileMap[screenTileLeft + 2, bottom] = 0;
            tileMap[screenTileLeft + 3, bottom] = 0;
            tileMap[screenTileLeft + 2, bottom - 1] = 0;
            tileMap[screenTileLeft + 3, bottom - 1] = 0;



            return;

        }

        private void DestroyNextGround(NBitPlane tileMap, NBitPlane attributeMap)
        {
          //  int screenTileLeft = _worldScroller.ViewPane.Left / _gameModule.Specs.TileWidth;

            int bottom = tileMap.Height-1;

            for(int x = 0; x < tileMap.Width; x += 2)
            {
                if (tileMap[x, bottom] != 0)
                {
                    _audioService.PlaySound(ChompAudioService.Sound.Break);

                    tileMap[x, bottom] = 0;
                    tileMap[x+1, bottom] = 0;
                    tileMap[x, bottom-1] = 0;
                    tileMap[x+1, bottom-1] = 0;
                    return;
                }
            }
        }

        private void FadeIn()
        {
            Theme theme = new Theme(_graphicsModule.GameSystem.Memory, ThemeType.CityBoss);
            var targetSpritePalette = _paletteModule.GetPalette(theme.Enemy1);
            var targetBossPalette = _paletteModule.GetPalette(theme.Background2);

            var spritePalette = _graphicsModule.GetSpritePalette(SpritePalette.Enemy1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 1);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 2);
            _paletteModule.FadeColor(spritePalette, targetSpritePalette, 3);

            var bossPalette = _paletteModule.BgPalette2;
            _paletteModule.FadeColor(bossPalette, targetBossPalette, 1);
            _paletteModule.FadeColor(bossPalette, targetBossPalette, 2);
            _paletteModule.FadeColor(bossPalette, targetBossPalette, 3);
        }

        protected override void UpdateDying()
        {
            _position.X = (byte)(WorldSprite.X + 123 - _tileModule.Scroll.X);
            _position.Y = (byte)(WorldSprite.Y - 77);          
        }

        private void CreateExplosions()
        {
            var x = _rng.Generate(5);
            var y = _rng.Generate(4);
            CreateExplosion((WorldSprite.X - 8) + x, WorldSprite.Y + y);
        }

        public override BombCollisionResponse HandleBombCollision(WorldSprite player)
        {
            _audioService.PlaySound(ChompAudioService.Sound.Lightning);           
            _hitPoints.Value--;
            WorldSprite.Status = WorldSpriteStatus.Dying;
         
            return BombCollisionResponse.Destroy;
        }

        public override bool CollidesWithBomb(WorldSprite bomb)
        {
            if (_phase.Value <= Phase.BossAppear)
                return false;

            return bomb.Bounds.Intersects(_eye1.WorldSprite.Bounds) || bomb.Bounds.Intersects(_eye2.WorldSprite.Bounds);
        }

        public override bool CollidesWithPlayer(PlayerController player)
        {
            if (_phase.Value <= Phase.BossAppear)
                return false;

            return false; //todo
        }
    }
}
