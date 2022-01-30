using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// USING AGREGADOS
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class Player : MonoBehaviourPun, IOnEventCallback
{
    #region Vars

    public GameManager gameManager;
    public PUN2_RoomController roomController;
    public Photon.Realtime.Player localPlayer;
    public int PAperTurn = 5;

    private bool generateCards = false;
    private int turnIteration = 1;
    private GameObject handHolder;
    private List<GameObject> carsPerTurn = new List<GameObject>();

    public const byte NotifyTurnChangeEventCode = 1;

    #endregion

    #region Events

    void Start()
    {
        roomController = FindObjectOfType<PUN2_RoomController>();
        localPlayer = PhotonNetwork.LocalPlayer;
        gameManager = FindObjectOfType<GameManager>();

        handHolder = GameObject.Find("HandHolder");
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
                        if (turnIteration == 1)
                        {
                            Debug.Log("Ronda: " + round + ". Turno: " + PhotonNetwork.LocalPlayer.NickName + ". Master turn: " + masterTurn + ", local turn: " + playerTurn);
                            // 1- ASIGNAR PA
                            int currentPA = (int)PhotonNetwork.LocalPlayer.CustomProperties["PA"]; // OBTENER LOS PA
                            currentPA += PAperTurn; // AñADIR MAS PA
                            if (currentPA > gameManager.PALimitPerPlayer) // SI SE PASA DE 10, LIMITARLO
                            {
                                currentPA = gameManager.PALimitPerPlayer;
                            }
                            Hashtable hashtable = new Hashtable();
                            hashtable.Add("PA", currentPA);
                            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);

                            // 2- ASIGNAR CARTAS
                            Elements myElement = (Elements)((int)PhotonNetwork.LocalPlayer.CustomProperties["element"]);
                            carsPerTurn = gameManager.getCardsByType(myElement);
                            // 2.1- Tenemos que limpiar la mano anterior, tecnicamente, el cardholder
                            cleanHandHolder();
                            // 2.1- Añadir las nuevas cartas
                            generateCards = true; // Esto pone a funcionar el evento on GUI

                            turnIteration++;
                        }
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

    private void cleanHandHolder()
    {
        foreach (Transform child in handHolder.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void OnGUI()
    {
        if (generateCards)
        {
            int distanceBetweenCards = 116;
            foreach (GameObject card in carsPerTurn)
            {
                GameObject instance = Instantiate(card);
                instance.transform.SetParent(handHolder.transform);
                instance.GetComponent<RectTransform>().position = new Vector3(distanceBetweenCards, 76, 0);
                distanceBetweenCards += 116;
            }
            generateCards = false;
        }
    }




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
        if (eventCode == NotifyTurnChangeEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            Vector3 targetPosition = (Vector3)data[0];
            for (int index = 1; index < data.Length; ++index)
            {
                int unitId = (int)data[index];
                UnitList[unitId].TargetPosition = targetPosition;
            }
        }
    }
}
