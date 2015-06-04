using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using TileEngine;
using Utils;
using System.IO;

namespace Hi.Android
{
	public class MainGame : Game
	{
		#region Declarations
		public GraphicsDeviceManager graphics;
		public SpriteBatch spriteBatch;
		Player player;
		//SpriteFont pericles8;
		Song normal,high,gettingHi,gettingNormal,title,rip;
		float sec = 0.0f;
		Boolean entro = true; 
		Vector2 scorePosition = new Vector2(20, 10);
		enum GameState { TitleScreen, Playing, PlayerDead, GameOver, Help, Paused };
		GameState gameState = GameState.TitleScreen;
		Vector2 gameOverPosition;
		Vector2 livesPosition;
		Vector2 inyeccionPosition;
		Vector2[] menuPositions = new Vector2[3];
		Vector2 framesPosition;
		Vector2 pausePosition;
		Rectangle pauseRectangle;
		//Rectangle screenRect;
		int helpIndex = 0;
		//Texture2D titleScreen;
		float deathTimer = 0.0f;
		float deathDelay = 2.0f;
		BitFont myFont;
		TouchCollection lastState;
		public static bool OnDrugs = false;
		#endregion

		#region Constructor
		public MainGame ()
		{
			graphics = new GraphicsDeviceManager (this);
			graphics.IsFullScreen = true;

			Content.RootDirectory = "HiContent";
		}
		#endregion

		#region Overrided Methods
		protected override void Initialize ()
		{
			int width = TouchPanel.DisplayWidth;
			int height = TouchPanel.DisplayHeight;
			gameOverPosition = new Vector2 (width / 3, height / 2);
			livesPosition = new Vector2(0, 0);
			inyeccionPosition = new Vector2 (0, 20);
			menuPositions [0] = new Vector2 (width / 3, height / 2);
			menuPositions [1] = new Vector2 (width / 3, height / 2+50);
			menuPositions [2] = new Vector2 (width / 3, height / 2+100);
			framesPosition = new Vector2 (width - 60, 0);
			pausePosition = gameOverPosition;
			pauseRectangle = new Rectangle (3 * width / 4, 0, width / 4, height / 3);
			base.Initialize ();
		}

		protected override void LoadContent ()
		{
			spriteBatch = new SpriteBatch (GraphicsDevice);
			using (var stream = TitleContainer.OpenStream ("HiContent/Textures/PlatformTiles.png"))
			{
				TileMap.Initialize(
					Texture2D.FromStream (this.GraphicsDevice, stream), 
					TouchPanel.DisplayWidth,
					TouchPanel.DisplayHeight
				);

			}
			Camera.Position = Vector2.Zero;
			Camera.ViewPortWidth = TouchPanel.DisplayWidth;
			Camera.ViewPortHeight = TouchPanel.DisplayHeight;
			Camera.WorldRectangle = new Rectangle (0, 0, TileMap.MapWidth * TileMap.TileSize, TileMap.MapHeight * TileMap.TileSize);


			normal = Content.Load<Song> (@"sounds/normal");
			high = Content.Load<Song> (@"sounds/High.wav");
			gettingHi = Content.Load<Song> (@"sounds/gettinHi");
			gettingNormal = Content.Load<Song> (@"sounds/gettingBack");
			title = Content.Load<Song> (@"sounds/titlescreen");
			rip = Content.Load<Song> (@"sounds/dead.wav");

			using (var stream = TitleContainer.OpenStream ("HiContent/Fonts/myFont/myFont_0.png"))
			{
				myFont = new BitFont (
					Path.Combine (Content.RootDirectory, "Fonts/myFont/myFont.fnt"),
					Texture2D.FromStream(GraphicsDevice, stream)
				);
			}

			player = new Player(GraphicsDevice);
			LevelManager.Initialize(GraphicsDevice, player);


			base.LoadContent ();
		}

