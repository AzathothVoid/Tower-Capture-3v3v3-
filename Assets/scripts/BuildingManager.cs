using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class BuildingManager : MonoBehaviourPun
{
    public static BuildingManager Instance { get; private set; }

    [SerializeField] public List<TeamBuilding> TeamBuildings;
    //public int[] cathedralBuildingIDS = new int[]{6, 7, 8};
    public Dictionary<int, int> cathedralBuildingIDS = new Dictionary<int, int> { { 6, 0 }, { 7, 1 }, {8 ,2 } };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        for (int i = 0; i < TeamBuildings.Count; i++)
        {
            TeamBuilding temp = TeamBuildings[i];
            temp._buildingScript = TeamBuildings[i].Building.GetComponent<BuildingCapture>();

            for (int j = 0; j < temp.AdjacentBuildings.Count; j++)
            {
                AdjacentBuilding tempAdjacent = temp.AdjacentBuildings[j];
                tempAdjacent._buildingScript = tempAdjacent.Building.GetComponent<BuildingCapture>();
                temp.AdjacentBuildings[j] = tempAdjacent;
            }

            TeamBuildings[i] = temp;
        }        

    }

    public void CaptureBuilding(int teamIndex, int buildingIndex, bool value, BUILDINGTYPE type)
    {
        if (PhotonNetwork.IsMasterClient)         
            photonView.RPC("RPC_CaptureBuilding", RpcTarget.All, teamIndex, buildingIndex, value, type);
        
    }

    [PunRPC]
    void RPC_CaptureBuilding(int teamIndex, int buildingIndex, bool value, BUILDINGTYPE type)
    {
        TeamBuilding teamBuilding = TeamBuildings[teamIndex];

        if (type == BUILDINGTYPE.MAIN)
            teamBuilding.isCaptured = value;
        else if (type == BUILDINGTYPE.ENEMYMAIN)
            teamBuilding.EnemyBuildingsCaptured[buildingIndex] = value;
        else if (type == BUILDINGTYPE.ADJACENT)
        {
            AdjacentBuilding adjacent = teamBuilding.AdjacentBuildings[buildingIndex];
            adjacent.isCaptured = value;
            teamBuilding.AdjacentBuildings[buildingIndex] = adjacent;
        }
        TeamBuildings[teamIndex] = teamBuilding;
    }
}


[System.Serializable]
public struct TeamBuilding
{
    public GameObject Building;
    public BuildingCapture _buildingScript;
    public List<int> EnemyBuildings;
    public List<bool> EnemyBuildingsCaptured;
    public List<AdjacentBuilding> AdjacentBuildings;
    public int teamID;
    public int buildingID;
    public int cathedralID;
    public bool isCaptured;    
}

[System.Serializable]
public struct AdjacentBuilding
{
    public GameObject Building;
    public BuildingCapture _buildingScript;
    public int teamID;
    public int buildingID;
    public bool isCaptured;
}

public enum BUILDINGTYPE
{
    MAIN,
    ADJACENT,
    ENEMYMAIN
}

