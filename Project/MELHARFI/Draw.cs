﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MELHARFI
{
    public partial class Manager
    {
        /// <summary>
        /// This method draw all object found on the 3 layers GfxBgrList, GfxObjList, GfxTopList on the form, this method is called inside the loop game
        /// </summary>
        /// <param name="e">e is a PaintEventArgs</param>
        private void Draw(PaintEventArgs e)
        {
            try
            {
                // nétoyage des listes des objets null
                BackgroundLayer.RemoveAll(f => f == null);
                ObjectLayer.RemoveAll(f => f == null);
                TopLayer.RemoveAll(f => f == null);

                // copie des liste
                List<IGfx> gfxBgrList;  // clone du GfxBgrList pour eviter de modifier une liste lors du rendu
                List<IGfx> gfxObjList;  // clone du GfxObjList pour eviter de modifier une liste lors du rendu
                List<IGfx> gfxTopList;  // clone du GfxTopList pour eviter de modifier une liste lors du rendu

                Zindex zi = new Zindex();                   // triage des listes

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

                // affichage de la 1ere couche réservé au Assets de l'arriere plant
                #region GfxBgr
                lock (((ICollection) gfxBgrList).SyncRoot)
                    if (!hideBagroundLayer)
                    {
                        foreach (IGfx t1 in gfxBgrList)
                        {
                            if (t1 != null && t1.GetType() == typeof(Bmp) && (t1 as Bmp).bmp != null)
                            {
                                Bmp b = t1 as Bmp;
                                b.Child.Sort(0, b.Child.Count, zi);
                                if (!b.Visible) continue;

                                #region parent
                                ImageAttributes imageAttrParent = new ImageAttributes();
                                if (b.newColorMap != null)
                                    imageAttrParent.SetRemapTable(b.newColorMap);
                                Point parentPoint = new Point(b.point.X, b.point.Y);
                                parentPoint.Offset(Padding);
                                e.Graphics.DrawImage(b.bmp, new Rectangle(parentPoint, b.rectangle.Size), b.rectangle.X, b.rectangle.Y, b.rectangle.Width, b.rectangle.Height, GraphicsUnit.Pixel, imageAttrParent);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in b.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (!childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + b.point.X + Padding.X, childB.point.Y + b.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrParent);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (!childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + b.point.X + Padding.X, childA.img.point.Y + b.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrParent);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, b.point.X + childT.point.X + Padding.X, b.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(b.point.X + childR.point.X + Padding.X, b.point.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (childF.Visible)
                                        {
                                            // Add Padding
                                            Point[] childPoints = (Point[])childF.Points.Clone();
                                            foreach (Point p in childPoints)
                                                p.Offset(Padding);

                                            e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                            if (childF.BorderColor != null)
                                                e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                        }
                                    }
                                }
                                //////////////////////////////////////////////////
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(Anim) && (t1 as Anim).img != null)
                            {
                                Anim a = t1 as Anim;
                                a.Child.Sort(0, a.Child.Count, zi);
                                if (!a.img.Visible) continue;

                                #region parent
                                ImageAttributes imageAttrParent = new ImageAttributes();
                                if (a.img.newColorMap != null)
                                    imageAttrParent.SetRemapTable(a.img.newColorMap);
                                Point parentPoint = new Point(a.img.point.X, a.img.point.Y);
                                parentPoint.Offset(Padding);
                                e.Graphics.DrawImage(a.img.bmp, new Rectangle(parentPoint, a.img.rectangle.Size), a.img.rectangle.X, a.img.rectangle.Y, a.img.rectangle.Width, a.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrParent);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in a.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (!childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + a.img.point.X + Padding.X, childB.point.Y + a.img.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (!childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + a.img.point.X + Padding.X, childA.img.point.Y + a.img.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, a.img.point.X + childT.point.X + Padding.X, a.img.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(a.img.point.X + childR.point.X + Padding.X, a.img.point.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = a.img.point.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = a.img.point.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(Txt))
                            {
                                Txt t = t1 as Txt;
                                t.Child.Sort(0, t.Child.Count, zi);
                                if (!t.Visible) continue;

                                #region parent
                                Point parentPoint = new Point(t.point.X + Padding.X, t.point.Y + Padding.Y);
                                e.Graphics.DrawString(t.Text, t.font, t.brush, parentPoint);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t2 in t.Child)
                                {
                                    if (t2 != null && t2.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t2 as Bmp;
                                        if (!childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + t.point.X + Padding.X, childB.point.Y + t.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t2 as Anim;
                                        if (!childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + t.point.X + Padding.X, childA.img.point.Y + t.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t2 as Txt;
                                        if (!childT.Visible) continue;
                                        e.Graphics.DrawString(childT.Text, childT.font, childT.brush, t.point.X + childT.point.X + Padding.X, t.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t2 as Rec;
                                        if (!childR.Visible) continue;
                                        e.Graphics.FillRectangle(childR.brush, new Rectangle(t.point.X + childR.point.X + Padding.X, t.point.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t2 as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = t.point.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = t.point.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(Rec))
                            {
                                Rec r = t1 as Rec;
                                r.Child.Sort(0, r.Child.Count, zi);
                                if (!r.Visible) continue;

                                #region parent
                                Point parentPoint = new Point(r.point.X + Padding.X, r.point.Y + Padding.Y);
                                e.Graphics.FillRectangle(r.brush, new Rectangle(parentPoint, r.size));
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in r.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (!childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + r.point.X + Padding.X, childB.point.Y + r.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (!childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + r.point.X + Padding.X, childA.img.point.Y + r.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (!childT.Visible) continue;
                                        e.Graphics.DrawString(childT.Text, childT.font, childT.brush, r.point.X + childT.point.X + Padding.X, r.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (!childR.Visible) continue;
                                        e.Graphics.FillRectangle(childR.brush, new Rectangle(r.point.X + childR.point.X + Padding.X, r.point.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = r.point.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = r.point.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon f = t1 as FillPolygon;
                                f.Child.Sort(0, f.Child.Count, zi);
                                if (!f.Visible) continue;
                                Point[] parentPoints = (Point[])f.Points.Clone();
                                foreach (Point p in f.Points)
                                    p.Offset(Padding);
                                #region parent
                                e.Graphics.FillPolygon(f.brush, parentPoints, f.fillMode);
                                if (f.BorderColor != null)
                                    e.Graphics.DrawPolygon(new Pen(f.BorderColor, f.BorderWidth), parentPoints);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in f.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (!childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + f.rectangle.X + Padding.X, childB.point.Y + f.rectangle.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (!childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + f.rectangle.X + Padding.X, childA.img.point.Y + f.rectangle.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (!childT.Visible) continue;
                                        e.Graphics.DrawString(childT.Text, childT.font, childT.brush, f.rectangle.X + childT.point.X + Padding.X, f.rectangle.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (!childR.Visible) continue;
                                        e.Graphics.FillRectangle(childR.brush, new Rectangle(f.rectangle.X + childR.point.X + Padding.X, f.rectangle.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = f.rectangle.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = f.rectangle.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                //////////////////////////////////////////////////
                                #endregion
                            }
                        }
                    }
                #endregion

                // affichage de la 2eme couche qui contiens les Assets
                #region GfxObj
                lock ((gfxObjList as ICollection).SyncRoot)
                    if (!hideObjectLayer)
                    {
                        foreach (IGfx t1 in gfxObjList)
                        {
                            if (t1 != null && t1.GetType() == typeof(Bmp) && (t1 as Bmp).bmp != null)
                            {
                                Bmp b = t1 as Bmp;
                                b.Child.Sort(0, b.Child.Count, zi);
                                if (b.bmp == null || !b.Visible) continue;
                                #region parent
                                ImageAttributes imageAttrParent = new ImageAttributes();
                                if (b.newColorMap != null)
                                    imageAttrParent.SetRemapTable(b.newColorMap);
                                Point parentPoint = new Point(b.point.X, b.point.Y);
                                parentPoint.Offset(Padding);
                                e.Graphics.DrawImage(b.bmp, new Rectangle(parentPoint, b.rectangle.Size), b.rectangle.X, b.rectangle.Y, b.rectangle.Width, b.rectangle.Height, GraphicsUnit.Pixel, imageAttrParent);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in b.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (childB.bmp == null || !childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + b.point.X + Padding.X, childB.point.Y + b.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (childA.img.bmp == null || !childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + b.point.X + Padding.X, childA.img.point.Y + b.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, b.point.X + childT.point.X + Padding.X, b.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(b.point.X + childR.point.X + Padding.X, b.point.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = b.point.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = b.point.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(Anim) && (t1 as Anim).img != null)
                            {
                                Anim a = t1 as Anim;
                                a.Child.Sort(0, a.Child.Count, zi);
                                if (a.img.bmp == null || !a.img.Visible) continue;

                                #region parent
                                ImageAttributes imageAttrParent = new ImageAttributes();
                                if (a.img.newColorMap != null)
                                    imageAttrParent.SetRemapTable(a.img.newColorMap);
                                Point parentPoint = new Point(a.img.point.X, a.img.point.Y);
                                parentPoint.Offset(Padding);
                                e.Graphics.DrawImage(a.img.bmp, new Rectangle(parentPoint, a.img.rectangle.Size), a.img.rectangle.X, a.img.rectangle.Y, a.img.rectangle.Width, a.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrParent);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in a.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (childB.bmp == null || !childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + a.img.point.X + Padding.X, childB.point.Y + a.img.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (childA.img.bmp == null || !childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + a.img.point.X + Padding.X, childA.img.point.Y + a.img.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, a.img.point.X + childT.point.X + Padding.X, a.img.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(a.img.point.X + childR.point.X + Padding.X, a.img.point.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = a.img.point.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = a.img.point.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(Txt))
                            {
                                Txt t = t1 as Txt;
                                t.Child.Sort(0, t.Child.Count, zi);
                                if (!t.Visible) continue;

                                #region parent
                                Point parentPoint = new Point(t.point.X + Padding.X, t.point.Y + Padding.Y);
                                e.Graphics.DrawString(t.Text, t.font, t.brush, parentPoint);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t2 in t.Child)
                                {
                                    if (t2 != null && t2.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t2 as Bmp;
                                        if (childB.bmp == null || !childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + t.point.X + Padding.X, childB.point.Y + t.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t2 as Anim;
                                        if (childA.img.bmp == null || !childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + t.point.X + Padding.X, childA.img.point.Y + t.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t2 as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, t.point.X + childT.point.X + Padding.X, t.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t2 as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(t.point.X + childR.point.X + Padding.X, t.point.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t2 as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = t.point.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = t.point.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(Rec))
                            {
                                Rec r = t1 as Rec;
                                r.Child.Sort(0, r.Child.Count, zi);
                                if (!r.Visible) continue;

                                #region parent
                                Point parentPoint = new Point(r.point.X + Padding.X, r.point.Y + Padding.Y);
                                e.Graphics.FillRectangle(r.brush, new Rectangle(parentPoint, r.size));
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in r.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (childB.bmp == null || !childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + r.point.X + Padding.X, childB.point.Y + r.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (childA.img.bmp == null || !childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + r.point.X + Padding.X, childA.img.point.Y + r.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, r.point.X + childT.point.X + Padding.X, r.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(r.point.X + childR.point.X + Padding.X, r.point.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = r.point.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = r.point.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon f = t1 as FillPolygon;
                                f.Child.Sort(0, f.Child.Count, zi);
                                if (!f.Visible) continue;

                                #region parent
                                Point[] parentPoints = (Point[])f.Points.Clone();
                                foreach (Point p in parentPoints)
                                    p.Offset(Padding);

                                e.Graphics.FillPolygon(f.brush, parentPoints, f.fillMode);
                                if (f.BorderColor != null)
                                    e.Graphics.DrawPolygon(new Pen(f.BorderColor, f.BorderWidth), parentPoints);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in f.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (childB.bmp == null || !childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + f.rectangle.X + Padding.X, childB.point.Y + f.rectangle.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (childA.img.bmp == null || !childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + f.rectangle.X + Padding.X, childA.img.point.Y + f.rectangle.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, f.rectangle.X + childT.point.X + Padding.X, f.rectangle.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(f.rectangle.X + childR.point.X + Padding.X, f.rectangle.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = f.rectangle.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = f.rectangle.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                #endregion

                // affichage de la 3eme couche qui contiens les Assets 
                #region GfxTop
                lock ((gfxTopList as ICollection).SyncRoot)
                    if (!hideTopLayer)
                    {
                        foreach (IGfx t1 in gfxTopList)
                        {
                            if (t1 != null && t1.GetType() == typeof(Bmp) && (t1 as Bmp).bmp != null)
                            {
                                Bmp b = t1 as Bmp;
                                b.Child.Sort(0, b.Child.Count, zi);
                                if (b.bmp == null || !b.Visible) continue;

                                #region parent
                                ImageAttributes imageAttrParent = new ImageAttributes();
                                if (b.newColorMap != null)
                                    imageAttrParent.SetRemapTable(b.newColorMap);
                                Point parentPoint = new Point(b.point.X, b.point.Y);
                                parentPoint.Offset(Padding);
                                e.Graphics.DrawImage(b.bmp, new Rectangle(parentPoint, b.rectangle.Size), b.rectangle.X, b.rectangle.Y, b.rectangle.Width, b.rectangle.Height, GraphicsUnit.Pixel, imageAttrParent);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in b.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (childB.bmp == null || !childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + b.point.X + Padding.X, childB.point.Y + b.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (childA.img.bmp == null || !childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + b.point.X + Padding.X, childA.img.point.Y + b.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, b.point.X + childT.point.X + Padding.X, b.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(b.point.X + childR.point.X + Padding.X, b.point.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = b.point.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = b.point.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(Anim) && (t1 as Anim).img != null)
                            {
                                Anim a = t1 as Anim;
                                a.Child.Sort(0, a.Child.Count, zi);
                                if (a.img.bmp == null || !a.img.Visible) continue;

                                #region parent
                                ImageAttributes imageAttrParent = new ImageAttributes();
                                if (a.img.newColorMap != null)
                                    imageAttrParent.SetRemapTable(a.img.newColorMap);
                                Point parentPoint = new Point(a.img.point.X, a.img.point.Y);
                                parentPoint.Offset(Padding);
                                e.Graphics.DrawImage(a.img.bmp, new Rectangle(parentPoint, a.img.rectangle.Size), a.img.rectangle.X, a.img.rectangle.Y, a.img.rectangle.Width, a.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrParent);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in a.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (childB.bmp == null || !childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + a.img.point.X + Padding.X, childB.point.Y + a.img.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (childA.img.bmp == null || !childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + a.img.point.X + Padding.X, childA.img.point.Y + a.img.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, a.img.point.X + childT.point.X + Padding.X, a.img.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(a.img.point.X + childR.point.X + Padding.X, a.img.point.Y + childR.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = a.img.point.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = a.img.point.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                //////////////////////////////////////////////////
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(Txt))
                            {
                                Txt t = t1 as Txt;
                                t.Child.Sort(0, t.Child.Count, zi);
                                if (!t.Visible) continue;

                                #region parent
                                Point parentPoint = new Point(t.point.X + Padding.X, t.point.Y + Padding.Y);
                                e.Graphics.DrawString(t.Text, t.font, t.brush, parentPoint);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t2 in t.Child)
                                {
                                    if (t2 != null && t2.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t2 as Bmp;
                                        if (childB.bmp == null || !childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + t.point.X + Padding.X, childB.point.Y + t.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t2 as Anim;
                                        if (childA.img.bmp == null || !childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + t.point.X + Padding.X, childA.img.point.Y + t.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t2 as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, t.point.X + childT.point.X + Padding.X, t.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t2 as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(childR.point.X + t.point.X + Padding.X, childR.point.Y + t.point.Y + Padding.Y, childR.size.Width, childR.size.Height));
                                    }
                                    else if (t2 != null && t2.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t2 as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = t.point.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = t.point.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(Rec))
                            {
                                Rec r = t1 as Rec;
                                r.Child.Sort(0, r.Child.Count, zi);
                                if (!r.Visible) continue;

                                #region parent
                                Point parentPoint = new Point(r.point.X + Padding.X, r.point.Y + Padding.Y);
                                e.Graphics.FillRectangle(r.brush, new Rectangle(parentPoint, r.size));
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in r.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (childB.bmp == null || !childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + r.point.X + Padding.X, childB.point.Y + r.point.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (childA.img.bmp == null || !childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + r.point.X + Padding.X, childA.img.point.Y + r.point.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, r.point.X + childT.point.X + Padding.X, r.point.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(new Point(childR.point.X + r.point.X + Padding.X, childR.point.Y + r.point.Y + Padding.Y), childR.size));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = r.point.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = r.point.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                            else if (t1 != null && t1.GetType() == typeof(FillPolygon))
                            {
                                FillPolygon f = t1 as FillPolygon;
                                f.Child.Sort(0, f.Child.Count, zi);
                                if (!f.Visible) continue;

                                #region parent
                                Point[] parentPoints = (Point[])f.Points.Clone();
                                foreach (Point p in parentPoints)
                                    p.Offset(Padding);
                                e.Graphics.FillPolygon(f.brush, parentPoints, f.fillMode);
                                if (f.BorderColor != null)
                                    e.Graphics.DrawPolygon(new Pen(f.BorderColor, f.BorderWidth), parentPoints);
                                #endregion
                                #region childs
                                ////////// affichage des elements enfants de l'objet Bmp
                                foreach (IGfx t in f.Child)
                                {
                                    if (t != null && t.GetType() == typeof(Bmp))
                                    {
                                        Bmp childB = t as Bmp;
                                        if (childB.bmp == null || !childB.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childB.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childB.newColorMap);
                                        e.Graphics.DrawImage(childB.bmp, new Rectangle(new Point(childB.point.X + f.rectangle.X + Padding.X, childB.point.Y + f.rectangle.Y + Padding.Y), childB.rectangle.Size), childB.rectangle.X, childB.rectangle.Y, childB.rectangle.Width, childB.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Anim))
                                    {
                                        Anim childA = t as Anim;
                                        if (childA.img.bmp == null || !childA.img.Visible) continue;
                                        ImageAttributes imageAttrChild = new ImageAttributes();
                                        if (childA.img.newColorMap != null)
                                            imageAttrChild.SetRemapTable(childA.img.newColorMap);
                                        e.Graphics.DrawImage(childA.img.bmp, new Rectangle(new Point(childA.img.point.X + f.rectangle.X + Padding.X, childA.img.point.Y + f.rectangle.Y + Padding.Y), childA.img.rectangle.Size), childA.img.rectangle.X, childA.img.rectangle.Y, childA.img.rectangle.Width, childA.img.rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
                                    }
                                    else if (t != null && t.GetType() == typeof(Txt))
                                    {
                                        Txt childT = t as Txt;
                                        if (childT.Visible)
                                            e.Graphics.DrawString(childT.Text, childT.font, childT.brush, f.rectangle.X + childT.point.X + Padding.X, f.rectangle.Y + childT.point.Y + Padding.Y);
                                    }
                                    else if (t != null && t.GetType() == typeof(Rec))
                                    {
                                        Rec childR = t as Rec;
                                        if (childR.Visible)
                                            e.Graphics.FillRectangle(childR.brush, new Rectangle(new Point(childR.point.X + f.rectangle.X + Padding.X, childR.point.Y + f.rectangle.Y + Padding.Y), childR.size));
                                    }
                                    else if (t != null && t.GetType() == typeof(FillPolygon))
                                    {
                                        FillPolygon childF = t as FillPolygon;
                                        if (!childF.Visible) continue;
                                        Point[] childPoints = new Point[childF.Points.Length];
                                        for (int cnt = 0; cnt < childF.Points.Length; cnt++)
                                        {
                                            childPoints[cnt].X = f.rectangle.X + childF.Points[cnt].X + Padding.X;
                                            childPoints[cnt].Y = f.rectangle.Y + childF.Points[cnt].Y + Padding.Y;
                                        }
                                        e.Graphics.FillPolygon(childF.brush, childPoints, childF.fillMode);
                                        if (childF.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(childF.BorderColor, childF.BorderWidth), childPoints);
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                #endregion

                gfxBgrList.Clear();
                gfxObjList.Clear();
                gfxTopList.Clear();

                // Get handle to device context.
                IntPtr hdc = e.Graphics.GetHdc();

                // Release handle to device context.
                e.Graphics.ReleaseHdc(hdc);
            }
            catch (Exception ex)
            {
                if (OutputErrorCallBack != null)
                    OutputErrorCallBack("Error rendering \n" + ex);
                else if (ShowErrorsInMessageBox)
                    MessageBox.Show("error rendering \n" + ex);
            }
        }
    }
}
