using MELHARFI.Manager.Gfx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MELHARFI.Manager
{
    enum MouseEvent
    {
        MouseClic,
        MouseDoubleClic,
        MouseDown,
        MouseUp
    }
    public partial class Manager
    {
        private void MouseEventHandler(MouseEventArgs e, MouseEvent mouseEvent)
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
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Anim":
                                            {
                                                Anim c = t as Anim;
                                                if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Rec":
                                            {
                                                Rec c = t as Rec;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Txt":
                                            {
                                                Txt c = t as Txt;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "FillPolygon":
                                            {
                                                FillPolygon c = t as FillPolygon;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }

                                    }
                                }
                                #endregion
                                #region parent
                                if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                {
                                    found = true;
                                    break;
                                }
                                else
                                    continue;
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
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Anim":
                                            {
                                                Anim c = t as Anim;
                                                if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Rec":
                                            {
                                                Rec c = t as Rec;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Txt":
                                            {
                                                Txt c = t as Txt;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "FillPolygon":
                                            {
                                                FillPolygon c = t as FillPolygon;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                    }
                                }
                                #endregion
                                #region parent
                                if (HandleParentGfxMouseEvent(p.Bmp, e, mouseEvent))
                                {
                                    found = true;
                                    break;
                                }
                                else
                                    continue;
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
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Anim":
                                            {
                                                Anim c = t as Anim;
                                                if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Rec":
                                            {
                                                Rec c = t as Rec;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Txt":
                                            {
                                                Txt c = t as Txt;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "FillPolygon":
                                            {
                                                FillPolygon c = t as FillPolygon;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                    }
                                }
                                #endregion
                                #region parent
                                if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                {
                                    found = true;
                                    break;
                                }
                                else
                                    continue;
                                #endregion
                            }
                        case "Txt":
                            {
                                Txt p = gfxTopList[cnt - 1] as Txt;
                                #region parent
                                if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                {
                                    found = true;
                                    break;
                                }
                                else
                                    continue;
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
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Anim":
                                            {
                                                Anim c = t as Anim;
                                                if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Rec":
                                            {
                                                Rec c = t as Rec;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "Txt":
                                            {
                                                Txt c = t as Txt;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                        case "FillPolygon":
                                            {
                                                FillPolygon c = t as FillPolygon;
                                                if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                {
                                                    found = true;
                                                    break;
                                                }
                                                else
                                                    continue;
                                            }
                                    }
                                }
                                #endregion
                                #region parent
                                if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                {
                                    found = true;
                                    break;
                                }
                                else
                                    continue;
                                #endregion
                            }
                    }
                }

                if (found == false)
                {
                    for (int cnt = gfxObjList.Count; cnt > 0; cnt--)
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
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Txt":
                                                {
                                                    Txt c = t as Txt;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                    {
                                        found = true;
                                        break;
                                    }
                                    else
                                        continue;
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
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Txt":
                                                {
                                                    Txt c = t as Txt;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }

                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (HandleParentGfxMouseEvent(p.Bmp, e, mouseEvent))
                                    {
                                        found = true;
                                        break;
                                    }
                                    else
                                        continue;
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
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Txt":
                                                {
                                                    Txt c = t as Txt;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                    {
                                        found = true;
                                        break;
                                    }
                                    else
                                        continue;
                                    #endregion
                                }
                            case "Txt":
                                {
                                    Txt p = gfxObjList[cnt - 1] as Txt;
                                    #region parent
                                    if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                    {
                                        found = true;
                                        break;
                                    }
                                    else
                                        continue;
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
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Anim":
                                                {
                                                    Anim c = t as Anim;
                                                    if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Rec":
                                                {
                                                    Rec c = t as Rec;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "Txt":
                                                {
                                                    Txt c = t as Txt;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                            case "FillPolygon":
                                                {
                                                    FillPolygon c = t as FillPolygon;
                                                    if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                    {
                                                        found = true;
                                                        break;
                                                    }
                                                    else
                                                        continue;
                                                }
                                        }
                                    }
                                    #endregion
                                    #region parent
                                    if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                    {
                                        found = true;
                                        break;
                                    }
                                    else
                                        continue;
                                    #endregion
                                }
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
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Anim":
                                                    {
                                                        Anim c = t as Anim;
                                                        if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Rec":
                                                    {
                                                        Rec c = t as Rec;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Txt":
                                                    {
                                                        Txt c = t as Txt;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "FillPolygon":
                                                    {
                                                        FillPolygon c = t as FillPolygon;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                            }
                                        }
                                        #endregion
                                        #region parent
                                        if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                        {
                                            found = true;
                                            break;
                                        }
                                        else
                                            continue;
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
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Anim":
                                                    {
                                                        Anim c = t as Anim;
                                                        if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Rec":
                                                    {
                                                        Rec c = t as Rec;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Txt":
                                                    {
                                                        Txt c = t as Txt;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "FillPolygon":
                                                    {
                                                        FillPolygon c = t as FillPolygon;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                            }
                                        }
                                        #endregion
                                        #region parent
                                        if (HandleParentGfxMouseEvent(p.Bmp, e, mouseEvent))
                                        {
                                            found = true;
                                            break;
                                        }
                                        else
                                            continue;
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
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Anim":
                                                    {
                                                        Anim c = t as Anim;
                                                        if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Rec":
                                                    {
                                                        Rec c = t as Rec;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Txt":
                                                    {
                                                        Txt c = t as Txt;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "FillPolygon":
                                                    {
                                                        FillPolygon c = t as FillPolygon;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                            }
                                        }
                                        #endregion
                                        #region parent
                                        if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                        {
                                            found = true;
                                            break;
                                        }
                                        else
                                            continue;
                                        #endregion
                                    }
                                case "Txt":
                                    {
                                        Txt p = gfxBgrList[cnt - 1] as Txt;
                                        #region parent
                                        if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                        {
                                            found = true;
                                            break;
                                        }
                                        else
                                            continue;
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
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Anim":
                                                    {
                                                        Anim c = t as Anim;
                                                        if (HandleChildGfxMouseEvent(p, c.Bmp, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Rec":
                                                    {
                                                        Rec c = t as Rec;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "Txt":
                                                    {
                                                        Txt c = t as Txt;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                                case "FillPolygon":
                                                    {
                                                        FillPolygon c = t as FillPolygon;
                                                        if (HandleChildGfxMouseEvent(p, c, e, mouseEvent))
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                        else
                                                            continue;
                                                    }
                                            }
                                        }
                                        #endregion
                                        #region parent
                                        if (HandleParentGfxMouseEvent(p, e, mouseEvent))
                                        {
                                            found = true;
                                            break;
                                        }
                                        else
                                            continue;
                                        #endregion
                                    }
                            }
                        }
                        break;
                }

                if(mouseEvent == MouseEvent.MouseUp)
                {
                    /*recherche de l'objet Bmp depuis la list GfxPressed pour activer la relache du bouton si il na pas été fait
                    puisqu'il peux bien éviter cette evenement quand l'utilisateur reste enfancé le bouton mais bouge la sourie
                    en dehors de l'objet puis relache, a ce moment l'evenement MouseUp ne s'active pas.
                    ce code fait en sorte de rendre l'image d'origine au moin*/

                    foreach (PressedGfx t in mouseDownRecorder)
                    {
                        foreach (IGfx t1 in gfxBgrList)
                            if (t1.GetType() == typeof(Bmp) && t1 as Bmp == t.Bitmap)
                                if (!t.Bitmap.IsSpriteSheet)
                                    t.Bitmap.ChangeBmp(t.OldPath);

                        foreach (IGfx t1 in gfxObjList)
                            if (t1 != null && t1.GetType() == typeof(Bmp) && t1 as Bmp == t.Bitmap)
                                if (!t.Bitmap.IsSpriteSheet)
                                    t.Bitmap.ChangeBmp(t.OldPath);

                        foreach (IGfx t1 in gfxTopList)
                            if (t1 != null && t1.GetType() == typeof(Bmp) && t1 as Bmp == t.Bitmap)
                                if (!t.Bitmap.IsSpriteSheet)
                                    t.Bitmap.ChangeBmp(t.OldPath);
                    }
                    mouseDownRecorder.Clear();
                }
            }
            catch (Exception ex)
            {
                OutputErrorCallBack(ex.Message);
            }
        }
        #region parent
        private bool HandleParentGfxMouseEvent(Bmp p, MouseEventArgs e, MouseEvent mouseEvent)
        {
            if (!p.Visible || !new Rectangle(p.Point.X, p.Point.Y, (p.IsSpriteSheet) ? p.Rectangle.Width : p.Bitmap.Width,
                (p.IsSpriteSheet) ? p.Rectangle.Height : p.Bitmap.Height).Contains(e.Location) ||
                (p.Bitmap.GetPixel(e.X - p.Point.X + ((p.IsSpriteSheet) ? p.Rectangle.X : 0),
                e.Y - p.Point.Y + ((p.IsSpriteSheet) ? p.Rectangle.Y : 0)) != GetPixel(e.X, e.Y) &&
                !p.EscapeGfxWhileMouseClic)) return false;

            switch(mouseEvent)
            {
                case MouseEvent.MouseClic:
                    p.FireMouseClic(e);
                    break;
                case MouseEvent.MouseDoubleClic:
                    p.FireMouseDoubleClic(e);
                    break;
                case MouseEvent.MouseDown:
                    p.FireMouseDown(e);
                    break;
                case MouseEvent.MouseUp:
                    p.FireMouseUp(e);
                    break;
            }
            return true;
        }
        private bool HandleParentGfxMouseEvent(Rec p, MouseEventArgs e, MouseEvent mouseEvent)
        {
            if (!p.Visible || !new Rectangle(p.Point, p.Size).Contains(e.Location)) return false;
            {
                SolidBrush sb = p.FillColor as SolidBrush;
                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseClic)
                    return false;

                switch (mouseEvent)
                {
                    case MouseEvent.MouseClic:
                        p.FireMouseClic(e);
                        break;
                    case MouseEvent.MouseDoubleClic:
                        p.FireMouseDoubleClic(e);
                        break;
                    case MouseEvent.MouseDown:
                        p.FireMouseDown(e);
                        break;
                    case MouseEvent.MouseUp:
                        p.FireMouseUp(e);
                        break;
                }
                return true;
            }
        }
        private bool HandleParentGfxMouseEvent(Txt p, MouseEventArgs e, MouseEvent mouseEvent)
        {
            if (!p.Visible || !new Rectangle(p.Point, TextRenderer.MeasureText(p.Text, p.Font)).Contains(e.Location)) return false;
            {
                SolidBrush sb = p.Brush as SolidBrush;
                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseClic)
                    return false;
                switch (mouseEvent)
                {
                    case MouseEvent.MouseClic:
                        p.FireMouseClic(e);
                        break;
                    case MouseEvent.MouseDoubleClic:
                        p.FireMouseDoubleClic(e);
                        break;
                    case MouseEvent.MouseDown:
                        p.FireMouseDown(e);
                        break;
                    case MouseEvent.MouseUp:
                        p.FireMouseUp(e);
                        break;
                }
                return true;
            }
        }
        private bool HandleParentGfxMouseEvent(FillPolygon p, MouseEventArgs e, MouseEvent mouseEvent)
        {
            if (!p.Visible || !p.Rectangle.Contains(e.Location)) return false;
            {
                SolidBrush sb = p.FillColor as SolidBrush;
                if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() && !p.EscapeGfxWhileMouseClic)
                    return false; ;
                switch (mouseEvent)
                {
                    case MouseEvent.MouseClic:
                        p.FireMouseClic(e);
                        break;
                    case MouseEvent.MouseDoubleClic:
                        p.FireMouseDoubleClic(e);
                        break;
                    case MouseEvent.MouseDown:
                        p.FireMouseDown(e);
                        break;
                    case MouseEvent.MouseUp:
                        p.FireMouseUp(e);
                        break;
                }
                return true;
            }
        }
        #endregion
        #region child
        private bool HandleChildGfxMouseEvent(IGfx p, Bmp c, MouseEventArgs e, MouseEvent mouseEvent)
        {
            if (!p.Visible || !c.Visible || c.Bitmap == null ||
                    !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                            (c.IsSpriteSheet) ? c.Rectangle.Width : c.Bitmap.Width,
                            (c.IsSpriteSheet) ? c.Rectangle.Height : c.Bitmap.Height)
                        .Contains(e.Location) ||
                    (c.Bitmap.GetPixel(e.X - c.Point.X - p.Point.X + c.Rectangle.X,
                            e.Y - c.Point.Y - p.Point.Y + c.Rectangle.Y) !=
                        GetPixel(e.X, e.Y) && !c.EscapeGfxWhileMouseClic)) return false;
            switch (mouseEvent)
            {
                case MouseEvent.MouseClic:
                    c.FireMouseClic(e);
                    break;
                case MouseEvent.MouseDoubleClic:
                    c.FireMouseDoubleClic(e);
                    break;
                case MouseEvent.MouseDown:
                    c.FireMouseDown(e);
                    break;
                case MouseEvent.MouseUp:
                    c.FireMouseUp(e);
                    break;
            }
            return true;
        }
        private bool HandleChildGfxMouseEvent(IGfx p, Rec c, MouseEventArgs e, MouseEvent mouseEvent)
        {
            if (!p.Visible || !c.Visible || !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                c.Size.Width, c.Size.Height).Contains(e.Location)) return false;
            SolidBrush sb = c.FillColor as SolidBrush;
            if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                !c.EscapeGfxWhileMouseClic) return false;
            switch (mouseEvent)
            {
                case MouseEvent.MouseClic:
                    c.FireMouseClic(e);
                    break;
                case MouseEvent.MouseDoubleClic:
                    c.FireMouseDoubleClic(e);
                    break;
                case MouseEvent.MouseDown:
                    c.FireMouseDown(e);
                    break;
                case MouseEvent.MouseUp:
                    c.FireMouseUp(e);
                    break;
            }
            return true;
        }
        private bool HandleChildGfxMouseEvent(IGfx p, Txt c, MouseEventArgs e, MouseEvent mouseEvent)
        {
            if (!p.Visible || !c.Visible || !new Rectangle(c.Point.X + p.Point.X, c.Point.Y + p.Point.Y,
                TextRenderer.MeasureText(c.Text, c.Font).Width,
                TextRenderer.MeasureText(c.Text, c.Font).Height).Contains(e.Location)) return false;
            SolidBrush sb = c.Brush as SolidBrush;
            if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                !c.EscapeGfxWhileMouseClic) return false;
            switch (mouseEvent)
            {
                case MouseEvent.MouseClic:
                    c.FireMouseClic(e);
                    break;
                case MouseEvent.MouseDoubleClic:
                    c.FireMouseDoubleClic(e);
                    break;
                case MouseEvent.MouseDown:
                    c.FireMouseDown(e);
                    break;
                case MouseEvent.MouseUp:
                    c.FireMouseUp(e);
                    break;
            }
            return true;
        }
        private bool HandleChildGfxMouseEvent(IGfx p, FillPolygon c, MouseEventArgs e, MouseEvent mouseEvent)
        {
            if (!p.Visible || !c.Visible || !new Rectangle(c.Rectangle.X + p.Point.X, c.Rectangle.Y + p.Point.Y,
                    c.Rectangle.Width, c.Rectangle.Height).Contains(e.Location)) return false;
            SolidBrush sb = c.FillColor as SolidBrush;
            if (sb.Color.ToArgb() != GetPixel(e.X, e.Y).ToArgb() &&
                !c.EscapeGfxWhileMouseClic) return false;
            switch (mouseEvent)
            {
                case MouseEvent.MouseClic:
                    c.FireMouseClic(e);
                    break;
                case MouseEvent.MouseDoubleClic:
                    c.FireMouseDoubleClic(e);
                    break;
                case MouseEvent.MouseDown:
                    c.FireMouseDown(e);
                    break;
                case MouseEvent.MouseUp:
                    c.FireMouseUp(e);
                    break;
            }
            return true;
        }
        #endregion
    }
}
