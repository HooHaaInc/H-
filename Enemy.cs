using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace Hi.Android {
    public class Enemy : GameObject {
        private Vector2 fallSpeed = new Vector2(0, 20);
        private float walkSpeed = 60.0f;
        private bool facingLeft = true;
        public bool Dead = false;
        private int type;


		public bool Killable {
			get { return type != 2; }
		}

        public bool Active {
			get { return MainGame.OnDrugs || type == 3; }
        }
        #region Constructor
		public Enemy(GraphicsDevice graphics, int cellX, int cellY, int type) {
            this.type = type;
			defaultLocation = new Vector2 (TileMap.TileSize * cellX, TileMap.TileSize * cellY);
			worldLocation = defaultLocation;
			string m = type == 1 ? "A" : type == 2 ? "C" : "B";

			using(var stream = TitleContainer.OpenStream("HiContent/Textures/Sprites/Monster" + m + "/Idle.png")){
				animations.Add ("idle",
					new AnimationStrip (
						Texture2D.FromStream(graphics, stream),
						48,
						"idle"));
			}

			animations ["idle"].LoopAnimation = true;
			animations ["idle"].setSignal (12);

			using(var stream = TitleContainer.OpenStream("HiContent/Textures/Sprites/Monster" + m + "/Run.png")) {
				animations.Add ("run",
					new AnimationStrip (
						Texture2D.FromStream(graphics, stream),
						48,
						"run"));
			}
			animations ["run"].FrameLength = 0.1f;
			animations ["run"].LoopAnimation = true;
			animations ["run"].setSignal (11);

			using(var stream = TitleContainer.OpenStream("HiContent/Textures/Sprites/Monster" + m + "/Die.png")) {
				animations.Add ("die",
					new AnimationStrip (
						Texture2D.FromStream(graphics, stream),
						48,
						"die"));
			}
			animations ["die"].LoopAnimation = false;
			
			animations ["die"].setSignal (14);
			
			frameWidth = 48;
			frameHeight = 48;
			CollisionRectangle = new Rectangle (9, 1, 25, 47);
			
			enabled = true;
			PlayAnimation ("run");
        }
        #endregion

        #region Public Methods
        public override void Update(GameTime gameTime) {
            if (!enabled || !Camera.ObjectIsVisible (WorldRectangle)) return;
            Vector2 oldLocation = worldLocation;

			if (!Dead && (MainGame.OnDrugs || type == 3) ) {
                velocity.X = 0;

                Vector2 direction = Vector2.UnitX;
                flipped = true;

                if (facingLeft) {
                    direction *= -1;
                    flipped = false;
                }

                direction *= walkSpeed;
                velocity += direction;
                velocity += fallSpeed;
            }

            Vector2 newPosition;
			if (!MainGame.OnDrugs && type!= 3)
            {
                worldLocation = defaultLocation;
                if (currentAnimation == "die") enabled = false;
                currentAnimation = "idle";
                updateAnimation(gameTime);
                return;
            }
            float elapsed = 1/60.0f;
            updateAnimation(gameTime);
            if (velocity.Y != 0) onGround = false;

            Vector2 moveAmount = velocity * elapsed + autoMove;

            moveAmount = horizontalCollisionTest(moveAmount);
            moveAmount = verticalCollisionTest(moveAmount);
            newPosition = worldLocation + moveAmount;
            worldLocation = newPosition;

			if(worldLocation.Y > Camera.WorldRectangle.Height){
				enabled = false;
			}


			if (!MainGame.OnDrugs && type !=3) {
				currentAnimation = "idle";
				return;
			} else
				currentAnimation = Dead ? "die" : "run";
            if (!Dead) {
                if (oldLocation == worldLocation) {
                    facingLeft = !facingLeft;
                }
            }
            else {
                if (animations[currentAnimation].FinishedPlaying) {
                    enabled = false;
                }
            }

        }
        #endregion

    }
}
