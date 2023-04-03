using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.MainGame.SpriteControllers.Base;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class ButtonController : ISpriteController
    {
        private SpriteDefinition _spriteDefinition;        
        private PlayerController _playerController;
        private DynamicBlockController _dynamicBlockController;
        private ChompAudioService _audio;
        private GameByte _state;

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
            _playerController = playerController;
            _audio = gameModule.AudioService;
            _dynamicBlockController = gameModule.DynamicBlocksController;

            _state = memoryBuilder.AddByte();
            
            WorldSprite = new WorldSprite(
                specs: gameModule.Specs,
                spriteDefinition: _spriteDefinition,
                memoryBuilder: memoryBuilder,
                spritesModule: gameModule.SpritesModule,
                scroller: gameModule.WorldScroller);
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

            var player = _playerController.WorldSprite;
            if(_state.Value == 0
                && _playerController.Motion.YSpeed > 0 
                && player.Bounds.Intersects(this.WorldSprite.Bounds))
            {
                player.Motion.YSpeed = -10;
                GetSprite().Tile = (byte)(Constants.ButtonTile + 1);
                _audio.PlaySound(ChompAudioService.Sound.ButtonPress);
                _dynamicBlockController.SwitchOffBlocks();
                _state.Value = 1;
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
                    InitializeSprite(0);
                }
            }
        }
    }

}
