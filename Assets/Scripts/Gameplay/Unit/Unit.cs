using HexMapToolkit;
using UnityEngine;
using MiniHexMap;

public enum UnitType
{
    Arrow, Spear, Sword
}

// default status: make formation
// attack: lock enemy
public enum UnitStatus
{
    Default, Attack
}

[RequireComponent(typeof(CombatMan), typeof(AudioSource))]
public class Unit: MonoBehaviour
{
    public UnitStats stats;
    public UnitRuntime runtime;

    private HexCell lastTarget;
    private CombatMan runner;
    public AudioSource source;

    public delegate void OnFinishMove();
    public static OnFinishMove FinishMoveEvent;

    private void Awake()
    {
        runner = GetComponent<CombatMan>();
        source = GetComponent<AudioSource>();
        runtime = ScriptableObject.CreateInstance<UnitRuntime>();
    }

    #region REFACTOR_THIS
    // todo temp code
    float lastAttackTime;

    private void Update()
    {
        // can attack and cooldown is over
        if (runtime.currCell.town != null && Time.time - lastAttackTime > 3f)
        {
            lastAttackTime = Time.time;
            this.Attack();
        }
    }
    #endregion

    // @todo move to best place, instead of just formation slot
    public void MakeFormation()
    {
        // find formation cell
        HexCell target = FormationSlot;
        // no place to move
        if (target == null)
        {
            target = runtime.formation.cell.GetClosestCell();
        }
        // push to nearest possible cell
        if (target.Unpassable() || (target.unit != null && target.unit != this))
        {
            target = runtime.formation.cell.GetClosestCell();
        }
        // force move on initialization
        if (runtime.currCell == null)
        {
            this.MoveTo(target);
        }
        // set target when is placed on map
        else
        {
            runtime.currTarget = target;
            runner.RegisterAction(MoveAction.Create(this));
        }
    }

    public void StepMove()
    {
        // reset remain speed
        runtime.remainSpeed = stats.speed;
        // start from which cell
        int from = runtime.currCell != null ? runtime.currCell.gridIndex : runtime.formation.cell.gridIndex;
        // calculate path when target changed or blocked
        if (lastTarget != runtime.currTarget || this.NextStepIsBlocked())
        {
            lastTarget = runtime.currTarget;
            runtime.currPath = AStar.Path(from, runtime.currTarget.gridIndex, HexGrids.instance.Cells);
        }
        // find next step cell
        if (runtime.currPath != null)
        {
            // calculate using speed
            HexCell nextStep = this.NextStep();
            // move to next cell
            if (nextStep) this.MoveTo(nextStep);
            // generate next step move
            if (runtime.currCell != runtime.currTarget || runtime.currTarget != FormationSlot)
            {
                runner.RegisterAction(MoveAction.Create(this));
            }
        }
    }

    public HexCell FormationSlot
    {
        get
        {
            HexCell rowCell = runtime.formation.RowCell(runtime.formation.cell, Row);
            HexCell colCell = runtime.formation.ColCell(rowCell, Col);
            return colCell;
        }
    }

    public int Row
    {
        get
        {
            switch (runtime.formation.currFormation)
            {
                case FormationType.Line:
                    return 0;
                case FormationType.Square:
                    if (runtime.unitIndex == 3)
                        return 2;
                    if (runtime.unitIndex == 1)
                        return 0;
                    return 1;
                case FormationType.Triangle:
                    if (runtime.unitIndex == 0)
                        return 0;
                    return 1;
                default:
                    throw new System.Exception("Formation not supported!");
            }
        }
    }

    public int Col
    {
        get
        {
            switch (runtime.formation.currFormation)
            {
                case FormationType.Line:
                    return runtime.unitIndex;
                case FormationType.Square:
                    if (runtime.unitIndex % 2 == 1)
                        return 1;
                    return runtime.unitIndex;
                case FormationType.Triangle:
                    if (runtime.unitIndex == 0)
                        return 1;
                    return runtime.unitIndex - 1;
                default:
                    throw new System.Exception("Formation not supported!");
            }
        }
    }

    public bool IsRangeSlot(HexCell cell)
    {
        if (stats.unitType != UnitType.Arrow)
            return false;

        int distance = cell.coordinates.Distance(runtime.currCell.coordinates);
        return distance >= stats.minAttackRange && distance <= stats.maxAttackRange;
    }

    public bool IsMeleeSlot(HexCell cell)
    {
        return runtime.currCell.IsNeighbour(cell);
    }

    public bool IsWeakSlot(HexCell cell)
    {
        HexDirection face = runtime.formation.face;

        HexCell c1 = runtime.currCell.GetNeighbor(face.Next().Next());
        HexCell c2 = runtime.currCell.GetNeighbor(face.Next().Next().Next());
        HexCell c3 = runtime.currCell.GetNeighbor(face.Next().Next().Next().Next());

        return cell == c1 || cell == c2 || cell == c3;
    }

    private void OnEnable()
    {
        FinishMoveEvent += this.SetVisibility;
    }

    private void OnDisable()
    {
        FinishMoveEvent -= this.SetVisibility;
    }

    //private void OnDrawGizmos()
    //{
    //    Vector3 pos = FormationSlot.transform.position;
    //    pos.y += FormationSlot.Elevation * 5f;

    //    Gizmos.DrawSphere(pos, 3f);
    //}
}
