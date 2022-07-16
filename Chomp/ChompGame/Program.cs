using ChompGame.GameSystem;
using ChompGame.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading.Tasks;

namespace ChompGame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            //var test3D = new Test3DDemo();
            //test3D.Run();
            //return;

            //var gameSystem = RunPong();
            //var gameSystem = RunFullScreenTest();
            // var gameSystem = RunSnake();
            // var gameSystem = RunRando();
            var game = RunPlatformer();

            bool quit = false;
            while (!quit)
            {
                var gs = game.GameSystem;
                if (gs == null)
                    continue;

                Console.Write(">");
                var command = Console.ReadLine();
                var parts = command.Split(' ');
                switch(parts[0])
                {
                    case "q":
                    case "quit":
                        quit = true;
                        break;
                    case "peek":
                        int address = int.Parse(parts[1]);
                        Console.WriteLine(gs.Memory[address]);
                        break;
                    case "poke":
                        address = int.Parse(parts[1]);
                        var newValue = byte.Parse(parts[2]);
                        Console.WriteLine(gs.Memory[address] = newValue);
                        break;
                    case "clear":
                    case "cls":
                        Console.Clear();
                        break; 
                }
            }
        }

        private static Game1 RunTest()
        {
            var specs = new PongSpecs();
            Func<GraphicsDevice,MainSystem> gameSystem = 
                (GraphicsDevice gd) => new MainSystem(specs, gd, s => new CoreGraphicsModule(s),
                   s => new TileModule(s),
                   s => new AudioModule(s),
                   s => new SpritesModule(s),
                   s => new TestModule(s));

            return RunGame(gameSystem);
        }

        private static Game1 RunFullScreenTest()
        {
            var specs = new FullScreenTestSpecs();
            Func<GraphicsDevice, MainSystem> gameSystem =
                 (GraphicsDevice gd) => new MainSystem(specs, gd, s => new CoreGraphicsModule(s),
               s => new TileModule(s),
               s => new SpritesModule(s),
               s => new InputModule(s),
               s => new FullScreenTestModule(s));

            return RunGame(gameSystem);
        }

        private static Game1 RunGame(Func<GraphicsDevice, MainSystem> createSystem)
        {
            var game = new Game1(createSystem);
            var mainSystem = game.GameSystem;

            Task.Run(() =>
            {
                game.Run();
            });

            return game;
        }

        private static Game1 RunPong()
        {
            var specs = new PongSpecs();

            Func<GraphicsDevice, MainSystem> gameSystem =
                (GraphicsDevice gd) => new MainSystem(specs, gd, s => new CoreGraphicsModule(s),
                s => new AudioModule(s),
                s => new TileModule(s),
                s => new SpritesModule(s),
                s => new InputModule(s),
                s => new PongModule(s, s.GetModule<InputModule>(),
                                       s.GetModule<AudioModule>(),
                                       s.GetModule<SpritesModule>(),
                                       s.GetModule<TileModule>()));

            return RunGame(gameSystem); 
        }

        private static Game1 RunSnake()
        {
            var specs = new SnakeSpecs();

            Func<GraphicsDevice, MainSystem> gameSystem =
                (GraphicsDevice gd) => new MainSystem(specs, gd, s => new CoreGraphicsModule(s),
                s => new AudioModule(s),
                s => new SpritesModule(s),
                s => new InputModule(s),
                s => new TileModule(s),
                s => new SnakeModule(s, s.GetModule<InputModule>(),
                                       s.GetModule<AudioModule>(),
                                       s.GetModule<SpritesModule>(),
                                       s.GetModule<TileModule>()));

            return RunGame(gameSystem);
        }

        private static Game1 RunPlatformer()
        {
            var specs = new PlatformerSpecs();

            Func<GraphicsDevice, MainSystem> gameSystem =
                (GraphicsDevice gd) => new MainSystem(specs, gd, s => new CoreGraphicsModule(s),
                s => new AudioModule(s),
                s => new SpritesModule(s),
                s => new InputModule(s),
                s => new TileModule(s),
                s => new PlatformerModule(s, s.GetModule<InputModule>(),
                                       s.GetModule<AudioModule>(),
                                       s.GetModule<SpritesModule>(),
                                       s.GetModule<TileModule>()));

            return RunGame(gameSystem);
        }

        private static Game1 RunRando()
        {
            var specs = new FullScreenTestSpecs();

            Func<GraphicsDevice, MainSystem> gameSystem =
                (GraphicsDevice gd) => new MainSystem(specs, gd, s => new CoreGraphicsModule(s),
                s => new AudioModule(s),
                s => new SpritesModule(s),
                s => new InputModule(s),
                s => new TileModule(s),
                s => new RandoModule(s));

            return RunGame(gameSystem);
        }
    }

}
