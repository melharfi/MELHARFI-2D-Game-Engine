using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using static MELHARFI.Manager.Manager;

namespace MELHARFI.Manager.Gfx
{
    /// <summary>
    /// Bmp class store information about image to be drawn
    /// </summary>
    public class Bmp : IGfx
    {
        #region properties

        #region Mouse Event Handler
        // classe Bmp qui contiens des infos sur les images, hérite de l'interface IGfx
        /// <summary>
        /// Delegate of Mouse Event Handler, not usfull for the user, it's a Mouse Event system mechanisme
        /// </summary>
        /// <param name="bmp">bmp is a graphic object "image" that raised the event</param>
        /// <param name="e">e is a Mouse Event Arguments with some handy information, as button of clic, position ...</param>
        public delegate void bmpMouseEventHandler(Bmp bmp, MouseEventArgs e);

        /// <summary>
        /// Handler when you double clic on object
        /// </summary>
        public event bmpMouseEventHandler MouseDoubleClic;

        /// <summary>
        /// Handler when you clic on object
        /// </summary>
        public event bmpMouseEventHandler MouseClic;

        /// <summary>
        /// Handler when you clic and keep button clicked on the object
        /// </summary>
        public event bmpMouseEventHandler MouseDown;

        /// <summary>
        /// Handler when you release button that has been clicked in an object
        /// </summary>
        public event bmpMouseEventHandler MouseUp;

        /// <summary>
        /// Handler when the mouse move inside an object, event will raise many time as you move the mouse over object
        /// </summary>
        public event bmpMouseEventHandler MouseMove;

        /// <summary>
        /// Handler when a mouse get outside of an object
        /// </summary>
        public event bmpMouseEventHandler MouseOut;

        /// <summary>
        /// Handler when a mouse get indise an object, the event is raised only 1 time
        /// </summary>
        public event bmpMouseEventHandler MouseOver;

        /// <summary>
        /// void raise mouse event if true
        /// </summary>
        public bool EscapeGfxWhileMouseDoubleClic = false;
        public bool EscapeGfxWhileMouseClic = false;
        public bool EscapeGfxWhileMouseOver = false;
        public bool EscapeGfxWhileMouseDown = false;
        public bool EscapeGfxWhileMouseUp = false;
        public bool EscapeGfxWhileMouseMove = false;
        public bool EscapeGfxWhileKeyDown = false;

        /// <summary>
        /// To fire the Mouse Double Clic event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseDoubleClic(MouseEventArgs e) => MouseDoubleClic?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Clic event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseClic(MouseEventArgs e) => MouseClic?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Down event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseDown(MouseEventArgs e) => MouseDown?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Up event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseUp(MouseEventArgs e) => MouseUp?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Move event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseMove(MouseEventArgs e) => MouseMove?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Over event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseOver(MouseEventArgs e) => MouseOver?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Out event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseOut(MouseEventArgs e)
        {
            if (MouseOut == null) return;
            ManagerInstance.mouseOverRecorder.Remove(this);  // pour que MouseOut ne cherche pas sur un Gfx qui n'est pas sur le devant
            MouseOut(this, e);
        }
        #endregion

        /// <summary>
        /// Layer holding sub graphics that is shown in the front of the parent, and its position is relative to parent
        /// </summary>

        public List<IGfx> Childs { get; set; } = new List<IGfx>();

        /// <summary>
        /// Layer to memorise all sequaces of rectangles of a sprite sheet, it's only a user side, and not a engine mechanisme
        /// </summary>
        public List<Rectangle> SpriteSheets = new List<Rectangle>();

        /// <summary>
        /// Bitmap image
        /// </summary>
        public Bitmap Bitmap;

        /// <summary>
        /// path to the given image
        /// </summary>
        public string Path;

        /// <summary>
        /// point where the image will be drawn on the form, Default is X = 0, Y = 0;
        /// </summary>
        public Point Point { get; set; } = Point.Empty;

        /// <summary>
        /// String value to store the name of the object, useful if you need to look for it in the appropriate layer, value is read only
        /// </summary>
        /// <returns>Return a string value as a name of the object</returns>
        public string Name { get; set; }

