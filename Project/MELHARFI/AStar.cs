using System;
using System.Collections.Generic;

/*
 * PATHFINDING C# Source Code
 * Edited by Mohssine EL HARFI
 * m.elharfi@gmail.com
 * pseudo : Th3-m0RpH3R
 * 
 * Original source : Bidou (http://www.csharpfr.com/auteurdetail.aspx?ID=13319)
 * 
 * Changes :
 * -The old built has a project inside to make a demonstration of the path in a simple grid, now it was cleaned and just the library is inside, no need to make any touche.
 * -the old built request the grid to be SQUARE, so what happen if you have a 5/8 grid ? it's not working, so with that built it's working with any dimention you like,
 *      because it's rendred to the nearest square value, for ex if your gride dimention is 5/8, the real gride behinde the code is 8/8, and the boxes added is considered as a wall, it's that simple ^^
 *      so you dont have to worry about it :)
 *  -The old built take the grid information from a file that you must generate by hand, now you can past the grid that you must initialise in you code by an 2D array of byte (byte[,]), that containe the obstacles so you can link to the main function, so easy
 *  -The old built use a reversed position X,Y in it's grid, that mean if you want to put an obstacle in the first line (0) and in the second column you should type gride[0,1] = 3, and in grid[x,y], the X is often refered to the columne, and Y the line
 *      so to add an obstacle with real coordinate you have to use gride[1,0] as we do always.
 *  -The code has been cleaned from some other stuff no realy interesting.
 * ****************************************************
 * 
 * How to use this :
 * in your project you must add that library as a reference, and in the top most of the code ad :
 *      using AStarAlgo;
 *  
 * in you Class you intialise the gride with :
 *      private Map _map = null;    (a new instance of the MAP that hold the grid inside) not initialised yet
        private List<MapPoint> _shortestPath = new List<MapPoint>();        (a list that hold the pathfinding node, that we should clear eache time we generate a new path)
 * 
 * we create a gride that hold the informations of our map, as obstacles (players, wall ...) for ex we'll take a X(Width) = 7, Y(Height) = 8 gride :
 *      byte[,] byteMap = new byte[7, 8];
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 8; j++)
                byteMap[i, j] = 0;  // all node is initialised as walked node for the moment
 * 
 * first we initialised the gride that hold the obstacle in our main function or anywhere :)
 * to make some node as not walking, simply as obstacle :
 *      byteMap[2, 0] = 3;          // in the column (X) 2, and line (Y) 0 there is an obstacle
 *      
 * the diferent kind of information we can put in the gride is :
 *      //walking node = 0
 *      //water node = 1
 *      //fire node = 2
 *      //wall = 3;
 *      
 *      _map = new Map(7, 8, new MapPoint(0, 0), new MapPoint(3, 0), byteMap);      // new instance of the map with the Width =7, Height = 8, Start Point (0,0) , end Point (3,0), and the gride of obstacles/informations as the 4th argument
 *      _shortestPath.Clear();  // we clear the waypoint/pathfinding node to clean it each time we make it
 *      
 *      // we check if the start point and end Point is valid
 *      if (this._map != null && this._map.StartPoint != MapPoint.InvalidPoint && this._map.EndPoint != MapPoint.InvalidPoint)
        {
            AStar AStar = new AStar(this._map);     // new instance of AStar class that take our map as argument to handle it
            List<MapPoint> sol = AStar.CalculateBestPath();     // we generate the waypoint with the CalculateBestPath() function
            if (sol != null) this._shortestPath.AddRange(sol);  // we put the result in the sol List<MapPoint>
            sol.Reverse();          // the result is generated from the endPoint to the startPoint, or we like to be reversed, from the start to the end :)
        }
 * ************************************************************
 * 
 *      // i sugest that you make a try & catch to prevent from any error, here below a sample
 *      using System;
        using System.IO;
        using System.Windows.Forms;
        using AStarAlgo;
        using System.Collections.Generic;

        namespace AStarTester
        {
            public partial class frmMain : Form
            {
                private Map _map = null;
                private List<MapPoint> _shortestPath = new List<MapPoint>();
 *              
 *              private void MaFunction_Clic(object sender, EventArgs e)
 *              {
 *                  try
                    {
                        byte[,] byteMap = new byte[7, 8];
                        for (int i = 0; i < 7; i++)
                            for (int j = 0; j < 8; j++)
                                byteMap[i, j] = 0;

                        byteMap[2, 0] = 3;      // wall

                        _map = new Map(7, 8, new MapPoint(0, 0), new MapPoint(3, 0), byteMap);
                        _shortestPath.Clear();

                        if (this._map != null && this._map.StartPoint != MapPoint.InvalidPoint && this._map.EndPoint != MapPoint.InvalidPoint)
                        {
                            AStar AStar = new AStar(this._map);
                            List<MapPoint> sol = AStar.CalculateBestPath();
                            if (sol != null) this._shortestPath.AddRange(sol);
                            sol.Reverse();
                            string data = "";
                            foreach (MapPoint mp in sol)
                                data += "(" + mp.X + "-" + mp.Y + ") ";
                            MessageBox.Show(data);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Erreur lors du chargement !");
                    }
 *              }
 *          }
 *          
 * 
 * ***********************************************************
 *          IF YOU LIKE IT? MAKE A DONATION TO CONTRIBUTE THE DEVELOPPEMENT plz :)
 */
