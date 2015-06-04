using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using TileEngine;

namespace Hi
{
    public class Player : GameObject
    {
        private Vector2 fallSpeed = new Vector2(0, 15);
        private float moveScale = 180.0f;
        private bool dead = false;
        public int drugCount = 0;
        private int score = 0;
        private int livesRemaining = 3;
        public int inyecciones = 5;
        KeyboardState lastState;
        public bool paused = false;
		private Vector2 safePosition;

        public bool Dead
        {
            get { return dead; }
        }

        public int Score
        {
            get { return score; }
            set { score = value; }
        }

        public int LivesRemaining
        {
            get { return livesRemaining; }
            set { livesRemaining = value; }
        }


        #region Constructor
        public Player(ContentManager content)
        {
            animations.Add("idle", new AnimationStrip(content.Load<Texture2D>(@"Textures\Sprites\Player\Idle"), 48, "idle"));

            animations["idle"].LoopAnimation = true;
            animations["idle"].setSignal(2);

            animations.Add("run", new AnimationStrip(content.Load<Texture2D>(@"Textures\Sprites\Player\Run"), 48, "run"));
            animations["run"].LoopAnimation = true;
            animations["run"].FrameLength = 0.1f;
            animations["run"].setSignal(11);

            animations.Add("jump",
                new AnimationStrip(
                    content.Load<Texture2D>(@"Textures\Sprites\Player\Jump"),
                    48,
                    "jump"));
            animations["jump"].LoopAnimation = false;
            animations["jump"].FrameLength = 0.08f;
            animations["jump"].NextAnimation = "idle";

            animations["jump"].setSignal(4);
            animations["jump"].setSignal(7);
            //  animations["jump"].setSignal(8);

            animations.Add("die",
                new AnimationStrip(
                    content.Load<Texture2D>(@"Textures\Sprites\Player\Die"),
                    48,
                    "die"));
            animations["die"].LoopAnimation = false;
            animations["die"].setSignal(15);
            animations["die"].FrameLength = .1f;
            frameWidth = 48;
            frameHeight = 48;
            CollisionRectangle = new Rectangle(9, 1, 23, 46);

            drawDepth = 0.825f;

            enabled = true;
            PlayAnimation("idle");
        }
        #endregion

        #region Public Methods
        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (!Dead)
            {
                string newAnimation = "idle";

                velocity = new Vector2(0, velocity.Y);
                GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
                if (keyState.IsKeyDown(Keys.Q) && lastState.IsKeyUp(Keys.Q))
					Clean();
                if (keyState.IsKeyDown(Keys.Left) ||
                    keyState.IsKeyDown(Keys.A))
                {
                    flipped = false;
                    newAnimation = "run";
                    velocity = new Vector2(-moveScale, velocity.Y);
                } if (keyState.IsKeyDown(Keys.Right) ||
                    keyState.IsKeyDown(Keys.D))
                {
                    flipped = true;
                    newAnimation = "run";
                    velocity = new Vector2(moveScale, velocity.Y);
                } if (keyState.IsKeyDown(Keys.Space) ||
                    (gamePad.Buttons.A == ButtonState.Pressed))
                {
                    if (onGround)
                    {
                        Jump();
                        newAnimation = "jump";
                    }
                } if (keyState.IsKeyDown(Keys.Up) ||
                    keyState.IsKeyDown(Keys.W))
                {
                    checkLevelTransition();
				} if (keyState.IsKeyDown(Keys.E) && lastState.IsKeyUp(Keys.E))
				{
					Drug();
					if (dead)
					{
						newAnimation = "die";
					}
				}

				if (currentAnimation == "jump")
                {
                    newAnimation = "jump";
                } else if (currentAnimation == "die")
                {
                    newAnimation = "die";
                }

                

                if (newAnimation != currentAnimation)
                {
                    PlayAnimation(newAnimation);
                }
                lastState = keyState;
            }
            float elapsed = 1/60.0f;
            velocity += fallSpeed;

