﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.GameSystem;
using ChompGame.Helpers;
using ChompGame.MainGame.SceneModels;
using ChompGame.MainGame.SpriteControllers;
using ChompGame.MainGame.SpriteModels;
using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;
using System.Reflection.Emit;

namespace ChompGame.MainGame
{
    enum DynamicBlockType : byte
    {
        Coin,
        DestructibleBlock,
        SwitchBlock
    }

    class DynamicBlock
    {
        private readonly Specs _specs;

        private TwoBitEnum<DynamicBlockType> _type;
        public DynamicBlockState State { get; }
        public DynamicBlockLocation Location { get; }

        private GameByte _destructionBitOffset;

        public byte DestructionBitOffset
        {
            get => _destructionBitOffset.Value;
            set => _destructionBitOffset.Value = value;
        }

        public int ByteLength => 3;

        public DynamicBlockType Type => _type.Value;    

        public DynamicBlock(DynamicBlockType type, SystemMemoryBuilder memoryBuilder, SceneDefinition scene, Specs specs)
        {
            _specs = specs;
            
            //note - 2 bits free
            _type = new TwoBitEnum<DynamicBlockType>(memoryBuilder.Memory, memoryBuilder.CurrentAddress, 0);
            State = new DynamicBlockState(memoryBuilder.Memory, memoryBuilder.CurrentAddress);
            memoryBuilder.AddByte();

            Location = new DynamicBlockLocation(memoryBuilder.Memory, memoryBuilder.CurrentAddress, scene, _specs);
            memoryBuilder.AddByte();

            _type.Value = type;

            _destructionBitOffset = memoryBuilder.AddByte();
        }

        public DynamicBlock(SystemMemory memory, int address, SceneDefinition scene, Specs specs)
        {
            _specs = specs;

            //note - 2 bits free
            _type = new TwoBitEnum<DynamicBlockType>(memory, address, 0);
            State = new DynamicBlockState(memory, address);
           
            Location = new DynamicBlockLocation(memory, address+1, scene, _specs);

            _destructionBitOffset = new GameByte(address + 2, memory);
        }

        public Rectangle TotalBounds => new Rectangle(Location.TileX * _specs.TileWidth, Location.TileY * _specs.TileHeight,
            _specs.TileWidth * 2, _specs.TileHeight * 2);
            
    }

    class DynamicBlockController
    {
        private ChompGameModule _gameModule;
        private GameByte _blockCount;
        private SceneDefinition _scene;
        private SpriteControllerPool<ExplosionController> _explosionControllers;
        private SpriteTileTable _spriteTileTable;
        private RewardsModule _rewardsModule;

        public DynamicBlockController(ChompGameModule gameModule, SpriteTileTable spriteTileTable)
        {
            _gameModule = gameModule;
            _spriteTileTable = spriteTileTable;
            _rewardsModule = gameModule.RewardsModule;
        }

