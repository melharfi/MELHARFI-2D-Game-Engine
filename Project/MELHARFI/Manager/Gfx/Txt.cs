using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static MELHARFI.Manager.Manager;

namespace MELHARFI.Manager.Gfx
{
    public class Txt : IGfx
    {
        #region properties

        #region event handlers
        // classe qui contiens les texts a afficher, hérite de l'interface IGfx
        public delegate void TxtMouseEventHandler(Txt txt, MouseEventArgs e);

        /// <summary>
        /// Handler when you double clic on object
        /// </summary>
        public event TxtMouseEventHandler MouseDoubleClic;

        /// <summary>
        /// Handler when you clic on object
        /// </summary>
        public event TxtMouseEventHandler MouseClic;

        /// <summary>
        /// Handler when Mouse Down clic on object
        /// </summary>
        public event TxtMouseEventHandler MouseDown;

        /// <summary>
        /// Handler when Mouse Up on object
        /// </summary>
        public event TxtMouseEventHandler MouseUp;

        /// <summary>
        /// Handler when Mouse Move on object
        /// </summary>
        public event TxtMouseEventHandler MouseMove;

        /// <summary>
        /// Handler when Mouse Out on object
        /// </summary>
        public event TxtMouseEventHandler MouseOut;

        /// <summary>
        /// Handler when you double clic on object
        /// </summary>
        public event TxtMouseEventHandler MouseOver;

        /// <summary>
        /// To fire the Mouse Double Clic event without interaction of user
        /// </summary>
        /// <param name="e">e is a MouseEventArgs object</param>
        internal void FireMouseDoubleClic(MouseEventArgs e)
        {
            MouseDoubleClic?.Invoke(this, e);
        }

        /// <summary>
        /// To fire the Mouse Clic event without interaction of user
        /// </summary>
        /// <param name="e">e is a MouseEventArgs object</param>
        internal void FireMouseClic(MouseEventArgs e)
        {
            MouseClic?.Invoke(this, e);
        }

        /// <summary>
        /// To fire the Mouse Down Clic event without interaction of user
        /// </summary>
        /// <param name="e">e is a MouseEventArgs object</param>
        internal void FireMouseDown(MouseEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        /// <summary>
        /// To fire the Mouse Up Clic event without interaction of user
        /// </summary>e is a MouseEventArgs object
        /// <param name="e">e is a MouseEventArgs object</param>
        internal void FireMouseUp(MouseEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        /// <summary>
        /// To fire the Mouse Move Clic event without interaction of user
        /// </summary>e is a MouseEventArgs object
        /// <param name="e"></param>
        internal void FireMouseMove(MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        /// <summary>
        /// To fire the Mouse Over Clic event without interaction of user
        /// </summary>
        /// <param name="e">e is a MouseEventArgs object</param>
        internal void FireMouseOver(MouseEventArgs e)
        {
            MouseOver?.Invoke(this, e);
        }

        /// <summary>
        /// To fire the Mouse Out Clic event without interaction of user
        /// </summary>
        /// <param name="e">e is a MouseEventArgs object</param>
        internal void FireMouseOut(MouseEventArgs e)
        {
            if (MouseOut == null) return;
            ManagerInstance.mouseOverRecorder.Remove(this);  // pour que MouseOut ne cherche pas sur un Gfx qui n'est pas sur le devant
            MouseOut(this, e);
        }
        #endregion

        #region event properties
        /// <summary>
        /// Useless for the moment, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseDoubleClic = false;

        /// <summary>
        /// Useless for the moment, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseClic = false;

        /// <summary>
        /// Useless for the moment, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseOver = false;

        /// <summary>
        /// Useless for the moment, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseDown = false;

        /// <summary>
        /// Useless for the moment, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseUp = false;

        /// <summary>
        /// Useless for the moment, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileMouseMove = false;

        /// <summary>
        /// Useless for the moment, not yet implemented
        /// </summary>
        public bool EscapeGfxWhileKeyDown = false;
        #endregion

        /// <summary>
        /// Text is a string value to be drawn to screen
        /// </summary>
        public string Text;

        /// <summary>
        /// point where the text will be drawn on the form, Default is X = 0, Y = 0;
        /// </summary>
        public Point Point { get; set; } = Point.Empty;

        /// <summary>
        /// string value giving the name of the object, read only
        /// </summary>
        /// <returns>return a string name value</returns>
        public string Name { get; set; }

        /// <summary>
        /// Tag is an object that you can affect to anything you want, usful to attach it to a class that hold some statistic to a players, Pay attention that you should cast the object
        /// </summary>
        /// <returns>Return an object value</returns>
        public object Tag { get; set; }

        /// <summary>
        /// Zindex is a int value indicating the deep of the object against the other object in same graphic layer, default is 1 , read only
        /// </summary>
        /// <returns>Return an int value</returns>
        public int Zindex { get; set; } = 1;

        /// <summary>
        /// Boolean value to change the state of object between visible and invisible
        /// </summary>
        /// <param name="_visible">If _visible is true, the object is visible, if false the object is invisible</param>
        public bool Visible { get; set; } = true;

        /// <summary>
        /// Font value for the text
        /// </summary>
        public Font Font = new Font("Verdana", 8);

        /// <summary>
        /// Color of the text
        /// </summary>
        public Brush Brush = Brushes.Black;

        /// <summary>
        /// Hold which layer the object is stored
        /// </summary>
        public Manager.TypeGfx TypeGfx;

        public Manager ManagerInstance { get; set; }
        #endregion

        #region constructors
        /// <summary>
        /// Empty constructor to draw text
        /// </summary>
        public Txt(Manager manager)
        {
            ManagerInstance = manager;
        }

        /// <summary>
        /// Txt constructor to draw text
        /// </summary>
        /// <param name="_txt">_txt equal text to be drawn as a string value</param>
        /// <param name="_point">_point is a value of X and Y position where the text will be draw</param>
        public Txt(string _txt, Point _point, Manager manager)
        {
            Text = _txt;
            Point = _point;
            ManagerInstance = manager;
            Zindex = ManagerInstance.ZOrder.Bgr();
            TypeGfx = TypeGfx.Background;
        }

        /// <summary>
        /// Txt constructor to draw text
        /// </summary>
        /// <param name="txt">_txt equal text to be drawn as a string value</param>
        /// <param name="point">_point is a value of X and Y position where the text will be draw</param>
        /// <param name="name">_name is a string value for the name of the object</param>
        /// <param name="typeGfx">_typeGfx is a TypeGfx type to record where the object is stored</param>
        /// <param name="visible">is a boolean value, if true then the object is visible, else the object is invisible</param>
        /// <param name="font">Font value for the text</param>
        /// <param name="brush">Brush is the color of the text</param>
        public Txt(string txt, Point point, string name, TypeGfx typeGfx, bool visible, Font font, Brush brush, Manager manager)
        {
            Text = txt;
            Point = new Point(point.X, point.Y);
            Name = name;
            Visible = visible;
            Font = font;
            Brush = brush;
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
