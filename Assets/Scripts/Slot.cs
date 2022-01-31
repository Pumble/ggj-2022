using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
// USING AGREGADOS
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.Video;

public class InnerPosition
{
    #region VARS

    public Vector3 location;
    public GameObject player;

    #endregion

    public InnerPosition(Vector3 location)
    {
        this.location = location;
        this.player = null;
    }
}

public class Slot : MonoBehaviourPun
{
    #region VARS

    public int playersCount = 4;
    public InnerPosition[] positions;
    public Card card = null;
    public int index = 0;
    public int PACost = 2;

    private string selectedCard;
    private VideoPlayer localVideoPlayer;
    private RawImage localRawImage;
    public GameObject localPlane;

    #endregion

    private void Awake()
    {
        positions = new InnerPosition[playersCount];
        GameObject[] objects = GameObject.FindGameObjectsWithTag("player_position");
        for (int i = 0; i < playersCount; i++)
        {
            positions[i] = new InnerPosition(objects[i].transform.position);
        }

        localVideoPlayer = GetComponentInChildren<VideoPlayer>();
        localRawImage = GetComponentInChildren<RawImage>();
    }

    #region Positions

    public int getFreePosition()
    {
        bool freePosition = false;
        int index = 0;
        do
        {
            if (positions[index].player == null)
            {
                freePosition = true;
            }
            else
            {
                index++;
            }
        } while (index < playersCount && freePosition == false);

        return index;
    }

    public Vector3 getLocationByIndex(int index)
    {
        return positions[index].location;
    }

    public Vector3 getPlayerLocation(int playerIndex)
    {
        return positions[playerIndex].location;
    }

    public void setPlayerInPosition(int index, GameObject player)
    {
        positions[index].player = player;
    }

    public int getPlayerLocation(GameObject player)
    {
        bool founded = false;
        int index = 0;
        for (index = 0; index < positions.Length && founded == false; index++)
        {
            if (positions[index].player == player)
            {
                founded = true;
            }
        }
        return index;
    }

    public void removePlayerFromLocationByIndex(int index)
    {
        positions[index].player = null;
    }

    #endregion

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        object[] data = (object[])photonEvent.CustomData;
        switch ((int)eventCode)
        {
            case (int)NetworkEvents.SummonCard:
                handle_SummonCard_Event(data);
                break;
        }
    }

    private void handle_SummonCard_Event(object[] data)
    {
        selectedCard = (string)data[0];
    }

    public void OnClick_SummonCard()
    {
        GameObject selectedCardGO = GameObject.Find(selectedCard);
        if (selectedCardGO != null)
        {
            // HACER LA COMPROBACION DE LOS PA
            int PA = (int)PhotonNetwork.LocalPlayer.CustomProperties["PA"];
            if (PA >= PACost)
            {
                // TENEMOS QUE PAGAR LOS PA POR COLOCAR LA CARTA
                Hashtable hashtable = new Hashtable();
                hashtable.Add("PA", PA - PACost);
                PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);

                card = selectedCardGO.GetComponent<Card>();
                showSummonedCard(selectedCardGO);

                // Array contains the data to share
                object[] content = new object[] { selectedCard };

                // You would have to set the Receivers to All in order to receive this event on the local client as well
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.All
                };
                PhotonNetwork.RaiseEvent((int)NetworkEvents.SummonedCard, content, raiseEventOptions, SendOptions.SendReliable);
            }
            else
            {
                Debug.Log("El jugador " + PhotonNetwork.LocalPlayer.NickName + " no tiene suficientes PA. Solo tiene: " + PA);
            }
        }
    }

    private void showSummonedCard(GameObject selectedCardGO)
    {
        VideoPlayer videoPlayer = selectedCardGO.GetComponent<VideoPlayer>();
        RawImage rawImage = selectedCardGO.GetComponent<RawImage>();

        localVideoPlayer.source = videoPlayer.source;
        localVideoPlayer.clip = videoPlayer.clip;
        localVideoPlayer.playOnAwake = videoPlayer.playOnAwake;
        localVideoPlayer.isLooping = videoPlayer.isLooping;
        localVideoPlayer.renderMode = videoPlayer.renderMode;
        localVideoPlayer.targetTexture = videoPlayer.targetTexture;

        localRawImage.texture = rawImage.texture;

        //MeshRenderer meshRenderer = localPlane.GetComponent<MeshRenderer>();
        //Material material = Resources.Load<Texture>("CardTextures/");
        //meshRenderer.material = material;

        //if (card != null)
        //{
        //}
    }
}
