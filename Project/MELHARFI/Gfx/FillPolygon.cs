using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MELHARFI
{
    public partial class Manager
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
                parentManager.MouseOverRecorder.Remove(this);  // pour que MouseOut ne cherche pas sur un Gfx qui n'est pas sur le devant
                MouseOut(this, e);
            }
            #endregion

            /// <summary>
            /// Layer holding sub graphics that is shown in the front of the parent, and its position is relative to parent
            /// </summary>
            public List<IGfx> Child = new List<IGfx>();

            /// <summary>
            /// point where the polygon will be drawn on the form;
            /// </summary>
            public Point[] point;

            private string name;

            /// <summary>
            /// String value to store the name of the object, useful if you need to look for it in the appropriate layer, value is read only
            /// </summary>
            /// <returns>Return a string value as a name of the object</returns>
            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            /// <summary>
            /// Zindex is a int value indicating the deep of the object against the other object in same graphic layer, default is 1
            /// </summary>
            private int zindex = 1;

            /// <summary>
            /// Zindex is a int value indicating the deep of the object against the other object in same graphic layer, default is 1
            /// </summary>
            /// <returns>Return an int value </returns>
            public int Zindex
            {
                get { return zindex; }
                set { zindex = value; }
            }

            /// <summary>
            /// Boolean, True make the object visible, False the object is invisible, default is True
            /// </summary>
            private bool visible = true;

            /// <summary>
            /// Boolean value indicating if the object is visible or invisible, value read only
            /// </summary>
            /// <returns>Return a boolean value</returns>
            public bool Visible
            {
                get { return visible; }
                set { visible = value; }
            }

            /// <summary>
            /// Hold which layer the object is stored
            /// </summary>
            public TypeGfx TypeGfx = TypeGfx.Background;

            public Brush brush;

            /// <summary>
            /// tag object to assigne to whatever you want
            /// </summary>
            private Object tag;

            /// <summary>
            /// Tag is an object that you can affect to anything you want, usful to attach it to a class that hold some statistic to a players, Pay attention that you should cast the object
            /// </summary>
            /// <returns>Return Object</returns>
            public object Tag
            {
                get { return tag; }
                set { tag = value; }
            }

            public FillMode fillMode = FillMode.Winding;

            /// <summary>
            /// Rectangle value representing a point and size, used in a spritesheet image
            /// </summary>
            private Rectangle _rectangle;

            /// <summary>
            /// Rectangle is an area of an image, used to show only a part of image, a Rectangle need a Point value defined by a X position and Y position, and Size value defined by a Width and Height
            /// </summary>
            public Rectangle rectangle
            {
                get { return _rectangle; }
                set { _rectangle = value; }
            }

            private Manager parentManager;

            /// <summary>
            /// A referense is requird to the Manager instance that hold this object
            /// </summary>
            public Manager ParentManager
            {
                get
                {
                    return parentManager;
                }
                set
                {
                    parentManager = value;
                }
            }
            #endregion

            #region constructors
            public FillPolygon(Manager manager, Brush _brush, Point[] _point, TypeGfx _typeGfx, string _name, bool _visible, FillMode _fillMode)
            {
                parentManager = manager;
                brush = _brush;
                point = _point;
                TypeGfx = _typeGfx;
                name = _name;
                visible = _visible;
                fillMode = _fillMode;
                rectangle = GetRectangle(_point); 
            }
            #endregion

            private Rectangle GetRectangle(Point[] _point)
            {
                int x = 0, y = 0, width = 0, height = 0;

                for (int cnt = 0; cnt < _point.Length; cnt++)
                {
                    if(cnt == 0)
                    {
                        x = _point[cnt].X;
                        width = _point[cnt].X;

                        y = _point[cnt].Y;
                        height = _point[cnt].Y;
                        continue;
                    }

                    if (x > _point[cnt].X)
                        x = _point[cnt].X;

                    if (width < _point[cnt].X)
                        width = _point[cnt].X;

                    if (y > _point[cnt].Y)
                        y = _point[cnt].Y;

                    if (height < _point[cnt].Y)
                        height = _point[cnt].Y;
                }

                return new Rectangle(x, y, width, height);
            }
            /// <summary>
            /// Create a perfect duplication of the Bmp object
            /// </summary>
            /// <returns>Return an object of Bmp, you need a cast to Bmp type</returns>
            public object Clone() => MemberwiseClone();
        }
    }
}
