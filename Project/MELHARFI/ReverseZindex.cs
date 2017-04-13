using System;
using System.Collections.Generic;

namespace MELHARFI
{
    public class ReverseZindex : IComparer<IGfx>
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

            if (x.GetType() == typeof(MELHARFI.Manager.Bmp))
            {
                MELHARFI.Manager.Bmp b = x as MELHARFI.Manager.Bmp;
                tmpX = b.Zindex;
            }
            else if (x.GetType() == typeof(MELHARFI.Manager.Anim))
            {
                MELHARFI.Manager.Anim a = x as MELHARFI.Manager.Anim;
                tmpX = a.img.Zindex;
            }
            else if (x.GetType() == typeof(MELHARFI.Manager.Txt))
            {
                MELHARFI.Manager.Txt t = x as MELHARFI.Manager.Txt;
                tmpX = t.Zindex;
            }
            else if (x.GetType() == typeof(MELHARFI.Manager.FillPolygon))
            {
                MELHARFI.Manager.FillPolygon f = x as MELHARFI.Manager.FillPolygon;
                tmpX = f.Zindex;
            }
            else
            {
                throw new NotImplementedException("object not found");
            }

            if (y.GetType() == typeof(MELHARFI.Manager.Bmp))
            {
                MELHARFI.Manager.Bmp b = y as MELHARFI.Manager.Bmp;
                tmpY = b.Zindex;
            }
            else if (y.GetType() == typeof(MELHARFI.Manager.Anim))
            {
                MELHARFI.Manager.Anim a = y as MELHARFI.Manager.Anim;
                tmpY = a.img.Zindex;
            }
            else if (y.GetType() == typeof(MELHARFI.Manager.Txt))
            {
                MELHARFI.Manager.Txt t = y as MELHARFI.Manager.Txt;
                tmpY = t.Zindex;
            }
            else if (y.GetType() == typeof(MELHARFI.Manager.FillPolygon))
            {
                MELHARFI.Manager.FillPolygon f = y as MELHARFI.Manager.FillPolygon;
                tmpY = f.Zindex;
            }
            else
            {
                // MELHARFI.Manager.Rec
                throw new NotImplementedException("object not found");
            }

            if (tmpX > tmpY)
                return -1;
            if (tmpX < tmpY)
                return 1;
            return 0;
        }
    }
}
