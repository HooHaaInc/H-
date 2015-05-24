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
		List<GameObject> aboveThings = new List<GameObject>();
		int danger;
		int limit;
		float elapsed = 0;
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
			velocity = new Vector2(3, 0);
			limit = 64;
			this.danger = 0;
			frameWidth = 64;
			frameHeight = 12;
			PlayAnimation ("plat");
			enabled = true;
		}
		#endregion

		#region Public Methods
		public override void Update (GameTime gameTime)
		{
			if (!enabled)
				return; 
			Vector2 newPosition;
			if(!Game1.OnDrugs){
				newPosition = defaultLocation;
				newPosition = new Vector2 (
					MathHelper.Clamp (newPosition.X, 0, Camera.WorldRectangle.Width - frameWidth),
					MathHelper.Clamp (newPosition.Y, 2 * (-TileMap.TileSize), Camera.WorldRectangle.Height - frameHeight));
				worldLocation = newPosition;
				currentAnimation = "table";
				for (int i=aboveThings.Count-1; i>=0; --i) {
					if (aboveThings [i].CollisionRectangle.Intersects (CollisionRectangle) && velocity.Y == 0)
						aboveThings [i].AutoMove = new Vector2 (0, 4);
					aboveThings.RemoveAt (i);
				}
				return;
			}
			elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;;
            updateAnimation(gameTime);
			if (velocity.Y == 0 && velocity.X == 0)
				return;
			newPosition = defaultLocation + limit*(new Vector2 ((float)Math.Sin (velocity.X*elapsed), (float)Math.Sin (velocity.Y*elapsed)));
			newPosition = new Vector2 (
				MathHelper.Clamp (newPosition.X, 0, Camera.WorldRectangle.Width - frameWidth),
				MathHelper.Clamp (newPosition.Y, 2 * (-TileMap.TileSize), Camera.WorldRectangle.Height - frameHeight));
			Vector2 moveAmount = newPosition - worldLocation;
			for (int i=aboveThings.Count-1; i>=0; --i) {
				//Console.Write (moveAmount);
				if (aboveThings [i].CollisionRectangle.Intersects (CollisionRectangle) && velocity.Y == 0)
					aboveThings [i].AutoMove = new Vector2(0, 4);
				else
					aboveThings [i].AutoMove = moveAmount; //autoCorrection(moveAmount, aboveThings[i].CollisionRectangle);
				aboveThings.RemoveAt (i);
			}
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

		public void addAbove(GameObject go){
			for (int i=0; i<aboveThings.Count; ++i)
				if (aboveThings [i] == go)
					return;
			aboveThings.Add (go);
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

