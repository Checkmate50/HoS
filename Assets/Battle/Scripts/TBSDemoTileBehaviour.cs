using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
  public class TBSDemoTileBehaviour : TiledBoardBehaviour
  {
    public TBSDemoTileBehaviour() {
      Verbose = true;
    }

    public override void Clicked(Tile tile, int button) {
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
      tile.Board.Deselect(0);
      tile.Board.AddSelected(tile);
      tile.Select(0);
      Tile target = tile.Board.Tiles[0];
      foreach (Tuple<ITiledBoardElement, int> t in tile.Board.GetOpacityList(tile, 1))
        if (t.First is Tile) {
          ((Tile)t.First).Shade(0.6f);
          Debug.Log(t.Second);
        }
    }

    // Behavior when the tile is right-clicked
    public void RightClicked(Tile tile) {
      tile.Shade(0.3f);
    }

    // Behavior when this tile is selected
    public override void Selected(Tile tile, int flag) {
      tile.Shade(0.5f);
    }

    // Behavior when this tile is deselected
    public override void Deselected(Tile tile, int flag) {
      tile.RemoveShade();
    }

    // Behavior applied to all tiles when any tile is unselected
    public override void ClearSelected(Tile tile, int flag) {
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