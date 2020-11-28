using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/UnitStats")]
public class UnitStats : ScriptableObject
{
    public UnitType unitType;

    public int health;
    public int speed;
    public int vision;

    public int meleeAttack;
    public int meleeDefense;
    public int rangeAttack;
    public int chargeBonus;
    public int terrainBonus;

    public int minAttackRange;
    public int maxAttackRange;

    public AudioClip walkSfx;
    public AudioClip attackSfx;
}
