using System;
using System.Collections.Generic;

namespace MELHARFI
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
        List<IGfx> Child { get; set; }
        Manager ParentManager { get; set; }
    }
}
