using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class DoorController : ISpriteController
    {
        private SpriteDefinition _spriteDefinition;
        private SpritesModule _spritesModule;
        private GameByte _levelTimer;
        private readonly InputModule _inputModule;
        private GameByte _openState;
        private PlayerController _playerController;
        private ChompAudioService _audio;

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

        public DoorController(
            ChompGameModule gameModule,
            PlayerController playerController,
            SystemMemoryBuilder memoryBuilder)
        {
            _spriteDefinition = new SpriteDefinition(SpriteType.Door, memoryBuilder.Memory);
            _spritesModule = gameModule.SpritesModule;
            _levelTimer = gameModule.LevelTimer;
            _inputModule = gameModule.InputModule;
            _playerController = playerController;
            _audio = gameModule.AudioService;

            _openState = memoryBuilder.AddByte();

            WorldSprite = new WorldSprite(
                specs: _spritesModule.Specs,
                spriteDefinition: _spriteDefinition,
                memoryBuilder: memoryBuilder,
                spritesModule: _spritesModule,
                scroller: gameModule.WorldScroller);
        }

        public Sprite GetSprite() => WorldSprite.GetSprite();
        public void InitializeSprite(byte palette)
        {
            var sprite = GetSprite();
            WorldSprite.ConfigureSprite(sprite);
            sprite.Palette = 0; //todo
            sprite.Priority = false;
        }

        public void Update()
        {
            WorldSprite.UpdateSprite();
            if (_openState.Value == 0)
                return;

            if ((_levelTimer.Value % 2) == 0)
                _openState.Value++;

            if(_openState == 1)
            {
                var sprite = GetSprite();
                sprite.Tile = (byte)(_spriteDefinition.Tile + 1);
            }
            else if (_openState == 10)
            {
                var sprite = GetSprite();
                sprite.Visible = false;
            }
            else if (_openState == 20)
            {
                _audio.PlaySound(ChompAudioService.Sound.DoorClose);
                _playerController.GetSprite().Visible = false;

                var sprite = GetSprite();
                sprite.Visible = true;
                sprite.Tile = (byte)(_spriteDefinition.Tile + 1);
            }
            else if (_openState == 30)
            {
                var sprite = GetSprite();
                sprite.Tile = (byte)(_spriteDefinition.Tile);
                _openState.Value = 0;
            }

        }

        public bool CheckPlayerOpen()
        {
            if (_openState.Value != 0)
                return false;

            if (!_inputModule.Player1.UpKey.IsDown())
                return false;

            if (!_playerController.WorldSprite.Bounds.Intersects(WorldSprite.Bounds))
                return false;

            _openState.Value = 1;
            _playerController.OnOpenDoor();
            _audio.PlaySound(ChompAudioService.Sound.DoorOpen);
            return true;
        }
    }
}
