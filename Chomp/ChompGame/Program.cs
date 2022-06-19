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
            var specs = new Specs();

            var gameSystem = new MainSystem(specs, s => new CoreGraphicsModule(s),
                s => new TileModule(s),
                s => new SpritesModule(s),
                s => new InputModule(s),
                s => new PongModule(s, s.GetModule<InputModule>(), 
                                       s.GetModule<SpritesModule>(),
                                       s.GetModule<TileModule>()));

            //var gameSystem = new MainSystem(specs, s => new CoreGraphicsModule(s),
            //   s => new TileModule(s),
            //   s => new SpritesModule(s),
            //   s => new TestModule(s));

            Task.Run(() =>
            {
                using (var game = new Game1(specs, gameSystem))
                    game.Run();
            });

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

    }
}
