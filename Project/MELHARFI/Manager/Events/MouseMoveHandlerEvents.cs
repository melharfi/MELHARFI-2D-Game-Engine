using MELHARFI.Manager.Gfx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MELHARFI.Manager
{
    public partial class Manager
    {
        /// <summary>
        /// Mouse Move Event
        /// </summary>
        /// <param name="e">e is a MouseEventArgs object with params like mouse button, position ...</param>
        private void MouseMoveHandlerEvents(MouseEventArgs e)
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

                    switch (gfxTopList[cnt - 1].GetType().Name)
                    {
                        case "Bmp":
                            {
                                Bmp p = gfxTopList[cnt - 1] as Bmp;
                                #region childs
                                p.Childs.Sort(0, p.Childs.Count, rzi);
                                foreach (IGfx t in p.Childs)
                                {
                                    switch (t.GetType().Name)
                                    {
                                        case "Bmp":
                                            {
                                                Bmp c = t as Bmp;
                                                if (p.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                {
                                                    if (c.Opacity == 1)
                                                    {
                                                        if (c.Bitmap.GetPixel(e.X - p.Point.X - c.Point.X + c.Rectangle.X,
                                                                e.Y - p.Point.Y - c.Point.Y + c.Rectangle.Y) !=
                                                            GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                        c.FireMouseMove(e);
                                                        found = true;

                                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                        if (mouseOutRecorder.IndexOf(c) == -1)
                                                            mouseOutRecorder.Add(c);

                                                        // inscription dans la liste GfxMouseOver
                                                        if (mouseOverRecorder.IndexOf(c) == -1)
                                                        {
                                                            mouseOverRecorder.Clear();
                                                            mouseOverRecorder.Add(c);
                                                            c.FireMouseOver(e);
                                                        }
                                                        break;
                                                    }
                                                    if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                    opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                    c.ChangeBmp(c.Path);
                                                    break;
                                                }
                                                for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                {
                                                    if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                    c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                    opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                }
                                                break;
                                            }
                                        case "Anim":
                                            {
                                                Anim c = t as Anim;
                                                if (p.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                {
                                                    if (c.Bmp.Opacity == 1)
                                                    {
                                                        if (
                                                            c.Bmp.Bitmap.GetPixel(
                                                                e.X - p.Point.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                e.Y - p.Point.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                            GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                        c.Bmp.FireMouseMove(e);
                                                        found = true;

                                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                        if (mouseOutRecorder.IndexOf(c) == -1)
                                                            mouseOutRecorder.Add(c);

                                                        // inscription dans la liste GfxMouseOver
                                                        if (mouseOverRecorder.IndexOf(c) == -1)
                                                        {
                                                            mouseOverRecorder.Clear();
                                                            mouseOverRecorder.Add(c);
                                                            c.Bmp.FireMouseOver(e);
                                                        }
                                                        break;
                                                    }
                                                    if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                    opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                    c.Bmp.ChangeBmp(c.Bmp.Path);
                                                    break;
                                                }
                                                for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                {
                                                    if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                    c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                    opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                }
                                                break;
                                            }
                                        case "Rec":
                                            {
                                                Rec c = t as Rec;
                                                if (!p.Visible || !c.Visible ||
                                                    !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                        c.Size.Width, c.Size.Height).Contains(e.Location))
                                                    continue;
                                                SolidBrush sb = c.FillColor as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                    continue;
                                                c.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(c) == -1)
                                                    mouseOutRecorder.Add(c);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(c) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(c);
                                                    c.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        case "Txt":
                                            {
                                                Txt c = t as Txt;
                                                if (!p.Visible || !c.Visible ||
                                                    !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                        TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                        TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(e.Location) ||
                                                    !c.Visible) continue;
                                                SolidBrush sb = c.Brush as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                    continue;
                                                c.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(c) == -1)
                                                    mouseOutRecorder.Add(c);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(c) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(c);
                                                    c.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        case "FillPolygon":
                                            {
                                                FillPolygon c = t as FillPolygon;
                                                if (!p.Visible || !c.Visible ||
                                                    !new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                        c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                    continue;
                                                SolidBrush sb = c.FillColor as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                    continue;
                                                c.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(c) == -1)
                                                    mouseOutRecorder.Add(c);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(c) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(c);
                                                    c.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                    }
                                }
                                #endregion
                                #region parent
                                if (!found && p.Bitmap != null && p.Visible && new Rectangle(p.Point.X, p.Point.Y, (p.IsSpriteSheet) ? p.Rectangle.Width : p.Bitmap.Width, (p.IsSpriteSheet) ? p.Rectangle.Height : p.Bitmap.Height).Contains(e.Location))
                                {
                                    if (p.Opacity == 1)
                                    {
                                        if (
                                            p.Bitmap.GetPixel(e.X - p.Point.X + ((p.IsSpriteSheet) ? p.Rectangle.X : 0),
                                                e.Y - p.Point.Y + ((p.IsSpriteSheet) ? p.Rectangle.Y : 0)) != GetPixel(e.X, e.Y) &&
                                            !p.EscapeGfxWhileMouseMove) continue;
                                        p.FireMouseMove(e);
                                        found = true;

                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (mouseOutRecorder.IndexOf(p) == -1)
                                            mouseOutRecorder.Add(p);

                                        // inscription dans la liste GfxMouseOver
                                        if (mouseOverRecorder.IndexOf(p) == -1)
                                        {
                                            mouseOverRecorder.Clear();
                                            mouseOverRecorder.Add(p);
                                            p.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (!(p.Opacity < 1) || !(p.Opacity > 0)) continue;
                                    opacityMouseMoveRecorder.Add(new OldDataMouseMove(p, p.Opacity));
                                    p.ChangeBmp(p.Path);
                                    break;
                                }
                                for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                {
                                    if (p != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                    p.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                    opacityMouseMoveRecorder.RemoveAt(cnt1);
                                }
                                break;
                                #endregion
                            }
                        case "Anim":
                            {
                                Anim p = gfxTopList[cnt - 1] as Anim;
                                #region childs
                                p.Childs.Sort(0, p.Childs.Count, rzi);
                                foreach (IGfx t in p.Childs)
                                {
                                    switch (t.GetType().Name)
                                    {
                                        case "Bmp":
                                            {
                                                Bmp c = t as Bmp;
                                                if (p.Bmp.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                {
                                                    if (c.Opacity == 1)
                                                    {
                                                        if (
                                                            c.Bitmap.GetPixel(
                                                                e.X - p.Bmp.Point.X - c.Point.X + c.Rectangle.X,
                                                                e.Y - p.Bmp.Point.Y - c.Point.Y + c.Rectangle.Y) !=
                                                            GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                        c.FireMouseMove(e);
                                                        found = true;

                                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                        if (mouseOutRecorder.IndexOf(c) == -1)
                                                            mouseOutRecorder.Add(c);

                                                        // inscription dans la liste GfxMouseOver
                                                        if (mouseOverRecorder.IndexOf(c) == -1)
                                                        {
                                                            mouseOverRecorder.Clear();
                                                            mouseOverRecorder.Add(c);
                                                            c.FireMouseOver(e);
                                                        }
                                                        break;
                                                    }
                                                    if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                    opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                    c.ChangeBmp(c.Path);
                                                    break;
                                                }
                                                for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                {
                                                    if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                    c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                    opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                }
                                                break;
                                            }
                                        case "Anim":
                                            {
                                                Anim c = t as Anim;
                                                if (p.Bmp.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Bmp.Point.X, c.Bmp.Point.Y + p.Bmp.Point.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                {
                                                    if (c.Bmp.Opacity == 1)
                                                    {
                                                        if (
                                                            c.Bmp.Bitmap.GetPixel(
                                                                e.X - p.Bmp.Point.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                e.Y - p.Bmp.Point.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                            GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                        c.Bmp.FireMouseMove(e);
                                                        found = true;

                                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                        if (mouseOutRecorder.IndexOf(c) == -1)
                                                            mouseOutRecorder.Add(c);

                                                        // inscription dans la liste GfxMouseOver
                                                        if (mouseOverRecorder.IndexOf(c) == -1)
                                                        {
                                                            mouseOverRecorder.Clear();
                                                            mouseOverRecorder.Add(c);
                                                            c.Bmp.FireMouseOver(e);
                                                        }
                                                        break;
                                                    }
                                                    if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                    opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                    c.Bmp.ChangeBmp(c.Bmp.Path);
                                                    break;
                                                }
                                                for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                {
                                                    if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                    c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                    opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                }
                                                break;
                                            }
                                        case "Rec":
                                            {
                                                Rec c = t as Rec;
                                                if (!p.Bmp.Visible || !c.Visible ||
                                                    !new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                        c.Size.Width, c.Size.Height).Contains(e.Location))
                                                    continue;
                                                SolidBrush sb = c.FillColor as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                    continue;
                                                c.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(c) == -1)
                                                    mouseOutRecorder.Add(c);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(c) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(c);
                                                    c.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        case "Txt":
                                            {
                                                Txt c = t as Txt;
                                                if (!p.Bmp.Visible || !c.Visible ||
                                                    !new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                        TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                        TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(e.Location) ||
                                                    !c.Visible) continue;
                                                SolidBrush sb = c.Brush as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                    continue;
                                                c.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(c) == -1)
                                                    mouseOutRecorder.Add(c);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(c) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(c);
                                                    c.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        case "FillPolygon":
                                            {
                                                FillPolygon c = t as FillPolygon;
                                                if (!p.Bmp.Visible || !c.Visible ||
                                                    !new Rectangle(c.Rectangle.X + p.Bmp.Point.X, c.Rectangle.Y + p.Bmp.Point.Y,
                                                        c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                    continue;
                                                SolidBrush sb = c.FillColor as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                    continue;
                                                c.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(c) == -1)
                                                    mouseOutRecorder.Add(c);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(c) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(c);
                                                    c.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                    }
                                }
                                #endregion
                                #region parent
                                if (found || !p.Bmp.Visible || p.Bmp.Bitmap == null ||
                                    !new Rectangle(p.Bmp.Point.X, p.Bmp.Point.Y,
                                        (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Width : p.Bmp.Bitmap.Width,
                                        (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Height : p.Bmp.Bitmap.Height).Contains(e.Location))
                                    continue;
                                {
                                    if (p.Bmp.Opacity == 1)
                                    {
                                        if (
                                            p.Bmp.Bitmap.GetPixel(
                                                e.X - p.Bmp.Point.X + ((p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.X : 0),
                                                e.Y - p.Bmp.Point.Y + ((p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Y : 0)) !=
                                            GetPixel(e.X, e.Y) && !p.Bmp.EscapeGfxWhileMouseMove) continue;
                                        p.Bmp.FireMouseMove(e);
                                        found = true;
                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (mouseOutRecorder.IndexOf(p) == -1)
                                            mouseOutRecorder.Add(p);

                                        // inscription dans la liste GfxMouseOver
                                        if (mouseOverRecorder.IndexOf(p) == -1)
                                        {
                                            mouseOverRecorder.Clear();
                                            mouseOverRecorder.Add(p);
                                            p.Bmp.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    if (p.Bmp.Opacity < 1 && p.Bmp.Opacity > 0)
                                    {
                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(p.Bmp, p.Bmp.Opacity));
                                        p.Bmp.ChangeBmp(p.Bmp.Path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (p.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                        p.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                    break;
                                }

                                #endregion
                            }
                        case "Rec":
                            {
                                Rec p = gfxTopList[cnt - 1] as Rec;
                                #region childs
                                p.Childs.Sort(0, p.Childs.Count, rzi);
                                foreach (IGfx t in p.Childs)
                                {
                                    switch (t.GetType().Name)
                                    {
                                        case "Bmp":
                                            {
                                                Bmp c = t as Bmp;
                                                if (p.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                {
                                                    if (c.Opacity == 1)
                                                    {
                                                        if (
                                                            c.Bitmap.GetPixel(e.X - p.Point.X - c.Point.X + c.Rectangle.X,
                                                                e.Y - p.Point.Y - c.Point.Y + c.Rectangle.Y) !=
                                                            GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                        c.FireMouseMove(e);
                                                        found = true;

                                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                        if (mouseOutRecorder.IndexOf(c) == -1)
                                                            mouseOutRecorder.Add(c);

                                                        // inscription dans la liste GfxMouseOver
                                                        if (mouseOverRecorder.IndexOf(c) == -1)
                                                        {
                                                            mouseOverRecorder.Clear();
                                                            mouseOverRecorder.Add(c);
                                                            c.FireMouseOver(e);
                                                        }
                                                        break;
                                                    }
                                                    if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                    opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                    c.ChangeBmp(c.Path);
                                                    break;
                                                }
                                                for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                {
                                                    if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                    c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                    opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                }
                                                break;
                                            }
                                        case "Anim":
                                            {
                                                Anim c = t as Anim;
                                                if (p.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                {
                                                    if (c.Bmp.Opacity == 1)
                                                    {
                                                        if (
                                                            c.Bmp.Bitmap.GetPixel(
                                                                e.X - p.Point.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                e.Y - p.Point.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                            GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                        c.Bmp.FireMouseMove(e);
                                                        found = true;

                                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                        if (mouseOutRecorder.IndexOf(c) == -1)
                                                            mouseOutRecorder.Add(c);

                                                        // inscription dans la liste GfxMouseOver
                                                        if (mouseOverRecorder.IndexOf(c) == -1)
                                                        {
                                                            mouseOverRecorder.Clear();
                                                            mouseOverRecorder.Add(c);
                                                            c.Bmp.FireMouseOver(e);
                                                        }
                                                        break;
                                                    }
                                                    if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                    opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                    c.Bmp.ChangeBmp(c.Bmp.Path);
                                                    break;
                                                }
                                                for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                {
                                                    if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                    c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                    opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                }
                                                break;
                                            }
                                        case "Rec":
                                            {
                                                Rec c = t as Rec;
                                                if (!p.Visible || !c.Visible ||
                                                    !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                        c.Size.Width, c.Size.Height).Contains(e.Location))
                                                    continue;
                                                SolidBrush sb = c.FillColor as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                    continue;
                                                c.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(c) == -1)
                                                    mouseOutRecorder.Add(c);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(c) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(c);
                                                    c.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        case "Txt":
                                            {
                                                Txt childR = t as Txt;
                                                if (!p.Visible || !childR.Visible ||
                                                    !new Rectangle(childR.Point.X + p.Point.X, childR.Point.Y + p.Point.Y,
                                                        TextRenderer.MeasureText(childR.Text, childR.Font).Width,
                                                        TextRenderer.MeasureText(childR.Text, childR.Font).Height).Contains(e.Location) ||
                                                    !childR.Visible) continue;
                                                SolidBrush sb = childR.Brush as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                                    continue;
                                                childR.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(childR) == -1)
                                                    mouseOutRecorder.Add(childR);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(childR) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(childR);
                                                    childR.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        case "FillPolygon":
                                            {
                                                FillPolygon c = t as FillPolygon;
                                                if (!p.Visible || !c.Visible ||
                                                    !new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                        c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                    continue;
                                                SolidBrush sb = c.FillColor as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                    continue;
                                                c.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(c) == -1)
                                                    mouseOutRecorder.Add(c);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(c) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(c);
                                                    c.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                    }
                                }
                                #endregion
                                #region parent
                                if (found || !p.Visible || !new Rectangle(p.Point, p.Size).Contains(e.Location) || !p.Visible)
                                    continue;
                                {
                                    SolidBrush sb = p.FillColor as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseMove)
                                        continue;
                                    p.FireMouseMove(e);
                                    found = true;
                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (mouseOutRecorder.IndexOf(p) == -1)
                                        mouseOutRecorder.Add(p);

                                    // inscription dans la liste GfxMouseOver
                                    if (mouseOverRecorder.IndexOf(p) == -1)
                                    {
                                        mouseOverRecorder.Clear();
                                        mouseOverRecorder.Add(p);
                                        p.FireMouseOver(e);
                                    }
                                    break;
                                }
                                #endregion
                            }
                        case "Txt":
                            {
                                Txt p = gfxTopList[cnt - 1] as Txt;
                                #region parent
                                if (found || !p.Visible || !new Rectangle(p.Point, TextRenderer.MeasureText(p.Text, p.Font)).Contains(e.Location) || !p.Visible) continue;
                                {
                                    SolidBrush sb = p.Brush as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseMove)
                                        continue;
                                    p.FireMouseMove(e);
                                    found = true;
                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (mouseOutRecorder.IndexOf(p) == -1)
                                        mouseOutRecorder.Add(p);

                                    // inscription dans la liste GfxMouseOver
                                    if (mouseOverRecorder.IndexOf(p) == -1)
                                    {
                                        mouseOverRecorder.Clear();
                                        mouseOverRecorder.Add(p);
                                        p.FireMouseOver(e);
                                    }
                                    break;
                                }
                                #endregion
                            }
                        case "FillPolygon":
                            {
                                FillPolygon p = gfxTopList[cnt - 1] as FillPolygon;
                                #region childs
                                p.Childs.Sort(0, p.Childs.Count, rzi);
                                foreach (IGfx t in p.Childs)
                                {
                                    switch (t.GetType().Name)
                                    {
                                        case "Bmp":
                                            {
                                                Bmp c = t as Bmp;
                                                if (p.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Rectangle.X, c.Point.Y + p.Rectangle.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                {
                                                    if (c.Opacity == 1)
                                                    {
                                                        if (
                                                            c.Bitmap.GetPixel(e.X - p.Rectangle.X - c.Point.X + c.Rectangle.X,
                                                                e.Y - p.Rectangle.Y - c.Point.Y + c.Rectangle.Y) !=
                                                            GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                        c.FireMouseMove(e);
                                                        found = true;

                                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                        if (mouseOutRecorder.IndexOf(c) == -1)
                                                            mouseOutRecorder.Add(c);

                                                        // inscription dans la liste GfxMouseOver
                                                        if (mouseOverRecorder.IndexOf(c) == -1)
                                                        {
                                                            mouseOverRecorder.Clear();
                                                            mouseOverRecorder.Add(c);
                                                            c.FireMouseOver(e);
                                                        }
                                                        break;
                                                    }
                                                    if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                    opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                    c.ChangeBmp(c.Path);
                                                    break;
                                                }
                                                for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                {
                                                    if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                    c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                    opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                }
                                                break;
                                            }
                                        case "Anim":
                                            {
                                                Anim c = t as Anim;
                                                if (p.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Rectangle.X, c.Bmp.Point.Y + p.Rectangle.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                {
                                                    if (c.Bmp.Opacity == 1)
                                                    {
                                                        if (
                                                            c.Bmp.Bitmap.GetPixel(
                                                                e.X - p.Rectangle.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                e.Y - p.Rectangle.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                            GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                        c.Bmp.FireMouseMove(e);
                                                        found = true;

                                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                        if (mouseOutRecorder.IndexOf(c) == -1)
                                                            mouseOutRecorder.Add(c);

                                                        // inscription dans la liste GfxMouseOver
                                                        if (mouseOverRecorder.IndexOf(c) == -1)
                                                        {
                                                            mouseOverRecorder.Clear();
                                                            mouseOverRecorder.Add(c);
                                                            c.Bmp.FireMouseOver(e);
                                                        }
                                                        break;
                                                    }
                                                    if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                    opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                    c.Bmp.ChangeBmp(c.Bmp.Path);
                                                    break;
                                                }
                                                for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                {
                                                    if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                    c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                    opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                }
                                                break;
                                            }
                                        case "Rec":
                                            {
                                                Rec c = t as Rec;
                                                if (!p.Visible || !c.Visible ||
                                                    !new Rectangle(c.Point.X + p.Rectangle.X, c.Point.Y + p.Rectangle.Y,
                                                        c.Size.Width, c.Size.Height).Contains(e.Location))
                                                    continue;
                                                SolidBrush sb = c.FillColor as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                    continue;
                                                c.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(c) == -1)
                                                    mouseOutRecorder.Add(c);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(c) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(c);
                                                    c.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        case "Txt":
                                            {
                                                Txt c = t as Txt;
                                                if (!p.Visible || !c.Visible ||
                                                    !new Rectangle(c.Point.X + p.Rectangle.X, c.Point.Y + p.Rectangle.Y,
                                                        TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                        TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(e.Location) ||
                                                    !c.Visible) continue;
                                                SolidBrush sb = c.Brush as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                    continue;
                                                c.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(c) == -1)
                                                    mouseOutRecorder.Add(c);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(c) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(c);
                                                    c.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                        case "FillPolygon":
                                            {
                                                FillPolygon c = t as FillPolygon;
                                                if (!p.Visible || !c.Visible ||
                                                    !new Rectangle(c.Rectangle.X + p.Rectangle.X, c.Rectangle.Y + p.Rectangle.Y,
                                                        c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                    continue;
                                                SolidBrush sb = c.FillColor as SolidBrush;
                                                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                    continue;
                                                c.FireMouseMove(e);
                                                found = true;

                                                // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                if (mouseOutRecorder.IndexOf(c) == -1)
                                                    mouseOutRecorder.Add(c);

                                                // inscription dans la liste GfxMouseOver
                                                if (mouseOverRecorder.IndexOf(c) == -1)
                                                {
                                                    mouseOverRecorder.Clear();
                                                    mouseOverRecorder.Add(c);
                                                    c.FireMouseOver(e);
                                                }
                                                break;
                                            }
                                    }
                                }
                                #endregion
                                #region parent
                                if (found || !p.Visible || !p.Rectangle.Contains(e.Location) || !p.Visible)
                                    continue;
                                {
                                    SolidBrush sb = p.FillColor as SolidBrush;
                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseMove)
                                        continue;
                                    p.FireMouseMove(e);
                                    found = true;
                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                    if (mouseOutRecorder.IndexOf(p) == -1)
                                        mouseOutRecorder.Add(p);

                                    // inscription dans la liste GfxMouseOver
                                    if (mouseOverRecorder.IndexOf(p) == -1)
                                    {
                                        mouseOverRecorder.Clear();
                                        mouseOverRecorder.Add(p);
                                        p.FireMouseOver(e);
                                    }
                                    break;
                                }
                                #endregion
                            }
                    }
                }

                // 2eme List Obj
                if (found == false)
                {
                    for (int cnt = gfxObjList.Count(); cnt > 0; cnt--)
                    {
                        if (found)
                            break;

                        switch (gfxObjList[cnt - 1].GetType().Name)
                        {
                            case "Bmp":
                                {
                                    Bmp p = gfxObjList[cnt - 1] as Bmp;
                                    #region childs
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    foreach (IGfx t in p.Childs)
                                    {
                                        switch (t.GetType().Name)
                                        {
                                            case "Bmp":
                                                {
                                                    Bmp c = t as Bmp;
                                                    if (p.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Opacity == 1)
                                                        {
                                                            if (c.Bitmap.GetPixel(e.X - p.Point.X - c.Point.X + c.Rectangle.X,
                                                                    e.Y - p.Point.Y - c.Point.Y + c.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                            c.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                        c.ChangeBmp(c.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (p.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Bmp.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bmp.Bitmap.GetPixel(
                                                                    e.X - p.Point.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                    e.Y - p.Point.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                            c.Bmp.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.Bmp.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                        c.Bmp.ChangeBmp(c.Bmp.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                            c.Size.Width, c.Size.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "Txt":
                                                {
                                                    Txt c = t as Txt;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(e.Location) ||
                                                        !c.Visible) continue;
                                                    SolidBrush sb = c.Brush as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                            c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (!found && p.Bitmap != null && p.Visible && new Rectangle(p.Point.X, p.Point.Y, (p.IsSpriteSheet) ? p.Rectangle.Width : p.Bitmap.Width, (p.IsSpriteSheet) ? p.Rectangle.Height : p.Bitmap.Height).Contains(e.Location))
                                    {
                                        if (p.Opacity == 1)
                                        {
                                            if (
                                                p.Bitmap.GetPixel(e.X - p.Point.X + ((p.IsSpriteSheet) ? p.Rectangle.X : 0),
                                                    e.Y - p.Point.Y + ((p.IsSpriteSheet) ? p.Rectangle.Y : 0)) != GetPixel(e.X, e.Y) &&
                                                !p.EscapeGfxWhileMouseMove) continue;
                                            p.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (mouseOutRecorder.IndexOf(p) == -1)
                                                mouseOutRecorder.Add(p);

                                            // inscription dans la liste GfxMouseOver
                                            if (mouseOverRecorder.IndexOf(p) == -1)
                                            {
                                                mouseOverRecorder.Clear();
                                                mouseOverRecorder.Add(p);
                                                p.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(p.Opacity < 1) || !(p.Opacity > 0)) continue;
                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(p, p.Opacity));
                                        p.ChangeBmp(p.Path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (p != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                        p.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                    break;
                                    #endregion
                                }
                            case "Anim":
                                {
                                    Anim p = gfxObjList[cnt - 1] as Anim;
                                    #region childs
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    foreach (IGfx t in p.Childs)
                                    {
                                        switch (t.GetType().Name)
                                        {
                                            case "Bmp":
                                                {
                                                    Bmp c = t as Bmp;
                                                    if (p.Bmp.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bitmap.GetPixel(
                                                                    e.X - p.Bmp.Point.X - c.Point.X + c.Rectangle.X,
                                                                    e.Y - p.Bmp.Point.Y - c.Point.Y + c.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                            c.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                        c.ChangeBmp(c.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (p.Bmp.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Bmp.Point.X, c.Bmp.Point.Y + p.Bmp.Point.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Bmp.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bmp.Bitmap.GetPixel(
                                                                    e.X - p.Bmp.Point.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                    e.Y - p.Bmp.Point.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                            c.Bmp.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.Bmp.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                        c.Bmp.ChangeBmp(c.Bmp.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (!p.Bmp.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                            c.Size.Width, c.Size.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "Txt":
                                                {
                                                    Txt c = t as Txt;
                                                    if (!p.Bmp.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(e.Location) ||
                                                        !c.Visible) continue;
                                                    SolidBrush sb = c.Brush as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (!p.Bmp.Visible || !c.Visible ||
                                                        !new Rectangle(c.Rectangle.X + p.Bmp.Point.X, c.Rectangle.Y + p.Bmp.Point.Y,
                                                            c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (found || !p.Bmp.Visible || p.Bmp.Bitmap == null ||
                                        !new Rectangle(p.Bmp.Point.X, p.Bmp.Point.Y,
                                            (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Width : p.Bmp.Bitmap.Width,
                                            (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Height : p.Bmp.Bitmap.Height).Contains(e.Location))
                                        continue;
                                    {
                                        if (p.Bmp.Opacity == 1)
                                        {
                                            if (
                                                p.Bmp.Bitmap.GetPixel(
                                                    e.X - p.Bmp.Point.X + ((p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.X : 0),
                                                    e.Y - p.Bmp.Point.Y + ((p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Y : 0)) !=
                                                GetPixel(e.X, e.Y) && !p.Bmp.EscapeGfxWhileMouseMove) continue;
                                            p.Bmp.FireMouseMove(e);
                                            found = true;
                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (mouseOutRecorder.IndexOf(p) == -1)
                                                mouseOutRecorder.Add(p);

                                            // inscription dans la liste GfxMouseOver
                                            if (mouseOverRecorder.IndexOf(p) == -1)
                                            {
                                                mouseOverRecorder.Clear();
                                                mouseOverRecorder.Add(p);
                                                p.Bmp.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (p.Bmp.Opacity < 1 && p.Bmp.Opacity > 0)
                                        {
                                            opacityMouseMoveRecorder.Add(new OldDataMouseMove(p.Bmp, p.Bmp.Opacity));
                                            p.Bmp.ChangeBmp(p.Bmp.Path);
                                            break;
                                        }
                                        for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                        {
                                            if (p.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                            p.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                            opacityMouseMoveRecorder.RemoveAt(cnt1);
                                        }
                                        break;
                                    }

                                    #endregion
                                }
                            case "Rec":
                                {
                                    Rec p = gfxObjList[cnt - 1] as Rec;
                                    #region childs
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    foreach (IGfx t in p.Childs)
                                    {
                                        switch (t.GetType().Name)
                                        {
                                            case "Bmp":
                                                {
                                                    Bmp c = t as Bmp;
                                                    if (p.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bitmap.GetPixel(e.X - p.Point.X - c.Point.X + c.Rectangle.X,
                                                                    e.Y - p.Point.Y - c.Point.Y + c.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                            c.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                        c.ChangeBmp(c.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (p.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Bmp.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bmp.Bitmap.GetPixel(
                                                                    e.X - p.Point.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                    e.Y - p.Point.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                            c.Bmp.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.Bmp.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                        c.Bmp.ChangeBmp(c.Bmp.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                            c.Size.Width, c.Size.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "Txt":
                                                {
                                                    Txt childR = t as Txt;
                                                    if (!p.Visible || !childR.Visible ||
                                                        !new Rectangle(childR.Point.X + p.Point.X, childR.Point.Y + p.Point.Y,
                                                            TextRenderer.MeasureText(childR.Text, childR.Font).Width,
                                                            TextRenderer.MeasureText(childR.Text, childR.Font).Height).Contains(e.Location) ||
                                                        !childR.Visible) continue;
                                                    SolidBrush sb = childR.Brush as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    childR.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(childR) == -1)
                                                        mouseOutRecorder.Add(childR);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(childR) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(childR);
                                                        childR.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                            c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (found || !p.Visible || !new Rectangle(p.Point, p.Size).Contains(e.Location) || !p.Visible)
                                        continue;
                                    {
                                        SolidBrush sb = p.FillColor as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseMove)
                                            continue;
                                        p.FireMouseMove(e);
                                        found = true;
                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (mouseOutRecorder.IndexOf(p) == -1)
                                            mouseOutRecorder.Add(p);

                                        // inscription dans la liste GfxMouseOver
                                        if (mouseOverRecorder.IndexOf(p) == -1)
                                        {
                                            mouseOverRecorder.Clear();
                                            mouseOverRecorder.Add(p);
                                            p.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    #endregion
                                }
                            case "Txt":
                                {
                                    Txt p = gfxObjList[cnt - 1] as Txt;
                                    #region parent
                                    if (found || !p.Visible || !new Rectangle(p.Point, TextRenderer.MeasureText(p.Text, p.Font)).Contains(e.Location) || !p.Visible) continue;
                                    {
                                        SolidBrush sb = p.Brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseMove)
                                            continue;
                                        p.FireMouseMove(e);
                                        found = true;
                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (mouseOutRecorder.IndexOf(p) == -1)
                                            mouseOutRecorder.Add(p);

                                        // inscription dans la liste GfxMouseOver
                                        if (mouseOverRecorder.IndexOf(p) == -1)
                                        {
                                            mouseOverRecorder.Clear();
                                            mouseOverRecorder.Add(p);
                                            p.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    #endregion
                                }
                            case "FillPolygon":
                                {
                                    FillPolygon p = gfxObjList[cnt - 1] as FillPolygon;
                                    #region childs
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    foreach (IGfx t in p.Childs)
                                    {
                                        switch (t.GetType().Name)
                                        {
                                            case "Bmp":
                                                {
                                                    Bmp c = t as Bmp;
                                                    if (p.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Rectangle.X, c.Point.Y + p.Rectangle.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bitmap.GetPixel(e.X - p.Rectangle.X - c.Point.X + c.Rectangle.X,
                                                                    e.Y - p.Rectangle.Y - c.Point.Y + c.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                            c.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                        c.ChangeBmp(c.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (p.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Rectangle.X, c.Bmp.Point.Y + p.Rectangle.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Bmp.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bmp.Bitmap.GetPixel(
                                                                    e.X - p.Rectangle.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                    e.Y - p.Rectangle.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                            c.Bmp.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.Bmp.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                        c.Bmp.ChangeBmp(c.Bmp.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Rectangle.X, c.Point.Y + p.Rectangle.Y,
                                                            c.Size.Width, c.Size.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "Txt":
                                                {
                                                    Txt c = t as Txt;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Rectangle.X, c.Point.Y + p.Rectangle.Y,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(e.Location) ||
                                                        !c.Visible) continue;
                                                    SolidBrush sb = c.Brush as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Rectangle.X + p.Rectangle.X, c.Rectangle.Y + p.Rectangle.Y,
                                                            c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (found || !p.Visible || !p.Rectangle.Contains(e.Location) || !p.Visible)
                                        continue;
                                    {
                                        SolidBrush sb = p.FillColor as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseMove)
                                            continue;
                                        p.FireMouseMove(e);
                                        found = true;
                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (mouseOutRecorder.IndexOf(p) == -1)
                                            mouseOutRecorder.Add(p);

                                        // inscription dans la liste GfxMouseOver
                                        if (mouseOverRecorder.IndexOf(p) == -1)
                                        {
                                            mouseOverRecorder.Clear();
                                            mouseOverRecorder.Add(p);
                                            p.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    #endregion
                                }
                        }
                    }
                }

                if (found == false)
                {
                    for (int cnt = gfxBgrList.Count(); cnt > 0; cnt--)
                    {
                        if (found)
                            break;

                        switch (gfxBgrList[cnt - 1].GetType().Name)
                        {
                            case "Bmp":
                                {
                                    Bmp p = gfxBgrList[cnt - 1] as Bmp;
                                    #region childs
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    foreach (IGfx t in p.Childs)
                                    {
                                        switch (t.GetType().Name)
                                        {
                                            case "Bmp":
                                                {
                                                    Bmp c = t as Bmp;
                                                    if (p.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Opacity == 1)
                                                        {
                                                            if (c.Bitmap.GetPixel(e.X - p.Point.X - c.Point.X + c.Rectangle.X,
                                                                    e.Y - p.Point.Y - c.Point.Y + c.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                            c.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                        c.ChangeBmp(c.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (p.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Bmp.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bmp.Bitmap.GetPixel(
                                                                    e.X - p.Point.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                    e.Y - p.Point.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                            c.Bmp.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.Bmp.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                        c.Bmp.ChangeBmp(c.Bmp.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                            c.Size.Width, c.Size.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "Txt":
                                                {
                                                    Txt c = t as Txt;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(e.Location) ||
                                                        !c.Visible) continue;
                                                    SolidBrush sb = c.Brush as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                            c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (!found && p.Bitmap != null && p.Visible && new Rectangle(p.Point.X, p.Point.Y, (p.IsSpriteSheet) ? p.Rectangle.Width : p.Bitmap.Width, (p.IsSpriteSheet) ? p.Rectangle.Height : p.Bitmap.Height).Contains(e.Location))
                                    {
                                        if (p.Opacity == 1)
                                        {
                                            if (
                                                p.Bitmap.GetPixel(e.X - p.Point.X + ((p.IsSpriteSheet) ? p.Rectangle.X : 0),
                                                    e.Y - p.Point.Y + ((p.IsSpriteSheet) ? p.Rectangle.Y : 0)) != GetPixel(e.X, e.Y) &&
                                                !p.EscapeGfxWhileMouseMove) continue;
                                            p.FireMouseMove(e);
                                            found = true;

                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (mouseOutRecorder.IndexOf(p) == -1)
                                                mouseOutRecorder.Add(p);

                                            // inscription dans la liste GfxMouseOver
                                            if (mouseOverRecorder.IndexOf(p) == -1)
                                            {
                                                mouseOverRecorder.Clear();
                                                mouseOverRecorder.Add(p);
                                                p.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (!(p.Opacity < 1) || !(p.Opacity > 0)) continue;
                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(p, p.Opacity));
                                        p.ChangeBmp(p.Path);
                                        break;
                                    }
                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                    {
                                        if (p != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                        p.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                    }
                                    break;
                                    #endregion
                                }
                            case "Anim":
                                {
                                    Anim p = gfxBgrList[cnt - 1] as Anim;
                                    #region childs
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    foreach (IGfx t in p.Childs)
                                    {
                                        switch (t.GetType().Name)
                                        {
                                            case "Bmp":
                                                {
                                                    Bmp c = t as Bmp;
                                                    if (p.Bmp.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bitmap.GetPixel(
                                                                    e.X - p.Bmp.Point.X - c.Point.X + c.Rectangle.X,
                                                                    e.Y - p.Bmp.Point.Y - c.Point.Y + c.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                            c.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                        c.ChangeBmp(c.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (p.Bmp.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Bmp.Point.X, c.Bmp.Point.Y + p.Bmp.Point.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Bmp.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bmp.Bitmap.GetPixel(
                                                                    e.X - p.Bmp.Point.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                    e.Y - p.Bmp.Point.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                            c.Bmp.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.Bmp.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                        c.Bmp.ChangeBmp(c.Bmp.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (!p.Bmp.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                            c.Size.Width, c.Size.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "Txt":
                                                {
                                                    Txt c = t as Txt;
                                                    if (!p.Bmp.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(e.Location) ||
                                                        !c.Visible) continue;
                                                    SolidBrush sb = c.Brush as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (!p.Bmp.Visible || !c.Visible ||
                                                        !new Rectangle(c.Rectangle.X + p.Bmp.Point.X, c.Rectangle.Y + p.Bmp.Point.Y,
                                                            c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (found || !p.Bmp.Visible || p.Bmp.Bitmap == null ||
                                        !new Rectangle(p.Bmp.Point.X, p.Bmp.Point.Y,
                                            (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Width : p.Bmp.Bitmap.Width,
                                            (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Height : p.Bmp.Bitmap.Height).Contains(e.Location))
                                        continue;
                                    {
                                        if (p.Bmp.Opacity == 1)
                                        {
                                            if (
                                                p.Bmp.Bitmap.GetPixel(
                                                    e.X - p.Bmp.Point.X + ((p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.X : 0),
                                                    e.Y - p.Bmp.Point.Y + ((p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Y : 0)) !=
                                                GetPixel(e.X, e.Y) && !p.Bmp.EscapeGfxWhileMouseMove) continue;
                                            p.Bmp.FireMouseMove(e);
                                            found = true;
                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                            if (mouseOutRecorder.IndexOf(p) == -1)
                                                mouseOutRecorder.Add(p);

                                            // inscription dans la liste GfxMouseOver
                                            if (mouseOverRecorder.IndexOf(p) == -1)
                                            {
                                                mouseOverRecorder.Clear();
                                                mouseOverRecorder.Add(p);
                                                p.Bmp.FireMouseOver(e);
                                            }
                                            break;
                                        }
                                        if (p.Bmp.Opacity < 1 && p.Bmp.Opacity > 0)
                                        {
                                            opacityMouseMoveRecorder.Add(new OldDataMouseMove(p.Bmp, p.Bmp.Opacity));
                                            p.Bmp.ChangeBmp(p.Bmp.Path);
                                            break;
                                        }
                                        for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                        {
                                            if (p.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                            p.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                            opacityMouseMoveRecorder.RemoveAt(cnt1);
                                        }
                                        break;
                                    }

                                    #endregion
                                }
                            case "Rec":
                                {
                                    Rec p = gfxBgrList[cnt - 1] as Rec;
                                    #region childs
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    foreach (IGfx t in p.Childs)
                                    {
                                        switch (t.GetType().Name)
                                        {
                                            case "Bmp":
                                                {
                                                    Bmp c = t as Bmp;
                                                    if (p.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bitmap.GetPixel(e.X - p.Point.X - c.Point.X + c.Rectangle.X,
                                                                    e.Y - p.Point.Y - c.Point.Y + c.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                            c.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                        c.ChangeBmp(c.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (p.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Bmp.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bmp.Bitmap.GetPixel(
                                                                    e.X - p.Point.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                    e.Y - p.Point.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                            c.Bmp.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.Bmp.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                        c.Bmp.ChangeBmp(c.Bmp.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                            c.Size.Width, c.Size.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "Txt":
                                                {
                                                    Txt childR = t as Txt;
                                                    if (!p.Visible || !childR.Visible ||
                                                        !new Rectangle(childR.Point.X + p.Point.X, childR.Point.Y + p.Point.Y,
                                                            TextRenderer.MeasureText(childR.Text, childR.Font).Width,
                                                            TextRenderer.MeasureText(childR.Text, childR.Font).Height).Contains(e.Location) ||
                                                        !childR.Visible) continue;
                                                    SolidBrush sb = childR.Brush as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !childR.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    childR.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(childR) == -1)
                                                        mouseOutRecorder.Add(childR);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(childR) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(childR);
                                                        childR.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                            c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (found || !p.Visible || !new Rectangle(p.Point, p.Size).Contains(e.Location) || !p.Visible)
                                        continue;
                                    {
                                        SolidBrush sb = p.FillColor as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseMove)
                                            continue;
                                        p.FireMouseMove(e);
                                        found = true;
                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (mouseOutRecorder.IndexOf(p) == -1)
                                            mouseOutRecorder.Add(p);

                                        // inscription dans la liste GfxMouseOver
                                        if (mouseOverRecorder.IndexOf(p) == -1)
                                        {
                                            mouseOverRecorder.Clear();
                                            mouseOverRecorder.Add(p);
                                            p.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    #endregion
                                }
                            case "Txt":
                                {
                                    Txt p = gfxBgrList[cnt - 1] as Txt;
                                    #region parent
                                    if (found || !p.Visible || !new Rectangle(p.Point, TextRenderer.MeasureText(p.Text, p.Font)).Contains(e.Location) || !p.Visible) continue;
                                    {
                                        SolidBrush sb = p.Brush as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseMove)
                                            continue;
                                        p.FireMouseMove(e);
                                        found = true;
                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (mouseOutRecorder.IndexOf(p) == -1)
                                            mouseOutRecorder.Add(p);

                                        // inscription dans la liste GfxMouseOver
                                        if (mouseOverRecorder.IndexOf(p) == -1)
                                        {
                                            mouseOverRecorder.Clear();
                                            mouseOverRecorder.Add(p);
                                            p.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    #endregion
                                }
                            case "FillPolygon":
                                {
                                    FillPolygon p = gfxBgrList[cnt - 1] as FillPolygon;
                                    #region childs
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    foreach (IGfx t in p.Childs)
                                    {
                                        switch (t.GetType().Name)
                                        {
                                            case "Bmp":
                                                {
                                                    Bmp c = t as Bmp;
                                                    if (p.Visible && c.Visible && c.Bitmap != null && new Rectangle(c.Point.X + p.Rectangle.X, c.Point.Y + p.Rectangle.Y, (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width, (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bitmap.GetPixel(e.X - p.Rectangle.X - c.Point.X + c.Rectangle.X,
                                                                    e.Y - p.Rectangle.Y - c.Point.Y + c.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseMove) continue;
                                                            c.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Opacity < 1) || !(c.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c, c.Opacity));
                                                        c.ChangeBmp(c.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (p.Visible && c.Bmp.Visible && c.Bmp.Bitmap != null && new Rectangle(c.Bmp.Point.X + p.Rectangle.X, c.Bmp.Point.Y + p.Rectangle.Y, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Width : c.Bmp.Bitmap.Width, (c.Bmp.IsSpriteSheet) ? c.Bmp.Rectangle.Height : c.Bmp.Bitmap.Height).Contains(e.Location))
                                                    {
                                                        if (c.Bmp.Opacity == 1)
                                                        {
                                                            if (
                                                                c.Bmp.Bitmap.GetPixel(
                                                                    e.X - p.Rectangle.X - c.Bmp.Point.X + c.Bmp.Rectangle.X,
                                                                    e.Y - p.Rectangle.Y - c.Bmp.Point.Y + c.Bmp.Rectangle.Y) !=
                                                                GetPixel(e.X, e.Y) && !c.Bmp.EscapeGfxWhileMouseMove) continue;
                                                            c.Bmp.FireMouseMove(e);
                                                            found = true;

                                                            // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                            if (mouseOutRecorder.IndexOf(c) == -1)
                                                                mouseOutRecorder.Add(c);

                                                            // inscription dans la liste GfxMouseOver
                                                            if (mouseOverRecorder.IndexOf(c) == -1)
                                                            {
                                                                mouseOverRecorder.Clear();
                                                                mouseOverRecorder.Add(c);
                                                                c.Bmp.FireMouseOver(e);
                                                            }
                                                            break;
                                                        }
                                                        if (!(c.Bmp.Opacity < 1) || !(c.Bmp.Opacity > 0)) continue;
                                                        opacityMouseMoveRecorder.Add(new OldDataMouseMove(c.Bmp, c.Bmp.Opacity));
                                                        c.Bmp.ChangeBmp(c.Bmp.Path);
                                                        break;
                                                    }
                                                    for (int cnt1 = 0; cnt1 < opacityMouseMoveRecorder.Count; cnt1++)
                                                    {
                                                        if (c.Bmp != opacityMouseMoveRecorder[cnt1].Bitmap) continue;
                                                        c.Bmp.ChangeBmp(opacityMouseMoveRecorder[cnt1].Bitmap.Path, opacityMouseMoveRecorder[cnt1].Opacity);
                                                        opacityMouseMoveRecorder.RemoveAt(cnt1);
                                                    }
                                                    break;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Rectangle.X, c.Point.Y + p.Rectangle.Y,
                                                            c.Size.Width, c.Size.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "Txt":
                                                {
                                                    Txt c = t as Txt;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Point.X + p.Rectangle.X, c.Point.Y + p.Rectangle.Y,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                            TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(e.Location) ||
                                                        !c.Visible) continue;
                                                    SolidBrush sb = c.Brush as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (!p.Visible || !c.Visible ||
                                                        !new Rectangle(c.Rectangle.X + p.Rectangle.X, c.Rectangle.Y + p.Rectangle.Y,
                                                            c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location))
                                                        continue;
                                                    SolidBrush sb = c.FillColor as SolidBrush;
                                                    if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !c.EscapeGfxWhileMouseMove)
                                                        continue;
                                                    c.FireMouseMove(e);
                                                    found = true;

                                                    // inscription dans la liste GfxMouseOut pour activer cette evenement
                                                    if (mouseOutRecorder.IndexOf(c) == -1)
                                                        mouseOutRecorder.Add(c);

                                                    // inscription dans la liste GfxMouseOver
                                                    if (mouseOverRecorder.IndexOf(c) == -1)
                                                    {
                                                        mouseOverRecorder.Clear();
                                                        mouseOverRecorder.Add(c);
                                                        c.FireMouseOver(e);
                                                    }
                                                    break;
                                                }
                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (found || !p.Visible || !p.Rectangle.Contains(e.Location) || !p.Visible)
                                        continue;
                                    {
                                        SolidBrush sb = p.FillColor as SolidBrush;
                                        if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseMove)
                                            continue;
                                        p.FireMouseMove(e);
                                        found = true;
                                        // inscription dans la liste GfxMouseOut pour activer cette evenement
                                        if (mouseOutRecorder.IndexOf(p) == -1)
                                            mouseOutRecorder.Add(p);

                                        // inscription dans la liste GfxMouseOver
                                        if (mouseOverRecorder.IndexOf(p) == -1)
                                        {
                                            mouseOverRecorder.Clear();
                                            mouseOverRecorder.Add(p);
                                            p.FireMouseOver(e);
                                        }
                                        break;
                                    }
                                    #endregion
                                }
                        }
                    }
                }
                #endregion
                #region handeling MouseOut
                ///////////////// handeling MouseOut //////////////////////
                if (mouseOutRecorder.Count <= 0) return;
                {
                    found = false;

                    for (int cnt = gfxTopList.Count(); cnt > 0; cnt--)
                    {
                        if (found)
                            break;

                        switch (gfxTopList[cnt - 1].GetType().Name)
                        {
                            case "Bmp":
                                {
                                    Bmp p = gfxTopList[cnt - 1] as Bmp;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (p.Visible && mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Point.X, p.Point.Y, (p.IsSpriteSheet) ? p.Rectangle.Width : p.Bitmap.Width, (p.IsSpriteSheet) ? p.Rectangle.Height : p.Bitmap.Height).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                            case "Anim":
                                {
                                    Anim p = gfxTopList[cnt - 1] as Anim;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Bmp.Point.X, p.Bmp.Point.Y, (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Width : p.Bmp.Bitmap.Width, (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Height : p.Bmp.Bitmap.Height).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.Bmp.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Bmp.Point.X,
                                                     c.Bmp.Point.Y + c.Bmp.Point.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Bmp.Point.X, c.Rectangle.Y + p.Bmp.Point.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                            case "Rec":
                                {
                                    Rec p = gfxTopList[cnt - 1] as Rec;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Point, p.Size).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                            case "Txt":
                                {
                                    Txt p = gfxTopList[cnt - 1] as Txt;
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Point, TextRenderer.MeasureText(p.Text, p.Font)).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    break;
                                    #endregion
                                }
                            case "FillPolygon":
                                {
                                    FillPolygon p = gfxTopList[cnt - 1] as FillPolygon;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Rectangle.Location, p.Rectangle.Size).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Rectangle.Location.X, c.Point.Y + p.Rectangle.Location.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Rectangle.Location.X, c.Bmp.Point.Y + p.Rectangle.Location.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Rectangle.Location.X, c.Point.Y + p.Rectangle.Location.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Rectangle.Location.X, c.Rectangle.Y + p.Rectangle.Location.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Rectangle.Location.X, c.Point.Y + p.Rectangle.Location.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                        }
                    }

                    for (int cnt = gfxObjList.Count(); cnt > 0; cnt--)
                    {
                        if (found)
                            break;

                        switch (gfxObjList[cnt - 1].GetType().Name)
                        {
                            case "Bmp":
                                {
                                    Bmp p = gfxObjList[cnt - 1] as Bmp;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (p.Visible && mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Point.X, p.Point.Y, (p.IsSpriteSheet) ? p.Rectangle.Width : p.Bitmap.Width, (p.IsSpriteSheet) ? p.Rectangle.Height : p.Bitmap.Height).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                            case "Anim":
                                {
                                    Anim p = gfxObjList[cnt - 1] as Anim;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Bmp.Point.X, p.Bmp.Point.Y, (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Width : p.Bmp.Bitmap.Width, (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Height : p.Bmp.Bitmap.Height).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.Bmp.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Bmp.Point.X,
                                                     c.Bmp.Point.Y + c.Bmp.Point.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Bmp.Point.X, c.Rectangle.Y + p.Bmp.Point.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                            case "Rec":
                                {
                                    Rec p = gfxObjList[cnt - 1] as Rec;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Point, p.Size).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                            case "Txt":
                                {
                                    Txt p = gfxObjList[cnt - 1] as Txt;
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Point, TextRenderer.MeasureText(p.Text, p.Font)).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    break;
                                    #endregion
                                }
                            case "FillPolygon":
                                {
                                    FillPolygon p = gfxObjList[cnt - 1] as FillPolygon;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Rectangle.Location, p.Rectangle.Size).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Rectangle.Location.X, c.Point.Y + p.Rectangle.Location.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Rectangle.Location.X, c.Bmp.Point.Y + p.Rectangle.Location.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Rectangle.Location.X, c.Point.Y + p.Rectangle.Location.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Rectangle.Location.X, c.Rectangle.Y + p.Rectangle.Location.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Rectangle.Location.X, c.Point.Y + p.Rectangle.Location.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                        }
                    }

                    for (int cnt = gfxBgrList.Count(); cnt > 0; cnt--)
                    {
                        if (found)
                            break;

                        switch (gfxBgrList[cnt - 1].GetType().Name)
                        {
                            case "Bmp":
                                {
                                    Bmp p = gfxBgrList[cnt - 1] as Bmp;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (p.Visible && mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Point.X, p.Point.Y, (p.IsSpriteSheet) ? p.Rectangle.Width : p.Bitmap.Width, (p.IsSpriteSheet) ? p.Rectangle.Height : p.Bitmap.Height).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                            case "Anim":
                                {
                                    Anim p = gfxBgrList[cnt - 1] as Anim;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Bmp.Point.X, p.Bmp.Point.Y, (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Width : p.Bmp.Bitmap.Width, (p.Bmp.IsSpriteSheet) ? p.Bmp.Rectangle.Height : p.Bmp.Bitmap.Height).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.Bmp.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Bmp.Point.X,
                                                     c.Bmp.Point.Y + c.Bmp.Point.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Bmp.Point.X, c.Rectangle.Y + p.Bmp.Point.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Bmp.Point.X, c.Point.Y + p.Bmp.Point.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                            case "Rec":
                                {
                                    Rec p = gfxBgrList[cnt - 1] as Rec;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Point, p.Size).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Point.X, c.Bmp.Point.Y + p.Point.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                            case "Txt":
                                {
                                    Txt p = gfxBgrList[cnt - 1] as Txt;
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Point, TextRenderer.MeasureText(p.Text, p.Font)).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    break;
                                    #endregion
                                }
                            case "FillPolygon":
                                {
                                    FillPolygon p = gfxBgrList[cnt - 1] as FillPolygon;
                                    p.Childs.Sort(0, p.Childs.Count, rzi);
                                    #region parent
                                    if (mouseOutRecorder.IndexOf(p) != -1 && (!new Rectangle(p.Rectangle.Location, p.Rectangle.Size).Contains(e.Location) || (mouseOverRecorder.Count > 0 && mouseOverRecorder.IndexOf(p) == -1)))
                                    {
                                        p.FireMouseOut(e);
                                        mouseOutRecorder.Remove(p);
                                        found = true;
                                        break;
                                    }
                                    #endregion
                                    #region childs
                                    foreach (IGfx t in p.Childs)
                                    {
                                        if (t != null && t.GetType() == typeof(Bmp))
                                        {
                                            Bmp c = t as Bmp;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Rectangle.Location.X, c.Point.Y + p.Rectangle.Location.Y,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                                                         (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                                                     .Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Anim))
                                        {
                                            Anim c = t as Anim;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Bmp.Point.X + p.Rectangle.Location.X, c.Bmp.Point.Y + p.Rectangle.Location.Y,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Width
                                                         : c.Bmp.Bitmap.Width,
                                                     (c.Bmp.IsSpriteSheet)
                                                         ? c.Bmp.Rectangle.Height
                                                         : c.Bmp.Bitmap.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.Bmp.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(Rec))
                                        {
                                            Rec c = t as Rec;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Rectangle.Location.X, c.Point.Y + p.Rectangle.Location.Y,
                                                     c.Size.Width, c.Size.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t != null && t.GetType() == typeof(FillPolygon))
                                        {
                                            FillPolygon c = t as FillPolygon;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Rectangle.X + p.Rectangle.Location.X, c.Rectangle.Y + p.Rectangle.Location.Y,
                                                     c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                        if (t == null || t.GetType() != typeof(Txt)) continue;
                                        {
                                            Txt c = t as Txt;
                                            if (mouseOutRecorder.IndexOf(c) == -1 ||
                                                (new Rectangle(c.Point.X + p.Rectangle.Location.X, c.Point.Y + p.Rectangle.Location.Y,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Width,
                                                     TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(
                                                     e.Location) &&
                                                 (mouseOverRecorder.Count <= 0 || mouseOverRecorder.IndexOf(c) != -1)))
                                                continue;
                                            c.FireMouseOut(e);
                                            mouseOutRecorder.Remove(c);
                                            found = true;
                                            break;
                                        }
                                    }
                                    break;
                                    #endregion
                                }
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                OutputErrorCallBack(ex.Message);
            }
        }
    }
}