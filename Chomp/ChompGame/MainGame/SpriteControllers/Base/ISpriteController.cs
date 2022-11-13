﻿using ChompGame.Data;

namespace ChompGame.MainGame.SpriteControllers.Base
{
    interface ISpriteController
    {
        byte SpriteIndex { get; set; }

        Sprite GetSprite();

        WorldSprite WorldSprite { get; }

        void ConfigureSprite(Sprite sprite);
    }
}