        public void InitializeDynamicBlocks(
            SceneDefinition scene, 
            SystemMemoryBuilder memoryBuilder, 
            NBitPlane levelTileMap,
            NBitPlane levelAttributeTable,
            SpriteControllerPool<ExplosionController> explosionControllers)
        {
            _scene = scene;
            if (_scene.IsAutoScroll)
                return;

            if (scene.Theme == ThemeType.TechBase2)
                CheckTechbase2Blocks();

            _explosionControllers = explosionControllers;

            DynamicScenePartHeader header = _gameModule.CurrentScenePartHeader;

            _blockCount = memoryBuilder.AddByte();

            byte nextDestructionBitOffset = 0;
            byte destructionBitOffset = 0;

            for (int i = 0; i < header.PartsCount; i++)
            {
                var sp = header.GetDynamicScenePart(i, scene, _gameModule.Specs);

                destructionBitOffset = nextDestructionBitOffset;
                nextDestructionBitOffset += sp.DestroyBitsRequired;

                DynamicBlockType type;

                switch (sp.Type)
                {
                    case ScenePartType.Coin:
                        type = DynamicBlockType.Coin;                        
                        break;
                    case ScenePartType.DestructibleBlock:
                        type = DynamicBlockType.DestructibleBlock;
                        break;
                    case ScenePartType.SwitchBlock:
                        type = DynamicBlockType.SwitchBlock;
                        break;
                    default:
                        continue;
                }

                _blockCount.Value++;
                var dynamicBlock = new DynamicBlock(type, memoryBuilder, scene, _gameModule.Specs);
                dynamicBlock.Location.TileX = sp.X;
                dynamicBlock.Location.TileY = sp.Y;
                dynamicBlock.State.TopLeft = sp.DynamicBlockState.TopLeft;
                dynamicBlock.State.TopRight = sp.DynamicBlockState.TopRight;
                dynamicBlock.State.BottomLeft = sp.DynamicBlockState.BottomLeft;
                dynamicBlock.State.BottomRight = sp.DynamicBlockState.BottomRight;

                if (dynamicBlock.Type == DynamicBlockType.SwitchBlock)
                    dynamicBlock.DestructionBitOffset = 0;
                else 
                    dynamicBlock.DestructionBitOffset = destructionBitOffset;

                RecallDestroyed(dynamicBlock);

                SetTiles(dynamicBlock, levelTileMap, levelAttributeTable);
            }
        }

        public void ResetCoinsForLevelBoss()
        {
            int address = _blockCount.Address + 1;
            int coinIndex = 0;

            for (int index = 0; index < _blockCount.Value; index++)
            {
                DynamicBlock block = new DynamicBlock(_gameModule.GameSystem.Memory, address, _scene, _gameModule.Specs);

                if (block.Type != DynamicBlockType.Coin)
                    continue;

                block.State.TopLeft = false;
                block.State.TopRight = false;
                block.State.BottomLeft = false;
                block.State.BottomRight = false;
                _gameModule.WorldScroller.ModifyTiles((t, a) => SetTiles(block, t, a));

                block.Location.TileY = 0;
                block.Location.TileX = (byte)(coinIndex * 2);
                coinIndex++;
               
                address += block.ByteLength;
            }
        }

        public void RestoreCoins()
        {
            int address = _blockCount.Address + 1;

            for (int index = 0; index < _blockCount.Value; index++)
            {
                DynamicBlock block = new DynamicBlock(_gameModule.GameSystem.Memory, address, _scene, _gameModule.Specs);

                if (block.Type != DynamicBlockType.Coin)
                    continue;

                block.State.TopLeft = true;
                block.State.TopRight = true;
                block.State.BottomLeft = true;
                block.State.BottomRight = true;
                _gameModule.WorldScroller.ModifyTiles((t, a) => SetTiles(block, t, a));
      
                address += block.ByteLength;
            }
        }

        public void PositionFreeCoinBlocksNearPlayer(byte tileX, byte tileY)
        {
            int address = _blockCount.Address + 1;
            int freeIndex = 0;

            for (int index = 0; index < _blockCount.Value; index++)
            {
                DynamicBlock block = new DynamicBlock(_gameModule.GameSystem.Memory, address, _scene, _gameModule.Specs);

                if (block.Type != DynamicBlockType.Coin)
                    continue;

                if (block.State.AnyOn)
                    continue;

                if((freeIndex % 2)==0)
                {
                    block.Location.TileY = tileY;
                    block.Location.TileX = (byte)(tileX + index);
                }
                else
                {
                    block.Location.TileY = tileY;
                    block.Location.TileX = (byte)(tileX - (index + 1));
                }

                freeIndex++;

                address += block.ByteLength;
            }
        }

