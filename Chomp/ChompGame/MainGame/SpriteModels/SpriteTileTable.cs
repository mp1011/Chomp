using ChompGame.Data;
using ChompGame.Data.Memory;
using System;

namespace ChompGame.MainGame.SpriteModels
{
    //max 16
    enum SpriteTileIndex : byte
    {
        Player,
        Bomb,
        Button,
        Block,
        Coin,
        Enemy1,
        Enemy2,
        Extra1,
        Extra2,
        Explosion,
        Door,
        Platform,
        Prize,
        Plane,
        Turret
    }

    class SpriteTileTable
    {
        private GameByteArray _spriteTiles;

        public byte GetTile(SpriteTileIndex index) => _spriteTiles[(int)index];

        public void SetTile(SpriteTileIndex index, byte value) => _spriteTiles[(int)index] = value;

        public byte DestructibleBlockTile => GetTile(SpriteTileIndex.Block);
        public byte CoinTile => GetTile(SpriteTileIndex.Coin);
        public byte DoorTile => GetTile(SpriteTileIndex.Door);
        public byte TurretTile => GetTile(SpriteTileIndex.Turret);

        public void BuildMemory(SystemMemoryBuilder memoryBuilder)
        {
            _spriteTiles = new GameByteArray(memoryBuilder.CurrentAddress, memoryBuilder.Memory);
            memoryBuilder.AddBytes(Enum.GetValues(typeof(SpriteTileIndex)).Length);
        }
    }
}
