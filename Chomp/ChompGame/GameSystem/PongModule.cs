using ChompGame.Data;
using ChompGame.ROM;

namespace ChompGame.GameSystem
{
    class PongModule : Module, ILogicUpdateHandler
    {
        private readonly InputModule _inputModule;
        private readonly SpritesModule _spritesModule;

        public PongModule(MainSystem mainSystem, InputModule inputModule, SpritesModule spritesModule) 
            : base(mainSystem)
        {
            _inputModule = inputModule;
            _spritesModule = spritesModule;
        }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
        }

        public override void OnStartup()
        {
            var tileModule = GameSystem.GetModule<TileModule>();
            var graphicsModule = GameSystem.CoreGraphicsModule;
           
            var nameTableLoader = new DiskNBitPlaneLoader();
            nameTableLoader.Load(
               new DiskFile(ContentFolder.NameTables, "pong.nt"),
               tileModule.NameTable);

            var patternTableLoader = new DiskNBitPlaneLoader();
            patternTableLoader.Load(
                new DiskFile(ContentFolder.PatternTables, "pong.pt"),
                graphicsModule.PatternTable);

            _spritesModule.Sprites[0].X = 1;
            _spritesModule.Sprites[0].Y = 12;
            _spritesModule.Sprites[0].Tile = 1;

            _spritesModule.Sprites[1].X = 27;
            _spritesModule.Sprites[1].Y = 12;
            _spritesModule.Sprites[1].Tile = 2;

            _spritesModule.Sprites[2].X = 12;
            _spritesModule.Sprites[2].Y = 12;
            _spritesModule.Sprites[2].Tile = 3;

        }

        public void OnLogicUpdate()
        {
            var playerPaddle = _spritesModule.GetSprite(0);
            if (playerPaddle.Y > 0 && _inputModule.UpKey.IsDown())
                playerPaddle.Y--;
            else if (playerPaddle.Y < Specs.ScreenHeight-Specs.TileHeight && _inputModule.DownKey.IsDown())
                playerPaddle.Y++;


        }
    }
}
