using System.Collections;
using System.Collections.Generic;
using System;
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
    public override 
    public List<Tile> TilesSelected { get; protected set; }  //The tile(s) most recently clicked
    public List<Tile> Tiles { get; protected set; }

    // Computes and returns the tile distance between the start and end board elements
    // Edges are considered to have the row/column of whichever tile is closer to the other board element
    // Thus, edges of the same tile have a tile distance of 0
    public int DistanceBetween(IBoardElement start, IBoardElement end) {
      if (!(start is Edge || start is Tile) || !(end is Edge || end is Tile))
        throw new ArgumentException("This method does not support board elements beyond edges and tiles");
      if (start is Edge) {
        Tuple<Tile, Tile> startTiles = ((Edge)start).Tiles;
        return Math.Min(DistanceBetween(startTiles.First, end), DistanceBetween(startTiles.Second, end));
      }
      if (end is Edge) {
        Tuple<Tile, Tile> endTiles = ((Edge)end).Tiles;
        return Math.Min(DistanceBetween(start, endTiles.First), DistanceBetween(start, endTiles.Second));
      }
      Tile startTile = (Tile)start;
      Tile endTile = (Tile)end;
      int rowDiff = Math.Abs(startTile.Row - endTile.Row);
      int modStartColumn = (startTile.Column * 2) + (startTile.Row % 2);
      int modEndColumn = (endTile.Column * 2) + (endTile.Row % 2);
      int colDiff = Math.Abs(modStartColumn - modEndColumn);
      if (rowDiff >= colDiff) {
        return rowDiff;
      }
      else {
        return (colDiff + rowDiff) / 2;
      }
    }

    // Given a line segment, and an optional origin and destination board element
    //   (the origin and destination are the tiles containing the start/end points of the line by default)
    // Returns the list of board elements that this line intersects
    // Note that the origin and destination are NOT included
    // Note also that warning is printed if a line endpoint is not in the origin or destination
    public List<IBoardElement> GetIntersections(LineSegment line) {
      Tile origin = null, dest = null;
      foreach (Tile tile in Tiles) {
        if (tile.Shape.Contains(line.Start))
          origin = tile;
        if (tile.Shape.Contains(line.End))
          dest = tile;
        if (origin != null && dest != null)
          break;
      }
      return GetIntersections(origin, dest, line);
    }
    public List<IBoardElement> GetIntersections(IBoardElement origin, IBoardElement dest, LineSegment line) {
      List<IBoardElement> toReturn = new List<IBoardElement>();
    }
    // Functions as the GetIntersections function, but provides a more resilient pair of results
    // In particular, returns two lists if the input line would be approximately overlapping an edge
    //   (each list corresponds to a new line drawn to each side of the overlapped line)
    // Otherwise returns a list and 'null' in the second argument
    public Tuple<List<IBoardElement>, List<IBoardElement>> GetSafeIntersections(IBoardElement origin, IBoardElement dest, LineSegment line) {
      RegularPolygon toCheck;
      if (origin is Tile)
        toCheck = ((Tile)origin).Shape;
      else
        toCheck = ((Edge)origin).Tiles.First.Shape; // Arbitrary choice
      // It is easy to check that on a regular board, a line will only overlap an edge if
      //   it matches the angle of a polygon line segment AND intersects a vertex
      foreach (LineSegment l in toCheck.LineSegments)
        if (FPErrorManager.AreEqual(line.Angle, l.Angle))
          foreach (Point vertex in toCheck.Vertices)
            if (line.Contains(vertex)) {
              double distance = toCheck.LineSegments[0].Length * .001;  // arbitrary choice and small offset
              LineSegment offset1 = line.Transpose(line.Angle + Math.PI / 2, distance);
              LineSegment offset2 = line.Transpose(line.Angle + Math.PI / 2, distance);
              return new Tuple<List<IBoardElement>, List<IBoardElement>>(GetIntersections(offset1), GetIntersections(offset2));
            }
      return new Tuple<List<IBoardElement>, List<IBoardElement>>(GetIntersections(line), null);
    }

    public List<IBoardElement> LineOfSight(Tile origin) {
      return _FirstList(GetOpacity(origin));
    }
    public List<IBoardElement> LineOfSight(Tile origin, int distance) {
      return _FirstList(GetOpacity(origin, distance));
    }
    public List<IBoardElement> LineOfSight(Tile origin, int distance, ICollection<Point> originCheck) {
      return _FirstList(GetOpacity(origin, distance, originCheck));
    }
    public List<IBoardElement> LineOfSight(Tile origin, int distance,
      ICollection<Point> originCheck, ICollection<Point> destinationCheck) {
      return _FirstList(GetOpacity(origin, distance, originCheck, destinationCheck));
    }
    public List<IBoardElement> LineOfSight(Tile origin, int distance, int opacityCutoff) {
      return _FirstList(GetOpacity(origin, distance, opacityCutoff));
    }
    public List<IBoardElement> LineOfSight(Tile origin, int distance, int opacityCutoff, Func<IBoardElement, int> opacityMap) {
      return _FirstList(GetOpacity(origin, distance, opacityCutoff, opacityMap));
    }
    public List<IBoardElement> LineOfSight(Tile origin, int distance, ICollection<Point> originCheck,
      ICollection<Point> destinationCheck, int opacityCutoff) {
      return _FirstList(GetOpacity(origin, distance, originCheck, destinationCheck, opacityCutoff));
    }
    public List<IBoardElement> LineOfSight(Tile origin, int distance, ICollection<Point> originCheck,
      ICollection<Point> destinationCheck, int opacityCutoff, Func<IBoardElement, int> opacityMap) {
      return _FirstList(GetOpacity(origin, distance, originCheck, destinationCheck, opacityCutoff, opacityMap));
    }
    protected List<IBoardElement> _FirstList(List<Tuple<IBoardElement, int>> list) {
      List<IBoardElement> toReturn = new List<IBoardElement>();
      foreach (Tuple<IBoardElement, int> item in list)
        toReturn.Add(item.First);
      return toReturn;
    }

    public List<Tuple<IBoardElement, int>> GetOpacity(Tile origin) {
      return GetOpacity(origin, Math.Max(rows, columns));
    }
    public List<Tuple<IBoardElement, int>> GetOpacity(Tile origin, int distance) {
      return GetOpacity(origin, distance, new List<Point> { origin.Shape.Center });
    }
    public List<Tuple<IBoardElement, int>> GetOpacity(Tile origin, int distance, ICollection<Point> originCheck) {
      return GetOpacity(origin, distance, originCheck, new List<Point> { origin.Shape.Center });
    }
    public List<Tuple<IBoardElement, int>> GetOpacity(Tile origin, int distance,
      ICollection<Point> originCheck, ICollection<Point> destinationCheck) {
      return GetOpacity(origin, distance, originCheck, destinationCheck, 1);
    }
    public List<Tuple<IBoardElement, int>> GetOpacity(Tile origin, int distance, int opacityCutoff) {
      return GetOpacity(origin, distance, opacityCutoff, e => e.Opacity);
    }
    public List<Tuple<IBoardElement, int>> GetOpacity(Tile origin, int distance, int opacityCutoff, Func<IBoardElement, int> opacityMap) {
      return GetOpacity(origin, distance, new List<Point> { origin.Shape.Center }, new List<Point> { origin.Shape.Center },
        opacityCutoff, opacityMap);
    }
    public List<Tuple<IBoardElement, int>> GetOpacity(Tile origin, int distance, ICollection<Point> originCheck,
      ICollection<Point> destinationCheck, int opacityCutoff) {
      return GetOpacity(origin, distance, originCheck, destinationCheck, opacityCutoff, e => e.Opacity);
    }
    public List<Tuple<IBoardElement, int>> GetOpacity(Tile origin, int distance, ICollection<Point> originCheck,
      ICollection<Point> destinationCheck, int opacityCutoff, Func<IBoardElement, int> opacityMap) {
      List<Tuple<IBoardElement, int>> toReturn = new List<Tuple<IBoardElement, int>>();
      List<IBoardElement> hasChecked = new List<IBoardElement>();
      Queue<IBoardElement> toCheck = new Queue<IBoardElement>();
      hasChecked.Add(origin);
      foreach (Tile t in origin.GetAdjacent())
        toCheck.Enqueue(t);

      while (toCheck.Count > 0) {
        IBoardElement next = toCheck.Dequeue();
        hasChecked.Add(next);
        int opacity = GetElementOpacity(origin, next, distance);
        if (opacity < opacityCutoff) {
          toReturn.Add(new Tuple<IBoardElement, int>(next, opacity));
        }
        if (opacity + opacityMap(next) < opacityCutoff) {
          if (next is Tile) {
            foreach (Edge e in ((Tile)next).GetEdges()) {
              if (!hasChecked.Contains(e))
                toCheck.Enqueue(e);
            }
          }
          else { // next is Edge
            Tuple<Tile, Tile> nextTiles = ((Edge)next).Tiles;
            if (!hasChecked.Contains(nextTiles.First))
              toCheck.Enqueue(nextTiles.First);
            else if (!hasChecked.Contains(nextTiles.Second))  // else cause we've checked either first or second for sure
              toCheck.Enqueue(nextTiles.Second);
          }
        }
      }

      return toReturn;
    }

    public int GetElementOpacity(Tile origin, IBoardElement target) {
      return GetElementOpacity(origin, target, Math.Max(rows, columns));
    }
    public int GetElementOpacity(Tile origin, IBoardElement target, int distance) {
      return GetElementOpacity(origin, target, distance, new List<Point> { origin.Shape.Center });
    }
    public int GetElementOpacity(Tile origin, IBoardElement target, int distance, Func<IBoardElement, int> opacityMap) {
      return GetElementOpacity(origin, target, distance, new List<Point> { origin.Shape.Center }, opacityMap);
    }
    public int GetElementOpacity(Tile origin, IBoardElement target, int distance, ICollection<Point> originPoints) {
      return GetElementOpacity(origin, target, distance, originPoints, e => e.Opacity);
    }
    public int GetElementOpacity(Tile origin, IBoardElement target, int distance,
      ICollection<Point> originPoints, Func<IBoardElement, int> opacityMap) {
      return GetElementOpacity(origin, target, distance, originPoints, new List<Point> { origin.Shape.Center }, opacityMap);
    }
    public int GetElementOpacity(Tile origin, IBoardElement target, int distance,
      ICollection<Point> originPoints, ICollection<Point> destinationPoints) {
      return GetElementOpacity(origin, target, distance, originPoints, destinationPoints, e => e.Opacity);
    }
    public int GetElementOpacity(Tile origin, IBoardElement target, int distance,
      ICollection<Point> originPoints, ICollection<Point> destinationPoints, Func<IBoardElement, int> opacityMap) {
      List<Point> destPoints = new List<Point>(destinationPoints);
      if (target is Edge) {

      }
    }

    // Given a starting object, destination object, line between them
    // And optional opacityMap (which by default just retrieves the serializable opacity of each element)
    // Returns the total opacity of the path between the given points
    // Note that the starting and destination objects are NOT included in this calculation
    public int GetLineOpacity(Tile origin, IBoardElement dest, LineSegment line) {
      return GetLineOpacity(origin, dest, line, e => e.Opacity);
    }
    public int GetLineOpacity(Tile origin, IBoardElement dest, LineSegment line, Func<IBoardElement, int> opacityMap) {
      int toReturn = 0;

      foreach (IBoardElement e in GetIntersections(origin, dest, line)) {
        toReturn += opacityMap(e);
      }
      return toReturn;
    }
  }
}