using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace battle
{
    public class Tile : MonoBehaviour
    {

        public Unit unitContained;

        [SerializeField]
        private int cost;
        private Board board;
        private bool selectable;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private int softDist;
        private List<Tile> hardPath;

        //A list of tiles adjacent to this tile
        //Sorted on creation
        private Tile[] adjacentTiles;
        private int currentLength;

        // Use this for initialization
        void Start() {
            unitContained = null;
            selectable = false;
            spriteRenderer = this.GetComponent<SpriteRenderer>();
            originalColor = spriteRenderer.color;
            hardPath = new List<Tile>();
        }

        // Update is called once per frame
        void Update() {

        }

        public override String ToString() {
            return "(" + this.transform.position.x + ", " + this.transform.position.y + ")";
        }

        // Creates a tile which can have up to 'adjacentCount' tiles adjacent
        public void Initialize(Board board, int adjacentCount) {
            this.adjacentTiles = new Tile[adjacentCount];
            this.board = board;
            currentLength = 0;
        }

        //Adds the given tile to the list of adjacent tiles in sorted order
        //Also adds this tile to the given tile by the same call
        public void addAdjacent(Tile t) {
            addAdjacent(t, true);
        }

        //Adds the given tile to the list of adjacent tiles in sorted order
        //Can decide whether to add this tile to the given tile
        public void addAdjacent(Tile t, bool addEquivalent) {
            currentLength++;
            if (currentLength > adjacentTiles.Length)
                throw new IndexOutOfRangeException("Attempting to add " + currentLength + " adjacent tiles to the tile " + this.ToString()
                    + "which can only hold " + adjacentTiles.Length + " tiles");

            for (int i = 0; i < currentLength - 1; i++) {
                if (adjacentTiles[i].getCost() > t.getCost()) {
                    //Insert t into list
                    for (int j = currentLength - 1; j > i; j--)
                        adjacentTiles[j] = adjacentTiles[j - 1];
                    adjacentTiles[i] = t;
                    if (addEquivalent)
                        t.addAdjacent(this, false);
                    return;
                }
            }

            adjacentTiles[currentLength - 1] = t;
            if (addEquivalent)
                t.addAdjacent(this, false);
        }

        public int getCost() {
            return this.cost;
        }

        public void shade(bool soft) {
            if (!soft)
                selectable = true;
            Color temp = spriteRenderer.color;
            if (soft)
                spriteRenderer.color = new Color(temp.r * .75f, temp.g * .75f, temp.b * .75f);
            else
                spriteRenderer.color = new Color(temp.r * .5f, temp.g * .5f, temp.b * .5f);
        }

        //Removes shade and set dist for either soft or hard
        public void removeShade(bool soft) {
            if (!soft) {
                selectable = false;
                hardPath.Clear();
            }
            resetDist(soft);
            spriteRenderer.color = originalColor;
            if (selectable)
                this.shade(false);
        }

        //Returns the list of adjacent tiles.  Note that this list is sorted
        public Tile[] getAdjacent() {
            return adjacentTiles;
        }

        public Tile[] getAllowedAdj(int distance, bool soft) {
            int index = 0;
            for (; index < adjacentTiles.Length; index++)
                if (this.getDist(soft) + adjacentTiles[index].getCost() > distance)
                    break;
            index -= 1;
            Tile[] toReturn = new Tile[index + 1];
            for (int i = 0; i <= index; i++) {
                toReturn[i] = adjacentTiles[i];
            }
            return toReturn;
        }

        public List<Tile> getPath() {
            return this.hardPath;
        }

        public void setPath(List<Tile> path) {
            hardPath.Clear();
            foreach (Tile item in path)
                hardPath.Add(item);
            hardPath.Add(this);
        }

        //Resets distances on this tile
        //If soft, doesn't reset hard distances
        public void resetDist(bool soft) {
            if (!soft)
                hardPath.Clear();
            softDist = 0;
        }

        //Sets the distance appropriately
        public void setSoftDist(int value) {
            softDist = value;
        }

        //Gets the appropriate distance
        public int getDist(bool soft) {
            return soft ? softDist : hardDist();
        }

        void OnMouseEnter() {
            if (hardPath.Count > 0)
                foreach (Tile t in hardPath)
                    t.shade(true);
        }

        void OnMouseExit() {
            if (hardPath.Count > 0)
                foreach (Tile t in hardPath)
                    t.removeShade(true);
        }

        void OnMouseOver() {
            if (Input.GetMouseButtonDown(0))
                if (hardPath.Count > 0)
                    board.move(hardPath.ToArray());
        }

        private int hardDist() {
            int sum = 0;
            foreach (Tile t in hardPath)
                sum += t.getCost();
            return sum;
        }
    }
}