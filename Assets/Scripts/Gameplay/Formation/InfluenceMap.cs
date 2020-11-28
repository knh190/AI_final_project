using UnityEngine;
using MiniHexMap;
using System.Collections.Generic;

public class InfluenceMap : ScriptableObject
{
    // terrain info & advantage map
    internal int[] terrainMap;
    // enemies
    internal int[] enemyMap;
    // friends
    internal int[] friendMap;
    // overall result
    internal int[] overallMap;

    internal readonly HashSet<Formation> friends = new HashSet<Formation>();
    internal readonly HashSet<Formation> enemies = new HashSet<Formation>();
    internal readonly Dictionary<Formation, int[]> selfInfluence = new Dictionary<Formation, int[]>();

    public delegate void OnTownDestroy();
    public static OnTownDestroy TownDestroyEvent;

    public void Initialize()
    {
        int size = HexGrids.instance.Cells.Length;

        terrainMap = new int[size];
        enemyMap = new int[size];
        friendMap = new int[size];
        overallMap = new int[size];

        //InitializeTerrainMap();
    }

    public void Refresh()
    {
        while (friends.Contains(null))
            friends.Remove(null);
        while (enemies.Contains(null))
            enemies.Remove(null);

        InitializeTerrainMap();
        InitializeFriendMap();
        InitializeEnemyMap();

        UpdateOverallMap();
    }

    public int[] UpdateOverallMap()
    {
        int size = HexGrids.instance.Cells.Length;

        for (int i = 0; i < size; i++)
        {
            overallMap[i] = friendMap[i] - enemyMap[i] + terrainMap[i];
        }
        return overallMap;
    }

    public static int[] ReduceSelfMap(int[] overall, int[] selfInfluence)
    {
        for (int i = 0; i < overall.Length; i++)
            overall[i] -= selfInfluence[i];
        return overall;
    }

    public void RegisterFriendFormation(Formation formation)
    {
        int size = HexGrids.instance.Cells.Length;
        friends.Add(formation);
        selfInfluence[formation] = new int[size];
    }

    public void RegisterEnemyFormation(Formation formation)
    {
        enemies.Add(formation);
    }

    public void InitializeTerrainMap()
    {
        int size = HexGrids.instance.Cells.Length;

        for (int i = 0; i < size; i++)
        {
            HexCell cell = HexGrids.instance.Cells[i];
            if (!cell.Unpassable())
            {
                int score = (cell.Elevation - 1) / 2;
                // weight on burning towns
                if (cell.town != null)
                    score += 5;
                // weight on hiding in grass
                if (cell.grass != null)
                    score += 2;
                terrainMap[i] = Mathf.Max(score, 0);
            }
        }
        //Debug.Log("Terrain map updated.");
    }

    public void InitializeEnemyMap()
    {
        int size = HexGrids.instance.Cells.Length;

        for (int i = 0; i < size; i++)
        {
            HexCell cell = HexGrids.instance.Cells[i];
            enemyMap[i] = 0;

            int inf = 0;
            foreach (Formation formation in enemies)
            {
                int _inf = 0;
                if (IsWeakPointFor(cell, formation))
                {
                    _inf -= MeleeAttackCountFor(cell, formation);
                }
                _inf += RangeAttackCountFor(cell, formation);
                _inf += MeleeAttackCountFor(cell, formation);

                inf += _inf;
            }
            enemyMap[i] = inf;
        }
    }

    public void InitializeFriendMap()
    {
        int size = HexGrids.instance.Cells.Length;

        for (int i = 0; i < size; i++)
        {
            HexCell cell = HexGrids.instance.Cells[i];
            friendMap[i] = 0;

            int inf = 0;
            foreach (Formation formation in friends)
            {
                int _inf = 0;
                if (IsWeakPointFor(cell, formation))
                {
                    _inf -= MeleeAttackCountFor(cell, formation);
                }
                _inf += RangeAttackCountFor(cell, formation);
                _inf += MeleeAttackCountFor(cell, formation);

                selfInfluence[formation][i] = _inf;
                inf += _inf;
            }
            friendMap[i] = inf;
        }
    }

    int RangeAttackCountFor(HexCell cell, Formation formation)
    {
        int cnt = 0;
        foreach (Unit unit in formation.Units)
        {
            if (unit.IsRangeSlot(cell))
            {
                cnt += unit.stats.rangeAttack;
            }
        }
        return cnt;
    }

    int MeleeAttackCountFor(HexCell cell, Formation formation)
    {
        int cnt = 0;
        foreach (Unit unit in formation.Units)
        {
            if (unit.IsMeleeSlot(cell))
            {
                cnt += unit.stats.meleeAttack;
            }
        }
        return cnt;
    }

    bool IsWeakPointFor(HexCell cell, Formation formation)
    {
        foreach (Unit unit in formation.Units)
        {
            if (unit.IsWeakSlot(cell)) return true;
        }
        return false;
    }

    private void OnEnable()
    {
        TownDestroyEvent += InitializeTerrainMap;
    }

    private void OnDisable()
    {
        TownDestroyEvent -= InitializeTerrainMap;
    }
}
