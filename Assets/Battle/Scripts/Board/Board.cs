using UnityEngine;
using System.Collections.Generic;
using System;

namespace Board
{
  public abstract class Board : MonoBehaviour
  {

    [SerializeField]
    protected BoardBehaviour BoardBehaviour;
    [SerializeField]
    protected UnitManager unitManager;

    public BoardNode NodeHovered { get; protected set; } //The BoardNode the mouse is currently on
    public List<BoardNode> NodesSelected { get; protected set; }  //The BoardNode(s) most recently clicked
    public List<BoardNode> Nodes { get; protected set; }

    // Use this for initialization as a subclass
    void Start() {
      CreateBoard();
      //unitManager = Instantiate(unitManager);
      //unitManager.Initialize(this);
      NodesSelected = new List<BoardNode>();
    }

    // Update is called once per frame
    void Update() {
      for (int button = 0; button < 5; button++)
        if (Input.GetMouseButtonDown(button))
          if (NodeHovered != null)
            BoardBehaviour.Clicked(NodeHovered, button);
    }

    // Creates a board of hexagons with 'rows', 'cols' columns, and nodes 'nodeLength' apart
    // Note that nodeLength also gives the vertical distance between nodes
    protected abstract void CreateBoard();

    public void SetNodeHovered(BoardNode n) {
      NodeHovered = n;
    }

    public void RemoveNodeHovered(BoardNode n) {
      if (NodeHovered == n)
        NodeHovered = null;
    }

    public void Deselect(int flag) {
      foreach (BoardNode n in NodesSelected)
        n.Deselect(flag);
      NodesSelected.Clear();
      foreach (BoardNode n in Nodes)
        n.ClearSelected(flag);
    }

    public void AddSelected(BoardNode BoardNode) {
      NodesSelected.Add(BoardNode);
    }

    //public int ComputeCost(ICollection<BoardNode> nodes) {
//
  //  }

    public int ComputeCost(ICollection<BoardNode> nodes, Func<BoardEdge, BoardNode, int> costMap) {
      int cost = 0;
      BoardNode prev = null;
      foreach (BoardNode n in nodes) {
        if (prev == null)
          prev = n; //skip the cost of the start of the path (cause we start there!)
        else {
          cost += costMap(prev.GetEdge(n), n);
          prev = n;
        }
      }
      return cost;
    }

    // Computes and returns the distance between the start and end board elements
    public abstract int DistanceBetween(IBoardElement start, IBoardElement end);

