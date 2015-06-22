using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using HooHaaUtils;

namespace TileEngine
{
	public static class TileMap
	{
		#region Declarations
		public const int MAPVERSION = 0;
		public const int TileSize = 32;
		public static int MapWidth = 160;
		public static int MapHeight = 24;
		static private MapSquare[,] mapCells = new MapSquare[MapWidth, MapHeight];
		static private int backgroundTexture = 0;
		static private Texture2D[] backgrounds;
		static private int[,] foreground = new int[MapWidth, MapHeight];
		public static BitFont spriteFont;
		static private Texture2D tileSheet;
		static private int drugoidal = 0;
		static public bool OnDrugs = false;
		#endregion

		public static void Main(string[] args){

		}

		#region Initialization

		static public void Initialize(Texture2D[] backgrounds, Texture2D tileTexture){//, int width, int height){
			tileSheet = tileTexture;
			//background = new int[MapWidth+width/TileSize, MapHeight + height/TileSize];
			TileMap.backgrounds = backgrounds;
			//foreground = //new int[MapWidth-width/(TileSize*3), MapHeight - height/(TileSize*3)];

			ClearMap ();

		}
		#endregion

		//Simple TileEngining

		#region MapCells

		public class MapCell{

			private int x, y;

			internal MapCell(int x, int y){
				this.x = x;
				this.y = y;
			}

			public int X {
				get{ return x; }
			}

			public int Y{
				get { return y; }
			}

			public Vector2 Center { 
				get{
					return new Vector2 (
						(int)(TileMap.TileSize * (x + 0.5f)),
						(int)(TileMap.TileSize * (y + 0.5f)));
				}
			}

			public Vector2 CellWorldPosition {
				get {
					return new Vector2 (
						x * TileMap.TileSize,
						y * TileMap.TileSize);
				}
			}

			public Rectangle CellWorldRectangle{
				get {
					return new Rectangle (
						x * TileMap.TileSize,
						y * TileMap.TileSize,
						TileMap.TileSize,
						TileMap.TileSize);
				}
			}

			public Vector2 CellScreenPosition{
				get { return Camera.WorldToScreen (CellWorldPosition); }
			}

			public Rectangle CellScreenRectangle{ 
				get { return Camera.WorldToScreen (CellWorldRectangle); }
			}
		}

		public static MapCell GetCellByPixel(Vector2 pixelLocation){ 
			return new MapCell (
				TileMap.GetColumnByPixelX ((int)pixelLocation.X),
				TileMap.GetRowByPixelY ((int)pixelLocation.Y));
		}

		static public int GetColumnByPixelX(int pixelX){
			return (int)MathHelper.Clamp (
				pixelX / TileSize,
				0, 
				MapWidth-1);
		}

		static public int GetRowByPixelY(int pixelY){
			return (int)MathHelper.Clamp (
				pixelY / TileSize,
				0,
				MapHeight-1); 
		}



		#endregion

		#region Mapsquares Info



		static public string MapSquareCodeValue(int cellX, int cellY){
			MapSquare square = GetMapSquareAtCell (cellX, cellY);
			return square != null ? square.CodeValue : "";
		}

		static public string MapSquareCodeValue(Vector2 cell){
			return MapSquareCodeValue ((int)cell.X, (int)cell.Y);
		}

		static public bool MapSquareIsPassable(int cellX, int cellY){
			MapSquare square = GetMapSquareAtCell (cellX, cellY);
			return square == null || square.IsPassable ();
		}

		static public bool MapSquareIsPassable(Vector2 cell){
			return MapSquareIsPassable ((int)cell.X, (int)cell.Y);
		}

		static public MapSquare GetMapSquareAtCell(int tileX, int tileY, bool canBeNull = true){
			if (tileX >= 0 && tileX < MapWidth && tileY >= 0 && tileY < MapHeight) {
				if (!canBeNull && mapCells [tileX, tileY] == null)
					mapCells [tileX, tileY] = new MapSquare ();
				return mapCells [tileX, tileY];
			}else
				return null;
		}

