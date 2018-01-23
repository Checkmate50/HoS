using UnityEngine;

namespace Board {
  public class BoardEdge : MonoBehaviour, IBoardElement {

    [SerializeField]
    protected int cost;

    public int Cost { get { return cost; } }
    public Tuple<BoardNode, BoardNode> Nodes { get; private set; }

    public void Initialize(Tuple<BoardNode, BoardNode> adjNodes) {
      Nodes = adjNodes;
    }

    // Given a node, returns the other node this edge is connected to
    public BoardNode GetOther(BoardNode node) {
      if (Nodes.First.Equals(node))
        return Nodes.Second;
      return Nodes.First;
    }

    public override bool Equals(object other) {
      if (!(other is BoardEdge))
        return false;
      return Nodes.Equals(((BoardEdge)other).Nodes);
    }

    public override string ToString() {
      return Nodes.ToString();
    }
  }
}