        /// <summary>
        /// Zindex is a int value indicating the deep of the object against the other object in same graphic layer, default is 1
        /// </summary>
        /// <returns>Return an int value </returns>
        public int Zindex { get; set; } = 1;

        /// <summary>
        /// Boolean value indicating if the object is visible or invisible, value read only
        /// </summary>
        /// <returns>Return a boolean value</returns>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Float, Opacity is the transparency of the object, 1F = full opaque (100% visible), 0.5F is 50% visible, 0F is invisible, default is 1 (100% visible)
        /// </summary>
        public float Opacity = 1;

        /// <summary>
        /// Hold which layer the object is stored
        /// </summary>
        public TypeGfx TypeGfx = TypeGfx.Background;

        /// <summary>
        /// Tag is an object that you can affect to anything you want, usful to attach it to a class that hold some statistic to a players, Pay attention that you should cast the object
        /// </summary>
        /// <returns>Return Object</returns>
        public object Tag { get; set; }

        /// <summary>
        /// Boolean value indicating if the picture is encrypted or not
        /// </summary>
        public bool Crypted;

        /// <summary>
        /// newColorMap is a ColorMap object, used to convert a color from one color to another in the all picture, usfull to paint some area like changing color of eyes, skin, hair
        /// </summary>
        public ColorMap[] NewColorMap;

        /// <summary>
        /// key is string of 8 characters (first key) for Rijndael AES encryption
        /// </summary>
        public string Key;

        /// <summary>
        /// iv is string of 8 characters called Initializing Vector (second key) for Rijndael AES encryption
        /// </summary>
        public string IV;

        /// <summary>
        /// Rectangle is an area of an image, used to show only a part of image, a Rectangle need a Point value defined by a X position and Y position, and Size value defined by a Width and Height
        /// </summary>
        public Rectangle Rectangle
        {
            get;
            set;
        }

        /// <summary>
        /// Flag equal true when the object show only a rectangle piece of the image (SpriteSheet), if false, then all the image is displayed
        /// </summary>
        public bool IsSpriteSheet { get; private set; }

        /// <summary>
        /// A referense is requird to the Manager instance that hold this object
        /// </summary>
        public Manager ManagerInstance { get; set; }
        #endregion

