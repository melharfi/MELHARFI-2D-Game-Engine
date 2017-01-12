using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MELHARFI.Gfx;

namespace MELHARFI
{
    public partial class Manager
    {
        /// <summary>
        /// Mouse Move Event
        /// </summary>
        /// <param name="e">e is a MouseEventArgs object with params like mouse button, position ...</param>
        private void MouseMoveHandleEvents(MouseEventArgs e)
        {
            try
            {
                #region Handeling MouseMove/MouseOver Event
                bool found = false;

                // nétoyage des listes des objets null
                GfxBgrList.RemoveAll(f => f == null);
                GfxObjList.RemoveAll(f => f == null);
                GfxTopList.RemoveAll(f => f == null);

                List<IGfx> gfxBgrList;  // clone du GfxBgrList pour eviter de modifier une liste lors du rendu
                List<IGfx> gfxObjList;  // clone du GfxObjList pour eviter de modifier une liste lors du rendu
                List<IGfx> gfxTopList;  // clone du GfxTopList pour eviter de modifier une liste lors du rendu

                // triage des listes
                Zindex zi = new Zindex();                   // triage des liste pour l'affichage, du plus petit zindex au plus grand
                ReverseZindex rzi = new ReverseZindex();    // triage des liste pour les controles, de plus grand au plus petit
                                                            // créer un miroire des listes pour eviter tous changement lors de l'affichage
                lock ((GfxBgrList as ICollection).SyncRoot)
                    gfxBgrList = GfxBgrList.GetRange(0, GfxBgrList.Count);
                gfxBgrList.Sort(0, gfxBgrList.Count, zi);

                lock ((GfxObjList as ICollection).SyncRoot)
                    gfxObjList = GfxObjList.GetRange(0, GfxObjList.Count);
                gfxObjList.Sort(0, gfxObjList.Count, zi);

                lock ((GfxTopList as ICollection).SyncRoot)
                    gfxTopList = GfxTopList.GetRange(0, GfxTopList.Count);
                gfxTopList.Sort(0, gfxTopList.Count, zi);

                // 1ere List "Top"
                for (int cnt = gfxTopList.Count(); cnt > 0; cnt--)
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
                                if (b.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                {
                                    if (childB.Opacity == 1)
                                    {
                                        if (
                                            childB.bmp.GetPixel(e.X - b.point.X - childB.point.X + childB.rectangle.X,
                                                e.Y - b.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                        childB.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (_gfxMouseOutList.IndexOf(childB) == -1)
                                            _gfxMouseOutList.Add(childB);

                                        // inscription dans la liste GfxMouseOver
                                        if (GfxMouseOverList.IndexOf(childB) == -1)
                                        {
                                            GfxMouseOverList.Clear();
                                            GfxMouseOverList.Add(childB);
                                            childB.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                    GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                    childB.ChangeBmp(childB.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                {
                                    if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                    childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                    GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Anim))
                            {
                                Anim childA = t as Anim;
                                if (b.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                {
                                    if (childA.img.Opacity == 1)
                                    {
                                        if (
                                            childA.img.bmp.GetPixel(
                                                e.X - b.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                e.Y - b.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                        childA.img.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (_gfxMouseOutList.IndexOf(childA) == -1)
                                            _gfxMouseOutList.Add(childA);

                                        // inscription dans la liste GfxMouseOver
                                        if (GfxMouseOverList.IndexOf(childA) == -1)
                                        {
                                            GfxMouseOverList.Clear();
                                            GfxMouseOverList.Add(childA);
                                            childA.img.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                    GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                    childA.img.ChangeBmp(childA.img.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                {
                                    if (childA.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                    childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                    GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Rec))
                            {
                                Rec childR = t as Rec;
                                if (!b.visible || !childR.visible ||
                                    !new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(childR) == -1)
                                    _gfxMouseOutList.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(childR) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t != null && t.GetType() == typeof(Txt))
                            {
                                Txt childR = t as Txt;
                                if (!b.visible || !childR.visible ||
                                    !new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location) ||
                                    !childR.visible) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(childR) == -1)
                                    _gfxMouseOutList.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(childR) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent
                        if (!found && b.bmp != null && b.visible && new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width, (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location))
                        {
                            if (b.Opacity == 1)
                            {
                                if (
                                    b.bmp.GetPixel(e.X - b.point.X + ((b.isSpriteSheet) ? b.rectangle.X : 0),
                                        e.Y - b.point.Y + ((b.isSpriteSheet) ? b.rectangle.Y : 0)) != GetPixel(e.X, e.Y) &&
                                    !b.EscapeGfxWhileMouseMove) continue;
                                b.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(b) == -1)
                                    _gfxMouseOutList.Add(b);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(b) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(b);
                                    b.FireMouseOver(e);
                                }
                                break;
                            }
                            if (!(b.Opacity < 1) || !(b.Opacity > 0)) continue;
                            GfxOpacityMouseMoveList.Add(new OldDataMouseMove(b, b.Opacity));
                            b.ChangeBmp(b.path);
                            break;
                        }
                        for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                        {
                            if (b != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                            b.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                            GfxOpacityMouseMoveList.RemoveAt(cnt1);
                        }

                        #endregion
                    }
                    else if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(Anim))
                    {
                        #region childs
                        Anim a = gfxTopList[cnt - 1] as Anim;
                        a.Child.Sort(0, a.Child.Count, rzi);
                        foreach (IGfx t in a.Child)
                        {
                            if (t != null && t.GetType() == typeof(Bmp))
                            {
                                Bmp childB = t as Bmp;
                                if (a.img.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                {
                                    if (childB.Opacity == 1)
                                    {
                                        if (
                                            childB.bmp.GetPixel(
                                                e.X - a.img.point.X - childB.point.X + childB.rectangle.X,
                                                e.Y - a.img.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                        childB.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (_gfxMouseOutList.IndexOf(childB) == -1)
                                            _gfxMouseOutList.Add(childB);

                                        // inscription dans la liste GfxMouseOver
                                        if (GfxMouseOverList.IndexOf(childB) == -1)
                                        {
                                            GfxMouseOverList.Clear();
                                            GfxMouseOverList.Add(childB);
                                            childB.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                    GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                    childB.ChangeBmp(childB.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                {
                                    if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                    childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                    GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Anim))
                            {
                                Anim childA = t as Anim;
                                if (a.img.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + a.img.point.X, childA.img.point.Y + a.img.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                {
                                    if (childA.img.Opacity == 1)
                                    {
                                        if (
                                            childA.img.bmp.GetPixel(
                                                e.X - a.img.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                e.Y - a.img.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                        childA.img.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (_gfxMouseOutList.IndexOf(childA) == -1)
                                            _gfxMouseOutList.Add(childA);

                                        // inscription dans la liste GfxMouseOver
                                        if (GfxMouseOverList.IndexOf(childA) == -1)
                                        {
                                            GfxMouseOverList.Clear();
                                            GfxMouseOverList.Add(childA);
                                            childA.img.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                    GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                    childA.img.ChangeBmp(childA.img.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                {
                                    if (childA.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                    childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                    GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Rec))
                            {
                                Rec childR = t as Rec;
                                if (!a.img.visible || !childR.visible ||
                                    !new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(childR) == -1)
                                    _gfxMouseOutList.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(childR) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t != null && t.GetType() == typeof(Txt))
                            {
                                Txt childR = t as Txt;
                                if (!a.img.visible || !childR.visible ||
                                    !new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location) ||
                                    !childR.visible) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(childR) == -1)
                                    _gfxMouseOutList.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(childR) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent

                        if (found || !a.img.visible || a.img.bmp == null ||
                            !new Rectangle(a.img.point.X, a.img.point.Y,
                                (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width,
                                (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(e.Location))
                            continue;
                        {
                            if (a.img.Opacity == 1)
                            {
                                if (
                                    a.img.bmp.GetPixel(
                                        e.X - a.img.point.X + ((a.img.isSpriteSheet) ? a.img.rectangle.X : 0),
                                        e.Y - a.img.point.Y + ((a.img.isSpriteSheet) ? a.img.rectangle.Y : 0)) !=
                                    GetPixel(e.X, e.Y) && !a.img.EscapeGfxWhileMouseMove) continue;
                                a.img.FireMouseMove(e);
                                found = true;
                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(a) == -1)
                                    _gfxMouseOutList.Add(a);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(a) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(a);
                                    a.img.FireMouseOver(e);
                                }
                                break;
                            }
                            if (a.img.Opacity < 1 && a.img.Opacity > 0)
                            {
                                GfxOpacityMouseMoveList.Add(new OldDataMouseMove(a.img, a.img.Opacity));
                                a.img.ChangeBmp(a.img.path);
                                break;
                            }
                            for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                            {
                                if (a.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                a.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                GfxOpacityMouseMoveList.RemoveAt(cnt1);
                            }
                        }

                        #endregion
                    }
                    else if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(Rec))
                    {
                        #region childs
                        Rec r = gfxTopList[cnt - 1] as Rec;
                        r.Child.Sort(0, r.Child.Count, rzi);
                        foreach (IGfx t in r.Child)
                        {
                            if (t != null && t.GetType() == typeof(Bmp))
                            {
                                Bmp childB = t as Bmp;
                                if (r.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                {
                                    if (childB.Opacity == 1)
                                    {
                                        if (
                                            childB.bmp.GetPixel(e.X - r.point.X - childB.point.X + childB.rectangle.X,
                                                e.Y - r.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                        childB.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (_gfxMouseOutList.IndexOf(childB) == -1)
                                            _gfxMouseOutList.Add(childB);

                                        // inscription dans la liste GfxMouseOver
                                        if (GfxMouseOverList.IndexOf(childB) == -1)
                                        {
                                            GfxMouseOverList.Clear();
                                            GfxMouseOverList.Add(childB);
                                            childB.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                    GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                    childB.ChangeBmp(childB.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                {
                                    if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                    childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                    GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Anim))
                            {
                                Anim childA = t as Anim;
                                if (r.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                {
                                    if (childA.img.Opacity == 1)
                                    {
                                        if (
                                            childA.img.bmp.GetPixel(
                                                e.X - r.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                e.Y - r.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                        childA.img.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (_gfxMouseOutList.IndexOf(childA) == -1)
                                            _gfxMouseOutList.Add(childA);

                                        // inscription dans la liste GfxMouseOver
                                        if (GfxMouseOverList.IndexOf(childA) == -1)
                                        {
                                            GfxMouseOverList.Clear();
                                            GfxMouseOverList.Add(childA);
                                            childA.img.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                    GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                    childA.img.ChangeBmp(childA.img.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                {
                                    if (childA.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                    childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                    GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Rec))
                            {
                                Rec childR = t as Rec;
                                if (!r.visible || !childR.visible ||
                                    !new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(childR) == -1)
                                    _gfxMouseOutList.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(childR) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t != null && t.GetType() == typeof(Txt))
                            {
                                Txt childR = t as Txt;
                                if (!r.visible || !childR.visible ||
                                    !new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location) ||
                                    !childR.visible) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(childR) == -1)
                                    _gfxMouseOutList.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(childR) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent

                        if (found || !r.visible || !new Rectangle(r.point, r.size).Contains(e.Location) || !r.visible)
                            continue;
                        {
                            SolidBrush sb = r.brush as SolidBrush;
                            if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !r.EscapeGfxWhileMouseMove)
                                continue;
                            r.FireMouseMove(e);
                            found = true;
                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                            if (_gfxMouseOutList.IndexOf(r) == -1)
                                _gfxMouseOutList.Add(r);

                            // inscription dans la liste GfxMouseOver
                            if (GfxMouseOverList.IndexOf(r) == -1)
                            {
                                GfxMouseOverList.Clear();
                                GfxMouseOverList.Add(r);
                                r.FireMouseOver(e);
                            }
                            break;
                        }

                        #endregion
                    }
                    else if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(Txt))
                    {
                        #region childs
                        Txt t = gfxTopList[cnt - 1] as Txt;
                        t.Child.Sort(0, t.Child.Count, rzi);
                        foreach (IGfx t1 in t.Child)
                        {
                            if (t1 != null && t1.GetType() == typeof(Bmp))
                            {
                                Bmp childB = t1 as Bmp;
                                if (t.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + t.point.X, childB.point.Y + t.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                {
                                    if (childB.Opacity == 1)
                                    {
                                        if (
                                            childB.bmp.GetPixel(e.X - t.point.X - childB.point.X + childB.rectangle.X,
                                                e.Y - t.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                        childB.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (_gfxMouseOutList.IndexOf(childB) == -1)
                                            _gfxMouseOutList.Add(childB);

                                        // inscription dans la liste GfxMouseOver
                                        if (GfxMouseOverList.IndexOf(childB) == -1)
                                        {
                                            GfxMouseOverList.Clear();
                                            GfxMouseOverList.Add(childB);
                                            childB.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                    GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                    childB.ChangeBmp(childB.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                {
                                    if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                    childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                    GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                }
                            }
                            else if (t1 != null && t1.GetType() == typeof(Anim))
                            {
                                Anim childA = t1 as Anim;
                                if (t.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + t.point.X, childA.img.point.Y + t.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                {
                                    if (childA.img.Opacity == 1)
                                    {
                                        if (
                                            childA.img.bmp.GetPixel(
                                                e.X - t.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                e.Y - t.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                        childA.img.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (_gfxMouseOutList.IndexOf(childA) == -1)
                                            _gfxMouseOutList.Add(childA);

                                        // inscription dans la liste GfxMouseOver
                                        if (GfxMouseOverList.IndexOf(childA) == -1)
                                        {
                                            GfxMouseOverList.Clear();
                                            GfxMouseOverList.Add(childA);
                                            childA.img.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                    GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                    childA.img.ChangeBmp(childA.img.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                {
                                    if (childA.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                    childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                    GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                }
                            }
                            else if (t1 != null && t1.GetType() == typeof(Rec))
                            {
                                Rec childR = t1 as Rec;
                                if (!t.visible || !childR.visible ||
                                    !new Rectangle(childR.point.X + t.point.X, childR.point.Y + t.point.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(childR) == -1)
                                    _gfxMouseOutList.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(childR) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t1 != null && t1.GetType() == typeof(Txt))
                            {
                                Txt childR = t1 as Txt;
                                if (!t.visible || !childR.visible ||
                                    !new Rectangle(childR.point.X + t.point.X, childR.point.Y + t.point.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location) ||
                                    !childR.visible) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(childR) == -1)
                                    _gfxMouseOutList.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(childR) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent

                        if (found || !t.visible ||
                            !new Rectangle(t.point, TextRenderer.MeasureText(t.Text, t.font)).Contains(e.Location) ||
                            !t.visible) continue;
                        {
                            SolidBrush sb = t.brush as SolidBrush;
                            if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !t.EscapeGfxWhileMouseMove)
                                continue;
                            t.FireMouseMove(e);
                            found = true;
                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                            if (_gfxMouseOutList.IndexOf(t) == -1)
                                _gfxMouseOutList.Add(t);

                            // inscription dans la liste GfxMouseOver
                            if (GfxMouseOverList.IndexOf(t) == -1)
                            {
                                GfxMouseOverList.Clear();
                                GfxMouseOverList.Add(t);
                                t.FireMouseOver(e);
                            }
                            break;
                        }

                        #endregion
                    }
                }

                // 2eme List Obj
                if (found == false)
                {
                    for (int cnt = gfxObjList.Count(); cnt > 0; cnt--)
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
                                    if (b.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - b.point.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - b.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childB) == -1)
                                                _gfxMouseOutList.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childB) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (b.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - b.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - b.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childA) == -1)
                                                _gfxMouseOutList.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childA) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childA.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!b.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location) ||
                                        !childR.visible) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childR = t as Txt;
                                    if (!b.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent
                            if (!found && b.visible && b.bmp != null && new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width, (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location))
                            {
                                if (b.Opacity == 1)
                                {
                                    if (
                                        b.bmp.GetPixel(e.X - b.point.X + ((b.isSpriteSheet) ? b.rectangle.X : 0),
                                            e.Y - b.point.Y + ((b.isSpriteSheet) ? b.rectangle.Y : 0)) !=
                                        GetPixel(e.X, e.Y) && !b.EscapeGfxWhileMouseMove) continue;
                                    b.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(b) == -1)
                                        _gfxMouseOutList.Add(b);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(b) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(b);
                                        b.FireMouseOver(e);
                                    }
                                    break;
                                }
                                if (!(b.Opacity < 1) || !(b.Opacity > 0)) continue;
                                GfxOpacityMouseMoveList.Add(new OldDataMouseMove(b, b.Opacity));
                                b.ChangeBmp(b.path);
                                break;
                            }
                            for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                            {
                                if (b != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                b.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                GfxOpacityMouseMoveList.RemoveAt(cnt1);
                            }

                            #endregion
                        }
                        else if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(Anim))
                        {
                            #region childs
                            Anim a = gfxObjList[cnt - 1] as Anim;
                            a.Child.Sort(0, a.Child.Count, rzi);
                            foreach (IGfx t in a.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (a.img.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - a.img.point.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - a.img.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childB) == -1)
                                                _gfxMouseOutList.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childB) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (childB.Opacity < 1 && childB.Opacity > 0)
                                        {
                                            GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                            childB.ChangeBmp(childB.path);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                        {
                                            if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                            childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                            GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                        }
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (a.img.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + a.img.point.X, childA.img.point.Y + a.img.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - a.img.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - a.img.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childA) == -1)
                                                _gfxMouseOutList.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childA) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childA.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!a.img.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childR = t as Txt;
                                    if (!a.img.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !(a.img.visible & a.img.bmp != null) ||
                                !new Rectangle(a.img.point.X, a.img.point.Y,
                                    (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width,
                                    (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(
                                    e.Location)) continue;
                            {
                                if (a.img.Opacity == 1)
                                {
                                    if (
                                        a.img.bmp.GetPixel(
                                            e.X - a.img.point.X + ((a.img.isSpriteSheet) ? a.img.rectangle.X : 0),
                                            e.Y - a.img.point.Y + ((a.img.isSpriteSheet) ? a.img.rectangle.Y : 0)) !=
                                        GetPixel(e.X, e.Y) && !a.img.EscapeGfxWhileMouseMove) continue;
                                    a.img.FireMouseMove(e);
                                    found = true;
                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(a) == -1)
                                        _gfxMouseOutList.Add(a);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(a) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(a);
                                        a.img.FireMouseOver(e);
                                    }
                                    break;
                                }
                                if (a.img.Opacity < 1 && a.img.Opacity > 0)
                                {
                                    GfxOpacityMouseMoveList.Add(new OldDataMouseMove(a.img, a.img.Opacity));
                                    a.img.ChangeBmp(a.img.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                {
                                    if (a.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                    a.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                    GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                }
                            }

                            #endregion
                        }
                        else if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(Rec))
                        {
                            #region childs
                            Rec r = gfxObjList[cnt - 1] as Rec;
                            r.Child.Sort(0, r.Child.Count, rzi);
                            foreach (IGfx t in r.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (r.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - r.point.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - r.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childB) == -1)
                                                _gfxMouseOutList.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childB) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (r.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - r.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - r.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childA) == -1)
                                                _gfxMouseOutList.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childA) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childA.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!r.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childR = t as Txt;
                                    if (!r.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !r.visible || !new Rectangle(r.point, r.size).Contains(e.Location)) continue;
                            {
                                SolidBrush sb = r.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !r.EscapeGfxWhileMouseMove)
                                    continue;
                                r.FireMouseMove(e);
                                found = true;
                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(r) == -1)
                                    _gfxMouseOutList.Add(r);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(r) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(r);
                                    r.FireMouseOver(e);
                                }
                                break;
                            }

                            #endregion
                        }
                        else if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(Txt))
                        {
                            #region childs
                            Txt t = gfxObjList[cnt - 1] as Txt;
                            t.Child.Sort(0, t.Child.Count, rzi);
                            foreach (IGfx t1 in t.Child)
                            {
                                if (t1 != null && t1.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t1 as Bmp;
                                    if (t.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + t.point.X, childB.point.Y + t.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - t.point.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - t.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childB) == -1)
                                                _gfxMouseOutList.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childB) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t1 != null && t1.GetType() == typeof(Anim))
                                {
                                    Anim childA = t1 as Anim;
                                    if (t.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + t.point.X, childA.img.point.Y + t.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - t.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - t.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childA) == -1)
                                                _gfxMouseOutList.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childA) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childA.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t1 != null && t1.GetType() == typeof(Rec))
                                {
                                    Rec childR = t1 as Rec;
                                    if (!t.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + t.point.X, childR.point.Y + t.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t1 != null && t1.GetType() == typeof(Txt))
                                {
                                    Txt childR = t1 as Txt;
                                    if (!t.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + t.point.X, childR.point.Y + t.point.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !t.visible ||
                                !new Rectangle(t.point, TextRenderer.MeasureText(t.Text, t.font)).Contains(e.Location))
                                continue;
                            {
                                SolidBrush sb = t.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !t.EscapeGfxWhileMouseMove)
                                    continue;
                                t.FireMouseMove(e);
                                found = true;
                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(t) == -1)
                                    _gfxMouseOutList.Add(t);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(t) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(t);
                                    t.FireMouseOver(e);
                                }
                                break;
                            }

                            #endregion
                        }
                    }
                }

                if (found == false)
                {
                    for (int cnt = gfxBgrList.Count(); cnt > 0; cnt--)
                    {
                        if (found)
                            break;

                        if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(Bmp))
                        {
                            #region childs
                            Bmp b = gfxBgrList[cnt - 1] as Bmp;
                            b.Child.Sort(0, b.Child.Count, rzi);
                            ////////// affichage des elements enfants de l'objet Bmp
                            foreach (IGfx t in b.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (b.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - b.point.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - b.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childB) == -1)
                                                _gfxMouseOutList.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childB) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (b.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - b.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - b.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childA) == -1)
                                                _gfxMouseOutList.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childA) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childA.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!b.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location) ||
                                        !childR.visible) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childT = t as Txt;
                                    if (!b.visible || !childT.visible ||
                                        !new Rectangle(childT.point.X + b.point.X, childT.point.Y + b.point.Y,
                                            TextRenderer.MeasureText(childT.Text, childT.font).Width,
                                            TextRenderer.MeasureText(childT.Text, childT.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childT.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childT.EscapeGfxWhileMouseMove) continue;
                                    childT.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childT) == -1)
                                        _gfxMouseOutList.Add(childT);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childT) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childT);
                                        childT.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !b.visible || b.bmp == null ||
                                !new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width,
                                    (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location) ||
                                (b.bmp.GetPixel(e.X - b.point.X + ((b.isSpriteSheet) ? b.rectangle.X : 0),
                                     e.Y - b.point.Y + ((b.isSpriteSheet) ? b.rectangle.Y : 0)) != GetPixel(e.X, e.Y) &&
                                 !b.EscapeGfxWhileMouseMove)) continue;
                            b.FireMouseMove(e);
                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                            if (_gfxMouseOutList.IndexOf(b) == -1)
                                _gfxMouseOutList.Add(b);

                            // inscription dans la liste GfxMouseOver
                            if (GfxMouseOverList.IndexOf(b) == -1)
                            {
                                GfxMouseOverList.Clear();
                                GfxMouseOverList.Add(b);
                                b.FireMouseOver(e);
                            }
                            break;

                            #endregion
                        }
                        if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(Anim))
                        {
                            #region childs
                            Anim a = gfxBgrList[cnt - 1] as Anim;
                            a.Child.Sort(0, a.Child.Count, rzi);
                            ////////// affichage des elements enfants de l'objet Bmp
                            foreach (IGfx t in a.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (a.img.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - a.img.point.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - a.img.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childB) == -1)
                                                _gfxMouseOutList.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childB) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (a.img.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + a.img.point.X, childA.img.point.Y + a.img.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - a.img.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - a.img.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childA) == -1)
                                                _gfxMouseOutList.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childA) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childA.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!a.img.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location) ||
                                        !childR.visible) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childT = t as Txt;
                                    if (!a.img.visible || !childT.visible ||
                                        !new Rectangle(childT.point.X + a.img.point.X, childT.point.Y + a.img.point.Y,
                                            TextRenderer.MeasureText(childT.Text, childT.font).Width,
                                            TextRenderer.MeasureText(childT.Text, childT.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childT.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childT.EscapeGfxWhileMouseMove) continue;
                                    childT.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childT) == -1)
                                        _gfxMouseOutList.Add(childT);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childT) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childT);
                                        childT.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !a.img.visible || a.img.bmp == null ||
                                !new Rectangle(a.img.point.X, a.img.point.Y,
                                    (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width,
                                    (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(
                                    e.Location) || (a.img.bmp.GetPixel(
                                                        e.X - a.img.point.X +
                                                        ((a.img.isSpriteSheet) ? a.img.rectangle.X : 0),
                                                        e.Y - a.img.point.Y +
                                                        ((a.img.isSpriteSheet) ? a.img.rectangle.Y : 0)) !=
                                                    GetPixel(e.X, e.Y) && !a.img.EscapeGfxWhileMouseMove)) continue;
                            a.img.FireMouseMove(e);
                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                            if (_gfxMouseOutList.IndexOf(a) == -1)
                                _gfxMouseOutList.Add(a);

                            // inscription dans la liste GfxMouseOver
                            if (GfxMouseOverList.IndexOf(a) == -1)
                            {
                                GfxMouseOverList.Clear();
                                GfxMouseOverList.Add(a);
                                a.img.FireMouseOver(e);
                            }
                            break;

                            #endregion
                        }
                        if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(Rec))
                        {
                            #region childs
                            Rec r = gfxBgrList[cnt - 1] as Rec;
                            r.Child.Sort(0, r.Child.Count, rzi);
                            ////////// affichage des elements enfants de l'objet Bmp
                            foreach (IGfx t in r.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (r.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - r.point.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - r.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childB) == -1)
                                                _gfxMouseOutList.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childB) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (r.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - r.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - r.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childA) == -1)
                                                _gfxMouseOutList.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childA) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        else if (childA.img.Opacity < 1 && childA.img.Opacity > 0)
                                        {
                                            GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                            childA.img.ChangeBmp(childA.img.path);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                        {
                                            if (childA.img == GfxOpacityMouseMoveList[cnt1].bmp)
                                            {
                                                childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                                GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                            }
                                        }
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (r.visible && childR.visible && new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y, childR.size.Width, childR.size.Height).Contains(e.Location))
                                    {
                                        if (childR.visible)
                                        {
                                            SolidBrush sb = childR.brush as SolidBrush;
                                            if (sb.Color.ToArgb() == GetPixel(e.X, e.Y).ToArgb() || childR.EscapeGfxWhileMouseMove)
                                            {
                                                childR.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (_gfxMouseOutList.IndexOf(childR) == -1)
                                                    _gfxMouseOutList.Add(childR);

                                                // inscription dans la liste GfxMouseOver
                                                if (GfxMouseOverList.IndexOf(childR) == -1)
                                                {
                                                    GfxMouseOverList.Clear();
                                                    GfxMouseOverList.Add(childR);
                                                    childR.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childR = t as Txt;
                                    if (r.visible && childR.visible && new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y, TextRenderer.MeasureText(childR.Text, childR.font).Width, TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location))
                                    {
                                        if (childR.visible)
                                        {
                                            SolidBrush sb = childR.brush as SolidBrush;
                                            if (sb.Color.ToArgb() == GetPixel(e.X, e.Y).ToArgb() || childR.EscapeGfxWhileMouseMove)
                                            {
                                                childR.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (_gfxMouseOutList.IndexOf(childR) == -1)
                                                    _gfxMouseOutList.Add(childR);

                                                // inscription dans la liste GfxMouseOver
                                                if (GfxMouseOverList.IndexOf(childR) == -1)
                                                {
                                                    GfxMouseOverList.Clear();
                                                    GfxMouseOverList.Add(childR);
                                                    childR.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !r.visible || !new Rectangle(r.point, r.size).Contains(e.Location)) continue;
                            {
                                SolidBrush sb = r.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !r.EscapeGfxWhileMouseMove)
                                    continue;
                                r.FireMouseMove(e);
                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(r) == -1)
                                    _gfxMouseOutList.Add(r);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(r) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(r);
                                    r.FireMouseOver(e);
                                }
                                break;
                            }

                            #endregion
                        }
                        if (gfxBgrList[cnt - 1] == null || gfxBgrList[cnt - 1].GetType() != typeof(Txt)) continue;
                        {
                            #region childs
                            Txt t = gfxBgrList[cnt - 1] as Txt;
                            t.Child.Sort(0, t.Child.Count, rzi);
                            ////////// affichage des elements enfants de l'objet Bmp
                            foreach (IGfx t1 in t.Child)
                            {
                                if (t1 != null && t1.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t1 as Bmp;
                                    if (t.visible && childB.visible && childB.bmp != null && new Rectangle(childB.point.X + t.point.X, childB.point.Y + t.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - t.point.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - t.point.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childB) == -1)
                                                _gfxMouseOutList.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childB) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childB != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childB.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t1 != null && t1.GetType() == typeof(Anim))
                                {
                                    Anim childA = t1 as Anim;
                                    if (t.visible && childA.img.visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + t.point.X, childA.img.point.Y + t.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - t.point.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - t.point.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (_gfxMouseOutList.IndexOf(childA) == -1)
                                                _gfxMouseOutList.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (GfxMouseOverList.IndexOf(childA) == -1)
                                            {
                                                GfxMouseOverList.Clear();
                                                GfxMouseOverList.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        GfxOpacityMouseMoveList.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < GfxOpacityMouseMoveList.Count; cnt1++)
                                    {
                                        if (childA.img != GfxOpacityMouseMoveList[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(GfxOpacityMouseMoveList[cnt1].bmp.path, GfxOpacityMouseMoveList[cnt1].opacity);
                                        GfxOpacityMouseMoveList.RemoveAt(cnt1);
                                    }
                                }
                                else if (t1 != null && t1.GetType() == typeof(Rec))
                                {
                                    Rec childR = t1 as Rec;
                                    if (!t.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + t.point.X, childR.point.Y + t.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location) ||
                                        !childR.visible) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t1 != null && t1.GetType() == typeof(Txt))
                                {
                                    Txt childR = t1 as Txt;
                                    if (!t.visible || !childR.visible ||
                                        !new Rectangle(childR.point.X + t.point.X, childR.point.Y + t.point.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (_gfxMouseOutList.IndexOf(childR) == -1)
                                        _gfxMouseOutList.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (GfxMouseOverList.IndexOf(childR) == -1)
                                    {
                                        GfxMouseOverList.Clear();
                                        GfxMouseOverList.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !t.visible ||
                                !new Rectangle(t.point, TextRenderer.MeasureText(t.Text, t.font)).Contains(e.Location))
                                continue;
                            {
                                SolidBrush sb = t.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !t.EscapeGfxWhileMouseMove)
                                    continue;
                                t.FireMouseMove(e);
                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (_gfxMouseOutList.IndexOf(t) == -1)
                                    _gfxMouseOutList.Add(t);

                                // inscription dans la liste GfxMouseOver
                                if (GfxMouseOverList.IndexOf(t) == -1)
                                {
                                    GfxMouseOverList.Clear();
                                    GfxMouseOverList.Add(t);
                                    t.FireMouseOver(e);
                                }
                                break;
                            }

                            #endregion
                        }
                    }
                }
                #endregion
                #region handeling MouseOut
                ///////////////// handeling MouseOut //////////////////////
                if (_gfxMouseOutList.Count <= 0) return;
                {
                    found = false;

                    for (int cnt = gfxTopList.Count(); cnt > 0; cnt--)
                    {
                        if (found)
                            break;

                        if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(Bmp))
                        {
                            Bmp b = gfxTopList[cnt - 1] as Bmp;
                            b.Child.Sort(0, b.Child.Count, rzi);
                            #region parent
                            if (b.visible && _gfxMouseOutList.IndexOf(b) != -1 && (!new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width, (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(b) == -1)))
                            {
                                b.FireMouseOut(e);
                                _gfxMouseOutList.Remove(b);
                                found = true;
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in b.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                        else if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(Anim))
                        {
                            Anim a = gfxTopList[cnt - 1] as Anim;
                            a.Child.Sort(0, a.Child.Count, rzi);
                            #region parent
                            if (_gfxMouseOutList.IndexOf(a) != -1 && (!new Rectangle(a.img.point.X, a.img.point.Y, (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width, (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(a) == -1)))
                            {
                                a.img.FireMouseOut(e);
                                _gfxMouseOutList.Remove(a);
                                found = true;
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in a.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + a.img.point.X,
                                             childA.img.point.Y + childA.img.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                        else if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(Rec))
                        {
                            Rec r = gfxTopList[cnt - 1] as Rec;
                            r.Child.Sort(0, r.Child.Count, rzi);
                            #region parent
                            if (_gfxMouseOutList.IndexOf(r) != -1 && (!new Rectangle(r.point, r.size).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(r) == -1)))
                            {
                                r.FireMouseOut(e);
                                _gfxMouseOutList.Remove(r);
                                found = true;
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in r.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                        else if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(Txt))
                        {
                            Txt t = gfxTopList[cnt - 1] as Txt;
                            t.Child.Sort(0, t.Child.Count, rzi);
                            #region parent
                            if (_gfxMouseOutList.IndexOf(t) != -1 && (!new Rectangle(t.point, TextRenderer.MeasureText(t.Text, t.font)).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(t) == -1)))
                            {
                                t.FireMouseOut(e);
                                _gfxMouseOutList.Remove(t);
                                found = true;
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t1 in t.Child)
                            {
                                if (t1 != null && t1.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t1 as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + t.point.X, childB.point.Y + t.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t1 != null && t1.GetType() == typeof(Anim))
                                {
                                    Anim childA = t1 as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + t.point.X, childA.img.point.Y + t.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t1 != null && t1.GetType() == typeof(Rec))
                                {
                                    Rec childR = t1 as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + t.point.X, childR.point.Y + t.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t1 == null || t1.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t1 as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + t.point.X, childR.point.Y + t.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                    }

                    for (int cnt = gfxObjList.Count(); cnt > 0; cnt--)
                    {
                        if (found)
                            break;

                        if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(Bmp))
                        {
                            Bmp b = gfxObjList[cnt - 1] as Bmp;
                            b.Child.Sort(0, b.Child.Count, rzi);
                            #region parent
                            if (_gfxMouseOutList.IndexOf(b) != -1 && (!new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width, (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(b) == -1)))
                            {
                                b.FireMouseOut(e);
                                _gfxMouseOutList.Remove(b);
                                found = true;
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in b.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                        else if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(Anim))
                        {
                            Anim a = gfxObjList[cnt - 1] as Anim;
                            a.Child.Sort(0, a.Child.Count, rzi);
                            #region parent
                            if (_gfxMouseOutList.IndexOf(a) != -1 && (!new Rectangle(a.img.point.X, a.img.point.Y, (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width, (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(a) == -1)))
                            {
                                a.img.FireMouseOut(e);
                                _gfxMouseOutList.Remove(a);
                                found = true;
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in a.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + a.img.point.X,
                                             childA.img.point.Y + childA.img.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                        else if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(Rec))
                        {
                            Rec r = gfxObjList[cnt - 1] as Rec;
                            r.Child.Sort(0, r.Child.Count, rzi);
                            #region parent
                            if (_gfxMouseOutList.IndexOf(r) != -1 && (!new Rectangle(r.point, r.size).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(r) == -1)))
                            {
                                r.FireMouseOut(e);
                                _gfxMouseOutList.Remove(r);
                                found = true;
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in r.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                        else if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(Txt))
                        {
                            Txt r = gfxObjList[cnt - 1] as Txt;
                            r.Child.Sort(0, r.Child.Count, rzi);
                            #region parent
                            if (_gfxMouseOutList.IndexOf(r) != -1 && (!new Rectangle(r.point, TextRenderer.MeasureText(r.Text, r.font)).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(r) == -1)))
                            {
                                r.FireMouseOut(e);
                                _gfxMouseOutList.Remove(r);
                                found = true;
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in r.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                    }

                    for (int cnt = gfxBgrList.Count(); cnt > 0; cnt--)
                    {
                        if (found)
                            break;

                        if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(Bmp))
                        {
                            Bmp b = gfxBgrList[cnt - 1] as Bmp;
                            b.Child.Sort(0, b.Child.Count, rzi);
                            #region parent
                            if (_gfxMouseOutList.IndexOf(b) != -1 && (!new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width, (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(b) == -1)))
                            {
                                b.FireMouseOut(e);
                                _gfxMouseOutList.Remove(b);
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in b.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                        else if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(Anim))
                        {
                            Anim a = gfxBgrList[cnt - 1] as Anim;
                            a.Child.Sort(0, a.Child.Count, rzi);
                            #region parent
                            if (_gfxMouseOutList.IndexOf(a) != -1 && (!new Rectangle(a.img.point.X, a.img.point.Y, (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width, (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(a) == -1)))
                            {
                                a.img.FireMouseOut(e);
                                _gfxMouseOutList.Remove(a);
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in a.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Anim childA = t as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + a.img.point.X,
                                             childA.img.point.Y + childA.img.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                        else if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(Rec))
                        {
                            Rec r = gfxBgrList[cnt - 1] as Rec;
                            r.Child.Sort(0, r.Child.Count, rzi);
                            #region
                            if (_gfxMouseOutList.IndexOf(r) != -1 && (!new Rectangle(r.point, r.size).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(r) == -1)))
                            {
                                r.FireMouseOut(e);
                                _gfxMouseOutList.Remove(r);
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in r.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                        else if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(Txt))
                        {
                            Txt t = gfxBgrList[cnt - 1] as Txt;
                            t.Child.Sort(0, t.Child.Count, rzi);
                            #region
                            if (_gfxMouseOutList.IndexOf(t) != -1 && (!new Rectangle(t.point, TextRenderer.MeasureText(t.Text, t.font)).Contains(e.Location) || (GfxMouseOverList.Count > 0 && GfxMouseOverList.IndexOf(t) == -1)))
                            {
                                t.FireMouseOut(e);
                                _gfxMouseOutList.Remove(t);
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t1 in t.Child)
                            {
                                if (t1 != null && t1.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t1 as Bmp;
                                    if (_gfxMouseOutList.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + t.point.X, childB.point.Y + t.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t1 != null && t1.GetType() == typeof(Anim))
                                {
                                    Anim childA = t1 as Anim;
                                    if (_gfxMouseOutList.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + t.point.X, childA.img.point.Y + t.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t1 != null && t1.GetType() == typeof(Rec))
                                {
                                    Rec childR = t1 as Rec;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + t.point.X, childR.point.Y + t.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t1 == null || t1.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t1 as Txt;
                                    if (_gfxMouseOutList.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + t.point.X, childR.point.Y + t.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (GfxMouseOverList.Count <= 0 || GfxMouseOverList.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    _gfxMouseOutList.Remove(childR);
                                    found = true;
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                        }
                    }
                }

                #endregion
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