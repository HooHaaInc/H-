#region Using Statements
using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using TileEngine;
using HooHaaUtils;
using System.IO;

#endregion

namespace LevelEditor
{
	/// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
		BitFont font;
		KeyboardState lastState;
		Vector2 cursor = new Vector2(0,0);
		Texture2D rect;
		string currentString;
		enum EditorState { Background = 0, Clean, Drugged, Foreground, Both, CellCode, Options }
		EditorState editorState = EditorState.Clean;
		string[] layer = new string[] { "BACKGROUND", "CLEAN", "DRUGGED", "FOREGROUND", "BOTH" };
		string[] options = new string[] {"Load Map", "Save Map", "Exit"};
		int currentOption = 0;
        Vector2 optionsPosition = new Vector2(350, 200);
		int currentMap = 0;

		MapSquare[] predefined = new MapSquare[] {
			new MapSquare(15, "", 0),
			new MapSquare(12, 14, "", 1),
			new MapSquare(14, 12, "", 2)
		};

		private Rectangle CursorRectangle{
			get{ 
				int dim = TileMap.TileSize;
				return new Rectangle (
					(int)(dim*cursor.X), 
					(int)(dim*cursor.Y),
					dim,
					dim); 
			}
		}

		private Vector2 CursorPosition{
			get{ 
				int dim = TileMap.TileSize;
				return new Vector2 (
					dim*cursor.X, 
					dim*cursor.Y); 
			}
		}

