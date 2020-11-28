using EasyButtons;
using UnityEngine;
using MiniHexMap;
using System.Collections.Generic;

public class FormationGenerator : MonoBehaviour
{
    public static FormationGenerator instance;

    public int coordX = 10;
    public int coordZ = 10;
    public HexCoordinates coord;
    public Formation formationPrefab;

    internal InfluenceMap playerInfluence;
    internal InfluenceMap enemyInfluence;

    private HexCell[] PlayerSpawnPoints;
    private HexCell[] EnemySpawnPoints;

    public Formation[] PlayerFormations
    {
        get { return this.GetFormations(FormationClan.Player); }
    }

    public Formation[] EnemyFormations
    {
        get { return this.GetFormations(FormationClan.Enemy); }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            DestroyImmediate(this);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        playerInfluence = ScriptableObject.CreateInstance<InfluenceMap>();
        playerInfluence.Initialize();

        enemyInfluence = ScriptableObject.CreateInstance<InfluenceMap>();
        enemyInfluence.Initialize();

        InitializeSpawnPoints();

        AutoGenerate();
    }

    [Button]
    public void Generate()
    {
        coord = new HexCoordinates(coordX, coordZ);

        if (formationPrefab == null)
        {
            Debug.LogWarning("Formation prefab is not set!");
            return;
        }

        HexCell cell = HexGrids.instance.GetCell(coord);
        if (cell == null)
        {
            Debug.LogWarning("Invalid cell!");
            return;
        }
        GenerateFormation(cell);
    }

    [Button]
    public void AutoGenerate()
    {
        if (formationPrefab == null)
        {
            Debug.LogWarning("Formation prefab is not set!");
            return;
        }
        GenerateAtSpawnPoint();
    }

    void GenerateAtSpawnPoint()
    {
        if (formationPrefab.clan == FormationClan.Enemy)
        {
            int randomIndex = (int)Random.value * (EnemySpawnPoints.Length - 1);
            GenerateFormation(EnemySpawnPoints[randomIndex]);
        }
        else
        {
            int randomIndex = (int)Random.value * (PlayerSpawnPoints.Length - 1);
            GenerateFormation(PlayerSpawnPoints[randomIndex]);
        }
    }

    private void LateUpdate()
    {
        // update influence map
        enemyInfluence.Refresh();
        playerInfluence.Refresh();

        // update influence ui
        DisplayInfluence.instance?.UpdateUI();
    }

    void GenerateFormation(HexCell cell)
    {
        Formation go = Instantiate(formationPrefab);
        go.transform.SetParent(transform, false);

        go.cell = cell;
        go.Initialize();

        if (go.clan == FormationClan.Enemy)
        {
            enemyInfluence.RegisterFriendFormation(go);
            playerInfluence.RegisterEnemyFormation(go);
        }
        else
        {
            playerInfluence.RegisterFriendFormation(go);
            enemyInfluence.RegisterEnemyFormation(go);
        }
        Debug.Log("Formation is placed on " + cell);
    }

    private void InitializeSpawnPoints()
    {
        List<HexCell> spawns = new List<HexCell>();

        // add all border passable cells to enemy spawns
        foreach (HexCell cell in HexGrids.instance.Cells)
        {
            if (cell.Unpassable()) continue;
            if (cell.IsBorder()) spawns.Add(cell);
        }
        EnemySpawnPoints = spawns.ToArray();
        spawns.Clear();

        // add center cells to player spawns
        Vector3 center = HexGrids.instance.Center;
        foreach (HexCell cell in HexGrids.instance.Cells)
        {
            if (cell.Unpassable() || cell.IsBorder()) continue;
            if (Vector3.Distance(cell.transform.position, center) <= 30f) spawns.Add(cell);
        }
        PlayerSpawnPoints = spawns.ToArray();
        spawns.Clear();

        Debug.Log("Enemy spawn points: " + EnemySpawnPoints.Length);
        Debug.Log("Player spawn points: " + PlayerSpawnPoints.Length);
    }

    public int GetInfluenceAtCell(int gridIndex, FormationClan clan)
    {
        if (clan == FormationClan.Enemy)
        {
            return enemyInfluence.overallMap[gridIndex];
        }
        return playerInfluence.overallMap[gridIndex];
    }
}
