using System;

namespace MELHARFI
{
    /// <summary>
    /// interface to unify Bmp, Txt, Rec, Anim classes
    /// </summary>
    public interface IGfx : ICloneable
    {
        string Name();
        object Tag();
        void Visible(bool visible);
        bool Visible();
        int Zindex();
    }
}
