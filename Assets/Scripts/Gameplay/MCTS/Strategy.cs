using System.Collections.Generic;
using MiniHexMap;
using UnityEngine;

namespace MCTS
{
    // Benchmark
    public static class NaiveStrategy
    {
        // gives simulation result based on heuristics
        public static int SimulateRandomPlayout(ref Node node)
        {
            int target = node.state.MoveToMapIndex;
            int result = node.state.InfluenceMap[target];

            node.state.CurrentMapIndex = target;

            return result;
        }
    }

    // Pick best influence tile
    // has penalty on distance
    public static class ShortestPathStrategy
    {
        // penalty on distance
        public static int SimulateRandomPlayout(ref Node node)
        {
            if (node.state.grids == null)
            {
                throw new System.Exception("State not initialized correctly. Need hex grids in state for the strategy.");
            }
            int curr = node.state.CurrentMapIndex;
            int target = node.state.MoveToMapIndex;
            int result = node.state.InfluenceMap[target];
            // distance to target
            HexCell currCell = node.state.grids.Cells[curr];
            HexCell targetCell = node.state.grids.Cells[target];
            int distance = currCell.coordinates.Distance(targetCell.coordinates);

            //Debug.Log("currCell: " + currCell + ", target: " + targetCell + ", distance: " + distance);

            node.state.CurrentMapIndex = target;
            return result - distance / 10;
        }
    }

    // Greedy pick closest max influence tile,
    // but influence has distance penalty
    public static class OptimalInfluenceStrategy
    {
        // return index on influence map
        public static int Pick(int currIndex, int[] influence, HexCell[] cells)
        {
            int bestIndex = 0;
            int bestScore = int.MinValue;
            for (int i = 0; i < influence.Length; i++)
            {
                if (influence[i] > 0)
                {
                    int distance = cells[currIndex].coordinates.Distance(cells[i].coordinates) / 5;
                    int score = influence[i] - distance;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIndex = i;
                    }
                }
            }
            return bestIndex;
        }
    }

    // Greedy pick closest max influence tile,
    // influence has no distance penalty
    public static class GreedyStrategy
    {
        // return index on influence map
        public static int Pick(int currIndex, int[] influence, HexCell[] cells)
        {
            int bestIndex = 0;
            int bestScore = int.MinValue;
            for (int i = 0; i < influence.Length; i++)
            {
                if (influence[i] > 0)
                {
                    int score = influence[i];
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestIndex = i;
                    }
                }
            }
            return bestIndex;
        }
    }
}
