using System.Collections.Generic;
using EasyButtons;
using MiniHexMap;
using UnityEngine;

public enum FormationType
{
    Line, Square, Triangle
}

public enum FormationClan
{
    Player, Enemy
}

// default: move towards towns
// attack town: burn town if no enemy's in sight
// attack enemy: lock enemy
public enum FormationStatus
{
    Default, AttackTown, AttackEnemy
}

public class Formation : MonoBehaviour
{
    [Header("Config")]
    [Range(0, 4)]
    public int size = 4;
    public FormationClan clan = FormationClan.Player;
    public Vector3 unitOffset;
    public Unit[] unitPrefab;

    [Header("Runtime")]
    public FormationType currFormation;
    public HexDirection face = HexDirection.NE;

    [HideInInspector]
    public HexCell cell;
    [HideInInspector]
    public FormationStatus status = FormationStatus.Default;

    private readonly List<Unit> units = new List<Unit>();

    public void Initialize()
    {
        if (unitPrefab == null)
        {
            Debug.LogWarning("Unit prefab is not set, cannot generate formation!");
            return;
        }
        if (unitPrefab.Length != size)
        {
            Debug.LogWarning("Unit prefab must match formation size!");
            return;
        }
        for (int i = 0; i < size; i++)
        {
            CreateUnit(i);
        }
        SortUnit();

        MakeFormation();
    }

    [Button]
    public void MakeFormation()
    {
        foreach (Unit unit in units)
        {
            unit.MakeFormation();
        }
    }

    public Unit[] Units
    {
        get
        {
            return units.ToArray();
        }
    }

    public HexCell[] Cells
    {
        get
        {
            List<HexCell> cells = new List<HexCell>();

            foreach (Unit unit in units)
            {
                cells.Add(unit.runtime.currCell);
            }
            return cells.ToArray();
        }
    }

    private void CreateUnit(int index)
    {
        GameObject prefab = unitPrefab[index].gameObject;
        GameObject go = HexGrids.instance.SetPrefabAtTop(prefab, cell, 1, false);
        go.transform.position = go.transform.position + unitOffset;

        Unit unit = go.GetComponent<Unit>();
        unit.transform.SetParent(transform, true);
        unit.runtime.formation = this;
        unit.runtime.unitIndex = index;

        units.Add(unit);
    }

    public void AddUnit(Unit unit)
    {
        unit.runtime.formation = this;
        unit.runtime.unitIndex = units.Count;

        units.Add(unit);
    }

    // todo
    public void RemoveUnit(int index)
    {
        // re-sort formation
        throw new System.NotImplementedException();
    }

    // arrow always at the end of a formation
    private void SortUnit()
    {
        int currIndex = 0;
        foreach (Unit unit in units)
        {
            if (unit.stats.unitType != UnitType.Arrow)
            {
                unit.runtime.unitIndex = currIndex;
                currIndex++;
            }
        }
        foreach (Unit unit in units)
        {
            if (unit.stats.unitType == UnitType.Arrow)
            {
                unit.runtime.unitIndex = currIndex;
                currIndex++;
            }
        }
    }

    public HexCell RowCell(HexCell start, int row = 0)
    {
        HexCell curr = start;
        HexDirection back = face.Opposite();

        for (int i = 0; i < row; i++)
        {
            curr = curr.GetNeighbor(back);
        }
        return curr;
    }

    public HexCell ColCell(HexCell start, int col = 0)
    {
        HexCell curr = start;

        for (int i = 0; i < col; i++)
        {
            if (curr != null)
                curr = curr.GetNeighbor(face.Next());
        }
        return curr;
    }

    public bool HasUnit(Unit unit)
    {
        return units.Contains(unit);
    }

    public bool EngageEnemy
    {
        get
        {
            foreach (Unit unit in units)
            {
                if (unit.InVisionRange()) return true;
            }
            return false;
        }
    }
}
