using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static MELHARFI.Manager.Manager;

namespace MELHARFI.Manager.Gfx
{
    /// <summary>
    /// Class to draw a filled polygon
    /// </summary>
    public class FillPolygon : IGfx
    {
        #region properties

        #region Mouse Event Handler
        // classe Bmp qui contiens des infos sur les images, hérite de l'interface IGfx
        /// <summary>
        /// Delegate of Mouse Event Handler, not usfull for the user, it's a Mouse Event system mechanisme
        /// </summary>
        /// <param name="bmp">bmp is a graphic object "image" that raised the event</param>
        /// <param name="e">e is a Mouse Event Arguments with some handy information, as button of clic, position ...</param>
        public delegate void FillPolygonMouseEventHandler(FillPolygon fillPolygon, MouseEventArgs e);

        /// <summary>
        /// Handler when you double clic on object
        /// </summary>
        public event FillPolygonMouseEventHandler MouseDoubleClic;

        /// <summary>
        /// Handler when you clic on object
        /// </summary>
        public event FillPolygonMouseEventHandler MouseClic;

        /// <summary>
        /// Handler when you clic and keep button clicked on the object
        /// </summary>
        public event FillPolygonMouseEventHandler MouseDown;

        /// <summary>
        /// Handler when you release button that has been clicked in an object
        /// </summary>
        public event FillPolygonMouseEventHandler MouseUp;

        /// <summary>
        /// Handler when the mouse move inside an object, event will raise many time as you move the mouse over object
        /// </summary>
        public event FillPolygonMouseEventHandler MouseMove;

        /// <summary>
        /// Handler when a mouse get outside of an object
        /// </summary>
        public event FillPolygonMouseEventHandler MouseOut;

        /// <summary>
        /// Handler when a mouse get indise an object, the event is raised only 1 time
        /// </summary>
        public event FillPolygonMouseEventHandler MouseOver;

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
        public void FireMouseOut(MouseEventArgs e)
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
        /// point where the polygon will be drawn on the form;
        /// </summary>
        public Point[] Points;

        public Point Point
        {
            get => Points[0];
            set => Points[0] = value;
        }

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
        /// Hold which layer the object is stored
        /// </summary>
        public TypeGfx TypeGfx = TypeGfx.Background;

        public Brush FillColor;

        /// <summary>
        /// Tag is an object that you can affect to anything you want, usful to attach it to a class that hold some statistic to a players, Pay attention that you should cast the object
        /// </summary>
        /// <returns>Return Object</returns>
        public object Tag { get; set; }

        public FillMode FillMode = FillMode.Winding;

        /// <summary>
        /// Rectangle is an area of an image, used to show only a part of image, a Rectangle need a Point value defined by a X position and Y position, and Size value defined by a Width and Height
        /// </summary>
        public Rectangle Rectangle
        {
            get;
            set;
        }

        /// <summary>
        /// Color of Border
        /// </summary>
        public Color BorderColor;

        /// <summary>
        /// Width of Border
        /// </summary>
        public int BorderWidth = 0;

        /// <summary>
        /// A referense is requird to the Manager instance that hold this object
        /// </summary>
        public Manager ManagerInstance { get; set; }
        #endregion

        #region constructors
        /// <summary>
        /// Draw filled polygon without border
        /// </summary>
        /// <param name="fillColor">Color of filled area</param>
        /// <param name="points">Points of polygone</param>
        /// <param name="typeGfx">Type of graphic: Background, Object, Top</param>
        /// <param name="name">name</param>
        /// <param name="visible">true, false</param>
        /// <param name="fillMode">Alternate, Winding</param>
        /// <param name="manager">manager who hold the object</param>
        public FillPolygon(Brush fillColor, Point[] points, TypeGfx typeGfx, string name, bool visible, FillMode fillMode, Manager manager)
        {
            ManagerInstance = manager;
            FillColor = fillColor;
            Points = points;
            TypeGfx = typeGfx;
            Name = name;
            Visible = visible;
            FillMode = fillMode;
            Rectangle = GetRectangle(points);
                
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
        }

        /// <summary>
        /// Draw filled polygon without border
        /// </summary>
        /// <param name="fillColor">Color of filled area</param>
        /// <param name="points">Points of polygone</param>
        /// <param name="typeGfx">Type of graphic: Background, Object, Top</param>
        /// <param name="name">name</param>
        /// <param name="visible">true, false</param>
        /// <param name="fillMode">Alternate, Winding</param>
        /// <param name="borderColor">Color of border</param>
        /// <param name="borderWidth">Border width</param>
        /// <param name="manager">manager who hold the object</param>
        public FillPolygon(Brush fillColor, Point[] points, TypeGfx typeGfx, string name, bool visible, FillMode fillMode, Color borderColor, int borderWidth, Manager manager)
        {
            ManagerInstance = manager;
            FillColor = fillColor;
            Points = points;
            TypeGfx = typeGfx;
            Name = name;
            Visible = visible;
            FillMode = fillMode;
            BorderColor = borderColor;
            BorderWidth = borderWidth;
            Rectangle = GetRectangle(points);
        }
        #endregion

        private Rectangle GetRectangle(Point[] points)
        {
            int x = 0, y = 0, width = 0, height = 0;

            for (int cnt = 0; cnt < points.Length; cnt++)
            {
                if(cnt == 0)
                {
                    x = points[cnt].X;
                    width = points[cnt].X;

                    y = points[cnt].Y;
                    height = points[cnt].Y;
                    continue;
                }

                if (x > points[cnt].X)
                    x = points[cnt].X;

                if (width < points[cnt].X)
                    width = points[cnt].X;

                if (y > points[cnt].Y)
                    y = points[cnt].Y;

                if (height < points[cnt].Y)
                    height = points[cnt].Y;
            }

            return new Rectangle(x, y, width - x, height - y);
        }
        /// <summary>
        /// Create a perfect duplication of the Bmp object
        /// </summary>
        /// <returns>Return an object of Bmp, you need a cast to Bmp type</returns>
        public object Clone() => MemberwiseClone();
    }
}
