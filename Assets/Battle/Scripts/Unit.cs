using UnityEngine;

namespace Board
{
  public class Unit : MonoBehaviour
  {
    [SerializeField]
    private int actionPoints;
    [SerializeField]
    private int initiative;
    [SerializeField]
    private int maxHealth;
    [SerializeField]
    private int range;
    [SerializeField]
    private int minDamage;
    [SerializeField]
    private int maxDamage;
    /*
    private int stackSize;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool noSelect;
    private bool myTurn;
    private Tile currentSpace;
    private Board board;
    private UnitManager unitManager;

    // Use this for initialization
    void Awake() {
      myTurn = false;
      spriteRenderer = GetComponent<SpriteRenderer>();
      originalColor = spriteRenderer.color;
    }

    // Update is called once per frame
    void Update() {

    }

    public void Initialize(int stackSize, Tile startSpace, Board board, UnitManager unitManager) {
      currentSpace = startSpace;
      this.board = board;
      this.unitManager = unitManager;
    }

    public int getactionPoints() {
      return actionPoints;
    }

    public int getInitiative() {
      return initiative;
    }

    public void startTurn() {
      board.select(currentSpace, actionPoints);
      Color temp = spriteRenderer.color;
      spriteRenderer.color = new Color(temp.r * 1.5f, temp.g * 1.5f, temp.b * 1.5f);
      myTurn = true;
    }

    public void move(Tile[] path) {
      Tile newSpace = path[path.Length - 1];
      transform.position = newSpace.transform.position;
      currentSpace.unitContained = null;
      newSpace.unitContained = this;
      currentSpace = newSpace;
      spriteRenderer.color = originalColor;
      myTurn = false;
      noSelect = true;
    }

    void OnMouseEnter() {
      if (noSelect) {
        noSelect = false;
        return;
      }
      if (!myTurn)
        board.select(currentSpace, actionPoints);
    }

    void OnMouseExit() {
      if (myTurn)
        return;
      board.deselect();
    }*/
  }
}