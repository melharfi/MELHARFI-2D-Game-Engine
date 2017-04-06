using MELHARFI;

namespace MELHARFI
{
    public partial class Manager
    {
        public class PressedGfx
        {
            // pour stoquer les gfx simulé en bouton lorsqu'il ont subit un changement lors de l'evenement clic
            // pour leurs donner leurs form d'origine si le MouseUp ne la pas fait
            public Bmp bmp;
            public string OldPath;

            public PressedGfx(Bmp _bmp, string _oldPath)
            {
                bmp = _bmp;
                OldPath = _oldPath;
            }
        }
    }
}
