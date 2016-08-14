using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace battle
{
    public class UnitManager : MonoBehaviour
    {
        [SerializeField]
        private Unit spider;
        [SerializeField]
        private Unit soldier;

        private Board board;
        private List<Unit> unitList;
        private UnitQueue initiativeQueue;
        private Unit currentUnit;
        private System.Random rand; //Explicit to avoid confusion with System.Random

        // Use this for initialization
        void Start() {
            unitList = new List<Unit>();
            initiativeQueue = new UnitQueue();
            rand = new System.Random();
            addUnits(3);
        }

        // Update is called once per frame
        void Update() {

        }

        public void Initialize(Board board) {
            this.board = board;
        }

        public void move(Tile[] path) {
            currentUnit.move(path);
            startTurn();
        }

        private void addUnits(int count) {
            List<Point> points = new List<Point>();
            for (int i = 0; i < count; i++) {
                Point point;
                do {
                    point = new Point(rand.Next(0, board.getHeight()), rand.Next(0, board.getWidth() / 3));
                } while (points.Contains(point));
                Tile tile = board.getTile(point.x, point.y);
                Unit newUnit = (Unit)GameObject.Instantiate(soldier, tile.transform.position, Quaternion.identity);
                newUnit.Initialize(tile, board, this);
                tile.unitContained = newUnit;
                unitList.Add(newUnit);
                initiativeQueue.insert(newUnit);
            }
            for (int i = 0; i < count; i++) {
                Point point;
                do {
                    point = new Point(rand.Next(0, board.getHeight()), rand.Next(2 * board.getWidth() / 3, board.getWidth()));
                } while (points.Contains(point));
                Tile tile = board.getTile(point.x, point.y);
                Unit newUnit = (Unit)GameObject.Instantiate(spider, tile.transform.position, Quaternion.identity);
                newUnit.Initialize(tile, board, this);
                tile.unitContained = newUnit;
                unitList.Add(newUnit);
                initiativeQueue.insert(newUnit);
            }
            startTurn();
        }

        private void startTurn() {
            if (initiativeQueue.empty())
                foreach (Unit u in unitList)
                    initiativeQueue.insert(u);
            currentUnit = initiativeQueue.pop();
            currentUnit.startTurn();
        }

        private class UnitQueue
        {
            private UnitNode head;

            public UnitQueue() {
                head = null;
            }

            public void clear() {
                head = null;
            }

            public bool empty() {
                return head == null;
            }

            public Unit pop() {
                Unit toReturn = head.item;
                head = head.next;
                return toReturn;
            }

            public void insert(Unit item) {
                if (head == null) {
                    head = new UnitNode(item);
                    return;
                }
                UnitNode prev = null;
                UnitNode current = head;
                while (current != null && current.item.getInitiative() >= item.getInitiative()) {
                    prev = current;
                    current = current.next;
                }
                if (prev == null)
                    head = new UnitNode(item, current);
                else
                    prev.next = new UnitNode(item, current);
            }
        }

        private class UnitNode
        {
            public Unit item;
            public UnitNode next;

            public UnitNode(Unit item) {
                this.item = item;
                this.next = null;
            }

            public UnitNode(Unit item, UnitNode next) {
                this.item = item;
                this.next = next;
            }
        }

        private class Point
        {
            public int x;
            public int y;

            public Point(int x, int y) {
                this.x = x;
                this.y = y;
            }

            public override bool Equals(object obj) {
                if (!(obj is Point))
                    return false;
                Point o = (Point)obj;
                return this.x == o.x && this.y == o.y;
            }

            public override int GetHashCode() {
                return base.GetHashCode();
            }
        }
    }
}