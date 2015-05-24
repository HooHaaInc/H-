using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TileEngine;

namespace Hi {
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
            get { return Game1.OnDrugs || type == 3; }
        }
        #region Constructor
        public Enemy(ContentManager content, int cellX, int cellY, int type) {
            this.type = type;
			defaultLocation = new Vector2 (TileMap.TileSize * cellX, TileMap.TileSize * cellY);
			worldLocation = defaultLocation;
			switch(type){
			case 1:
				animations.Add ("idle",
				                new AnimationStrip (
					content.Load<Texture2D> (
					@"Textures\Sprites\MonsterA\Idle"),
					48,
					"idle"));
				animations ["idle"].LoopAnimation = true;
				animations ["idle"].setSignal (12);

				animations.Add ("run",
				                new AnimationStrip (
					content.Load<Texture2D> (
					@"Textures\Sprites\MonsterA\Run"),
					48,
					"run"));
				animations ["run"].FrameLength = 0.1f;
				animations ["run"].LoopAnimation = true;

				animations ["run"].setSignal (11);

				animations.Add ("die",
				                new AnimationStrip (
					content.Load<Texture2D> (
					@"Textures\Sprites\MonsterA\Die"),
					48,
					"die"));
				animations ["die"].LoopAnimation = false;

				animations ["die"].setSignal (14);

				frameWidth = 48;
				frameHeight = 48;
				CollisionRectangle = new Rectangle (9, 1, 25, 47);

				enabled = true;
				PlayAnimation ("run");
				break;
			case 2:
				animations.Add ("idle",
				                new AnimationStrip (
					content.Load<Texture2D> (
					@"Textures\Sprites\MonsterC\Idle"),
					48,
					"idle"));
				animations ["idle"].LoopAnimation = true;
				animations ["idle"].setSignal (12);

				animations.Add ("run",
				                new AnimationStrip (
					content.Load<Texture2D> (
					@"Textures\Sprites\MonsterC\Run"),
					48,
					"run"));
				animations ["run"].FrameLength = 0.1f;
				animations ["run"].LoopAnimation = true;

				animations ["run"].setSignal (11);

				animations.Add ("die",
				                new AnimationStrip (
					content.Load<Texture2D> (
					@"Textures\Sprites\MonsterC\Die"),
					48,
					"die"));
				animations ["die"].LoopAnimation = false;

				animations ["die"].setSignal (14);

				frameWidth = 48;
				frameHeight = 48;
				CollisionRectangle = new Rectangle (9, 1, 25, 47);

				enabled = true;
				PlayAnimation ("run");
				break;
            case 3:
				animations.Add ("idle",
				                new AnimationStrip (
					content.Load<Texture2D> (
					@"Textures\Sprites\MonsterB\Idle"),
					48,
					"idle"));
				animations ["idle"].LoopAnimation = true;
				animations ["idle"].setSignal (12);

				animations.Add ("run",
				                new AnimationStrip (
					content.Load<Texture2D> (
					@"Textures\Sprites\MonsterB\Run"),
					48,
					"run"));
				animations ["run"].FrameLength = 0.1f;
				animations ["run"].LoopAnimation = true;

				animations ["run"].setSignal (11);

				animations.Add ("die",
				                new AnimationStrip (
					content.Load<Texture2D> (
					@"Textures\Sprites\MonsterB\Die"),
					48,
					"die"));
				animations ["die"].LoopAnimation = false;

				animations ["die"].setSignal (14);

				frameWidth = 48;
				frameHeight = 48;
				CollisionRectangle = new Rectangle (9, 1, 25, 47);

				enabled = true;
				PlayAnimation ("run");
				break;
			}
        }
        #endregion

        #region Public Methods
        public override void Update(GameTime gameTime) {
            if (!enabled || !Camera.onCamera(WorldRectangle)) return;
            Vector2 oldLocation = worldLocation;

            if (!Dead && (Game1.OnDrugs || type == 3) ) {
                velocity = new Vector2(0, velocity.Y);

                Vector2 direction = new Vector2(1, 0);
                flipped = true;

                if (facingLeft) {
                    direction = new Vector2(-1, 0);
                    flipped = false;
                }

                direction *= walkSpeed;
                velocity += direction;
                velocity += fallSpeed;
            }

            //base.Update(gameTime);


            //if (!enabled || ! Camera.onCamera(collisionRectangle)) return;
            Vector2 newPosition;
            if (!Game1.OnDrugs && type!= 3)
            {
                newPosition = defaultLocation;
                newPosition = new Vector2(MathHelper.Clamp(newPosition.X, 0, Camera.WorldRectangle.Width - frameWidth),
                                          MathHelper.Clamp(newPosition.Y, 2 * (-TileMap.TileSize), Camera.WorldRectangle.Height - frameHeight));
                worldLocation = newPosition;
                if (currentAnimation == "die") enabled = false;
                currentAnimation = "idle";
                updateAnimation(gameTime);
                return;
            }
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            updateAnimation(gameTime);
            if (velocity.Y != 0) onGround = false;

            Vector2 moveAmount = velocity * elapsed + autoMove;

            moveAmount = horizontalCollisionTest(moveAmount);
            moveAmount = verticalCollisionTest(moveAmount);
            newPosition = worldLocation + moveAmount;
            newPosition = new Vector2(MathHelper.Clamp(newPosition.X, 0, Camera.WorldRectangle.Width - frameWidth),
            MathHelper.Clamp(newPosition.Y, 2 * (-TileMap.TileSize), Camera.WorldRectangle.Height - frameHeight));
            worldLocation = newPosition;



			if (!Game1.OnDrugs && type !=3) {
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
