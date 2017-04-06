using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MELHARFI
{
    public partial class Manager
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
            //public event RecKeyEventHandler KeyDown;

            /// <summary>
            /// Layer holding sub graphics that is shown in the front of the parent, and its position is relative to parent
            /// </summary>
            public List<IGfx> Child = new List<IGfx>();

            /// <summary>
            /// tag object to assigne to whatever you want
            /// </summary>
            private object tag;

            /// <summary>
            /// Hold which layer the object is stored
            /// </summary>
            public MELHARFI.Manager.TypeGfx TypeGfx;

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
            /// String value to store the name of the object, useful if you need to look for it in the appropriate layer, value is read only
            /// </summary>
            /// <returns>Return a string value as a name of the object</returns>

            /// <summary>
            /// Tag is an object that you can affect to anything you want, usful to attach it to a class that hold some statistic to a players, Pay attention that you should cast the object
            /// </summary>
            /// <returns>Return Object</returns>
            public object Tag
            {
                get { return tag; }
                set { tag = value; }
            }

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
            /// Zindex is a int value indicating the deep of the object against the other object in same graphic layer, default is 1 , read only
            /// </summary>
            /// <returns>Return an int value</returns>
            public int Zindex
            {
                get { return zindex; }
                set { zindex = value; }
            }

            private Manager parentManager;

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

            /// <summary>
            /// To fire the Mouse Double Clic event without interaction of user
            /// </summary>
            /// <param name="e">e = MouseEventArgs</param>
            public void FireMouseDoubleClic(MouseEventArgs e)
            {
                MouseDoubleClic?.Invoke(this, e);
            }

            /// <summary>
            /// To fire the Mouse Clic event without interaction of user
            /// </summary>
            /// <param name="e">e = MouseEventArgs</param>
            public void FireMouseClic(MouseEventArgs e)
            {
                MouseClic?.Invoke(this, e);
            }

            /// <summary>
            /// To fire the Mouse Down event without interaction of user
            /// </summary>
            /// <param name="e">e = MouseEventArgs</param>
            public void FireMouseDown(MouseEventArgs e)
            {
                MouseDown?.Invoke(this, e);
            }

            /// <summary>
            /// To fire the Mouse Up event without interaction of user
            /// </summary>
            /// <param name="e">e = MouseEventArgs</param>
            public void FireMouseUp(MouseEventArgs e)
            {
                MouseUp?.Invoke(this, e);
            }

            /// <summary>
            /// To fire the Mouse Move event without interaction of user
            /// </summary>
            /// <param name="e">e = MouseEventArgs</param>
            public void FireMouseMove(MouseEventArgs e)
            {
                MouseMove?.Invoke(this, e);
            }

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

            /// <summary>
            /// To fire the Mouse Over event without interaction of user
            /// </summary>
            /// <param name="e">Return an object of Bmp, you need a cast to Bmp type</param>
            public void FireMouseOver(MouseEventArgs e)
            {
                MouseOver?.Invoke(this, e);
            }

            /// <summary>
            /// Fire the ChangedBrush event without a user interaction
            /// </summary>
            /// <param name="e">e = MouseEventArgs</param>
            public void FireChangedBrush(MouseEventArgs e)
            {
                ChangedBrush?.Invoke(this, e);
            }

            /// <summary>
            /// Size of the rectangle
            /// </summary>
            public Size size;

            /// <summary>
            /// Point where the rectangle will be drawn
            /// </summary>
            public Point point;

            /// <summary>
            /// Name of the object, usfull if you need to search it by its name
            /// </summary>
            private string name;

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
            /// Boolean, True make the object visible, False the object is invisible, default is True
            /// </summary>
            private bool visible = true;

            private Brush _brush;

            /// <summary>
            /// Color of rectangle
            /// </summary>
            public Brush brush
            {
                get { return _brush; }
                set { _brush = value; FireChangedBrush(null); }
            }
            #endregion

            #region constructors
            /// <summary>
            /// Empty constructor
            /// </summary>
            public Rec(Manager manager)
            {
                parentManager = manager;
            }

            /// <summary>
            /// Constructor of Rectangle
            /// </summary>
            /// <param name="b">b is a color as a Brush type</param>
            /// <param name="_point">_point is the location where the rectangle will be drawn to the form</param>
            /// <param name="_size">Size of the rectangle</param>
            /// <param name="_name">Name of the object</param>
            /// <param name="_typeGfx">Type of layer that hold the Rectangle</param>
            /// <param name="_visible">Boolean value indicating if the rectangle is visible or note</param>
            public Rec(Brush b, Point _point, Size _size, string _name, MELHARFI.Manager.TypeGfx _typeGfx, bool _visible, Manager manager)
            {
                _brush = b;
                point = _point;
                size = _size;
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
                parentManager = manager;
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
}
