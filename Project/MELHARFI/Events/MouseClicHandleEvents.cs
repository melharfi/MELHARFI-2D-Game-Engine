using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MELHARFI
{
    public partial class Manager
    {
        /// <summary>
        /// Mouse Clic event
        /// </summary>
        /// <param name="e">e is a MouseEventArgs object with params like mouse button, position ...</param>
        private void MouseClicHandleEvents(MouseEventArgs e)
        {
            try
            {
                bool found = false;         // variable de contrôle qui nous permet de sortir de la boucle dès qu'un condidat est trouvé

                // nétoyage des listes des objets null
                BackgroundLayer.RemoveAll(f => f == null);
                ObjectLayer.RemoveAll(f => f == null);
                TopLayer.RemoveAll(f => f == null);

                List<IGfx> gfxBgrList;  // clone du GfxBgrList pour eviter de modifier une liste lors du rendu
                List<IGfx> gfxObjList;  // clone du GfxObjList pour eviter de modifier une liste lors du rendu
                List<IGfx> gfxTopList;  // clone du GfxTopList pour eviter de modifier une liste lors du rendu

                // triage des listes
                Zindex zi = new Zindex();                   // triage des liste pour l'affichage, du plus petit zindex au plus grand
                ReverseZindex rzi = new ReverseZindex();    // triage des liste pour les controles, de plus grand au plus petit
                // créer un miroire des listes pour eviter tous changement lors de l'affichage
                lock ((BackgroundLayer as ICollection).SyncRoot)
                    gfxBgrList = BackgroundLayer.GetRange(0, BackgroundLayer.Count);
                gfxBgrList.Sort(0, gfxBgrList.Count, zi);

                lock ((ObjectLayer as ICollection).SyncRoot)
                    gfxObjList = ObjectLayer.GetRange(0, ObjectLayer.Count);
                gfxObjList.Sort(0, gfxObjList.Count, zi);

                lock ((TopLayer as ICollection).SyncRoot)
                    gfxTopList = TopLayer.GetRange(0, TopLayer.Count);
                gfxTopList.Sort(0, gfxTopList.Count, zi);

                for (int cnt = gfxTopList.Count; cnt > 0; cnt--)
                {
                    if (found)
                        break;

                    if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(Bmp))
                    {
                        #region childs
                        Bmp b = gfxTopList[cnt - 1] as Bmp;
                        b.Child.Sort(0, b.Child.Count, rzi);
                        foreach (IGfx t in b.Child)
                        {
                            if (t != null && t.GetType() == typeof(Bmp))
                            {
                                Bmp childB = t as Bmp;
                                if (!b.Visible || !childB.Visible || childB.bmp == null ||
                                    !new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y,
                                            (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                            (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                        .Contains(e.Location) ||
                                    (childB.bmp.GetPixel(e.X - childB.point.X - b.point.X + childB.rectangle.X,
                                            e.Y - childB.point.Y - b.point.Y + childB.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseClic)) continue;
                                childB.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(Anim))
                            {
                                Anim childA = t as Anim;
                                if (!b.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                    !new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y,
                                        (childA.img.isSpriteSheet)
                                            ? childA.img.rectangle.Width
                                            : childA.img.bmp.Width,
                                        (childA.img.isSpriteSheet)
                                            ? childA.img.rectangle.Height
                                            : childA.img.bmp.Height).Contains(e.Location) ||
                                    (childA.img.bmp.GetPixel(
                                            e.X - childA.img.point.X - b.point.X + childA.img.rectangle.X,
                                            e.Y - childA.img.point.Y - b.point.Y + childA.img.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                childA.img.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(Rec))
                            {
                                Rec childR = t as Rec;
                                if (!b.Visible || !childR.Visible ||
                                    !new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childR.EscapeGfxWhileMouseClic) continue;
                                childR.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon childF = t as FillPolygon;
                                if (!b.Visible || !childF.Visible ||
                                    !new Rectangle(childF.rectangle.X + b.point.X, childF.rectangle.Y + b.point.Y,
                                        childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location)) continue;
                                SolidBrush sb = childF.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childF.EscapeGfxWhileMouseClic) continue;
                                childF.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t == null || t.GetType() != typeof(Txt)) continue;
                            {
                                Txt childR = t as Txt;
                                if (!b.Visible || !childR.Visible ||
                                    !new Rectangle(childR.Location.X + b.point.X, childR.Location.Y + b.point.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                        e.Location)) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childR.EscapeGfxWhileMouseClic) continue;
                                childR.FireMouseClic(e);
                                found = true;
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent

                        if (found || b.bmp == null || !b.Visible ||
                            !new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width,
                                (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location) ||
                            (b.bmp.GetPixel(e.X - b.point.X + ((b.isSpriteSheet) ? b.rectangle.X : 0),
                                    e.Y - b.point.Y + ((b.isSpriteSheet) ? b.rectangle.Y : 0)) != GetPixel(e.X, e.Y) &&
                                !b.EscapeGfxWhileMouseClic)) continue;
                        b.FireMouseClic(e);
                        found = true;
                        break;

                        #endregion
                    }
                    if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(Anim))
                    {
                        #region childs
                        Anim a = gfxTopList[cnt - 1] as Anim;
                        a.Child.Sort(0, a.Child.Count, rzi);
                        foreach (IGfx t in a.Child)
                        {
                            if (t != null && t.GetType() == typeof(Bmp))
                            {
                                Bmp childB = t as Bmp;
                                if (!a.img.Visible || !childB.Visible || childB.bmp == null ||
                                    !new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y,
                                            (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                            (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                        .Contains(e.Location) ||
                                    (childB.bmp.GetPixel(e.X - childB.point.X - a.img.point.X + childB.rectangle.X,
                                            e.Y - childB.point.Y - a.img.point.Y + childB.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseClic)) continue;
                                childB.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(Anim))
                            {
                                Anim childA = t as Anim;
                                if (!a.img.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                    !new Rectangle(childA.img.point.X + a.img.point.X,
                                        childA.img.point.Y + a.img.point.Y,
                                        (childA.img.isSpriteSheet)
                                            ? childA.img.rectangle.Width
                                            : childA.img.bmp.Width,
                                        (childA.img.isSpriteSheet)
                                            ? childA.img.rectangle.Height
                                            : childA.img.bmp.Height).Contains(e.Location) ||
                                    (childA.img.bmp.GetPixel(
                                            e.X - childA.img.point.X - a.img.point.X + childA.img.rectangle.X,
                                            e.Y - childA.img.point.Y - a.img.point.Y + childA.img.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                childA.img.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(Rec))
                            {
                                Rec childR = t as Rec;
                                if (!a.img.Visible || !childR.Visible ||
                                    !new Rectangle(childR.point.X + a.img.point.X,
                                            childR.point.Y + a.img.point.Y, childR.size.Width, childR.size.Height)
                                        .Contains(e.Location)) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childR.EscapeGfxWhileMouseClic) continue;
                                childR.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon childF = t as FillPolygon;
                                if (!a.img.Visible || !childF.Visible ||
                                    !new Rectangle(childF.rectangle.X + a.img.point.X, childF.rectangle.Y + a.img.point.Y,
                                        childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location)) continue;
                                SolidBrush sb = childF.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childF.EscapeGfxWhileMouseClic) continue;
                                childF.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t == null || t.GetType() != typeof(Txt)) continue;
                            {
                                Txt childR = t as Txt;
                                if (!a.img.Visible || !childR.Visible ||
                                    !new Rectangle(childR.Location.X + a.img.point.X,
                                        childR.Location.Y + a.img.point.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                        e.Location)) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childR.EscapeGfxWhileMouseClic) continue;
                                childR.FireMouseClic(e);
                                found = true;
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent
                        if (found || !a.img.Visible || a.img.bmp == null ||
                            !new Rectangle(a.img.point.X, a.img.point.Y,
                                (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width,
                                (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(
                                e.Location) || (a.img.bmp.GetPixel(
                                                    e.X - a.img.point.X +
                                                    ((a.img.isSpriteSheet) ? a.img.rectangle.X : 0),
                                                    e.Y - a.img.point.Y +
                                                    ((a.img.isSpriteSheet) ? a.img.rectangle.Y : 0)) !=
                                                GetPixel(e.X, e.Y) && !a.img.EscapeGfxWhileMouseClic)) continue;
                        a.img.FireMouseClic(e);
                        found = true;
                        break;
                        #endregion
                    }
                    if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(Rec))
                    {
                        #region childs
                        Rec r = gfxTopList[cnt - 1] as Rec;
                        r.Child.Sort(0, r.Child.Count, rzi);
                        foreach (IGfx t in r.Child)
                        {
                            if (t != null && t.GetType() == typeof(Bmp))
                            {
                                Bmp childB = t as Bmp;
                                if (!r.Visible || !childB.Visible || childB.bmp == null ||
                                    !new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y,
                                            (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                            (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                        .Contains(e.Location) ||
                                    (childB.bmp.GetPixel(e.X - childB.point.X - r.point.X + childB.rectangle.X,
                                            e.Y - childB.point.Y - r.point.Y + childB.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseClic)) continue;
                                childB.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(Anim))
                            {
                                Anim childA = t as Anim;
                                if (!r.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                    !new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y,
                                        (childA.img.isSpriteSheet)
                                            ? childA.img.rectangle.Width
                                            : childA.img.bmp.Width,
                                        (childA.img.isSpriteSheet)
                                            ? childA.img.rectangle.Height
                                            : childA.img.bmp.Height).Contains(e.Location) ||
                                    (childA.img.bmp.GetPixel(
                                            e.X - childA.img.point.X - r.point.X + childA.img.rectangle.X,
                                            e.Y - childA.img.point.Y - r.point.Y + childA.img.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                childA.img.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(Rec))
                            {
                                Rec childR = t as Rec;
                                if (!r.Visible || !childR.Visible ||
                                    !new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childR.EscapeGfxWhileMouseClic) continue;
                                childR.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon childF = t as FillPolygon;
                                if (!r.Visible || !childF.Visible ||
                                    !new Rectangle(childF.rectangle.X + r.point.X, childF.rectangle.Y + r.point.Y,
                                        childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location)) continue;
                                SolidBrush sb = childF.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childF.EscapeGfxWhileMouseClic) continue;
                                childF.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t == null || t.GetType() != typeof(Txt)) continue;
                            {
                                Txt childR = t as Txt;
                                if (!r.Visible || !childR.Visible ||
                                    !new Rectangle(childR.Location.X + r.point.X, childR.Location.Y + r.point.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                        e.Location)) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childR.EscapeGfxWhileMouseClic) continue;
                                childR.FireMouseClic(e);
                                found = true;
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent

                        if (!found != r.Visible || !new Rectangle(r.point, r.size).Contains(e.Location)) continue;
                        {
                            SolidBrush sb = r.brush as SolidBrush;
                            if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !r.EscapeGfxWhileMouseClic)
                                continue;
                            r.FireMouseClic(e);
                            found = true;
                            break;
                        }

                        #endregion
                    }
                    if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(FillPolygon))
                    {
                        #region childs
                        FillPolygon f = gfxTopList[cnt - 1] as FillPolygon;
                        f.Child.Sort(0, f.Child.Count, rzi);
                        foreach (IGfx t in f.Child)
                        {
                            if (t != null && t.GetType() == typeof(Bmp))
                            {
                                Bmp childB = t as Bmp;
                                if (!f.Visible || !childB.Visible || childB.bmp == null ||
                                    !new Rectangle(childB.point.X + f.rectangle.X, childB.point.Y + f.rectangle.Y,
                                            (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                            (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                        .Contains(e.Location) ||
                                    (childB.bmp.GetPixel(e.X - childB.point.X - f.rectangle.X + childB.rectangle.X,
                                            e.Y - childB.point.Y - f.rectangle.Y + childB.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseClic)) continue;
                                childB.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(Anim))
                            {
                                Anim childA = t as Anim;
                                if (!f.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                    !new Rectangle(childA.img.point.X + f.rectangle.X, childA.img.point.Y + f.rectangle.Y,
                                        (childA.img.isSpriteSheet)
                                            ? childA.img.rectangle.Width
                                            : childA.img.bmp.Width,
                                        (childA.img.isSpriteSheet)
                                            ? childA.img.rectangle.Height
                                            : childA.img.bmp.Height).Contains(e.Location) ||
                                    (childA.img.bmp.GetPixel(
                                            e.X - childA.img.point.X - f.rectangle.X + childA.img.rectangle.X,
                                            e.Y - childA.img.point.Y - f.rectangle.Y + childA.img.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                childA.img.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(Rec))
                            {
                                Rec childR = t as Rec;
                                if (!f.Visible || !childR.Visible ||
                                    !new Rectangle(childR.point.X + f.rectangle.X, childR.point.Y + f.rectangle.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childR.EscapeGfxWhileMouseClic) continue;
                                childR.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t != null && t.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon childF = t as FillPolygon;
                                if (!f.Visible || !childF.Visible ||
                                    !new Rectangle(childF.rectangle.X + f.rectangle.X, childF.rectangle.Y + f.rectangle.Y,
                                        childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location)) continue;
                                SolidBrush sb = childF.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childF.EscapeGfxWhileMouseClic) continue;
                                childF.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t == null || t.GetType() != typeof(Txt)) continue;
                            {
                                Txt childR = t as Txt;
                                if (!f.Visible || !childR.Visible ||
                                    !new Rectangle(childR.Location.X + f.rectangle.X, childR.Location.Y + f.rectangle.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                        e.Location)) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childR.EscapeGfxWhileMouseClic) continue;
                                childR.FireMouseClic(e);
                                found = true;
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent

                        if (!found != f.Visible || !f.rectangle.Contains(e.Location)) continue;
                        {
                            SolidBrush sb = f.brush as SolidBrush;
                            if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !f.EscapeGfxWhileMouseClic)
                                continue;
                            f.FireMouseClic(e);
                            found = true;
                            break;
                        }

                        #endregion
                    }
                    if (gfxTopList[cnt - 1] == null || gfxTopList[cnt - 1].GetType() != typeof(Txt)) continue;
                    {
                        #region childs
                        Txt t = gfxTopList[cnt - 1] as Txt;
                        t.Child.Sort(0, t.Child.Count, rzi);
                        foreach (IGfx t1 in t.Child)
                        {
                            if (t1 != null && t1.GetType() == typeof(Bmp))
                            {
                                Bmp childB = t1 as Bmp;
                                if (!t.Visible || !childB.Visible || childB.bmp == null ||
                                    !new Rectangle(childB.point.X + t.Location.X, childB.point.Y + t.Location.Y,
                                            (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                            (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                        .Contains(e.Location) ||
                                    (childB.bmp.GetPixel(e.X - childB.point.X - t.Location.X + childB.rectangle.X,
                                            e.Y - childB.point.Y - t.Location.Y + childB.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseClic)) continue;
                                childB.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t1 != null && t1.GetType() == typeof(Anim))
                            {
                                Anim childA = t1 as Anim;
                                if (!t.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                    !new Rectangle(childA.img.point.X + t.Location.X, childA.img.point.Y + t.Location.Y,
                                        (childA.img.isSpriteSheet)
                                            ? childA.img.rectangle.Width
                                            : childA.img.bmp.Width,
                                        (childA.img.isSpriteSheet)
                                            ? childA.img.rectangle.Height
                                            : childA.img.bmp.Height).Contains(e.Location) ||
                                    (childA.img.bmp.GetPixel(
                                            e.X - childA.img.point.X - t.Location.X + childA.img.rectangle.X,
                                            e.Y - childA.img.point.Y - t.Location.Y + childA.img.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                childA.img.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t1 != null && t1.GetType() == typeof(Rec))
                            {
                                Rec childR = t1 as Rec;
                                if (!t.Visible || !childR.Visible ||
                                    !new Rectangle(childR.point.X + t.Location.X, childR.point.Y + t.Location.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childR.EscapeGfxWhileMouseClic) continue;
                                childR.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t1 != null && t.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon childF = t1 as FillPolygon;
                                if (!t.Visible || !childF.Visible ||
                                    !new Rectangle(childF.rectangle.X + t.Location.X, childF.rectangle.Y + t.Location.Y,
                                        childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location)) continue;
                                SolidBrush sb = childF.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childF.EscapeGfxWhileMouseClic) continue;
                                childF.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            if (t1 == null || t1.GetType() != typeof(Txt)) continue;
                            {
                                Txt childR = t1 as Txt;
                                if (!t.Visible || !childR.Visible ||
                                    !new Rectangle(childR.Location.X + t.Location.X, childR.Location.Y + t.Location.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                        e.Location)) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                    !childR.EscapeGfxWhileMouseClic) continue;
                                childR.FireMouseClic(e);
                                found = true;
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent

                        if (!found != t.Visible ||
                            !new Rectangle(t.Location, TextRenderer.MeasureText(t.Text, t.font)).Contains(e.Location))
                            continue;
                        {
                            SolidBrush sb = t.brush as SolidBrush;
                            if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !t.EscapeGfxWhileMouseClic)
                                continue;
                            t.FireMouseClic(e);
                            found = true;
                            break;
                        }

                        #endregion
                    }
                }

                if (found == false)
                {
                    for (int cnt = gfxObjList.Count; cnt > 0; cnt--)
                    {
                        if (found)
                            break;

                        if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(Bmp))
                        {
                            #region childs
                            Bmp b = gfxObjList[cnt - 1] as Bmp;
                            b.Child.Sort(0, b.Child.Count, rzi);
                            foreach (IGfx t in b.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (!b.Visible || !childB.Visible || childB.bmp == null ||
                                        !new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y,
                                                (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                            .Contains(e.Location) ||
                                        (childB.bmp.GetPixel(e.X - childB.point.X - b.point.X + childB.rectangle.X,
                                                e.Y - childB.point.Y - b.point.Y + childB.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseClic)) continue;
                                    childB.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (!b.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                        !new Rectangle(childA.img.point.X + b.point.X,
                                            childA.img.point.Y + b.point.Y,
                                            (childA.img.isSpriteSheet)
                                                ? childA.img.rectangle.Width
                                                : childA.img.bmp.Width,
                                            (childA.img.isSpriteSheet)
                                                ? childA.img.rectangle.Height
                                                : childA.img.bmp.Height).Contains(e.Location) ||
                                        (childA.img.bmp.GetPixel(
                                                e.X - childA.img.point.X - b.point.X + childA.img.rectangle.X,
                                                e.Y - childA.img.point.Y - b.point.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                    childA.img.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!b.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseClic) continue;
                                    childR.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (!b.Visible || !childF.Visible ||
                                        !new Rectangle(childF.rectangle.X + b.point.X, childF.rectangle.Y + b.point.Y,
                                            childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childF.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childF.EscapeGfxWhileMouseClic) continue;
                                    childF.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (!b.Visible || !childR.Visible ||
                                        !new Rectangle(childR.Location.X + b.point.X, childR.Location.Y + b.point.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseClic) continue;
                                    childR.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !b.Visible || b.bmp == null ||
                                !new Rectangle(b.point.X, b.point.Y,
                                    (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width,
                                    (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location))
                                continue;
                            if (
                                b.bmp.GetPixel(e.X - b.point.X + ((b.isSpriteSheet) ? b.rectangle.X : 0),
                                    e.Y - b.point.Y + ((b.isSpriteSheet) ? b.rectangle.Y : 0)) !=
                                GetPixel(e.X, e.Y) && !b.EscapeGfxWhileMouseClic) continue;
                            b.FireMouseClic(e);
                            found = true;
                            break;

                            #endregion
                        }
                        if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(Anim))
                        {
                            #region childs
                            Anim a = gfxObjList[cnt - 1] as Anim;
                            a.Child.Sort(0, a.Child.Count, rzi);
                            foreach (IGfx t in a.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (!a.img.Visible || !childB.Visible || childB.bmp == null ||
                                        !new Rectangle(childB.point.X + a.img.point.X,
                                                childB.point.Y + a.img.point.Y,
                                                (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                            .Contains(e.Location) || (childB.bmp.GetPixel(
                                                                            e.X - childB.point.X - a.img.point.X +
                                                                            childB.rectangle.X,
                                                                            e.Y - childB.point.Y - a.img.point.Y +
                                                                            childB.rectangle.Y) != GetPixel(e.X, e.Y) &&
                                                                        !childB.EscapeGfxWhileMouseClic)) continue;
                                    childB.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (!a.img.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                        !new Rectangle(childA.img.point.X + a.img.point.X,
                                            childA.img.point.Y + a.img.point.Y,
                                            (childA.img.isSpriteSheet)
                                                ? childA.img.rectangle.Width
                                                : childA.img.bmp.Width,
                                            (childA.img.isSpriteSheet)
                                                ? childA.img.rectangle.Height
                                                : childA.img.bmp.Height).Contains(e.Location) ||
                                        (childA.img.bmp.GetPixel(
                                                e.X - childA.img.point.X - a.img.point.X + childA.img.rectangle.X,
                                                e.Y - childA.img.point.Y - a.img.point.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                    childA.img.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!a.img.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + a.img.point.X,
                                                childR.point.Y + a.img.point.Y, childR.size.Width, childR.size.Height)
                                            .Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseClic) continue;
                                    childR.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (!a.img.Visible || !childF.Visible ||
                                        !new Rectangle(childF.rectangle.X + a.img.point.X, childF.rectangle.Y + a.img.point.Y,
                                            childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childF.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childF.EscapeGfxWhileMouseClic) continue;
                                    childF.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (!a.img.Visible || !childR.Visible ||
                                        !new Rectangle(childR.Location.X + a.img.point.X,
                                            childR.Location.Y + a.img.point.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseClic) continue;
                                    childR.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent
                            if (found || !a.img.Visible || a.img.bmp == null ||
                                !new Rectangle(a.img.point.X, a.img.point.Y,
                                    (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width,
                                    (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(
                                    e.Location) || (a.img.bmp.GetPixel(
                                                        e.X - a.img.point.X +
                                                        ((a.img.isSpriteSheet) ? a.img.rectangle.X : 0),
                                                        e.Y - a.img.point.Y +
                                                        ((a.img.isSpriteSheet) ? a.img.rectangle.Y : 0)) !=
                                                    GetPixel(e.X, e.Y) && !a.img.EscapeGfxWhileMouseClic)) continue;
                            a.img.FireMouseClic(e);
                            found = true;
                            break;
                            #endregion
                        }
                        if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(Rec))
                        {
                            #region childs
                            Rec r = gfxObjList[cnt - 1] as Rec;
                            r.Child.Sort(0, r.Child.Count, rzi);
                            foreach (IGfx t in r.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (!r.Visible || !childB.Visible || childB.bmp == null ||
                                        !new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y,
                                                (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                            .Contains(e.Location)) continue;
                                    if (
                                        childB.bmp.GetPixel(e.X - childB.point.X - r.point.X + childB.rectangle.X,
                                            e.Y - childB.point.Y - r.point.Y + childB.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseClic) continue;
                                    childB.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (!r.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                        !new Rectangle(childA.img.point.X + r.point.X,
                                            childA.img.point.Y + r.point.Y,
                                            (childA.img.isSpriteSheet)
                                                ? childA.img.rectangle.Width
                                                : childA.img.bmp.Width,
                                            (childA.img.isSpriteSheet)
                                                ? childA.img.rectangle.Height
                                                : childA.img.bmp.Height).Contains(e.Location) ||
                                        (childA.img.bmp.GetPixel(
                                                e.X - childA.img.point.X - r.point.X + childA.img.rectangle.X,
                                                e.Y - childA.img.point.Y - r.point.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                    childA.img.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!r.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseClic) continue;
                                    childR.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (!r.Visible || !childF.Visible ||
                                        !new Rectangle(childF.rectangle.X + r.point.X,
                                                childF.rectangle.Y + r.point.Y, childF.rectangle.Width, childF.rectangle.Height)
                                            .Contains(e.Location)) continue;
                                    SolidBrush sb = childF.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childF.EscapeGfxWhileMouseClic) continue;
                                    childF.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (!r.Visible || !childR.Visible ||
                                        !new Rectangle(childR.Location.X + r.point.X, childR.Location.Y + r.point.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseClic) continue;
                                    childR.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent
                            if (found || !r.Visible || !new Rectangle(r.point, r.size).Contains(e.Location))
                                continue;
                            {
                                SolidBrush sb = r.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !r.EscapeGfxWhileMouseClic)
                                    continue;
                                r.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            #endregion
                        }
                        if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(FillPolygon))
                        {
                            #region childs
                            FillPolygon f = gfxObjList[cnt - 1] as FillPolygon;
                            f.Child.Sort(0, f.Child.Count, rzi);
                            foreach (IGfx t in f.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (!f.Visible || !childB.Visible || childB.bmp == null ||
                                        !new Rectangle(childB.point.X + f.rectangle.X, childB.point.Y + f.rectangle.Y,
                                                (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                            .Contains(e.Location)) continue;
                                    if (
                                        childB.bmp.GetPixel(e.X - childB.point.X - f.rectangle.X + childB.rectangle.X,
                                            e.Y - childB.point.Y - f.rectangle.Y + childB.rectangle.Y) !=
                                        GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseClic) continue;
                                    childB.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (!f.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                        !new Rectangle(childA.img.point.X + f.rectangle.X,
                                            childA.img.point.Y + f.rectangle.Y,
                                            (childA.img.isSpriteSheet)
                                                ? childA.img.rectangle.Width
                                                : childA.img.bmp.Width,
                                            (childA.img.isSpriteSheet)
                                                ? childA.img.rectangle.Height
                                                : childA.img.bmp.Height).Contains(e.Location) ||
                                        (childA.img.bmp.GetPixel(
                                                e.X - childA.img.point.X - f.rectangle.X + childA.img.rectangle.X,
                                                e.Y - childA.img.point.Y - f.rectangle.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                    childA.img.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!f.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + f.rectangle.X, childR.point.Y + f.rectangle.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseClic) continue;
                                    childR.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (!f.Visible || !childF.Visible ||
                                        !new Rectangle(childF.rectangle.X + f.rectangle.X,
                                                childF.rectangle.Y + f.rectangle.Y, childF.rectangle.Width, childF.rectangle.Height)
                                            .Contains(e.Location)) continue;
                                    SolidBrush sb = childF.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childF.EscapeGfxWhileMouseClic) continue;
                                    childF.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (!f.Visible || !childR.Visible ||
                                        !new Rectangle(childR.Location.X + f.rectangle.X, childR.Location.Y + f.rectangle.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseClic) continue;
                                    childR.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent
                            if (found || !f.Visible || !f.rectangle.Contains(e.Location))
                                continue;
                            {
                                SolidBrush sb = f.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !f.EscapeGfxWhileMouseClic)
                                    continue;
                                f.FireMouseClic(e);
                                found = true;
                                break;
                            }
                            #endregion
                        }
                        if (gfxObjList[cnt - 1] == null || gfxObjList[cnt - 1].GetType() != typeof(Txt)) continue;
                        {
                            #region childs
                            Txt t = gfxObjList[cnt - 1] as Txt;
                            t.Child.Sort(0, t.Child.Count, rzi);
                            foreach (IGfx t1 in t.Child)
                            {
                                if (t1 != null && t1.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t1 as Bmp;
                                    if (!t.Visible || !childB.Visible || childB.bmp == null ||
                                        !new Rectangle(childB.point.X + t.Location.X, childB.point.Y + t.Location.Y,
                                                (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                            .Contains(e.Location) ||
                                        (childB.bmp.GetPixel(e.X - childB.point.X - t.Location.X + childB.rectangle.X,
                                                e.Y - childB.point.Y - t.Location.Y + childB.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseClic)) continue;
                                    childB.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t1 != null && t1.GetType() == typeof(Anim))
                                {
                                    Anim childA = t1 as Anim;
                                    if (!t.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                        !new Rectangle(childA.img.point.X + t.Location.X,
                                            childA.img.point.Y + t.Location.Y,
                                            (childA.img.isSpriteSheet)
                                                ? childA.img.rectangle.Width
                                                : childA.img.bmp.Width,
                                            (childA.img.isSpriteSheet)
                                                ? childA.img.rectangle.Height
                                                : childA.img.bmp.Height).Contains(e.Location) ||
                                        (childA.img.bmp.GetPixel(
                                                e.X - childA.img.point.X - t.Location.X + childA.img.rectangle.X,
                                                e.Y - childA.img.point.Y - t.Location.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                    childA.img.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t1 != null && t1.GetType() == typeof(Rec))
                                {
                                    Rec childR = t1 as Rec;
                                    if (!t.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + t.Location.X, childR.point.Y + t.Location.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseClic) continue;
                                    childR.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                                if (t1 == null || t1.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t1 as Txt;
                                    if (!t.Visible || !childR.Visible ||
                                        !new Rectangle(childR.Location.X + t.Location.X, childR.Location.Y + t.Location.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseClic) continue;
                                    childR.FireMouseClic(e);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !t.Visible ||
                                !new Rectangle(t.Location, TextRenderer.MeasureText(t.Text, t.font)).Contains(
                                    e.Location)) continue;
                            {
                                SolidBrush sb = t.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !t.EscapeGfxWhileMouseClic)
                                    continue;
                                t.FireMouseClic(e);
                                found = true;
                                break;
                            }

                            #endregion
                        }
                    }
                }

                switch (found)
                {
                    case false:
                        for (int cnt = gfxBgrList.Count(); cnt > 0; cnt--)
                        {
                            if (found)
                                break;

                            if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(Bmp))
                            {
                                #region childs
                                Bmp b = gfxBgrList[cnt - 1] as Bmp;
                                b.Child.Sort(0, b.Child.Count, rzi);
                                foreach (IGfx t in b.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (!b.Visible || !childB.Visible || childB.bmp == null ||
                                            !new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y,
                                                    (childB.isSpriteSheet)
                                                        ? childB.rectangle.Width
                                                        : childB.bmp.Width,
                                                    (childB.isSpriteSheet)
                                                        ? childB.rectangle.Height
                                                        : childB.bmp.Height)
                                                .Contains(e.Location) || (childB.bmp.GetPixel(
                                                                                e.X - childB.point.X - b.point.X +
                                                                                childB.rectangle.X,
                                                                                e.Y - childB.point.Y - b.point.Y +
                                                                                childB.rectangle.Y) !=
                                                                            GetPixel(e.X, e.Y) ||
                                                                            !childB.EscapeGfxWhileMouseClic))
                                            continue;
                                        childB.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (!b.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                            !new Rectangle(childA.img.point.X + b.point.X,
                                                childA.img.point.Y + b.point.Y,
                                                (childA.img.isSpriteSheet)
                                                    ? childA.img.rectangle.Width
                                                    : childA.img.bmp.Width,
                                                (childA.img.isSpriteSheet)
                                                    ? childA.img.rectangle.Height
                                                    : childA.img.bmp.Height).Contains(e.Location)) continue;
                                        if (
                                            childA.img.bmp.GetPixel(
                                                e.X - childA.img.point.X - b.point.X + childA.img.rectangle.X,
                                                e.Y - childA.img.point.Y - b.point.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic) continue;
                                        childA.img.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (!b.Visible || !childR.Visible ||
                                            !new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                                childR.size.Width, childR.size.Height).Contains(e.Location))
                                            continue;
                                        SolidBrush sb = childR.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childR.EscapeGfxWhileMouseClic) continue;
                                        childR.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!b.Visible || !childF.Visible ||
                                            !new Rectangle(childF.rectangle.X + b.point.X, childF.rectangle.Y + b.point.Y,
                                                childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location))
                                            continue;
                                        SolidBrush sb = childF.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childF.EscapeGfxWhileMouseClic) continue;
                                        childF.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t == null || t.GetType() != typeof(Txt)) continue;
                                    {
                                        Txt childR = t as Txt;
                                        if (!b.Visible || !childR.Visible ||
                                            !new Rectangle(childR.Location.X + b.point.X, childR.Location.Y + b.point.Y,
                                                TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                                TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                                e.Location)) continue;
                                        SolidBrush sb = childR.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childR.EscapeGfxWhileMouseClic) continue;
                                        childR.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                }
                                //////////////////////////////////////////////////
                                #endregion
                                #region parent

                                if (found || !b.Visible || b.bmp == null || !new Rectangle(b.point.X, b.point.Y,
                                        (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width,
                                        (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location) ||
                                    (b.bmp.GetPixel(e.X - b.point.X + ((b.isSpriteSheet) ? b.rectangle.X : 0),
                                            e.Y - b.point.Y + ((b.isSpriteSheet) ? b.rectangle.Y : 0)) !=
                                        GetPixel(e.X, e.Y) && !b.EscapeGfxWhileMouseClic)) continue;
                                b.FireMouseClic(e);
                                break;

                                #endregion
                            }
                            if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(Anim))
                            {
                                #region childs
                                Anim a = gfxBgrList[cnt - 1] as Anim;
                                a.Child.Sort(0, a.Child.Count, rzi);
                                foreach (IGfx t in a.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (!a.img.Visible || !childB.Visible || childB.bmp == null ||
                                            !new Rectangle(childB.point.X + a.img.point.X,
                                                    childB.point.Y + a.img.point.Y,
                                                    (childB.isSpriteSheet)
                                                        ? childB.rectangle.Width
                                                        : childB.bmp.Width,
                                                    (childB.isSpriteSheet)
                                                        ? childB.rectangle.Height
                                                        : childB.bmp.Height)
                                                .Contains(e.Location) || (childB.bmp.GetPixel(
                                                                                e.X - childB.point.X - a.img.point.X +
                                                                                childB.rectangle.X,
                                                                                e.Y - childB.point.Y - a.img.point.Y +
                                                                                childB.rectangle.Y) !=
                                                                            GetPixel(e.X, e.Y) &&
                                                                            !childB.EscapeGfxWhileMouseClic))
                                            continue;
                                        childB.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (!a.img.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                            !new Rectangle(childA.img.point.X + a.img.point.X,
                                                childA.img.point.Y + a.img.point.Y,
                                                (childA.img.isSpriteSheet)
                                                    ? childA.img.rectangle.Width
                                                    : childA.img.bmp.Width,
                                                (childA.img.isSpriteSheet)
                                                    ? childA.img.rectangle.Height
                                                    : childA.img.bmp.Height).Contains(e.Location) ||
                                            (childA.img.bmp.GetPixel(
                                                    e.X - childA.img.point.X - a.img.point.X + childA.img.rectangle.X,
                                                    e.Y - childA.img.point.Y - a.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                        childA.img.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (!a.img.Visible || !childR.Visible ||
                                            !new Rectangle(childR.point.X + a.img.point.X,
                                                childR.point.Y + a.img.point.Y, childR.size.Width,
                                                childR.size.Height).Contains(e.Location)) continue;
                                        SolidBrush sb = childR.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childR.EscapeGfxWhileMouseClic) continue;
                                        childR.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!a.img.Visible || !childF.Visible ||
                                            !new Rectangle(childF.rectangle.X + a.img.point.X, childF.rectangle.Y + a.img.point.Y,
                                                childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location))
                                            continue;
                                        SolidBrush sb = childF.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childF.EscapeGfxWhileMouseClic) continue;
                                        childF.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t == null || t.GetType() != typeof(Txt)) continue;
                                    {
                                        Txt childR = t as Txt;
                                        if (!a.img.Visible || !childR.Visible ||
                                            !new Rectangle(childR.Location.X + a.img.point.X,
                                                childR.Location.Y + a.img.point.Y,
                                                TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                                TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                                e.Location)) continue;
                                        SolidBrush sb = childR.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childR.EscapeGfxWhileMouseClic) continue;
                                        childR.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                }
                                //////////////////////////////////////////////////
                                #endregion
                                #region parent

                                if (found || !a.img.Visible || a.img.bmp == null ||
                                    !new Rectangle(a.img.point.X, a.img.point.Y,
                                        (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width,
                                        (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(
                                        e.Location) || (a.img.bmp.GetPixel(
                                                            e.X - a.img.point.X +
                                                            ((a.img.isSpriteSheet) ? a.img.rectangle.X : 0),
                                                            e.Y - a.img.point.Y +
                                                            ((a.img.isSpriteSheet) ? a.img.rectangle.Y : 0)) !=
                                                        GetPixel(e.X, e.Y) && !a.img.EscapeGfxWhileMouseClic))
                                    continue;
                                a.img.FireMouseClic(e);
                                break;

                                #endregion
                            }
                            if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(Rec))
                            {
                                #region childs
                                Rec r = gfxBgrList[cnt - 1] as Rec;
                                r.Child.Sort(0, r.Child.Count, rzi);
                                foreach (IGfx t in r.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (!r.Visible || !childB.Visible || childB.bmp == null ||
                                            !new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y,
                                                    (childB.isSpriteSheet)
                                                        ? childB.rectangle.Width
                                                        : childB.bmp.Width,
                                                    (childB.isSpriteSheet)
                                                        ? childB.rectangle.Height
                                                        : childB.bmp.Height)
                                                .Contains(e.Location) || (childB.bmp.GetPixel(
                                                                                e.X - childB.point.X - r.point.X +
                                                                                childB.rectangle.X,
                                                                                e.Y - childB.point.Y - r.point.Y +
                                                                                childB.rectangle.Y) !=
                                                                            GetPixel(e.X, e.Y) &&
                                                                            !childB.EscapeGfxWhileMouseClic))
                                            continue;
                                        childB.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (!r.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                            !new Rectangle(childA.img.point.X + r.point.X,
                                                childA.img.point.Y + r.point.Y,
                                                (childA.img.isSpriteSheet)
                                                    ? childA.img.rectangle.Width
                                                    : childA.img.bmp.Width,
                                                (childA.img.isSpriteSheet)
                                                    ? childA.img.rectangle.Height
                                                    : childA.img.bmp.Height).Contains(e.Location) ||
                                            (childA.img.bmp.GetPixel(
                                                    e.X - childA.img.point.X - r.point.X + childA.img.rectangle.X,
                                                    e.Y - childA.img.point.Y - r.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                        childA.img.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (!r.Visible || !childR.Visible ||
                                            !new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                                childR.size.Width, childR.size.Height).Contains(e.Location))
                                            continue;
                                        SolidBrush sb = childR.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childR.EscapeGfxWhileMouseClic) continue;
                                        childR.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!r.Visible || !childF.Visible ||
                                            !new Rectangle(childF.rectangle.X + r.point.X, childF.rectangle.Y + r.point.Y,
                                                childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location))
                                            continue;
                                        SolidBrush sb = childF.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childF.EscapeGfxWhileMouseClic) continue;
                                        childF.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t == null || t.GetType() != typeof(Txt)) continue;
                                    {
                                        Txt childR = t as Txt;
                                        if (!r.Visible || !childR.Visible ||
                                            !new Rectangle(childR.Location.X + r.point.X, childR.Location.Y + r.point.Y,
                                                TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                                TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                                e.Location)) continue;
                                        SolidBrush sb = childR.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childR.EscapeGfxWhileMouseClic) continue;
                                        childR.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                }
                                //////////////////////////////////////////////////
                                #endregion
                                #region parent

                                if (found || !r.Visible || !new Rectangle(r.point, r.size).Contains(e.Location))
                                    continue;
                                {
                                    SolidBrush sb = r.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !r.EscapeGfxWhileMouseClic) continue;
                                    r.FireMouseClic(e);
                                    break;
                                }

                                #endregion
                            }
                            if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(FillPolygon))
                            {
                                #region childs
                                FillPolygon f = gfxBgrList[cnt - 1] as FillPolygon;
                                f.Child.Sort(0, f.Child.Count, rzi);
                                foreach (IGfx t in f.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (!f.Visible || !childB.Visible || childB.bmp == null ||
                                            !new Rectangle(childB.point.X + f.rectangle.X, childB.point.Y + f.rectangle.Y,
                                                    (childB.isSpriteSheet)
                                                        ? childB.rectangle.Width
                                                        : childB.bmp.Width,
                                                    (childB.isSpriteSheet)
                                                        ? childB.rectangle.Height
                                                        : childB.bmp.Height)
                                                .Contains(e.Location) || (childB.bmp.GetPixel(
                                                                                e.X - childB.point.X - f.rectangle.X +
                                                                                childB.rectangle.X,
                                                                                e.Y - childB.point.Y - f.rectangle.Y +
                                                                                childB.rectangle.Y) !=
                                                                            GetPixel(e.X, e.Y) &&
                                                                            !childB.EscapeGfxWhileMouseClic))
                                            continue;
                                        childB.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (!f.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                            !new Rectangle(childA.img.point.X + f.rectangle.X,
                                                childA.img.point.Y + f.rectangle.Y,
                                                (childA.img.isSpriteSheet)
                                                    ? childA.img.rectangle.Width
                                                    : childA.img.bmp.Width,
                                                (childA.img.isSpriteSheet)
                                                    ? childA.img.rectangle.Height
                                                    : childA.img.bmp.Height).Contains(e.Location) ||
                                            (childA.img.bmp.GetPixel(
                                                    e.X - childA.img.point.X - f.rectangle.X + childA.img.rectangle.X,
                                                    e.Y - childA.img.point.Y - f.rectangle.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                        childA.img.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (!f.Visible || !childR.Visible ||
                                            !new Rectangle(childR.point.X + f.rectangle.X, childR.point.Y + f.rectangle.Y,
                                                childR.size.Width, childR.size.Height).Contains(e.Location))
                                            continue;
                                        SolidBrush sb = childR.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childR.EscapeGfxWhileMouseClic) continue;
                                        childR.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!f.Visible || !childF.Visible ||
                                            !new Rectangle(childF.rectangle.X + f.rectangle.X, childF.rectangle.Y + f.rectangle.Y,
                                                childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location))
                                            continue;
                                        SolidBrush sb = childF.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childF.EscapeGfxWhileMouseClic) continue;
                                        childF.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t == null || t.GetType() != typeof(Txt)) continue;
                                    {
                                        Txt childR = t as Txt;
                                        if (!f.Visible || !childR.Visible ||
                                            !new Rectangle(childR.Location.X + f.rectangle.X, childR.Location.Y + f.rectangle.Y,
                                                TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                                TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                                e.Location)) continue;
                                        SolidBrush sb = childR.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childR.EscapeGfxWhileMouseClic) continue;
                                        childR.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                }
                                //////////////////////////////////////////////////
                                #endregion
                                #region parent

                                if (found || !f.Visible || !f.rectangle.Contains(e.Location))
                                    continue;
                                {
                                    SolidBrush sb = f.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !f.EscapeGfxWhileMouseClic) continue;
                                    f.FireMouseClic(e);
                                    break;
                                }

                                #endregion
                            }
                            if (gfxBgrList[cnt - 1] == null || gfxBgrList[cnt - 1].GetType() != typeof(Txt))
                                continue;
                            {
                                #region childs
                                Txt t = gfxBgrList[cnt - 1] as Txt;
                                t.Child.Sort(0, t.Child.Count, rzi);
                                foreach (IGfx t1 in t.Child)
                                {
                                    if (t1 != null && t1.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t1 as Bmp;
                                        if (!t.Visible || !childB.Visible || childB.bmp == null ||
                                            !new Rectangle(childB.point.X + t.Location.X, childB.point.Y + t.Location.Y,
                                                    (childB.isSpriteSheet)
                                                        ? childB.rectangle.Width
                                                        : childB.bmp.Width,
                                                    (childB.isSpriteSheet)
                                                        ? childB.rectangle.Height
                                                        : childB.bmp.Height)
                                                .Contains(e.Location) || (childB.bmp.GetPixel(
                                                                                e.X - childB.point.X - t.Location.X +
                                                                                childB.rectangle.X,
                                                                                e.Y - childB.point.Y - t.Location.Y +
                                                                                childB.rectangle.Y) !=
                                                                            GetPixel(e.X, e.Y) &&
                                                                            !childB.EscapeGfxWhileMouseClic))
                                            continue;
                                        childB.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t1 != null && t1.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t1 as Anim;
                                        if (!t.Visible || !childA.img.Visible || childA.img.bmp == null ||
                                            !new Rectangle(childA.img.point.X + t.Location.X,
                                                childA.img.point.Y + t.Location.Y,
                                                (childA.img.isSpriteSheet)
                                                    ? childA.img.rectangle.Width
                                                    : childA.img.bmp.Width,
                                                (childA.img.isSpriteSheet)
                                                    ? childA.img.rectangle.Height
                                                    : childA.img.bmp.Height).Contains(e.Location) ||
                                            (childA.img.bmp.GetPixel(
                                                    e.X - childA.img.point.X - t.Location.X + childA.img.rectangle.X,
                                                    e.Y - childA.img.point.Y - t.Location.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseClic)) continue;
                                        childA.img.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t1 != null && t1.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t1 as Rec;
                                        if (!t.Visible || !childR.Visible ||
                                            !new Rectangle(childR.point.X + t.Location.X, childR.point.Y + t.Location.Y,
                                                childR.size.Width, childR.size.Height).Contains(e.Location))
                                            continue;
                                        SolidBrush sb = childR.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childR.EscapeGfxWhileMouseClic) continue;
                                        childR.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t1 != null && t1.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t1 as FillPolygon;
                                        if (!t.Visible || !childF.Visible ||
                                            !new Rectangle(childF.rectangle.X + t.Location.X, childF.rectangle.Y + t.Location.Y,
                                                childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location))
                                            continue;
                                        SolidBrush sb = childF.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childF.EscapeGfxWhileMouseClic) continue;
                                        childF.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                    if (t1 == null || t1.GetType() != typeof(Txt)) continue;
                                    {
                                        Txt childR = t1 as Txt;
                                        if (!t.Visible || !childR.Visible ||
                                            !new Rectangle(childR.Location.X + t.Location.X, childR.Location.Y + t.Location.Y,
                                                TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                                TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                                e.Location)) continue;
                                        SolidBrush sb = childR.brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                            !childR.EscapeGfxWhileMouseClic) continue;
                                        childR.FireMouseClic(e);
                                        found = true;
                                        break;
                                    }
                                }
                                //////////////////////////////////////////////////
                                #endregion
                                #region parent

                                if (found || !t.Visible ||
                                    !new Rectangle(t.Location, TextRenderer.MeasureText(t.Text, t.font)).Contains(
                                        e.Location)) continue;
                                {
                                    SolidBrush sb = t.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !t.EscapeGfxWhileMouseClic) continue;
                                    t.FireMouseClic(e);
                                    break;
                                }

                                #endregion
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                if (OutputErrorCallBack != null)
                    OutputErrorCallBack("Error \n" + ex);
                else if (ShowErrorsInMessageBox)
                    MessageBox.Show("Error \n" + ex);
            }
        }
    }
}
