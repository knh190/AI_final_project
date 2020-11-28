using System.Collections.Generic;
using HexMapToolkit;
using MiniHexMap;

public static class HexCellExtension
{
    public static bool Unpassable(this HexCell cell)
    {
        if (cell.Elevation < 0 || cell.unit != null || cell.grass != null ||
            cell.material == HexMaterial.DarkRed ||
            cell.material == HexMaterial.Red)
            return true;

        bool connected = false;
        foreach (HexCell c in cell.GetAllNeighbors())
        {
            if (System.Math.Abs(c.Elevation - cell.Elevation) <= 1)
            {
                connected = true;
                break;
            }
        }
        return !connected;
    }

    public static bool IsNeighbour(this HexCell cell, HexCell other)
    {
        foreach (HexCell c in cell.GetAllNeighbors())
        {
            if (c == other) return true;
        }
        return false;
    }

    public static bool IsBorder(this HexCell cell)
    {
        return cell.GetAllNeighbors().Length < 6;
    }

    public static int MoveCostTo(this HexCell cell, HexCell target)
    {
        if (cell == target)
            return 0;

        bool isNeighbour = false;
        foreach (HexCell c in cell.GetAllNeighbors())
        {
            if (c == target)
            {
                isNeighbour = true;
                break;
            }
        }
        if (!isNeighbour)
            return HexCell.MaxMoveCost;
        if (target.unit != null)
            return HexCell.MaxMoveCost;

        if (System.Math.Abs(cell.Elevation - target.Elevation) >= 2)
            return HexCell.MaxMoveCost;

        return cell.MoveCost;
    }

    public static HexCell GetClosestCell(this HexCell cell)
    {
        HexCell target = BFS.Closest(cell.gridIndex, HexGrids.instance.Cells);
        if (target == null)
            throw new System.Exception("Cannot find proper cell to target! " + cell);
        return target;
    }
}

public static class HexCoordinatesExtension
{
    public static int Distance(this HexCoordinates c1, HexCoordinates c2)
    {
        return (System.Math.Abs(c1.X - c2.X) + System.Math.Abs(c1.Z - c2.Z) + System.Math.Abs(c1.Y - c2.Y)) / 2;
    }
}

public static class MapUtils
{
    public static int MoveCostOnPath(HexCell cell, List<HexCell> path)
    {
        if (path == null)
            return 0;

        int result = 0;
        int index = path.Count - 1;

        HexCell curr = path[index];
        while (curr != cell && index > 0)
        {
            HexCell next = path[index - 1];
            result += curr.MoveCostTo(next);
            curr = next;
            index--;
        }
        return result;
    }
}