using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MELHARFI
{
    public static class GUIGenerator
    {
        // Création de controls avec la prise en charge du DoubleBuffering pour eviter un probleme de Flickering
        public class ExForm : System.Windows.Forms.Form
        {
            public ExForm()
            {
                this.SetStyle(
                    System.Windows.Forms.ControlStyles.UserPaint |
                    System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                    System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                    true);
            }
        }
        public class ExPanel : System.Windows.Forms.Panel
        {
            public ExPanel()
            {
                this.SetStyle(
                    System.Windows.Forms.ControlStyles.UserPaint |
                    System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                    System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                    true);
            }
        }
        public class ExPictureBox : System.Windows.Forms.PictureBox
        {
            public ExPictureBox()
            {
                this.SetStyle(
                    System.Windows.Forms.ControlStyles.UserPaint |
                    System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                    System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                    true);
            }
        }
    }
}
