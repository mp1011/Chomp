﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers;
using Microsoft.Xna.Framework;

namespace ChompGame.MainGame
{
    enum DynamicBlockType : byte
    {
        Coin,
        DestructibleBlock,
        LockedDoor,
        Button
    }

    class DynamicBlock
    {
        private readonly Specs _specs;

        private TwoBitEnum<DynamicBlockType> _type;
        public DynamicBlockState State { get; }
        public DynamicBlockLocation Location { get; }

        public int ByteLength => 2;

        public DynamicBlockType Type => _type.Value;    

        public DynamicBlock(DynamicBlockType type, SystemMemoryBuilder memoryBuilder, SceneDefinition scene, Specs specs)
        {
            _specs = specs;
            
            //note - 2 bits free
            _type = new TwoBitEnum<DynamicBlockType>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0);
            State = new DynamicBlockState(memoryBuilder.Memory, memoryBuilder.CurrentAddress);
            memoryBuilder.AddByte();

            Location = new DynamicBlockLocation(memoryBuilder.Memory, memoryBuilder.CurrentAddress, scene);
            memoryBuilder.AddByte();

            _type.Value = type;
        }

        public DynamicBlock(SystemMemory memory, int address, SceneDefinition scene, Specs specs)
        {
            _specs = specs;

            //note - 2 bits free
            _type = new TwoBitEnum<DynamicBlockType>(memory, address, 0);
            State = new DynamicBlockState(memory, address);
           
            Location = new DynamicBlockLocation(memory, address+1, scene);
        }

        public Rectangle TotalBounds => new Rectangle(Location.TileX * _specs.TileWidth, Location.TileY * _specs.TileHeight,
            _specs.TileWidth * 2, _specs.TileHeight * 2);
            
    }

    class DynamicBlockController
    {
        private ChompGameModule _gameModule;
        private GameByte _partCount;
        private SceneDefinition _scene;
        private SpriteControllerPool<ExplosionController> _explosionControllers;

        public DynamicBlockController(ChompGameModule gameModule)
        {
            _gameModule = gameModule;
        }

        public void InitializeDynamicBlocks(
            SceneDefinition scene, 
            SystemMemoryBuilder memoryBuilder, 
            NBitPlane levelTileMap,
            SpriteControllerPool<ExplosionController> explosionControllers)
        {
            _scene = scene;
            _explosionControllers = explosionControllers;

            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            _partCount = memoryBuilder.AddByte();

            for (int i = 0; i < header.PartsCount; i++)
            {
                ScenePart sp = header.GetScenePart(i, scene);

                DynamicBlockType type;

                switch (sp.Type)
                {
                    case ScenePartType.Coin:
                        type = DynamicBlockType.Coin;
                        break;
                    case ScenePartType.DestructibleBlock:
                        type = DynamicBlockType.DestructibleBlock;
                        break;
                    case ScenePartType.LockedDoor:
                        type = DynamicBlockType.LockedDoor;
                        break;
                    case ScenePartType.Button:
                        type = DynamicBlockType.Button;
                        break;
                    default:
                        continue;
                }

                _partCount.Value++;
                var dynamicBlock = new DynamicBlock(type, memoryBuilder, scene, _gameModule.Specs);
                dynamicBlock.Location.TileX = sp.X;
                dynamicBlock.Location.TileY = sp.Y;
                dynamicBlock.State.TopLeft = sp.DynamicBlockState.TopLeft;
                dynamicBlock.State.TopRight = sp.DynamicBlockState.TopRight;
                dynamicBlock.State.BottomLeft = sp.DynamicBlockState.BottomLeft;
                dynamicBlock.State.BottomRight = sp.DynamicBlockState.BottomRight;

                SetTiles(dynamicBlock, levelTileMap);
            }
        }

        private void SetTiles(DynamicBlock block, NBitPlane tileMap)
        {
            if (block.Type == DynamicBlockType.DestructibleBlock)
            {
                if (block.State.TopLeft)
                    tileMap[block.Location.TileX, block.Location.TileY] = Constants.DestructibleBlockTile;
                else
                    tileMap[block.Location.TileX, block.Location.TileY] = 0;

                if (block.State.TopRight)
                    tileMap[block.Location.TileX + 1, block.Location.TileY] = Constants.DestructibleBlockTile;
                else
                    tileMap[block.Location.TileX + 1, block.Location.TileY] = 0;

                if (block.State.BottomLeft)
                    tileMap[block.Location.TileX, block.Location.TileY + 1] = Constants.DestructibleBlockTile;
                else
                    tileMap[block.Location.TileX, block.Location.TileY + 1] = 0;

                if (block.State.BottomRight)
                    tileMap[block.Location.TileX + 1, block.Location.TileY + 1] = Constants.DestructibleBlockTile;
                else
                    tileMap[block.Location.TileX + 1, block.Location.TileY + 1] = 0;
            }
            else
                throw new System.NotImplementedException();
        }

        public void HandleBombCollision(CollisionInfo collisionInfo)
        {
            if (!collisionInfo.DynamicBlockCollision)
                return;

            int address = _partCount.Address + 1;

            for (int index = 0; index < _partCount.Value; index++)
            {
                DynamicBlock block = new DynamicBlock(_gameModule.GameSystem.Memory, address, _scene, _gameModule.Specs);
                     
                if(block.State.AnyOn
                    && (block.Location.TileX == collisionInfo.TileX || block.Location.TileX + 1 == collisionInfo.TileX)
                    && (block.Location.TileY == collisionInfo.TileY || block.Location.TileY + 1 == collisionInfo.TileY))
                {

                    _gameModule.AudioService.PlaySound(ChompAudioService.Sound.Noise);

                    if (block.State.TopLeft)
                        SpawnExplosion(block, 0, 0);
                    if (block.State.TopRight)
                        SpawnExplosion(block, 1, 0);
                    if (block.State.BottomLeft)
                        SpawnExplosion(block, 0, 1);
                    if (block.State.BottomRight)
                        SpawnExplosion(block, 1, 1);

                    block.State.TopLeft = false;
                    block.State.TopRight = false;
                    block.State.BottomLeft = false;
                    block.State.BottomRight = false;

                    _gameModule.WorldScroller.ModifyTiles(t => SetTiles(block, t));
                }
              
                address += block.ByteLength;
            }
        }
      
        private void SpawnExplosion(DynamicBlock block, int xMod, int yMod)
        {
            var explosionController = _explosionControllers.TryAddNew(3);
            if (explosionController != null)
            {
                explosionController.WorldSprite.X = (block.Location.TileX + xMod) * _gameModule.Specs.TileWidth;
                explosionController.WorldSprite.Y = (block.Location.TileY + yMod + Constants.StatusBarTiles) * _gameModule.Specs.TileHeight;
                explosionController.SetMotion(xMod, yMod);
            }
        }
    }
}