    // Given a starting BoardNode, returns all nodes that can be reached with 'distance' movement
    // Optional Arguments
    // 'costMap' is a function indicating the cost of moving through a board element (default the serializable cost of that element)
    // 'allowable' is a function indicating whether to ignore given board elements when searching (default everything is allowed)
    // 'heuristic' is a function indicating a preference for certain board elements (default heuristic is 0 for all elements
    public List<Tuple<BoardNode, int>> Explore(BoardNode start, int distance) {
      return Explore(start, distance, (e, n) => e.Cost + n.Cost);
    }
    public List<Tuple<BoardNode, int>> Explore(BoardNode start, int distance, Func<BoardEdge, BoardNode, int> costMap) {
      return Explore(start, distance, costMap, (n, e, p) => true);
    }
    public List<Tuple<BoardNode, int>> Explore(BoardNode start, int distance, Func<BoardNode, bool> allowable) {
      return Explore(start, distance, (n, e, p) => allowable(n));
    }
    public List<Tuple<BoardNode, int>> Explore(BoardNode start, int distance, Func<BoardEdge, bool> allowable) {
      return Explore(start, distance, (n, e, p) => allowable(e));
    }
    public List<Tuple<BoardNode, int>> Explore(BoardNode start, int distance, Func<BoardNode, BoardEdge, ICollection<BoardNode>, bool> allowable) {
      return Explore(start, distance, (e, n) => e.Cost + n.Cost, allowable);
    }
    public List<Tuple<BoardNode, int>> Explore(BoardNode start, int distance, 
      Func<BoardEdge, BoardNode, int> costMap, Func<BoardNode, bool> allowable) {
      return Search(start, l => false, costMap, (n, e, p) => allowable(n) && ComputeCost(p, costMap) <= distance, n => 0);
    }
    public List<Tuple<BoardNode, int>> Explore(BoardNode start, int distance, 
      Func<BoardEdge, BoardNode, int> costMap, Func<BoardEdge, bool> allowable) {
      return Search(start, l => false, costMap, (n, e, p) => allowable(e) && ComputeCost(p, costMap) <= distance, n => 0);
    }
    public List<Tuple<BoardNode, int>> Explore(BoardNode start, int distance, Func<BoardEdge, BoardNode, int> costMap,
      Func<BoardNode, BoardEdge, ICollection<BoardNode>, bool> allowable) {
      return Search(start, l => false, costMap, (n, e, p) => allowable(n, e, p) && ComputeCost(p, costMap) <= distance, n => 0);
    }
    public List<Tuple<BoardNode, int>> Explore(BoardNode start, int distance, Func<BoardEdge, BoardNode, int> costMap, 
      Func<BoardNode, bool> allowable, Func<IBoardElement, int> heuristic) {
      return Search(start, l => false, costMap, (n, e, p) => allowable(n) && ComputeCost(p, costMap) <= distance, heuristic);
    }
    public List<Tuple<BoardNode, int>> Explore(BoardNode start, int distance, Func<BoardEdge, BoardNode, int> costMap, 
      Func<BoardEdge, bool> allowable, Func<IBoardElement, int> heuristic) {
      return Search(start, l => false, costMap, (n, e, p) => allowable(e) && ComputeCost(p, costMap) <= distance, heuristic);
    }
    public List<Tuple<BoardNode, int>> Explore(BoardNode start, int distance, Func<BoardEdge, BoardNode, int> costMap, 
      Func<BoardNode, BoardEdge, ICollection<BoardNode>, bool> allowable, Func<IBoardElement, int> heuristic) {
      return Search(start, l => false, costMap, (n, e, p) => allowable(n, e, p) && ComputeCost(p, costMap) <= distance, heuristic);
    }

    // Given a BoardNode and a destination, returns the lowest cost path from the start BoardNode to the destination
    public List<Tuple<BoardNode, int>> Search(BoardNode start, BoardNode destination) {
      return Search(start, destination, (e, n) => e.Cost + n.Cost);
    }
    public List<Tuple<BoardNode, int>> Search(BoardNode start, BoardNode destination, Func<BoardEdge, BoardNode, int> costMap) {
      return Search(start, destination, costMap, (n, e, p) => true);
    }
    public List<Tuple<BoardNode, int>> Search(BoardNode start, BoardNode destination, Func<BoardNode, bool> allowable) {
      return Search(start, destination, (n, e, p) => allowable(n));
    }
    public List<Tuple<BoardNode, int>> Search(BoardNode start, BoardNode destination, Func<BoardEdge, bool> allowable) {
      return Search(start, destination, (n, e, p) => allowable(e));
    }
    public List<Tuple<BoardNode, int>> Search(BoardNode start, BoardNode destination, 
      Func<BoardNode, BoardEdge, ICollection<BoardNode>, bool> allowable) {
      return Search(start, destination, (n, e) => n.Cost + e.Cost, allowable);
    }
    public List<Tuple<BoardNode, int>> Search(BoardNode start, BoardNode destination,
      Func<BoardEdge, BoardNode, int> costMap, Func<BoardNode, bool> allowable) {
      return Search(start, destination, costMap, (n, e, p) => allowable(n));
    }
    public List<Tuple<BoardNode, int>> Search(BoardNode start, BoardNode destination,
      Func<BoardEdge, BoardNode, int> costMap, Func<BoardEdge, bool> allowable) {
      return Search(start, destination, costMap, (n, e, p) => allowable(e));
    }
    public List<Tuple<BoardNode, int>> Search(BoardNode start, BoardNode destination, Func<BoardEdge, BoardNode, int> costMap,
      Func<BoardNode, BoardEdge, ICollection<BoardNode>, bool> allowable) {
      return Search(start, l => l.Contains(destination), costMap, allowable, n => n is BoardNode ? DistanceBetween((BoardNode)n, destination) : 0);
    }
    public List<Tuple<BoardNode, int>> Search(BoardNode start, BoardNode destination, Func<BoardEdge, BoardNode, int> costMap,
      Func<BoardNode, BoardEdge, ICollection<BoardNode>, bool> allowable, Func<IBoardElement, int> heuristic) {
      return Search(start, l => l.Contains(destination), costMap, allowable, heuristic);
    }

