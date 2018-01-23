using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Board
{
  public class SquareBoard : TiledBoard
  {
    [SerializeField]
    protected Tile empty;
    [SerializeField]
    protected Tile rough;
    [SerializeField]
    protected TileEdge edge;
    [SerializeField]
    protected TileEdge wall;

    // Computes and returns the tile distance between the start and end board elements
    // Edges are considered to have the row/column of whichever tile is closer to the other board element
    // Thus, edges of the same tile have a tile distance of 0
    public override int DistanceBetween(ITiledBoardElement start, ITiledBoardElement end) {
      throw new NotImplementedException();
    }

    protected override void CreateBoard() {
      throw new NotImplementedException();
    }

    public override Tile GetTileAtPoint(Point point) {
      throw new NotImplementedException();
    }
  }
}
