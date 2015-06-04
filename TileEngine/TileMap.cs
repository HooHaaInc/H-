using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Utils;

namespace TileEngine
{
	public static class TileMap
	{
		#region Declarations
		public const int TileSize = 32;
		public const int MapWidth = 160;
		public const int MapHeight = 24;
		public const int MapLayers = 3;
		public const int defaultBackground = 16;
		static private MapSquare[,] mapCells = new MapSquare[MapWidth, MapHeight];
		static private int[,] background;
		static private int[,] foreground;
		public static BitFont spriteFont;
		static private Texture2D tileSheet;
		static private int drugoidal = 0;
		static public bool OnDrugs = false;
		#endregion

		public static void Main(string[] args){

		}

		#region Initialization

		static public void Initialize(Texture2D tileTexture, int width, int height){
			tileSheet = tileTexture;
			background = new int[MapWidth+width/TileSize, MapHeight + height/TileSize];
			foreground = new int[MapWidth-width/(TileSize*3), MapHeight - height/(TileSize*3)];

			ClearMap ();

		}
		#endregion

		#region Simple TileEngining

		/// <summary>
		/// 	Los Size Layers son background, mapsquare y foreground (las de tamaño distinto)
		/// 	[background, mapsquare, foreground]
		/// </summary>
		/// <returns>The size layer.</returns>
		/// <param name="allLayers">
		/// 	All layers son las size layers mas el drugged y clean del mapsquare 
		/// 	[background, clean, drugged, foreground]
		/// </param>
		static public int GetSizeLayer(int allLayers){
			if (allLayers >= 4)
				return 1;
			return (allLayers + 1) / 2;
		}

		static public int LayerTileSize(int sizelayer){
			if (sizelayer > 4)
				return 1;
			return TileSize / 2 * (sizelayer + 1);
		}

		static public int GetColumnByPixelX(int pixelX, int sizelayer = 1){
			return (int)MathHelper.Clamp (
				pixelX / LayerTileSize (sizelayer), 
				0, 
				(sizelayer==1? MapWidth : sizelayer==0 ? BackgroundWidth : ForegroundWidth)-1);
		}

		static public int GetRowByPixelY(int pixelY, int sizelayer = 1){
			return (int)MathHelper.Clamp (
				pixelY / LayerTileSize (sizelayer),
				0,
				(sizelayer == 1 ? MapHeight : sizelayer==0 ? BackgroundHeight : ForegroundHeight)-1);
		}

		static public Vector2 GetCellByPixel(Vector2 pixelLocation, int sizelayer = 1){
			return new Vector2 (
				GetColumnByPixelX ((int)pixelLocation.X, sizelayer),
			    GetRowByPixelY ((int)pixelLocation.Y, sizelayer));
		}

		/// <summary>
		/// Gets the cell by pixel between layers.
		/// </summary>
		/// <returns>The cell by pixel between layers.</returns>
		/// <param name="pixelLocation">Pixel location.</param>
		/// <param name="frm">From. SizeLayer</param>
		/// <param name="to">To. SizeLayer</param>
		static public Vector2 GetCellByPixelBetweenLayers(Vector2 pixelLocation, int frm, int to){
			Vector2 fromCamera = Camera.WorldToScreen (pixelLocation, frm);
			return new Vector2 (
				GetColumnByPixelX ((int)(Camera.Position.X/2*(to+1) + fromCamera.X), to),
				GetRowByPixelY ((int)(Camera.Position.Y/2*(to+1) + fromCamera.Y), to));
		}

		static public Vector2 GetCellCenter(int squareX, int squareY, int sizelayer = 1){
			return new Vector2 (
				(int)(LayerTileSize (sizelayer)*(squareX + 0.5f)),
				(int)(LayerTileSize (sizelayer)*(squareY + 0.5f)));
		}

		static public Vector2 GetCellCenter(Vector2 square, int sizelayer = 1){
			return GetCellCenter (
				(int)square.X,
				(int)square.Y, sizelayer);
		}

		static public Vector2 CellWorldPosition(int x, int y, int sizelayer =1){
			return new Vector2 (
				x * LayerTileSize (sizelayer),
				y * LayerTileSize (sizelayer));
		}

