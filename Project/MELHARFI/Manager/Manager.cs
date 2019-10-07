using MELHARFI.Manager;
using MELHARFI.Manager.Gfx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MELHARFI.Manager
{
    // gfx motor namespace

    public partial class Manager
    {
        #region Properties

        /// <summary>
        /// Singleton of Manager that lead to deal with all graphic and drawing stuff
        /// </summary>
        public static List<Manager> Managers = new List<Manager>();

        /// <summary>
        /// g is the graphic object used in the paint method, usfull if you need to do some tweak to it
        /// </summary>
        private readonly Graphics graphics;

        /// <summary>
        /// Control where graphics will be drawn
        /// </summary>
        public Control Control;

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

        internal readonly List<Control> gfxControlLayerOnlyVisible = new List<Control>(); // pour memoriser just les controles qui sont visible

        /// <summary>
        /// Layer to store all objects has been clicked by mouse, it's a Mouse Down mechanisme, 
        /// </summary>
        internal List<PressedGfx> mouseDownRecorder = new List<PressedGfx>();  // enregistre tout les bouttons qui ont été enfancé pour s'assurer qu'il ont bien été relaché lords du MouseUp

        /// <summary>
        /// Layer to store all objects the opacity is 0 (invisible), it's a Mouse Over mechanisme
        /// </summary>
        internal List<OldDataMouseMove> opacityMouseMoveRecorder = new List<OldDataMouseMove>();      // stock tout les gfx dons l'opacité <1 puis devenu opaque pour les remetre a leurs opacité quand la souris en MouseOut sur le meme gfx

        /// <summary>
        /// Layer store all graphics when dealing with Mouse Out event, not usfull for you, it's only a MouseOut mechanisme
        /// </summary>
        internal readonly List<IGfx> mouseOutRecorder = new List<IGfx>();  // stoque tout les gfx abonnée à l'evenement MouseOut

        /// <summary>
        /// Layer store all graphics when dealing with Mouse over event, not usfull for you, it's only a MouseOver mechanisme
        /// </summary>
        internal List<IGfx> mouseOverRecorder = new List<IGfx>(); // stoque tout les gfx en mode MouseOver pour lancer l'evenement MouseOut lors des superposition des objet, comme ca on saura si un autre objet est déja sur le devant

        /// <summary>
        /// If it's true, all Background graphics will be invisible
        /// </summary>
        bool hideBagroundLayer;

        /// <summary>
        /// If it's true, all Objects graphics will be invisible
        /// </summary>
        private bool hideObjectLayer;

        /// <summary>
        /// If it's true, all Top graphics will be invisible
        /// </summary>
        private bool hideTopLayer;

        /// <summary>
        /// Callback delegate to follow errors to a method string that accept a string parameter, not usfull for user
        /// </summary>
        /// <param name="s">s hold the error message</param>
        public delegate void DrawOutputErrorCallBack(string s);        // un callback pour recevoir les message d'erreur généré par la methode draw

        //TODO Remove static
        /// <summary>
        /// Instance of callback to follow a method with 1 string parameter that hold the error message
        /// </summary>
        public DrawOutputErrorCallBack OutputErrorCallBack;

        public string Name { get; set; }

        public ZOrder ZOrder = new ZOrder();
        #endregion
        #region Contructor
        /// <summary>
        /// Manager method to initialize MELHARFI Engine
        /// </summary>
        /// <param name="control">Control to be drawn</param>
        public Manager(Control control, string name)
        {
            Control = control;
            SetDoubleBuffered(control);
            graphics = Control.CreateGraphics();
            Control.Paint += control_Paint;
            Control.MouseClick += control_MouseClick;
            Control.MouseDown += control_MouseDown;
            Control.MouseUp += control_MouseUp;
            Control.MouseMove += control_MouseMove;
            Control.MouseDoubleClick += control_MouseDoubleClick;
            RefreshTimer.Interval = Fps;
            RefreshTimer.Tick += refreshTimer_Tick;
            RefreshTimer.Enabled = true;
            Name = name;
            Managers.Add(this);
        }
        #endregion

        #region Dynamique Methode
        void refreshTimer_Tick(object sender, EventArgs e)
        {
            Control.Refresh();
        }

        void SetDoubleBuffered(System.Windows.Forms.Control control)
        {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp =
                  typeof(System.Windows.Forms.Control).GetProperty(
                        "DoubleBuffered",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            aProp.SetValue(control, true, null);
        }

        void control_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.MouseEventHandler(e, MouseEvent.MouseDoubleClic);
        }
        void control_MouseMove(object sender, MouseEventArgs e)
        {
            this.MouseMoveHandlerEvents(e);
        }
        void control_MouseUp(object sender, MouseEventArgs e)
        {
            this.MouseEventHandler(e, MouseEvent.MouseUp);
        }
        void control_MouseDown(object sender, MouseEventArgs e)
        {
            this.MouseEventHandler(e, MouseEvent.MouseDown);
        }
        void control_MouseClick(object sender, MouseEventArgs e)
        {
            this.MouseEventHandler(e, MouseEvent.MouseClic);
        }
        void control_Paint(object sender, PaintEventArgs e)
        {
            ImageAnimator.UpdateFrames();   // pour metre a jour les images gif animés
            e.Graphics.Clear(Background);   // effacement de l'ecran
            Draw(e);                        // methode de dessin
        }
        /// <summary>
        /// force control to be double buffered because it's a protected property
        /// </summary>
        /// <param name="c"></param>
        

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

            mouseDownRecorder.Clear();    // netoiyage de la liste des gfx/button enfancé
            opacityMouseMoveRecorder.Clear();
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
        public Bitmap Opacity(Image img, float opacityvalue)
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
            if (Control == null) return color;
            IntPtr hDc = GetDC(Control.Handle);
            int colorRef = GetPixel(hDc, x, y);
            color = Color.FromArgb(
                colorRef & 0x000000FF,
                (colorRef & 0x0000FF00) >> 8,
                (colorRef & 0x00FF0000) >> 16);
            ReleaseDC(Control.Handle, hDc);
            return color;
        }

        /// <summary>
        /// Close method is considered like a destructor because it clean events, screen, stop loop and dispose the the graphic object, Pay attention, if this method is called the Manager is no longer able to draw because of disposed graphoc, another instance should be created then
        /// </summary>
        public void Close()
        {
            Control.MouseClick -= control_MouseClick;
            Control.MouseDown -= control_MouseDown;
            Control.MouseMove -= control_MouseMove;
            Control.MouseUp -= control_MouseUp;
            Control.Paint -= control_Paint;
            Clear(true);
            RefreshTimer.Stop();
            RefreshTimer.Dispose();
            graphics.Dispose();
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
                    gfxControlLayerOnlyVisible.Add(t);
                    t.Hide();
                }
            else
            {
                foreach (Control t in gfxControlLayerOnlyVisible)
                    t.Visible = true;
                gfxControlLayerOnlyVisible.Clear();
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
