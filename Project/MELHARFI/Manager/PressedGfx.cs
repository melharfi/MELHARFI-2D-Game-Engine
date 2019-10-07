using MELHARFI.Manager.Gfx;

namespace MELHARFI.Manager
{
    /// <summary>
    /// pour stoquer les gfx simulé en bouton lorsqu'il ont subit un changement lors de l'evenement clic, pour leurs donner leurs form d'origine si le MouseUp ne la pas fait
    /// </summary>
    internal class PressedGfx
    {
        public Bmp Bitmap;
        public string OldPath;

        public PressedGfx(Bmp bitmap, string oldPath)
        {
            Bitmap = bitmap;
            OldPath = oldPath;
        }
    }
}