        #region constructors
        /// <summary>
        /// Empty Bmp constructor for a bitmap, Pay attention that you should initialise the other parameters later
        /// </summary>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(Manager manager)
        {
            // constructeur vide qui sert a initialiser l'objet Bmp avant de connaitre l'image qui va l'occuper
            // vus que ce n'est pas possible d'initialiser un objet Bitmap sans connaitre la source
            // cela instancie la classe Bmp avant, et affecter une image apres
            ManagerInstance = manager;
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, Manager manager)
        {
            Path = bitmap;
            Point = point;
            ManagerInstance = manager;

            Bitmap = new Bitmap(Path);
            if (Bitmap.RawFormat.Equals(ImageFormat.Gif))
                ImageAnimator.Animate(Bitmap, null);
            Rectangle = new Rectangle(new Point(0, 0), new Size(Bitmap.Width, Bitmap.Height));

            Zindex = ManagerInstance.ZOrder.Bgr();
            TypeGfx =  TypeGfx.Background;   
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="rectangle">Rectangle is a square area defined by its point (X, Y) and its size (width, height) </param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, Rectangle rectangle, Manager manager)
        {
            Path = bitmap;
            Point = point;
            ManagerInstance = manager;

            Bitmap = new Bitmap(Path);
            if (Bitmap.RawFormat.Equals(ImageFormat.Gif))
                ImageAnimator.Animate(Bitmap, null);

            Rectangle = rectangle;
            IsSpriteSheet = true;

            Zindex = ManagerInstance.ZOrder.Bgr();
            TypeGfx = TypeGfx.Background;
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="Key">string value of 8 character, it's the first key of Rijndael AES encryption</param>
        /// <param name="IV">string value of 8 character, it's the second key of Rijndael AES encryption called Initializing Vector</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, string Key, string IV, Manager manager)
        {
            Path = bitmap;
            Point = point;
            Crypted = true;
            this.Key = Key;
            this.IV = IV;
            ManagerInstance = manager;

            using (Stream m = new Crypt().DecryptFile(Path, Key, IV))
            {
                Bitmap tmp = new Bitmap(m);

                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    Bitmap = tmp;
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }
            Rectangle = new Rectangle(new Point(0, 0), new Size(Bitmap.Width, Bitmap.Height));

            Zindex = ManagerInstance.ZOrder.Bgr();
            TypeGfx = TypeGfx.Background;
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="Key">string value of 8 character, it's the first key of Rijndael AES encryption</param>
        /// <param name="IV">string value of 8 character, it's the second key of Rijndael AES encryption called Initializing Vector</param>
        /// <param name="rectangle">Rectangle is a square area defined by its point (X, Y) and its size (width, height) </param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, string Key, string IV, Rectangle rectangle, Manager manager)
        {
            Path = bitmap;
            Point = point;
            Crypted = true;
            this.Key = Key;
            this.IV = IV;
            ManagerInstance = manager;

            using (Stream m = new Crypt().DecryptFile(Path, Key, IV))
            {
                Bitmap bmp = new Bitmap(m);
                if (bmp.RawFormat.Equals(ImageFormat.Gif))
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }

            Rectangle = rectangle;
            IsSpriteSheet = true;

            Zindex = ManagerInstance.ZOrder.Bgr();
            TypeGfx = TypeGfx.Background;
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="size">size is defined by it's width value and height value, making a image expanded</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, Size size, Manager manager)
        {
            Path = bitmap;
            Point = point;
            ManagerInstance = manager;

            using (Bitmap tmp = new Bitmap(Path))
            {
                Bitmap = new Bitmap(new Bitmap(Path), size);
                if (tmp.RawFormat.Equals(ImageFormat.Gif))
                {
                    // si ce constructeur n'affiche pas une image gif avec just le rectangle défini il faut utiliser juste bmp = new Bitmap(path); qui ne prend pas le rectangle en considération
                    //bmp = new Bitmap(path);
                        
                    ImageAnimator.Animate(Bitmap, null);
                }
            }
                    
            Rectangle = new Rectangle(new Point(0, 0), size);

            Zindex = ManagerInstance.ZOrder.Bgr();
            TypeGfx = TypeGfx.Background;
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="size">size is defined by it's width value and height value, making a image expanded</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(Bitmap bitmap, Point point, Size size, Manager manager)
        {
            // generalement utilisé pour redimentionner une image pris depuis un spritesheet
            Point = point;
            Bitmap = new Bitmap(bitmap, size);
            ManagerInstance = manager;

            if (Bitmap.RawFormat.Equals(ImageFormat.Gif))
                ImageAnimator.Animate(Bitmap, null);
            Rectangle = new Rectangle(new Point(0, 0), size);

            Zindex = ManagerInstance.ZOrder.Bgr();
            TypeGfx = TypeGfx.Background;
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="size">size is defined by it's width value and height value, making a image expanded</param>
        /// <param name="Key">string value of 8 character, it's the first key of Rijndael AES encryption</param>
        /// <param name="IV">string value of 8 character, it's the second key of Rijndael AES encryption called Initializing Vector</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, Size size, string Key, string IV, Manager manager)
        {
            Path = bitmap;
            Crypted = true;
            this.Key = Key;
            this.IV = IV;
            Point = point;
            ManagerInstance = manager;

            using (Stream m = new Crypt().DecryptFile(Path, Key, IV))
            {
                Bitmap tmp = new Bitmap(m);
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    Bitmap = new Bitmap(new Bitmap(m), size);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }
            Rectangle = new Rectangle(new Point(0, 0), size);

            Zindex = ManagerInstance.ZOrder.Bgr();
            TypeGfx = TypeGfx.Background;
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, string name, TypeGfx typeGfx, bool visible, Manager manager)
        {
            Path = bitmap;
            Point = point;
            Name = name;
            Visible = visible;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            using (Bitmap tmp = new Bitmap(Path))
            {
                Bitmap = new Bitmap(Path);
                if (tmp.RawFormat.Equals(ImageFormat.Gif))
                    ImageAnimator.Animate(Bitmap, null);
            }
            Rectangle = new Rectangle(new Point(0, 0), new Size(Bitmap.Width, Bitmap.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="rectangle">Rectangle is a square area defined by its point (X, Y) and its size (width, height) </param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, string name, TypeGfx typeGfx, bool visible, Rectangle rectangle, Manager manager)
        {
            Path = bitmap;
            Point = point;
            Name = name;
            Visible = visible;
            Rectangle = rectangle;
            IsSpriteSheet = true;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            using (Bitmap tmp = new Bitmap(Path))
            {
                Bitmap = new Bitmap(Path);
                if (tmp.RawFormat.Equals(ImageFormat.Gif))
                    ImageAnimator.Animate(Bitmap, null);
            }
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="Key">string value of 8 character, it's the first key of Rijndael AES encryption</param>
        /// <param name="IV">string value of 8 character, it's the second key of Rijndael AES encryption called Initializing Vector</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, string name, TypeGfx typeGfx, bool visible, string Key, string IV, Manager manager)
        {
            // instancier sans spésification de la taille, (taille d'origine de l'image)
            Path = bitmap;
            Point = point;
            Name = name;
            Visible = visible;
            Crypted = true;
            this.Key = Key;
            this.IV = IV;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            using (Stream m = new Crypt().DecryptFile(Path, Key, IV))
            {
                Bitmap = new Bitmap(m);
                if (Bitmap.RawFormat.Equals(ImageFormat.Gif))
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }

            Rectangle = new Rectangle(new Point(0, 0), Bitmap.Size);
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="Key">string value of 8 character, it's the first key of Rijndael AES encryption</param>
        /// <param name="IV">string value of 8 character, it's the second key of Rijndael AES encryption called Initializing Vector</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, string name, TypeGfx typeGfx, bool visible, string Key, string IV, Rectangle rectangle, Manager manager)
        {
            // instancier sans spésification de la taille, (taille d'origine de l'image)
            Path = bitmap;
            Point = point;
            Name = name;
            Visible = visible;
            Crypted = true;
            this.Key = Key;
            this.IV = IV;
            Rectangle = rectangle;
            IsSpriteSheet = true;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            using (Stream m = new Crypt().DecryptFile(Path, Key, IV))
            {
                Bitmap = new Bitmap(m);
                if (Bitmap.RawFormat.Equals(ImageFormat.Gif))
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="size">size is defined by it's width value and height value, making a image expanded</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, Size size, string name, TypeGfx typeGfx, bool visible, Manager manager)
        {
            Path = bitmap;
            Name = name;
            Visible = visible;
            Point = point;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            using (Bitmap tmp = new Bitmap(Path))
            {
                Bitmap = new Bitmap(new Bitmap(Path), size);
                if (tmp.RawFormat.Equals(ImageFormat.Gif))
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }
            Rectangle = new Rectangle(new Point(0, 0), size);
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="size">size is defined by it's width value and height value, making a image expanded</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="Key">string value of 8 character, it's the first key of Rijndael AES encryption</param>
        /// <param name="IV">string value of 8 character, it's the second key of Rijndael AES encryption called Initializing Vector</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, Size size, string name, TypeGfx typeGfx, bool visible, string Key, string IV, Manager manager)
        {
            Path = bitmap;
            Crypted = true;
            this.Key = Key;
            this.IV = IV;
            Point = point;
            Name = name;
            Visible = visible;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx) + " type is not supported", typeGfx, null);
            }

            using (Stream m = new Crypt().DecryptFile(Path, Key, IV))
            {
                Bitmap tmp = new Bitmap(m);
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    Bitmap = new Bitmap(new Bitmap(m), size);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                tmp.Dispose();
            }

            Rectangle = new Rectangle(new Point(0, 0), size);
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="size">size is defined by it's width value and height value, making a image expanded</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="opacity">Float value indicating the transparency of the object, ex if value equal 0F then the object is invisible, if equal to 0.5F the transparent by 50%, if equal to 1F then completly opaque</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, Size size, string name, TypeGfx typeGfx, bool visible, float opacity, Manager manager)
        {
            Path = bitmap;
            Point = point;
            Name = name;
            Visible = visible;
            Opacity = opacity;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            using (Bitmap tmp = new Bitmap(Path))
            {
                Bitmap = ManagerInstance.Opacity(new Bitmap(tmp, size), opacity);
                if (tmp.RawFormat.Equals(ImageFormat.Gif))
                    ImageAnimator.Animate(Bitmap, null);
            }
            Rectangle = new Rectangle(new Point(0, 0), Bitmap.Size);
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="size">size is defined by it's width value and height value, making a image expanded</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="opacity">Float value indicating the transparency of the object, ex if value equal 0F then the object is invisible, if equal to 0.5F the transparent by 50%, if equal to 1F then completly opaque</param>
        /// <param name="Key">string value of 8 character, it's the first key of Rijndael AES encryption</param>
        /// <param name="IV">string value of 8 character, it's the second key of Rijndael AES encryption called Initializing Vector</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, Size size, string name, TypeGfx typeGfx, bool visible, float opacity, string Key, string IV, Manager manager)
        {
            Path = bitmap;
            Point = point;
            Name = name;
            Visible = visible;
            Opacity = opacity;
            Crypted = true;
            this.Key = Key;
            this.IV = IV;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            using (Bitmap tmp = new Bitmap(new Crypt().DecryptFile(Path, Key, IV)))
            {
                if (tmp.RawFormat.Equals(ImageFormat.Gif))
                    Bitmap = ManagerInstance.Opacity(new Bitmap(tmp, size), opacity);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }

            Rectangle = new Rectangle(new Point(0, 0), size);
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="opacity">Float value indicating the transparency of the object, ex if value equal 0F then the object is invisible, if equal to 0.5F the transparent by 50%, if equal to 1F then completly opaque</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, string name, TypeGfx typeGfx, bool visible, float opacity, Manager manager)
        {
            Path = bitmap;
            Point = point;
            Name = name;
            Visible = visible;
            Opacity = opacity;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            Bitmap = ManagerInstance.Opacity(new Bitmap(Path), opacity);
            if (Bitmap.RawFormat.Equals(ImageFormat.Gif))
                ImageAnimator.Animate(Bitmap, null);

            Rectangle = new Rectangle(new Point(0, 0), Bitmap.Size);
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="opacity">Float value indicating the transparency of the object, ex if value equal 0F then the object is invisible, if equal to 0.5F the transparent by 50%, if equal to 1F then completly opaque</param>
        /// <param name="rectangle">Rectangle is a square area defined by its point (X, Y) and its size (width, height) </param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, string name, TypeGfx typeGfx, bool visible, float opacity, Rectangle rectangle, Manager manager)
        {
            Path = bitmap;
            Point = point;
            Name = name;
            Visible = visible;
            Opacity = opacity;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            Bitmap = ManagerInstance.Opacity(new Bitmap(bitmap), opacity);
            if (Bitmap.RawFormat.Equals(ImageFormat.Gif))
                ImageAnimator.Animate(Bitmap, null);

            Rectangle = rectangle;
            IsSpriteSheet = true;
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="opacity">Float value indicating the transparency of the object, ex if value equal 0F then the object is invisible, if equal to 0.5F the transparent by 50%, if equal to 1F then completly opaque</param>
        /// <param name="Key">string value of 8 character, it's the first key of Rijndael AES encryption</param>
        /// <param name="IV">string value of 8 character, it's the second key of Rijndael AES encryption called Initializing Vector</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, string name, TypeGfx typeGfx, bool visible, float opacity, string Key, string IV, Manager manager)
        {
            Path = bitmap;
            Name = name;
            Visible = visible;
            Opacity = opacity;
            Point = point;
            Crypted = true;
            this.Key = Key;
            this.IV = IV;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            using (Stream m = new Crypt().DecryptFile(Path, Key, IV))
            {
                Bitmap = ManagerInstance.Opacity(new Bitmap(new Bitmap(m)), opacity);
                if (Bitmap.RawFormat.Equals(ImageFormat.Gif))
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }
            Rectangle = new Rectangle(new Point(0, 0), Bitmap.Size);
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="bitmap">Path of the bitmap as string</param>
        /// <param name="point">Point were the picture will be drawn on the form</param>
        /// <param name="name">String value as a name of the object</param>
        /// <param name="typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="opacity">Float value indicating the transparency of the object, ex if value equal 0F then the object is invisible, if equal to 0.5F the transparent by 50%, if equal to 1F then completly opaque</param>
        /// <param name="Key">string value of 8 character, it's the first key of Rijndael AES encryption</param>
        /// <param name="IV">string value of 8 character, it's the second key of Rijndael AES encryption called Initializing Vector</param>
        /// <param name="manager">Reference to the manager instance that hold this object</param>
        public Bmp(string bitmap, Point point, string name, TypeGfx typeGfx, bool visible, float opacity, string Key, string IV, Rectangle rectangle, Manager manager)
        {
            Path = bitmap;
            Name = name;
            Visible = visible;
            Opacity = opacity;
            Point = point;
            Crypted = true;
            this.Key = Key;
            this.IV = IV;
            ManagerInstance = manager;

            switch (typeGfx)
            {
                case TypeGfx.Background:
                    Zindex = ManagerInstance.ZOrder.Bgr();
                    TypeGfx = TypeGfx.Background;
                    break;
                case TypeGfx.Object:
                    Zindex = ManagerInstance.ZOrder.Obj();
                    TypeGfx = TypeGfx.Object;
                    break;
                case TypeGfx.Top:
                    Zindex = ManagerInstance.ZOrder.Top();
                    TypeGfx = TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            using (Stream m = new Crypt().DecryptFile(Path, Key, IV))
            {
                Bitmap = ManagerInstance.Opacity(new Bitmap(m), opacity);
                if (Bitmap.RawFormat.Equals(ImageFormat.Gif))
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }
            IsSpriteSheet = true;
            Rectangle = rectangle;
        }
        #endregion

        #region functions
        /// <summary>
        /// Changing the picture of the Bmp object
        /// </summary>
        /// <param name="bitmap">bitmap is the path of the image gived as a string</param>
        public void ChangeBmp(string bitmap)
        {
            try
            {
                Path = bitmap;
                Opacity = 1;
                Bitmap bmp2;    // clone pour eviter un probleme "l'objet est actuelement utilisé ailleur"
                if (Crypted)
                    using (Bitmap tmp = new Bitmap(new Crypt().DecryptFile(Path, Key, IV)))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp2 = new Bitmap(new Bitmap(tmp), Bitmap.Size);
                        else
                            throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                else
                    using (Bitmap tmp = new Bitmap(Path))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp2 = new Bitmap(new Bitmap(tmp), Bitmap.Size);
                        else
                        {
                            bmp2 = new Bitmap(Path);
                            ImageAnimator.Animate(bmp2, null);
                        }

                Bitmap = bmp2;
                Rectangle = new Rectangle(new Point(0, 0), Bitmap.Size);
                IsSpriteSheet = false;
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        /// <summary>
        /// Changing the picture of the Bmp object, this overload change the size of the picture too
        /// </summary>
        /// <param name="bitmap">bitmap is the path of the image gived as a string</param>
        /// <param name="size">Size is a value of Width and Height to resize the picture</param>
        public void ChangeBmp(string bitmap, Size size)
        {
            try
            {
                Path = bitmap;
                Opacity = 1;
                if (Crypted)
                    using (Bitmap tmp = new Bitmap(new Crypt().DecryptFile(Path, Key, IV)))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            Bitmap = new Bitmap(new Bitmap(tmp), size.Width, size.Height);
                        else
                            throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                else
                    using (Bitmap tmp = new Bitmap(Path))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            Bitmap = new Bitmap(new Bitmap(tmp), size.Width, size.Height);
                        else
                        {
                            Bitmap = new Bitmap(Path);
                            ImageAnimator.Animate(Bitmap, null);
                        }
                Rectangle = new Rectangle(new Point(0, 0), new Size(size.Width, size.Height));
                IsSpriteSheet = false;
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        /// <summary>
        /// Changing the picture of the Bmp object, and draw only a part of the image determined by a Point(X, Y) and a Size(Width, Height)
        /// </summary>
        /// <param name="bitmap">bitmap is the path of the image gived as a string</param>
        /// <param name="rectangle">Range of image to be drawn, Rectangle equal a given Point(X, Y) and Size(Width, Height)</param>
        public void ChangeBmp(string bitmap, Rectangle rectangle)
        {
            try
            {
                Path = bitmap;
                Opacity = 1;
                if (Crypted)
                    using (Bitmap tmp = new Bitmap(new Crypt().DecryptFile(Path, Key, IV)))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            Bitmap = new Bitmap(new Bitmap(tmp), Bitmap.Size);
                        else
                            throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                else
                    using (Bitmap tmp = new Bitmap(Path))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            Bitmap = new Bitmap(new Bitmap(tmp), Bitmap.Size);
                        else
                        {
                            Bitmap = new Bitmap(Path);
                            ImageAnimator.Animate(Bitmap, null);
                        }
                IsSpriteSheet = true;
                Rectangle = rectangle;
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        /// <summary>
        /// Changing the picture of the Bmp object, and it's opacity
        /// </summary>
        /// <param name="bitmap">bitmap is the path of the image gived as a string</param>
        /// <param name="opacity">Float value, if equal 0F the image is invisible, if equal to 0.5F then the image is transparent by 50%, if equal to 1F then image is opaque 100% visible</param>
        public void ChangeBmp(string bitmap, float opacity)
        {
            try
            {
                Path = bitmap;
                Opacity = opacity;
                if (Crypted)
                    using (Bitmap tmp = new Bitmap(new Crypt().DecryptFile(Path, Key, IV)))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            Bitmap = ManagerInstance.Opacity(new Bitmap(tmp, Bitmap.Size), opacity);
                        else
                            throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                else
                    using (Bitmap tmp = new Bitmap(Path))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            Bitmap = ManagerInstance.Opacity(new Bitmap(tmp, Bitmap.Size), opacity);
                        else
                        {
                            Bitmap = new Bitmap(Path);
                            ImageAnimator.Animate(Bitmap, null);
                        }
                Rectangle = new Rectangle(new Point(0, 0), Bitmap.Size);
                IsSpriteSheet = false;
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        /// <summary>
        /// Changing the picture of the Bmp object, and it's opacity and range to be drawn
        /// </summary>
        /// <param name="bitmap">bitmap is the path of the image gived as a string</param>
        /// <param name="opacity">Float value, if equal 0F the image is invisible, if equal to 0.5F then the image is transparent by 50%, if equal to 1F then image is opaque 100% visible</param>
        /// <param name="rectangle">Range of image to be drawn, Rectangle equal a given Point(X, Y) and Size(Width, Height)</param>
        public void ChangeBmp(string bitmap, float opacity, Rectangle rectangle)
        {
            try
            {
                Path = bitmap;
                Opacity = opacity;
                if (Crypted)
                    using (Bitmap tmp = new Bitmap(new Crypt().DecryptFile(Path, Key, IV)))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            Bitmap = ManagerInstance.Opacity(new Bitmap(tmp, Bitmap.Size), opacity);
                        else
                            throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                else
                    using (Bitmap tmp = new Bitmap(Path))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            Bitmap = ManagerInstance.Opacity(new Bitmap(tmp, Bitmap.Size), opacity);
                        else
                        {
                            Bitmap = new Bitmap(Path);
                            ImageAnimator.Animate(Bitmap, null);
                        }
                IsSpriteSheet = true;
                Rectangle = rectangle;
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }
        #endregion

        private void ExceptionHandler(Exception ex)
        {
            ManagerInstance.OutputErrorCallBack(ex.ToString());
        }

        /// <summary>
        /// Create a perfect duplication of the Bmp object
        /// </summary>
        /// <returns>Return an object of Bmp, you need a cast to Bmp type</returns>
        public object Clone() => MemberwiseClone();
    }
}
