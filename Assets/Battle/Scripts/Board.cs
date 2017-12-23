using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace battle
{
  public class Board : MonoBehaviour
  {

    [SerializeField]
    private int rows;
    [SerializeField]
    private int columns;
    [SerializeField]
    private float tileLength;
    [SerializeField]
    private Tile empty;
    [SerializeField]
    private Tile rough;
    [SerializeField]
    private Edge edge;
    [SerializeField]
    private UnitManager unitManager;

    private Tile tileHovered; //The tile the mouse is currently on
    private List<Tile> tilesSelected;  //The tile(s) most recently clicked

    public Tile[,] Grid { get; private set; }
    public List<Tile> Tiles { get; private set; }

    // Use this for initialization
    void Start() {
      CreateBoard();
      unitManager = Instantiate(unitManager);
      unitManager.Initialize(this);
    }

    // Update is called once per frame
    void Update() {
    }

    // Creates a board of hexagons with 'rows', 'cols' columns, and tiles 'tileLength' apart
    // Note that tileLength also gives the vertical distance between tiles
    private void CreateBoard() {
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
          currentTile.Initialize(this, row, col);
          Grid[row, col] = currentTile;
          Tiles.Add(currentTile);

          Vector2 edgePos;
          Edge newEdge;
          if (row > 0) {
            // Upper-left
            if (row % 2 == 1 || col > 0) {
              edgePos = new Vector2(pos.x - (tileLength / 4), pos.y + (tileLength / 2));
              newEdge = Instantiate(edge, edgePos, Quaternion.Euler(0, 0, 30));
              Grid[row, col].AddEdge(Grid[row - 1, col - (1 - row % 2)], edge);
            }
            // Upper-right
            if (row % 2 == 0 || col < columns - 1) {
              edgePos = new Vector2(pos.x + (tileLength / 4), pos.y + (tileLength / 2));
              newEdge = Instantiate(edge, edgePos, Quaternion.Euler(0, 0, -30));
              Grid[row, col].AddEdge(Grid[row - 1, col + 1 - (1 - row % 2)], edge);
            }
          }
          if (col > 0) {
            // Left
            edgePos = new Vector2(pos.x - (tileLength / 2), pos.y);
            newEdge = Instantiate(edge, edgePos, Quaternion.Euler(0, 0, 90));
            Grid[row, col].AddEdge(Grid[row, col - 1], edge);
          }
        }
      }
    }

    protected void MouseClicked(int button) {
      Debug.Log(button);
      if (tileHovered != null)
        tileHovered.Clicked(button);
    }

    public void SetTileHovered(Tile t) {
      tileHovered = t;
      Debug.Log(tileHovered);
    }

    public void RemoveTileHovered(Tile t) {
      if (tileHovered == t)
        tileHovered = null;
    }

    public void Deselect() {
      foreach (Tile t in tilesSelected)
        TileBehavior.Deselect(t);
      tilesSelected.Clear();
      foreach (Tile t in Tiles)
        TileBehavior.ClearSelected(t);
    }

    public void AddSelected(Tile tile) {
      tilesSelected.Add(tile);
    }

    public static int DistanceBetween(IBoardElement start, IBoardElement end) {
      int toReturn = Math.Abs(start.Row - end.Row) + Math.Abs(start.Column - end.Column);
      if ((start.Row - end.Row) % 2 == 1) {  //Handle row offset case stuff
        if (start.Row % 2 == 1)
          if (start.Column <= end.Column)
            toReturn -= 1;
          else if (end.Column >= start.Column)
            toReturn -= 1;
      }
      return toReturn;
    }

    public static int ComputeCost(ICollection<Tile> tiles) {
      int cost = 0;
      Tile prev = null;
      foreach (Tile t in tiles) {
        if (prev == null)
          prev = t; //skip the cost of the start of the path (cause we start there!)
        else {
          cost += t.Cost + prev.GetEdge(t).Cost;
          prev = t;
        }
      }
      return cost;
    }

    public static List<Tile> Explore(Tile start, int distance) {
      Debug.Log(start);
      return Explore(start, distance, 0);
    }
    public static List<Tile> Explore(Tile start, int distance, int flag) {
      return Explore(start, distance, flag, t => 0);
    }
    public static List<Tile> Explore(Tile start, int distance, int flag, Func<Tile, bool> allowable) {
      return Explore(start, distance, flag, (t, p) => allowable(t));
    }
    public static List<Tile> Explore(Tile start, int distance, int flag, Func<Tile, ICollection<Tile>, bool> allowable) {
      return Explore(start, distance, flag, allowable, t => 0);
    }
    public static List<Tile> Explore(Tile start, int distance, int flag, Func<IBoardElement, int> heuristic) {
      return Search(start, t => false, l => false, flag, (t, p) => ComputeCost(p) < distance, heuristic);
    }
    public static List<Tile> Explore(Tile start, int distance, int flag,
      Func<Tile, bool> allowable, Func<IBoardElement, int> heuristic) {
      return Search(start, t => false, l => false, flag, (t, p) => allowable(t) && ComputeCost(p) < distance, heuristic);
    }
    public static List<Tile> Explore(Tile start, int distance, int flag, 
      Func<Tile, ICollection<Tile>, bool> allowable, Func<IBoardElement, int> heuristic) {
      return Search(start, t => false, l => false, flag, (t, p) => allowable(t, p) && ComputeCost(p) < distance, heuristic);
    }

    // Provides a move path for all tiles from the start tile until no tiles are available
    //   OR the next tile fulfills the endTile condition OR the endCondition is fulfilled
    // The includeLast flag indicates whether or not to include the tile fulfilling the endTile condition
    // The flag indicates what sort of behavior each tile will observe when explored
    // The heuristic allows certain paths to be favored when exploring
    // The edgeCost indicates edges that have an additional cost
    // Allowable indicates whether or not certain tiles should be excluded from the calculation
    // The endTile function causes this function to immediately return when a tile with the given property is found
    // The end condition causes this function to immediately return when the found tiles fulfill the given property
    public static List<Tile> Search(Tile start, Func<Tile, bool> endTile, Func<List<Tile>, bool> endCondition, int flag,
      Func<Tile, ICollection<Tile>, bool> allowable, Func<IBoardElement, int> heuristic) {

      PriorityQueue<Tile> queue = new PriorityQueue<Tile>();
      Dictionary<Tile, int> pathCosts = new Dictionary<Tile, int>();
      Dictionary<Tile, ICollection<Tile>> paths = new Dictionary<Tile, ICollection<Tile>>();
      List<Tile> foundTiles = new List<Tile>();
      foundTiles.Add(start);

      foreach (Tile t in start.GetAdjacent()) {
        List<Tile> newPath = new List<Tile>();
        newPath.Add(start);
        newPath.Add(t);
        if (allowable(t, newPath)) {
          paths[t] = newPath;
          pathCosts[t] = ComputeCost(paths[t]);
          queue.Insert(t, -(pathCosts[t] + heuristic(t)), true);
        }
      }

      while (queue.Count > 0) {
        Tile next = queue.Pop();
        next.MovePath = paths[next];
        next.Search(flag);
        foundTiles.Add(next);

        if (endTile(next) || endCondition(foundTiles))
          return foundTiles;
        foreach (Tile t in next.GetAdjacent()) {
          if (foundTiles.Contains(t))
            continue;
          List<Tile> newPath = new List<Tile>(paths[next]);
          newPath.Add(t);
          if (!allowable(t, newPath))
            continue;
          if (queue.Insert(t, ComputeCost(newPath) + heuristic(t) + heuristic(next.GetEdge(t)), true)) {
            paths[t] = new List<Tile>();
            pathCosts[t] = ComputeCost(paths[t]);
          }
        }
      }
      return foundTiles;
    }

    public static List<Tile> SearchForTile(Tile start, Tile destination) {
      return SearchForTile(start, destination, 0);
    }
    public static List<Tile> SearchForTile(Tile start, Tile destination, int flag) {
      return SearchForTile(start, destination, flag, t => true);
    }
    public static List<Tile> SearchForTile(Tile start, Tile destination, int flag, Func<Tile, bool> allowable) {
      return SearchForTile(start, destination, flag, (t, p) => allowable(t), t => DistanceBetween(t, destination));
    }
    public static List<Tile> SearchForTile(Tile start, Tile destination, int flag, Func<Tile, ICollection<Tile>, bool> allowable) {
      return Search(start, t => t == destination, t => false, flag, allowable, t => DistanceBetween(t, destination));
    }
    public static List<Tile> SearchForTile(Tile start, Tile destination, int flag, Func<IBoardElement, int> heuristic) {
      return SearchForTile(start, destination, flag, (t, p) => true, heuristic);
    }
    public static List<Tile> SearchForTile(Tile start, Tile destination, 
      int flag, Func<Tile, ICollection<Tile>, bool> allowable, Func<IBoardElement, int> heuristic) {
      return Search(start, t => t == destination, l => false, flag, allowable, heuristic);
    }
  }
}