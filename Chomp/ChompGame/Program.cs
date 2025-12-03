using ChompGame.GameSystem;
using ChompGame.MainGame;
using ChompGame.MainGame.SceneModels;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace ChompGame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            var init = InitSystem(RomLoad.Disk);
            var game = new Game1(init);
            game.Run();
        }
        
        public static Func<GraphicsDevice, ContentManager, MainSystem> InitSystem(RomLoad romLoad)
        {
            var specs = new ChompGameSpecs();

            Func<GraphicsDevice, ContentManager, MainSystem> gameSystem =
                (GraphicsDevice gd, ContentManager cm) => new MainSystem(specs, gd, s => new CoreGraphicsModule(s),
                s => new InputModule(s),
                s => new SpritesModule(s),
                s => new TileModule(s),
                s => new RandomModule(s),
                s => new BankAudioModule(s),
                s => new ChompAudioService(s.GetModule<BankAudioModule>()),              
                s => new RewardsModule(s),
                s => new MusicModule(s, cm),
                s => new PaletteModule(s, s.CoreGraphicsModule, s.GetModule<TileModule>(), s.GameRAM),
                s => new ChompGameModule(romLoad, s, s.GetModule<InputModule>(),
                                       s.GetModule<BankAudioModule>(),
                                       s.GetModule<SpritesModule>(),
                                       s.GetModule<TileModule>(),
                                       s.GetModule<MusicModule>(),
                                       s.GetModule<RewardsModule>(),
                                       s.GetModule<PaletteModule>()));

            return gameSystem;
        }
    }

}