        public void SpawnCoins(Rectangle region)
        {
            if (_scene.IsAutoScroll)
                return;

            int address = _blockCount.Address + 1;

            for (int index = 0; index < _blockCount.Value; index++)
            {
                DynamicBlock block = new DynamicBlock(_gameModule.GameSystem.Memory, address, _scene, _gameModule.Specs);

                if (block.Type != DynamicBlockType.Coin)
                    continue;

                bool anyChanged = false;

                if(region.Intersects(block.Location.TopLeftRegion))
                {
                    block.State.TopLeft = true;
                    anyChanged = true;
                }
                if (region.Intersects(block.Location.TopRightRegion))
                {
                    block.State.TopRight = true;
                    anyChanged = true;
                }
                if (region.Intersects(block.Location.BottomLeftRegion))
                {
                    block.State.BottomLeft = true;
                    anyChanged = true;
                }
                if (region.Intersects(block.Location.BottomRightRegion))
                {
                    block.State.BottomRight = true;
                    anyChanged = true;
                }

                if(anyChanged)
                {
                    _gameModule.WorldScroller.ModifyTiles((t, a) => SetTiles(block, t, a));
                }

                address += block.ByteLength;
            }

        }

        private void CheckTechbase2Blocks()
        {
            int groupOffset = 1;

            for(Level level = Level.Level6_18_Techbase11; level <= Level.Level6_35_TechbaseCubeDD; level++)
            {
                GameDebug.DebugLog($"TB2 - Checking {level}", DebugLogFlags.Misc);
                ScenePartsHeader header = new ScenePartsHeader(level, _gameModule.GameSystem.Memory);
                int nextOffset = CheckTechbase2Blocks(header, groupOffset);
                if (nextOffset == -1)
                {
                    GameDebug.DebugLog($"TB2 - End at {level}", DebugLogFlags.Misc);
                    return;
                }
                else
                {
                    GameDebug.DebugLog($"Offset bits = {nextOffset}", DebugLogFlags.Misc);
                }

                groupOffset += nextOffset;
                GameDebug.DebugLog($"Total offset = {groupOffset}", DebugLogFlags.Misc);
            }

            _gameModule.ScenePartsDestroyed.SwitchBlocksOff = true;
        }
        
        private int CheckTechbase2Blocks(ScenePartsHeader header, int initialOffset)
        {
            byte nextDestructionBitOffset = 0;
            byte destructionBitOffset = 0;

            for (int i = 0; i < header.PartsCount; i++)
            {
                var sp = header.GetDynamicScenePart(i, _scene, _gameModule.Specs);

                destructionBitOffset = nextDestructionBitOffset;

                int bitsToCheck = sp.Type switch {
                    ScenePartType.Coin => 4,
                    ScenePartType.EnemyType1 => 1,
                    _ => 0
                };

                GameDebug.DebugLog($"Checking part {i + 1} of {header.PartsCount} ({sp.Type})", DebugLogFlags.Misc);

                for(int di = 0; di < bitsToCheck; di++)
                {
                    if (!_gameModule.ScenePartsDestroyed.IsDestroyed2(initialOffset + destructionBitOffset + di))
                        return -1;
                }

                nextDestructionBitOffset += sp.DestroyBitsRequired;
            }

            return nextDestructionBitOffset;
        }

        private void RecallDestroyed(DynamicBlock block)
        {
            if(block.Type == DynamicBlockType.Coin)
            {
                block.State.TopLeft = block.State.TopLeft && !_gameModule.ScenePartsDestroyed.IsDestroyed(block.DestructionBitOffset);
                block.State.TopRight = block.State.TopRight && !_gameModule.ScenePartsDestroyed.IsDestroyed(block.DestructionBitOffset + 1); 
                block.State.BottomLeft = block.State.BottomLeft && !_gameModule.ScenePartsDestroyed.IsDestroyed(block.DestructionBitOffset + 2);
                block.State.BottomRight = block.State.BottomRight && !_gameModule.ScenePartsDestroyed.IsDestroyed(block.DestructionBitOffset + 3); ;
            }
            else if(block.Type == DynamicBlockType.DestructibleBlock)
            {
                if(_gameModule.ScenePartsDestroyed.IsDestroyed(block.DestructionBitOffset))
                {
                    block.State.TopLeft = false;
                    block.State.TopRight = false;
                    block.State.BottomLeft = false;
                    block.State.BottomRight = false;
                }
            }
            else if(block.Type == DynamicBlockType.SwitchBlock)
            {
                if (_gameModule.ScenePartsDestroyed.SwitchBlocksOff)
                {
                    block.State.TopLeft = false;
                    block.State.TopRight = false;
                    block.State.BottomLeft = false;
                    block.State.BottomRight = false;
                }
            }
        }

