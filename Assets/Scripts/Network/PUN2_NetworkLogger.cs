using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// USING AGREGADOS
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Pun;

public class PUN2_NetworkLogger : MonoBehaviourPunCallbacks
{
    private void FixedUpdate()
    {
        Debug.Log(
            "Cluster: " + PhotonNetwork.CurrentCluster + "\n" +
            "Lobby: " + PhotonNetwork.CurrentLobby.Name + ", type: " + PhotonNetwork.CurrentLobby.Type + "\n" +
            "Lobby: " + PhotonNetwork.CurrentRoom.Name + ", players: " + PhotonNetwork.CurrentRoom.PlayerCount.ToString()
        );
    }
}
