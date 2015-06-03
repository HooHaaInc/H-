using System;

namespace TileEngine
{
	[Serializable]
	public class Map
	{
		public MapSquare[,] mapCells;
		public int[,] background;
		public int[,] foreground;

		public Map (MapSquare[,] mapC, int[,] backG, int[,] foreG)
		{
			mapCells = mapC;
			background = backG;
			foreground = foreG;
		}
	}
}

