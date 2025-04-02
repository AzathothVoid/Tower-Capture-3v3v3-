using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class TowerDictionary : SerializableDictionary<int, TowerData>
{
    
}

[System.Serializable]
public struct TowerData
{
    public List<int> Adjacents;
    public GameObject _building;
    public bool isCaptured;
    public int controllingTeam;
}