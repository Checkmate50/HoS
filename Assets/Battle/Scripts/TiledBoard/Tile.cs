using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
  public class Tile : BoardNode
  {
    public RegularPolygon Shape { get; private set; }

    public void Initialize(Board board, RegularPolygon shape, int row, int col) {
      Initialize(board, row, col);
      Shape = shape;
    }
  }
}
