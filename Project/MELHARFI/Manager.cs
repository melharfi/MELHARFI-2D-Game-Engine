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
        public static Dictionary<Manager, string> ManagerInstances = new Dictionary<Manager, string>();

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
        public List<IGfx> BackgroundLayer = new List<IGfx>();    // Premiere couche des objets {Bmp (Bitmap), et Txt(Text)} arriere plan

        /// <summary>
        /// Second layer to display your dynamic objects
        /// </summary>
        public List<IGfx> ObjectLayer = new List<IGfx>();    // Deuxieme couche des objets {Bmp (Bitmap), et Txt(Text)} objet de jeu

        /// <summary>
        /// Third layer to display object that must stay on the front, like statistics, information, interaction, controls ...
        /// </summary>
        public List<IGfx> TopLayer = new List<IGfx>();    // troisieme couche des objets sur le devant,obj na pas d'event, non cliquable

        /// <summary>
        /// Layer to store object you don't want them to be removed when you call the clean method
        /// </summary>
        public List<IGfx> FixedObjectLayer = new List<IGfx>();  // troisieme couche des objets {Bmp (Bitmap), et Txt(Text)} hud + menu option ...

        /// <summary>
        /// Layer to record Windows Controls depending on the game purpose
        /// </summary>
        public List<Control> ControlLayer = new List<Control>(); // Troiseme couche des objet {Control}  
              
        private readonly List<Control> _gfxControlLayerOnlyVisible = new List<Control>(); // pour memoriser just les controles qui sont visible

        /// <summary>
        /// Layer to store all objects has been clicked by mouse, it's a Mouse Down mechanisme, 
        /// </summary>
        private List<PressedGfx> MouseDownRecorder = new List<PressedGfx>();  // enregistre tout les bouttons qui ont été enfancé pour s'assurer qu'il ont bien été relaché lords du MouseUp

        /// <summary>
        /// Layer to store all objects the opacity is 0 (invisible), it's a Mouse Over mechanisme
        /// </summary>
        public List<OldDataMouseMove> OpacityMouseMoveRecorder = new List<OldDataMouseMove>();      // stock tout les gfx dons l'opacité <1 puis devenu opaque pour les remetre a leurs opacité quand la souris en MouseOut sur le meme gfx

        /// <summary>
        /// Layer store all graphics when dealing with Mouse Out event, not usfull for you, it's only a MouseOut mechanisme
        /// </summary>
        private readonly List<IGfx> MouseOutRecorder = new List<IGfx>();  // stoque tout les gfx abonnée à l'evenement MouseOut

        /// <summary>
        /// Layer store all graphics when dealing with Mouse over event, not usfull for you, it's only a MouseOver mechanisme
        /// </summary>
        public List<IGfx> MouseOverRecorder = new List<IGfx>(); // stoque tout les gfx en mode MouseOver pour lancer l'evenement MouseOut lors des superposition des objet, comme ca on saura si un autre objet est déja sur le devant

        /// <summary>
        /// If it's true, all Background graphics will be invisible
        /// </summary>
        bool hideBagroundLayer;

        /// <summary>
        /// If it's true, all Objects graphics will be invisible
        /// </summary>
        bool hideObjectLayer;

        /// <summary>
        /// If it's true, all Top graphics will be invisible
        /// </summary>
        bool hideTopLayer;

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

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        #endregion
        #region Contructor
        /// <summary>
        /// Manager method to initialize MELHARFI Engine
        /// </summary>
        /// <param name="_control">Control to be drawn</param>
        public Manager(Control _control, string _name)
        {
            control = _control;
            g = control.CreateGraphics();
            control.Paint += control_Paint;
            control.MouseClick += control_MouseClick;
            control.MouseDown += control_MouseDown;
            control.MouseUp += control_MouseUp;
            control.MouseMove += control_MouseMove;
            control.MouseDoubleClick += control_MouseDoubleClick;
            RefreshTimer.Interval = Fps;
            RefreshTimer.Tick += refreshTimer_Tick;
            RefreshTimer.Enabled = true;
            name = _name;
            ManagerInstances.Add(this, _name);
        }
        #endregion
        #region Dynamique Methode
        void refreshTimer_Tick(object sender, EventArgs e)
        {
            control.Refresh();
        }
        void control_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.MouseDoubleClicHandleEvents(e);
        }
        void control_MouseMove(object sender, MouseEventArgs e)
        {
            this.MouseMoveHandleEvents(e);
        }
        void control_MouseUp(object sender, MouseEventArgs e)
        {
            this.MouseUpHandleEvents(e);
        }
        void control_MouseDown(object sender, MouseEventArgs e)
        {
            this.MouseDownHandleEvents(e);
        }
        void control_MouseClick(object sender, MouseEventArgs e)
        {
            this.MouseClicHandleEvents(e);
        }
        void control_Paint(object sender, PaintEventArgs e)
        {
            ImageAnimator.UpdateFrames();   // pour metre a jour les images gif animés
            e.Graphics.Clear(Background);   // effacement de l'ecran
            Draw(e);                        // methode de dessin
        }

        /// <summary>
        /// Clear() methode to clear all layers (BackgroundLayer, ObjectLayer, TopLayer, ControlLayer), make attention that the graphics object marked as Fixed in FixedObjectLayer layer are not removed
        /// </summary>
        public void Clear()
        {
            for (var cnt = BackgroundLayer.Count - 1; cnt >= 0; cnt--)
            {
                if (FixedObjectLayer.IndexOf(BackgroundLayer[cnt]) != -1) continue;
                BackgroundLayer[cnt].Visible = false;
                BackgroundLayer.RemoveAt(cnt);
            }

            for (var cnt = ObjectLayer.Count - 1; cnt >= 0; cnt--)
            {
                if (FixedObjectLayer.IndexOf(ObjectLayer[cnt]) != -1) continue;
                if (ObjectLayer[cnt] == null) continue;
                ObjectLayer[cnt].Visible = false;
                ObjectLayer.RemoveAt(cnt);
            }

            for (int cnt = TopLayer.Count - 1; cnt >= 0; cnt--)
            {
                if (FixedObjectLayer.IndexOf(TopLayer[cnt]) != -1) continue;
                if (TopLayer[cnt] == null) continue;
                TopLayer[cnt].Visible = false;
                TopLayer.RemoveAt(cnt);
            }

            foreach (var t in ControlLayer)
                t?.Dispose();
            ControlLayer.Clear();

            MouseDownRecorder.Clear();    // netoiyage de la liste des gfx/button enfancé
            OpacityMouseMoveRecorder.Clear();
            ZOrder.Clear();
                
        }

        /// <summary>
        /// Clear() methode to clear all layers (BackgroundLayer, ObjectLayer, TopLayer, ControlLayer), make attention that the graphics object marked as Fixed in FixedObjectLayer layer are not removed
        /// </summary>
        /// <param name="all">if All = true, all objects will be removed even those one marked as Fixed in FixedObjectLayer layer, if All = false nothing happen</param>
        public void Clear(bool all)
        {
            for (int cnt = BackgroundLayer.Count - 1; cnt >= 0; cnt--)
                BackgroundLayer[cnt].Visible = false;
            BackgroundLayer.Clear();

            for (int cnt = ObjectLayer.Count - 1; cnt >= 0; cnt--)
                ObjectLayer[cnt].Visible = false;
            ObjectLayer.Clear();

            for (int cnt = TopLayer.Count - 1; cnt >= 0; cnt--)
                TopLayer[cnt].Visible = false;
            TopLayer.Clear();

            for (int cnt = ControlLayer.Count - 1; cnt >= 0; cnt--)
                ControlLayer[cnt].Dispose();
            ControlLayer.Clear();
        }

        /// <summary>
        /// Clear() method to clear a specific layer (BackgroundLayer or ObjectLayer or TopLayer or ControlLayer)
        /// </summary>
        /// <param name="LayerName">Igfx layer to be cleared, use "Bgr" for BackgroundLayer, or "Obj" for ObjectLayer, or "Ctrl" for ControlLayer, or "Top" for TopLayer, or "Fixed" for FixedcLayer</param>
        /// <param name="all">if All = true, all objects will be removed even those one marked as Fixed in FixedObjectLayer layer, if All = false only non fixed objects is removed</param>
        public void Clear(Layers LayerName, bool all)
        {
            switch (LayerName)
            {
                case Layers.Background:
                    for (int cnt = BackgroundLayer.Count - 1; cnt >= 0; cnt--)
                    {
                        if (all)
                        {
                            BackgroundLayer[cnt].Visible = false;
                            BackgroundLayer.RemoveAt(cnt);
                        }
                        else if (FixedObjectLayer.IndexOf(BackgroundLayer[cnt]) == -1)
                        {
                            BackgroundLayer[cnt].Visible = false;
                            BackgroundLayer.RemoveAt(cnt);
                        }
                    }
                    BackgroundLayer.RemoveAll(f => f == null);
                    break;
                case Layers.Object:
                    for (int cnt = ObjectLayer.Count - 1; cnt >= 0; cnt--)
                    {
                        if (all)
                        {
                            ObjectLayer[cnt].Visible = false;
                            ObjectLayer.RemoveAt(cnt);
                        }
                        else if (FixedObjectLayer.IndexOf(ObjectLayer[cnt]) == -1)
                        {
                            ObjectLayer[cnt].Visible = false;
                            ObjectLayer.RemoveAt(cnt);
                        }
                    }
                    BackgroundLayer.RemoveAll(f => f == null);
                    break;
                case Layers.Control:
                    foreach (Control t in ControlLayer)
                        t.Dispose();
                    ControlLayer.Clear();
                    break;
                case Layers.Top:
                    for (int cnt = TopLayer.Count - 1; cnt >= 0; cnt--)
                    {
                        if (all)
                        {
                            TopLayer[cnt].Visible = false;
                            TopLayer.RemoveAt(cnt);
                        }
                        else if (FixedObjectLayer.IndexOf(TopLayer[cnt]) == -1)
                        {
                            TopLayer[cnt].Visible = false;
                            TopLayer.RemoveAt(cnt);
                        }
                    }
                    BackgroundLayer.RemoveAll(f => f == null);
                    break;
                case Layers.Fixed:
                    foreach (IGfx t in FixedObjectLayer)
                        t.Visible = false;
                    FixedObjectLayer.Clear();
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
        public static Bitmap Opacity(Image img, float opacityvalue)
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
            control.MouseClick -= control_MouseClick;
            control.MouseDown -= control_MouseDown;
            control.MouseMove -= control_MouseMove;
            control.MouseUp -= control_MouseUp;
            control.Paint -= control_Paint;
            Clear(true);
            RefreshTimer.Stop();
            RefreshTimer.Dispose();
            g.Dispose();
        }

        /// <summary>
        /// Hide or Show all graphics stored in the Background layer "HideBackgroundLayer"
        /// </summary>
        /// <param name="hide">Hide = true or false</param>
        public void HideBackgroundLayer(bool hide)
        {
            hideBagroundLayer = hide;
        }

        /// <summary>
        /// Hide or Show all graphics stored in the Object layer "HideObjectList"
        /// </summary>
        /// <param name="hide">Hide = true or false</param>
        public void HideObjectLayer(bool hide)
        {
            hideObjectLayer = hide;
        }

        /// <summary>
        /// Hide or Show all graphics stored in the Top layer "HideTopList"
        /// </summary>
        /// <param name="hide">Hide = true or false</param>
        public void HideTopLayer(bool hide)
        {
            hideTopLayer = hide;
        }

        /// <summary>
        /// Hide or Show all windows controls stored in the Control layer "ControlLayer"
        /// </summary>
        /// <param name="hide">Hide = true or false</param>
        public void HideControlLayer(bool hide)
        {
            if (hide)
                foreach (Control t in ControlLayer)
                    t.Hide();
            else
                foreach (Control t in ControlLayer)
                    t.Show();
        }

        /// <summary>
        /// Hide or Show windows controls, it's different than "HideControlLayer(bool Hide)", assuming you have 10 controls, 1 already hidden, and 9 visibles, and in some point you want to show an animation and need to hide all controls, after the animation is exited you want to show back the controls but ONLY the one that has been visible (9 controls), so this method store the visible object in an extra layer called _gfxControlLayerOnlyVisible to check them later
        /// </summary>
        /// <param name="hide">Hide is a boolean value True or False</param>
        public void HideControlLayerOnlyVisible(bool hide)
        {
            if (hide)
                foreach (Control t in ControlLayer)
                {
                    if (!t.Visible) continue;
                    _gfxControlLayerOnlyVisible.Add(t);
                    t.Hide();
                }
            else
            {
                foreach (Control t in _gfxControlLayerOnlyVisible)
                    t.Visible = true;
                _gfxControlLayerOnlyVisible.Clear();
            }
        }
        #endregion

        /// <summary>
        /// typeGfx hold type of layer, "bgr" is the Background layer, obj is the Object layer, and top is the top most shown object
        /// </summary>
        public enum TypeGfx
        {
            Background, Object, Top
            // bgr=Background, obj=objet, top= sur le devant
        }

        public enum Layers
        {
            Background, Object, Top, Control, Fixed
        }
    }
}
