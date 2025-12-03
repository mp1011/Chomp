
using ChompGame;
using ChompGame.MainGame;

var init = ChompGame.Program.InitSystem(RomLoad.Code);
var game = new GameDev(init);
game.Run();
       