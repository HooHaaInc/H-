using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TileEngine;

namespace Hi
{
    public class Drug : GameObject
    {
        #region Constructor
        public Drug(ContentManager Content, int cellX, int cellY)
        {
            worldLocation.X = TileMap.TileSize * cellX;
            worldLocation.Y = TileMap.TileSize * cellY;
			defaultLocation = worldLocation;
            frameWidth = 48;
            frameHeight = 48;
            animations.Add("idle",
                            new AnimationStrip(Content.Load<Texture2D>(@"Textures/drug"), 48, "idle"));
            animations["idle"].LoopAnimation = true;
            animations["idle"].FrameLength = 0.15f;
            animations["idle"].setSignal(5);
            PlayAnimation("idle");
            drawDepth = 0.875f;
            collisionRectangle = new Rectangle(0, 0, 32, 32);
            enabled = true;
        }
        #endregion
    }
}