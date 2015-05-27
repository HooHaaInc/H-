using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileEngine
{
	[Serializable]
	public class MapSquare
	{
		#region Declarations
		public const int ALWAYS_PASSABLE = 0;
		public const int NORMAL_PASSABLE = 1;
		public const int DRUGGED_PASSABLE = 2;
		public const int NEVER_PASSABLE = 3;

		public int[] LayerTiles = new int[3];
		public string CodeValue = "";
		public int Passable = 0;
		#endregion

		public static MapSquare Neutral{
			get{ return new MapSquare (
					0,
					0,
					0,
					"",
					0);
			}
		}

		

		#region Constructor
		public MapSquare (
			int interactive,
			int clean,
			int drugged,
			string code,
			int passable)
		{
			LayerTiles [0] = interactive;
			LayerTiles [1] = clean;
			LayerTiles [2] = drugged;
			CodeValue = code;
			Passable = passable;
		}
		#endregion

		#region Public Methods
		public void TogglePassable(){
			++Passable;
			if (Passable == 4)
				Passable = 0;
		}

		public void Copy(MapSquare mapSquare){
			LayerTiles [0] = mapSquare.LayerTiles[0];
			LayerTiles [1] = mapSquare.LayerTiles[1];
			LayerTiles [2] = mapSquare.LayerTiles[2];
			CodeValue = mapSquare.CodeValue;
			Passable = mapSquare.Passable;
		}

		public int EditMode(int mapLayer){
			if (mapLayer>=0 && mapLayer<3 && LayerTiles[mapLayer] != 0)
				return LayerTiles [mapLayer];
			else return LayerTiles[0];
		}

		public bool ActiveOnEditor(int allLayers){
			if (allLayers == 0 || allLayers == 4)
				return false;
			if (allLayers == 1)
				return true;
			return LayerTiles [allLayers - 1] != 0;
		}

		public bool IsPassable(){
			/*switch(Passable){
				case 0:
				return true;
				///break;
				case 1:
				return !TileMap.OnDrugs;
				//break;
				case 2:
				return TileMap.OnDrugs;
				//break;
			}
			return false;*/
			return ((TileMap.OnDrugs ? 1: 2) & Passable) == 0;
		}

		public bool IsPassable(int maplayer){
			switch(Passable){
			case 0:
				return true;
				///break;
			case 1:
				return maplayer != 2;
				//break;
			case 2:
				return maplayer != 1;
				//break;
			}
			return false;

		}

		public int ToDraw(){
			int i = TileMap.OnDrugs ? 2 : 1;
			if (LayerTiles [i] != 0)
				return LayerTiles [i];
			else
				return LayerTiles [0];
		}
		#endregion
	}
}

