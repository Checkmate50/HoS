using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
  public class Tile : BoardNode, ITiledBoardElement
  {
    [SerializeField]
    private int opacity;

    new public TiledBoard Board { get { return (TiledBoard)base.Board; } }
    public IShape Shape { get { return Polygon; } }
    public RegularPolygon Polygon { get; private set; }
    private Dictionary<LineSegment, TileEdge> geometricMap;
    public int Opacity { get { return opacity; }  }

    public int Row { get; private set; }
    public int Column { get; private set; }

    public Tile() {
      geometricMap = new Dictionary<LineSegment, TileEdge>();
    }

    public void Initialize(Board board, BoardBehaviour behavior, RegularPolygon polygon, int row, int col) {
      Initialize(board, behavior);
      Row = row;
      Column = col;
      Polygon = polygon;
    }

    public override string ToString() {
      return "(" + Row + ", " + Column + ")";
    }

    // Add an edge and associated BoardNode
    // An optional line segment allows specification on the direction of the new BoardNode
    // If this is not provided, the edge will be assumed to be in the geometric direction of the new BoardNode
    public void AddEdge(Tile toAdd, TileEdge edge) {
      LineSegment checkLine = new LineSegment(Shape.Center, toAdd.Shape.Center);
      foreach (LineSegment ls in Shape.LineSegments) {
        if (ls.Intersection(checkLine) != null) {
          AddEdge(toAdd, edge, ls);
          return;
        }
      }
      AddEdge(toAdd, edge, null);
    }
    public void AddEdge(Tile toAdd, TileEdge edge, LineSegment line) {
      AddEdge(toAdd, edge, line, true);
    }
    protected void AddEdge(Tile toAdd, TileEdge edge, LineSegment line, bool willDuplicate) {
      if (Behaviour.Verbose && line == null)
        Debug.Log("Warning: tiles " + ToString() + " and " + toAdd.ToString() + " were not given a geometric edge");
      AddEdge(toAdd, edge, false);
      geometricMap.Add(line, edge);
      if (willDuplicate) {
        edge.Initialize(new Tuple<Tile, Tile>(this, toAdd), line);
        toAdd.AddEdge(this, edge, line, false);
      }
    }

    // Returns the tile edge whose geometry intersects the given point (or null if none exist)
    public TileEdge GetTileEdge(Point point) {
      foreach (LineSegment line in Polygon.LineSegments)
        if (line.Contains(point))
          return GetTileEdge(line);
      return null;
    }
    // Returns the tile edge corresponding to the given line (or null if none exist)
    public TileEdge GetTileEdge(LineSegment line) {
      if (geometricMap.ContainsKey(line))
        return geometricMap[line];
      return null;
    }
  }
}
