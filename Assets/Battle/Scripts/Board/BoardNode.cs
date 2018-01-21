using UnityEngine;
using System;
using System.Collections.Generic;

namespace Board
{
  public class BoardNode : MonoBehaviour, IBoardElement
  {
    protected List<Unit> unitsContained;
    protected List<int> teamsContained;
    

    [SerializeField]
    protected int cost;
    [SerializeField]
    protected int damage;
    [SerializeField]
    protected int opacity;

    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    protected int softDist;
    protected Tile attackPosition;
    protected List<float> appliedShades;

    protected int pathCost;

    public Board board { get; private set; }

    public int Cost { get { return cost < 0 ? 0 : cost; } }
    public int Damage { get { return damage; } }
    public int Opacity { get { return opacity; } }

    private ICollection<Tile> movePath;
    public ICollection<Tile> MovePath {
      get {
        return movePath;
      }
      set {
        if (movePath.Count < 2)
          return;
        pathCost = Board.ComputeCost(movePath);
      }
    }

    public int PathCost { get; protected set; }
    public int Row { get; private set; }
    public int Column { get; private set; }

    //A list of tiles adjacent to this tile
    private List<Tile> adjacentTiles;
    private Dictionary<Tile, Edge> edges;
    private Dictionary<LineSegment, Edge> geometricMap;

    public BoardNode() {
      unitsContained = new List<Unit>();
      teamsContained = new List<int>();
      adjacentTiles = new List<Tile>();
      edges = new Dictionary<Tile, Edge>();
      movePath = new List<Tile>();
    }

    // Use this for initialization
    void Start() {
      spriteRenderer = GetComponent<SpriteRenderer>();
      originalColor = spriteRenderer.color;
      appliedShades = new List<float>();
    }

    // Update is called once per frame
    void Update() {

    }

    public override string ToString() {
      return "(" + Row + ", " + Column + ")";
    }

    // Creates a tile which can have up to 'adjacentCount' tiles adjacent
    public void Initialize(Board board, int row, int col) {
      this.board = board;
      Row = row;
      Column = col;
    }

    public void Clicked(int button) {
      switch (button) {
        case 0:
          TileBehavior.LeftClicked(this);
          break;
        case 1:
          TileBehavior.RightClicked(this);
          break;
      }
    }

    public List<Edge> GetEdges() {
      List<Edge> toReturn = new List<Edge>();
      foreach (Tile t in adjacentTiles)
        toReturn.Add(edges[t]);
      return toReturn;
    }

    public Edge GetEdge(Tile t) {
      if (!edges.ContainsKey(t))
        throw new ArgumentException("No edge between this tile and target");
      return edges[t];
    }

    // Add an edge and associated tile
    // An optional line segment allows specification on the direction of the new tile
    // If this is not provided, the edge will be assumed to be in the geometric direction of the new tile
    public void AddEdge(Tile toAdd, Edge edge) {
      LineSegment checkLine = new LineSegment(Shape.Center, toAdd.Shape.Center);
      foreach (LineSegment ls in Shape.LineSegments)
        if (ls.Intersection(checkLine) != null) {
          AddEdge(toAdd, edge, ls);
          return;
        }
      AddEdge(toAdd, edge, null);
    }
    public void AddEdge(Tile toAdd, Edge edge, LineSegment line) {
      AddEdge(toAdd, edge, line, true);
    }
    private void AddEdge(Tile toAdd, Edge edge, LineSegment line, bool willDuplicate) {
      if (TileBehavior.VERBOSE)
        Debug.Log("Warning: tiles " + ToString() + " and " + toAdd.ToString() + " were not given a geometric edge");
      adjacentTiles.Add(toAdd);
      edges[toAdd] = edge;
      if (willDuplicate) {
        edge.Initialize(new Tuple<Tile, Tile>(this, toAdd), line);
        toAdd.AddEdge(this, edge, line, false);
      }
    }

    //Replaces the given tile to the list of adjacent tiles
    public void SetAdjacent(Tile toRemove, Tile toAdd) {
      for (int i = 0; i < adjacentTiles.Count; i++)
        if (adjacentTiles[i] == toRemove)
          adjacentTiles[i] = toAdd;
    }

    //Replace this tile in each adjacent tile
    public void ReplaceTile(Tile replacement) {
      foreach (Tile t in adjacentTiles)
        t.SetAdjacent(this, replacement);
    }

    //Shades this tile to a given factor
    public void Shade(float factor) {
      Shade(factor, true);
    }

    private void Shade(float factor, bool addShade) {
      Color color = spriteRenderer.color;
      spriteRenderer.color = new Color(color.r * factor, color.g * factor, color.b * factor);
      appliedShades.Add(factor);
    }

    // Removes all shade
    public void RemoveShade() {
      spriteRenderer.color = originalColor;
      appliedShades = new List<float>();
    }

    // Removes a specific shade (if it has applied this shade)
    public void RemoveShade(float shade) {
      spriteRenderer.color = originalColor;
      appliedShades.Remove(shade);
      foreach (float s in appliedShades)
        Shade(shade, false);
    }

    // Returns the list of adjacent tiles
    public ICollection<Tile> GetAdjacent() {
      return adjacentTiles;
    }
    
    public void ClearPath() {
      MovePath.Clear();
      PathCost = int.MaxValue; //Unexplored
    }

    public void SetPath(ICollection<Tile> path) {
      MovePath.Clear();
      foreach (Tile item in path)
        MovePath.Add(item);
      MovePath.Add(this);
    }

    protected void OnMouseEnter() {
      board.SetTileHovered(this);
    }

    protected void OnMouseExit() {
      board.RemoveTileHovered(this);
    }

    public void Deselect() {
      Deselect(0);
    }

    public void Deselect(int flag) {
      TileBehavior.Deselected(this, flag);
    }

    public void ClearSelected() {
      ClearSelected(0);
    }

    public void ClearSelected(int flag) {
      TileBehavior.ClearSelected(this, flag);
    }

    public void Select() {
      Select(0);
    }

    public void Select(int flag) {
      TileBehavior.Selected(this, flag);
    }
  }
}