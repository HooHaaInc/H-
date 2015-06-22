using System;

namespace TileEngine
{
	[Serializable]
	public class Map
	{
		public MapSquare[,] mapCells;
		public int background;
		public int[,] foreground;
		public int version;

		public Map (int backG, MapSquare[,] mapC, int[,] foreG, int version)
		{
			mapCells = mapC;
			background = backG;
			foreground = foreG;
		}
	}
}

