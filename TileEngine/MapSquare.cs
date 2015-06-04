using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TileEngine
{
	[Serializable]
	public class MapSquare
	{

		/// <summary>
		/// xor (^) toggle
		/// or (|) on
		/// and (&) off
		/// </summary>
		public static readonly int NORMAL_PASSABLE = 1;
		public static readonly int DRUGGED_PASSABLE = 2;

		#region Declarations
		/// <summary>
		/// The layer tiles.
		/// if null, never has a tile.
		/// if length == 1, always use [0].
		/// if length == 2, [clean, drugged]
		/// </summary>
		private int[] layerTiles = null;
		private string codeValue = "";
		private int passable = 3;
		#endregion

		#region Constructors
		public MapSquare(){}

		public MapSquare (
			int clean,
			int drugged,
			string code,
			int pass)
		{
			layerTiles = new int[2];
			layerTiles [0] = clean;
			layerTiles [1] = drugged;
			codeValue = code;
			passable = pass;
		}

		public MapSquare(int interactive, string code, int pass){
			layerTiles = new int[1];
			layerTiles [0] = interactive;
			codeValue = code;
			passable = pass;
		}
		#endregion

		#region Properties
		public string CodeValue{
			get { return codeValue; }
			set { codeValue = value; }
		}

		public int Passable{
			get{ return passable; }
		}

		/// <summary>
		/// Gets the tile in the specified layer i, [0] if i is out of range, or -1 if null.
		/// Sets the tile in the specified layer i (0,1), if i is pass range, recreates with new length.  
		/// </summary>
		/// <param name="i">The index.</param>
		public int this[int i]{
			get{
				if (layerTiles == null)
					return -1;
				if (i + 1 > layerTiles.Length || i < 0)
					return layerTiles [0];
				return layerTiles [i];
			}

			set{
				if (i > 1 || i<0)
					return;
				if (layerTiles == null || i+1 > layerTiles.Length) {
					layerTiles = new int[i+1];
				}
				layerTiles [i] = value;
			}
		}

		/// <summary>
		/// Gets the layer with the specified i layer.
		/// Sets the layer with the specified i layer (0,1), with forced l length (0,1).
		/// </summary>
		/// <param name="i">The index.</param>
		/// <param name="l">Forced length</param>
		public int this[int i, int l]{
			set{
				if (l > 2 || l<1 || i<0 || i>1)
					return;
				if (layerTiles == null || l != layerTiles.Length) {
					layerTiles = new int[l];
				}
				layerTiles [i] = value;
			}
			get{ 
				if(layerTiles == null || layerTiles.Length != l)
					return -1;
				return layerTiles[i]; 
			}
		}
		#endregion

		#region Public Methods
		public void TogglePassable(int maplayer){
			if (maplayer > 3 || maplayer < 0)
				return;
			passable ^= maplayer+1;
		}

		/// <summary>
		/// Determines whether this instance is active on editor in the specified currentState.
		/// </summary>
		/// <returns><c>true</c> if this instance is active on editor in the specified currentState; otherwise, <c>false</c>.</returns>
		/// <param name="currentState">
		/// Current state. { background = 0, clean, drugged, foreground, both, codevalue, options }
		/// </param>
		public bool IsActiveOnEditor(int currentState){
			if (currentState == 4)
				return (layerTiles != null && layerTiles.Length == 1 ? layerTiles [0] : -1) > 0;
			if (currentState < 1 || currentState > 2)
				return false;
			return (layerTiles != null && layerTiles.Length == 2 ? layerTiles[currentState-1] : -1) > 0;
		}

		public bool IsPassable(){
			return ((TileMap.OnDrugs ? 2: 1) & Passable) != 0;
		}

		public bool IsPassable(int maplayer){
			return ((maplayer+1) & Passable) != 0;

		}

		public bool HasToDraw(){
			return this[TileMap.OnDrugs?1:0] > 0;
		}

		public void Copy(MapSquare a){
			if(a.layerTiles != null){
				layerTiles = new int[a.layerTiles.Length];
				for (int i=0; i<a.layerTiles.Length; ++i)
					layerTiles [i] = a.layerTiles [i];
			}
			codeValue = a.codeValue;
			passable = a.passable;
		}

		public bool IsEmpty(){
			if (codeValue != null && codeValue != "")
				return false;
			if (passable != 3)
				return false;
			if (layerTiles == null)
				return true;
			for (int i=0; i<layerTiles.Length; ++i)
				if (layerTiles [i] > 0)
					return false;
			return true;
		}
		#endregion
	}
}

