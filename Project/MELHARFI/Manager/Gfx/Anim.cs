using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Timers;
using MELHARFI;
using static MELHARFI.Manager.Manager;

namespace MELHARFI.Manager.Gfx
{
    public class Anim : IGfx
    {
        #region Properties
        struct AnimCell
        {
            public string cell;
            public int id;
            public int posX;
            public int posY;
            public float Opacity;
            public Bitmap bitmap;
            public int interval;
            public Rectangle rec;
        }

        readonly Timer aTimer;      // notre timer
        readonly List<AnimCell> cellList = new List<AnimCell>();

        /// <summary>
        /// Bitmap object, it's the image parent and the first one to be shown in the animation
        /// </summary>
        public Bmp Bmp;                  // image bmp qui represente l'image parent ou image en cours de l'anim

        /// <summary>
        /// Counter is an int value to indicate how many sprites
        /// </summary>
        public int Counter;

        /// <summary>
        /// Bool value, if true the animation will loop infinitely, if false the animation will close in the end
        /// </summary>
        public bool AutoResetAnim = true;       // fait en sorte que l'anim se relance apres sa fin

        /// <summary>
        /// Boolean value indicating if the picture is encrypted or not
        /// </summary>
        public bool Encrypted;

        /// <summary>
        /// key is a string of 8 characters (first key) for Rijndael AES encryption
        /// </summary>
        public string Key;

        /// <summary>
        /// iv is string of 8 characters called Initializing Vector (second key) for Rijndael AES encryption
        /// </summary>
        public string IV;

        public List<IGfx> Childs { get; set; } = new List<IGfx>();

        /// <summary>
        /// Make the animation run by a reversed mode
        /// </summary>
        public bool Reverse = false;

        /// <summary>
        /// Hold which layer the object is stored
        /// </summary>
        public Manager.TypeGfx TypeGfx;

        /// <summary>
        /// If true, all frame will have the same point as the parent picture, if false, each frame take its self point
        /// </summary>
        public bool PointOfParent = false;             // si false, l'anim prend la position de chaque itération mémorisé dans la structure, si true elle prend la position de l'image parent, utile pour faire bouger une anim selon la logique, 

        bool autoResetTimer;                    // relance le timer si il a été fini, un peut pareil que AutoResetAnim, mais a la difference que cela va afficher seulement la 1ere instance de l'anim et s'arreter

        /// <summary>
        /// Boolean Value, if true the animation will stop but keep showing the last frame to the screen
        /// </summary>
        public bool AutoResetTimer
        {
            get { return autoResetTimer; }
            set { autoResetTimer = value; aTimer.AutoReset = autoResetTimer; }
        }

        //public bool RectangleOfParent = false;          // a utiliser si l'animation dois utiliser les attributs Rectangles (pointe, size) de chaque itération de l'animation, si true, les attributs original de la 1ere occurance

        /// <summary>
        /// Make the animation hide the last frame when it's finished
        /// </summary>
        public bool HideAtLastFrame = false;            // cache l'animation apres sa fin

        /// <summary>
        /// String value to store the name of the object, useful if you need to look for it in the appropriate layer, value is read only
        /// </summary>
        /// <returns>Return a string value as a name of the object</returns>
        public string Name { get; set; }

        public Point Point
        {
            get => Bmp.Point;
            set => Bmp.Point = value;
        }

        /// <summary>
        /// Hold a reference of the parent manager instance, requird to access to parent objetcs
        /// </summary>
        public Manager ManagerInstance { get; set; }

        /// <summary>
        /// Tag is an object that you can affect to anything you want, usful to attach it to a class that hold some statistic to a players, Pay attention that you should cast the object
        /// </summary>
        /// <returns>Return Object</returns>
        public object Tag { get; set; }

        /// <summary>
        /// Boolean value indicating if the object is visible or invisible, value read only
        /// </summary>
        /// <returns>Return a boolean value</returns>
        public bool Visible
        {
            get { return Bmp.Visible; }
            set { Bmp.Visible = value; }
        }

