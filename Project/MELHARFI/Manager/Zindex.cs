using MELHARFI.Manager.Gfx;
using System.Collections.Generic;

namespace MELHARFI.Manager
{
    /// <summary>
    /// classe IComparer pour classer les objets
    /// </summary>
    public class Zindex : IComparer<IGfx>
    {
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
                tmpX = b.Zindex;
            }
            else if (x.GetType() == typeof(Anim))
            {
                Anim a = x as Anim;
                tmpX = a.Bmp.Zindex;
            }
            else if (x.GetType() == typeof(Txt))
            {
                Txt t = x as Txt;
                tmpX = t.Zindex;
            }
            else if (x.GetType() == typeof(FillPolygon))
            {
                FillPolygon f = x as FillPolygon;
                tmpX = f.Zindex;
            }
            else
            {
                // Rec
                Rec r = x as Rec;
                tmpX = r.Zindex;
            }

            if (y.GetType() == typeof(Bmp))
            {
                Bmp b = y as Bmp;
                tmpY = b.Zindex;
            }
            else if (y.GetType() == typeof(Anim))
            {
                Anim a = y as Anim;
                tmpY = a.Bmp.Zindex;
            }
            else if (y.GetType() == typeof(Txt))
            {
                Txt t = y as Txt;
                tmpY = t.Zindex;
            }
            else if (y.GetType() == typeof(FillPolygon))
            {
                // Rec
                FillPolygon f = y as FillPolygon;
                tmpY = f.Zindex;
            }
            else
            {
                // Rec
                Rec r = y as Rec;
                tmpY = r.Zindex;
            }

            if (tmpX > tmpY)
                return 1;
            if (tmpX < tmpY)
                return -1;
            return 0;
        }
    }
}