        private void SetTiles(DynamicBlock block, NBitPlane tileMap, NBitPlane attributeTable)
        {
            var attrX = block.Location.TileX / _gameModule.Specs.AttributeTableBlockSize;
            var attrY = block.Location.TileY / _gameModule.Specs.AttributeTableBlockSize;

            attributeTable[attrX, attrY] = block.Type switch {
                DynamicBlockType.Coin => 2,
                DynamicBlockType.SwitchBlock => 1,
                _ => 3
            };

            byte tile = block.Type switch {
                DynamicBlockType.DestructibleBlock => _spriteTileTable.DestructibleBlockTile,
                DynamicBlockType.SwitchBlock => _spriteTileTable.DestructibleBlockTile,
                DynamicBlockType.Coin => _spriteTileTable.CoinTile,
                _ => 0
            };

            if (block.State.TopLeft)
                tileMap[block.Location.TileX, block.Location.TileY] = tile;
            else
                tileMap[block.Location.TileX, block.Location.TileY] = 0;

            if (block.State.TopRight)
                tileMap[block.Location.TileX + 1, block.Location.TileY] = tile;
            else
                tileMap[block.Location.TileX + 1, block.Location.TileY] = 0;

            if (block.State.BottomLeft)
                tileMap[block.Location.TileX, block.Location.TileY + 1] = tile;
            else
                tileMap[block.Location.TileX, block.Location.TileY + 1] = 0;

            if (block.State.BottomRight)
                tileMap[block.Location.TileX + 1, block.Location.TileY + 1] = tile;
            else
                tileMap[block.Location.TileX + 1, block.Location.TileY + 1] = 0;
        }

        public int HandleCoinCollision(CollisionInfo collisionInfo, WorldSprite player)
        {
            if (!collisionInfo.DynamicBlockCollision)
                return 0;

            int address = _blockCount.Address + 1;

            for (int index = 0; index < _blockCount.Value; index++)
            {
                DynamicBlock block = new DynamicBlock(_gameModule.GameSystem.Memory, address, _scene, _gameModule.Specs);

                if (block.Type == DynamicBlockType.Coin
                    && block.State.AnyOn)
                {
                    int count = 0;

                    if(block.State.TopLeft 
                        && collisionInfo.CoinTileX == block.Location.TileX 
                        && collisionInfo.CoinTileY == block.Location.TileY)
                    {
                        _gameModule.ScenePartsDestroyed.SetDestroyed(block.DestructionBitOffset);
                        block.State.TopLeft = false;
                        count++;
                    }

                    if (block.State.TopRight
                       && collisionInfo.CoinTileX == block.Location.TileX + 1
                       && collisionInfo.CoinTileY == block.Location.TileY)
                    {
                        _gameModule.ScenePartsDestroyed.SetDestroyed(block.DestructionBitOffset+1);
                        block.State.TopRight = false;
                        count++;
                    }

                    if (block.State.BottomLeft
                       && collisionInfo.CoinTileX == block.Location.TileX
                       && collisionInfo.CoinTileY == block.Location.TileY + 1)
                    {
                        _gameModule.ScenePartsDestroyed.SetDestroyed(block.DestructionBitOffset+2);
                        block.State.BottomLeft = false;
                        count++;
                    }

                    if (block.State.BottomRight
                       && collisionInfo.CoinTileX == block.Location.TileX + 1
                       && collisionInfo.CoinTileY == block.Location.TileY + 1)
                    {
                        _gameModule.ScenePartsDestroyed.SetDestroyed(block.DestructionBitOffset+3);
                        block.State.BottomRight = false;
                        count++;
                    }

                    if (count > 0)
                    {
                        _gameModule.AudioService.PlaySound(ChompAudioService.Sound.CollectCoin);
                        _gameModule.WorldScroller.ModifyTiles((t,a) => SetTiles(block, t, a));
                        _rewardsModule.CheckRewards(count);
                        return count;
                    }
                }

                address += block.ByteLength;
            }

            return 0;
        }

