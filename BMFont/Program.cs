#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace HootileriHaa
{
    static class Program
    {
        static void Main()
        {
			RangeList<string, int> rist = new RangeList<string, int> ();
			rist.Add ("hoo", 0);
			rist.Add ("haa", 1);
			rist.Add ("inc", 2);
			RangeList<string, int>.RangeListIterator iterator = rist [0, 2];
			while(iterator.Next ())
				Console.Write (iterator.CurrentElement + "," + iterator.CurrentKey.ToString () + ";");
        }
    }
}
