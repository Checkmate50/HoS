using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace battle {
  public class Edge : MonoBehaviour, IBoardElement {

    public int Cost { get; protected set; }
    public int Damage { get; protected set; }
    public Tuple<Tile, Tile> Tiles { get; private set; }

    public int Row { get; private set; }

    public int Column { get; private set; }

    // Use this for initialization
    void Start() { }

    public void Initialize(Tuple<Tile, Tile> adjTiles, int cost) {
      Cost = cost;
      Tiles = adjTiles;
      Row = adjTiles.Second.Row;
      Column = adjTiles.Second.Column;
    }
  }
}
