using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Board
{
  public abstract class BoardBehaviour : MonoBehaviour
  {
    public bool Verbose {
      get; protected set;
    }

    public abstract void Clicked(BoardNode node, int button);

    // Behavior when this tile is selected
    public abstract void Selected(BoardNode node, int flag);

    // Behavior when this tile is deselected
    public abstract void Deselected(BoardNode node, int flag);

    // Behavior applied to all tiles when any tile is unselected
    public abstract void ClearSelected(BoardNode node, int flag);
  }
}