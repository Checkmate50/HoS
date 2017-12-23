using UnityEngine; 
namespace battle {
  public static class TileBehavior {
    // Behavior when the tile is left-clicked
    public static void LeftClicked(Tile tile) {
      Debug.Log("clicked: " + tile);
      tile.board.AddSelected(tile);
      Board.Explore(tile, 5);
    }

    // Behavior when the tile is right-clicked
    public static void RightClicked(Tile tile) {
      tile.Shade(0.7f);
    }

    // Behavior when the tile is searched by the selected tile
    public static void SearchSelected(Tile tile) {
      tile.Shade(0.5f);
    }

    // Behavior when the tile is searched with no preconditions (default behavior)
    public static void Search(Tile tile) {
      tile.Shade(0.3f);
    }

    // Behavior when this tile is deselected
    public static void Deselect(Tile tile) {
      ;
    }

    // Behavior applied to all tiles when any tile is unselected
    public static void ClearSelected(Tile tile) {
      tile.ResetShade();
    }

    // Behavior applied when a tile that was default search is cleared
    public static void ClearSearch(Tile tile) {
      tile.RemoveShade(0.3f);
    }
  }
}