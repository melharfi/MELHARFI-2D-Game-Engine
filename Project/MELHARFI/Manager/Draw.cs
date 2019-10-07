using MELHARFI.Manager.Gfx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;

namespace MELHARFI.Manager
{
    public partial class Manager
    {
        /// <summary>
        /// This method draw all object found on the 3 layers GfxBgrList, GfxObjList, GfxTopList on the form, this method is called inside the loop game
        /// </summary>
        /// <param name="e">e is a PaintEventArgs</param>
        private void Draw(PaintEventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
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
                if(!hideBagroundLayer)
                    lock (((ICollection)gfxBgrList).SyncRoot)
                        foreach (IGfx parentGfx in gfxBgrList)
                        {
                            switch (parentGfx.GetType().Name)
                            {
                                case "Bmp":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Bmp p = DrawBmpParent(parentGfx, zi, e);
                                        #endregion
                                        #region childs
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawFillPolygone(childGfx, p.Point, e);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
                                case "Anim":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Anim p = DrawAnimParent(parentGfx, zi, e);
                                        #endregion
                                        #region childs
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawFillPolygone(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
                                case "Txt":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Txt p = DrawTxtParent(parentGfx, e);
                                        #endregion
                                        #region childs
                                        //foreach (IGfx childGfx in p.Childs)
                                        //{
                                        //    switch(childGfx.GetType().Name)
                                        //    {
                                        //        case "Bmp":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawBmpChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "Anim":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawAnimChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "Txt":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawTxtChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "Rec":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawRecChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "FillPolygon":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawFillPolygone(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //    }
                                        //}
                                        #endregion
                                    }
                                    break;
                                case "Rec":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Rec p = DrawRecParent(parentGfx, zi, e);
                                        #endregion
                                        #region childs
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawFillPolygone(childGfx, p.Point, e);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
                                case "FillPolygon":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        FillPolygon p = DrawFillPolygonParent(parentGfx, zi, e);
                                        #endregion
                                        #region childs
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawFillPolygone(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
                            }
                        }
                #endregion

                // affichage de la 2eme couche qui contiens les Assets
                #region GfxObj
                if (!hideObjectLayer)
                    lock ((gfxObjList as ICollection).SyncRoot)
                        foreach (IGfx parentGfx in gfxObjList)
                        {
                            switch(parentGfx.GetType().Name)
                            {
                                case "Bmp":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Bmp p = DrawBmpParent(parentGfx, zi, e);
                                        #endregion
                                        #region childs
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawFillPolygone(childGfx, p.Point, e);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
                                case "Anim":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Anim p = DrawAnimParent(parentGfx, zi, e);
                                        #endregion
                                        #region childs
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawFillPolygone(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
                                case "Txt":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Txt p = DrawTxtParent(parentGfx, e);
                                        #endregion
                                        #region childs
                                        //foreach (IGfx childGfx in p.Childs)
                                        //{
                                        //    switch(childGfx.GetType().Name)
                                        //    {
                                        //        case "Bmp":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawBmpChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "Anim":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawAnimChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "Txt":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawTxtChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "Rec":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawRecChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "FillPolygon":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawFillPolygone(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //    }
                                        //}
                                        #endregion
                                    }
                                    break;
                                case "Rec":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Rec p = DrawRecParent(parentGfx, zi, e);
                                        #endregion
                                        #region childs
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawFillPolygone(childGfx, p.Point, e);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
                                case "FillPolygon":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        FillPolygon p = DrawFillPolygonParent(parentGfx, zi, e);
                                        #endregion
                                        #region childs
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawFillPolygone(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
                            }
                        }
                #endregion

                // affichage de la 3eme couche qui contiens les Assets 
                #region GfxTop
                if (!hideTopLayer)
                    lock ((gfxTopList as ICollection).SyncRoot)
                        foreach (IGfx parentGfx in gfxTopList)
                        {
                            switch (parentGfx.GetType().Name)
                            {
                                case "Bmp":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Bmp p = DrawBmpParent(parentGfx, zi, e);
                                        #endregion
                                        #region childs
                                        ////////// affichage des elements enfants de l'objet Bmp
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        FillPolygon c = childGfx as FillPolygon;
                                                        Point[] newPoint = new Point[c.Points.Length];
                                                        for (int cnt = 0; cnt < c.Points.Length; cnt++)
                                                        {
                                                            newPoint[cnt].X = p.Point.X + c.Points[cnt].X;
                                                            newPoint[cnt].Y = p.Point.Y + c.Points[cnt].Y;
                                                        }
                                                        e.Graphics.FillPolygon(c.FillColor, newPoint, c.FillMode);
                                                        if (c.BorderColor != null)
                                                            e.Graphics.DrawPolygon(new Pen(c.BorderColor, c.BorderWidth), c.Points);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
                                case "Anim":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Anim p = DrawAnimParent(parentGfx, zi, e);
                                        #endregion
                                        #region childs
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Bmp.Point, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        FillPolygon c = childGfx as FillPolygon;
                                                        Point[] newPoint = new Point[c.Points.Length];
                                                        for (int cnt = 0; cnt < c.Points.Length; cnt++)
                                                        {
                                                            newPoint[cnt].X = p.Bmp.Point.X + c.Points[cnt].X;
                                                            newPoint[cnt].Y = p.Bmp.Point.Y + c.Points[cnt].Y;
                                                        }
                                                        e.Graphics.FillPolygon(c.FillColor, newPoint, c.FillMode);
                                                        if (c.BorderColor != null)
                                                            e.Graphics.DrawPolygon(new Pen(c.BorderColor, c.BorderWidth), c.Points);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
                                case "Txt":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Txt p = DrawTxtParent(parentGfx, e);
                                        #endregion
                                        #region childs
                                        //foreach (IGfx childGfx in p.Childs)
                                        //{
                                        //    switch(childGfx.GetType().Name)
                                        //    {
                                        //        case "Bmp":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawBmpChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "Anim":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawAnimChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "Txt":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawTxtChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "Rec":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                DrawRecChild(childGfx, p.Point, e);
                                        //            }
                                        //            break;
                                        //        case "FillPolygon":
                                        //            {
                                        //                if (!childGfx.Visible) continue;
                                        //                FillPolygon c = childGfx as FillPolygon;
                                        //                Point[] newPoint = new Point[c.Points.Length];
                                        //                for (int cnt = 0; cnt < c.Points.Length; cnt++)
                                        //                {
                                        //                    newPoint[cnt].X = p.Point.X + c.Points[cnt].X;
                                        //                    newPoint[cnt].Y = p.Point.Y + c.Points[cnt].Y;
                                        //                }
                                        //                e.Graphics.FillPolygon(c.FillColor, newPoint, c.FillMode);
                                        //                if (c.BorderColor != null)
                                        //                    e.Graphics.DrawPolygon(new Pen(c.BorderColor, c.BorderWidth), c.Points);
                                        //            }
                                        //            break;
                                        //    }
                                        //}
                                        #endregion
                                    }
                                    break;
                                case "Rec":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        Rec p = DrawRecParent(parentGfx, zi, e);
                                        #endregion
                                        #region childs
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Point, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        FillPolygon c = childGfx as FillPolygon;
                                                        Point[] newPoint = new Point[c.Points.Length];
                                                        for (int cnt = 0; cnt < c.Points.Length; cnt++)
                                                        {
                                                            newPoint[cnt].X = p.Point.X + c.Points[cnt].X;
                                                            newPoint[cnt].Y = p.Point.Y + c.Points[cnt].Y;
                                                        }
                                                        e.Graphics.FillPolygon(c.FillColor, newPoint, c.FillMode);
                                                        if (c.BorderColor != null)
                                                            e.Graphics.DrawPolygon(new Pen(c.BorderColor, c.BorderWidth), c.Points);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
                                case "FillPolygon":
                                    {
                                        #region parent
                                        if (!parentGfx.Visible) continue;
                                        FillPolygon p = parentGfx as FillPolygon;
                                        p.Childs.Sort(0, p.Childs.Count, zi);
                                        e.Graphics.FillPolygon(p.FillColor, p.Points, p.FillMode);
                                        if (p.BorderColor != null)
                                            e.Graphics.DrawPolygon(new Pen(p.BorderColor, p.BorderWidth), p.Points);
                                        #endregion
                                        #region childs
                                        foreach (IGfx childGfx in p.Childs)
                                        {
                                            switch(childGfx.GetType().Name)
                                            {
                                                case "Bmp":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawBmpChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "Anim":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawAnimChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "Txt":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawTxtChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "Rec":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        DrawRecChild(childGfx, p.Rectangle.Location, e);
                                                    }
                                                    break;
                                                case "FillPolygon":
                                                    {
                                                        if (!childGfx.Visible) continue;
                                                        FillPolygon c = childGfx as FillPolygon;
                                                        Point[] newPoint = new Point[c.Points.Length];
                                                        for (int cnt = 0; cnt < c.Points.Length; cnt++)
                                                        {
                                                            newPoint[cnt].X = p.Rectangle.X + c.Points[cnt].X;
                                                            newPoint[cnt].Y = p.Rectangle.Y + c.Points[cnt].Y;
                                                        }
                                                        e.Graphics.FillPolygon(c.FillColor, newPoint, c.FillMode);
                                                        if (c.BorderColor != null)
                                                            e.Graphics.DrawPolygon(new Pen(c.BorderColor, c.BorderWidth), c.Points);
                                                    }
                                                    break;
                                            }
                                        }
                                        #endregion
                                    }
                                    break;
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
                OutputErrorCallBack("Error rendering \n" + ex);
            }
            sw.Stop();
        }

        #region parent draw methodes
        private Bmp DrawBmpParent(IGfx parentGfx, Zindex zi, PaintEventArgs e)
        {
            Bmp p = parentGfx as Bmp;
            p.Childs.Sort(0, p.Childs.Count, zi);
            ImageAttributes imageAttrParent = new ImageAttributes();
            if (p.NewColorMap != null)
                imageAttrParent.SetRemapTable(p.NewColorMap);
            e.Graphics.DrawImage(p.Bitmap, new Rectangle(p.Point, p.Rectangle.Size), p.Rectangle.X, p.Rectangle.Y, p.Rectangle.Width, p.Rectangle.Height, GraphicsUnit.Pixel, imageAttrParent);
            return p;
        }
        private Anim DrawAnimParent(IGfx parentGfx, Zindex zi, PaintEventArgs e)
        {
            Anim p = parentGfx as Anim;
            p.Childs.Sort(0, p.Childs.Count, zi);
            ImageAttributes imageAttrParent = new ImageAttributes();
            if (p.Bmp.NewColorMap != null)
                imageAttrParent.SetRemapTable(p.Bmp.NewColorMap);
            e.Graphics.DrawImage(p.Bmp.Bitmap, new Rectangle(p.Bmp.Point, p.Bmp.Rectangle.Size), p.Bmp.Rectangle.X, p.Bmp.Rectangle.Y, p.Bmp.Rectangle.Width, p.Bmp.Rectangle.Height, GraphicsUnit.Pixel, imageAttrParent);
            return p;
        }
        private Txt DrawTxtParent(IGfx parentGfx, PaintEventArgs e)
        {
            Txt p = parentGfx as Txt;
            e.Graphics.DrawString(p.Text, p.Font, p.Brush, p.Point);
            return p;
        }
        private Rec DrawRecParent(IGfx parentGfx, Zindex zi, PaintEventArgs e)
        {
            Rec p = parentGfx as Rec;
            p.Childs.Sort(0, p.Childs.Count, zi);
            e.Graphics.FillRectangle(p.FillColor, new Rectangle(p.Point, p.Size));
            if (p.BorderColor != null)
                e.Graphics.DrawRectangle(new Pen(p.BorderColor, p.BorderWidth), new Rectangle(p.Point, p.Size));
            return p;
        }
        private FillPolygon DrawFillPolygonParent(IGfx parentGfx, Zindex zi, PaintEventArgs e)
        {
            FillPolygon p = parentGfx as FillPolygon;
            p.Childs.Sort(0, p.Childs.Count, zi);
            e.Graphics.FillPolygon(p.FillColor, p.Points, p.FillMode);
            if (p.BorderColor != null)
                e.Graphics.DrawPolygon(new Pen(p.BorderColor, p.BorderWidth), p.Points);
            return p;
        }
        #endregion

        #region child draw methodes
        private void DrawBmpChild(IGfx childGfx, Point parentPoint, PaintEventArgs e)
        {
            Bmp c = childGfx as Bmp;
            ImageAttributes imageAttrChild = new ImageAttributes();
            if (c.NewColorMap != null)
                imageAttrChild.SetRemapTable(c.NewColorMap);
            e.Graphics.DrawImage(c.Bitmap, new Rectangle(new Point(c.Point.X + parentPoint.X, c.Point.Y + parentPoint.Y), c.Rectangle.Size), c.Rectangle.X, c.Rectangle.Y, c.Rectangle.Width, c.Rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
        }
        private void DrawAnimChild(IGfx childGfx, Point parentPoint, PaintEventArgs e)
        {
            Anim c = childGfx as Anim;
            ImageAttributes imageAttrChild = new ImageAttributes();
            if (c.Bmp.NewColorMap != null)
                imageAttrChild.SetRemapTable(c.Bmp.NewColorMap);
            e.Graphics.DrawImage(c.Bmp.Bitmap, new Rectangle(new Point(c.Bmp.Point.X + parentPoint.X, c.Bmp.Point.Y + parentPoint.Y), c.Bmp.Rectangle.Size), c.Bmp.Rectangle.X, c.Bmp.Rectangle.Y, c.Bmp.Rectangle.Width, c.Bmp.Rectangle.Height, GraphicsUnit.Pixel, imageAttrChild);
        }
        private void DrawTxtChild(IGfx childGfx, Point parentPoint, PaintEventArgs e)
        {
            Txt c = childGfx as Txt;
            e.Graphics.DrawString(c.Text, c.Font, c.Brush, parentPoint.X + c.Point.X, parentPoint.Y + c.Point.Y);
        }
        private void DrawRecChild(IGfx childGfx, Point parentPoint, PaintEventArgs e)
        {
            Rec c = childGfx as Rec;
            e.Graphics.FillRectangle(c.FillColor, new Rectangle(parentPoint.X + c.Point.X, parentPoint.Y + c.Point.Y, c.Size.Width, c.Size.Height));
        }
        private void DrawFillPolygone(IGfx childGfx, Point parentPoint, PaintEventArgs e)
        {
            FillPolygon c = childGfx as FillPolygon;
            Point[] newPoint = new Point[c.Points.Length];
            for (int cnt = 0; cnt < c.Points.Length; cnt++)
            {
                newPoint[cnt].X = parentPoint.X + c.Points[cnt].X;
                newPoint[cnt].Y = parentPoint.Y + c.Points[cnt].Y;
            }
            e.Graphics.FillPolygon(c.FillColor, newPoint, c.FillMode);
            if (c.BorderColor != null)
                e.Graphics.DrawPolygon(new Pen(c.BorderColor, c.BorderWidth), newPoint);
        }
        #endregion
    }
}
