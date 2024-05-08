using ChompGame.Data;
using ChompGame.Extensions;
using ChompGame.MainGame.SpriteModels;

namespace ChompGame.MainGame.SpriteControllers
{
    class AnimationController
    {
        private readonly SpriteDefinition _spriteDefinition;
        private readonly SpriteTileTable _spriteTileTable;
        private readonly GameByte _levelTimer;

        public AnimationController(SpriteDefinition spriteDefinition, SpriteTileTable spriteTileTable, GameByte levelTimer)
        {
            _spriteDefinition = spriteDefinition;
            _spriteTileTable = spriteTileTable;
            _levelTimer = levelTimer;
        }

        public void Update(WorldSprite worldSprite, IMotion motion)
        {

            if (worldSprite.Status != WorldSpriteStatus.Active)
                return;

            var sprite = worldSprite.GetSprite();
            if (_spriteDefinition.FlipXWhenMovingLeft)
            {
                if (motion.TargetXSpeed < 0 && !sprite.FlipX)
                {
                    sprite.FlipX = true;
                }
                else if (motion.TargetXSpeed > 0 && sprite.FlipX)
                {
                    sprite.FlipX = false;
                }
            }

            // this is meant to allow for overriding the tile, but unsure if this will cause other problems
            if (_spriteDefinition.AnimationStyle == AnimationStyle.NoAnimation && sprite.SizeY == 1)                
                return;

            bool shouldAnimate = _spriteDefinition.AnimationStyle switch {
                AnimationStyle.AlwaysAnimate => true,
                AnimationStyle.AnimateWhenMoving => motion.XSpeed != 0,
                AnimationStyle.AnimateLowerTileOnly => motion.XSpeed != 0,
                _ => false
            };

            byte spriteTile = _spriteTileTable.GetTile(worldSprite.TileIndex);

            if (!shouldAnimate)
            {
                sprite.Tile = spriteTile;
                sprite.Tile2Offset = 1;
            }
            else if ((_levelTimer.Value % 16) == 0)
            {
                if (_spriteDefinition.AnimationStyle == AnimationStyle.AnimateLowerTileOnly)
                    sprite.Tile2Offset = sprite.Tile2Offset.Toggle(1, 2);
                else
                    sprite.Tile = sprite.Tile.Toggle(spriteTile, (byte)(spriteTile + sprite.SizeX));
            }
        }
    }
}
