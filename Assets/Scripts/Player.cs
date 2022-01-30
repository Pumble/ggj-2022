using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// USING AGREGADOS
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Player : MonoBehaviourPun
{
    #region Vars

    public GameManager gameManager;
    public PUN2_RoomController roomController;
    public Photon.Realtime.Player localPlayer;

    #endregion

    #region Events

    void Start()
    {
        roomController = FindObjectsOfType<PUN2_RoomController>()[0];
        localPlayer = PhotonNetwork.LocalPlayer;
    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.LocalPlayer != null)
            {
                bool matchInCourse = false;
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("matchInCourse"))
                {
                    matchInCourse = (bool)PhotonNetwork.CurrentRoom.CustomProperties["matchInCourse"];
                }

                if (matchInCourse == true)
                {
                    int masterTurn = (int)PhotonNetwork.CurrentRoom.CustomProperties["turn"];
                    int playerTurn = (int)PhotonNetwork.LocalPlayer.CustomProperties["turn"];
                    int round = (int)PhotonNetwork.CurrentRoom.CustomProperties["round"];

                    if (masterTurn == playerTurn)
                    {
                        Debug.Log("Ronda: " + round + ". Turno: " + PhotonNetwork.LocalPlayer.NickName + ". Master turn: " + masterTurn + ", local turn: " + playerTurn);
                    }
                }
            }
            else
            {
                Debug.LogWarning("PhotonNetwork.LocalPlayer es null en Player.cs");
            }
        }
        else
        {
            Debug.LogWarning("PhotonNetwork.CurrentRoom es null en Player.cs");
        }
    }

    #endregion

    public void move(int from, int to)
    {
        Slot slotFrom = roomController.slots[from].GetComponent<Slot>();
        Slot slotTo = roomController.slots[to].GetComponent<Slot>();

        Vector3 fromPosition = roomController.slots[0].transform.position + slotFrom.getPlayerLocation((int)localPlayer.CustomProperties["slot"]);
        int nextFreePosition = slotTo.getFreePosition();
        Vector3 toPosition = roomController.slots[2].transform.position + slotTo.getLocationByIndex(nextFreePosition);

        // Remove from old position to the new one
        slotFrom.removePlayerFromLocationByIndex((int)localPlayer.CustomProperties["slot"]);

        Hashtable hashtable = new Hashtable();
        hashtable.Add("slot", nextFreePosition);
        PhotonNetwork.SetPlayerCustomProperties(hashtable);

        StartCoroutine(moveToken(3f, fromPosition, toPosition, this.transform));
    }

    IEnumerator moveToken(float time, Vector3 from, Vector3 to, Transform transform)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(from, to, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
