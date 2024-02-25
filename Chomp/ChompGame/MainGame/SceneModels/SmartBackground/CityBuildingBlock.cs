using ChompGame.Data;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ChompGame.MainGame.SceneModels.SmartBackground
{
    class CityBuildingBlock : SmartBackgroundBlock
    {
        private const int MinWidth = 5;
        private const int Height = 4;


        public CityBuildingBlock(SceneDefinition sceneDefinition) : base(sceneDefinition)
        {
        }

        private const int RoofLeft = 40;
        private const int RoofTop = 41;
        private const int Brick = 42;
        private const int RoofRight = 43;
        private const int WindowTopLeft = 44;
        private const int WindowTopRight = 45;
        private const int WindowBottomLeft = 46;
        private const int WindowBottomRight = 47;
        private const int DoorTop = 48;
        private const int Door = 49;
        private const int Building = 50;

        protected override IEnumerable<Rectangle> DetermineRegions(NBitPlane nameTable)
        {
            var lastGroundY = 0;
            Point lastGroundStart = Point.Zero;

            for(int leftEdge = 0; leftEdge < nameTable.Width - MinWidth; leftEdge += 2)
            {
                var groundY = nameTable
                    .Find(new Point(leftEdge, nameTable.Height - 1), 0, -1, p => !CollisionDetector.IsTileSolid(p)).Y + 1;
           
                if(leftEdge == 0 || groundY != lastGroundY)
                {
                    var groundStart = new Point(leftEdge, groundY); 

                    if(leftEdge > 0)
                    {
                        int availableWidth = groundStart.X - lastGroundStart.X;
                        int x = lastGroundStart.X;
                        int width = (availableWidth / 2) * 2;
                       
                        if(RandomModule.FixedRandom((byte)(leftEdge * groundY)) > 128)
                        {
                            x += 2;
                            width -= 2;
                        }

                        if (RandomModule.FixedRandom((byte)(leftEdge + groundY)) > 128)
                        {
                            width -= 2;
                        }

                        if (availableWidth >= MinWidth)
                        {
                            yield return new Rectangle(x, lastGroundStart.Y - Height,
                                (width / 2) * 2, Height);
                        }
                    }

                    lastGroundStart = groundStart;
                }

                lastGroundY = groundY;
            }            
        }

        protected override void AddBlock(Rectangle region, NBitPlane nameTable)
        {
            nameTable.ForEach(new Point(region.X, region.Y), new Point(region.Right, region.Bottom),
                (x, y, b) =>
                {
                    int blockX = x - region.X;
                    int blockY = y - region.Y;

                    if (blockX == 0 && blockY == 0)
                        nameTable[x, y] = RoofLeft;
                    else if (blockX == region.Width-1 && blockY == 0)
                        nameTable[x, y] = RoofRight;
                    else if (blockY == 0)
                        nameTable[x, y] = RoofTop;
                    else if(blockX == 0 || blockX == region.Width-1)
                        nameTable[x, y] = 0;
                    else if (blockX == 1 || blockX == region.Width - 2)
                        nameTable[x, y] = Brick;
                    else
                        nameTable[x, y] = Building;
                });

            int windows = (region.Width - 3) / 2;

            for(int i = 0; i < windows; i++)
            {
                if (i == windows / 2)
                    AddDoor(2 + (i * 2), region, nameTable);
                else if (i < windows / 2)
                    AddWindow(2 + (i*2), region, nameTable);
                else
                    AddWindow(1 + (i * 2), region, nameTable);
            }

            //  AddWindow(2, region, nameTable);
            //AddDoor(4, region, nameTable);
            // AddWindow(5, region, nameTable);
        }

        private void AddWindow(int position, Rectangle region, NBitPlane nameTable)
        {
            nameTable[region.X + position, region.Bottom - 3] = WindowTopLeft;
            nameTable[region.X + position + 1, region.Bottom - 3] = WindowTopRight;
            nameTable[region.X + position, region.Bottom - 2] = WindowBottomLeft;
            nameTable[region.X + position + 1, region.Bottom - 2] = WindowBottomRight;
        }

        private void AddDoor(int position, Rectangle region, NBitPlane nameTable)
        {

            nameTable[region.X + position, region.Bottom - 3] = DoorTop;
            nameTable[region.X + position, region.Bottom - 2] = Door;
            nameTable[region.X + position, region.Bottom - 1] = Door;
        }
    }
}