		public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "LevelEditorContent";	            
			graphics.PreferredBackBufferWidth = 800;
			graphics.PreferredBackBufferHeight = 480;
			graphics.ApplyChanges ();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
				
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
			Texture2D[] backgrounds = new Texture2D[1];
			backgrounds [0] = Content.Load<Texture2D> ("Textures/city");
			TileMap.Initialize (backgrounds, Content.Load <Texture2D> ("Textures/PlatformTiles"));
			font = new BitFont (Content);
			TileMap.spriteFont = font;
			rect = Content.Load <Texture2D> ("rect");
			Camera.WorldRectangle = new Rectangle(0, 0, 
			                                      TileMap.MapWidth * TileMap.TileSize, 
			                                      TileMap.MapHeight * TileMap.TileSize);
			Camera.ViewPort = new Rectangle (
				0,
				0, 
				graphics.PreferredBackBufferWidth, 
				graphics.PreferredBackBufferHeight);
            //TODO: use this.Content to load your game content here 
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
			KeyboardState keyState = Keyboard.GetState ();
			MapSquare prevtile = null;
			int prevground;
            // For Mobile devices, this logic will close the Game when the Back button is pressed
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
			{
				Exit();
			}
			switch(editorState){
			case EditorState.Background:
//				if (keyState.IsKeyDown (Keys.Up) && lastState.IsKeyUp (Keys.Up)) {
//					prevground = TileMap.GetBackgroundAtCell (cursor);//.GetMapSquareAtPixel (new Vector2 (cursor.X, cursor.Y));
//					if (cursor.Y > 0)
//						--cursor.Y;
//					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl))
//						TileMap.SetBackgroundAtCell (cursor, prevground);
//				} else if (keyState.IsKeyDown (Keys.Down) && lastState.IsKeyUp (Keys.Down)) {
//					prevground = TileMap.GetBackgroundAtCell (cursor);
//					if (cursor.Y < TileMap.BackgroundHeight - 1)
//						++cursor.Y;
//					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl))
//						TileMap.SetBackgroundAtCell (cursor, prevground);
//				} else if (keyState.IsKeyDown (Keys.Left) && lastState.IsKeyUp (Keys.Left)) {
//					prevground = TileMap.GetBackgroundAtCell (cursor);
//					if (cursor.X > 0)
//						--cursor.X;
//					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl))
//						TileMap.SetBackgroundAtCell (cursor, prevground);
//				} else if (keyState.IsKeyDown (Keys.Right) && lastState.IsKeyUp (Keys.Right)) {
//					prevground = TileMap.GetBackgroundAtCell (cursor);
//					if (cursor.X < TileMap.BackgroundWidth - 1)
//						++cursor.X;
//					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl))
//						TileMap.SetBackgroundAtCell (cursor, prevground);
//				} else 
				if (keyState.IsKeyDown (Keys.PageUp) && lastState.IsKeyUp (Keys.PageUp)) {
					++TileMap.BackgroundIndex;
				} else if (keyState.IsKeyDown (Keys.PageDown) && lastState.IsKeyUp (Keys.PageDown)) {
					--TileMap.BackgroundIndex;
				} else if (keyState.IsKeyDown (Keys.C) && lastState.IsKeyUp (Keys.C)) {
					//cursor = TileMap.GetCellByPixelBetweenLayers (CursorPosition, 0, 2);
					editorState = EditorState.Foreground;
				} else if (keyState.IsKeyDown (Keys.X) && lastState.IsKeyUp (Keys.X)) {
					//cursor = TileMap.GetCellByPixelBetweenLayers (CursorPosition, 0, 1);
					editorState = EditorState.Clean;
				} else if (keyState.IsKeyDown (Keys.Escape) && lastState.IsKeyUp (Keys.Escape)) {
					editorState = EditorState.Options;
				}
				repositionCamera ();
				break;
			case EditorState.Foreground:
				if (keyState.IsKeyDown (Keys.Up) && lastState.IsKeyUp (Keys.Up)) {
					prevground = TileMap.GetForegroundAtCell (cursor);//.GetMapSquareAtPixel (new Vector2 (cursor.X, cursor.Y));
					if (cursor.Y > 0)
						--cursor.Y;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl))
						TileMap.SetForegroundAtCell (cursor, prevground);
				} else if (keyState.IsKeyDown (Keys.Down) && lastState.IsKeyUp (Keys.Down)) {
					prevground = TileMap.GetForegroundAtCell (cursor);
					if (cursor.Y < TileMap.MapHeight - 1)
						++cursor.Y;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl))
						TileMap.SetForegroundAtCell (cursor, prevground);
				} else if (keyState.IsKeyDown (Keys.Left) && lastState.IsKeyUp (Keys.Left)) {
					prevground = TileMap.GetForegroundAtCell (cursor);
					if (cursor.X > 0)
						--cursor.X;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl))
						TileMap.SetForegroundAtCell (cursor, prevground);
				} else if (keyState.IsKeyDown (Keys.Right) && lastState.IsKeyUp (Keys.Right)) {
					prevground = TileMap.GetForegroundAtCell (cursor);
					if (cursor.X < TileMap.MapWidth - 1)
						++cursor.X;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl))
						TileMap.SetForegroundAtCell (cursor, prevground);
				} else if (keyState.IsKeyDown (Keys.PageUp) && lastState.IsKeyUp (Keys.PageUp)) {
					TileMap.IncrementForegroundAtCell (cursor);
				} else if (keyState.IsKeyDown (Keys.PageDown) && lastState.IsKeyUp (Keys.PageDown)) {
					TileMap.DecrementForegroundAtCell (cursor);
				} else if (keyState.IsKeyDown (Keys.Delete) && lastState.IsKeyUp (Keys.Delete)) {
					TileMap.SetForegroundAtCell (cursor, 0);
				} else if (keyState.IsKeyDown (Keys.Z) && lastState.IsKeyUp (Keys.Z)) {
					editorState = EditorState.Background;

				} else if (keyState.IsKeyDown (Keys.X) && lastState.IsKeyUp (Keys.X)) {
					editorState = EditorState.Clean;
				}else if (keyState.IsKeyDown (Keys.Escape) && lastState.IsKeyUp (Keys.Escape)) {
					editorState = EditorState.Options;
				}
				repositionCamera ();
				break;
			//case EditorState.Map:
			case EditorState.Clean:
			case EditorState.Drugged:
				if (keyState.IsKeyDown (Keys.Up) && lastState.IsKeyUp (Keys.Up)) {
					prevtile = TileMap.GetMapSquareAtCell (cursor);//.GetMapSquareAtPixel (new Vector2 (cursor.X, cursor.Y));
					if (cursor.Y > 0)
						--cursor.Y;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl)) {
						TileMap.SetMapSquareAtCell (cursor, prevtile);
					}
				} else if (keyState.IsKeyDown (Keys.Down) && lastState.IsKeyUp (Keys.Down)) {
					prevtile = TileMap.GetMapSquareAtCell (cursor);
					if (cursor.Y < TileMap.MapHeight -1)
						++cursor.Y;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl)) {
						TileMap.SetMapSquareAtCell (cursor, prevtile);
					}
				} else if (keyState.IsKeyDown (Keys.Left) && lastState.IsKeyUp (Keys.Left)) {
					prevtile = TileMap.GetMapSquareAtCell (cursor);
					if (cursor.X > 0)
						--cursor.X;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl)) {
						TileMap.SetMapSquareAtCell (cursor, prevtile);
					}
				} else if (keyState.IsKeyDown (Keys.Right) && lastState.IsKeyUp (Keys.Right)) {
					prevtile = TileMap.GetMapSquareAtCell (cursor);
					if (cursor.X < TileMap.MapWidth -1)
						++cursor.X;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl)) {
						TileMap.SetMapSquareAtCell (cursor, prevtile);
					}
				} else if (keyState.IsKeyDown (Keys.PageUp) && lastState.IsKeyUp (Keys.PageUp)) {
						++TileMap.GetMapSquareAtCell (cursor, false)[(int)editorState-1, 2];
				} else if (keyState.IsKeyDown (Keys.PageDown) && lastState.IsKeyUp (Keys.PageDown)) {
					--TileMap.GetMapSquareAtCell (cursor, false)[(int)editorState-1, 2];
				} else if (keyState.IsKeyDown (Keys.Enter) && lastState.IsKeyUp (Keys.Enter)) {
					editorState = EditorState.CellCode;
					currentString = TileMap.GetMapSquareAtCell (cursor,false).CodeValue;
					TileMap.GetMapSquareAtCell (cursor).CodeValue = "";
					//currentLayer = -1;
				}else if (keyState.IsKeyDown (Keys.Space) && lastState.IsKeyUp (Keys.Space)) {
					TileMap.GetMapSquareAtCell (cursor,false).TogglePassable ((int)editorState-1);
				}else if (keyState.IsKeyDown (Keys.Delete) && lastState.IsKeyUp (Keys.Delete)) {
					TileMap.SetMapSquareAtCell (cursor, null);
				}else if (keyState.IsKeyDown (Keys.Escape) && lastState.IsKeyUp (Keys.Escape)) {
					editorState = EditorState.Options;
				}else if (keyState.IsKeyDown (Keys.Z) && lastState.IsKeyUp (Keys.Z)) {
					editorState = EditorState.Background;
				}else if (keyState.IsKeyDown (Keys.C) && lastState.IsKeyUp (Keys.C)) {
					editorState = EditorState.Foreground;
				}else if (keyState.IsKeyDown (Keys.A) && lastState.IsKeyUp (Keys.A)) {
					editorState = EditorState.Both;
				}else if (keyState.IsKeyDown (Keys.S) && lastState.IsKeyUp (Keys.S)) {
					editorState = EditorState.Clean;
				}else if (keyState.IsKeyDown (Keys.D) && lastState.IsKeyUp (Keys.D)) {
					editorState = EditorState.Drugged;
				}else if( keyState.IsKeyDown (Keys.D1) && lastState.IsKeyUp(Keys.D1)){
					TileMap.SetMapSquareAtCell (cursor, predefined [0]);
				}else if( keyState.IsKeyDown (Keys.D2) && lastState.IsKeyUp(Keys.D2)){
					TileMap.SetMapSquareAtCell (cursor, predefined [1]);
				}else if( keyState.IsKeyDown (Keys.D3) && lastState.IsKeyUp(Keys.D3)){
					TileMap.SetMapSquareAtCell (cursor, predefined [2]);
				}
				repositionCamera ();
				break;
			case EditorState.Both:
				if (keyState.IsKeyDown (Keys.Up) && lastState.IsKeyUp (Keys.Up)) {
					prevtile = TileMap.GetMapSquareAtCell (cursor);//.GetMapSquareAtPixel (new Vector2 (cursor.X, cursor.Y));
					if (cursor.Y > 0)
						--cursor.Y;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl)) {
						TileMap.SetMapSquareAtCell (cursor, prevtile);
					}
				} else if (keyState.IsKeyDown (Keys.Down) && lastState.IsKeyUp (Keys.Down)) {
					prevtile = TileMap.GetMapSquareAtCell (cursor);
					if (cursor.Y < TileMap.MapHeight -1)
						++cursor.Y;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl)) {
						TileMap.SetMapSquareAtCell (cursor, prevtile);
					}
				} else if (keyState.IsKeyDown (Keys.Left) && lastState.IsKeyUp (Keys.Left)) {
					prevtile = TileMap.GetMapSquareAtCell (cursor);
					if (cursor.X > 0)
						--cursor.X;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl)) {
						TileMap.SetMapSquareAtCell (cursor, prevtile);
					}
				} else if (keyState.IsKeyDown (Keys.Right) && lastState.IsKeyUp (Keys.Right)) {
					prevtile = TileMap.GetMapSquareAtCell (cursor);
					if (cursor.X < TileMap.MapWidth -1)
						++cursor.X;
					if (keyState.IsKeyDown (Keys.LeftControl) || keyState.IsKeyDown (Keys.RightControl)) {
						TileMap.SetMapSquareAtCell (cursor, prevtile);
					}
				} else if (keyState.IsKeyDown (Keys.PageUp) && lastState.IsKeyUp (Keys.PageUp)) {
					++TileMap.GetMapSquareAtCell (cursor,false)[0, 1];
				} else if (keyState.IsKeyDown (Keys.PageDown) && lastState.IsKeyUp (Keys.PageDown)) {
					--TileMap.GetMapSquareAtCell (cursor,false)[0, 1];
				} else if (keyState.IsKeyDown (Keys.Enter) && lastState.IsKeyUp (Keys.Enter)) {
					editorState = EditorState.CellCode;
					currentString = TileMap.GetMapSquareAtCell (cursor,false).CodeValue;
					TileMap.GetMapSquareAtCell (cursor).CodeValue = "";
					//currentLayer = -1;
				}else if (keyState.IsKeyDown (Keys.Space) && lastState.IsKeyUp (Keys.Space)) {
					TileMap.GetMapSquareAtCell (cursor,false).TogglePassable (2);
				}else if (keyState.IsKeyDown (Keys.Delete) && lastState.IsKeyUp (Keys.Delete)) {
					TileMap.SetMapSquareAtCell (cursor, null);
				}else if (keyState.IsKeyDown (Keys.Escape) && lastState.IsKeyUp (Keys.Escape)) {
					editorState = EditorState.Options;
				}else if (keyState.IsKeyDown (Keys.Z) && lastState.IsKeyUp (Keys.Z)) {
					editorState = EditorState.Background;
				}else if (keyState.IsKeyDown (Keys.C) && lastState.IsKeyUp (Keys.C)) {
					editorState = EditorState.Foreground;
				}else if (keyState.IsKeyDown (Keys.S) && lastState.IsKeyUp (Keys.S)) {
					editorState = EditorState.Clean;
				}else if (keyState.IsKeyDown (Keys.D) && lastState.IsKeyUp (Keys.D)) {
					editorState = EditorState.Drugged;
				}else if( keyState.IsKeyDown (Keys.D1) && lastState.IsKeyUp(Keys.D1)){
					TileMap.SetMapSquareAtCell (cursor, predefined [0]);
				}else if( keyState.IsKeyDown (Keys.D2) && lastState.IsKeyUp(Keys.D2)){
					TileMap.SetMapSquareAtCell (cursor, predefined [1]);
				}else if( keyState.IsKeyDown (Keys.D3) && lastState.IsKeyUp(Keys.D3)){
					TileMap.SetMapSquareAtCell (cursor, predefined [2]);
				}
				repositionCamera ();
				break;
			case EditorState.CellCode:
				if(keyState.IsKeyDown (Keys.Enter) && lastState.IsKeyUp (Keys.Enter)){
					editorState = EditorState.Clean;
					TileMap.GetMapSquareAtCell (cursor).CodeValue = currentString;
				}if(keyState.IsKeyDown (Keys.Back) && lastState.IsKeyUp (Keys.Back)){
					if(currentString.Length > 0) currentString = currentString.Substring (0, currentString.Length - 1);
					break;
				}
				foreach (Keys key in keyState.GetPressedKeys ()){
					bool repeated = false;
					foreach (Keys kay in lastState.GetPressedKeys ()){
						if (key == kay){
							repeated = true;
							break;
						}
					}
					if (!repeated && key.ToString ().Length == 1)
						currentString += key.ToString ();
				}
				break;
			case EditorState.Options:
				if (keyState.IsKeyDown (Keys.Escape) && lastState.IsKeyUp (Keys.Escape)) {
					editorState = EditorState.Clean;
				} else if (keyState.IsKeyDown (Keys.Enter) && lastState.IsKeyUp (Keys.Enter)) {
					switch (currentOption) {
					case 0:
						LoadMap (currentMap);
						editorState = EditorState.Clean;
						break;
					case 1:
						SaveMap (currentMap);
						editorState = EditorState.Clean;
						break;
					case 2:
						Exit ();
						break;
					}
				} else if (keyState.IsKeyDown (Keys.Down) && lastState.IsKeyUp (Keys.Down)) {
					if (currentOption < 2)
						++currentOption;
				} else if (keyState.IsKeyDown (Keys.Up) && lastState.IsKeyUp (Keys.Up)) {
					if (currentOption > 0)
						--currentOption;
				} else if (keyState.IsKeyDown (Keys.PageUp) && lastState.IsKeyUp (Keys.PageUp)) {
					if (currentMap < 999)
						++currentMap;
					else
						currentMap = 0;
				} else if (keyState.IsKeyDown (Keys.PageDown) && lastState.IsKeyUp (Keys.PageDown)) {
					if (currentMap > 0)
						--currentMap;
					else
						currentMap = 999;
				}
				break;
			}
            // TODO: Add your update logic here
			lastState = keyState;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
           	graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
		
			spriteBatch.Begin(
				SpriteSortMode.Immediate,
				BlendState.AlphaBlend);
			TileMap.DrawEditMode (spriteBatch, (int)editorState);// > 3 ? 1 : (int)editorState);

			if (editorState == EditorState.Both ||
			    editorState == EditorState.Clean ||
			    editorState == EditorState.Drugged ||
				editorState == EditorState.Background ||
				editorState == EditorState.Foreground) {
				spriteBatch.Draw (
					rect,
					Camera.WorldToScreen (CursorRectangle, TileMap.TileSize),//cursor,
					Color.White* 0.4f);
				font.DrawText (spriteBatch, 600, 0, layer [(int)editorState]);
				//font.DrawText (spriteBatch, new Vector2 (0, 0), "Camera.Position: " + Camera.Position.ToString ());
				//font.DrawText (spriteBatch, new Vector2 (0, 20), "CursorPosition: " + CursorPosition.ToString ());
			}

			if(editorState == EditorState.CellCode){

				font.DrawText (spriteBatch, Camera.WorldToScreen (CursorPosition), currentString);
				font.DrawText (spriteBatch, 600, 0, "CODEVALUE");
			}


			if(editorState == EditorState.Options){

				font.DrawText (spriteBatch, optionsPosition - Vector2.UnitY*50, "MAP" +currentMap.ToString().PadLeft(3, '0'));
				font.DrawText (spriteBatch, optionsPosition, options[0]);
				font.DrawText (spriteBatch, optionsPosition + Vector2.UnitY*20, options[1]);
				font.DrawText (spriteBatch, optionsPosition + Vector2.UnitY*40, options[2]);
				font.DrawText (spriteBatch, optionsPosition + Vector2.UnitY*(20*currentOption) - Vector2.UnitX*10, ">");
			}

			spriteBatch.End ();
            base.Draw(gameTime);
        }

		private void repositionCamera()
		{
			Vector2 screenLoc= Camera.WorldToScreen(CursorPosition, TileMap.TileSize);

			if (screenLoc.X > 480)
			{
				Camera.Move(new Vector2(screenLoc.X - 480, 0));
			}

			if (screenLoc.X < 240)
			{
				Camera.Move(new Vector2(screenLoc.X - 240, 0));
			}

			if (screenLoc.Y > 320)
			{
				Camera.Move(new Vector2(0, screenLoc.Y - 320));
			}

			if (screenLoc.Y < 160/*< 200*/)
			{
				Camera.Move(new Vector2(0, screenLoc.Y - 160));
			}
		}

		public void LoadMap(int levelNumber){
			try{
			TileMap.LoadMap((System.IO.FileStream)TitleContainer.OpenStream(
				@"LevelEditorContent/Maps/MAP"+levelNumber.ToString().PadLeft(3, '0') + ".MAP"));
			}catch{
				TileMap.ClearMap ();
			}
		}

		public void SaveMap(int levelNumber){
			TileMap.SaveMap (new FileStream (@"LevelEditorContent/Maps/MAP" + levelNumber.ToString ().PadLeft (3, '0') + ".MAP",
			                FileMode.Create));
		}
    }
}

