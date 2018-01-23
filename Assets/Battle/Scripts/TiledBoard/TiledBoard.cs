using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace Board
{
  public abstract class TiledBoard : Board
  {
    [SerializeField]
    protected int rows;
    [SerializeField]
    protected int columns;
    [SerializeField]
    protected float tileLength;

    public Tile[,] Grid { get; protected set; }
    public List<Tile> TilesSelected { get; protected set; }  //The tile(s) most recently clicked
    public List<Tile> Tiles { get; protected set; }

    public override int DistanceBetween(IBoardElement start, IBoardElement end) {
      return DistanceBetween(BoardToTile(start), BoardToTile(end));
    }
    // Computes and returns the tile distance between the start and end board elements
    // Edges are considered to have the row/column of whichever tile is closer to the other board element
    // Thus, edges of the same tile have a tile distance of 0
    public abstract int DistanceBetween(ITiledBoardElement start, ITiledBoardElement end);

    public abstract Tile GetTileAtPoint(Point point);

    // Given a line segment, and an optional origin and destination board element
    //   (the origin and destination are the tiles containing the start/end points of the line by default)
    // Returns the list of board elements that this line intersects
    // Note that the origin and destination are NOT included
    // Note also that warning is printed if a line endpoint is not in the origin or destination
    public List<ITiledBoardElement> GetIntersections(LineSegment line) {
      return GetIntersections(GetTileAtPoint(line.Start), line);
    }
    public List<ITiledBoardElement> GetIntersections(ITiledBoardElement origin, LineSegment line) {
      return GetIntersections(origin, null, line);
    }
    public List<ITiledBoardElement> GetIntersections(ITiledBoardElement origin, ITiledBoardElement dest, LineSegment line) {
      Tile nextTile;
      if (origin is Tile)
        nextTile = ((Tile)origin);
      else
        nextTile = ((TileEdge)origin).Tiles.First; // Arbitrary choice
      List<ITiledBoardElement> toReturn = new List<ITiledBoardElement>();
      while (!dest.Equals(nextTile)) {
        Point intersectPoint = Point.ClosestToPoint(line.End, nextTile.Polygon.Intersection(line));
        TileEdge nextEdge = nextTile.GetTileEdge(intersectPoint);
        if (nextEdge == null) {
          if (BoardBehaviour.Verbose)
            Debug.Log("End target " + dest + " not reached along line " + line);
          return toReturn;
        }
        toReturn.Add(nextEdge);
        if (dest.Equals(nextEdge))
          return toReturn;
        nextTile = nextEdge.GetOther(nextTile);
        toReturn.Add(nextTile);
      }
      return toReturn;
    }

    // Functions as the GetIntersections function, but provides a more resilient pair of results
    // In particular, returns two lists if the input line would be approximately overlapping an TileEdge
    //   (each list corresponds to a new line drawn to each side of the overlapped line)
    // Otherwise returns a list and 'null' in the second argument
    public Tuple<List<ITiledBoardElement>, List<ITiledBoardElement>> GetSafeIntersections
      (ITiledBoardElement origin, ITiledBoardElement dest, LineSegment line) {
      RegularPolygon toCheck;
      if (origin is Tile)
        toCheck = ((Tile)origin).Polygon;
      else
        toCheck = ((TileEdge)origin).Tiles.First.Polygon; // Arbitrary choice
      // It is easy to check that on a regular board, a line will only overlap an TileEdge if
      //   it matches the angle of a polygon line segment AND intersects a vertex
      foreach (LineSegment l in toCheck.LineSegments)
        if (FPErrorManager.AreEqual(line.Angle, l.Angle))
          foreach (Point vertex in toCheck.Vertices)
            if (line.Contains(vertex)) {
              double distance = toCheck.LineSegments[0].Length * .001;  // arbitrary choice and small offset
              LineSegment offset1 = line.Transpose(line.Angle + Math.PI / 2, distance).Extend(distance * 10, line.End);
              LineSegment offset2 = line.Transpose(line.Angle - Math.PI / 2, distance).Extend(distance * 10, line.End);
              return new Tuple<List<ITiledBoardElement>, List<ITiledBoardElement>>
                (GetIntersections(origin, dest, offset1), GetIntersections(origin, dest, offset2));
            }
      return new Tuple<List<ITiledBoardElement>, List<ITiledBoardElement>>(GetIntersections(origin, dest, line), null);
    }

    public List<ITiledBoardElement> LineOfSight(Tile origin) {
      return _FirstList(GetOpacityList(origin));
    }
    public List<ITiledBoardElement> LineOfSight(Tile origin, int distance) {
      return _FirstList(GetOpacityList(origin, distance));
    }
    public List<ITiledBoardElement> LineOfSight(Tile origin, int distance, ICollection<Point> originCheck) {
      return _FirstList(GetOpacityList(origin, distance, originCheck));
    }
    public List<ITiledBoardElement> LineOfSight(Tile origin, int distance,
      ICollection<Point> originCheck, Func<ITiledBoardElement, ICollection<Point>> destinationPoints) {
      return _FirstList(GetOpacityList(origin, distance, originCheck, destinationPoints));
    }
    public List<ITiledBoardElement> LineOfSight(Tile origin, int distance, int opacityCutoff) {
      return _FirstList(GetOpacityList(origin, distance, opacityCutoff));
    }
    public List<ITiledBoardElement> LineOfSight(Tile origin, int distance, int opacityCutoff, Func<ITiledBoardElement, int> opacityMap) {
      return _FirstList(GetOpacityList(origin, distance, opacityCutoff, opacityMap));
    }
    public List<ITiledBoardElement> LineOfSight(Tile origin, int distance, ICollection<Point> originCheck,
      Func<ITiledBoardElement, ICollection<Point>> destinationPoints, int opacityCutoff) {
      return _FirstList(GetOpacityList(origin, distance, originCheck, destinationPoints, opacityCutoff));
    }
    public List<ITiledBoardElement> LineOfSight(Tile origin, int distance, ICollection<Point> originCheck,
      Func<ITiledBoardElement, ICollection<Point>> destinationPoints, int opacityCutoff, Func<ITiledBoardElement, int> opacityMap) {
      return _FirstList(GetOpacityList(origin, distance, originCheck, destinationPoints, opacityCutoff, opacityMap));
    }

    // Given an origin tile, returns a tuple of every tile and the opacity of drawing a line between the origin and that tile
    // Optional arguments as as follows:
    //   distance - the maximum distance of tile to consider
    //   originPoints - the points to start checking opacity from
    //   destinationPoints - a function to indicate the points of the destination to check opacity to
    public List<Tuple<ITiledBoardElement, int>> GetOpacityList(Tile origin) {
      return GetOpacityList(origin, Math.Max(rows, columns));
    }
    public List<Tuple<ITiledBoardElement, int>> GetOpacityList(Tile origin, int distance) {
      return GetOpacityList(origin, distance, new List<Point> { origin.Shape.Center });
    }
    public List<Tuple<ITiledBoardElement, int>> GetOpacityList(Tile origin, int distance, ICollection<Point> originPoints) {
      return GetOpacityList(origin, distance, originPoints, e => new List<Point> { e.Shape.Center });
    }
    public List<Tuple<ITiledBoardElement, int>> GetOpacityList(Tile origin, int distance,
      ICollection<Point> originPoints, Func<ITiledBoardElement, ICollection<Point>> destinationPoints) {
      return GetOpacityList(origin, distance, originPoints, destinationPoints, 1);
    }
    public List<Tuple<ITiledBoardElement, int>> GetOpacityList(Tile origin, int distance, int opacityCutoff) {
      return GetOpacityList(origin, distance, opacityCutoff, e => e.Opacity);
    }
    public List<Tuple<ITiledBoardElement, int>> GetOpacityList(Tile origin, int distance, int opacityCutoff, Func<ITiledBoardElement, int> opacityMap) {
      return GetOpacityList(origin, distance, new List<Point> { origin.Shape.Center }, e => new List<Point> { origin.Shape.Center },
        opacityCutoff, opacityMap);
    }
    public List<Tuple<ITiledBoardElement, int>> GetOpacityList(Tile origin, int distance, ICollection<Point> originPoints,
      Func<ITiledBoardElement, ICollection<Point>> destinationPoints, int opacityCutoff) {
      return GetOpacityList(origin, distance, originPoints, destinationPoints, opacityCutoff, e => e.Opacity);
    }
    public List<Tuple<ITiledBoardElement, int>> GetOpacityList(Tile origin, int distance, ICollection<Point> originPoints,
      Func<ITiledBoardElement, ICollection<Point>> destinationPoints, int opacityCutoff, Func<ITiledBoardElement, int> opacityMap) {
      //Get the nodes
      Func<BoardEdge, BoardNode, int> costMap =
        (e, n) => GetElementOpacity(origin, BoardToTile(n), originPoints, destinationPoints(BoardToTile(n)), opacityMap);
      List<Tuple<BoardNode, int>> foundNodes = Search(origin, l => false, costMap,
        (t, e, p) => costMap(e, t) < opacityCutoff && DistanceBetween(origin, t) <= distance, e => 0);
      // Convert the nodes to the correct format and setup edge calculations
      List<Tuple<ITiledBoardElement, int>> toReturn = new List<Tuple<ITiledBoardElement, int>>();
      Dictionary<BoardNode, int> opacityValues = new Dictionary<BoardNode, int>();
      List<TileEdge> foundEdges = new List<TileEdge>();
      foreach (Tuple<BoardNode, int> foundNode in foundNodes) {
        toReturn.Add(new Tuple<ITiledBoardElement, int>(BoardToTile(foundNode.First), foundNode.Second));
        opacityValues.Add(foundNode.First, foundNode.Second);
      }
      // Actually get the edge values
      foreach (Tuple<BoardNode, int> foundNode in foundNodes) {
        Tile foundTile = (Tile)foundNode.First;
        foreach (TileEdge edge in foundTile.GetEdges()) {
          if (!foundEdges.Contains(edge)) {
            foundEdges.Add(edge);
            Func<BoardNode, int> f = n => opacityValues.ContainsKey(n) ? opacityValues[n] : int.MaxValue;
            toReturn.Add(new Tuple<ITiledBoardElement, int>(edge, Math.Min(f(foundTile), f(edge.GetOther(foundTile)))));
          }
        }
      }

      return toReturn;
    }

    // Given a tile origin and element (tile or edge) destination
    // Returns the total opacity betwee the origin and target
    // Optional arguments are as follows:
    //   opacityMap- a function indicating what opacity to assign each board element, defaults to the serializable opacity of each element
    //   origin/targetPoints- a list of points; the minimum opacity between pairs of these points is the returned value
    //                        defaults to a one element list of the center of the origin and target
    public int GetElementOpacity(Tile origin, ITiledBoardElement destination) {
      return GetElementOpacity(origin, destination, new List<Point> { origin.Shape.Center });
    }
    public int GetElementOpacity(Tile origin, ITiledBoardElement destination, Func<ITiledBoardElement, int> opacityMap) {
      return GetElementOpacity(origin, destination, new List<Point> { origin.Shape.Center }, opacityMap);
    }
    public int GetElementOpacity(Tile origin, ITiledBoardElement destination, ICollection<Point> originPoints) {
      return GetElementOpacity(origin, destination, originPoints, e => e.Opacity);
    }
    public int GetElementOpacity(Tile origin, ITiledBoardElement destination,
      ICollection<Point> originPoints, Func<ITiledBoardElement, int> opacityMap) {
      return GetElementOpacity(origin, destination, originPoints, new List<Point> { destination.Shape.Center }, opacityMap);
    }
    public int GetElementOpacity(Tile origin, ITiledBoardElement destination,
      ICollection<Point> originPoints, ICollection<Point> destinationPoints) {
      return GetElementOpacity(origin, destination, originPoints, destinationPoints, e => e.Opacity);
    }
    public int GetElementOpacity(Tile origin, ITiledBoardElement destination,
      ICollection<Point> originPoints, ICollection<Point> destinationPoints, Func<ITiledBoardElement, int> opacityMap) {
      int opacity = int.MaxValue;
      foreach (Point op in originPoints) {
        foreach (Point dp in destinationPoints) {
          int tempOpacity = 0;
          Tuple<List<ITiledBoardElement>, List<ITiledBoardElement>> intersections = GetSafeIntersections(origin, destination, new LineSegment(op, dp));
          foreach (ITiledBoardElement e in intersections.First)
            tempOpacity += opacityMap(e);
          opacity = Math.Min(opacity, tempOpacity);
          if (intersections.Second != null) {
            tempOpacity = 0;
            foreach (ITiledBoardElement e in intersections.First)
              tempOpacity += opacityMap(e);
            opacity = Math.Min(opacity, tempOpacity);
          }
        }
      }
      return opacity;
    }

    // Given a starting object, destination object, line between them
    // And optional opacityMap (which by default just retrieves the serializable opacity of each element)
    // Returns the total opacity of the path between the given points
    // Note that the starting and destination objects are NOT included in this calculation
    public int GetLineOpacity(Tile origin, ITiledBoardElement dest, LineSegment line) {
      return GetLineOpacity(origin, dest, line, e => e.Opacity);
    }
    public int GetLineOpacity(Tile origin, ITiledBoardElement dest, LineSegment line, Func<ITiledBoardElement, int> opacityMap) {
      int toReturn = 0;
      foreach (ITiledBoardElement e in GetIntersections(origin, line)) {
        toReturn += opacityMap(e);
      }
      return toReturn;
    }

    protected List<ITiledBoardElement> _FirstList(List<Tuple<ITiledBoardElement, int>> list) {
      List<ITiledBoardElement> toReturn = new List<ITiledBoardElement>();
      foreach (Tuple<ITiledBoardElement, int> item in list)
        toReturn.Add(item.First);
      return toReturn;
    }
    private ITiledBoardElement BoardToTile(IBoardElement element) {
      if (element is ITiledBoardElement)
        return (ITiledBoardElement)element;
      throw new ArgumentException("Tile board cannot act on an ITiledBoardElement that was not a tile board element");
    }
  }
}