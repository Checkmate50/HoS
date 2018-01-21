using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
  public class TBSDemoTileBehavior : TileBehavior
  {
    public void Clicked(Tile tile, int button) {
      switch (button) {
        case 0:
          LeftClicked(tile);
          break;
        case 1:
          RightClicked(tile);
          break;
      }
    }

    // Behavior when the tile is left-clicked
    public void LeftClicked(Tile tile) {
      tile.board.Deselect(0);
      tile.board.AddSelected(tile);
      tile.Select(0);
      foreach (Tile t in Board.Explore(tile, 3, (Edge e) => !(e is Wall)))
        t.Shade(0.6f);
    }

    // Behavior when the tile is right-clicked
    public void RightClicked(Tile tile) {
      tile.Shade(0.3f);
    }

    // Behavior when this tile is selected
    public void Selected(Tile tile, int flag) {
      tile.Shade(0.5f);
    }

    // Behavior when this tile is deselected
    public void Deselected(Tile tile, int flag) {
      tile.RemoveShade();
    }

    // Behavior applied to all tiles when any tile is unselected
    public void ClearSelected(Tile tile, int flag) {
      tile.RemoveShade();
    }

    // Behavior applied when a tile that was default search is cleared
    public void ClearSearched(Tile tile, int flag) {
      switch (flag) {
        case 0:
          tile.RemoveShade(0.6f);
          break;
        case 1:
          tile.RemoveShade(0.7f);
          break;
      }
    }
  }
}