namespace MELHARFI
{
    namespace AStarAlgo
    {
        public class AStar
        {
            private Map _map = null;
            private SortedNodeList<Node> _open = new SortedNodeList<Node>();
            private NodeList<Node> _close = new NodeList<Node>();

            public AStar(Map map)
            {
                if (map == null) throw new ArgumentException("map cannot be null");
                this._map = map;
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Calculate the shortest path between the start point and the end point.
            /// </summary>
            /// <remarks> The path is reversed. </remarks>
            /// <returns> The shortest path. </returns>
            /// ----------------------------------------------------------------------------------------
            public List<MapPoint> CalculateBestPath()
            {
                Node.Map = this._map;
                Node startNode = new Node(null, this._map.StartPoint);
                this._open.Add(startNode);

                while (this._open.Count > 0)
                {
                    Node best = this._open.RemoveFirst();           // This is the best node
                    if (best.MapPoint == this._map.EndPoint)        // We are finished
                    {
                        List<MapPoint> sol = new List<MapPoint>();  // The solution
                        while (best.Parent != null)
                        {
                            sol.Add(best.MapPoint);
                            best = best.Parent;
                        }
                        return sol; // Return the solution when the parent is null (the first point)
                    }
                    this._close.Add(best);
                    this.AddToOpen(best, best.GetPossibleNode());
                }
                // No path found
                return null;
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Add a list of nodes to the open list if needed.
            /// </summary>
            /// <param name="current"> The current nodes. </param>
            /// <param name="nodes"> The nodes to add. </param>
            /// ----------------------------------------------------------------------------------------
            private void AddToOpen(Node current, IEnumerable<Node> nodes)
            {
                foreach (Node node in nodes)
                {
                    if (!this._open.Contains(node))
                    {
                        if (!this._close.Contains(node)) this._open.AddDichotomic(node);
                    }
                    // Else really nedded ?
                    else
                    {
                        if (node.CostWillBe() < this._open[node].Cost) node.Parent = current;
                    }
                }
            }
        }
        internal class Node : INode
        {
            // Represents the map
            private static Map _map = null;

            private int _costG = 0; // From start point to here
            private Node _parent = null;
            private MapPoint _currentPoint = MapPoint.InvalidPoint;

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Create a new Node.
            /// </summary>
            /// <param name="parent"> The parent node. </param>
            /// <param name="currentPoint"> The current point. </param>
            /// ----------------------------------------------------------------------------------------
            public Node(Node parent, MapPoint currentPoint)
            {
                this._currentPoint = currentPoint;
                this.SetParent(parent);
            }

            #region Properties

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get or set the Map.
            /// </summary>
            /// ----------------------------------------------------------------------------------------
            public static Map Map
            {
                get { return _map; }
                set { _map = value; }
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get or set the parent.
            /// </summary>
            /// ----------------------------------------------------------------------------------------
            public Node Parent
            {
                get { return this._parent; }
                set { this.SetParent(value); }
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get the cost.
            /// </summary>
            /// ----------------------------------------------------------------------------------------
            public int Cost
            {
                get { return this._costG; }
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get the F distance (Cost + Heuristic).
            /// </summary>
            /// ----------------------------------------------------------------------------------------
            public int F
            {
                get { return this._costG + this.GetHeuristic(); }
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get the location of the node.
            /// </summary>
            /// ----------------------------------------------------------------------------------------
            public MapPoint MapPoint
            {
                get { return this._currentPoint; }
            }

            #endregion

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Set the parent.
            /// </summary>
            /// <param name="parent"> The parent to set. </param>
            /// ----------------------------------------------------------------------------------------
            private void SetParent(Node parent)
            {
                this._parent = parent;
                // Refresh the cost : the cost of the parent + the cost of the current point
                if (parent != null) this._costG = this._parent.Cost + _map.GetCost(this._currentPoint);
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// The cost if you move to this.
            /// </summary>
            /// <returns> The futur cost. </returns>
            /// --------- -------------------------------------------------------------------------------
            public int CostWillBe()
            {
                return (this._parent != null ? this._parent.Cost + _map.GetCost(this._currentPoint) : 0);
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Calculate the heuristic. (absolute x and y displacement).
            /// </summary>
            /// <returns> The heuristic. </returns>
            /// ----------------------------------------------------------------------------------------
            public int GetHeuristic()
            {
                return (Math.Abs(this._currentPoint.X - _map.EndPoint.X) + Math.Abs(this._currentPoint.Y - _map.EndPoint.Y));
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get the possible node.
            /// </summary>
            /// <returns> A list of possible node. </returns>
            /// ----------------------------------------------------------------------------------------
            public List<Node> GetPossibleNode()
            {
                List<Node> nodes = new List<Node>();
                MapPoint mapPt = new MapPoint();

                // Top
                mapPt.X = _currentPoint.X;
                mapPt.Y = _currentPoint.Y + 1;
                if (!_map.IsWall(mapPt)) nodes.Add(new Node(this, mapPt.Clone()));

                // Right
                mapPt.X = _currentPoint.X + 1;
                mapPt.Y = _currentPoint.Y;
                if (!_map.IsWall(mapPt)) nodes.Add(new Node(this, mapPt.Clone()));

                // Left
                mapPt.X = _currentPoint.X - 1;
                mapPt.Y = _currentPoint.Y;
                if (!_map.IsWall(mapPt)) nodes.Add(new Node(this, mapPt.Clone()));

                // Bottom
                mapPt.X = _currentPoint.X;
                mapPt.Y = _currentPoint.Y - 1;
                if (!_map.IsWall(mapPt)) nodes.Add(new Node(this, mapPt.Clone()));

                return nodes;
            }
        }
        internal class NodeList<T> : List<T> where T : INode
        {
            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Remove and return the first node.
            /// </summary>
            /// <returns> The first Node. </returns>
            /// ----------------------------------------------------------------------------------------
            public T RemoveFirst()
            {
                T first = this[0];
                this.RemoveAt(0);
                return first;
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Chek if the collection contains a Node (the MapPoint are compared by value!).
            /// </summary>
            /// <param name="node"> The node to check. </param>
            /// <returns> True if it's contained, otherwise false. </returns>
            /// ----------------------------------------------------------------------------------------
            public new bool Contains(T node)
            {
                return this[node] != null;
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get a node from the collection (the MapPoint are compared by value!).
            /// </summary>
            /// <param name="node"> The node to get. </param>
            /// <returns> The node with the same MapPoint. </returns>
            /// ----------------------------------------------------------------------------------------
            public T this[T node]
            {
                get
                {
                    foreach (T n in this)
                    {
                        if (n.MapPoint == node.MapPoint) return n;
                    }
                    return default(T);
                }
            }
        }
        internal class SortedNodeList<T> : NodeList<T> where T : INode
        {
            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Insert the node in the collection with a dichotomic algorithm.
            /// </summary>
            /// <param name="node"> The node to add.</param>
            /// ----------------------------------------------------------------------------------------
            public void AddDichotomic(T node)
            {
                int left = 0;
                int right = this.Count - 1;
                int center = 0;

                while (left <= right)
                {
                    center = (left + right) / 2;
                    if (node.F < this[center].F) right = center - 1;
                    else if (node.F > this[center].F) left = center + 1;
                    else { left = center; break; }
                }
                this.Insert(left, node);
            }
        }
        public class MapPoint
        {
            private int _x = 0;
            private int _y = 0;

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Create a new MapPoint.
            /// </summary>
            /// ----------------------------------------------------------------------------------------
            public MapPoint()
            {
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Create a new MapPoint.
            /// </summary>
            /// <param name="x"> The x-coordinate. </param>
            /// <param name="y"> The x-coordinate. </param>
            /// ----------------------------------------------------------------------------------------
            public MapPoint(int x, int y)
            {
                this._x = x;
                this._y = y;
            }

            #region Properties

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get an invalid point.
            /// </summary>
            /// ----------------------------------------------------------------------------------------
            public static MapPoint InvalidPoint
            {
                get { return new MapPoint(-1, -1); }
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get the x-coordinate.
            /// </summary>
            /// ----------------------------------------------------------------------------------------
            public int X
            {
                get { return this._x; }
                internal set { this._x = value; }
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get the y-coordinate.
            /// </summary>
            /// ----------------------------------------------------------------------------------------
            public int Y
            {
                get { return this._y; }
                internal set { this._y = value; }
            }

            #endregion

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Operator override ! Now : value comparison.
            /// </summary>
            /// <param name="labyPt1"> The 1st point. </param>
            /// <param name="labyPt2"> The 2nd point. </param>
            /// <returns> True if the points are equals (by value!). </returns>
            /// ----------------------------------------------------------------------------------------
            public static bool operator ==(MapPoint labyPt1, MapPoint labyPt2)
            {
                return (labyPt1.X == labyPt2.X && labyPt1.Y == labyPt2.Y);
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Operator override ! Now : value comparison.
            /// </summary>
            /// <param name="point1"> The 1st point. </param>
            /// <param name="point2"> The 2nd point. </param>
            /// <returns> True if the points are equals (by value!). </returns>
            /// ----------------------------------------------------------------------------------------
            public static bool operator !=(MapPoint point1, MapPoint point2)
            {
                return !(point1 == point2);
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Value comparison.
            /// </summary>
            /// <param name="obj">The object to compare.</param>
            /// <returns> True if the points are equals (by value!). </returns>
            /// ----------------------------------------------------------------------------------------
            public override bool Equals(object obj)
            {
                if (!(obj is MapPoint)) return false;
                MapPoint point = (MapPoint)obj;
                return point == this;
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// This is the same implementation than System.Drawing.Point.
            /// </summary>
            /// <returns></returns>
            /// ----------------------------------------------------------------------------------------
            public override int GetHashCode()
            {
                return (this._x ^ this._y);
            }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Clone the current object.
            /// </summary>
            /// <returns> A new instance with the same content. </returns>
            /// ----------------------------------------------------------------------------------------
            public MapPoint Clone()
            {
                return new MapPoint(this._x, this._y);
            }
        }
        public interface INode
        {
            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get the F distance (Cost + Heuristic).
            /// </summary>
            /// ----------------------------------------------------------------------------------------
            int F { get; }

            /// ----------------------------------------------------------------------------------------
            /// <summary>
            /// Get the location of the node.
            /// </summary>
            /// ----------------------------------------------------------------------------------------
            MapPoint MapPoint { get; }
        }
        public class Map
        {
            private int[,] _costs = null;
            private byte[,] _map = null;
            private MapPoint _startPt = MapPoint.InvalidPoint;
            private MapPoint _endPt = MapPoint.InvalidPoint;
            private int width;
            private int height;
            private int maxSize;

            public Map(int Width, int Height, MapPoint StartPt, MapPoint EndPt, byte[,] byteMap)
            {
                // Load the map and assign the costs
                this.width = Width;
                this.height = Height;
                _startPt = StartPt;
                _endPt = EndPt;

                this.LoadLabyrinthe(byteMap);
            }

            #region Properties

            public int Length
            {
                get { return this._map.GetLength(0); }
            }

            public int Size
            {
                get { return this.Length * this.Length; }
            }

            public MapPoint StartPoint
            {
                get { return this._startPt; }
            }

            public MapPoint EndPoint
            {
                get { return this._endPt; }
            }

            public byte this[int x, int y]
            {
                get { return this._map[x, y]; }
            }

            #endregion
            public void LoadLabyrinthe(byte[,] byteMap)
            {
                // Start and endpoint
                maxSize = (width > height) ? width : height;

                this._map = new byte[maxSize, maxSize];  // Constraint : game must be square !
                this._costs = new int[maxSize, maxSize]; // Constraint : costs must be square !

                for (int i = 0; i < maxSize; i++)
                    for (int j = 0; j < maxSize; j++)
                        _map[i, j] = 0;

                //remise en obstacle tous les cases ajoutés
                for (int i = 0; i < maxSize; i++)
                    for (int j = 0; j < maxSize; j++)
                        if (width < height && j >= width)
                            _map[i, j] = 3;
                        else if (height < width && j >= height)
                            _map[j, i] = 3;

                // remise en obstacle les éléments dans la liste
                for (int i = 0; i < byteMap.GetLength(0); i++)
                    for (int j = 0; j < byteMap.GetLength(1); j++)
                        _map[j, i] = byteMap[i, j];

                // reafectaion des id des obstacles
                for (int i = 0; i < this.Length; i++)
                {
                    for (int j = 0; j < this.Length; j++)
                    {
                        int cost = 0;
                        switch (this._map[i, j])
                        {
                            case 0: cost = 1; break;    // Normal
                            case 1: cost = 2; break;    // Water
                            case 2: cost = 5; break;    // Fire
                            case 3: cost = -1; break;   // Wall
                            default: cost = 1; break;   // Normal
                        }
                        this._costs[j, i] = cost;
                    }
                }
            }

            /// ----------------------------------------------------------------------------------------
            /// Check if a point is valid.
            /// <returns> True if the point is valid, otherwise false. </returns>
            /// ----------------------------------------------------------------------------------------
            public bool IsPointValid(MapPoint labyPt)
            {
                return (this.Length > labyPt.X && labyPt.X >= 0 && labyPt.Y >= 0 && this.Length > labyPt.Y);
            }

            /// ----------------------------------------------------------------------------------------
            /// Check if the current point is a wall (outside point = wall).
            /// <returns> True if it is a wall. </returns>
            /// ----------------------------------------------------------------------------------------
            public bool IsWall(MapPoint labyPt)
            {
                return this.GetCost(labyPt) < 0;
            }

            /// ----------------------------------------------------------------------------------------
            /// Get the cost of a Point.
            /// ----------------------------------------------------------------------------------------
            public int GetCost(MapPoint labyPt)
            {
                if (this.IsPointValid(labyPt)) return this._costs[labyPt.X, labyPt.Y];
                return -2;
            }
        }
    }
}