using UnityEngine;
using System.Collections;

namespace battle
{
    public class Unit : MonoBehaviour
    {
        [SerializeField]
        private int movement;
        [SerializeField]
        private int initiative;

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
            spriteRenderer = this.GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
        }

        // Update is called once per frame
        void Update() {

        }

        public void Initialize(Tile startSpace, Board board, UnitManager unitManager) {
            this.currentSpace = startSpace;
            this.board = board;
            this.unitManager = unitManager;
        }

        public int getMovement() {
            return movement;
        }

        public int getInitiative() {
            return initiative;
        }

        public void startTurn() {
            board.shade(currentSpace, movement, false);
            Color temp = spriteRenderer.color;
            this.spriteRenderer.color = new Color(temp.r * 1.5f, temp.g * 1.5f, temp.b * 1.5f);
            myTurn = true;
        }

        public void move(Tile[] path) {
            Tile newSpace = path[path.Length - 1];
            this.transform.position = newSpace.transform.position;
            currentSpace.unitContained = null;
            newSpace.unitContained = this;
            currentSpace = newSpace;
            this.spriteRenderer.color = originalColor;
            myTurn = false;
            noSelect = true;
        }

        void OnMouseEnter() {
            if (noSelect) {
                noSelect = false;
                return;
            }
            if (!myTurn)
                board.shade(currentSpace, movement, true);
        }

        void OnMouseExit() {
            if (myTurn)
                return;
            board.removeShade(true);
        }
    }
}