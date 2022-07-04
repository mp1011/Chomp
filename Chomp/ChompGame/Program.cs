using ChompGame.GameSystem;
using ChompGame.Graphics;
using System;
using System.Threading.Tasks;

namespace ChompGame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            //var gameSystem = RunPong();
            //var gameSystem = RunFullScreenTest();
            var gameSystem = RunSnake();
           // var gameSystem = RunRando();

            bool quit = false;
            while (!quit)
            {
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
                        Console.WriteLine(gameSystem.Memory[address]);
                        break;
                    case "poke":
                        address = int.Parse(parts[1]);
                        var newValue = byte.Parse(parts[2]);
                        Console.WriteLine(gameSystem.Memory[address] = newValue);
                        break;
                    case "clear":
                    case "cls":
                        Console.Clear();
                        break; 
                }
            }
        }

        private static MainSystem RunTest()
        {
            var specs = new PongSpecs();
            var gameSystem = new MainSystem(specs, s => new CoreGraphicsModule(s),
               s => new TileModule(s),
               s => new AudioModule(s),
               s => new SpritesModule(s),
               s => new TestModule(s));

            return RunGame(gameSystem);
        }

        private static MainSystem RunFullScreenTest()
        {
            var specs = new FullScreenTestSpecs();
            var gameSystem = new MainSystem(specs, s => new CoreGraphicsModule(s),
               s => new TileModule(s),
               s => new SpritesModule(s),
               s => new InputModule(s),
               s => new FullScreenTestModule(s));

            return RunGame(gameSystem);
        }

        private static MainSystem RunGame(MainSystem mainSystem)
        {
            Task.Run(() =>
            {
                using (var game = new Game1(mainSystem))
                    game.Run();
            });

            return mainSystem;
        }

        private static MainSystem RunPong()
        {
            var specs = new PongSpecs();

            var gameSystem = new MainSystem(specs, s => new CoreGraphicsModule(s),
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

        private static MainSystem RunSnake()
        {
            var specs = new SnakeSpecs();

            var gameSystem = new MainSystem(specs, s => new CoreGraphicsModule(s),
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

        private static MainSystem RunRando()
        {
            var specs = new FullScreenTestSpecs();

            var gameSystem = new MainSystem(specs, s => new CoreGraphicsModule(s),
                s => new AudioModule(s),
                s => new SpritesModule(s),
                s => new InputModule(s),
                s => new TileModule(s),
                s => new RandoModule(s));

            return RunGame(gameSystem);
        }
    }

}
