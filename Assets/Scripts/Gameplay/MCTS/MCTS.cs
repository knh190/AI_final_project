using System.Collections.Generic;
using MiniHexMap;
using UnityEngine;

namespace MCTS
{
    public class State
    {
        public int WinScore; // score of advantage
        public int VisitCount;
        public int[] InfluenceMap { get; private set; } // combined friend & enemy map & village map
        public int MoveToMapIndex { get; private set; } // target map position
        public int CurrentMapIndex; // current map position
        public HexGrids grids; // grids to calculate distance

        public State(int[] influenceMap, int curr, int target, HexGrids grids = null)
        {
            this.InfluenceMap = influenceMap;
            this.CurrentMapIndex = curr;
            this.MoveToMapIndex = target;
            this.WinScore = 0;
            this.grids = grids;
        }

        // possible states based on current state
        public State[] AllPossibleStates()
        {
            List<State> states = new List<State>();

            //Debugger.instance?.ClearHighlight();

            for (int i = 0; i < InfluenceMap.Length; i++)
            {
                if (InfluenceMap[i] > 0)
                {
                    State newState = new State(InfluenceMap, CurrentMapIndex, i, grids);
                    states.Add(newState);

                    //Debugger.instance?.HighlightCell(grids.Cells[i], InfluenceMap[i] / 10f);
                }
            }
            Debug.Log("possible states: " + states.Count);
            
            return states.ToArray();
        }
    }

    public class Node
    {
        public Node parent;
        public List<Node> children;
        public State state;
        public int level;

        public Node(State state)
        {
            this.children = new List<Node>();
            this.state = state;
        }

        public Node(State state, Node parent)
        {
            this.parent = parent;
            this.children = new List<Node>();
            this.state = state;
            this.level = parent.level + 1;
        }

        public Node RandomChildNode()
        {
            if (children.Count == 0)
                return null;
            return children[Random.Range(0, children.Count)];
        }

        public Node ChildWithMaxScore()
        {
            if (children.Count == 0)
                return null;

            Node[] sortByWinScore = children.ToArray();
            System.Array.Sort(sortByWinScore, CompareByWinScore);

            return sortByWinScore[children.Count - 1];
        }

        public float UctValue(int totalVisit) {
            if (state.VisitCount == 0)
            {
                return int.MaxValue;
            }
            return (state.WinScore / (float)state.VisitCount) +
                1.41f * Mathf.Sqrt(Mathf.Log(totalVisit, 2f) / (float)state.VisitCount);
        }

        private static int CompareByWinScore(Node n1, Node n2)
        {
            return n1.state.WinScore.CompareTo(n2.state.WinScore);
        }
    }

    public class MCTree
    {
        public Node root { get; internal set; }
    }

    public class MonteCarloTreeSearch
    {
        public int MaxSearchStep = 30;
        public int MaxExpandLevel = 5;

        public delegate int SimulateRandomPlayout(ref Node node);
        SimulateRandomPlayout simulateFunc;

        MCTree tree;

        // create tree and initialize states
        public void Initialize(State state, SimulateRandomPlayout simulateFunc)
        {
            this.simulateFunc = simulateFunc;
            tree = new MCTree
            {
                root = new Node(state)
            };
        }

        // find next move
        public State NextMove()
        {
            if (tree == null)
            {
                Debug.LogError("MCTS not initialized!");
                return null;
            }
            Debug.Log("best score before: " + tree.root.state.WinScore + ", index: " + tree.root.state.MoveToMapIndex);

            for (int step = 0; step < MaxSearchStep; step++)
            {
                Node promisingNode = SelectPromisingNode(tree.root);
                // if game not finished & depth allowed
                if (promisingNode.level < MaxExpandLevel)
                    ExpandNode(promisingNode);

                Node nodeToExplore = promisingNode;
                if (promisingNode.children.Count > 0)
                {
                    nodeToExplore = nodeToExplore.RandomChildNode();
                }
                int playoutResult = simulateFunc(ref nodeToExplore);
                BackPropogation(nodeToExplore, playoutResult);

                //Debug.Log("step: " + step + ", simulation result: " + playoutResult + ", index: " + nodeToExplore.state.MoveToMapIndex);
            }
            // take move
            Node winnerNode = tree.root.ChildWithMaxScore();
            if (winnerNode != null)
                tree.root = winnerNode;

            Debug.Log("best score: " + tree.root.state.WinScore + ", index: " + tree.root.state.MoveToMapIndex);

            return tree.root.state;
        }

        Node SelectPromisingNode(Node node)
        {
            Node curr = node;
            if (curr.children.Count != 0)
            {
                curr = UCT.FindBestNodeWithUCT(curr);
            }
            return curr;
        }

        // create new children using all possible states
        void ExpandNode(Node node)
        {
            if (node.children.Count != 0)
                return;
            State[] possibleStates = node.state.AllPossibleStates();
            foreach (State state in possibleStates)
            {
                Node newNode = new Node(state, node);
                node.children.Add(newNode);
            }
        }

        // update visit count & win score
        void BackPropogation(Node node, int simulationResult)
        {
            Node curr = node;
            while (curr != null)
            {
                curr.state.VisitCount += 1;
                curr.state.WinScore = Mathf.Max(simulationResult, curr.state.WinScore);
                curr = curr.parent;
            }
        }
    }

    static class UCT
    {
        public static Node FindBestNodeWithUCT(Node node)
        {
            if (node.children.Count == 0)
                return null;

            Node bestChild = null;

            float bestUCT = float.MinValue;
            int parentVisitCount = node.state.VisitCount;

            foreach (Node child in node.children)
            {
                float currUCT = child.UctValue(parentVisitCount);
                if (currUCT > bestUCT)
                {
                    bestUCT = currUCT;
                    bestChild = child;
                }
            }
            return bestChild;
        }
    }
}
