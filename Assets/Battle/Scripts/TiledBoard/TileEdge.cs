using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
  public class TileEdge : BoardEdge, ITiledBoardElement
  {
    [SerializeField]
    protected int opacity;

    public Tuple<Tile, Tile> Tiles { get; private set; }
    public IShape Shape { get { return Line; } }

    public int Opacity { get { return opacity; } }
    public LineSegment Line { get; private set; }

    public void Initialize(Tuple<Tile, Tile> adjTiles, LineSegment line) {
      Initialize(new Tuple<BoardNode, BoardNode>(adjTiles.First, adjTiles.Second));
      Tiles = adjTiles;
      Line = line;
    }

    // Given a node, returns the other node this edge is connected to
    public Tile GetOther(Tile tile) {
      if (Tiles.First.Equals(tile))
        return Tiles.Second;
      return Tiles.First;
    }
  }
}