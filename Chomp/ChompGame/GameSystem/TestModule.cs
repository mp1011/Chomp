using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.ROM;
using Microsoft.Xna.Framework.Input;
using System;

namespace ChompGame.GameSystem
{
    class TestModule : Module, IMasterModule
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

            var sprite0 = _spritesModule.GetSprite(0);
            sprite0.Tile = 4;
            sprite0.X = 12;
            sprite0.Y = 0;

            //_spritesModule.Sprites[1].Tile = 5;
            //_spritesModule.Sprites[1].X = 20;
            //_spritesModule.Sprites[1].Y = 16;

            //_spritesModule.Sprites[2].Tile = 6;
            //_spritesModule.Sprites[2].X = 24;
            //_spritesModule.Sprites[2].Y = 20;


            var tileModule = GameSystem.GetModule<TileModule>();
            var graphicsModule = GameSystem.CoreGraphicsModule;

            var nameTableLoader = new DiskNBitPlaneLoader();
            nameTableLoader.Load(
               new DiskFile(ContentFolder.NameTables, "test.nt"),
               tileModule.NameTable);

            var patternTableLoader = new DiskNBitPlaneLoader();
            patternTableLoader.Load(
                new DiskFile(ContentFolder.PatternTables, "test_4color.pt"),
                graphicsModule.SpritePatternTable);
        }

        private bool wasLeftDown;
        private bool wasRightDown;
        
        private byte noteTimer;
        private void Note(int basis, int octave, int semitone)
        {
          
            var thisOctave = basis * Math.Pow(2, octave);
            var frequency = thisOctave * Math.Pow(2, (double)semitone / 12.0);

            throw new NotImplementedException();
            //var audio = GameSystem.GetModule<AudioModule>();
            //audio.Channels[0].Value = (ushort)(frequency);
            //audio.Channels[0].Volume=200;
            //audio.Channels[0].Playing = true;
            //noteTimer = 10;
        }

        private int _octave = 1;
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
                    throw new NotImplementedException();
                    //var audio = GameSystem.GetModule<BaseAudioModule>();
                    //audio.Channels[0].Playing = false;
                }
            }

            int lowOctave = 110;

            if (state.IsKeyDown(Keys.Q))
                Note(lowOctave,_octave,0);
            if (state.IsKeyDown(Keys.W))
                Note(lowOctave, _octave, 1);
            if (state.IsKeyDown(Keys.E))
                Note(lowOctave, _octave, 2);
            if (state.IsKeyDown(Keys.R))
                Note(lowOctave, _octave, 3);
            if (state.IsKeyDown(Keys.T))
                Note(lowOctave, _octave, 4);
            if (state.IsKeyDown(Keys.Y))
                Note(lowOctave, _octave, 5);
            if (state.IsKeyDown(Keys.U))
                Note(lowOctave, _octave, 6);
            if (state.IsKeyDown(Keys.I))
                Note(lowOctave, _octave, 7);
            if (state.IsKeyDown(Keys.O))
                Note(lowOctave, _octave, 8);
            if (state.IsKeyDown(Keys.P))
                Note(lowOctave, _octave, 9);
            if (state.IsKeyDown(Keys.OemOpenBrackets))
                Note(lowOctave, _octave, 10);
            if (state.IsKeyDown(Keys.OemCloseBrackets))
                Note(lowOctave, _octave, 11);
            if (state.IsKeyDown(Keys.OemPipe))
                Note(lowOctave, _octave, 12);


            if (!wasLeftDown && state.IsKeyDown(Keys.Left))
                _octave--;
            if (!wasRightDown && state.IsKeyDown(Keys.Right))
                _octave++;

    

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
                var sprite0 = _spritesModule.GetSprite(0);

                if (leftKey)
                    sprite0.X--;
                else if (righKey)
                    sprite0.X++;

                if (upKey)
                    sprite0.Y--;
                else if (downKey)
                    sprite0.Y++;
            }
            else if (state.IsKeyDown(Keys.D))
            {
                var sprite1 = _spritesModule.GetSprite(0);

                if (leftKey)
                    sprite1.X--;
                else if (righKey)
                    sprite1.X++;

                if (upKey)
                    sprite1.Y--;
                else if (downKey)
                    sprite1.Y++;
            }
            else if (state.IsKeyDown(Keys.F))
            {
                var sprite2 = _spritesModule.GetSprite(0);

                if (leftKey)
                    sprite2.X--;
                else if (righKey)
                    sprite2.X++;

                if (upKey)
                    sprite2.Y--;
                else if (downKey)
                    sprite2.Y++;
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

        public void OnVBlank()
        {
            _tileModule.OnVBlank();
            _spritesModule.OnVBlank();
        }

        public void OnHBlank()
        {
            _tileModule.OnHBlank();
            _spritesModule.OnHBlank();
        }

        public byte GetPalette(int pixel)
        {
            return 0;
        }
    }
}
