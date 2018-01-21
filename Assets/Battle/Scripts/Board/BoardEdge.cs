using UnityEngine;

namespace Board {
  public class Edge : MonoBehaviour, IBoardElement {

    [SerializeField]
    protected int cost;
    [SerializeField]
    protected int opacity;

    public int Cost { get { return cost; } }
    public int Damage { get { return damage; } }
    public int Opacity { get { return opacity; } }
    public Tuple<Tile, Tile> Tiles { get; private set; }
    public LineSegment Line { get; private set; }

    // Use this for initialization
    void Start() { }

    public void Initialize(Tuple<Tile, Tile> adjTiles, LineSegment line) {
      Tiles = adjTiles;
      Line = line;
    }

    public override string ToString() {
      return Tiles.ToString();
    }
  }
}