		static public MapSquare GetMapSquareAtCell(Vector2 pos, bool canBeNull = true){
			return GetMapSquareAtCell ((int)pos.X, (int)pos.Y, canBeNull);
		}

		static public void SetMapSquareAtCell(int tileX, int tileY, MapSquare tile){
			if (tileX >= 0 && tileX < MapWidth && tileY >= 0 && tileY < MapHeight) {
				if(tile == null){
					mapCells [tileX, tileY] = null;
					return;
				}
				if (mapCells [tileX, tileY] == null)
					mapCells [tileX, tileY] = new MapSquare ();
				mapCells [tileX, tileY].Copy (tile);
			}
		}

		static public void SetMapSquareAtCell(Vector2 pos, MapSquare tile){
			SetMapSquareAtCell ((int)pos.X, (int)pos.Y, tile);
		}

		static public MapSquare GetMapSquareAtPixel(int pixelX, int pixelY){
			return GetMapSquareAtCell (
				GetColumnByPixelX (pixelX),
				GetRowByPixelY (pixelY));
		}

		static public MapSquare GetMapSquareAtPixel(Vector2 pixelLocation){
			return GetMapSquareAtPixel (
				(int)pixelLocation.X,
				(int)pixelLocation.Y);
		}

		static public bool MapSquareIsPassableByPixel(Vector2 pixelLocation){
			return MapSquareIsPassable (
				GetColumnByPixelX ((int)pixelLocation.X),
				GetRowByPixelY ((int)pixelLocation.Y));
		}

		static public bool MapSquareIsPassableByPixel(int x, int y){
			return MapSquareIsPassable (
				GetColumnByPixelX (x),
				GetRowByPixelY (y));
		}

		static public bool isRectanglePassable(Rectangle rect){
			int startX = GetColumnByPixelX (rect.X);
			int startY = GetRowByPixelY (rect.Y);
			int endX = GetColumnByPixelX (rect.X + rect.Width - 1);
			int endY = GetRowByPixelY (rect.Y + rect.Height - 1);
			for (int x = startX; x <= endX; ++x)
				for (int y = startY; y <= endY; ++y)
					if (!MapSquareIsPassable(x,y))
						return false;
			return true;
		}
		#endregion

		#region Tile and Tile Sheet Handling
		public static int TilesPerRow{
			get{ return tileSheet.Width / TileSize; }
		}

		public static Rectangle TileSourceRectangle(int tileIndex){
			return new Rectangle (
				(tileIndex % TilesPerRow) * TileSize,
				(tileIndex / TilesPerRow) * TileSize,
				TileSize,
				TileSize
			);
		}
		#endregion

		#region Drawing
		static public void Begin(SpriteBatch spriteBatch)//, bool drugged)
		{
			int startX, endX, startY, endY;

			spriteBatch.Draw (
				backgrounds[backgroundTexture],
				new Rectangle (0,0, Camera.ViewPortWidth, Camera.ViewPortHeight),
				Color.White);

			startX = GetColumnByPixelX ((int)Camera.Position.X);
			endX = GetColumnByPixelX ((int)Camera.Position.X + Camera.ViewPortWidth);
			startY = GetRowByPixelY ((int)Camera.Position.Y);
			endY = GetRowByPixelY ((int)Camera.Position.Y + Camera.ViewPortHeight);
			for (int x = startX; x <= endX; x++)
				for (int y = startY; y <= endY; y++) {
					if (mapCells[x,y] == null || !mapCells[x,y].HasToDraw())
						continue;
					spriteBatch.Draw (
						tileSheet, 
						CellScreenRectangle (x, y), 
						TileSourceRectangle (mapCells [x, y][OnDrugs?1:0]),
						Color.White
					);
				}
		}

