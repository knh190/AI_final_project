using System.Collections.Generic;
using MiniHexMap;
using UnityEngine;

public static class UnitExtensions
{
    public static HexCell NextStep(this Unit unit)
    {
        if (unit.runtime.currPath == null || unit.runtime.currPath.Count == 0)
            return null;

        HexCell next = unit.runtime.currCell;
        List<HexCell> path = unit.runtime.currPath;

        float accMoveCost = 0;

        for (int index = path.Count - 1; index >= 0; index--)
        {
            accMoveCost += next == null ? 0 : next.MoveCostTo(path[index]);

            if (accMoveCost > unit.runtime.remainSpeed)
            {
                return path[System.Math.Min(index + 1, path.Count - 1)];
            }
            next = path[index];
        }
        return path[0];
    }

    public static bool PathIsBlocked(this Unit unit)
    {
        if (unit.runtime.currPath == null)
            return false;

        foreach (HexCell cell in unit.runtime.currPath)
        {
            if (cell.unit != null && cell.unit != unit) return true;
        }
        return false;
    }

    public static bool NextStepIsBlocked(this Unit unit)
    {
        if (unit.runtime.currPath == null || unit.runtime.currPath.Count == 0)
            return false;

        HexCell cell = unit.runtime.currPath[0];

        if (cell.unit != null && cell.unit != unit)
        {
            return true;
        }
        return false;
    }

    public static void TakeCell(this Unit unit, HexCell target)
    {
        if (unit.runtime.currCell != null)
        {
            unit.runtime.currCell.unit = null;
            unit.runtime.currCell = null;
        }
        target.unit = unit;
        unit.runtime.currCell = target;
    }

    public static void MoveTo(this Unit unit, HexCell cell)
    {
        // blocked by another unit
        if (cell.unit != null && cell.unit != unit) return;
        // display on map
        HexGrids.instance.SetGameObjectAtTop(unit.gameObject, cell);
        unit.transform.position = unit.transform.position + unit.runtime.formation.unitOffset;
        // update map occupation
        unit.TakeCell(cell);
        // update runtime
        unit.runtime.remainSpeed -= MapUtils.MoveCostOnPath(cell, unit.runtime.currPath);
        // update currPath
        if (unit.runtime.currPath != null) unit.runtime.currPath.Remove(cell);
        // Set all units visibility
        Unit.FinishMoveEvent?.Invoke();
        // Show grass animation
        if (cell.grass) Animation.AnimateManager.instance?.Shake(cell.grass);
        // Play SFX
        if (unit.stats.walkSfx) unit.source.PlayOneShot(unit.stats.walkSfx);
    }
}

public static class UnitCombatExtensions
{
    public static float TerrainBonus(this Unit unit, HexCell cell, HexCell target)
    {
        if (cell.Elevation > target.Elevation)
            return unit.stats.terrainBonus;
        if (cell.Elevation < target.Elevation)
            return unit.stats.terrainBonus * -1;
        return 0;
    }

    public static void Attack(this Unit unit)
    {
        UnitRuntime ctx = unit.runtime;

        if (ctx.formation.clan == FormationClan.Enemy)
        {
            // burn house
            if (ctx.currCell.town != null)
            {
                // Play SFX
                if (unit.stats.walkSfx) unit.source.PlayOneShot(unit.stats.walkSfx);
                // Burn town
                Town town = ctx.currCell.town.GetComponent<Town>();
                town.TakeDamage(unit.stats.meleeAttack);
                //Debug.Log("Damage: " + unit.stats.meleeAttack + ", to: " + town.cell + ", remain: " + town.runtime.Health);
            }
            else
            {
                // attack enemy
            }
        }
    }
}

public static class UnitEnemyExtensions
{
    // invisible in grass, visible in range
    public static void SetVisibility(this Unit unit)
    {
        if (unit.runtime.formation.clan == FormationClan.Player) return;
        if (!unit.runtime.currCell) return;

        SpriteRenderer render = unit.GetComponent<SpriteRenderer>();
        render.enabled = !unit.runtime.currCell.grass || unit.InVisionRange();
    }

    public static bool InVisionRange(this Unit unit)
    {
        bool isEnemy = unit.runtime.formation.clan == FormationClan.Enemy;

        Formation[] formations = isEnemy ?
            FormationGenerator.instance.EnemyFormations :
            FormationGenerator.instance.PlayerFormations;

        foreach (Formation formation in formations)
        {
            // ignore same clan
            if (formation.clan == unit.runtime.formation.clan) continue;
            // calculate distance
            foreach (Unit u in formation.Units)
            {
                int distance = unit.runtime.currCell.coordinates.Distance(u.runtime.currCell.coordinates);
                if (distance <= u.stats.vision)
                {
                    //Debug.Log("enemy vision exposed!");
                    return true;
                }
            }
        }
        return false;
    }
}