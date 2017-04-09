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
                                if (b.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
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
                                        if (MouseOutRecorder.IndexOf(childB) == -1)
                                            MouseOutRecorder.Add(childB);

                                        // inscription dans la liste GfxMouseOver
                                        if (MouseOverRecorder.IndexOf(childB) == -1)
                                        {
                                            MouseOverRecorder.Clear();
                                            MouseOverRecorder.Add(childB);
                                            childB.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                    OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                    childB.ChangeBmp(childB.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                    childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                    OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Anim))
                            {
                                Anim childA = t as Anim;
                                if (b.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
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
                                        if (MouseOutRecorder.IndexOf(childA) == -1)
                                            MouseOutRecorder.Add(childA);

                                        // inscription dans la liste GfxMouseOver
                                        if (MouseOverRecorder.IndexOf(childA) == -1)
                                        {
                                            MouseOverRecorder.Clear();
                                            MouseOverRecorder.Add(childA);
                                            childA.img.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                    OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                    childA.img.ChangeBmp(childA.img.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                    childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                    OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Rec))
                            {
                                Rec childR = t as Rec;
                                if (!b.Visible || !childR.Visible ||
                                    !new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                    MouseOutRecorder.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t != null && t.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon childF = t as FillPolygon;
                                if (!b.Visible || !childF.Visible ||
                                    !new Rectangle(childF.rectangle.X + b.point.X, childF.rectangle.Y + b.point.Y,
                                        childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childF.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childF.EscapeGfxWhileMouseMove)
                                    continue;
                                childF.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childF) == -1)
                                    MouseOutRecorder.Add(childF);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childF) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childF);
                                    childF.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t != null && t.GetType() == typeof(Txt))
                            {
                                Txt childR = t as Txt;
                                if (!b.Visible || !childR.Visible ||
                                    !new Rectangle(childR.Location.X + b.point.X, childR.Location.Y + b.point.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location) ||
                                    !childR.Visible) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                    MouseOutRecorder.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent
                        if (!found && b.bmp != null && b.Visible && new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width, (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location))
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
                                if (MouseOutRecorder.IndexOf(b) == -1)
                                    MouseOutRecorder.Add(b);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(b) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(b);
                                    b.FireMouseOver(e);
                                }
                                break;
                            }
                            if (!(b.Opacity < 1) || !(b.Opacity > 0)) continue;
                            OpacityMouseMoveRecorder.Add(new OldDataMouseMove(b, b.Opacity));
                            b.ChangeBmp(b.path);
                            break;
                        }
                        for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                        {
                            if (b != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                            b.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                            OpacityMouseMoveRecorder.RemoveAt(cnt1);
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
                                if (a.img.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
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
                                        if (MouseOutRecorder.IndexOf(childB) == -1)
                                            MouseOutRecorder.Add(childB);

                                        // inscription dans la liste GfxMouseOver
                                        if (MouseOverRecorder.IndexOf(childB) == -1)
                                        {
                                            MouseOverRecorder.Clear();
                                            MouseOverRecorder.Add(childB);
                                            childB.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                    OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                    childB.ChangeBmp(childB.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                    childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                    OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Anim))
                            {
                                Anim childA = t as Anim;
                                if (a.img.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + a.img.point.X, childA.img.point.Y + a.img.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
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
                                        if (MouseOutRecorder.IndexOf(childA) == -1)
                                            MouseOutRecorder.Add(childA);

                                        // inscription dans la liste GfxMouseOver
                                        if (MouseOverRecorder.IndexOf(childA) == -1)
                                        {
                                            MouseOverRecorder.Clear();
                                            MouseOverRecorder.Add(childA);
                                            childA.img.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                    OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                    childA.img.ChangeBmp(childA.img.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                    childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                    OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Rec))
                            {
                                Rec childR = t as Rec;
                                if (!a.img.Visible || !childR.Visible ||
                                    !new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                    MouseOutRecorder.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t != null && t.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon childF = t as FillPolygon;
                                if (!a.img.Visible || !childF.Visible ||
                                    !new Rectangle(childF.rectangle.X + a.img.point.X, childF.rectangle.Y + a.img.point.Y,
                                        childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childF.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childF.EscapeGfxWhileMouseMove)
                                    continue;
                                childF.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childF) == -1)
                                    MouseOutRecorder.Add(childF);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childF) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childF);
                                    childF.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t != null && t.GetType() == typeof(Txt))
                            {
                                Txt childR = t as Txt;
                                if (!a.img.Visible || !childR.Visible ||
                                    !new Rectangle(childR.Location.X + a.img.point.X, childR.Location.Y + a.img.point.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location) ||
                                    !childR.Visible) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                    MouseOutRecorder.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent

                        if (found || !a.img.Visible || a.img.bmp == null ||
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
                                if (MouseOutRecorder.IndexOf(a) == -1)
                                    MouseOutRecorder.Add(a);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(a) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(a);
                                    a.img.FireMouseOver(e);
                                }
                                break;
                            }
                            if (a.img.Opacity < 1 && a.img.Opacity > 0)
                            {
                                OpacityMouseMoveRecorder.Add(new OldDataMouseMove(a.img, a.img.Opacity));
                                a.img.ChangeBmp(a.img.path);
                                break;
                            }
                            for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                            {
                                if (a.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                a.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                OpacityMouseMoveRecorder.RemoveAt(cnt1);
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
                                if (r.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
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
                                        if (MouseOutRecorder.IndexOf(childB) == -1)
                                            MouseOutRecorder.Add(childB);

                                        // inscription dans la liste GfxMouseOver
                                        if (MouseOverRecorder.IndexOf(childB) == -1)
                                        {
                                            MouseOverRecorder.Clear();
                                            MouseOverRecorder.Add(childB);
                                            childB.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                    OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                    childB.ChangeBmp(childB.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                    childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                    OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Anim))
                            {
                                Anim childA = t as Anim;
                                if (r.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
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
                                        if (MouseOutRecorder.IndexOf(childA) == -1)
                                            MouseOutRecorder.Add(childA);

                                        // inscription dans la liste GfxMouseOver
                                        if (MouseOverRecorder.IndexOf(childA) == -1)
                                        {
                                            MouseOverRecorder.Clear();
                                            MouseOverRecorder.Add(childA);
                                            childA.img.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                    OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                    childA.img.ChangeBmp(childA.img.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                    childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                    OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Rec))
                            {
                                Rec childR = t as Rec;
                                if (!r.Visible || !childR.Visible ||
                                    !new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                    MouseOutRecorder.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t != null && t.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon childF = t as FillPolygon;
                                if (!r.Visible || !childF.Visible ||
                                    !new Rectangle(childF.rectangle.X + r.point.X, childF.rectangle.Y + r.point.Y,
                                        childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childF.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childF.EscapeGfxWhileMouseMove)
                                    continue;
                                childF.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childF) == -1)
                                    MouseOutRecorder.Add(childF);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childF) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childF);
                                    childF.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t != null && t.GetType() == typeof(Txt))
                            {
                                Txt childR = t as Txt;
                                if (!r.Visible || !childR.Visible ||
                                    !new Rectangle(childR.Location.X + r.point.X, childR.Location.Y + r.point.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location) ||
                                    !childR.Visible) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                    MouseOutRecorder.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent

                        if (found || !r.Visible || !new Rectangle(r.point, r.size).Contains(e.Location) || !r.Visible)
                            continue;
                        {
                            SolidBrush sb = r.brush as SolidBrush;
                            if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !r.EscapeGfxWhileMouseMove)
                                continue;
                            r.FireMouseMove(e);
                            found = true;
                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                            if (MouseOutRecorder.IndexOf(r) == -1)
                                MouseOutRecorder.Add(r);

                            // inscription dans la liste GfxMouseOver
                            if (MouseOverRecorder.IndexOf(r) == -1)
                            {
                                MouseOverRecorder.Clear();
                                MouseOverRecorder.Add(r);
                                r.FireMouseOver(e);
                            }
                            break;
                        }

                        #endregion
                    }
                    else if (gfxTopList[cnt - 1] != null && gfxTopList[cnt - 1].GetType() == typeof(FillPolygon))
                    {
                        #region childs
                        FillPolygon f = gfxTopList[cnt - 1] as FillPolygon;
                        f.Child.Sort(0, f.Child.Count, rzi);
                        foreach (IGfx t in f.Child)
                        {
                            if (t != null && t.GetType() == typeof(Bmp))
                            {
                                Bmp childB = t as Bmp;
                                if (f.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + f.rectangle.X, childB.point.Y + f.rectangle.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                {
                                    if (childB.Opacity == 1)
                                    {
                                        if (
                                            childB.bmp.GetPixel(e.X - f.rectangle.X - childB.point.X + childB.rectangle.X,
                                                e.Y - f.rectangle.Y - childB.point.Y + childB.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                        childB.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (MouseOutRecorder.IndexOf(childB) == -1)
                                            MouseOutRecorder.Add(childB);

                                        // inscription dans la liste GfxMouseOver
                                        if (MouseOverRecorder.IndexOf(childB) == -1)
                                        {
                                            MouseOverRecorder.Clear();
                                            MouseOverRecorder.Add(childB);
                                            childB.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                    OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                    childB.ChangeBmp(childB.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                    childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                    OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Anim))
                            {
                                Anim childA = t as Anim;
                                if (f.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + f.rectangle.X, childA.img.point.Y + f.rectangle.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                {
                                    if (childA.img.Opacity == 1)
                                    {
                                        if (
                                            childA.img.bmp.GetPixel(
                                                e.X - f.rectangle.X - childA.img.point.X + childA.img.rectangle.X,
                                                e.Y - f.rectangle.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                        childA.img.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (MouseOutRecorder.IndexOf(childA) == -1)
                                            MouseOutRecorder.Add(childA);

                                        // inscription dans la liste GfxMouseOver
                                        if (MouseOverRecorder.IndexOf(childA) == -1)
                                        {
                                            MouseOverRecorder.Clear();
                                            MouseOverRecorder.Add(childA);
                                            childA.img.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                    OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                    childA.img.ChangeBmp(childA.img.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                    childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                    OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                }
                            }
                            else if (t != null && t.GetType() == typeof(Rec))
                            {
                                Rec childR = t as Rec;
                                if (!f.Visible || !childR.Visible ||
                                    !new Rectangle(childR.point.X + f.rectangle.X, childR.point.Y + f.rectangle.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                    MouseOutRecorder.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t != null && t.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon childF = t as FillPolygon;
                                if (!f.Visible || !childF.Visible ||
                                    !new Rectangle(childF.rectangle.X + f.rectangle.X, childF.rectangle.Y + f.rectangle.Y,
                                        childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childF.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childF.EscapeGfxWhileMouseMove)
                                    continue;
                                childF.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childF) == -1)
                                    MouseOutRecorder.Add(childF);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childF) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childF);
                                    childF.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t != null && t.GetType() == typeof(Txt))
                            {
                                Txt childR = t as Txt;
                                if (!f.Visible || !childR.Visible ||
                                    !new Rectangle(childR.Location.X + f.rectangle.X, childR.Location.Y + f.rectangle.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location) ||
                                    !childR.Visible) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                    MouseOutRecorder.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent

                        if (found || !f.Visible || !f.rectangle.Contains(e.Location) || !f.Visible)
                            continue;
                        {
                            SolidBrush sb = f.brush as SolidBrush;
                            if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !f.EscapeGfxWhileMouseMove)
                                continue;
                            f.FireMouseMove(e);
                            found = true;
                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                            if (MouseOutRecorder.IndexOf(f) == -1)
                                MouseOutRecorder.Add(f);

                            // inscription dans la liste GfxMouseOver
                            if (MouseOverRecorder.IndexOf(f) == -1)
                            {
                                MouseOverRecorder.Clear();
                                MouseOverRecorder.Add(f);
                                f.FireMouseOver(e);
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
                                if (t.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + t.Location.X, childB.point.Y + t.Location.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                {
                                    if (childB.Opacity == 1)
                                    {
                                        if (
                                            childB.bmp.GetPixel(e.X - t.Location.X - childB.point.X + childB.rectangle.X,
                                                e.Y - t.Location.Y - childB.point.Y + childB.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                        childB.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (MouseOutRecorder.IndexOf(childB) == -1)
                                            MouseOutRecorder.Add(childB);

                                        // inscription dans la liste GfxMouseOver
                                        if (MouseOverRecorder.IndexOf(childB) == -1)
                                        {
                                            MouseOverRecorder.Clear();
                                            MouseOverRecorder.Add(childB);
                                            childB.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                    OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                    childB.ChangeBmp(childB.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                    childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                    OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                }
                            }
                            else if (t1 != null && t1.GetType() == typeof(Anim))
                            {
                                Anim childA = t1 as Anim;
                                if (t.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + t.Location.X, childA.img.point.Y + t.Location.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                {
                                    if (childA.img.Opacity == 1)
                                    {
                                        if (
                                            childA.img.bmp.GetPixel(
                                                e.X - t.Location.X - childA.img.point.X + childA.img.rectangle.X,
                                                e.Y - t.Location.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                            GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                        childA.img.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (MouseOutRecorder.IndexOf(childA) == -1)
                                            MouseOutRecorder.Add(childA);

                                        // inscription dans la liste GfxMouseOver
                                        if (MouseOverRecorder.IndexOf(childA) == -1)
                                        {
                                            MouseOverRecorder.Clear();
                                            MouseOverRecorder.Add(childA);
                                            childA.img.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                    OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                    childA.img.ChangeBmp(childA.img.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                    childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                    OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                }
                            }
                            else if (t1 != null && t1.GetType() == typeof(Rec))
                            {
                                Rec childR = t1 as Rec;
                                if (!t.Visible || !childR.Visible ||
                                    !new Rectangle(childR.point.X + t.Location.X, childR.point.Y + t.Location.Y,
                                        childR.size.Width, childR.size.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                    MouseOutRecorder.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t1 != null && t1.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon childF = t1 as FillPolygon;
                                if (!t.Visible || !childF.Visible ||
                                    !new Rectangle(childF.rectangle.X + t.Location.X, childF.rectangle.Y + t.Location.Y,
                                        childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location))
                                    continue;
                                SolidBrush sb = childF.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childF.EscapeGfxWhileMouseMove)
                                    continue;
                                childF.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childF) == -1)
                                    MouseOutRecorder.Add(childF);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childF) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childF);
                                    childF.FireMouseOver(e);
                                }
                                break;
                            }
                            else if (t1 != null && t1.GetType() == typeof(Txt))
                            {
                                Txt childR = t1 as Txt;
                                if (!t.Visible || !childR.Visible ||
                                    !new Rectangle(childR.Location.X + t.Location.X, childR.Location.Y + t.Location.Y,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                        TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location) ||
                                    !childR.Visible) continue;
                                SolidBrush sb = childR.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                    continue;
                                childR.FireMouseMove(e);
                                found = true;

                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                    MouseOutRecorder.Add(childR);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(childR);
                                    childR.FireMouseOver(e);
                                }
                                break;
                            }
                        }
                        //////////////////////////////////////////////////
                        #endregion
                        #region parent

                        if (found || !t.Visible ||
                            !new Rectangle(t.Location, TextRenderer.MeasureText(t.Text, t.font)).Contains(e.Location) ||
                            !t.Visible) continue;
                        {
                            SolidBrush sb = t.brush as SolidBrush;
                            if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !t.EscapeGfxWhileMouseMove)
                                continue;
                            t.FireMouseMove(e);
                            found = true;
                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                            if (MouseOutRecorder.IndexOf(t) == -1)
                                MouseOutRecorder.Add(t);

                            // inscription dans la liste GfxMouseOver
                            if (MouseOverRecorder.IndexOf(t) == -1)
                            {
                                MouseOverRecorder.Clear();
                                MouseOverRecorder.Add(t);
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
                                    if (b.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childB) == -1)
                                                MouseOutRecorder.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childB) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (b.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childA) == -1)
                                                MouseOutRecorder.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childA) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!b.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location) ||
                                        !childR.Visible) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (!b.Visible || !childF.Visible ||
                                        !new Rectangle(childF.rectangle.X + b.point.X, childF.rectangle.Y + b.point.Y,
                                            childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location) ||
                                        !childF.Visible) continue;
                                    SolidBrush sb = childF.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childF.EscapeGfxWhileMouseMove) continue;
                                    childF.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childF) == -1)
                                        MouseOutRecorder.Add(childF);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childF) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childF);
                                        childF.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childR = t as Txt;
                                    if (!b.Visible || !childR.Visible ||
                                        !new Rectangle(childR.Location.X + b.point.X, childR.Location.Y + b.point.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent
                            if (!found && b.Visible && b.bmp != null && new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width, (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location))
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
                                    if (MouseOutRecorder.IndexOf(b) == -1)
                                        MouseOutRecorder.Add(b);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(b) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(b);
                                        b.FireMouseOver(e);
                                    }
                                    break;
                                }
                                if (!(b.Opacity < 1) || !(b.Opacity > 0)) continue;
                                OpacityMouseMoveRecorder.Add(new OldDataMouseMove(b, b.Opacity));
                                b.ChangeBmp(b.path);
                                break;
                            }
                            for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                            {
                                if (b != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                b.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                OpacityMouseMoveRecorder.RemoveAt(cnt1);
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
                                    if (a.img.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childB) == -1)
                                                MouseOutRecorder.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childB) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (childB.Opacity < 1 && childB.Opacity > 0)
                                        {
                                            OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                            childB.ChangeBmp(childB.path);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                        {
                                            if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                            childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                            OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                        }
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (a.img.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + a.img.point.X, childA.img.point.Y + a.img.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childA) == -1)
                                                MouseOutRecorder.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childA) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!a.img.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (!a.img.Visible || !childF.Visible ||
                                        !new Rectangle(childF.rectangle.X + a.img.point.X, childF.rectangle.Y + a.img.point.Y,
                                            childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childF.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childF.EscapeGfxWhileMouseMove) continue;
                                    childF.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childF) == -1)
                                        MouseOutRecorder.Add(childF);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childF) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childF);
                                        childF.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childR = t as Txt;
                                    if (!a.img.Visible || !childR.Visible ||
                                        !new Rectangle(childR.Location.X + a.img.point.X, childR.Location.Y + a.img.point.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !(a.img.Visible & a.img.bmp != null) ||
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
                                    if (MouseOutRecorder.IndexOf(a) == -1)
                                        MouseOutRecorder.Add(a);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(a) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(a);
                                        a.img.FireMouseOver(e);
                                    }
                                    break;
                                }
                                if (a.img.Opacity < 1 && a.img.Opacity > 0)
                                {
                                    OpacityMouseMoveRecorder.Add(new OldDataMouseMove(a.img, a.img.Opacity));
                                    a.img.ChangeBmp(a.img.path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (a.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                    a.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                    OpacityMouseMoveRecorder.RemoveAt(cnt1);
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
                                    if (r.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childB) == -1)
                                                MouseOutRecorder.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childB) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (r.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childA) == -1)
                                                MouseOutRecorder.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childA) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!r.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (!r.Visible || !childF.Visible ||
                                        !new Rectangle(childF.rectangle.X + r.point.X, childF.rectangle.Y + r.point.Y,
                                            childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childF.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childF.EscapeGfxWhileMouseMove) continue;
                                    childF.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childF) == -1)
                                        MouseOutRecorder.Add(childF);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childF) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childF);
                                        childF.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childR = t as Txt;
                                    if (!r.Visible || !childR.Visible ||
                                        !new Rectangle(childR.Location.X + r.point.X, childR.Location.Y + r.point.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !r.Visible || !new Rectangle(r.point, r.size).Contains(e.Location)) continue;
                            {
                                SolidBrush sb = r.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !r.EscapeGfxWhileMouseMove)
                                    continue;
                                r.FireMouseMove(e);
                                found = true;
                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(r) == -1)
                                    MouseOutRecorder.Add(r);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(r) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(r);
                                    r.FireMouseOver(e);
                                }
                                break;
                            }

                            #endregion
                        }
                        else if (gfxObjList[cnt - 1] != null && gfxObjList[cnt - 1].GetType() == typeof(FillPolygon))
                        {
                            #region childs
                            FillPolygon f = gfxObjList[cnt - 1] as FillPolygon;
                            f.Child.Sort(0, f.Child.Count, rzi);
                            foreach (IGfx t in f.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (f.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + f.rectangle.X, childB.point.Y + f.rectangle.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - f.rectangle.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - f.rectangle.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (MouseOutRecorder.IndexOf(childB) == -1)
                                                MouseOutRecorder.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childB) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (f.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + f.rectangle.X, childA.img.point.Y + f.rectangle.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - f.rectangle.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - f.rectangle.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (MouseOutRecorder.IndexOf(childA) == -1)
                                                MouseOutRecorder.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childA) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!f.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + f.rectangle.X, childR.point.Y + f.rectangle.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (!f.Visible || !childF.Visible ||
                                        !new Rectangle(childF.rectangle.X + f.rectangle.X, childF.rectangle.Y + f.rectangle.Y,
                                            childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childF.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childF.EscapeGfxWhileMouseMove) continue;
                                    childF.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childF) == -1)
                                        MouseOutRecorder.Add(childF);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childF) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childF);
                                        childF.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childR = t as Txt;
                                    if (!f.Visible || !childR.Visible ||
                                        !new Rectangle(childR.Location.X + f.rectangle.X, childR.Location.Y + f.rectangle.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !f.Visible || !f.rectangle.Contains(e.Location)) continue;
                            {
                                SolidBrush sb = f.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !f.EscapeGfxWhileMouseMove)
                                    continue;
                                f.FireMouseMove(e);
                                found = true;
                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(f) == -1)
                                    MouseOutRecorder.Add(f);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(f) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(f);
                                    f.FireMouseOver(e);
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
                                    if (t.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + t.Location.X, childB.point.Y + t.Location.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - t.Location.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - t.Location.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (MouseOutRecorder.IndexOf(childB) == -1)
                                                MouseOutRecorder.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childB) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t1 != null && t1.GetType() == typeof(Anim))
                                {
                                    Anim childA = t1 as Anim;
                                    if (t.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + t.Location.X, childA.img.point.Y + t.Location.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - t.Location.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - t.Location.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (MouseOutRecorder.IndexOf(childA) == -1)
                                                MouseOutRecorder.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childA) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t1 != null && t1.GetType() == typeof(Rec))
                                {
                                    Rec childR = t1 as Rec;
                                    if (!t.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + t.Location.X, childR.point.Y + t.Location.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t1 != null && t1.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t1 as FillPolygon;
                                    if (!t.Visible || !childF.Visible ||
                                        !new Rectangle(childF.rectangle.X + t.Location.X, childF.rectangle.Y + t.Location.Y,
                                            childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location)) continue;
                                    SolidBrush sb = childF.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childF.EscapeGfxWhileMouseMove) continue;
                                    childF.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childF) == -1)
                                        MouseOutRecorder.Add(childF);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childF) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childF);
                                        childF.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t1 != null && t1.GetType() == typeof(Txt))
                                {
                                    Txt childR = t1 as Txt;
                                    if (!t.Visible || !childR.Visible ||
                                        !new Rectangle(childR.Location.X + t.Location.X, childR.Location.Y + t.Location.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !t.Visible ||
                                !new Rectangle(t.Location, TextRenderer.MeasureText(t.Text, t.font)).Contains(e.Location))
                                continue;
                            {
                                SolidBrush sb = t.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !t.EscapeGfxWhileMouseMove)
                                    continue;
                                t.FireMouseMove(e);
                                found = true;
                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(t) == -1)
                                    MouseOutRecorder.Add(t);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(t) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(t);
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
                                    if (b.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childB) == -1)
                                                MouseOutRecorder.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childB) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (b.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childA) == -1)
                                                MouseOutRecorder.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childA) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!b.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location) ||
                                        !childR.Visible) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (!b.Visible || !childF.Visible ||
                                        !new Rectangle(childF.rectangle.X + b.point.X, childF.rectangle.Y + b.point.Y,
                                            childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location) ||
                                        !childF.Visible) continue;
                                    SolidBrush sb = childF.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childF.EscapeGfxWhileMouseMove) continue;
                                    childF.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childF) == -1)
                                        MouseOutRecorder.Add(childF);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childF) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childF);
                                        childF.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childT = t as Txt;
                                    if (!b.Visible || !childT.Visible ||
                                        !new Rectangle(childT.Location.X + b.point.X, childT.Location.Y + b.point.Y,
                                            TextRenderer.MeasureText(childT.Text, childT.font).Width,
                                            TextRenderer.MeasureText(childT.Text, childT.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childT.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childT.EscapeGfxWhileMouseMove) continue;
                                    childT.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childT) == -1)
                                        MouseOutRecorder.Add(childT);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childT) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childT);
                                        childT.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !b.Visible || b.bmp == null ||
                                !new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width,
                                    (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location) ||
                                (b.bmp.GetPixel(e.X - b.point.X + ((b.isSpriteSheet) ? b.rectangle.X : 0),
                                     e.Y - b.point.Y + ((b.isSpriteSheet) ? b.rectangle.Y : 0)) != GetPixel(e.X, e.Y) &&
                                 !b.EscapeGfxWhileMouseMove)) continue;
                            b.FireMouseMove(e);
                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                            if (MouseOutRecorder.IndexOf(b) == -1)
                                MouseOutRecorder.Add(b);

                            // inscription dans la liste GfxMouseOver
                            if (MouseOverRecorder.IndexOf(b) == -1)
                            {
                                MouseOverRecorder.Clear();
                                MouseOverRecorder.Add(b);
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
                                    if (a.img.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childB) == -1)
                                                MouseOutRecorder.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childB) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (a.img.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + a.img.point.X, childA.img.point.Y + a.img.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childA) == -1)
                                                MouseOutRecorder.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childA) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (!a.img.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location) ||
                                        !childR.Visible) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (!a.img.Visible || !childF.Visible ||
                                        !new Rectangle(childF.rectangle.X + a.img.point.X, childF.rectangle.Y + a.img.point.Y,
                                            childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location) ||
                                        !childF.Visible) continue;
                                    SolidBrush sb = childF.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childF.EscapeGfxWhileMouseMove) continue;
                                    childF.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childF) == -1)
                                        MouseOutRecorder.Add(childF);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childF) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childF);
                                        childF.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t != null && t.GetType() == typeof(Txt))
                                {
                                    Txt childT = t as Txt;
                                    if (!a.img.Visible || !childT.Visible ||
                                        !new Rectangle(childT.Location.X + a.img.point.X, childT.Location.Y + a.img.point.Y,
                                            TextRenderer.MeasureText(childT.Text, childT.font).Width,
                                            TextRenderer.MeasureText(childT.Text, childT.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childT.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childT.EscapeGfxWhileMouseMove) continue;
                                    childT.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childT) == -1)
                                        MouseOutRecorder.Add(childT);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childT) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childT);
                                        childT.FireMouseOver(e);
                                    }
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
                                                    GetPixel(e.X, e.Y) && !a.img.EscapeGfxWhileMouseMove)) continue;
                            a.img.FireMouseMove(e);
                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                            if (MouseOutRecorder.IndexOf(a) == -1)
                                MouseOutRecorder.Add(a);

                            // inscription dans la liste GfxMouseOver
                            if (MouseOverRecorder.IndexOf(a) == -1)
                            {
                                MouseOverRecorder.Clear();
                                MouseOverRecorder.Add(a);
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
                                    if (r.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childB) == -1)
                                                MouseOutRecorder.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childB) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (r.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
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
                                            if (MouseOutRecorder.IndexOf(childA) == -1)
                                                MouseOutRecorder.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childA) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        else if (childA.img.Opacity < 1 && childA.img.Opacity > 0)
                                        {
                                            OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                            childA.img.ChangeBmp(childA.img.path);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                        {
                                            if (childA.img == OpacityMouseMoveRecorder[cnt1].bmp)
                                            {
                                                childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                                OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                            }
                                        }
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (r.Visible && childR.Visible && new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y, childR.size.Width, childR.size.Height).Contains(e.Location))
                                    {
                                        if (childR.Visible)
                                        {
                                            SolidBrush sb = childR.brush as SolidBrush;
                                            if (sb.Color.ToArgb() == GetPixel(e.X, e.Y).ToArgb() || childR.EscapeGfxWhileMouseMove)
                                            {
                                                childR.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                                    MouseOutRecorder.Add(childR);

                                                // inscription dans la liste GfxMouseOver
                                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                                {
                                                    MouseOverRecorder.Clear();
                                                    MouseOverRecorder.Add(childR);
                                                    childR.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childR = t as FillPolygon;
                                    if (r.Visible && childR.Visible && new Rectangle(childR.rectangle.X + r.point.X, childR.rectangle.Y + r.point.Y, childR.rectangle.Width, childR.rectangle.Height).Contains(e.Location))
                                    {
                                        if (childR.Visible)
                                        {
                                            SolidBrush sb = childR.brush as SolidBrush;
                                            if (sb.Color.ToArgb() == GetPixel(e.X, e.Y).ToArgb() || childR.EscapeGfxWhileMouseMove)
                                            {
                                                childR.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                                    MouseOutRecorder.Add(childR);

                                                // inscription dans la liste GfxMouseOver
                                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                                {
                                                    MouseOverRecorder.Clear();
                                                    MouseOverRecorder.Add(childR);
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
                                    if (r.Visible && childR.Visible && new Rectangle(childR.Location.X + r.point.X, childR.Location.Y + r.point.Y, TextRenderer.MeasureText(childR.Text, childR.font).Width, TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location))
                                    {
                                        if (childR.Visible)
                                        {
                                            SolidBrush sb = childR.brush as SolidBrush;
                                            if (sb.Color.ToArgb() == GetPixel(e.X, e.Y).ToArgb() || childR.EscapeGfxWhileMouseMove)
                                            {
                                                childR.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                                    MouseOutRecorder.Add(childR);

                                                // inscription dans la liste GfxMouseOver
                                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                                {
                                                    MouseOverRecorder.Clear();
                                                    MouseOverRecorder.Add(childR);
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

                            if (found || !r.Visible || !new Rectangle(r.point, r.size).Contains(e.Location)) continue;
                            {
                                SolidBrush sb = r.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !r.EscapeGfxWhileMouseMove)
                                    continue;
                                r.FireMouseMove(e);
                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(r) == -1)
                                    MouseOutRecorder.Add(r);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(r) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(r);
                                    r.FireMouseOver(e);
                                }
                                break;
                            }

                            #endregion
                        }
                        if (gfxBgrList[cnt - 1] != null && gfxBgrList[cnt - 1].GetType() == typeof(FillPolygon))
                        {
                            #region childs
                            FillPolygon f = gfxBgrList[cnt - 1] as FillPolygon;
                            f.Child.Sort(0, f.Child.Count, rzi);
                            ////////// affichage des elements enfants de l'objet Bmp
                            foreach (IGfx t in f.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (f.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + f.rectangle.X, childB.point.Y + f.rectangle.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - f.rectangle.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - f.rectangle.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (MouseOutRecorder.IndexOf(childB) == -1)
                                                MouseOutRecorder.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childB) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (f.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + f.rectangle.X, childA.img.point.Y + f.rectangle.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - f.rectangle.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - f.rectangle.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (MouseOutRecorder.IndexOf(childA) == -1)
                                                MouseOutRecorder.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childA) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        else if (childA.img.Opacity < 1 && childA.img.Opacity > 0)
                                        {
                                            OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                            childA.img.ChangeBmp(childA.img.path);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                        {
                                            if (childA.img == OpacityMouseMoveRecorder[cnt1].bmp)
                                            {
                                                childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                                OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                            }
                                        }
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (f.Visible && childR.Visible && new Rectangle(childR.point.X + f.rectangle.X, childR.point.Y + f.rectangle.Y, childR.size.Width, childR.size.Height).Contains(e.Location))
                                    {
                                        if (childR.Visible)
                                        {
                                            SolidBrush sb = childR.brush as SolidBrush;
                                            if (sb.Color.ToArgb() == GetPixel(e.X, e.Y).ToArgb() || childR.EscapeGfxWhileMouseMove)
                                            {
                                                childR.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                                    MouseOutRecorder.Add(childR);

                                                // inscription dans la liste GfxMouseOver
                                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                                {
                                                    MouseOverRecorder.Clear();
                                                    MouseOverRecorder.Add(childR);
                                                    childR.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childR = t as FillPolygon;
                                    if (f.Visible && childR.Visible && new Rectangle(childR.rectangle.X + f.rectangle.X, childR.rectangle.Y + f.rectangle.Y, childR.rectangle.Width, childR.rectangle.Height).Contains(e.Location))
                                    {
                                        if (childR.Visible)
                                        {
                                            SolidBrush sb = childR.brush as SolidBrush;
                                            if (sb.Color.ToArgb() == GetPixel(e.X, e.Y).ToArgb() || childR.EscapeGfxWhileMouseMove)
                                            {
                                                childR.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                                    MouseOutRecorder.Add(childR);

                                                // inscription dans la liste GfxMouseOver
                                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                                {
                                                    MouseOverRecorder.Clear();
                                                    MouseOverRecorder.Add(childR);
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
                                    if (f.Visible && childR.Visible && new Rectangle(childR.Location.X + f.rectangle.X, childR.Location.Y + f.rectangle.Y, TextRenderer.MeasureText(childR.Text, childR.font).Width, TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(e.Location))
                                    {
                                        if (childR.Visible)
                                        {
                                            SolidBrush sb = childR.brush as SolidBrush;
                                            if (sb.Color.ToArgb() == GetPixel(e.X, e.Y).ToArgb() || childR.EscapeGfxWhileMouseMove)
                                            {
                                                childR.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (MouseOutRecorder.IndexOf(childR) == -1)
                                                    MouseOutRecorder.Add(childR);

                                                // inscription dans la liste GfxMouseOver
                                                if (MouseOverRecorder.IndexOf(childR) == -1)
                                                {
                                                    MouseOverRecorder.Clear();
                                                    MouseOverRecorder.Add(childR);
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
                            if (found || !f.Visible || !f.rectangle.Contains(e.Location)) continue;
                            {
                                SolidBrush sb = f.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !f.EscapeGfxWhileMouseMove)
                                    continue;
                                f.FireMouseMove(e);
                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(f) == -1)
                                    MouseOutRecorder.Add(f);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(f) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(f);
                                    f.FireMouseOver(e);
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
                                    if (t.Visible && childB.Visible && childB.bmp != null && new Rectangle(childB.point.X + t.Location.X, childB.point.Y + t.Location.Y, (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width, (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height).Contains(e.Location))
                                    {
                                        if (childB.Opacity == 1)
                                        {
                                            if (
                                                childB.bmp.GetPixel(
                                                    e.X - t.Location.X - childB.point.X + childB.rectangle.X,
                                                    e.Y - t.Location.Y - childB.point.Y + childB.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childB.EscapeGfxWhileMouseMove) continue;
                                            childB.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (MouseOutRecorder.IndexOf(childB) == -1)
                                                MouseOutRecorder.Add(childB);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childB) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childB);
                                                childB.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childB.Opacity < 1) || !(childB.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childB, childB.Opacity));
                                        childB.ChangeBmp(childB.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childB != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childB.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t1 != null && t1.GetType() == typeof(Anim))
                                {
                                    Anim childA = t1 as Anim;
                                    if (t.Visible && childA.img.Visible && childA.img.bmp != null && new Rectangle(childA.img.point.X + t.Location.X, childA.img.point.Y + t.Location.Y, (childA.img.isSpriteSheet) ? childA.img.rectangle.Width : childA.img.bmp.Width, (childA.img.isSpriteSheet) ? childA.img.rectangle.Height : childA.img.bmp.Height).Contains(e.Location))
                                    {
                                        if (childA.img.Opacity == 1)
                                        {
                                            if (
                                                childA.img.bmp.GetPixel(
                                                    e.X - t.Location.X - childA.img.point.X + childA.img.rectangle.X,
                                                    e.Y - t.Location.Y - childA.img.point.Y + childA.img.rectangle.Y) !=
                                                GetPixel(e.X, e.Y) && !childA.img.EscapeGfxWhileMouseMove) continue;
                                            childA.img.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (MouseOutRecorder.IndexOf(childA) == -1)
                                                MouseOutRecorder.Add(childA);

                                            // inscription dans la liste GfxMouseOver
                                            if (MouseOverRecorder.IndexOf(childA) == -1)
                                            {
                                                MouseOverRecorder.Clear();
                                                MouseOverRecorder.Add(childA);
                                                childA.img.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(childA.img.Opacity < 1) || !(childA.img.Opacity > 0)) continue;
                                        OpacityMouseMoveRecorder.Add(new OldDataMouseMove(childA.img, childA.img.Opacity));
                                        childA.img.ChangeBmp(childA.img.path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < OpacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (childA.img != OpacityMouseMoveRecorder[cnt1].bmp) continue;
                                        childA.img.ChangeBmp(OpacityMouseMoveRecorder[cnt1].bmp.path, OpacityMouseMoveRecorder[cnt1].opacity);
                                        OpacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                }
                                else if (t1 != null && t1.GetType() == typeof(Rec))
                                {
                                    Rec childR = t1 as Rec;
                                    if (!t.Visible || !childR.Visible ||
                                        !new Rectangle(childR.point.X + t.Location.X, childR.point.Y + t.Location.Y,
                                            childR.size.Width, childR.size.Height).Contains(e.Location) ||
                                        !childR.Visible) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t1 != null && t1.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childR = t1 as FillPolygon;
                                    if (!t.Visible || !childR.Visible ||
                                        !new Rectangle(childR.rectangle.X + t.Location.X, childR.rectangle.Y + t.Location.Y,
                                            childR.rectangle.Width, childR.rectangle.Height).Contains(e.Location) ||
                                        !childR.Visible) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                                else if (t1 != null && t1.GetType() == typeof(Txt))
                                {
                                    Txt childR = t1 as Txt;
                                    if (!t.Visible || !childR.Visible ||
                                        !new Rectangle(childR.Location.X + t.Location.X, childR.Location.Y + t.Location.Y,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                            TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                            e.Location)) continue;
                                    SolidBrush sb = childR.brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                                        !childR.EscapeGfxWhileMouseMove) continue;
                                    childR.FireMouseMove(e);
                                    found = true;

                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (MouseOutRecorder.IndexOf(childR) == -1)
                                        MouseOutRecorder.Add(childR);

                                    // inscription dans la liste GfxMouseOver
                                    if (MouseOverRecorder.IndexOf(childR) == -1)
                                    {
                                        MouseOverRecorder.Clear();
                                        MouseOverRecorder.Add(childR);
                                        childR.FireMouseOver(e);
                                    }
                                    break;
                                }
                            }
                            //////////////////////////////////////////////////
                            #endregion
                            #region parent

                            if (found || !t.Visible ||
                                !new Rectangle(t.Location, TextRenderer.MeasureText(t.Text, t.font)).Contains(e.Location))
                                continue;
                            {
                                SolidBrush sb = t.brush as SolidBrush;
                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !t.EscapeGfxWhileMouseMove)
                                    continue;
                                t.FireMouseMove(e);
                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                if (MouseOutRecorder.IndexOf(t) == -1)
                                    MouseOutRecorder.Add(t);

                                // inscription dans la liste GfxMouseOver
                                if (MouseOverRecorder.IndexOf(t) == -1)
                                {
                                    MouseOverRecorder.Clear();
                                    MouseOverRecorder.Add(t);
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
                if (MouseOutRecorder.Count <= 0) return;
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
                            if (b.Visible && MouseOutRecorder.IndexOf(b) != -1 && (!new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width, (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(b) == -1)))
                            {
                                b.FireMouseOut(e);
                                MouseOutRecorder.Remove(b);
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
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (MouseOutRecorder.IndexOf(childF) == -1 ||
                                        (new Rectangle(childF.rectangle.X + b.point.X, childF.rectangle.Y + b.point.Y,
                                             childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childF) != -1)))
                                        continue;
                                    childF.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childF);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + b.point.X, childR.Location.Y + b.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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
                            if (MouseOutRecorder.IndexOf(a) != -1 && (!new Rectangle(a.img.point.X, a.img.point.Y, (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width, (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(a) == -1)))
                            {
                                a.img.FireMouseOut(e);
                                MouseOutRecorder.Remove(a);
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
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + a.img.point.X,
                                             childA.img.point.Y + childA.img.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (MouseOutRecorder.IndexOf(childF) == -1 ||
                                        (new Rectangle(childF.rectangle.X + a.img.point.X, childF.rectangle.Y + a.img.point.Y,
                                             childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childF) != -1)))
                                        continue;
                                    childF.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childF);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + a.img.point.X, childR.Location.Y + a.img.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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
                            if (MouseOutRecorder.IndexOf(r) != -1 && (!new Rectangle(r.point, r.size).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(r) == -1)))
                            {
                                r.FireMouseOut(e);
                                MouseOutRecorder.Remove(r);
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
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t as FillPolygon;
                                    if (MouseOutRecorder.IndexOf(childF) == -1 ||
                                        (new Rectangle(childF.rectangle.X + r.point.X, childF.rectangle.Y + r.point.Y,
                                             childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childF) != -1)))
                                        continue;
                                    childF.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childF);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + r.point.X, childR.Location.Y + r.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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
                            if (MouseOutRecorder.IndexOf(t) != -1 && (!new Rectangle(t.Location, TextRenderer.MeasureText(t.Text, t.font)).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(t) == -1)))
                            {
                                t.FireMouseOut(e);
                                MouseOutRecorder.Remove(t);
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
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + t.Location.X, childB.point.Y + t.Location.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t1 != null && t1.GetType() == typeof(Anim))
                                {
                                    Anim childA = t1 as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + t.Location.X, childA.img.point.Y + t.Location.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t1 != null && t1.GetType() == typeof(Rec))
                                {
                                    Rec childR = t1 as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + t.Location.X, childR.point.Y + t.Location.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t1 == null || t1.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t1 as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + t.Location.X, childR.Location.Y + t.Location.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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
                            if (MouseOutRecorder.IndexOf(b) != -1 && (!new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width, (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(b) == -1)))
                            {
                                b.FireMouseOut(e);
                                MouseOutRecorder.Remove(b);
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
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + b.point.X, childR.Location.Y + b.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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
                            if (MouseOutRecorder.IndexOf(a) != -1 && (!new Rectangle(a.img.point.X, a.img.point.Y, (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width, (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(a) == -1)))
                            {
                                a.img.FireMouseOut(e);
                                MouseOutRecorder.Remove(a);
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
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + a.img.point.X,
                                             childA.img.point.Y + childA.img.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + a.img.point.X, childR.Location.Y + a.img.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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
                            if (MouseOutRecorder.IndexOf(r) != -1 && (!new Rectangle(r.point, r.size).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(r) == -1)))
                            {
                                r.FireMouseOut(e);
                                MouseOutRecorder.Remove(r);
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
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + r.point.X, childR.Location.Y + r.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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
                            if (MouseOutRecorder.IndexOf(r) != -1 && (!new Rectangle(r.Location, TextRenderer.MeasureText(r.Text, r.font)).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(r) == -1)))
                            {
                                r.FireMouseOut(e);
                                MouseOutRecorder.Remove(r);
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
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + r.Location.X, childB.point.Y + r.Location.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + r.Location.X, childA.img.point.Y + r.Location.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.Location.X, childR.point.Y + r.Location.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + r.Location.X, childR.Location.Y + r.Location.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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
                            if (MouseOutRecorder.IndexOf(b) != -1 && (!new Rectangle(b.point.X, b.point.Y, (b.isSpriteSheet) ? b.rectangle.Width : b.bmp.Width, (b.isSpriteSheet) ? b.rectangle.Height : b.bmp.Height).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(b) == -1)))
                            {
                                b.FireMouseOut(e);
                                MouseOutRecorder.Remove(b);
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in b.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + b.point.X, childB.point.Y + b.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + b.point.X, childA.img.point.Y + b.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + b.point.X, childR.point.Y + b.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + b.point.X, childR.Location.Y + b.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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
                            if (MouseOutRecorder.IndexOf(a) != -1 && (!new Rectangle(a.img.point.X, a.img.point.Y, (a.img.isSpriteSheet) ? a.img.rectangle.Width : a.img.bmp.Width, (a.img.isSpriteSheet) ? a.img.rectangle.Height : a.img.bmp.Height).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(a) == -1)))
                            {
                                a.img.FireMouseOut(e);
                                MouseOutRecorder.Remove(a);
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in a.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + a.img.point.X, childB.point.Y + a.img.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Anim childA = t as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + a.img.point.X,
                                             childA.img.point.Y + childA.img.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + a.img.point.X, childR.point.Y + a.img.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + a.img.point.X, childR.Location.Y + a.img.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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
                            #region parent
                            if (MouseOutRecorder.IndexOf(r) != -1 && (!new Rectangle(r.point, r.size).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(r) == -1)))
                            {
                                r.FireMouseOut(e);
                                MouseOutRecorder.Remove(r);
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t in r.Child)
                            {
                                if (t != null && t.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t as Bmp;
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + r.point.X, childB.point.Y + r.point.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Anim))
                                {
                                    Anim childA = t as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + r.point.X, childA.img.point.Y + r.point.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t != null && t.GetType() == typeof(Rec))
                                {
                                    Rec childR = t as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + r.point.X, childR.point.Y + r.point.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t == null || t.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + r.point.X, childR.Location.Y + r.point.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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
                            #region parent
                            if (MouseOutRecorder.IndexOf(t) != -1 && (!new Rectangle(t.Location, TextRenderer.MeasureText(t.Text, t.font)).Contains(e.Location) || (MouseOverRecorder.Count > 0 && MouseOverRecorder.IndexOf(t) == -1)))
                            {
                                t.FireMouseOut(e);
                                MouseOutRecorder.Remove(t);
                                break;
                            }
                            #endregion
                            #region childs
                            foreach (IGfx t1 in t.Child)
                            {
                                if (t1 != null && t1.GetType() == typeof(Bmp))
                                {
                                    Bmp childB = t1 as Bmp;
                                    if (MouseOutRecorder.IndexOf(childB) == -1 ||
                                        (new Rectangle(childB.point.X + t.Location.X, childB.point.Y + t.Location.Y,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Width : childB.bmp.Width,
                                                 (childB.isSpriteSheet) ? childB.rectangle.Height : childB.bmp.Height)
                                             .Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childB) != -1)))
                                        continue;
                                    childB.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childB);
                                    found = true;
                                    break;
                                }
                                if (t1 != null && t1.GetType() == typeof(Anim))
                                {
                                    Anim childA = t1 as Anim;
                                    if (MouseOutRecorder.IndexOf(childA) == -1 ||
                                        (new Rectangle(childA.img.point.X + t.Location.X, childA.img.point.Y + t.Location.Y,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Width
                                                 : childA.img.bmp.Width,
                                             (childA.img.isSpriteSheet)
                                                 ? childA.img.rectangle.Height
                                                 : childA.img.bmp.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childA) != -1)))
                                        continue;
                                    childA.img.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childA);
                                    found = true;
                                    break;
                                }
                                if (t1 != null && t1.GetType() == typeof(Rec))
                                {
                                    Rec childR = t1 as Rec;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.point.X + t.Location.X, childR.point.Y + t.Location.Y,
                                             childR.size.Width, childR.size.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
                                    found = true;
                                    break;
                                }
                                if (t1 != null && t1.GetType() == typeof(FillPolygon))
                                {
                                    FillPolygon childF = t1 as FillPolygon;
                                    if (MouseOutRecorder.IndexOf(childF) == -1 ||
                                        (new Rectangle(childF.rectangle.X + t.Location.X, childF.rectangle.Y + t.Location.Y,
                                             childF.rectangle.Width, childF.rectangle.Height).Contains(e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childF) != -1)))
                                        continue;
                                    childF.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childF);
                                    found = true;
                                    break;
                                }
                                if (t1 == null || t1.GetType() != typeof(Txt)) continue;
                                {
                                    Txt childR = t1 as Txt;
                                    if (MouseOutRecorder.IndexOf(childR) == -1 ||
                                        (new Rectangle(childR.Location.X + t.Location.X, childR.Location.Y + t.Location.Y,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Width,
                                             TextRenderer.MeasureText(childR.Text, childR.font).Height).Contains(
                                             e.Location) &&
                                         (MouseOverRecorder.Count <= 0 || MouseOverRecorder.IndexOf(childR) != -1)))
                                        continue;
                                    childR.FireMouseOut(e);
                                    MouseOutRecorder.Remove(childR);
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