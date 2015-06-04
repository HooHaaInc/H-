using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Utils
{
	public class BitFont
	{
		#region Declarations
		private FontFile fontFile;
		private Texture2D fontTexture;
		private Dictionary<char, FontChar> characterMap = new Dictionary<char, FontChar> ();
		#endregion

		#region Constructor
		public BitFont (string font_fnt, Texture2D texture)
		{
//			string fontFilePath = font_fnt;
//			//using (var stream = TitleContainer.OpenStream(fontFilePath)){
//			fontFile = FontLoader.Load (fontFilePath);
//			fontTexture = texture;
//
//			//	stream.Close ();
//			//}
			using(var stream = TitleContainer.OpenStream(font_fnt))
			{
				fontFile = FontLoader.Load(stream);
				// textRenderer initialization will go here
				stream.Close();
			}
			fontTexture = texture;

			foreach(var fontCharacter in fontFile.Chars){
				char c = (char)fontCharacter.ID;
				characterMap.Add (c, fontCharacter);
			}
		}
		#endregion

		#region Draw
		public void DrawText(SpriteBatch spriteBatch, int x, int y, string text){
			int dx = x, dy = y;
			foreach(char c in text){
				FontChar fc;
				if(characterMap.TryGetValue (c, out fc)){
					Rectangle sourceRectangle = new Rectangle (fc.X, fc.Y, fc.Width, fc.Height);
					Rectangle position = new Rectangle (dx + fc.XOffset, dy + fc.YOffset, fc.Width, fc.Height);
					spriteBatch.Draw (fontTexture, position, sourceRectangle, Color.White);
					dx += fc.XAdvance;
				}
			}
		}

		public void DrawText(SpriteBatch spriteBatch, Vector2 position, string text){
			DrawText (spriteBatch, (int)position.X, (int)position.Y, text);
		}
		#endregion
	}
}