		protected override void Update (GameTime gameTime)
		{
			TouchCollection touches = TouchPanel.GetState ();
//			Vector2 center = new Vector2 (TouchPanel.DisplayWidth / 2, TouchPanel.DisplayHeight / 2);
//			Vector2 velocity = new Vector2();
//			if (touches.Count > 0)
//			{
//				velocity.X = touches [0].Position.X - center.X;
//				velocity.Y = touches [0].Position.Y - center.Y;
//
//				if (velocity.X != 0.0f || velocity.Y != 0.0f)
//				{
//					velocity.Normalize();
//					const float desiredSpeed = 200;
//					velocity *= desiredSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
//				}
//			}
//			Camera.Move (velocity);
			float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

			switch(gameState){
			case GameState.TitleScreen:
				if (entro) {
					MediaPlayer.Stop ();
					//MediaPlayer.Play (title);
					entro = false;
				}

				for (int i = 0; i < touches.Count; ++i) {
					Rectangle buttonArea = new Rectangle (
						                       (int)menuPositions [0].X, 
						                       (int)menuPositions [0].Y, 
						                       TouchPanel.DisplayWidth / 3, 
						                       50);
					if (buttonArea.Contains (touches [i].Position)) {
						StartNewGame ();
						gameState = GameState.Playing;
						//MediaPlayer.Play (normal);
						break;
					}
					buttonArea.X = (int)menuPositions [1].X;
					buttonArea.Y = (int)menuPositions [1].Y;
					if (buttonArea.Contains (touches [i].Position)) {
						gameState = GameState.Help;
						helpIndex = 0;
						break;
					}
					buttonArea.X = (int)menuPositions [2].X;
					buttonArea.Y = (int)menuPositions [2].Y;
					if (buttonArea.Contains (touches [i].Position)) {
						Exit ();
						break;
					}

				}
				break;
			case GameState.Help:
				if (touches.Count > 0)
				{
					helpIndex++;
					if (helpIndex == 3) gameState = GameState.TitleScreen;

				}
				break;
			case GameState.Playing:
				player.Update (gameTime);
				LevelManager.Update (gameTime);
				sec += (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (player.Dead) {
					MediaPlayer.Pause ();
					//MediaPlayer.Play (rip);
					MediaPlayer.IsRepeating = false;
					if (player.LivesRemaining > 0) {
						gameState = GameState.PlayerDead;
						deathTimer = 0.0f;
					} else {
						gameState = GameState.GameOver;
						deathTimer = 0.0f;
					}
				} if (OnDrugs) {
					if (!entro && sec > 4.2f) {
						MediaPlayer.Pause ();
						//MediaPlayer.Play (high);
						MediaPlayer.IsRepeating = true;
						entro = true;
					}
					if (!TileMap.OnDrugs) {
						MediaPlayer.Pause ();
						//MediaPlayer.Play (gettingHi);
						sec = 0.0f;
						entro = false;
						MediaPlayer.IsRepeating = false;
						TileMap.OnDrugs = true;
						if(!TileMap.isRectanglePassable(player.CollisionRectangle)){
							player.Kill ();
						}
					}
				} else{
					if (!entro && sec > 2.0f) {
						MediaPlayer.Pause ();
						//MediaPlayer.Play (normal);
						MediaPlayer.IsRepeating = true;
						entro = true;
					}
					if (TileMap.OnDrugs) {
						MediaPlayer.Pause ();
						//MediaPlayer.Play (gettingNormal);
						sec = 0.0f;
						entro = false;
						MediaPlayer.IsRepeating = false;
						TileMap.OnDrugs = false;
						if(!TileMap.isRectanglePassable(player.CollisionRectangle)){
							player.GoSafe ();
						}
					}
				}
				for(int i=0; i<touches.Count; ++i)
					if (pauseRectangle.Contains(touches[i].Position) && !lastState.Contains(touches[i]))
					{
						MediaPlayer.Volume = 0.3f;
						gameState = GameState.Paused;
					}
				break;
			case GameState.Paused:
				sec += (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (OnDrugs && (!entro && sec > 4.2f)) {
					MediaPlayer.Pause ();
					//MediaPlayer.Play (high);
					MediaPlayer.IsRepeating = true;
					entro = true;
				}else if (!OnDrugs && (!entro && sec > 2.0f)) {
					MediaPlayer.Pause ();
					//MediaPlayer.Play (normal);
					MediaPlayer.IsRepeating = true;
					entro = true;
				}
				for(int i=0; i<touches.Count; ++i)
					if (pauseRectangle.Contains(touches[i].Position) && !lastState.Contains(touches[i]))
					{
						MediaPlayer.Volume = 1.0f;
						gameState = GameState.Playing;
					}
				break;
			case GameState.PlayerDead:
				player.Update(gameTime);
				LevelManager.Update(gameTime);

				deathTimer += elapsed;
				if (deathTimer > deathDelay){
					player.WorldLocation = Vector2.Zero;
					LevelManager.ReloadLevel();
					player.Revive();
					entro = false;
					gameState = GameState.Playing;
				}
				break;
			case GameState.GameOver:
				player.Update(gameTime);
				deathTimer += elapsed;
				if (deathTimer > deathDelay){
					gameState = GameState.TitleScreen;
					player.WorldLocation = Vector2.Zero;
				}
				break;
			}
			lastState = touches;
			base.Update (gameTime);
		}

		protected override void Draw (GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin(
				SpriteSortMode.Immediate,
				BlendState.AlphaBlend);

			switch(gameState){
			case GameState.TitleScreen:
				//spritebatch.Draw(titleScreen);
				myFont.DrawText (spriteBatch, menuPositions [0], "Jugar");
				myFont.DrawText (spriteBatch, menuPositions [1], "Instrucciones");
				myFont.DrawText (spriteBatch, menuPositions [2], "Salir");
				break;
			case GameState.Help:
				switch(helpIndex){
				case 0:
					spriteBatch.Draw(Content.Load<Texture2D>(@"Textures\HelpMenus\HelpMenu1"), Vector2.Zero, Color.White);
					break;
				case 1:
					spriteBatch.Draw(Content.Load<Texture2D>(@"Textures\HelpMenus\HelpMenu2"), Vector2.Zero, Color.White);
					break;
				case 2:
					spriteBatch.Draw(Content.Load<Texture2D>(@"Textures\HelpMenus\HelpMenu3"), Vector2.Zero, Color.White);
					break;
				}
				break;
			case GameState.Playing:
			case GameState.Paused:
			case GameState.PlayerDead:
			case GameState.GameOver:
				TileMap.Begin(spriteBatch);
				player.Draw(spriteBatch);
				LevelManager.Draw(spriteBatch);
				TileMap.End (spriteBatch);
				myFont.DrawText (spriteBatch, scorePosition, "Drogas: " + player.drugCount.ToString ());
				myFont.DrawText (spriteBatch, inyeccionPosition, "Inyecciones: " + player.inyecciones.ToString ());
				myFont.DrawText (spriteBatch, livesPosition, "Vidas: " + player.LivesRemaining.ToString ());
				myFont.DrawText (spriteBatch, framesPosition, (1/(float)(gameTime.ElapsedGameTime.TotalSeconds)).ToString ());
				if (gameState == GameState.GameOver)
					myFont.DrawText (spriteBatch, gameOverPosition, "G A M E O V E R !");
				else if (gameState == GameState.Paused) {
					spriteBatch.Draw(
						Content.Load<Texture2D>(@"Textures\greenTexture"), 
						Camera.ViewPort,
						Color.White);
					myFont.DrawText (spriteBatch, pausePosition, "PAUSA");
				}
				break;
			}

			spriteBatch.End();
			base.Draw(gameTime);
		}
		#endregion

		#region Helper Method
		private void StartNewGame()
		{
			player.Revive();
			player.LivesRemaining = 3;
			player.WorldLocation = Vector2.Zero;
			LevelManager.LoadLevel(0);
		}
		#endregion
	}


}

