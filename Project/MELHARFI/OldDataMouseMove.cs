using MELHARFI.Gfx;

namespace MELHARFI
{
    public class OldDataMouseMove
    {
        // stoque les donné de l'objet qui est redevenu opaque alors qu'il été moitié transparent
        // vus que le type bmp est de type reference, on peux pas garder la valeur opacity d'origine
        // pour l'appliquer sur l'objet apres MouseMove (en sortant de l'objet pour qu'il redeviens semie transparent)
        public Bmp bmp;
        public float opacity;

        public OldDataMouseMove(Bmp _bmp, float _opacity)
        {
            bmp = _bmp;
            opacity = _opacity;
        }
    }
}
