﻿using ChompGame.Data;
using ChompGame.Data.Memory;
using ChompGame.Extensions;
using ChompGame.GameSystem;
using ChompGame.MainGame.SceneModels;
using Microsoft.Xna.Framework;
using System;

namespace ChompGame.MainGame
{

    abstract class WorldScroller
    {
        protected Specs _specs;
        protected readonly byte _halfWindowSize;
        protected readonly byte _forwardSeamOffset;
        protected readonly byte _backwardSeamOffset;
        protected readonly byte _tilesPerScreen;

        protected GameByte _scrollWindowBegin, _seamTile, _worldScrollX, _worldScrollY;

        protected TileModule _tileModule;
        protected SpritesModule _spritesModule;

        protected WorldSprite _focusSprite;
        protected NBitPlane _levelNameTable;
        protected NBitPlane _levelAttributeTable;

        protected SceneDefinition _sceneDefinition;

        public NBitPlane LevelNameTable => _levelNameTable;
        public NBitPlane LevelAttributeTable => _levelAttributeTable;
         
        public WorldScroller(
            SystemMemoryBuilder memoryBuilder,
            Specs specs, 
            TileModule tileModule, 
            SpritesModule spritesModule)
        {
            _specs = specs;
            _tileModule = tileModule;
            _spritesModule = spritesModule;
            _scrollWindowBegin = memoryBuilder.AddByte();
            _seamTile = memoryBuilder.AddByte();
            _worldScrollX = memoryBuilder.AddByte();
            _worldScrollY = memoryBuilder.AddByte();
            _halfWindowSize = (byte)(_specs.ScreenWidth / 2);
            _tilesPerScreen = (byte)(_specs.ScreenHeight / _specs.TileHeight);
            _forwardSeamOffset = (byte)((_specs.ScreenWidth + _halfWindowSize) / _specs.TileWidth);
            _backwardSeamOffset = (byte)(_specs.NameTableWidth - _forwardSeamOffset - 1);
        }

        public byte ScrollWindowBegin
        {
            get => _scrollWindowBegin.Value;
            private set => _scrollWindowBegin.Value = value;
        }

        private byte ScrollWindowEnd => (byte)((ScrollWindowBegin + _specs.ScreenWidth) % _specs.NameTablePixelWidth);

        private byte ScrollYMax => (byte)((_levelNameTable.Height * _specs.TileHeight) - _specs.ScreenHeight);
        public byte WorldScrollPixelX => (byte)(_worldScrollX.Value * _specs.TileWidth);
        public byte WorldScrollPixelY => (byte)(_worldScrollY.Value * _specs.TileHeight);

        public abstract Rectangle ViewPane { get; }

        protected virtual byte ScrollableSectionBegin => 0;

        public void Initialize(SceneDefinition scene, WorldSprite focusSprite, NBitPlane levelNameTable, NBitPlane levelAttributeTable)
        {
            _sceneDefinition = scene;
            _focusSprite = focusSprite;
            _levelNameTable = levelNameTable;
            _levelAttributeTable = levelAttributeTable;        
            _tileModule.Scroll.X = 0;
            _tileModule.Scroll.Y = 0;
            _spritesModule.Scroll.X = 0;
            _spritesModule.Scroll.Y = 0;
        }

        public abstract bool Update();

        public abstract void OffsetCamera(int x, int y);

        public int DistanceFromViewpane(Rectangle r)
        {
            if (r.Intersects(ViewPane))
                return 0;

            var rightDistance = ViewPane.X - r.Right;
            var leftDistance = r.Left - ViewPane.Right;
            var upDistance = ViewPane.Y - r.Bottom;
            var downDistance = r.Top - ViewPane.Bottom;

            int hDistance = rightDistance > leftDistance ? rightDistance : leftDistance;
            int vDistance = upDistance > downDistance ? upDistance : downDistance;

            return hDistance > vDistance ? hDistance : vDistance;
        }
           
        public void ModifyTiles(Action<NBitPlane, NBitPlane> modify)
        {
            modify(_levelNameTable, _levelAttributeTable);
            RefreshNametable();
        }

        public abstract void RefreshNametable();

        protected void CopyTileColumn(byte worldColumn, byte ntColumn)
        {
            int bottomSectionBegin = ScrollableSectionBegin;

            for (int row = bottomSectionBegin; row < _levelNameTable.Height; row++)
            {
                _tileModule.NameTable[ntColumn, row + Constants.StatusBarTiles] = _levelNameTable[worldColumn, row];
                _tileModule.AttributeTable[ntColumn / _specs.AttributeTableBlockSize,
                    (row + Constants.StatusBarTiles) / _specs.AttributeTableBlockSize] = _levelAttributeTable[worldColumn / _specs.AttributeTableBlockSize, row / _specs.AttributeTableBlockSize];
            }

            GameDebug.DebugLog($"Seam Update: W{worldColumn}->N{ntColumn}", DebugLogFlags.WorldScroller);
        }
    }
}
