using System.Collections.Generic;
using MELHARFI.Gfx;

namespace MELHARFI
{
    public class Zindex : IComparer<IGfx>
    {
        // classe IComparer pour classer les objets
        public int Compare(IGfx x, IGfx y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                return -1;
            }
            if (y == null)
            {
                return 1;
            }
            int tmpX, tmpY;

            if (x.GetType() == typeof(Bmp))
            {
                Bmp b = x as Bmp;
                tmpX = b.zindex;
            }
            else if (x.GetType() == typeof(Anim))
            {
                Anim a = x as Anim;
                tmpX = a.img.zindex;
            }
            else if (x.GetType() == typeof(Txt))
            {
                Txt t = x as Txt;
                tmpX = t.zindex;
            }
            else
            {
                // Rec
                Rec r = x as Rec;
                tmpX = r.zindex;
            }

            if (y.GetType() == typeof(Bmp))
            {
                Bmp b = y as Bmp;
                tmpY = b.zindex;
            }
            else if (y.GetType() == typeof(Anim))
            {
                Anim a = y as Anim;
                tmpY = a.img.zindex;
            }
            else if (y.GetType() == typeof(Txt))
            {
                Txt t = y as Txt;
                tmpY = t.zindex;
            }
            else
            {
                // Rec
                Rec r = y as Rec;
                tmpY = r.zindex;
            }

            if (tmpX > tmpY)
                return 1;
            if (tmpX < tmpY)
                return -1;
            return 0;
        }
    }
}
