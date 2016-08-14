using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace battle
{
    public class Board : MonoBehaviour
    {

        [SerializeField]
        private int r;
        [SerializeField]
        private int c;
        [SerializeField]
        private float tl;
        [SerializeField]
        private Tile empty;
        [SerializeField]
        private Tile rough;
        [SerializeField]
        private UnitManager um;

        private UnitManager unitManager;
        private Tile[,] board; //A list of all tiles on the board ordered by row and column
        private LinkedList<Tile> softShaded; //A list of softly shaded tiles
        private LinkedList<Tile> hardShaded; //A list of hard shaded tiles

        // Use this for initialization
        void Start() {
            CreateBoard(r, c, tl);
            softShaded = new LinkedList<Tile>();
            hardShaded = new LinkedList<Tile>();
            unitManager = (UnitManager)GameObject.Instantiate(um);
            unitManager.Initialize(this);
        }

        // Update is called once per frame
        void Update() {
        }

        public Tile getTile(int row, int col) {
            return board[row, col];
        }

        public int getHeight() {
            return board.GetLength(0);
        }
        public int getWidth() {
            return board.GetLength(1);
        }

        // Creates a board of hexagons with 'rows', 'cols' columns, and tiles 'tileLength' apart
        // Note that tileLength implicitely gives the vertical distance between tiles
        public void CreateBoard(int rows, int cols, float tileLength) {
            board = new Tile[rows, cols];
            float totalLength = tileLength * (cols - 1);
            float totalHeight = tileLength * (rows - 1);
            System.Random myRand = new System.Random();

            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < cols; j++) {

                    int adjCount = 6;
                    if (i == 0 || i == rows - 1)
                        adjCount -= 2;
                    if (j == 0 || j == cols - 1) {
                        adjCount -= 1;
                        if (i % 2 == 0 && j == 0)
                            adjCount -= 2;
                        else if (i % 2 == 1 && j == cols - 1)
                            adjCount -= 2;
                    }
                    if (adjCount == 1)
                        adjCount = 2;

                    //Note that the x becomes more positive while the y becomes more negative as j/i increase
                    Vector2 pos = new Vector2((j * tileLength) - (totalLength / 2) - (tileLength / 4), (totalHeight / 2) - (i * tileLength));
                    if (i % 2 == 1)
                        pos.x += tileLength / 2;
                    Tile currentTile;
                    if (myRand.Next(0, 100) > 70)
                        currentTile = (Tile)GameObject.Instantiate(rough, pos, Quaternion.identity);
                    else
                        currentTile = (Tile)GameObject.Instantiate(empty, pos, Quaternion.identity);
                    currentTile.Initialize(this, adjCount);

                    if (j > 0)
                        currentTile.addAdjacent(board[i, j - 1]);
                    if (i > 0) {
                        currentTile.addAdjacent(board[i - 1, j]);
                        if (i % 2 == 0 && j != 0)
                            currentTile.addAdjacent(board[i - 1, j - 1]);
                        if (i % 2 == 1 && j != cols - 1)
                            currentTile.addAdjacent(board[i - 1, j + 1]);
                    }

                    board[i, j] = currentTile;
                }
            }
        }

        //Removes shading from all tiles in the soft list and empties the list
        //If not soft, also removes shading/selectability from all tiles in the hard list and empties the list
        public void removeShade(bool soft) {
            if (!soft) {
                foreach (Tile t in hardShaded)
                    t.removeShade(false);
                hardShaded.Clear();
            }
            foreach (Tile t in softShaded)
                t.removeShade(true);
            softShaded.Clear();
        }

        //Shades all tiles within the given 'distance' from the 'start' tile
        //If not soft, shades the tiles hard and adds selectability
        public void shade(Tile start, int distance, bool soft) {
            removeShade(soft);
            getLL(soft).AddFirst(start);
            LLQueue tileQueue = new LLQueue();
            foreach (Tile t in start.getAllowedAdj(distance, soft)) {
                if (t.unitContained != null)
                    continue;
                if (soft)
                    t.setSoftDist(t.getCost());
                else
                    t.setPath(new List<Tile>());
                tileQueue.insert(t, soft);
            }
            while (!tileQueue.empty()) {
                Tile next = tileQueue.pop();
                next.shade(soft);
                getLL(soft).AddFirst(next);
                foreach (Tile t in next.getAllowedAdj(distance, soft)) {
                    if (getLL(soft).Contains(t))
                        continue;
                    if (t.unitContained != null)
                        continue;
                    if (t.getDist(soft) != 0 && t.getDist(soft) <= next.getDist(soft) + t.getCost())
                        continue;
                    if (t.getDist(soft) > 0)
                        tileQueue.remove(t);
                    if (soft)
                        t.setSoftDist(next.getDist(soft) + t.getCost());
                    if (!soft)
                        t.setPath(next.getPath());
                    tileQueue.insert(t, soft);
                }
            }
        }

        public void move(Tile[] path) {
            removeShade(false);
            unitManager.move(path);
        }

        private LinkedList<Tile> getLL(bool soft) {
            if (soft)
                return softShaded;
            return hardShaded;
        }

        private class LLQueue
        {
            private TileNode head;

            public LLQueue() {
                head = null;
            }

            public bool empty() {
                return head == null;
            }

            public Tile pop() {
                Tile toReturn = head.item;
                head = head.next;
                return toReturn;
            }

            public void insert(Tile item, bool soft) {
                if (head == null) {
                    head = new TileNode(item);
                    return;
                }
                TileNode prev = null;
                TileNode current = head;
                while (current != null && current.item.getDist(soft) <= item.getDist(soft)) {
                    prev = current;
                    current = current.next;
                }
                if (prev == null)
                    head = new TileNode(item, current);
                else
                    prev.next = new TileNode(item, current);
            }

            public void remove(Tile item) {
                TileNode prev = null;
                TileNode current = head;
                while (current.item != item) {
                    prev = current;
                    current = current.next;
                }
                if (prev == null)
                    head = current.next;
                else
                    prev.next = current.next;
            }
        }

        private class TileNode
        {
            public Tile item;
            public TileNode next;

            public TileNode(Tile item) {
                this.item = item;
                this.next = null;
            }

            public TileNode(Tile item, TileNode next) {
                this.item = item;
                this.next = next;
            }
        }
    }
}