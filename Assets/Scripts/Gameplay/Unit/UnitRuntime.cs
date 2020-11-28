using System.Collections.Generic;
using MiniHexMap;
using UnityEngine;

public class UnitRuntime : ScriptableObject
{
    public Formation formation;
    public HexCell currCell;
    public int unitIndex;
    public List<HexCell> currPath;
    public float remainSpeed;
    public HexCell currTarget;
}