            repositionCamera();
            //base.Update(gameTime, true);

            if (!enabled) return;

            updateAnimation(gameTime);
            if (velocity.Y != 0) onGround = false;
            Vector2 moveAmount = velocity * elapsed + AutoMove;
            moveAmount = horizontalCollisionTest(moveAmount);
            moveAmount = verticalCollisionTest(moveAmount);
            AutoMove = Vector2.Zero;
            if (!dead)
            {
                if (!onGround)
                {
                    if (currentAnimation == "jump")
                    {
                        if (moveAmount.Y > 0 && animations["jump"].signalIndex == 0)
                        {
                            //animations["jump"].nextFrame();
                            animations["jump"].currentFrame = 7;
                            animations["jump"].signalIndex = 1;
                        }
                    }
                    else
                    {
                        if (moveAmount.Y > 0)
                        {
                            currentAnimation = "jump";
                            PlayAnimation("jump");
                            animations["jump"].currentFrame = 7;
                            animations["jump"].signalIndex = 1;
                        }
                    }
                }
                else
                {
                    if (currentAnimation == "jump") 
						if (animations["jump"].signalIndex == 1) animations["jump"].nextFrame();
                }
            }
           	worldLocation += moveAmount;
			if(worldLocation.Y > Camera.WorldRectangle.Height){
				Kill ();
			}
        }

        public void Jump()
        {
            velocity.Y = -450;
            animations["jump"].signalIndex = 0;
            PlayAnimation("jump");
        }

        public void Kill()
        {
			if (!dead) {
				LivesRemaining--;
				velocity.X = 0;
				dead = true;
				PlayAnimation ("die");
			}
        }

		public void Clean()
		{
			if (!Game1.OnDrugs) return;
			if (inyecciones > 0)
			{
				inyecciones--;
				Game1.OnDrugs = false;
			}
			
		}

        public void Drug()
        {
            if (!Game1.OnDrugs && drugCount > 0)
            {
                Game1.OnDrugs = true;
                drugCount--;
				safePosition = worldLocation;
            }
        }

		public void GoSafe(){
			worldLocation = safePosition;
		}

        public void Revive()
        {
            drugCount = 0;
            inyecciones = 5;
            PlayAnimation("idle");
			velocity = Vector2.Zero;
            dead = false;
			Game1.OnDrugs = false;
        }
        #endregion

        #region Helper Methods
        private void repositionCamera()
        {
            int screenLocX = (int)Camera.WorldToScreen(worldLocation).X;

            if (screenLocX > 410)
            {
                Camera.Move(new Vector2(screenLocX - 410, 0));
            }

            if (screenLocX < 390)
            {
                Camera.Move(new Vector2(screenLocX - 390, 0));
            }

            int screenLocY = (int)Camera.WorldToScreen(worldLocation).Y;

            if (screenLocY > 250)
            {
                Camera.Move(new Vector2(0, screenLocY - 250));
            }

            if (screenLocY < 230/*< 200*/)
            {
                Camera.Move(new Vector2(0, screenLocY - 230));
            }
        }

        private void checkLevelTransition()
        {
            Vector2 centerCell = TileMap.GetCellByPixel(WorldCenter);
            if (TileMap.MapSquareCodeValue(centerCell) == ("EXIT"))
            {
                livesRemaining = 1;
                Kill();
            }
                /*string[] code = TileMap.CellCodeValue(centerCell).Split('_');

                if (code.Length != 4)
                    return;

                LevelManager.LoadLevel(int.Parse(code[1]));

                WorldLocation = new Vector2(
                    int.Parse(code[2]) * TileMap.TileWidth,
                    int.Parse(code[3]) * TileMap.TileHeight);

                LevelManager.RespawnLocation = WorldLocation;

                velocity = Vector2.Zero;*/
            //}
        }

        public void setOnGround(bool onGround)
        {
            this.onGround = onGround;
        }
        #endregion


    }
}