        /// <summary>
        /// Zindex is a int value indicating the deep of the object against the other object in same graphic layer, default is 1 , read only
        /// </summary>
        /// <returns>Return an int value</returns>
        public int Zindex
        {
            get { return Bmp.Zindex; }
            set { Bmp.Zindex = value; }
        }

        public bool DestroyAfterLastFrame = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Anim constructor
        /// </summary>
        /// <param name="interval">Int value indicating time between each frame</param>
        /// <param name="Key">string value of 8 character, it's the first key of Rijndael AES encryption</param>
        /// <param name="IV">string value of 8 character, it's the second key of Rijndael AES encryption called Initializing Vector</param>
        /// <param name="manager">Reference of Manager instance that hold this object</param>
        public Anim(int interval, string Key, string IV, Manager manager)
        {
            Encrypted = true;
            this.Key = Key;
            this.IV = IV;
            Interval = interval;
            ManagerInstance = manager;

            aTimer = new Timer(interval) { AutoReset = true };
            aTimer.Elapsed += aTimer_Elapsed;
            aTimer.SynchronizingObject = null;   
        }

        /// <summary>
        /// Anim constructor
        /// </summary>
        /// <param name="interval">Int value indicating time between each frame</param>
        /// <param name="manager">Reference of Manager instance that hold this object</param>
        public Anim(double interval, Manager manager)
        {
            Encrypted = false;
            Interval = interval;
            ManagerInstance = manager;

            aTimer = new Timer(interval) { AutoReset = true };
            aTimer.Elapsed += aTimer_Elapsed;
            aTimer.SynchronizingObject = null;
        }

        /// <summary>
        /// Anim constructor
        /// </summary>
        /// <param name="Key">string value of 8 character, it's the first key of Rijndael AES encryption</param>
        /// <param name="IV">string value of 8 character, it's the second key of Rijndael AES encryption called Initializing Vector</param>
        /// <param name="manager">Reference of Manager instance that hold this object</param>
        public Anim(string Key, string IV, Manager manager)
        {
            // quand on aimerai attribuer un timer a chaque frame
            // a combiner avec un constructeur qui contiens le variable Interval :
            // public void AddCell(string asset, int id, int posX, int posY,int Interval)

            Encrypted = true;
            this.Key = Key;
            this.IV = IV;
            Interval = 50;
            ManagerInstance = manager;

            aTimer = new Timer(50) { AutoReset = true };
            aTimer.Elapsed += aTimer_Elapsed;
            aTimer.SynchronizingObject = null;
        }

        /// <summary>
        /// Empty anim constructor
        /// </summary>
        /// <param name="manager">Reference of Manager instance that hold this object</param>
        public Anim(Manager manager)
        {
            // quand on aimerai attribuer un timer a chaque frame
            // a combiner avec un constructeur qui contiens le variable Interval :
            // public void AddCell(string asset, int id, int posX, int posY,int Interval)

            Encrypted = false;
            Interval = 50;
            ManagerInstance = manager;

            aTimer = new Timer(50) { AutoReset = true };
            aTimer.Elapsed += aTimer_Elapsed;
            aTimer.SynchronizingObject = null;
        }
        #endregion

