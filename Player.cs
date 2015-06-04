using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;
using TileEngine;

namespace Hi.Android
{
    public class Player : GameObject
    {
		#region Declarations
        private Vector2 fallSpeed = new Vector2(0, 15);
        private float moveScale = 180.0f;
        private bool dead = false;
        public int drugCount = 0;
        private int score = 0;
        private int livesRemaining = 3;
        public int inyecciones = 5;
		TouchCollection lastState;
        public bool paused = false;
		private Vector2 safePosition;

		private readonly Rectangle JumpButtonArea;
		private readonly Rectangle DrugButtonArea;
		#endregion

		#region Properties
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
		#endregion


        #region Constructor
		public Player(GraphicsDevice graphics)
		{
			JumpButtonArea = new Rectangle (
				3 * TouchPanel.DisplayWidth / 4,
				2 * TouchPanel.DisplayHeight / 3,
				TouchPanel.DisplayWidth / 4,
				TouchPanel.DisplayHeight / 3
			);

			DrugButtonArea = new Rectangle (
				0,
				2 * TouchPanel.DisplayHeight / 3,
				TouchPanel.DisplayWidth / 4,
				TouchPanel.DisplayHeight / 3
			);

			using (var stream = TitleContainer.OpenStream ("HiContent/Textures/Sprites/Player/Idle.png")) {
				animations.Add ("idle", new AnimationStrip (
					Texture2D.FromStream (graphics, stream), 
					48, 
					"idle"));
			}

			animations ["idle"].LoopAnimation = true;
			animations ["idle"].setSignal (2);

			using (var stream = TitleContainer.OpenStream ("HiContent/Textures/Sprites/Player/Run.png")){
				animations.Add ("run", new AnimationStrip (
					Texture2D.FromStream (graphics, stream), 
					48, 
					"run"));
			}
            animations["run"].LoopAnimation = true;
            animations["run"].FrameLength = 0.1f;
            animations["run"].setSignal(11);

			using (var stream = TitleContainer.OpenStream ("HiContent/Textures/Sprites/Player/Jump.png")) {
				animations.Add ("jump",
					new AnimationStrip (
						Texture2D.FromStream(graphics, stream),
						48,
						"jump"));
			}
            animations["jump"].LoopAnimation = false;
            animations["jump"].FrameLength = 0.08f;
            animations["jump"].NextAnimation = "idle";

            animations["jump"].setSignal(4);
            animations["jump"].setSignal(7);
            //  animations["jump"].setSignal(8);

			using (var stream = TitleContainer.OpenStream("HiContent/Textures/Sprites/Player/Die.png")){
				animations.Add ("die",
					new AnimationStrip (
						Texture2D.FromStream(graphics, stream),
						48,
						"die"));
			}
            animations["die"].LoopAnimation = false;
            animations["die"].setSignal(15);
            animations["die"].FrameLength = .1f;
            frameWidth = 48;
            frameHeight = 48;
            CollisionRectangle = new Rectangle(9, 1, 23, 46);

            drawDepth = 0.825f;
			flipped = true;
            enabled = true;
            PlayAnimation("idle");
        }
        #endregion

        #region Public Methods
        public override void Update(GameTime gameTime)
        {
			TouchCollection keyState = TouchPanel.GetState();

            if (!Dead)
            {
                string newAnimation = "run";

				velocity = new Vector2(moveScale, velocity.Y);
				bool alreadyDrugged = false;
				for(int i=0; i< keyState.Count; ++i){
					if(!alreadyDrugged && DrugButtonArea.Contains(keyState[i].Position) &&
						!lastState.Contains(keyState[i])){
						if(MainGame.OnDrugs)
							Clean ();
						else
							Drug();
						alreadyDrugged = true;
					}if(onGround && JumpButtonArea.Contains(keyState[i].Position)){
						Jump();
						newAnimation = "jump";
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

            const float elapsed = 1/60.0f;
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
			if (!MainGame.OnDrugs) return;
			if (inyecciones > 0)
			{
				inyecciones--;
				MainGame.OnDrugs = false;
			}
			
		}

        public void Drug()
        {
			if (!MainGame.OnDrugs && drugCount > 0)
            {
				MainGame.OnDrugs = true;
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
			MainGame.OnDrugs = false;
        }
        #endregion

        #region Helper Methods
        private void repositionCamera()
        {
            int screenLocX = (int)Camera.WorldToScreen(worldLocation).X;

			if (screenLocX > TouchPanel.DisplayWidth/2)
            {
				Camera.Move(new Vector2(screenLocX - TouchPanel.DisplayWidth/2, 0));
            }

			if (screenLocX < TouchPanel.DisplayWidth/2)
            {
				Camera.Move(new Vector2(screenLocX - TouchPanel.DisplayWidth/2, 0));
            }

            int screenLocY = (int)Camera.WorldToScreen(worldLocation).Y;

			if (screenLocY > TouchPanel.DisplayHeight/2)
            {
				Camera.Move(new Vector2(0, screenLocY - TouchPanel.DisplayHeight/2));
            }

			if (screenLocY < TouchPanel.DisplayHeight/2 /*< 200*/)
            {
				Camera.Move(new Vector2(0, screenLocY - TouchPanel.DisplayHeight/2));
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
