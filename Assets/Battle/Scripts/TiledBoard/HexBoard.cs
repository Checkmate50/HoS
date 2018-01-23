using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Board
{
  public class HexBoard : TiledBoard
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
      if (!(start is TileEdge || start is Tile) || !(end is TileEdge || end is Tile))
        throw new ArgumentException("This method does not support board elements beyond edges and tiles");
      if (start is TileEdge) {
        Tuple<Tile, Tile> startTiles = ((TileEdge)start).Tiles;
        return Math.Min(DistanceBetween(startTiles.First, end), DistanceBetween(startTiles.Second, end));
      }
      if (end is TileEdge) {
        Tuple<Tile, Tile> endTiles = ((TileEdge)end).Tiles;
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

    protected override void CreateBoard() {
      Nodes = new List<BoardNode>();
      Tiles = new List<Tile>();
      Grid = new Tile[rows, columns];
      float totalLength = tileLength * (columns - 1);
      float totalHeight = tileLength * (rows - 1);
      System.Random myRand = new System.Random();

      for (int row = 0; row < rows; row++) {
        for (int col = 0; col < columns; col++) {
          //Note that the x becomes more positive while the y becomes more negative as j/i increase
          Vector2 pos = new Vector2((col * tileLength) - (totalLength / 2) - (tileLength / 4), (totalHeight / 2) - (row * tileLength));
          if (row % 2 == 1)
            pos.x += tileLength / 2;
          Tile currentTile;
          if (myRand.Next(0, 100) > 70)
            currentTile = Instantiate(rough, pos, Quaternion.identity);
          else
            currentTile = Instantiate(empty, pos, Quaternion.identity);
          currentTile.Initialize(this, BoardBehaviour, new RegularHexagon(new Point(pos.x, pos.y)), row, col);
          Grid[row, col] = currentTile;
          Nodes.Add(currentTile);
          Tiles.Add(currentTile);
          TileEdge toInstantiate;
          if (myRand.Next(0, 100) > 80)
            toInstantiate = wall;
          else
            toInstantiate = edge;
          Vector2 edgePos;
          TileEdge newEdge;
          if (row > 0) {
            // Upper-left
            if (row % 2 == 1 || col > 0) {
              edgePos = new Vector2(pos.x - (tileLength / 4), pos.y + (tileLength / 2));
              newEdge = Instantiate(toInstantiate, edgePos, Quaternion.Euler(0, 0, 30));
              Grid[row, col].AddEdge(Grid[row - 1, col - (1 - row % 2)], newEdge);
            }
            // Upper-right
            if (row % 2 == 0 || col < columns - 1) {
              edgePos = new Vector2(pos.x + (tileLength / 4), pos.y + (tileLength / 2));
              newEdge = Instantiate(toInstantiate, edgePos, Quaternion.Euler(0, 0, -30));
              Grid[row, col].AddEdge(Grid[row - 1, col + 1 - (1 - row % 2)], newEdge);
            }
          }
          if (col > 0) {
            // Left
            edgePos = new Vector2(pos.x - (tileLength / 2), pos.y);
            newEdge = Instantiate(toInstantiate, edgePos, Quaternion.Euler(0, 0, 90));
            Grid[row, col].AddEdge(Grid[row, col - 1], newEdge);
          }
        }
      }
    }

    public override Tile GetTileAtPoint(Point point) {
      throw new NotImplementedException();
    }
  }
}