        #region Methodes
        /// <summary>
        /// Inititalisation method for the animation
        /// </summary>
        /// <param name="typeGfx">Hold which layer the object is stored</param>
        /// <param name="crypted">Boolean value indicating if the picture is encrypted or not</param>
        public void Ini(Manager.TypeGfx typeGfx, bool crypted)
        {
            switch (typeGfx)
            {
                case Manager.TypeGfx.Background:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Background, true, Key, IV, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Background, true, ManagerInstance);

                    Bmp.TypeGfx = Manager.TypeGfx.Background;
                    break;
                case Manager.TypeGfx.Object:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Object, true, Key, IV, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Object, true, ManagerInstance);
                    Bmp.TypeGfx = Manager.TypeGfx.Object;
                    break;
                case Manager.TypeGfx.Top:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Top, true, Key, IV, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Top, true, ManagerInstance);
                    Bmp.TypeGfx = Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }
        }

        /// <summary>
        /// Inititalisation method for the animation
        /// </summary>
        /// <param name="typeGfx">Hold which layer the object is stored</param>
        /// <param name="Name">Name of the object, usfull if you need to search it by its name</param>
        /// <param name="crypted">Boolean value indicating if the picture is encrypted or not</param>
        public void Ini(Manager.TypeGfx typeGfx, string Name, bool crypted)
        {
            switch (typeGfx)
            {
                case Manager.TypeGfx.Background:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Background, true, Key, IV, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Background, true, ManagerInstance);
                    Bmp.TypeGfx = Manager.TypeGfx.Background;
                    break;
                case Manager.TypeGfx.Object:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Object, true, Key, IV, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Background, true, ManagerInstance);
                    Bmp.TypeGfx = Manager.TypeGfx.Object;
                    break;
                case Manager.TypeGfx.Top:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Top, true, Key, IV, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Background, true, ManagerInstance);
                    Bmp.TypeGfx = Manager.TypeGfx.Top;
                    Bmp.Rectangle = cellList[Counter].rec;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }

            this.Name = Name;
            Bmp.Name = this.Name;
        }

        /// <summary>
        /// Inititalisation method for the animation
        /// </summary>
        /// <param name="typeGfx">Hold which layer the object is stored</param>
        /// <param name="rectangle">Rectangle value representing a point and size, used in a spritesheet image</param>
        /// <param name="crypted">Boolean value indicating if the picture is encrypted or not</param>
        public void Ini(Manager.TypeGfx typeGfx, Rectangle rectangle, bool crypted)
        {
            switch (typeGfx)
            {
                case Manager.TypeGfx.Background:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Background, true, Key, IV, rectangle, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Background, true, rectangle, ManagerInstance);
                    Bmp.Rectangle = cellList[0].rec;
                    Bmp.TypeGfx = Manager.TypeGfx.Background;
                    break;
                case Manager.TypeGfx.Object:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Object, true, Key, IV, rectangle, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Object, true, rectangle, ManagerInstance);
                    Bmp.Rectangle = cellList[0].rec;
                    Bmp.TypeGfx = Manager.TypeGfx.Object;
                    break;
                case Manager.TypeGfx.Top:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Top, true, Key, IV, rectangle, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), "unamedAnim", Manager.TypeGfx.Top, true, rectangle, ManagerInstance);
                    Bmp.Rectangle = cellList[0].rec;
                    Bmp.TypeGfx = Manager.TypeGfx.Top;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(typeGfx), typeGfx, null);
            }
        }

        /// <summary>
        /// Inititalisation method for the animation
        /// </summary>
        /// <param name="typeGfx">Hold which layer the object is stored</param>
        /// <param name="rectangle">Rectangle value representing a point and size, used in a spritesheet image</param>
        /// <param name="Name">Name of the object, usfull if you need to search it by its name</param>
        /// <param name="crypted">Boolean value indicating if the picture is encrypted or not</param>
        public void Ini(Manager.TypeGfx typeGfx, Rectangle rectangle, string Name, bool crypted)
        {
            switch (typeGfx)
            {
                case Manager.TypeGfx.Background:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Background, true, Key, IV, rectangle, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Background, true, rectangle, ManagerInstance);
                    Bmp.Rectangle = cellList[0].rec;
                    Bmp.TypeGfx = Manager.TypeGfx.Background;
                    break;
                case Manager.TypeGfx.Object:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Object, true, Key, IV, rectangle, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Object, true, rectangle, ManagerInstance);
                    Bmp.Rectangle = cellList[0].rec;
                    Bmp.TypeGfx = Manager.TypeGfx.Object;
                    break;
                case Manager.TypeGfx.Top:
                    Bmp = crypted ? new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Top, true, Key, IV, rectangle, ManagerInstance) : new Bmp(cellList[0].cell, new Point(cellList[Counter].posX, cellList[Counter].posY), Name, Manager.TypeGfx.Top, true, rectangle, ManagerInstance);
                    Bmp.Rectangle = cellList[0].rec;
                    Bmp.TypeGfx = Manager.TypeGfx.Top;
                    break;
            }
            this.Name = Name;
            Bmp.Name = this.Name;
        }

        /// <summary>
        /// Method to launch the animation
        /// </summary>
        public void Start()
        {
            if (Reverse)
                Counter = cellList.Count - 1;
            else
                Counter = 0;

            if (!Bmp.Bitmap.RawFormat.Equals(ImageFormat.Gif))
                aTimer.Start();
            else
                ImageAnimator.Animate(Bmp.Bitmap, null);
        }

        /// <summary>
        /// Boolean value, if true then the animation timer is enabled, if false then it's desabled
        /// </summary>
        /// <param name="e">e is state of the timer as bool</param>
        public bool Enabled
        {
            get
            {
                return aTimer.Enabled;
            }
            set
            {
                aTimer.Enabled = value;
            }
        }

        /// <summary>
        /// Close the time by calling the Stop method folowwed by the close method
        /// </summary>
        public void Close()
        {
            aTimer.Stop();
            aTimer.Close();
        }

        /// <summary>
        /// Int value indicating interval between each frame in millisecond
        /// </summary>
        /// <param name="i">i is an int value</param>

        public double Interval
        {
            get
            {
                return aTimer.Interval;
            }
            set
            {
                aTimer.Interval = value;
            }
        }

        private void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (aTimer)
            {
                if (!Reverse && AutoResetAnim && Counter == cellList.Count)
                    Counter = 0;
                else if (Reverse && AutoResetAnim && Counter < 0)
                    Counter = cellList.Count - 1;
                else if (!Reverse && !AutoResetAnim && Counter == cellList.Count)
                {
                    Close();
                    Bmp.Visible = !HideAtLastFrame;

                    if (DestroyAfterLastFrame)
                        destroyAfterLastFrame();
                    return;
                }
                else if (Reverse && !AutoResetAnim && Counter < 0)
                {
                    Close();
                    Bmp.Visible = !HideAtLastFrame;
                    if (DestroyAfterLastFrame)
                        destroyAfterLastFrame();
                    return;
                }
                Bmp.Bitmap = cellList[Counter].bitmap;
                if (!PointOfParent)
                    Bmp.Point = new Point(cellList[Counter].posX, cellList[Counter].posY);
                if (cellList[Counter].rec != Rectangle.Empty)
                    Bmp.Rectangle = cellList[Counter].rec;
                Counter = (Reverse) ? Counter - 1 : Counter + 1;
                if (!Reverse && Counter != cellList.Count)
                {
                    double TOLERANCE = 0;
                    if (Math.Abs(aTimer.Interval - cellList[Counter].interval) > TOLERANCE)
                        aTimer.Interval = cellList[Counter].interval;
                }
                else if (!Reverse && Counter == cellList.Count)
                    aTimer.Interval = cellList[0].interval;
                else if (Reverse && Counter == cellList.Count)
                    aTimer.Interval = cellList[cellList.Count - 1].interval;
            }
        }

        /// <summary>
        /// Make the animation invisible and stop the animation
        /// </summary>
        
        private void destroyAfterLastFrame()
        {
            // detruit les objets crées
            Bmp.Visible = false;
            aTimer.Stop();
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="width">Width of current sequance</param>
        /// <param name="height">Height of current sequance</param>
        public void AddCell(string asset, int id, int posX, int posY, int width, int height)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = 1,
                interval = (int)Interval
            };

            if (Encrypted)
            {
                Bitmap uncryptedBitmap = new Bitmap(new Crypt().DecryptFile(asset, Key, IV));
                Bitmap resizedBitmap = new Bitmap(uncryptedBitmap, new Size(width, height));
                cell.bitmap = resizedBitmap;
            }
            else
            {
                Bitmap bitmap = new Bitmap(asset);
                cell.bitmap = new Bitmap(bitmap, new Size(width, height));
            }
            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="width">Width of current sequance</param>
        /// <param name="height">Height of current sequance</param>
        /// <param name="rec">rec is a Rectangle object to pick only a part of the picture, usfull when dealing with a spritesheet</param>
        public void AddCell(string asset, int id, int posX, int posY, int width, int height, Rectangle rec)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = 1,
                interval = (int)Interval,
                rec = rec
            };
            if (Encrypted)
            {
                Bitmap uncryptedBitmap = new Bitmap(new Crypt().DecryptFile(asset, Key, IV));
                Bitmap resizedBitmap = new Bitmap(uncryptedBitmap, new Size(width, height));
                cell.bitmap = resizedBitmap;
            }
            else
            {
                Bitmap bitmap = new Bitmap(asset);
                cell.bitmap = new Bitmap(bitmap, new Size(width, height));
            }
            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="width">Width of current sequance</param>
        /// <param name="height">Height of current sequance</param>
        /// <param name="Interval">Int value indicating interval that take this current sequance when displaying in millisecond</param>
        public void AddCell(string asset, int id, int posX, int posY, int width, int height, int Interval)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = 1,
                interval = Interval
            };
            if (Encrypted)
            {
                Bitmap uncryptedBitmap = new Bitmap(new Crypt().DecryptFile(asset, Key, IV));
                Bitmap resizedBitmap = new Bitmap(uncryptedBitmap, new Size(width, height));
                cell.bitmap = resizedBitmap;
            }
            else
            {
                Bitmap bitmap = new Bitmap(asset);
                cell.bitmap = new Bitmap(bitmap, new Size(width, height));
            }
            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="width">Width of current sequance</param>
        /// <param name="height">Height of current sequance</param>
        /// <param name="opacity">Opacity is a float object, if equal to 0F then the current sequance if invisible, if equal to 0.5F then the sequance if 50% transparent, if equal to 1F then the sequance is completly visible 'Opaque'</param>
        public void AddCell(string asset, int id, int posX, int posY, int width, int height, float opacity)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = opacity,
                interval = (int)Interval
            };

            if (Encrypted)
            {
                Bitmap uncryptedBitmap = new Bitmap(new Crypt().DecryptFile(asset, Key, IV));
                Bitmap resizedBitmap = new Bitmap(uncryptedBitmap, new Size(width, height));
                cell.bitmap = resizedBitmap;
            }
            else
            {
                Bitmap bitmap = new Bitmap(asset);
                cell.bitmap = new Bitmap(bitmap, new Size(width, height));
            }

            cell.bitmap = ManagerInstance.Opacity(cell.bitmap, opacity);
            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="width">Width of current sequance</param>
        /// <param name="height">Height of current sequance</param>
        /// <param name="opacity">Opacity is a float object, if equal to 0F then the current sequance if invisible, if equal to 0.5F then the sequance if 50% transparent, if equal to 1F then the sequance is completly visible 'Opaque'</param>
        /// <param name="Interval">Int value indicating interval that take this current sequance when displaying in millisecond</param>
        public void AddCell(string asset, int id, int posX, int posY, int width, int height, float opacity, int Interval)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = opacity,
                interval = Interval
            };

            if (Encrypted)
            {
                Bitmap uncryptedBitmap = new Bitmap(new Crypt().DecryptFile(asset, Key, IV));
                Bitmap resizedBitmap = new Bitmap(uncryptedBitmap, new Size(width, height));
                cell.bitmap = resizedBitmap;
            }
            else
            {
                Bitmap bitmap = new Bitmap(asset);
                cell.bitmap = new Bitmap(bitmap, new Size(width, height));
            }

            cell.rec = new Rectangle(new Point(0, 0), new Size(width, height));
            cell.bitmap = ManagerInstance.Opacity(cell.bitmap, opacity);
            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        public void AddCell(string asset, int id, int posX, int posY)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = 1,
                interval = (int)Interval,
                bitmap =
                    Encrypted ? new Bitmap(new Crypt().DecryptFile(asset, Key, IV)) : new Bitmap(asset)
            };

            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="rec">rec is a Rectangle object to pick only a part of the picture, usfull when dealing with a spritesheet</param>
        public void AddCell(string asset, int id, int posX, int posY, Rectangle rec)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = 1,
                interval = (int)Interval,
                rec = rec,
                bitmap =
                    Encrypted ? new Bitmap(new Crypt().DecryptFile(asset, Key, IV)) : new Bitmap(asset)
            };

            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="Interval">Int value indicating interval that take this current sequance when displaying in millisecond</param>
        public void AddCell(string asset, int id, int posX, int posY, int Interval)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = 1,
                interval = Interval,
                bitmap =
                    Encrypted ? new Bitmap(new Crypt().DecryptFile(asset, Key, IV)) : new Bitmap(asset)
            };

            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="rec">rec is a Rectangle object to pick only a part of the picture, usfull when dealing with a spritesheet</param>
        /// <param name="Interval">Int value indicating interval that take this current sequance when displaying in millisecond</param>
        public void AddCell(string asset, int id, int posX, int posY, Rectangle rec, int Interval)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = 1,
                interval = (int)Interval,
                rec = rec
            };
            cell.interval = Interval;

            cell.bitmap = Encrypted ? new Bitmap(new Crypt().DecryptFile(asset, Key, IV)) : new Bitmap(asset);

            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="opacity">Opacity is a float object, if equal to 0F then the current sequance if invisible, if equal to 0.5F then the sequance if 50% transparent, if equal to 1F then the sequance is completly visible 'Opaque'</param>
        public void AddCell(string asset, int id, int posX, int posY, float opacity)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = opacity,
                interval = (int)Interval
            };

            var tmp = Encrypted ? new Bitmap(new Crypt().DecryptFile(asset, Key, IV)) : new Bitmap(asset);

            cell.bitmap = ManagerInstance.Opacity(tmp, opacity);
            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="opacity">Opacity is a float object, if equal to 0F then the current sequance if invisible, if equal to 0.5F then the sequance if 50% transparent, if equal to 1F then the sequance is completly visible 'Opaque'</param>
        /// <param name="rec">rec is a Rectangle object to pick only a part of the picture, usfull when dealing with a spritesheet</param>
        public void AddCell(string asset, int id, int posX, int posY, float opacity, Rectangle rec)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = opacity,
                interval = (int)Interval,
                rec = rec
            };

            var tmp = Encrypted ? new Bitmap(new Crypt().DecryptFile(asset, Key, IV)) : new Bitmap(asset);

            cell.bitmap = ManagerInstance.Opacity(tmp, opacity);
            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="opacity">Opacity is a float object, if equal to 0F then the current sequance if invisible, if equal to 0.5F then the sequance if 50% transparent, if equal to 1F then the sequance is completly visible 'Opaque'</param>
        /// <param name="Interval">Int value indicating interval that take this current sequance when displaying in millisecond</param>
        public void AddCell(string asset, int id, int posX, int posY, float opacity, int Interval)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = opacity,
                interval = Interval
            };

            var tmp = Encrypted ? new Bitmap(new Crypt().DecryptFile(asset, Key, IV)) : new Bitmap(asset);

            cell.bitmap = ManagerInstance.Opacity(tmp, opacity);
            cellList.Add(cell);
        }

        /// <summary>
        /// Add assets to the image sequances
        /// </summary>
        /// <param name="asset">asset is the full path of the picture file</param>
        /// <param name="id">id is an int value as an identifier of the sequance</param>
        /// <param name="posX">posX is an int value of the horizontal position of the picture</param>
        /// <param name="posY">posY is an int value of the vertical position of the picture</param>
        /// <param name="opacity">Opacity is a float object, if equal to 0F then the current sequance if invisible, if equal to 0.5F then the sequance if 50% transparent, if equal to 1F then the sequance is completly visible 'Opaque'</param>
        /// <param name="Interval">Int value indicating interval that take this current sequance when displaying in millisecond</param>
        /// <param name="rec">rec is a Rectangle object to pick only a part of the picture, usfull when dealing with a spritesheet</param>
        public void AddCell(string asset, int id, int posX, int posY, float opacity, int Interval, Rectangle rec)
        {
            AnimCell cell = new AnimCell
            {
                cell = asset,
                id = id,
                posX = posX,
                posY = posY,
                Opacity = opacity,
                interval = Interval,
                rec = rec
            };

            var tmp = Encrypted ? new Bitmap(new Crypt().DecryptFile(asset, Key, IV)) : new Bitmap(asset);

            cell.bitmap = ManagerInstance.Opacity(tmp, opacity);
            cellList.Add(cell);
        }

        /// <summary>
        /// Create a perfect duplication of the Bmp object
        /// </summary>
        /// <returns>Return an object of Bmp, you need a cast to Bmp type</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
        #endregion
    }
}
