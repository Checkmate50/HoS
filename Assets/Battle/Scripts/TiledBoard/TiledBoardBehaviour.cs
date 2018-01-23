using System;
using System.Collections.Generic;
using UnityEngine; 
namespace Board {
  public abstract class TiledBoardBehaviour : BoardBehaviour {

    public override void Clicked(BoardNode node, int button) {
      Clicked(NodeToTile(node), button);
    }
    public abstract void Clicked(Tile tile, int button);

    // Behavior when this tile is selected
    public override void Selected(BoardNode node, int flag) {
      Selected(NodeToTile(node), flag);
    }
    public abstract void Selected(Tile tile, int flag);

    // Behavior when this tile is deselected
    public override void Deselected(BoardNode node, int flag) {
      Deselected(NodeToTile(node), flag);
    }
    public abstract void Deselected(Tile tile, int flag);

    // Behavior applied to all tiles when any tile is unselected
    public override void ClearSelected(BoardNode node, int flag) {
      ClearSelected(NodeToTile(node), flag);
    }
    public abstract void ClearSelected(Tile tile, int flag);

    private Tile NodeToTile(BoardNode node) {
      if (node is Tile)
        return (Tile)node;
      throw new ArgumentException("Cannot interpret a non-tile node for a tiled behavior");
    }
  }
}