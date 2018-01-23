using UnityEngine;
using System;
using System.Collections.Generic;

namespace Board
{
  public class BoardNode : ModifiableSprite, IBoardElement {
    protected List<Unit> unitsContained;
    protected List<int> teamsContained;

    [SerializeField]
    protected int cost;

    public Board Board { get; private set; }
    public BoardBehaviour Behaviour { get; private set; }
    public int Cost { get { return cost < 0 ? 0 : cost; } }

    public ICollection<BoardNode> MovePath { get; private set; }
    public int PathCost { get; private set; }

    //A list of tiles adjacent to this BoardNode
    public List<BoardNode> AdjacentNodes { get; private set; }
    public Dictionary<BoardNode, BoardEdge> Edges { get; private set; }
    

    public BoardNode() {
      unitsContained = new List<Unit>();
      teamsContained = new List<int>();
      AdjacentNodes = new List<BoardNode>();
      Edges = new Dictionary<BoardNode, BoardEdge>();
    }

    // Update is called once per frame
    void Update() {

    }

    // Creates a BoardNode which can have up to 'adjacentCount' tiles adjacent
    public void Initialize(Board board, BoardBehaviour behaviour) {
      Board = board;
      Behaviour = behaviour;
    }

    public void Clicked(int button) {
      Behaviour.Clicked(this, button);
    }

    public List<BoardEdge> GetEdges() {
      List<BoardEdge> toReturn = new List<BoardEdge>();
      foreach (BoardNode t in AdjacentNodes)
        toReturn.Add(Edges[t]);
      return toReturn;
    }

    public BoardEdge GetEdge(BoardNode t) {
      if (!Edges.ContainsKey(t))
        throw new ArgumentException("No BoardEdge between this BoardNode and target");
      return Edges[t];
    }

    // Add an BoardEdge and associated BoardNode
    public void AddEdge(BoardNode toAdd, BoardEdge BoardEdge) {
      AddEdge(toAdd, BoardEdge, true);
    }
    protected void AddEdge(BoardNode toAdd, BoardEdge BoardEdge, bool willDuplicate) {
      if (Behaviour.Verbose)
        Debug.Log("Warning: tiles " + ToString() + " and " + toAdd.ToString() + " were not given a geometric BoardEdge");
      AdjacentNodes.Add(toAdd);
      Edges[toAdd] = BoardEdge;
      if (willDuplicate) {
        BoardEdge.Initialize(new Tuple<BoardNode, BoardNode>(this, toAdd));
        toAdd.AddEdge(this, BoardEdge, false);
      }
    }

    //Replaces the given BoardNode to the list of adjacent tiles
    public void SetAdjacentNode(BoardNode toRemove, BoardNode toAdd) {
      for (int i = 0; i < AdjacentNodes.Count; i++)
        if (AdjacentNodes[i] == toRemove)
          AdjacentNodes[i] = toAdd;
    }

    //Replace this BoardNode in each adjacent BoardNode
    public void ReplaceNode(BoardNode replacement) {
      foreach (BoardNode t in AdjacentNodes)
        t.SetAdjacentNode(this, replacement);
    }
    
    public void ClearPath() {
      MovePath.Clear();
    }

    public void SetPath(ICollection<BoardNode> path, int pathCost) {
      MovePath = path;
      PathCost = pathCost;
    }

    protected void OnMouseEnter() {
      Board.SetNodeHovered(this);
    }

    protected void OnMouseExit() {
      Board.RemoveNodeHovered(this);
    }

    public void Deselect(int flag) {
      Behaviour.Deselected(this, flag);
    }

    public void ClearSelected(int flag) {
      Behaviour.ClearSelected(this, flag);
    }

    public void Select(int flag) {
      Behaviour.Selected(this, flag);
    }
  }
}