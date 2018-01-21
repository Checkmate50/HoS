using System;
using System.Collections.Generic;
using UnityEngine; 
namespace Board {
  public abstract class TileBehavior : MonoBehaviour {
    public bool Verbose {
      get; protected set;
    }

    public abstract void Clicked(Tile tile, int button);

    // Behavior when the tile is left-clicked
    public abstract void LeftClicked(Tile tile);

    // Behavior when the tile is right-clicked
    public abstract void RightClicked(Tile tile);

    // Behavior when this tile is selected
    public abstract void Selected(Tile tile, int flag);

    // Behavior when this tile is deselected
    public abstract void Deselected(Tile tile, int flag);

    // Behavior applied to all tiles when any tile is unselected
    public abstract void ClearSelected(Tile tile, int flag);

    // Behavior applied when a tile that was default search is cleared
    public abstract void ClearSearched(Tile tile, int flag);
  }
}