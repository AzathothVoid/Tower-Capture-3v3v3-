using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Team Settings")]
    public GameObject[] teamPlayerPrefabs; // Index 0:Team0, 1:Team1, 2:Team2
    public Transform[] teamSpawnPoints;    // Index 0:Team0, 1:Team1, 2:Team2

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        PhotonNetwork.JoinOrCreateRoom("MainRoom", new RoomOptions(), TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}");
        AssignTeamAndSpawnPlayer();
    }

    void AssignTeamAndSpawnPlayer()
    {
        // Get team ID based on join order (0,1,2,0,1,2...)
        int teamID = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % 3;
        Debug.Log($"Assigned Team: {teamID}");

        // Validate prefabs and spawn points
        if (!ValidateTeamResources(teamID)) return;

        // Spawn player with team-specific prefab
        GameObject player = PhotonNetwork.Instantiate(
            teamPlayerPrefabs[teamID].name,
            teamSpawnPoints[teamID].position,
            Quaternion.identity
        );

        SetupPlayerTeamComponent(player, teamID);
    }

    bool ValidateTeamResources(int teamID)
    {
        if (teamPlayerPrefabs.Length < 3 || teamSpawnPoints.Length < 3)
        {
            Debug.LogError("Missing team prefabs or spawn points!");
            return false;
        }

        if (teamPlayerPrefabs[teamID] == null || teamSpawnPoints[teamID] == null)
        {
            Debug.LogError($"Missing resources for team {teamID}!");
            return false;
        }
        return true;
    }

    void SetupPlayerTeamComponent(GameObject player, int teamID)
    {
        PlayerTeam pt = player.GetComponent<PlayerTeam>();
        if (pt != null)
        {
            pt.teamID = teamID;
            UpdateNetworkProperties(teamID);
            pt.photonView.RPC("RPC_SetTeamID", RpcTarget.AllBuffered, teamID);
        }
        else
        {
            Debug.LogWarning("Player prefab missing PlayerTeam component!");
        }
    }


    void UpdateNetworkProperties(int teamID)
    {
        Hashtable teamProperty = new Hashtable();
        teamProperty.Add("teamID", teamID);
        PhotonNetwork.LocalPlayer.SetCustomProperties(teamProperty);
    }
}