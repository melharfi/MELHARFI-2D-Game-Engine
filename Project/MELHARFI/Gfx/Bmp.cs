using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace MELHARFI.Gfx
{
    /// <summary>
    /// Bmp class store information about image to be drawn
    /// </summary>
    public class Bmp : IGfx
    {
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
        /// useless, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseDoubleClic = false;
        public bool EscapeGfxWhileMouseClic = false;
        public bool EscapeGfxWhileMouseOver = false;
        public bool EscapeGfxWhileMouseDown = false;
        public bool EscapeGfxWhileMouseUp = false;
        public bool EscapeGfxWhileMouseMove = false;
        public bool EscapeGfxWhileKeyDown = false;

        /// <summary>
        /// Layer holding sub graphics that is shown in the front of the parent, and its position is relative to parent
        /// </summary>
        public List<IGfx> Child = new List<IGfx>();

        /// <summary>
        /// Layer to memorise all sequaces of rectangles of a sprite sheet, it's only a user side, and not a engine mechanisme
        /// </summary>
        public List<Rectangle> SpriteSheet = new List<Rectangle>();

        /// <summary>
        /// Bitmap image
        /// </summary>
        public Bitmap bmp;

        /// <summary>
        /// path to the given image
        /// </summary>
        public string path;

        /// <summary>
        /// point where the image will be drawn on the form, Default is X = 0, Y = 0;
        /// </summary>
        public Point point = Point.Empty;

        /// <summary>
        /// Name of the object, usfull if you need to search it by its name
        /// </summary>
        public string name;

        /// <summary>
        /// Zindex is a int value indicating the deep of the object against the other object in same graphic layer, default is 1
        /// </summary>
        public int zindex = 1;

        /// <summary>
        /// Boolean, True make the object visible, False the object is invisible, default is True
        /// </summary>
        public bool visible = true;

        /// <summary>
        /// Float, Opacity is the transparency of the object, 1F = full opaque (100% visible), 0.5F is 50% visible, 0F is invisible, default is 1 (100% visible)
        /// </summary>
        public float Opacity = 1;

        /// <summary>
        /// Hold which layer the object is stored
        /// </summary>
        public MELHARFI.Manager.TypeGfx TypeGfx = MELHARFI.Manager.TypeGfx.Bgr;

        /// <summary>
        /// tag object to assigne to whatever you want
        /// </summary>
        public Object tag;

        /// <summary>
        /// Boolean value indicating if the picture is encrypted or not
        /// </summary>
        public bool Crypted;

        /// <summary>
        /// Rectangle value representing a point and size, used in a spritesheet image
        /// </summary>
        private Rectangle _rectangle;

        /// <summary>
        /// newColorMap is a ColorMap object, used to convert a color from one color to another in the all picture, usfull to paint some area like changing color of eyes, skin, hair
        /// </summary>
        public ColorMap[] newColorMap;

        /// <summary>
        /// Byte value between 0 until 255, it's the value of encrypting algorithme
        /// </summary>
        public byte Crypt;

        /// <summary>
        /// Rectangle is an area of an image, used to show only a part of image, a Rectangle need a Point value defined by a X position and Y position, and Size value defined by a Width and Height
        /// </summary>
        public Rectangle rectangle
        {
            get { return _rectangle; }
            set { _rectangle = value; }
        }

        /// <summary>
        /// Flag equal true when the object show only a rectangle piece of the image (SpriteSheet), if false, then all the image is displayed
        /// </summary>
        public bool isSpriteSheet => IsSpriteSheet;

        private bool IsSpriteSheet;

        /// <summary>
        /// String value to store the name of the object, useful if you need to look for it in the appropriate layer, value is read only
        /// </summary>
        /// <returns>Return a string value as a name of the object</returns>
        public string Name()
        {
            // read only
            return name;
        }

        /// <summary>
        /// Tag is an object that you can affect to anything you want, usful to attach it to a class that hold some statistic to a players, Pay attention that you should cast the object
        /// </summary>
        /// <returns>Return Object</returns>
        public object Tag()
        {
            // read only
            return tag;
        }

        /// <summary>
        /// Boolean value indicating if the object is visible or invisible, value read only
        /// </summary>
        /// <returns>Return a boolean value</returns>
        public bool Visible()
        {
            // read only
            return visible;
        }

        /// <summary>
        /// Boolean value to change the state of object between visible or invisible
        /// </summary>
        /// <param name="_visible">If _visible is true, the object is visible, if false the object is invisible</param>
        public void Visible(bool _visible)
        {
            // write only
            visible = _visible;
        }

        /// <summary>
        /// Zindex is a int value indicating the deep of the object against the other object in same graphic layer, default is 1 , read only
        /// </summary>
        /// <returns>Return an int value </returns>
        public int Zindex()
        {
            // read only
            return zindex;
        }

        /// <summary>
        /// Zindex is a int value indicating the deep of the object against the other objects in same graphic layer, this function allow you to modify the Zindex value, read mode
        /// </summary>
        /// <param name="i">i is an int value for the zindex</param>
        public void Zindex(int i)
        {
            // write only
            zindex = i;
        }

        /// <summary>
        /// Empty Bmp constructor for a bitmap, Pay attention that you should initialise the other parameters later
        /// </summary>
        public Bmp()
        {
            // constructeur vide qui sert a initialiser l'objet Bmp avant de connaitre l'image qui va l'occuper
            // vus que ce n'est pas possible d'initialiser un objet Bitmap sans connaitre la source
            // cela instancie la classe Bmp avant, et affecter une image apres
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        public Bmp(string _bmp, Point _point)
        {
            path = _bmp;
            point = _point;
            bmp = new Bitmap(path);
            if (bmp.RawFormat.Equals(ImageFormat.Gif))
                ImageAnimator.Animate(bmp, null);
            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_rectangle">Rectangle is a square area defined by its point (X, Y) and its size (width, height) </param>
        public Bmp(string _bmp, Point _point, Rectangle _rectangle)
        {
            path = _bmp;
            point = _point;
            bmp = new Bitmap(path);
            using (Bitmap tmp = new Bitmap(path))
                if (tmp.RawFormat.Equals(ImageFormat.Gif))
                {
                    bmp = new Bitmap(new Bitmap(path), tmp.Width, tmp.Height);
                    ImageAnimator.Animate(bmp, null);
                }

            rectangle = _rectangle;
            IsSpriteSheet = true;
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="crypt">Byte value must be between 0 until 255 represent a value for decrypting the picture </param>
        public Bmp(string _bmp, Point _point, byte crypt)
        {
            path = _bmp;
            point = _point;
            Crypted = true;
            Crypt = crypt;

            using (MemoryStream m = new MemoryStream(Cryptography.DecryptFile(path, crypt)))
            {
                Bitmap tmp = new Bitmap(m);
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    bmp = new Bitmap(new Bitmap(m), tmp.Width, tmp.Height);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                tmp.Dispose();
            }
            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_rectangle">Rectangle is a square area defined by its point (X, Y) and its size (width, height) </param>
        public Bmp(string _bmp, Point _point, byte crypt, Rectangle _rectangle)
        {
            path = _bmp;
            point = _point;
            Crypted = true;
            Crypt = crypt;

            using (MemoryStream m = new MemoryStream(Cryptography.DecryptFile(path, crypt)))
            {
                Bitmap tmp = new Bitmap(m);
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    bmp = new Bitmap(new Bitmap(m), tmp.Width, tmp.Height);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                tmp.Dispose();
            }

            rectangle = _rectangle;
            IsSpriteSheet = true;
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_size">_size is defined by it's width value and height value, making a image expanded</param>
        public Bmp(string _bmp, Point _point, Size _size)
        {
            path = _bmp;
            point = _point;
            using (Bitmap tmp = new Bitmap(path))
                if (tmp.RawFormat.Equals(ImageFormat.Gif))
                {
                    // si ce constructeur n'affiche pas une image gif avec just le rectangle défini il faut utiliser juste bmp = new Bitmap(path); qui ne prend pas le rectangle en considération
                    //bmp = new Bitmap(path);
                    bmp = new Bitmap(new Bitmap(path), _size);
                    ImageAnimator.Animate(bmp, null);
                }
                else
                    bmp = new Bitmap(new Bitmap(path), _size);
            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_size">_size is defined by it's width value and height value, making a image expanded</param>
        public Bmp(Bitmap _bmp, Point _point, Size _size)
        {
            // generalement utilisé pour redimentionner une image pris depuis un spritesheet
            point = _point;
            if (bmp.RawFormat.Equals(ImageFormat.Gif))
            {
                bmp = new Bitmap(_bmp, _size);
                ImageAnimator.Animate(bmp, null);
            }
            else
                bmp = new Bitmap(_bmp, _size);
            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_size">_size is defined by it's width value and height value, making a image expanded</param>
        public Bmp(string _bmp, Point _point, Size _size, byte crypt)
        {
            path = _bmp;
            Crypted = true;
            Crypt = crypt;
            point = _point;

            using (MemoryStream m = new MemoryStream(Cryptography.DecryptFile(path, crypt)))
            {
                Bitmap tmp = new Bitmap(m);
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    bmp = new Bitmap(new Bitmap(m), _size);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                tmp.Dispose();
            }
            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        public Bmp(string _bmp, Point _point, string _name, MELHARFI.Manager.TypeGfx _typeGfx, bool _visible)
        {
            path = _bmp;
            point = _point;
            name = _name;
            visible = _visible;

            switch (_typeGfx)
            {
                case MELHARFI.Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI.Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI.Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI.Manager.TypeGfx.Obj;
                    break;
                case MELHARFI.Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI.Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx), _typeGfx, null);
            }

            using (Bitmap tmp = new Bitmap(path))
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    bmp = new Bitmap(tmp, tmp.Width, tmp.Height);
                else
                {
                    bmp = new Bitmap(path);
                    ImageAnimator.Animate(bmp, null);
                }

            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="_rectangle">Rectangle is a square area defined by its point (X, Y) and its size (width, height) </param>
        public Bmp(string _bmp, Point _point, string _name, MELHARFI.Manager.TypeGfx _typeGfx, bool _visible, Rectangle _rectangle)
        {
            path = _bmp;
            point = _point;
            name = _name;
            visible = _visible;
            rectangle = _rectangle;
            IsSpriteSheet = true;

            switch (_typeGfx)
            {
                case MELHARFI. Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI. Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Obj;
                    break;
                case MELHARFI. Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx), _typeGfx, null);
            }

            using (Bitmap tmp = new Bitmap(path))
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    bmp = new Bitmap(tmp, tmp.Width, tmp.Height);
                else
                {
                    bmp = new Bitmap(path);
                    ImageAnimator.Animate(bmp, null);
                }
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        public Bmp(string _bmp, Point _point, string _name, MELHARFI. Manager.TypeGfx _typeGfx, bool _visible, byte crypt)
        {
            // instancier sans spésification de la taille, (taille d'origine de l'image)
            path = _bmp;
            point = _point;
            name = _name;
            visible = _visible;
            Crypted = true;
            Crypt = crypt;

            switch (_typeGfx)
            {
                case MELHARFI. Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI. Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Obj;
                    break;
                case MELHARFI. Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx), _typeGfx, null);
            }

            using (MemoryStream m = new MemoryStream(Cryptography.DecryptFile(path, crypt)))
            {
                Bitmap tmp = new Bitmap(m);
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    bmp = new Bitmap(new Bitmap(m), tmp.Width, tmp.Height);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }

            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        public Bmp(string _bmp, Point _point, string _name, MELHARFI. Manager.TypeGfx _typeGfx, bool _visible, byte crypt, Rectangle _rectangle)
        {
            // instancier sans spésification de la taille, (taille d'origine de l'image)
            path = _bmp;
            point = _point;
            name = _name;
            visible = _visible;
            Crypted = true;
            Crypt = crypt;
            rectangle = _rectangle;
            IsSpriteSheet = true;

            switch (_typeGfx)
            {
                case MELHARFI. Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI. Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Obj;
                    break;
                case MELHARFI. Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx), _typeGfx, null);
            }

            using (MemoryStream m = new MemoryStream(Cryptography.DecryptFile(path, crypt)))
            {
                Bitmap tmp = new Bitmap(m);
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    bmp = new Bitmap(new Bitmap(m), tmp.Width, tmp.Height);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                tmp.Dispose();
            }
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_size">_size is defined by it's width value and height value, making a image expanded</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        public Bmp(string _bmp, Point _point, Size _size, string _name, MELHARFI. Manager.TypeGfx _typeGfx, bool _visible)
        {
            path = _bmp;
            name = _name;
            visible = _visible;
            point = _point;

            switch (_typeGfx)
            {
                case MELHARFI. Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI. Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Obj;
                    break;
                case MELHARFI. Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx), _typeGfx, null);
            }

            using (Bitmap tmp = new Bitmap(path))
            {
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    bmp = new Bitmap(new Bitmap(path), _size);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }
            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_size">_size is defined by it's width value and height value, making a image expanded</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        public Bmp(string _bmp, Point _point, Size _size, string _name, MELHARFI. Manager.TypeGfx _typeGfx, bool _visible, byte crypt)
        {
            path = _bmp;
            Crypted = true;
            Crypt = crypt;
            point = _point;
            name = _name;
            visible = _visible;

            switch (_typeGfx)
            {
                case MELHARFI. Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI. Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Obj;
                    break;
                case MELHARFI. Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx) + " type is not supported", _typeGfx, null);
            }

            using (MemoryStream m = new MemoryStream(Cryptography.DecryptFile(path, crypt)))
            {
                Bitmap tmp = new Bitmap(m);
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    bmp = new Bitmap(new Bitmap(m), _size);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                tmp.Dispose();
            }

            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_size">_size is defined by it's width value and height value, making a image expanded</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="opacity">Float value indicating the transparency of the object, ex if value equal 0F then the object is invisible, if equal to 0.5F the transparent by 50%, if equal to 1F then completly opaque</param>
        public Bmp(string _bmp, Point _point, Size _size, string _name, MELHARFI. Manager.TypeGfx _typeGfx, bool _visible, float opacity)
        {
            path = _bmp;
            point = _point;
            name = _name;
            visible = _visible;
            Opacity = opacity;

            switch (_typeGfx)
            {
                case MELHARFI. Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI. Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Obj;
                    break;
                case MELHARFI. Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx), _typeGfx, null);
            }

            using (Bitmap tmp = new Bitmap(path))
                if (tmp.RawFormat.Equals(ImageFormat.Gif))
                {
                    bmp = new Bitmap(path);
                    ImageAnimator.Animate(bmp, null);
                }
                else
                    bmp = MELHARFI.Manager.ChangeOpacity(new Bitmap(new Bitmap(path), _size), opacity);

            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_size">_size is defined by it's width value and height value, making a image expanded</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        public Bmp(string _bmp, Point _point, Size _size, string _name, MELHARFI. Manager.TypeGfx _typeGfx, bool _visible, float opacity, byte crypt)
        {
            path = _bmp;
            point = _point;
            name = _name;
            visible = _visible;
            Opacity = opacity;
            Crypted = true;
            Crypt = crypt;

            switch (_typeGfx)
            {
                case MELHARFI. Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI. Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Obj;
                    break;
                case MELHARFI. Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx), _typeGfx, null);
            }

            using (Bitmap tmp = new Bitmap(new MemoryStream(Cryptography.DecryptFile(path, crypt))))
            {
                if (tmp.RawFormat.Equals(ImageFormat.Gif))
                    bmp = MELHARFI.Manager.ChangeOpacity(new Bitmap(new Bitmap(tmp, tmp.Width, tmp.Height), _size), opacity);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }

            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="opacity">Float value indicating the transparency of the object, ex if value equal 0F then the object is invisible, if equal to 0.5F the transparent by 50%, if equal to 1F then completly opaque</param>
        public Bmp(string _bmp, Point _point, string _name, MELHARFI. Manager.TypeGfx _typeGfx, bool _visible, float opacity)
        {
            path = _bmp;
            Bitmap tmp = new Bitmap(path);
            point = _point;
            name = _name;
            visible = _visible;
            Opacity = opacity;

            switch (_typeGfx)
            {
                case MELHARFI. Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI. Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Obj;
                    break;
                case MELHARFI. Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx), _typeGfx, null);
            }

            if (tmp.RawFormat.Equals(ImageFormat.Gif))
            {
                bmp = new Bitmap(path);
                ImageAnimator.Animate(bmp, null);
            }
            else
                bmp = MELHARFI.Manager.ChangeOpacity(new Bitmap(new Bitmap(path), tmp.Width, tmp.Height), opacity);

            tmp.Dispose();
            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="opacity">Float value indicating the transparency of the object, ex if value equal 0F then the object is invisible, if equal to 0.5F the transparent by 50%, if equal to 1F then completly opaque</param>
        /// <param name="_rectangle">Rectangle is a square area defined by its point (X, Y) and its size (width, height) </param>
        public Bmp(string _bmp, Point _point, string _name, MELHARFI. Manager.TypeGfx _typeGfx, bool _visible, float opacity, Rectangle _rectangle)
        {
            path = _bmp;
            Bitmap tmp = new Bitmap(path);
            point = _point;
            name = _name;
            visible = _visible;
            Opacity = opacity;

            switch (_typeGfx)
            {
                case MELHARFI. Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI. Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Obj;
                    break;
                case MELHARFI. Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx), _typeGfx, null);
            }

            if (tmp.RawFormat.Equals(ImageFormat.Gif))
            {
                bmp = new Bitmap(path);
                ImageAnimator.Animate(bmp, null);
            }
            else
                bmp = MELHARFI.Manager.ChangeOpacity(new Bitmap(new Bitmap(path), tmp.Width, tmp.Height), opacity);

            tmp.Dispose();
            rectangle = _rectangle;
            IsSpriteSheet = true;
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="opacity">Float value indicating the transparency of the object, ex if value equal 0F then the object is invisible, if equal to 0.5F the transparent by 50%, if equal to 1F then completly opaque</param>
        public Bmp(string _bmp, Point _point, string _name, MELHARFI. Manager.TypeGfx _typeGfx, bool _visible, float opacity, byte crypt)
        {
            path = _bmp;
            name = _name;
            visible = _visible;
            Opacity = opacity;
            point = _point;
            Crypted = true;
            Crypt = crypt;

            switch (_typeGfx)
            {
                case MELHARFI. Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI. Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Obj;
                    break;
                case MELHARFI. Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx), _typeGfx, null);
            }

            using (MemoryStream m = new MemoryStream(Cryptography.DecryptFile(path, crypt)))
            {
                Bitmap tmp = new Bitmap(m);
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                {
                    bmp = MELHARFI.Manager.ChangeOpacity(new Bitmap(tmp, tmp.Width, tmp.Height), opacity);
                    tmp.Dispose();
                }
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
            }

            rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
        }

        /// <summary>
        /// Bmp constructor for a bitmap
        /// </summary>
        /// <param name="_bmp">Path of the bitmap as string</param>
        /// <param name="_point">Point were the picture will be drawn on the form</param>
        /// <param name="_name">String value as a name of the object</param>
        /// <param name="_typeGfx">To record the type of the object, useful if you need to now in which layer is stored</param>
        /// <param name="_visible">Boolean value, if true the object is visible, if false the object is invisible</param>
        /// <param name="opacity">Float value indicating the transparency of the object, ex if value equal 0F then the object is invisible, if equal to 0.5F the transparent by 50%, if equal to 1F then completly opaque</param>
        public Bmp(string _bmp, Point _point, string _name, MELHARFI. Manager.TypeGfx _typeGfx, bool _visible, float opacity, byte crypt, Rectangle _rectangle)
        {
            path = _bmp;
            name = _name;
            visible = _visible;
            Opacity = opacity;
            point = _point;
            Crypted = true;
            Crypt = crypt;

            switch (_typeGfx)
            {
                case MELHARFI. Manager.TypeGfx.Bgr:
                    zindex = ZOrder.Bgr();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Bgr;
                    break;
                case MELHARFI. Manager.TypeGfx.Obj:
                    zindex = ZOrder.Obj();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Obj;
                    break;
                case MELHARFI. Manager.TypeGfx.Top:
                    zindex = ZOrder.Top();
                    TypeGfx = MELHARFI. Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_typeGfx), _typeGfx, null);
            }

            using (MemoryStream m = new MemoryStream(Cryptography.DecryptFile(path, crypt)))
            {
                Bitmap tmp = new Bitmap(m);
                if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                    bmp = MELHARFI.Manager.ChangeOpacity(new Bitmap(tmp, tmp.Width, tmp.Height), opacity);
                else
                    throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");

                tmp.Dispose();
            }
            IsSpriteSheet = true;
            rectangle = _rectangle;
        }

        /// <summary>
        /// Changing the picture of the Bmp object
        /// </summary>
        /// <param name="_bmp">_bmp is the path of the image gived as a string</param>
        public void ChangeBmp(string _bmp)
        {
            try
            {
                path = _bmp;
                Opacity = 1;
                Bitmap bmp2;    // clone pour eviter un probleme "lobjet est actuelement utilisé ailleur"
                if (Crypted)
                    using (Bitmap tmp = new Bitmap(new MemoryStream(Cryptography.DecryptFile(path, Crypt))))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp2 = new Bitmap(new Bitmap(tmp), tmp.Width, tmp.Height);
                        else
                            throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                else
                    using (Bitmap tmp = new Bitmap(path))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp2 = new Bitmap(new Bitmap(tmp), tmp.Width, tmp.Height);
                        else
                        {
                            bmp2 = new Bitmap(path);
                            ImageAnimator.Animate(bmp2, null);
                        }

                bmp = bmp2;
                rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
                IsSpriteSheet = false;
            }
            catch (Exception ex)
            {
                if (MELHARFI.Manager.OutputErrorCallBack != null)
                    MELHARFI.Manager.OutputErrorCallBack("Error Changing Bitmap \n" + ex);
                else if (MELHARFI.Manager.ShowErrorsInMessageBox)
                    MessageBox.Show("error Changing Bitmap \n" + ex);
            }
        }

        /// <summary>
        /// Changing the picture of the Bmp object, this overload change the size of the picture too
        /// </summary>
        /// <param name="_bmp">_bmp is the path of the image gived as a string</param>
        /// <param name="_size">Size is a value of Width and Height to resize the picture</param>
        public void ChangeBmp(string _bmp, Size _size)
        {
            try
            {
                path = _bmp;
                Opacity = 1;
                if (Crypted)
                    using (Bitmap tmp = new Bitmap(new MemoryStream(Cryptography.DecryptFile(path, Crypt))))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp = new Bitmap(new Bitmap(tmp), _size.Width, _size.Height);
                        else
                            throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                else
                    using (Bitmap tmp = new Bitmap(path))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp = new Bitmap(new Bitmap(tmp), _size.Width, _size.Height);
                        else
                        {
                            bmp = new Bitmap(path);
                            ImageAnimator.Animate(bmp, null);
                        }
                rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
                IsSpriteSheet = false;
            }
            catch (Exception ex)
            {
                if (MELHARFI.Manager.OutputErrorCallBack != null)
                    MELHARFI.Manager.OutputErrorCallBack("Error Changing Bitmap \n" + ex);
                else if (MELHARFI.Manager.ShowErrorsInMessageBox)
                    MessageBox.Show("error Changing Bitmap \n" + ex);
            }
        }

        /// <summary>
        /// Changing the picture of the Bmp object, and draw only a part of the image determined by a Point(X, Y) and a Size(Width, Height)
        /// </summary>
        /// <param name="_bmp">_bmp is the path of the image gived as a string</param>
        /// <param name="_rectangle">Range of image to be drawn, Rectangle equal a given Point(X, Y) and Size(Width, Height)</param>
        public void ChangeBmp(string _bmp, Rectangle _rectangle)
        {
            try
            {
                path = _bmp;
                Opacity = 1;
                if (Crypted)
                    using (Bitmap tmp = new Bitmap(new MemoryStream(Cryptography.DecryptFile(path, Crypt))))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp = new Bitmap(new Bitmap(tmp), tmp.Width, tmp.Height);
                        else
                            throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                else
                    using (Bitmap tmp = new Bitmap(path))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp = new Bitmap(new Bitmap(tmp), tmp.Width, tmp.Height);
                        else
                        {
                            bmp = new Bitmap(path);
                            ImageAnimator.Animate(bmp, null);
                        }
                IsSpriteSheet = true;
                rectangle = _rectangle;
            }
            catch (Exception ex)
            {
                if (MELHARFI.Manager.OutputErrorCallBack != null)
                    MELHARFI.Manager.OutputErrorCallBack("Error Changing Bitmap \n" + ex);
                else if (MELHARFI.Manager.ShowErrorsInMessageBox)
                    MessageBox.Show("error Changing Bitmap \n" + ex);
            }
        }

        /// <summary>
        /// Changing the picture of the Bmp object, and it's opacity
        /// </summary>
        /// <param name="_bmp">_bmp is the path of the image gived as a string</param>
        /// <param name="opacity">Float value, if equal 0F the image is invisible, if equal to 0.5F then the image is transparent by 50%, if equal to 1F then image is opaque 100% visible</param>
        public void ChangeBmp(string _bmp, float opacity)
        {
            try
            {
                path = _bmp;
                Opacity = opacity;
                if (Crypted)
                    using (Bitmap tmp = new Bitmap(new MemoryStream(Cryptography.DecryptFile(path, Crypt))))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp = MELHARFI.Manager.ChangeOpacity(new Bitmap(tmp, tmp.Width, tmp.Height), opacity);
                        else
                            throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                else
                    using (Bitmap tmp = new Bitmap(path))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp = MELHARFI.Manager.ChangeOpacity(new Bitmap(tmp, tmp.Width, tmp.Height), opacity);
                        else
                        {
                            bmp = new Bitmap(path);
                            ImageAnimator.Animate(bmp, null);
                        }
                rectangle = new Rectangle(new Point(0, 0), new Size(bmp.Width, bmp.Height));
                IsSpriteSheet = false;
            }
            catch (Exception ex)
            {
                if (MELHARFI.Manager.OutputErrorCallBack != null)
                    MELHARFI.Manager.OutputErrorCallBack("Error Changing Bitmap \n" + ex);
                else if (MELHARFI.Manager.ShowErrorsInMessageBox)
                    MessageBox.Show("error Changing Bitmap \n" + ex);
            }
        }

        /// <summary>
        /// Changing the picture of the Bmp object, and it's opacity and range to be drawn
        /// </summary>
        /// <param name="_bmp">_bmp is the path of the image gived as a string</param>
        /// <param name="opacity">Float value, if equal 0F the image is invisible, if equal to 0.5F then the image is transparent by 50%, if equal to 1F then image is opaque 100% visible</param>
        /// <param name="_rectangle">Range of image to be drawn, Rectangle equal a given Point(X, Y) and Size(Width, Height)</param>
        public void ChangeBmp(string _bmp, float opacity, Rectangle _rectangle)
        {
            try
            {
                path = _bmp;
                Opacity = opacity;
                if (Crypted)
                    using (Bitmap tmp = new Bitmap(new MemoryStream(Cryptography.DecryptFile(path, Crypt))))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp = MELHARFI.Manager.ChangeOpacity(new Bitmap(tmp, tmp.Width, tmp.Height), opacity);
                        else
                            throw new Exception("le cryptage d'une image Gif n'est pas possible vus qu'il provoque un probleme generique en GDI+");
                else
                    using (Bitmap tmp = new Bitmap(path))
                        if (!tmp.RawFormat.Equals(ImageFormat.Gif))
                            bmp = MELHARFI.Manager.ChangeOpacity(new Bitmap(tmp, tmp.Width, tmp.Height), opacity);
                        else
                        {
                            bmp = new Bitmap(path);
                            ImageAnimator.Animate(bmp, null);
                        }
                IsSpriteSheet = true;
                rectangle = _rectangle;
            }
            catch (Exception ex)
            {
                if (MELHARFI.Manager.OutputErrorCallBack != null)
                    MELHARFI.Manager.OutputErrorCallBack("Error Changing Bitmap \n" + ex);
                else if (MELHARFI.Manager.ShowErrorsInMessageBox)
                    MessageBox.Show("error Changing Bitmap \n" + ex);
            }
        }

        /// <summary>
        /// To fire the Mouse Double Clic event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        public void FireMouseDoubleClic(MouseEventArgs e) => MouseDoubleClic?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Clic event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        public void FireMouseClic(MouseEventArgs e) => MouseClic?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Down event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        public void FireMouseDown(MouseEventArgs e) => MouseDown?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Up event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        public void FireMouseUp(MouseEventArgs e) => MouseUp?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Move event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        public void FireMouseMove(MouseEventArgs e) => MouseMove?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Over event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        public void FireMouseOver(MouseEventArgs e) => MouseOver?.Invoke(this, e);

        /// <summary>
        /// To fire the Mouse Out event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        public void FireMouseOut(MouseEventArgs e)
        {
            if (MouseOut == null) return;
            MELHARFI.Manager.manager.GfxMouseOverList.Remove(this);  // pour que MouseOut ne cherche pas sur un Gfx qui n'est pas sur le devant
            MouseOut(this, e);
        }

        /// <summary>
        /// Create a perfect duplication of the Bmp object
        /// </summary>
        /// <returns>Return an object of Bmp, you need a cast to Bmp type</returns>
        public object Clone() => MemberwiseClone();
    }
}