        public bool HandleBombCollision(CollisionInfo collisionInfo)
        {
            if (!collisionInfo.DynamicBlockCollision)
                return false;

            int address = _blockCount.Address + 1;

            for (int index = 0; index < _blockCount.Value; index++)
            {
                DynamicBlock block = new DynamicBlock(_gameModule.GameSystem.Memory, address, _scene, _gameModule.Specs);
                    
                if(block.Type == DynamicBlockType.DestructibleBlock 
                    && block.State.AnyOn
                    && (block.Location.TileX == collisionInfo.BreakableTileX || block.Location.TileX + 1 == collisionInfo.BreakableTileX)
                    && (block.Location.TileY == collisionInfo.BreakableTileY || block.Location.TileY + 1 == collisionInfo.BreakableTileY))
                {

                    _gameModule.AudioService.PlaySound(ChompAudioService.Sound.Break);

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

                    _gameModule.ScenePartsDestroyed.SetDestroyed(block.DestructionBitOffset);

                    _gameModule.WorldScroller.ModifyTiles((t,a) => SetTiles(block, t, a));
                    return true;
                }
              
                address += block.ByteLength;
            }

            return false;
        }
      
        private void SpawnExplosion(DynamicBlock block, int xMod, int yMod)
        {
            var explosionController = _explosionControllers.TryAddNew();
            if (explosionController != null)
            {
                explosionController.WorldSprite.X = (block.Location.TileX + xMod) * _gameModule.Specs.TileWidth;
                explosionController.WorldSprite.Y = (block.Location.TileY + yMod + Constants.StatusBarTiles) * _gameModule.Specs.TileHeight;
                explosionController.SetMotion(xMod, yMod);
            }
        }

        public void ToggleSwitchBlocks()
        {
            _gameModule.ScenePartsDestroyed.SwitchBlocksOff = !_gameModule.ScenePartsDestroyed.SwitchBlocksOff;

            if (_gameModule.ScenePartsDestroyed.SwitchBlocksOff)
                SwitchBlocksOff();
            else
                SwitchBlocksOn();  
        }
    
        private void SwitchBlocksOff()
        {
            int address = _blockCount.Address + 1;
            for (int index = 0; index < _blockCount.Value; index++)
            {
                DynamicBlock block = new DynamicBlock(_gameModule.GameSystem.Memory, address, _scene, _gameModule.Specs);

                if (block.Type == DynamicBlockType.SwitchBlock)
                {
                    block.State.TopLeft = false;
                    block.State.TopRight = false;
                    block.State.BottomLeft = false;
                    block.State.BottomRight = false;                    
                    _gameModule.WorldScroller.ModifyTiles((t, a) => SetTiles(block, t, a));
                }

                address += block.ByteLength;
            }
        }

        private void SwitchBlocksOn()
        {
            var header = _gameModule.CurrentScenePartHeader;
            int address = _blockCount.Address + 1;

            for (int i = 0; i < header.PartsCount; i++)
            {
                var sp = header.GetDynamicScenePart(i, _scene, _gameModule.Specs);

                switch (sp.Type)
                {
                    case ScenePartType.Coin:
                    case ScenePartType.DestructibleBlock:
                    case ScenePartType.SwitchBlock:
                        break;
                    default:
                        continue;
                }

                DynamicBlock block = new DynamicBlock(_gameModule.GameSystem.Memory, address, _scene, _gameModule.Specs);
                if(block.Type == DynamicBlockType.SwitchBlock)
                {
                    block.State.TopLeft = sp.DynamicBlockState.TopLeft;
                    block.State.TopRight = sp.DynamicBlockState.TopRight;
                    block.State.BottomLeft = sp.DynamicBlockState.BottomLeft;
                    block.State.BottomRight = sp.DynamicBlockState.BottomRight;                    
                    _gameModule.WorldScroller.ModifyTiles((t, a) => SetTiles(block, t, a));
                }
                address += block.ByteLength;
            }
        }
    }
}
