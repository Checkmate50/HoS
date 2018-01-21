using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Board
{
  public abstract class Board : MonoBehaviour
  {

    [SerializeField]
    protected TileBehavior tileBehavior;
    [SerializeField]
    protected UnitManager unitManager;

    public BoardNode NodeHovered { get; protected set; } //The BoardNode the mouse is currently on
    public List<BoardNode> NodesSelected { get; protected set; }  //The BoardNode(s) most recently clicked
    public List<BoardNode> Nodes { get; protected set; }

    // Use this for initialization
    void Start() {
      CreateBoard();
      unitManager = Instantiate(unitManager);
      unitManager.Initialize(this);
      NodesSelected = new List<BoardNode>();
    }

    // Update is called once per frame
    void Update() {
      for (int button = 0; button < 5; button++)
        if (Input.GetMouseButtonDown(button))
          if (NodeHovered != null)
            tileBehavior.Clicked(NodeHovered, button);
    }

    // Creates a board of hexagons with 'rows', 'cols' columns, and tiles 'tileLength' apart
    // Note that tileLength also gives the vertical distance between tiles
    protected abstract void CreateBoard();

    public void SetTileHovered(BoardNode t) {
      NodeHovered = t;
    }

    public void RemoveTileHovered(BoardNode t) {
      if (NodeHovered == t)
        NodeHovered = null;
    }

    public void Deselect(int flag) {
      foreach (BoardNode t in NodesSelected)
        t.Deselect(flag);
      NodesSelected.Clear();
      foreach (BoardNode t in Nodes)
        t.ClearSelected(flag);
    }

    public void AddSelected(BoardNode BoardNode) {
      NodesSelected.Add(BoardNode);
    }

    public static int ComputeCost(ICollection<BoardNode> tiles) {
      int cost = 0;
      BoardNode prev = null;
      foreach (BoardNode t in tiles) {
        if (prev == null)
          prev = t; //skip the cost of the start of the path (cause we start there!)
        else {
          cost += t.Cost + prev.GetEdge(t).Cost;
          prev = t;
        }
      }
      return cost;
    }

    // Given a starting BoardNode, returns all tiles that can be reached with 'distance' movement
    // Optional Arguments
    // 'allowable' is a function indicating whether to ignore given board elements when searching
    // 'heuristic' is a function indicating a preference for certain board elements
    // 
    public static List<BoardNode> Explore(BoardNode start, int distance) {
      return Explore(start, distance);
    }
    public static List<BoardNode> Explore(BoardNode start, int distance, Func<BoardNode, bool> allowable) {
      return Explore(start, distance, (t, e, p) => allowable(t));
    }
    public static List<BoardNode> Explore(BoardNode start, int distance, Func<Edge, bool> allowable) {
      return Explore(start, distance, (t, e, p) => allowable(e));
    }
    public static List<BoardNode> Explore(BoardNode start, int distance, Func<BoardNode, Edge, ICollection<BoardNode>, bool> allowable) {
      return Explore(start, distance, allowable, t => 0);
    }
    public static List<BoardNode> Explore(BoardNode start, int distance, Func<IBoardElement, int> heuristic) {
      return Search(start, l => false, (t, e, p) => ComputeCost(p) <= distance, heuristic);
    }
    public static List<BoardNode> Explore(BoardNode start, int distance, Func<BoardNode, bool> allowable, Func<IBoardElement, int> heuristic) {
      return Search(start, l => false, (t, e, p) => allowable(t) && ComputeCost(p) <= distance, heuristic);
    }
    public static List<BoardNode> Explore(BoardNode start, int distance, Func<Edge, bool> allowable, Func<IBoardElement, int> heuristic) {
      return Search(start, l => false, (t, e, p) => allowable(e) && ComputeCost(p) <= distance, heuristic);
    }
    public static List<BoardNode> Explore(BoardNode start, int distance,
  Func<BoardNode, Edge, ICollection<BoardNode>, bool> allowable, Func<IBoardElement, int> heuristic) {
      return Search(start, l => false, (t, e, p) => allowable(t, e, p) && ComputeCost(p) <= distance, heuristic);
    }

    // Given a BoardNode and a destination, searches for the shortest path from the start BoardNode to the destination
    public static List<BoardNode> Search(BoardNode start, BoardNode destination) {
      return Search(start, destination);
    }
    public static List<BoardNode> Search(BoardNode start, BoardNode destination, Func<BoardNode, bool> allowable) {
      return Search(start, destination, (t, e, p) => allowable(t));
    }
    public static List<BoardNode> Search(BoardNode start, BoardNode destination, Func<Edge, bool> allowable) {
      return Search(start, destination, (t, e, p) => allowable(e));
    }
    public static List<BoardNode> Search(BoardNode start, BoardNode destination, Func<BoardNode, Edge, ICollection<BoardNode>, bool> allowable) {
      return Search(start, l => l.Contains(destination), allowable, t => t is BoardNode ? DistanceBetween((BoardNode)t, destination) : 0);
    }
    public static List<BoardNode> Search(BoardNode start, BoardNode destination, Func<IBoardElement, int> heuristic) {
      return Search(start, destination, (t, e, p) => true, heuristic);
    }
    public static List<BoardNode> Search(BoardNode start, BoardNode destination,
      Func<BoardNode, Edge, ICollection<BoardNode>, bool> allowable, Func<IBoardElement, int> heuristic) {
      return Search(start, l => l.Contains(destination), allowable, heuristic);
    }

    /*
     * Provides a move path for all tiles from the start BoardNode until no tiles are available
     *   OR the next BoardNode fulfills the endTile condition OR the endCondition is fulfilled
     * The includeLast flag indicates whether or not to include the BoardNode fulfilling the endTile condition
     * The flag indicates what sort of behavior each BoardNode will observe when explored
     * The heuristic allows certain paths to be favored when exploring
     * The edgeCost indicates edges that have an additional cost
     * Allowable indicates whether or not certain tiles should be excluded from the calculation
     * The endTile function causes this function to immediately return when a BoardNode with the given property is found
     * The end condition causes this function to immediately return when the found tiles fulfill the given property
     */
    public static List<BoardNode> Search(BoardNode start, Func<List<BoardNode>, bool> endCondition,
      Func<BoardNode, Edge, ICollection<BoardNode>, bool> allowable, Func<IBoardElement, int> heuristic) {
      PriorityQueue<BoardNode> queue = new PriorityQueue<BoardNode>();
      Dictionary<BoardNode, int> pathCosts = new Dictionary<BoardNode, int>();
      Dictionary<BoardNode, ICollection<BoardNode>> paths = new Dictionary<BoardNode, ICollection<BoardNode>>();
      List<BoardNode> foundTiles = new List<BoardNode>();
      foreach (BoardNode t in start.GetAdjacent()) {
        List<BoardNode> newPath = new List<BoardNode>();
        newPath.Add(start);
        newPath.Add(t);
        if (allowable(t, start.GetEdge(t), newPath)) {
          paths[t] = newPath;
          pathCosts[t] = ComputeCost(paths[t]);
          queue.Insert(t, -(pathCosts[t] + heuristic(t) + heuristic(start.GetEdge(t))), true);
        }
      }

      while (queue.Count > 0) {
        BoardNode next = queue.Dequeue();
        next.MovePath = paths[next];
        foundTiles.Add(next);

        if (endCondition(foundTiles))
          return foundTiles;
        foreach (BoardNode t in next.GetAdjacent()) {
          if (t == start || foundTiles.Contains(t))
            continue;
          List<BoardNode> newPath = new List<BoardNode>(paths[next]);
          newPath.Add(t);
          if (!allowable(t, next.GetEdge(t), newPath))
            continue;
          int newPathCost = pathCosts[next] + next.GetEdge(t).Cost + t.Cost;
          if (queue.Insert(t, -(newPathCost + heuristic(t) + heuristic(next.GetEdge(t))), true))
            paths[t] = newPath;
          pathCosts[t] = newPathCost;
        }
      }
      return foundTiles;
    }
  }
}
 