using System;
using System.Collections.Generic;
using System.Drawing;

namespace MELHARFI.Manager
{
    /// <summary>
    /// classe pour referencer les 4 coordonées des sprites réunis dans une image, X,Y,Width,Height
    /// </summary>
    public class SpriteSheet
    {
        struct SpriteSheetData
        {
            public string asset;
            public Int16 id;
            public Rectangle rectangle;
        }
        readonly List<SpriteSheetData> SpriteSheetPoint = new List<SpriteSheetData>();
        /// <summary>
        /// Method to store an instance of a sprite sheet
        /// </summary>
        /// <param name="Asset">asset is the name of the SpriteSheet</param>
        /// <param name="id">id is an int value as an identifier of the sequance, usefull if there's many instance of spriteSheet that share samename but the identifier should be different</param>
        /// <param name="_rectanle">_rectanle is a Rectangle object to pick only a part of the picture, usfull when dealing with a spritesheet</param>
        public void SetSpriteSheet(string Asset, Int16 id, Rectangle _rectanle)
        {
            SpriteSheetData ssd = new SpriteSheetData
            {
                asset = Asset,
                id = id,
                rectangle = _rectanle
            };
            SpriteSheetPoint.Add(ssd);
        }
        /// <summary>
        /// Method to return a rectangle value for the gived asset name
        /// </summary>
        /// <param name="AssetName">asset is the name of the SpriteSheet to look for</param>
        /// <param name="id">id is an int value as an identifier of the sequance, usefull if there's many instance of spriteSheet that share samename but the identifier should be different</param>
        /// <returns>Return a ractangle for the coordinate of a sequance</returns>
        public Rectangle GetSpriteSheet(string AssetName, int id)
        {
            Rectangle rec = Rectangle.Empty;
            SpriteSheetData ssd = SpriteSheetPoint.Find(f => f.asset == AssetName && f.id == id);
            rec = ssd.rectangle;
            return rec;
        }
    }
}
