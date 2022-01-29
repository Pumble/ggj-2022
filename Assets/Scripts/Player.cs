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

    private void Update()
    {
        if (gameManager.MatchInCourse)
        {
            //if ((int)PhotonNetwork.LocalPlayer.CustomProperties["turn"] == roomController.turn)
            //{ 
            //    // AQUI ME PUEDO MOVER

            //}
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

    //public void setLife(int a)
    //{
    //    if(PhotonNetwork.is

    //    if ((life + a) <= 100)
    //    {
    //        life = life + a;
    //    }
    //    else
    //    {
    //        if ((life + a) > 100)
    //        {
    //            life = 100;
    //        }
    //        else
    //        {
    //            if ((life + a) <= 0)
    //            {
    //                life = 0;
    //                death();
    //            }
    //        }
    //    }
    //}
}