    /*
     * Provides a move path for all nodes from the start BoardNode until no nodes are available
     *   OR the next BoardNode fulfills the endNode condition OR the endCondition is fulfilled
     * The includeLast flag indicates whether or not to include the BoardNode fulfilling the endNode condition
     * The flag indicates what sort of Behaviour each BoardNode will observe when explored
     * The heuristic allows certain paths to be favored when exploring
     * The edgeCost indicates edges that have an additional cost
     * Allowable indicates whether or not certain nodes should be excluded from the calculation
     * The endNode function causes this function to immediately return when a BoardNode with the given property is found
     * The end condition causes this function to immediately return when the found nodes fulfill the given property
     */
    public List<Tuple<BoardNode, int>> Search(BoardNode start, Func<List<BoardNode>, bool> endCondition, Func<BoardEdge, BoardNode, int> costMap,
      Func<BoardNode, BoardEdge, ICollection<BoardNode>, bool> allowable, Func<IBoardElement, int> heuristic) {
      PriorityQueue<BoardNode> queue = new PriorityQueue<BoardNode>();
      Dictionary<BoardNode, int> pathCosts = new Dictionary<BoardNode, int>();
      Dictionary<BoardNode, ICollection<BoardNode>> paths = new Dictionary<BoardNode, ICollection<BoardNode>>();
      List<BoardNode> foundNodes = new List<BoardNode>();
      List<Tuple<BoardNode, int>> toReturn = new List<Tuple<BoardNode, int>>();
      foreach (BoardEdge e in start.GetEdges()) {
        BoardNode n = e.GetOther(start);
        List<BoardNode> newPath = new List<BoardNode>();
        newPath.Add(start);
        newPath.Add(n);
        if (allowable(n, start.GetEdge(n), newPath)) {
          paths[n] = newPath;
          pathCosts[n] = ComputeCost(paths[n], costMap);
          queue.Insert(n, -(pathCosts[n] + heuristic(n) + heuristic(start.GetEdge(n))), true);
        }
      }

      while (queue.Count > 0) {
        BoardNode next = queue.Dequeue();
        foundNodes.Add(next);
        toReturn.Add(new Tuple<BoardNode, int> (next, pathCosts[next]));

        if (endCondition(foundNodes))
          return toReturn;
        foreach (BoardEdge e in next.GetEdges()) {
          BoardNode n = e.GetOther(next);
          if (foundNodes.Contains(n))
            continue;
          List<BoardNode> newPath = new List<BoardNode>(paths[next]);
          newPath.Add(n);
          if (!allowable(n, next.GetEdge(n), newPath))
            continue;
          int newPathCost = pathCosts[next] + next.GetEdge(n).Cost + n.Cost;
          if (queue.Insert(n, -(newPathCost + heuristic(n) + heuristic(next.GetEdge(n))), true))
            paths[n] = newPath;
          pathCosts[n] = newPathCost;
        }
      }
      return toReturn;
    }
  }
}
 