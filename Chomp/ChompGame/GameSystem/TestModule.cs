using ChompGame.Data;
using ChompGame.ROM;
using Microsoft.Xna.Framework.Input;
using System;

namespace ChompGame.GameSystem
{
    class TestModule : Module, ILogicUpdateHandler
    {
        private TileModule _tileModule;
        private SpritesModule _spritesModule;

        public TestModule(MainSystem mainSystem) : base(mainSystem) { }

        public override void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
        }

        public override void OnStartup()
        {
            _tileModule = GameSystem.GetModule<TileModule>();
            _spritesModule = GameSystem.GetModule<SpritesModule>();

            _spritesModule.Sprites[0].Tile = 4;
            _spritesModule.Sprites[0].X = 12;
            _spritesModule.Sprites[0].Y = 0;

            _spritesModule.Sprites[1].Tile = 5;
            _spritesModule.Sprites[1].X = 20;
            _spritesModule.Sprites[1].Y = 16;

            _spritesModule.Sprites[2].Tile = 6;
            _spritesModule.Sprites[2].X = 24;
            _spritesModule.Sprites[2].Y = 20;


            var tileModule = GameSystem.GetModule<TileModule>();
            var graphicsModule = GameSystem.CoreGraphicsModule;

            var nameTableLoader = new DiskNBitPlaneLoader();
            nameTableLoader.Load(
               new DiskFile(ContentFolder.NameTables, "test.nt"),
               tileModule.NameTable);

            var patternTableLoader = new DiskNBitPlaneLoader();
            patternTableLoader.Load(
                new DiskFile(ContentFolder.PatternTables, "test_4color.pt"),
                graphicsModule.PatternTable);
        }

        private bool wasLeftDown;
        private bool wasRightDown;

        private byte noteTimer;
        private void Note(ushort value)
        {
            var audio = GameSystem.GetModule<AudioModule>();
            audio.Channels[0].Value = value;
            audio.Channels[0].Playing = true;
            noteTimer = 10;
        }

        public void OnLogicUpdate()
        {
            KeyboardState state = Keyboard.GetState();
            bool leftKey = state.IsKeyDown(Keys.Left);
            bool righKey = state.IsKeyDown(Keys.Right);
            bool upKey = state.IsKeyDown(Keys.Up);
            bool downKey = state.IsKeyDown(Keys.Down);

            if(noteTimer > 0)
            {
                noteTimer--;
                if(noteTimer == 0)
                {
                    var audio = GameSystem.GetModule<AudioModule>();
                    audio.Channels[0].Playing = false;

                }
            }

            if (state.IsKeyDown(Keys.D1))
                Note(200);
            if (state.IsKeyDown(Keys.D2))
                Note(220); // 10
            if (state.IsKeyDown(Keys.D3))
                Note(250); // 15
            if (state.IsKeyDown(Keys.D4))
                Note(260); // 5
            if (state.IsKeyDown(Keys.D5))
                Note(300); // 20
            if (state.IsKeyDown(Keys.D6))
                Note(330); // 15
            if (state.IsKeyDown(Keys.D7))
                Note(380); // 25
            if (state.IsKeyDown(Keys.D8))
                Note(400); // 10


            //if (state.IsKeyDown(Keys.D1))
            //    Note(100);
            //if (state.IsKeyDown(Keys.D2))
            //    Note(110); // 10
            //if (state.IsKeyDown(Keys.D3))
            //    Note(125); // 15
            //if (state.IsKeyDown(Keys.D4))
            //    Note(130); // 5
            //if (state.IsKeyDown(Keys.D5))
            //    Note(150); // 20
            //if (state.IsKeyDown(Keys.D6))
            //    Note(165); // 15
            //if (state.IsKeyDown(Keys.D7))
            //    Note(190); // 25
            //if (state.IsKeyDown(Keys.D8))
            //    Note(200); // 10
            //if (state.IsKeyDown(Keys.D9))
            //    Note(900);
            //if (state.IsKeyDown(Keys.D0))
            //    Note(1000);



            if (state.IsKeyDown(Keys.S))
            {
                if (leftKey)
                    _spritesModule.Sprites[0].X--;
                else if (righKey)
                    _spritesModule.Sprites[0].X++;

                if (upKey)
                    _spritesModule.Sprites[0].Y--;
                else if (downKey)
                    _spritesModule.Sprites[0].Y++;
            }
            else if (state.IsKeyDown(Keys.D))
            {
                if (leftKey)
                    _spritesModule.Sprites[1].X--;
                else if (righKey)
                    _spritesModule.Sprites[1].X++;

                if (upKey)
                    _spritesModule.Sprites[1].Y--;
                else if (downKey)
                    _spritesModule.Sprites[1].Y++;
            }
            else if (state.IsKeyDown(Keys.F))
            {
                if (leftKey)
                    _spritesModule.Sprites[2].X--;
                else if (righKey)
                    _spritesModule.Sprites[2].X++;

                if (upKey)
                    _spritesModule.Sprites[2].Y--;
                else if (downKey)
                    _spritesModule.Sprites[2].Y++;
            }

            else if (state.IsKeyDown(Keys.LeftShift))
            {
                if (leftKey)
                    _tileModule.Scroll.X++;

                if (righKey)
                    _tileModule.Scroll.X--;
            }
            else
            {
                if (leftKey && !wasLeftDown)
                    _tileModule.Scroll.X++;

                if (righKey && !wasRightDown)
                    _tileModule.Scroll.X--;
            }

            wasLeftDown = leftKey;
            wasRightDown = righKey;

        }

       
    }
}