		public static void End(SpriteBatch spriteBatch){
				int startX = GetColumnByPixelX ((int)Camera.Position.X);
				int endX = GetColumnByPixelX ((int)Camera.Position.X + Camera.ViewPortWidth);
				int startY = GetRowByPixelY ((int)Camera.Position.Y);
				int endY = GetRowByPixelY ((int)Camera.Position.Y + Camera.ViewPortHeight);
				for (int x = startX; x <= endX; x++)
					for (int y = startY; y <= endY; y++) {
					if (foreground[x,y] <=0 )
						continue;
					spriteBatch.Draw (
						tileSheet, 
						CellScreenRectangle (x, y), 
						TileSourceRectangle (foreground[x, y]),
						Color.White
						); 
				}
			}

		#endregion

		#region Loading and Saving Maps

        public static void SaveMap(FileStream fileStream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
			Map mapita = new Map (backgroundTexture, mapCells, foreground, MAPVERSION);
            formatter.Serialize(fileStream, mapita);
            fileStream.Close();
        }

        public static void LoadMap(FileStream fileStream)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Map aux = (Map)formatter.Deserialize(fileStream);
				if(aux.version != MAPVERSION){
					throw new MapIncompatible ();
				}
                for (int x = 0; x < aux.mapCells.GetLength(0); x++)
                {
                    for (int y = 0; y < aux.mapCells.GetLength(1); y++)
                    {
                        mapCells[x, y] = aux.mapCells[x, y];
                    }
                }

				backgroundTexture = aux.background;

