using System;
using System.Collections.Generic;
using System.Drawing;

namespace MELHARFI.Manager.Gfx
{
    /// <summary>
    /// interface to unify Bmp, Txt, Rec, Anim classes
    /// </summary>
    public interface IGfx : ICloneable
    {
        string Name { get; set; }
        object Tag { get; set; }
        bool Visible { get; set; }
        int Zindex { get; set; }
        Point Point { get; set; }
        Manager ManagerInstance { get; set; }
    }
}
