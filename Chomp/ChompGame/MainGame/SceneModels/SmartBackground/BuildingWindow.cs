using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.Helpers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace ChompGame.MainGame.SceneModels.SmartBackground
{
    class BuildingWindow : SmartBackgroundBlock
    {
        private ChompGameModule _gameModule;
        private const int WindowSize = 4;

        public BuildingWindow(SceneDefinition sceneDefinition, ChompGameModule gameModule) : base(sceneDefinition)
        {
            _gameModule = gameModule;
        }

        protected override void AddBlock(Rectangle region, NBitPlane nameTable)
        {
            nameTable.ForEach(new Point(region.Left, region.Top),
                new Point(region.Right, region.Bottom),
                (x, y, b) =>
                {
                    nameTable[x, y] = 7;
                    if(y == region.Bottom-2)
                    {
                        nameTable[x, y] = (byte)(24 + (x-region.Left));
                    }
                    else if(y == region.Bottom-1)
                    {
                        nameTable[x, y] = 29;
                    }
                });
        }

        private bool IsBlockFree(NBitPlane nameTable, List<Rectangle> windowRegions, int x, int y)
        {
            if(CollisionDetector.IsTileSolid(nameTable[x, y]))
                return false;

            foreach(var r in windowRegions)
            {
                if(r.Contains(x, y))
                    return false;
            }

            return true;
        }

        protected override IEnumerable<Rectangle> DetermineRegions(NBitPlane nameTable)
        {
            List<Rectangle> regions = new List<Rectangle>();

            if (_sceneDefinition.ScrollStyle == ScrollStyle.None)
            {
                regions.Add(new Rectangle(6, 4, WindowSize, WindowSize));
            }
            else if(_gameModule.CurrentLevel == Level.Level3_4_Building1_Room2)
            {
                regions.Add(new Rectangle(6, 6, WindowSize, WindowSize));
                regions.Add(new Rectangle(12, 6, WindowSize, WindowSize));
                regions.Add(new Rectangle(18, 6, WindowSize, WindowSize));

            }
            else if (_gameModule.CurrentLevel == Level.Level3_11_Building3_Room1)
            {
                regions.Add(new Rectangle(6, 6, WindowSize, WindowSize));
                regions.Add(new Rectangle(18, 6, WindowSize, WindowSize));

                regions.Add(new Rectangle(6, 20, WindowSize, WindowSize));
                regions.Add(new Rectangle(18, 20, WindowSize, WindowSize));

            }

            return regions;


        }
    }
}
