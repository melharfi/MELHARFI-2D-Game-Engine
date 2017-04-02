using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MELHARFI
{
    // gfx motor namespace

    public partial class Manager
    {
        #region Champs

        /// <summary>
        /// Singleton of Manager that lead to deal with all graphic and drawing stuff
        /// </summary>
        public static Manager manager;

        /// <summary>
        /// g is the graphic object used in the paint method, usfull if you need to do some tweak to it
        /// </summary>
        public Graphics g;

        /// <summary>
        /// mainForm is a pointer to your form where graphics should be drawn
        /// </summary>
        public Control control;

        /// <summary>
        /// Changing the background color, Black is the color by default if not initialized
        /// </summary>
        public Color Background = Color.Black;

        /// <summary>
        /// Timer to refresh the paint method, it's considered as a part of the loop game system
        /// </summary>
        public Timer RefreshTimer = new Timer();

        /// <summary>
        /// frame skip per seconds is the time the timer "refreshTimer" will call the paint method, it's considered as a part of the loop game system
        /// </summary>
        public int Fps = 40;

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern int GetPixel(IntPtr hDc, int x, int y);
        //[DllImport("gdi32.dll")]
        //static extern int SetPixel(IntPtr hDC, int x, int y, int color);

        /// <summary>
        /// First layer to display object considered as a background objects
        /// </summary>
        public List<IGfx> GfxBgrList = new List<IGfx>();    // Premiere couche des objets {Bmp (Bitmap), et Txt(Text)} arriere plan

        /// <summary>
        /// Second layer to display your dynamic objects
        /// </summary>
        public List<IGfx> GfxObjList = new List<IGfx>();    // Deuxieme couche des objets {Bmp (Bitmap), et Txt(Text)} objet de jeu

        /// <summary>
        /// Third layer to display object that must stay on the front, like statistics, information, interaction, controls ...
        /// </summary>
        public List<IGfx> GfxTopList = new List<IGfx>();    // troisieme couche des objets sur le devant,obj na pas d'event, non cliquable

        /// <summary>
        /// Layer to store object you don't want them to be removed when you call the clean method
        /// </summary>
        public List<IGfx> GfxFixedList = new List<IGfx>();  // troisieme couche des objets {Bmp (Bitmap), et Txt(Text)} hud + menu option ...
            
        /// <summary>
        /// Layer to record Windows Controls depending on the game purpose
        /// </summary>
        public List<Control> GfxCtrlList = new List<Control>(); // Troiseme couche des objet {Control}  
              
        private readonly List<Control> _gfxCtrlListOnlyVisible = new List<Control>(); // pour memoriser just les controles qui sont visible

        /// <summary>
        /// Layer to store all objects has been clicked by mouse, it's a Mouse Down mechanisme, 
        /// </summary>
        public List<PressedGfx> GfxMousePressedList = new List<PressedGfx>();  // enregistre tout les bouttons qui ont été enfancé pour s'assurer qu'il ont bien été relaché lords du MouseUp

        /// <summary>
        /// Layer to store all objects the opacity is 0 (invisible), it's a Mouse Over mechanisme
        /// </summary>
        public List<OldDataMouseMove> GfxOpacityMouseMoveList = new List<OldDataMouseMove>();      // stock tout les gfx dons l'opacité <1 puis devenu opaque pour les remetre a leurs opacité quand la souris en MouseOut sur le meme gfx

        /// <summary>
        /// Layer store all graphics when dealing with Mouse Out event, not usfull for you, it's only a MouseOut mechanisme
        /// </summary>
        private readonly List<IGfx> _gfxMouseOutList = new List<IGfx>();  // stoque tout les gfx abonnée à l'evenement MouseOut

        /// <summary>
        /// Layer store all graphics when dealing with Mouse over event, not usfull for you, it's only a MouseOver mechanisme
        /// </summary>
        public List<IGfx> GfxMouseOverList = new List<IGfx>(); // stoque tout les gfx en mode MouseOver pour lancer l'evenement MouseOut lors des superposition des objet, comme ca on saura si un autre objet est déja sur le devant

        /// <summary>
        /// If it's true, all Background graphics will be invisible
        /// </summary>
        bool _hideGfxBgr;

        /// <summary>
        /// If it's true, all Objects graphics will be invisible
        /// </summary>
        bool _hideGfxObj;

        /// <summary>
        /// If it's true, all Top graphics will be invisible
        /// </summary>
        bool _hideGfxTop;

        /// <summary>
        /// Callback delegate to follow errors to a method string that accept a string parameter, not usfull for user
        /// </summary>
        /// <param name="s">s hold the error message</param>
        public delegate void DrawOutputErrorCallBack(string s);        // un callback pour recevoir les message d'erreur généré par la methode draw

        /// <summary>
        /// Instance of callback to follow a method with 1 string parameter that hold the error message
        /// </summary>
        public static DrawOutputErrorCallBack OutputErrorCallBack;

        /// <summary>
        /// IF true the error message will be shown in MessageBox control
        /// </summary>
        public static bool ShowErrorsInMessageBox = true;
        #endregion
        #region Contructor
        /// <summary>
        /// Manager method to initialize MELHARFI Engine
        /// </summary>
        /// <param name="_control">Control to be drawn</param>
        public Manager(Control _control)
        {
            control = _control;
            g = control.CreateGraphics();
            control.Paint += mainForm_Paint;
            control.MouseClick += mainForm_MouseClick;
            control.MouseDown += mainForm_MouseDown;
            control.MouseUp += mainForm_MouseUp;
            control.MouseMove += mainForm_MouseMove;
            control.MouseDoubleClick += mainForm_MouseDoubleClick;
            RefreshTimer.Interval = Fps;
            RefreshTimer.Tick += refreshTimer_Tick;
            RefreshTimer.Enabled = true;
        }
        #endregion
        #region Dynamique Methode
        void refreshTimer_Tick(object sender, EventArgs e)
        {
            control.Refresh();
        }
        void mainForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.MouseDoubleClicHandleEvents(e);
        }
        void mainForm_MouseMove(object sender, MouseEventArgs e)
        {
            this.MouseMoveHandleEvents(e);
        }
        void mainForm_MouseUp(object sender, MouseEventArgs e)
        {
            this.MouseUpHandleEvents(e);
        }
        void mainForm_MouseDown(object sender, MouseEventArgs e)
        {
            this.MouseDownHandleEvents(e);
        }
        void mainForm_MouseClick(object sender, MouseEventArgs e)
        {
            this.MouseClicHandleEvents(e);
            //manager.MouseClicHandleEvents(e);
        }
        void mainForm_Paint(object sender, PaintEventArgs e)
        {
            ImageAnimator.UpdateFrames();   // pour metre a jour les images gif animés
            e.Graphics.Clear(Background);   // effacement de l'ecran
            Draw(e);                        // methode de dessin
        }

        /// <summary>
        /// Clear() methode to clear all layers (GfxBgrList, GfxObjList, GfxTopList, GfxCtrlList), make attention that the graphics object marked as Fixed in GfxFixedList layer are not removed
        /// </summary>
        public void Clear()
        {
            for (var cnt = GfxBgrList.Count - 1; cnt >= 0; cnt--)
            {
                if (GfxFixedList.IndexOf(GfxBgrList[cnt]) != -1) continue;
                GfxBgrList[cnt].Visible(false);
                GfxBgrList.RemoveAt(cnt);
            }

            for (var cnt = GfxObjList.Count - 1; cnt >= 0; cnt--)
            {
                if (GfxFixedList.IndexOf(GfxObjList[cnt]) != -1) continue;
                if (GfxObjList[cnt] == null) continue;
                GfxObjList[cnt].Visible(false);
                GfxObjList.RemoveAt(cnt);
            }

            for (int cnt = GfxTopList.Count - 1; cnt >= 0; cnt--)
            {
                if (GfxFixedList.IndexOf(GfxTopList[cnt]) != -1) continue;
                if (GfxTopList[cnt] == null) continue;
                GfxTopList[cnt].Visible(false);
                GfxTopList.RemoveAt(cnt);
            }

            foreach (var t in GfxCtrlList)
                t?.Dispose();
            GfxCtrlList.Clear();

            GfxMousePressedList.Clear();    // netoiyage de la liste des gfx/button enfancé
            GfxOpacityMouseMoveList.Clear();
            ZOrder.Clear();
                
        }

        /// <summary>
        /// Clear() methode to clear all layers (GfxBgrList, GfxObjList, GfxTopList, GfxCtrlList), make attention that the graphics object marked as Fixed in GfxFixedList layer are not removed
        /// </summary>
        /// <param name="all">if All = true, all objects will be removed even those one marked as Fixed in GfxFixedList layer, if All = false nothing happen</param>
        public void Clear(bool all)
        {
            for (int cnt = GfxBgrList.Count - 1; cnt >= 0; cnt--)
                GfxBgrList[cnt].Visible(false);
            GfxBgrList.Clear();

            for (int cnt = GfxObjList.Count - 1; cnt >= 0; cnt--)
                GfxObjList[cnt].Visible(false);
            GfxObjList.Clear();

            for (int cnt = GfxTopList.Count - 1; cnt >= 0; cnt--)
                GfxTopList[cnt].Visible(false);
            GfxTopList.Clear();

            for (int cnt = GfxCtrlList.Count - 1; cnt >= 0; cnt--)
                GfxCtrlList[cnt].Dispose();
            GfxCtrlList.Clear();
        }

        /// <summary>
        /// Clear() method to clear a specific layer (GfxBgrList or GfxObjList or GfxTopList or GfxCtrlList)
        /// </summary>
        /// <param name="igfxListName">Igfx layer to be cleared, use "Bgr" for GfxBgrList, or "Obj" for GfxObjList, or "Ctrl" for GfxCtrlList, or "Top" for GfxTopList, or "Fixed" for GfxFixedList</param>
        /// <param name="all">if All = true, all objects will be removed even those one marked as Fixed in GfxFixedList layer, if All = false only non fixed objects is removed</param>
        public void Clear(string igfxListName, bool all)
        {
            switch (igfxListName)
            {
                case "Bgr":
                    for (int cnt = GfxBgrList.Count - 1; cnt >= 0; cnt--)
                    {
                        if (all)
                        {
                            GfxBgrList[cnt].Visible(false);
                            GfxBgrList.RemoveAt(cnt);
                        }
                        else if (GfxFixedList.IndexOf(GfxBgrList[cnt]) == -1)
                        {
                            GfxBgrList[cnt].Visible(false);
                            GfxBgrList.RemoveAt(cnt);
                        }
                    }
                    GfxBgrList.RemoveAll(f => f == null);
                    break;
                case "Obj":
                    for (int cnt = GfxObjList.Count - 1; cnt >= 0; cnt--)
                    {
                        if (all)
                        {
                            GfxObjList[cnt].Visible(false);
                            GfxObjList.RemoveAt(cnt);
                        }
                        else if (GfxFixedList.IndexOf(GfxObjList[cnt]) == -1)
                        {
                            GfxObjList[cnt].Visible(false);
                            GfxObjList.RemoveAt(cnt);
                        }
                    }
                    GfxBgrList.RemoveAll(f => f == null);
                    break;
                case "Ctrl":
                    foreach (Control t in GfxCtrlList)
                        t.Dispose();
                    GfxCtrlList.Clear();
                    break;
                case "Top":
                    for (int cnt = GfxTopList.Count - 1; cnt >= 0; cnt--)
                    {
                        if (all)
                        {
                            GfxTopList[cnt].Visible(false);
                            GfxTopList.RemoveAt(cnt);
                        }
                        else if (GfxFixedList.IndexOf(GfxTopList[cnt]) == -1)
                        {
                            GfxTopList[cnt].Visible(false);
                            GfxTopList.RemoveAt(cnt);
                        }
                    }
                    GfxBgrList.RemoveAll(f => f == null);
                    break;
                case "Fixed":
                    foreach (IGfx t in GfxFixedList)
                        t.Visible(false);
                    GfxFixedList.Clear();
                    break;
            }

            ZOrder.Clear();
        }

        /// <summary>
        /// ChangeOpacity method let you change an opacity (transparency) of an image,
        /// </summary>
        /// <param name="img">img is an Image object</param>
        /// <param name="opacityvalue">opacityvalue is a float number if equal to 0F mean the image will be completly transparent(invisible), if opacityvalue = 0.5F mean the image is transparent by 50%, if opacityvalue = 1F mean the image is 100% visible</param>
        /// <returns>Return new image with the new opacity</returns>
        public static Bitmap ChangeOpacity(Image img, float opacityvalue)
        {
            // methode qui créée une matrice de tout les couleurs de l'imag, et qui modifie la couche Alpha pour la transparence.
            // la methode dois être appelé delui le nom de la classe "Manager" et non l'instance "manager", puisqu'elle est static(non instanciable)
            // tout fois les evenent ne marche pas dessus vus que GetPixel ignore la transparence appliqué, il faut rendre l'image Opaque si non.

            Bitmap bmp = new Bitmap(img.Width, img.Height); // Determining Width and Height of Source Image
            Graphics graphics = Graphics.FromImage(bmp);
            ColorMatrix colormatrix = new ColorMatrix {Matrix33 = opacityvalue};
            ImageAttributes imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel, imgAttribute);
            graphics.Dispose();   // Releasing all resource used by graphics
            return bmp;
        }

        /// <summary>
        /// GetPixel let you pick the color of pixel from a screen with a given X and Y position
        /// </summary>
        /// <param name="x">x is the horizontal coordinate represented with an Int32 value</param>
        /// <param name="y">y is the vertical coordinate represented with an Int32 value</param>
        /// <returns>return a Color object</returns>
        public Color GetPixel(int x, int y)
        {
            // pour avoir la couleur du pixel pointé par la souris
            Color color = Color.Empty;
            if (control == null) return color;
            IntPtr hDc = GetDC(control.Handle);
            int colorRef = GetPixel(hDc, x, y);
            color = Color.FromArgb(
                colorRef & 0x000000FF,
                (colorRef & 0x0000FF00) >> 8,
                (colorRef & 0x00FF0000) >> 16);
            ReleaseDC(control.Handle, hDc);
            return color;
        }

        /// <summary>
        /// Close method is considered like a destructor because it clean events, screen, stop loop and dispose the the graphic object, Pay attention, if this method is called the Manager is no longer able to draw because of disposed graphoc, another instance should be created then
        /// </summary>
        public void Close()
        {
            control.MouseClick -= mainForm_MouseClick;
            control.MouseDown -= mainForm_MouseDown;
            control.MouseMove -= mainForm_MouseMove;
            control.MouseUp -= mainForm_MouseUp;
            control.Paint -= mainForm_Paint;
            Clear(true);
            RefreshTimer.Stop();
            RefreshTimer.Dispose();
            g.Dispose();
        }

        /// <summary>
        /// Hide or Show all graphics stored in the Background layer "HideGfxBgrList"
        /// </summary>
        /// <param name="hide">Hide = true or false</param>
        public void HideGfxBgrList(bool hide)
        {
            _hideGfxBgr = hide;
        }

        /// <summary>
        /// Hide or Show all graphics stored in the Object layer "HideGfxObjList"
        /// </summary>
        /// <param name="hide">Hide = true or false</param>
        public void HideGfxObjList(bool hide)
        {
            _hideGfxObj = hide;
        }

        /// <summary>
        /// Hide or Show all graphics stored in the Top layer "HideGfxTopList"
        /// </summary>
        /// <param name="hide">Hide = true or false</param>
        public void HideGfxTopList(bool hide)
        {
            _hideGfxTop = hide;
        }

        /// <summary>
        /// Hide or Show all windows controls stored in the Control layer "GfxCtrlList"
        /// </summary>
        /// <param name="hide">Hide = true or false</param>
        public void HideGfxCtrlList(bool hide)
        {
            if (hide)
                foreach (Control t in GfxCtrlList)
                    t.Hide();
            else
                foreach (Control t in GfxCtrlList)
                    t.Show();
        }

        /// <summary>
        /// Hide or Show windows controls, it's different than "HideGfxCtrlList(bool Hide)", assuming you have 10 controls, 1 already hidden, and 9 visibles, and in some point you want to show an animation and need to hide all controls, after the animation is exited you want to show back the controls but ONLY the one that has been visible (9 controls), so this method store the visible object in an extra layer called GfxCtrlListOnlyVisible to check them later
        /// </summary>
        /// <param name="hide">Hide is a boolean value True or False</param>
        public void HideGfxCtrlListOnlyVisible(bool hide)
        {
            if (hide)
                foreach (Control t in GfxCtrlList)
                {
                    if (!t.Visible) continue;
                    _gfxCtrlListOnlyVisible.Add(t);
                    t.Hide();
                }
            else
            {
                foreach (Control t in _gfxCtrlListOnlyVisible)
                    t.Visible = true;
                _gfxCtrlListOnlyVisible.Clear();
            }
        }
        #endregion

        /// <summary>
        /// typeGfx hold type of layer, "bgr" is the Background layer, obj is the Object layer, and top is the top most shown object
        /// </summary>
        public enum TypeGfx
        {
            Bgr, Obj, Top
            // bgr=Background, obj=objet, top= sur le devant
        }
    }
}
