using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class DoorController : ISpriteController
    {
        private SpriteDefinition _spriteDefinition;
        private SpriteTileTable _spriteTileTable;
        private SpritesModule _spritesModule;
        private GameByte _levelTimer;
        private readonly InputModule _inputModule;
        private MaskedByte _openState;
        private GameBit _doorType;

        private PlayerController _playerController;
        private ChompAudioService _audio;
        private ExitsModule _exitsModule;

        public ExitType DoorType
        {
            get => _doorType.Value
                ? ExitType.DoorForward
                : ExitType.DoorBack;
            set
            {
                _doorType.Value = (value == ExitType.DoorForward);
            }
        }

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

        public DoorController(
            ChompGameModule gameModule,
            PlayerController playerController,
            SystemMemoryBuilder memoryBuilder)
        {
            _spriteDefinition = new SpriteDefinition(SpriteType.Door, memoryBuilder.Memory);
            _spritesModule = gameModule.SpritesModule;
            _spriteTileTable = gameModule.SpriteTileTable;
            _levelTimer = gameModule.LevelTimer;
            _inputModule = gameModule.InputModule;
            _playerController = playerController;
            _audio = gameModule.AudioService;
            _exitsModule = gameModule.ExitsModule;

            _openState = memoryBuilder.AddMaskedByte(Bit.Right6);
            _doorType = new GameBit(_openState.Address, Bit.Bit6, memoryBuilder.Memory);
                
            WorldSprite = new WorldSprite(
                specs: _spritesModule.Specs,
                spriteTileTable: gameModule.SpriteTileTable,
                spriteDefinition: _spriteDefinition,
                memoryBuilder: memoryBuilder,
                spritesModule: _spritesModule,
                scroller: gameModule.WorldScroller,
                index: SpriteTileIndex.Door);
        }

        public Sprite GetSprite() => WorldSprite.GetSprite();
        public void InitializeSprite(byte palette)
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

            byte baseTile = _spriteTileTable.DoorTile; 
            if (WorldSprite.Status != WorldSpriteStatus.Active)
                return;

            WorldSprite.UpdateSprite();
            if (_openState.Value == 0)
                return;

            if(_openState == 1)
            {
                var sprite = GetSprite();
                sprite.Tile = (byte)(baseTile + 1);
            }
            else if (_openState == 10)
            {
                var sprite = GetSprite();
                sprite.Visible = false;
            }
            else if (_openState == 20)
            {
                _audio.PlaySound(ChompAudioService.Sound.DoorClose);
               
                var sprite = GetSprite();
                sprite.Visible = true;
                sprite.Tile = (byte)(baseTile + 1);
            }
            else if (_openState == 30)
            {
                var sprite = GetSprite();
                sprite.Tile = baseTile;
            }
            else if (_openState == 40)
            {
                _exitsModule.OnDoorEntered(DoorType);
                _openState.Value = 0;
            }

            if ((_levelTimer.Value % 2) == 0)
                _openState.Value++;
        }

        private void HideIfOutOfBounds()
        {
            var boundsCheck = WorldSprite.CheckInBounds();

            if (boundsCheck == BoundsCheck.FarOutOfBounds || boundsCheck == BoundsCheck.OutOfBounds)
            {
                GameDebug.DebugLog("Door hidden");
                WorldSprite.Hide();
            }
            else if (Status != WorldSpriteStatus.Active)
            {
                WorldSprite.Show();
                if (Status == WorldSpriteStatus.Active)
                {
                    InitializeSprite(0);
                }
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
