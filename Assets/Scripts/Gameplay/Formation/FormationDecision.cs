using MCTS;
using MiniHexMap;
using UnityEngine;

[RequireComponent(typeof(Formation))]
public class FormationDecision : MonoBehaviour
{
    public bool enableDecision = false;

    [HideInInspector]
    private Formation formation;
    [HideInInspector]
    public HexCell targetCell;

    private delegate int StrategyFunc(int currIndex, int[] influence, HexCell[] cells);
    //private readonly MonteCarloTreeSearch search = new MonteCarloTreeSearch
    //{
    //    MaxSearchStep = 30,
    //    MaxExpandLevel = 1
    //};

    private float lastStatusSetTime;
    private readonly float changeStatusPerSeconds = 1f;

    private void Awake()
    {
        formation = GetComponent<Formation>();
    }

    private void Update()
    {
        float now = Time.time;

        // set formation battle status
        if (now - lastStatusSetTime > changeStatusPerSeconds)
        {
            lastStatusSetTime = now;
            SetStatus();
        }
        // only make decision when don't have a target
        // or the determined target is blocked.
        if (enableDecision && (!targetCell || TargetIsBlocked()))
        {
            MakeDecision();
        }
    }

    public void MakeDecision()
    {
        if (targetCell == null)
            targetCell = formation.cell;

        InfluenceMap influenceMap = formation.clan == FormationClan.Enemy ?
            FormationGenerator.instance.enemyInfluence :
            FormationGenerator.instance.playerInfluence;

        // influence except for the formation self
        int[] influence = influenceMap.UpdateOverallMap();

        if (influenceMap.selfInfluence.ContainsKey(formation))
        {
            int[] selfInfluence = influenceMap.selfInfluence[formation];
            influence = InfluenceMap.ReduceSelfMap(influence, selfInfluence);
        }
        else
        {
            Debug.LogWarning("Influence Map not contain the formation. Cannot make decision.");
            return;
        }

        // Decide which step to take next.
        // MCTS strategy is broken, use this instead.

        StrategyFunc strategy = PickStrategy();
        int targetIndex = strategy(formation.cell.gridIndex, influence, HexGrids.instance.Cells);
        targetCell = HexGrids.instance.Cells[targetIndex];

        //Debug.Log("Decide target: " + targetCell + ", influence: " + influence[targetIndex]);

        // prevent dancing
        if (formation.cell.coordinates.Distance(targetCell.coordinates) > 0f)
        {
            formation.cell = targetCell;
            formation.MakeFormation();
        }
        //Debugger.instance?.ClearHighlight();
        //Debugger.instance?.HighlightCell(targetCell);
    }

    // pick strategy for a formation
    // according to the formation status
    private StrategyFunc PickStrategy()
    {
        switch (formation.status)
        {
            case FormationStatus.Default:
                return GreedyStrategy.Pick;
            case FormationStatus.AttackTown:
            case FormationStatus.AttackEnemy:
                return OptimalInfluenceStrategy.Pick;
            default:
                throw new System.Exception("Formation status not supported!");
        }
    }

    private bool TargetIsBlocked()
    {
        // test if target cell is blocked by the other formation units
        bool occupiedBySelf = formation.HasUnit(targetCell.unit);
        return !occupiedBySelf;
    }

    private void SetStatus()
    {
        // status default if no town & enemy in vision
        // status lock enemy if enemy's in vision
        // status lock town if town's in vision & no enemy
        if (!formation.EngageEnemy)
        {
            formation.status = FormationStatus.Default;

            if (targetCell != null && targetCell.town != null)
                formation.status = FormationStatus.AttackTown;
        }
        else
        {
            formation.status = FormationStatus.AttackEnemy;
        }
    }
}
