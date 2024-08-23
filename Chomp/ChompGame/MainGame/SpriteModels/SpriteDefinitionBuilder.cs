using ChompGame.Data.Memory;

namespace ChompGame.MainGame.SpriteModels
{
    static class SpriteDefinitionBuilder
    {
        public static void BuildSpriteDefinitions(SystemMemoryBuilder memoryBuilder)
        {
            SpriteType _;
            //player
            new SpriteDefinition(memoryBuilder,
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
                secondTileOffset: 0,
                sizeX: 1,
                sizeY: 1,
                gravityStrength: GravityStrength.None,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.NoAnimation,
                collidesWithBackground: false,
                flipXWhenMovingLeft: false);

            //boss eye 
            new SpriteDefinition(memoryBuilder,
               secondTileOffset: 1,
               sizeX: 2,
               sizeY: 2,
               gravityStrength: GravityStrength.None,
               movementSpeed: MovementSpeed.Slow,
               animationStyle: AnimationStyle.NoAnimation,
               collidesWithBackground: false,
               flipXWhenMovingLeft: false);

            //boss jaw 
            new SpriteDefinition(memoryBuilder,
               secondTileOffset: 0,
               sizeX: 2,
               sizeY: 1,
               gravityStrength: GravityStrength.None,
               movementSpeed: MovementSpeed.Slow,
               animationStyle: AnimationStyle.NoAnimation,
               collidesWithBackground: false,
               flipXWhenMovingLeft: false);

            //boss arm 
            new SpriteDefinition(memoryBuilder,
               secondTileOffset: 0,
               sizeX: 1,
               sizeY: 1,
               gravityStrength: GravityStrength.None,
               movementSpeed: MovementSpeed.Slow,
               animationStyle: AnimationStyle.NoAnimation,
               collidesWithBackground: false,
               flipXWhenMovingLeft: false);

            //prize
            new SpriteDefinition(memoryBuilder,
                secondTileOffset: 0,
                sizeX: 1,
                sizeY: 1,
                gravityStrength: GravityStrength.None,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.NoAnimation,
                collidesWithBackground: true,
                flipXWhenMovingLeft: false);

            //player head
            new SpriteDefinition(memoryBuilder,
                secondTileOffset: 0,
                sizeX: 1,
                sizeY: 1,
                gravityStrength: GravityStrength.None,
                movementSpeed: MovementSpeed.Fast,
                animationStyle: AnimationStyle.NoAnimation,
                collidesWithBackground: false,
                flipXWhenMovingLeft: false);

            //plane
            new SpriteDefinition(memoryBuilder,
                secondTileOffset: 0,
                sizeX: 2,
                sizeY: 1,
                gravityStrength: GravityStrength.None,
                movementSpeed: MovementSpeed.VeryFast,
                animationStyle: AnimationStyle.NoAnimation,
                collidesWithBackground: true,
                flipXWhenMovingLeft: false);

            //rocket
            new SpriteDefinition(memoryBuilder,
              secondTileOffset: 0,
              sizeX: 2,
              sizeY: 1,
              gravityStrength: GravityStrength.None,
              movementSpeed: MovementSpeed.Fast,
              animationStyle: AnimationStyle.NoAnimation,
              collidesWithBackground: true,
              flipXWhenMovingLeft: false);

            //ogre
            new SpriteDefinition(memoryBuilder,
              secondTileOffset: 0,
              sizeX: 1,
              sizeY: 2,
              gravityStrength: GravityStrength.Medium,
              movementSpeed: MovementSpeed.VerySlow,
              animationStyle: AnimationStyle.AnimateWhenMoving,
              collidesWithBackground: true,
              flipXWhenMovingLeft: true);

            //ogre bullet
            new SpriteDefinition(memoryBuilder,
              secondTileOffset: 0,
              sizeX: 1,
              sizeY: 1,
              gravityStrength: GravityStrength.Low,
              movementSpeed: MovementSpeed.Fast,
              animationStyle: AnimationStyle.NoAnimation,
              collidesWithBackground: false,
              flipXWhenMovingLeft: false);

            //crocodile
            new SpriteDefinition(memoryBuilder,
              secondTileOffset: 0,
              sizeX: 2,
              sizeY: 1,
              gravityStrength: GravityStrength.High,
              movementSpeed: MovementSpeed.Fast,
              animationStyle: AnimationStyle.AnimateWhenMoving,
              collidesWithBackground: true,
              flipXWhenMovingLeft: true);

            _ = SpriteType.Mage;
            new SpriteDefinition(memoryBuilder,
                secondTileOffset: 1,
                sizeX: 2,
                sizeY: 2,
                gravityStrength: GravityStrength.None,
                movementSpeed: MovementSpeed.VerySlow,
                animationStyle: AnimationStyle.AlwaysAnimate,
                collidesWithBackground: false,
                flipXWhenMovingLeft: true);
        }
    }
}
