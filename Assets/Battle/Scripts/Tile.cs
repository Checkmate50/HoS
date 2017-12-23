using UnityEngine;
using System;
using System.Collections.Generic;

namespace battle
{
  public class Tile : MonoBehaviour, IBoardElement
  {
    protected List<Unit> unitsContained;
    protected List<int> teamsContained;

    [SerializeField]
    protected int moveCost;
    [SerializeField]
    protected int damage;

    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    protected int softDist;
    protected Tile attackPosition;
    protected List<float> appliedShades;

    protected int pathCost;

    public Board board { get; private set; }

    public int Cost {
      get {
        return moveCost < 0 ? 0 : moveCost;
      }
    }

    public int Damage {
      get {
        return damage;
      }
    }

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

    public Tile() {
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
    }

    // Update is called once per frame
    void Update() {

    }

    public override string ToString() {
      return "(" + transform.position.x + ", " + transform.position.y + ")";
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

    public Edge GetEdge(Tile t) {
      if (!edges.ContainsKey(t))
        throw new ArgumentException("No edge between this tile and target");
      return edges[t];
    }

    public void AddEdge(Tile toAdd, Edge edge) {
      AddEdge(toAdd, edge, 0, true);
    }
    public void AddEdge(Tile toAdd, Edge edge, int cost) {
      AddEdge(toAdd, edge, cost, true);
    }
    public void AddEdge(Tile toAdd, Edge edge, int cost, bool willDuplicate) {
      adjacentTiles.Add(toAdd);
      edges[toAdd] = edge;
      if (willDuplicate) {
        edge.Initialize(new Tuple<Tile, Tile>(this, toAdd), cost);
        toAdd.AddEdge(this, edge, cost, false);
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

    //Shades this tile by a given factor
    public void Shade(float factor) {
      Shade(factor, true);
    }

    private void Shade(float factor, bool addShade) {
      Color color = spriteRenderer.color;
      spriteRenderer.color = new Color(color.r * factor, color.g * factor, color.b * factor);
      appliedShades.Add(factor);
    }

    public void RemoveShade(float shade) {
      spriteRenderer.color = originalColor;
      appliedShades.Remove(shade);
      foreach (float s in appliedShades)
        Shade(shade, false);
    }

    //Removes shade
    public void ResetShade() {
      spriteRenderer.color = originalColor;
      appliedShades = new List<float>();
    }

    //Returns the list of adjacent tiles
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

    public void Search(int flag) {
      switch (flag) {
        case 1:
          TileBehavior.SearchSelected(this);
          break;
        default:
          TileBehavior.Search(this);
          break;
      }  
    }

    protected void OnMouseEnter() {
      board.SetTileHovered(this);
    }

    protected void OnMouseExit() {
      board.RemoveTileHovered(this);
    }

    protected void Select() {
      board.AddSelected(this);
    }
  }
}