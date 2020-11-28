/*
 * Reference:
 *  https://www.redblobgames.com/pathfinding/a-star/introduction.html
 *  https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
 */

using System.Collections.Generic;
using UnityEngine;
using MiniHexMap;
using Priority_Queue;

namespace HexMapToolkit
{
    static class PathfindingBase
    {
        public static List<HexCell> ConstructPath(HexCell from, HexCell to, Dictionary<HexCell, HexCell> visited)
        {
            List<HexCell> path = new List<HexCell>();
            HexCell curr = to;

            while (curr != from)
            {
                path.Add(curr);

                if (!visited.ContainsKey(curr))
                {
                    Debug.LogWarning("Cannot find path to target!");
                    return null;
                }
                curr = visited[curr];
            }
            if (curr == from)
            {
                path.Add(curr);
            }
            return path;
        }
    }

    public static class AStar
    {
        public static List<HexCell> Path(int from, int to, HexCell[] cells, int searchDepth = 500)
        {
            SimplePriorityQueue<HexCell> frontier = new SimplePriorityQueue<HexCell>();
            Dictionary<HexCell, HexCell> visited = new Dictionary<HexCell, HexCell>();
            Dictionary<HexCell, float> costAcc = new Dictionary<HexCell, float>();

            HexCell start = cells[from];
            HexCell goal = cells[to];

            VisitCell(start, null, 0, ref frontier, ref visited);
            costAcc[start] = 0;

            int depth = 0;

            while (frontier.Count > 0)
            {
                HexCell curr = frontier.Dequeue();

                if (depth > searchDepth)
                {
                    Debug.LogWarning("rearched A* max search depth.");
                    break;
                }
                if (curr == goal) break;

                foreach (HexCell cell in curr.GetAllNeighbors())
                {
                    float nextCost = curr.MoveCostTo(cell) + costAcc[curr];

                    if (!visited.ContainsKey(cell) || nextCost < costAcc[cell])
                    {
                        costAcc[cell] = nextCost;

                        float priority = nextCost + HeuristicCost(cell, goal);

                        VisitCell(cell, curr, priority, ref frontier, ref visited);
                    }
                }
                depth++;
            }
            return PathfindingBase.ConstructPath(start, goal, visited);
        }

        static float HeuristicCost(HexCell cell, HexCell goal)
        {
            return Abs(cell.coordinates.X - goal.coordinates.X) + Abs(cell.coordinates.Z - goal.coordinates.Z);
        }

        static float Abs(float a)
        {
            return a < 0 ? -a : a;
        }

        static void VisitCell(HexCell cell, HexCell from, float priority,
            ref SimplePriorityQueue<HexCell> frontier,
            ref Dictionary<HexCell, HexCell> visited)
        {
            frontier.Enqueue(cell, priority);
            visited[cell] = from;
        }
    }

    public static class BFS
    {
        public static HexCell Closest(int start, HexCell[] cells, int searchDepth = 30)
        {
            HexCell curr = cells[start];
            HexCell target = null;
            Queue<HexCell> frontier = new Queue<HexCell>();
            HashSet<HexCell> visited = new HashSet<HexCell>();

            frontier.Enqueue(curr);

            int depth = 0;
            while (frontier.Count > 0)
            {
                curr = frontier.Dequeue();
                visited.Add(curr);

                if (depth > searchDepth) break;

                if (!curr.Unpassable() && !curr.unit)
                {
                    target = curr;
                    break;
                }
                foreach (HexCell cell in curr.GetAllNeighbors())
                {
                    if (visited.Contains(cell))
                        continue;
                    frontier.Enqueue(cell);
                }
                depth++;
            }
            return target;
        }
    }
}