				for (int x = 0; x < aux.foreground.GetLength(0); x++)
				{
					for (int y = 0; y < aux.foreground.GetLength(1); y++)
					{
						foreground[x, y] = aux.foreground[x, y];
					}
				}
            }
            catch
            {
                ClearMap();
				Console.Write ("ERROR: no se puedo read archivo .MAP");
            } finally{
				fileStream.Close ();
			}
        }
        public static void ClearMap()
        {
			backgroundTexture = 0;

			for (int x=0; x<MapWidth; ++x)
				for (int y=0; y<MapHeight; ++y)
					mapCells [x, y] = null;

			for (int x = 0; x < MapWidth; x++)
				for (int y = 0; y < MapHeight; y++)
					foreground[x, y] = 0;
        }
		#endregion

		//Editing Mode Functions

		#region Background Info

		public static int BackgroundWidth{
			get { return backgrounds[backgroundTexture].Width; }
		}

		public static int BackgroundHeight{
			get { return backgrounds[backgroundTexture].Height; }
		}

		public static Texture2D BackgroundTexture{
			get { return backgrounds[backgroundTexture]; }
		}

		public static int BackgroundIndex{
			get { return backgroundTexture; }
			set { backgroundTexture = value >= backgrounds.Length ? 0 : value < 0 ? backgrounds.Length - 1 : value; }
		}

		#endregion

		#region Foreground Info

		public class Foreground{
			int[,] f;

			internal Foreground(int[,] f){
				this.f = f;
			}

			public int this[int x, int y]{
				get{
					if (x >= 0 && x < MapWidth && y >= 0 && y < MapHeight)
						return f [x, y];
					else
						return -1;
				}
				set{
					if (x >= 0 && x < MapWidth && y >= 0 && y < MapHeight)
						foreground [x, y] = value;
				}
			}

			public int this[Vector2 tile]{
				get{ return this [(int)tile.X, (int)tile.Y]; }
				set{ this [(int)tile.X, (int)tile.Y] = value; }
			}

			static public int AtPixel(int pixelX, int pixelY){
				return this[
					TileMap.GetColumnByPixelX (pixelX),
					TileMap.GetRowByPixelY (pixelY)];
			}

			static public int AtPixel(Vector2 pixelLocation){
				return AtPixel (
					(int)pixelLocation.X,
					(int)pixelLocation.Y);
			}
		}

		#endregion

		#region Edit Mode Drawing

		// Drawing
		/// <summary>
		/// Draws the edit mode.
		/// </summary>
		/// <param name="spriteBatch">Sprite batch.</param>
		/// <param name="currentState">
		/// Current state { background=0, clean, drugged, foreground, both, codevalue, options }
		/// </param>
		static public void DrawEditMode(SpriteBatch spriteBatch, int currentState)//, bool drugged)
		{
			int startX, endX, startY, endY;
//			startX = (int)Camera.Position.X/2;
//			endX = (int)Camera.Position.X/2 + Camera.ViewPortWidth;
//			startY = (int)Camera.Position.Y/2;
//			endY = (int)Camera.Position.Y/2 + Camera.ViewPortHeight;
//			for(int x=0; x<BackgroundWidth/Camera.ViewPortWidth; ++x){
//				for(int y=0; y<BackgroundHeight/Camera.ViewPortHeight; ++y){
//					spriteBatch.Draw (
//						backgrounds[backgroundTexture],
//						new Vector2 (x * BackgroundWidth - startX, y * BackgroundHeight - startY),
//						Color.White);
//				}
//			}

			spriteBatch.Draw (
				backgrounds[backgroundTexture],
				new Rectangle (0,0, Camera.ViewPortWidth, Camera.ViewPortHeight),
				Color.White);

			startX = GetColumnByPixelX ((int)Camera.Position.X);
			endX = GetColumnByPixelX ((int)Camera.Position.X + Camera.ViewPortWidth);
			startY = GetRowByPixelY ((int)Camera.Position.Y);
			endY = GetRowByPixelY ((int)Camera.Position.Y + Camera.ViewPortHeight);
			for (int x = startX; x <= endX; x++)
				for (int y = startY; y <= endY; y++) {
					if (mapCells[x,y] == null)
						continue;
					if (mapCells [x, y].IsActiveOnEditor (currentState)) {
						spriteBatch.Draw (
							tileSheet, 
							CellScreenRectangle (x, y), 
							TileSourceRectangle (mapCells [x, y] [currentState >= 3 ? 0 : currentState - 1]),
							Color.White
						);
					}
					DrawEditModeItems (spriteBatch, x, y);
				}
			for (int x = startX; x <= endX; x++)
				for (int y = startY; y <= endY; y++) {
				if (foreground[x,y]==0)
					continue;
				spriteBatch.Draw (
					tileSheet, 
					CellScreenRectangle (x, y), 
					TileSourceRectangle (foreground[x, y]),
					Color.White*(currentState >= 4 ? 1.0f: 0.5f)
					); 
			}
			if (currentState > 4)
				spriteBatch.Draw (
					tileSheet,
					new Rectangle (0, 0, Camera.ViewPortWidth, Camera.ViewPortHeight),
					TileSourceRectangle (1),
					Color.Black * 0.5f);
		}

		public static void DrawEditModeItems(SpriteBatch spriteBatch, int x, int y){

			spriteBatch.Draw (
				tileSheet,
				CellScreenRectangle (x, y),
				TileSourceRectangle (1),
				new Color (
					mapCells[x,y].IsPassable (0) ? 0 : 255, 
					mapCells[x,y].IsPassable (1) ? 0 : 255, 0)*0.5f);
			if(mapCells[x, y].CodeValue.Length != 0){
				spriteFont.DrawText (
					spriteBatch, 
					CellScreenPosition (x,y), 
					mapCells [x, y].CodeValue);
			}
		}
		#endregion


	}

	public class MapIncompatible : System.ApplicationException {
		public MapIncompatible(string message = null, System.Exception inner = null): base(message, inner){
		}
		protected MapIncompatible(
			System.Runtime.Serialization.SerializationInfo info,
		    System.Runtime.Serialization.StreamingContext context):
		base(info, context){}
	}
}

