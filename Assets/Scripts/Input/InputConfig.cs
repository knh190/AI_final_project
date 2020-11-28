using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/InputConfig")]
public class InputConfig : ScriptableObject
{
    [Header("Config")]
    public Image outline;
    public Image outlineFormation;
    public Image outlineInfluence;

    [Header("Camera Zoom")]
    [Range(0, .5f)]
    public float zoomSpeed = .05f;
    [Range(1, 50)]
    public float zoomLimitMin = 25f;
    [Range(50, 100)]
    public float zoomLimitMax = 50f;

    [Header("Camera Pan")]
    [Range(50, 100)]
    public float panSpeed = 50f;
    [Range(100, 200)]
    public float panSpeedMax = 200f;
    [Range(50, 100)]
    public float panSpeedMin = 50f;
    [Range(1, 5)]
    public float secondsToMaxSpeed = 3;
    [Range(10, 100)]
    public float panBorderLimit = 10f;
}
