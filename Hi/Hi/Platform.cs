using System;

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using TileEngine;

namespace Hi
{
	public class Platform : GameObject
	{
		public static int UP_DANGER = 1;
		public static int DOWN_DANGER = 2;
		public static int LEFT_DANGER = 4;
		public static int RIGHT_DANGER = 8;

		#region Declarations
		int danger;
		int limit;
		float elapsed = 0;
		Vector2 moveAmount;
		#endregion

		#region Constructor

		public Platform(ContentManager content, int x, int y){
			worldLocation = new Vector2 (TileMap.TileSize * x, TileMap.TileSize * y);
			defaultLocation = worldLocation;
			animations.Add ("plat", new AnimationStrip (
				content.Load<Texture2D> (@"Textures\Sprites\ladrilloGris"), 64, "plat"));
			animations ["plat"].LoopAnimation = true;
			animations ["plat"].FrameLength = 0.15f;
			animations ["plat"].setSignal (20);

			animations.Add ("table", new AnimationStrip (
				content.Load <Texture2D> (@"Textures\Sprites\MonsterA\Idle"), 48, "table"));
			animations ["table"].LoopAnimation = true;
			animations ["table"].setSignal (20);

			drawDepth = 0.875f;
			collisionRectangle = new Rectangle(0, 0, 64, 12);
			velocity = new Vector2(0, 3);
			limit = 96;
			this.danger = UP_DANGER;
			frameWidth = 64;
			frameHeight = 12;
			PlayAnimation ("plat");
			enabled = true;
		}
		#endregion

		#region Public Methods
		public override void Update (GameTime gameTime)
		{
			//al bajar, al traspasar por debajo no funciona
			if (!enabled)
				return; 
			Vector2 newPosition;
			if(!Game1.OnDrugs){
				worldLocation = defaultLocation;
				currentAnimation = "plat";
				moveAmount = Vector2.Zero;
				return;
			}
			elapsed += 1/60.0f;
            updateAnimation(gameTime);
			if (velocity.Y == 0 && velocity.X == 0)
				return;
			newPosition = defaultLocation + limit*(new Vector2 ((float)Math.Sin (velocity.X*elapsed), (float)Math.Sin (velocity.Y*elapsed)));
			moveAmount = newPosition - worldLocation;
			worldLocation = newPosition;
			currentAnimation = "plat";
            //base.Update(gameTime);

		}

		public bool ContainsPixel(Vector2 coord){
			return worldLocation.Y <= coord.Y && 
				worldLocation.Y + collisionRectangle.Height >= coord.Y &&
				worldLocation.X <= coord.X &&
				worldLocation.X + collisionRectangle.Width >= coord.X;
		}

		public Vector2 Positionate(GameObject go){
			return moveAmount + Vector2.UnitY*
				(worldLocation.Y - go.CollisionRectangle.Height - go.WorldLocation.Y);
		}

		private Vector2 autoCorrection(Vector2 moveAmount, Rectangle rekt){
			Vector2 movement = Vector2.Zero;
			if (!rekt.Intersects (CollisionRectangle))
				return moveAmount;
			if(rekt.X + rekt.Width - worldLocation.X < collisionRectangle.Width){
				movement.X = rekt.X + rekt.Width - worldLocation.X;
			}else{
				movement.X = worldLocation.X + collisionRectangle.Width - rekt.X;
			}

			//Console.Write (movement);
			return movement;


		}
		#endregion
	}
}

