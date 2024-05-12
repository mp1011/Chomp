using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class ButtonController : ISpriteController
    {
        private SpriteDefinition _spriteDefinition;
        private SpriteTileTable _spriteTileTable;
        private PlayerController _playerController;
        private DynamicBlockController _dynamicBlockController;
        private ChompAudioService _audio;
        private GameByte _state;
        private GameByte _levelTimer;

        public SpritePalette Palette => SpritePalette.Platform;
        public byte SpriteIndex
        {
            get => WorldSprite.SpriteIndex;
            set => WorldSprite.SpriteIndex.Value = value;
        }

        public WorldSpriteStatus Status
        {
            get => WorldSprite.Status;
            set => WorldSprite.Status = value;
        }

        public WorldSprite WorldSprite { get; }

        public byte DestructionBitOffset
        {
            get => 0;
            set { }
        }

        public FallCheck FallCheck { get; set; } 

        public ButtonController(
            ChompGameModule gameModule,
            PlayerController playerController,
            SystemMemoryBuilder memoryBuilder)
        {
            _spriteDefinition = new SpriteDefinition(SpriteType.Button, memoryBuilder.Memory);
            _spriteTileTable = gameModule.SpriteTileTable;
            _playerController = playerController;
            _audio = gameModule.AudioService;
            _dynamicBlockController = gameModule.DynamicBlocksController;
            _levelTimer = gameModule.LevelTimer;
            _state = memoryBuilder.AddByte();
            
            WorldSprite = new WorldSprite(
                specs: gameModule.Specs,
                spriteTileTable: _spriteTileTable,
                spriteDefinition: _spriteDefinition,
                memoryBuilder: memoryBuilder,
                spritesModule: gameModule.SpritesModule,
                scroller: gameModule.WorldScroller,
                index: SpriteTileIndex.Button,
                palette: new TwoBitEnum<SpritePalette>(memoryBuilder.Memory, 0,0));
        }

        public Sprite GetSprite() => WorldSprite.GetSprite();
        public void InitializeSprite()
        {
            var sprite = GetSprite();
            WorldSprite.ConfigureSprite(sprite);
            sprite.Palette = 0; //todo
            sprite.FlipX = false;
            sprite.FlipY = false;
            sprite.Priority = true;
        }

        public void Update()
        {
            HideIfOutOfBounds();

            var player = _playerController.WorldSprite;
            if(_state.Value == 0
                && _playerController.Motion.YSpeed > 0 
                && player.Bounds.Intersects(this.WorldSprite.Bounds))
            {
                _playerController.Motion.YSpeed = -30;
                WorldSprite.Tile = (byte)(_spriteTileTable.GetTile(SpriteTileIndex.Button) + 1);
                _audio.PlaySound(ChompAudioService.Sound.ButtonPress);
                _dynamicBlockController.ToggleSwitchBlocks();
                _state.Value = 1;
            }
            else if(_state.Value > 0 && _levelTimer.IsMod(8))
            {
                if (_state.Value == 7)
                {
                    WorldSprite.Tile = (byte)(_spriteTileTable.GetTile(SpriteTileIndex.Button));
                    _state.Value = 0;
                }
                else
                    _state.Value++;
                
            }

        }

        private void HideIfOutOfBounds()
        {
            var boundsCheck = WorldSprite.CheckInBounds();

            if (boundsCheck == BoundsCheck.FarOutOfBounds || boundsCheck == BoundsCheck.OutOfBounds)
            {
                WorldSprite.Hide();
            }
            else if (Status != WorldSpriteStatus.Active)
            {
                WorldSprite.Show();
                if (Status == WorldSpriteStatus.Active)
                {
                    InitializeSprite();
                }
            }
        }
    }

}
