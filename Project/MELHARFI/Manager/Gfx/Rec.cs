using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static MELHARFI.Manager.Manager;

namespace MELHARFI.Manager.Gfx
{
    public class Rec : IGfx
    {
        #region Properties
        #region Mouse Event Handler
        /// <summary>
        /// Delegate of Mouse Event Handler, not usfull for the user, it's a Mouse Event system mechanisme
        /// </summary>
        /// <param name="rec">Rec object</param>
        /// <param name="e">e is a MouseEventArgs</param>
        public delegate void RecMouseEventHandler(Rec rec, MouseEventArgs e);

        /// <summary>
        /// Delegate of Mouse Event Handler, not usfull for the user, it's a Key Event system mechanisme
        /// </summary>
        /// <param name="rec">Rec object</param>
        /// <param name="e">e is a KeyEventArgs</param>
        public delegate void RecKeyEventHandler(Rec rec, KeyEventArgs e);

        /// <summary>
        /// Handler when you double clic on object
        /// </summary>
        public event RecMouseEventHandler MouseDoubleClic;

        /// <summary>
        /// Handler when you clic on object
        /// </summary>
        public event RecMouseEventHandler MouseClic;

        /// <summary>
        /// Handler when Mouse Down on object
        /// </summary>
        public event RecMouseEventHandler MouseDown;

        /// <summary>
        /// Handler when Mouse Up on object
        /// </summary>
        public event RecMouseEventHandler MouseUp;

        /// <summary>
        /// Handler when Mouse Move on object
        /// </summary>
        public event RecMouseEventHandler MouseMove;

        /// <summary>
        /// Handler when Mouse Out on object
        /// </summary>
        public event RecMouseEventHandler MouseOut;

        /// <summary>
        /// Handler when Mouse Over on object
        /// </summary>
        public event RecMouseEventHandler MouseOver;

        /// <summary>
        /// ChangedBrush Event Handler
        /// </summary>
        public event RecMouseEventHandler ChangedBrush;
        #endregion
        /// <summary>
        /// Layer holding sub graphics that is shown in the front of the parent, and its position is relative to parent
        /// </summary>

        public List<IGfx> Childs { get; set; } = new List<IGfx>();

        /// <summary>
        /// Hold which layer the object is stored
        /// </summary>
        public TypeGfx TypeGfx;

        /// <summary>
        /// useless, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseDoubleClic = false;

        /// <summary>
        /// useless, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseClic = false;

        /// <summary>
        /// useless, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseOver = false;

        /// <summary>
        /// useless, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseDown = false;

        /// <summary>
        /// useless, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseUp = false;

        /// <summary>
        /// useless, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseMove = false;

        /// <summary>
        /// Tag is an object that you can affect to anything you want, usful to attach it to a class that hold some statistic to a players, Pay attention that you should cast the object
        /// </summary>
        /// <returns>Return Object</returns>
        public object Tag { get; set; }

        /// <summary>
        /// Boolean value indicating if the object is visible or invisible, value read only
        /// </summary>
        /// <returns>Return a boolean value</returns>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Zindex is a int value indicating the deep of the object against the other object in same graphic layer, default is 1 , read only
        /// </summary>
        /// <returns>Return an int value</returns>
        public int Zindex { get; set; } = 1;

        public Manager ManagerInstance { get; set; }

        /// <summary>
        /// To fire the Mouse Double Clic event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseDoubleClic(MouseEventArgs e)
        {
            MouseDoubleClic?.Invoke(this, e);
        }

        /// <summary>
        /// To fire the Mouse Clic event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseClic(MouseEventArgs e)
        {
            MouseClic?.Invoke(this, e);
        }

        /// <summary>
        /// To fire the Mouse Down event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseDown(MouseEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        /// <summary>
        /// To fire the Mouse Up event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseUp(MouseEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        /// <summary>
        /// To fire the Mouse Move event without interaction of user
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireMouseMove(MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

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

        /// <summary>
        /// To fire the Mouse Over event without interaction of user
        /// </summary>
        /// <param name="e">Return an object of Bmp, you need a cast to Bmp type</param>
        internal void FireMouseOver(MouseEventArgs e)
        {
            MouseOver?.Invoke(this, e);
        }

        /// <summary>
        /// Fire the ChangedBrush event without a user interaction
        /// </summary>
        /// <param name="e">e = MouseEventArgs</param>
        internal void FireChangedBrush(MouseEventArgs e)
        {
            ChangedBrush?.Invoke(this, e);
        }

        /// <summary>
        /// Size of the rectangle
        /// </summary>
        public Size Size;

        /// <summary>
        /// Point where the rectangle will be drawn
        /// </summary>
        public Point Point { get; set; } = Point.Empty;

        public string Name { get; set; }

        private Brush fillColor;

        /// <summary>
        /// Color of rectangle
        /// </summary>
        public Brush FillColor
        {
            get
            {
                return fillColor;
            }
            set
            {
                fillColor = value;
                FireChangedBrush(null);
            }
        }

        /// <summary>
        /// Color of Border
        /// </summary>
        public Color BorderColor;

        /// <summary>
        /// Width of Border
        /// </summary>
        public int BorderWidth = 0;
        #endregion

        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fillColor">Shape Color</param>
        /// <param name="point">Point where shape will be drawn</param>
        /// <param name="size">Size of the shape</param>
        /// <param name="typeGfx">Shape placeHolder</param>
        public Rec(Brush fillColor, Point point, Size size, TypeGfx typeGfx, Manager manager)
        {
            this.fillColor = fillColor;
            Point = point;
            Size = size;
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
            }
        }

        /// <summary>
        /// Constructor of Rectangle
        /// </summary>
        /// <param name="fillColor">b is a color as a Brush type</param>
        /// <param name="point">_point is the location where the rectangle will be drawn to the form</param>
        /// <param name="size">Size of the rectangle</param>
        /// <param name="name">Name of the object</param>
        /// <param name="typeGfx">Type of layer that hold the Rectangle</param>
        /// <param name="visible">Boolean value indicating if the rectangle is visible or note</param>
        public Rec(Brush fillColor, Point point, Size size, string name, TypeGfx typeGfx, bool visible, Manager manager)
        {
            this.fillColor = fillColor;
            Point = point;
            Size = size;
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
            }
        }
        #endregion

        /// <summary>
        /// Create a perfect duplication of the Bmp object
        /// </summary>
        /// <returns>Return an object of Bmp, you need a cast to Bmp type</returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