		static public Rectangle CellWorldRectangle(int x, int y, int sizelayer = 1){
			int dim = LayerTileSize (sizelayer);
			return new Rectangle (
				x * dim,
				y * dim,
				dim,
				dim);
		}

		static public Rectangle CellWorldRectangle(Vector2 square, int sizelayer = 1){
			return CellWorldRectangle (
				(int)square.X,
				(int)square.Y,
				sizelayer);
		}

		static public Vector2 CellScreenPosition(int x, int y, int sizelayer = 1){
			return Camera.WorldToScreen (CellWorldPosition (x, y, sizelayer), sizelayer);
		}

		static public Rectangle CellScreenRectangle(int x, int y, int sizelayer = 1){
			return Camera.WorldToScreen (CellWorldRectangle (x, y, sizelayer), sizelayer);
		}

		static public Rectangle CellScreenRectangle(Vector2 square, int sizelayer = 1){
			return CellScreenRectangle ((int)square.X, (int)square.Y, sizelayer);
		}

		// Mapsquares Info

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
				GetColumnByPixelX (pixelX, 1),
				GetRowByPixelY (pixelY, 1));
		}

		static public MapSquare GetMapSquareAtPixel(Vector2 pixelLocation){
			return GetMapSquareAtPixel (
				(int)pixelLocation.X,
				(int)pixelLocation.Y);
		}

		static public bool MapSquareIsPassableByPixel(Vector2 pixelLocation){
			return MapSquareIsPassable (
				GetColumnByPixelX ((int)pixelLocation.X, 1),
				GetRowByPixelY ((int)pixelLocation.Y, 1));
		}

		static public bool MapSquareIsPassableByPixel(int x, int y){
			return MapSquareIsPassable (
				GetColumnByPixelX (x, 1),
				GetRowByPixelY (y, 1));
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
			startX = GetColumnByPixelX ((int)Camera.Position.X/2, 0);
			endX = GetColumnByPixelX ((int)Camera.Position.X/2 + Camera.ViewPortWidth, 0);
			startY = GetRowByPixelY ((int)Camera.Position.Y/2, 0);
			endY = GetRowByPixelY ((int)Camera.Position.Y/2 + Camera.ViewPortHeight, 0);
			++drugoidal;
			for (int x = startX; x <= endX; x++){
				for (int y = startY; y <= endY; y++) {
					spriteBatch.Draw (
						tileSheet, 
						CellScreenRectangle (x, y, 0), 
						TileSourceRectangle (background [x, y]),
						OnDrugs ? new Color (drugoidal % 256, (drugoidal + 85) % 256, (drugoidal + 170) % 256) : Color.White
					); 
				}
			}
			startX = GetColumnByPixelX ((int)Camera.Position.X, 1);
			endX = GetColumnByPixelX ((int)Camera.Position.X + Camera.ViewPortWidth, 1);
			startY = GetRowByPixelY ((int)Camera.Position.Y, 1);
			endY = GetRowByPixelY ((int)Camera.Position.Y + Camera.ViewPortHeight, 1);
			for (int x = startX; x <= endX; x++)
				for (int y = startY; y <= endY; y++) {
					if (mapCells[x,y] == null || !mapCells[x,y].HasToDraw())
						continue;
					spriteBatch.Draw (
						tileSheet, 
						CellScreenRectangle (x, y, 1), 
						TileSourceRectangle (mapCells [x, y][OnDrugs?1:0]),
						Color.White
					);
				}
		}

		public static void End(SpriteBatch spriteBatch){
				int startX = GetColumnByPixelX ((int)Camera.Position.X/2*3, 2);
				int endX = GetColumnByPixelX ((int)Camera.Position.X/2*3 + Camera.ViewPortWidth, 2);
				int startY = GetRowByPixelY ((int)Camera.Position.Y/2*3, 2);
				int endY = GetRowByPixelY ((int)Camera.Position.Y/2*3 + Camera.ViewPortHeight, 2);
				for (int x = startX; x <= endX; x++)
					for (int y = startY; y <= endY; y++) {
					if (foreground[x,y] <=0 )
						continue;
					spriteBatch.Draw (
						tileSheet, 
						CellScreenRectangle (x, y, 2), 
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
			Map mapita = new Map (mapCells, background, foreground);
            formatter.Serialize(fileStream, mapita);
            fileStream.Close();
        }

        public static void LoadMap(FileStream fileStream)
        {
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                Map aux = (Map)formatter.Deserialize(fileStream);
                for (int x = 0; x < aux.mapCells.GetLength(0); x++)
                {
                    for (int y = 0; y < aux.mapCells.GetLength(1); y++)
                    {
                        mapCells[x, y] = aux.mapCells[x, y];
                    }
                }

				for (int x = 0; x < aux.background.GetLength(0); x++)
				{
					for (int y = 0; y < aux.background.GetLength(1); y++)
					{
						background[x, y] = aux.background[x, y];
					}
				}

				for (int x = 0; x < aux.foreground.GetLength(0); x++)
				{
					for (int y = 0; y < aux.foreground.GetLength(1); y++)
					{
						foreground[x, y] = aux.foreground[x, y];
					}
				}

                fileStream.Close();
            }
            catch
            {
                ClearMap();
            }
        }
        public static void ClearMap()
        {

			for (int x = 0; x < BackgroundWidth; x++)
				for (int y = 0; y < BackgroundHeight; y++)
					background [x, y] = defaultBackground;

			for (int x = 0; x < ForegroundWidth; x++)
				for (int y = 0; y < ForegroundHeight; y++)
					foreground[x, y] = 0;
        }
		#endregion

		#region Editing Mode Functions

		// Background Info

		public static int BackgroundWidth{
			get{ return background.GetLength (0); }
		}

		public static int BackgroundHeight{
			get{ return background.GetLength (1); }
		}

		static public int GetBackgroundAtCell(int tileX, int tileY){
			if (tileX >= 0 && tileX < background.GetLength (0) && tileY >= 0 && tileY < background.GetLength (1))
				return background [tileX, tileY];
			else
				return -1;
		}

		static public int GetBackgroundAtCell(Vector2 pos){
			return GetBackgroundAtCell ((int)pos.X, (int)pos.Y);
		}

		static public void SetBackgroundAtCell(int tileX, int tileY, int tile){
			if (tileX >= 0 && tileX < background.GetLength (0) && tileY >= 0 && tileY < background.GetLength (1))
				background [tileX, tileY] = tile;
		}

		static public void SetBackgroundAtCell(Vector2 pos, int tile){
			SetBackgroundAtCell ((int)pos.X, (int)pos.Y, tile);
		}

		static public void IncrementBackgroundAtCell(int tileX, int tileY){
			if (tileX >= 0 && tileX < background.GetLength (0) && tileY >= 0 && tileY < background.GetLength (1))
				++background [tileX, tileY];
		}

		static public void IncrementBackgroundAtCell(Vector2 pos){
			IncrementBackgroundAtCell ((int)pos.X, (int)pos.Y);
		}

		static public void DecrementBackgroundAtCell(int tileX, int tileY){
			if (tileX >= 0 && tileX < background.GetLength (0) && tileY >= 0 && tileY < background.GetLength (1))
				--background [tileX, tileY];
		}

		static public void DecrementBackgroundAtCell(Vector2 pos){
			DecrementBackgroundAtCell ((int)pos.X, (int)pos.Y);
		}

		static public int GetBackgroundAtPixel(int pixelX, int pixelY){
			return GetBackgroundAtCell (
				GetColumnByPixelX (pixelX, 0),
				GetRowByPixelY (pixelY, 0));
		}

		static public int GetBackgroundAtPixel(Vector2 pixelLocation){
			return GetBackgroundAtPixel (
				(int)pixelLocation.X,
				(int)pixelLocation.Y);
		}

		// Foreground Info

		public static int ForegroundWidth{
			get{ return foreground.GetLength (0); }
		}

		public static int ForegroundHeight{
			get{ return foreground.GetLength (1); }
		}

		static public int GetForegroundAtCell(int tileX, int tileY){
			if (tileX >= 0 && tileX < foreground.GetLength (0) && tileY >= 0 && tileY < foreground.GetLength (1))
				return foreground [tileX, tileY];
			else
				return -1;
		}

		static public int GetForegroundAtCell(Vector2 pos){
			return GetForegroundAtCell ((int)pos.X, (int)pos.Y);
		}

		static public void SetForegroundAtCell(int tileX, int tileY, int tile){
			if (tileX >= 0 && tileX < foreground.GetLength (0) && tileY >= 0 && tileY < foreground.GetLength (1))
				foreground [tileX, tileY] = tile;
		}

		static public void SetForegroundAtCell(Vector2 pos, int tile){
			SetForegroundAtCell ((int)pos.X, (int)pos.Y, tile);
		}

		static public void IncrementForegroundAtCell(int tileX, int tileY){
			if (tileX >= 0 && tileX < foreground.GetLength (0) && tileY >= 0 && tileY < foreground.GetLength (1))
				++foreground [tileX, tileY];
		}

		static public void IncrementForegroundAtCell(Vector2 pos){
			IncrementForegroundAtCell ((int)pos.X, (int)pos.Y);
		}

		static public void DecrementForegroundAtCell(int tileX, int tileY){
			if (tileX >= 0 && tileX < foreground.GetLength (0) && tileY >= 0 && tileY < foreground.GetLength (1))
				--foreground [tileX, tileY];
		}

		static public void DecrementForegroundAtCell(Vector2 pos){
			DecrementForegroundAtCell ((int)pos.X, (int)pos.Y);
		}

		static public int GetForegroundAtPixel(int pixelX, int pixelY){
			return GetForegroundAtCell (
				GetColumnByPixelX (pixelX, 2),
				GetRowByPixelY (pixelY, 2));
		}

		static public int GetForegroundAtPixel(Vector2 pixelLocation){
			return GetForegroundAtPixel (
				(int)pixelLocation.X,
				(int)pixelLocation.Y);
		}

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
			startX = GetColumnByPixelX ((int)Camera.Position.X/2, 0);
			endX = GetColumnByPixelX ((int)Camera.Position.X/2 + Camera.ViewPortWidth, 0);
			startY = GetRowByPixelY ((int)Camera.Position.Y/2, 0);
			endY = GetRowByPixelY ((int)Camera.Position.Y/2 + Camera.ViewPortHeight, 0);
			for (int x = startX; x <= endX; x++)
				for (int y = startY; y <= endY; y++) {
				if (x < 0 || y < 0 || x >= background.GetLength (0) || y >= background.GetLength (1))
					continue;
				spriteBatch.Draw (
					tileSheet, 
					CellScreenRectangle (x, y, 0), 
					TileSourceRectangle (background[x, y]),
					Color.White
					); 
			}
			startX = GetColumnByPixelX ((int)Camera.Position.X, 1);
			endX = GetColumnByPixelX ((int)Camera.Position.X + Camera.ViewPortWidth, 1);
			startY = GetRowByPixelY ((int)Camera.Position.Y, 1);
			endY = GetRowByPixelY ((int)Camera.Position.Y + Camera.ViewPortHeight, 1);
			for (int x = startX; x <= endX; x++)
				for (int y = startY; y <= endY; y++) {
					if (x < 0 || y < 0 || x >= MapWidth || y >= MapHeight || mapCells[x,y] == null)
						continue;
					if (mapCells [x, y].IsActiveOnEditor (currentState)) {
						spriteBatch.Draw (
							tileSheet, 
							CellScreenRectangle (x, y, 1), 
							TileSourceRectangle (mapCells [x, y] [currentState >= 3 ? 0 : currentState - 1]),
							Color.White
						);
					}
					DrawEditModeItems (spriteBatch, x, y);
				}
			startX = GetColumnByPixelX ((int)Camera.Position.X/2*3, 2);
			endX = GetColumnByPixelX ((int)Camera.Position.X/2*3 + Camera.ViewPortWidth, 2);
			startY = GetRowByPixelY ((int)Camera.Position.Y/2*3, 2);
			endY = GetRowByPixelY ((int)Camera.Position.Y/2*3 + Camera.ViewPortHeight, 2);
			for (int x = startX; x <= endX; x++)
				for (int y = startY; y <= endY; y++) {
				if (x < 0 || y < 0 || x >= foreground.GetLength (0) || y >= foreground.GetLength (1) || foreground[x,y]==0)
					continue;
				spriteBatch.Draw (
					tileSheet, 
					CellScreenRectangle (x, y, 2), 
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
}

