using ChompGame.Data;
using ChompGame.Extensions;
using System.Diagnostics;

namespace ChompGame.MainGame.SceneModels
{
    class MistAutoscrollBgModule
    {
        private GameByte _longTimer;
        private GameByte _timer;
        private ChompGameModule _gameModule;

        public MistAutoscrollBgModule(ChompGameModule gameModule)
        {
            _longTimer = gameModule.LevelTimerLong;
            _timer = gameModule.LevelTimer;
            _gameModule = gameModule;
        }

        public void Update()
        {
            if (_gameModule.CurrentLevel == Level.Level5_25_Plane2)
            {
                if (_longTimer.Value > 32)
                {
                    if (_timer.IsMod(16) && _gameModule.TileModule.Scroll.Y > 196)
                    {
                        _gameModule.TileModule.Scroll.Y--;
                    }
                    else if (_gameModule.TileModule.Scroll.Y == 0)
                    {
                        _gameModule.WorldScroller.ModifyTiles(AddStars);
                        _gameModule.TileModule.Scroll.Y--;
                    }
                }
            }
            else if (_gameModule.CurrentLevel == Level.Level5_26_Plane3)
            {                    
                if (_gameModule.TileModule.Scroll.Y == 0)
                {
                _gameModule.TileModule.Scroll.Y = 196;
                _gameModule.WorldScroller.ModifyTiles(AddStars);
                    _gameModule.TileModule.Scroll.Y--;
                }
            }
        }

        private void AddStars(NBitPlane tileMap, NBitPlane attributeTable)
        {
            attributeTable.ForEach((x, y, b) =>
            {
                if(y > 8)
                    attributeTable[x, y] = 1;
            });

            tileMap.ForEach((x, y, b) =>
            {
               if (y > 16)
                    tileMap[x, y] = (byte)((_gameModule.RandomModule.Generate(2) == 0) ? 6:0);
            });
        }
    }
}
