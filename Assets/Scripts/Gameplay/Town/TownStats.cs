using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/TownStats")]
public class TownStats : ScriptableObject
{
    public float Health;
    public float RecoveryPerSecond;
    public float RecoveryAfterSeconds;

    public AudioClip burnSfx;
}
