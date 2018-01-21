using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
  public class HexBoard : Board
  {
    [SerializeField]
    protected Tile empty;
    [SerializeField]
    protected Tile rough;
    [SerializeField]
    protected Edge edge;
    [SerializeField]
    protected Edge wall;

    protected override void CreateBoard() {
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
          currentTile.Initialize(this, new RegularHexagon(), row, col);
          Grid[row, col] = currentTile;
          Tiles.Add(currentTile);
          Edge toInstantiate;
          if (myRand.Next(0, 100) > 80)
            toInstantiate = wall;
          else
            toInstantiate = edge;
          Vector2 edgePos;
          Edge newEdge;
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
  }
}
