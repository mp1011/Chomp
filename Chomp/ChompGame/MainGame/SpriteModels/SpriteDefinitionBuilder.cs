﻿using ChompGame.Data.Memory;

namespace ChompGame.MainGame.SpriteModels
{
    static class SpriteDefinitionBuilder
    {
        public static void BuildSpriteDefinitions(SystemMemoryBuilder memoryBuilder)
        {
            
            //player
            new SpriteDefinition(memoryBuilder,
                tile: 1,
                secondTileOffset: 1,
                sizeX: 1,
                sizeY: 2,
                gravityStrength: GravityStrength.Medium,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.AnimateLowerTileOnly,
                collidesWithBackground: true,
                flipXWhenMovingLeft: true);

            //lizard
            new SpriteDefinition(memoryBuilder,
                tile: 3,
                secondTileOffset: 1,
                sizeX: 1,
                sizeY: 2,
                gravityStrength: GravityStrength.High,
                movementSpeed: MovementSpeed.Slow,
                animationStyle: AnimationStyle.AnimateWhenMoving,
                collidesWithBackground: true,
                flipXWhenMovingLeft: true,
                stopsAtLedges: true);

            //lizard fireball
            new SpriteDefinition(memoryBuilder,
                tile: 5,
                secondTileOffset: 0,
                sizeX: 1,
                sizeY: 1,
                gravityStrength: GravityStrength.None,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.NoAnimation,
                collidesWithBackground: false,
                flipXWhenMovingLeft: true);

            //bird
            new SpriteDefinition(memoryBuilder,
                tile: 17,
                secondTileOffset: 0,
                sizeX: 2,
                sizeY: 1,
                gravityStrength: GravityStrength.None,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.AlwaysAnimate,
                collidesWithBackground: false,
                flipXWhenMovingLeft: true);

            //bomb
            new SpriteDefinition(memoryBuilder,
                tile: 2,
                secondTileOffset: 0,
                sizeX: 1,
                sizeY: 1,
                gravityStrength: GravityStrength.Medium,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.NoAnimation,
                collidesWithBackground: true,
                flipXWhenMovingLeft: false);

            //door 
            new SpriteDefinition(memoryBuilder,
               tile: Constants.DoorTile,
               secondTileOffset: 1,
               sizeX: 1,
               sizeY: 2,
               gravityStrength: GravityStrength.None,
               movementSpeed: MovementSpeed.VerySlow,
               animationStyle: AnimationStyle.NoAnimation,
               collidesWithBackground: false,
               flipXWhenMovingLeft: false);

            //platform
            new SpriteDefinition(memoryBuilder,
              tile: Constants.PlatformTile,
              secondTileOffset: 0,
              sizeX: 2,
              sizeY: 1,
              gravityStrength: GravityStrength.None,
              movementSpeed: MovementSpeed.VerySlow,
              animationStyle: AnimationStyle.NoAnimation,
              collidesWithBackground: false,
              flipXWhenMovingLeft: false);

            //explosion
            new SpriteDefinition(memoryBuilder,
              tile: Constants.ExplosionTile,
              secondTileOffset: 0,
              sizeX: 1,
              sizeY: 1,
              gravityStrength: GravityStrength.High,
              movementSpeed: MovementSpeed.Slow,
              animationStyle: AnimationStyle.AlwaysAnimate,
              collidesWithBackground: false,
              flipXWhenMovingLeft: false);

            //button
            new SpriteDefinition(memoryBuilder,
             tile: Constants.ButtonTile,
             secondTileOffset: 0,
             sizeX: 1,
             sizeY: 1,
             gravityStrength: GravityStrength.None,
             movementSpeed: MovementSpeed.Fast,
             animationStyle: AnimationStyle.NoAnimation,
             collidesWithBackground: false,
             flipXWhenMovingLeft: false);

            //chomp
            new SpriteDefinition(memoryBuilder,
             tile: 3,
             secondTileOffset: 1,
             sizeX: 2,
             sizeY: 2,
             gravityStrength: GravityStrength.None,
             movementSpeed: MovementSpeed.Fast,
             animationStyle: AnimationStyle.AlwaysAnimate,
             collidesWithBackground: false,
             flipXWhenMovingLeft: false);

            //boss fireball
            new SpriteDefinition(memoryBuilder,
                tile: 8,
                secondTileOffset: 0,
                sizeX: 1,
                sizeY: 1,
                gravityStrength: GravityStrength.None,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.NoAnimation,
                collidesWithBackground: false,
                flipXWhenMovingLeft: false);
        }
    }
}
