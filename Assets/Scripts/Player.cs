using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// USING AGREGADOS
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;
using Photon.Realtime;
using UnityEngine.UI;

public class Player : MonoBehaviourPun, IOnEventCallback
{
    #region Vars

    public GameManager gameManager;
    public PUN2_RoomController roomController;
    public Photon.Realtime.Player localPlayer;
    public int PAperTurn = 5;

    private bool generateCards = false;
    private GameObject handHolder;
    private List<GameObject> carsPerTurn = new List<GameObject>();
    private int whatTurnIsIt = 0;

    public GameObject selectedCard;

    #endregion

    #region Events

    void Start()
    {
        roomController = FindObjectOfType<PUN2_RoomController>();
        localPlayer = PhotonNetwork.LocalPlayer;
        gameManager = FindObjectOfType<GameManager>();

        handHolder = GameObject.Find("HandHolder");
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
        if (handHolder != null)
        {
            foreach (Transform child in handHolder.transform)
            {
                child.gameObject.SetActive(false);
                GameObject.Destroy(child.gameObject);
            }
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
                Card cardClass = instance.GetComponent<Card>();
                instance.name = PhotonNetwork.LocalPlayer.NickName + "_" + cardClass.title + "_" + whatTurnIsIt;

                Button button = instance.GetComponent<Button>();
                button.onClick.AddListener(() =>
                {
                    OnClick_Card(instance);
                });

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
        Debug.Log("Evento recibido: " + (int)eventCode);

        object[] data = (object[])photonEvent.CustomData;
        switch ((int)eventCode)
        {
            case (int)NetworkEvents.TurnChange:
                handle_TurnChange_Event(data);
                break;
            case (int)NetworkEvents.SummonedCard:
                handle_SummonedCard_Event(data);
                break;
        }
    }

    public void OnClick_Card(GameObject goCard)
    {
        selectedCard = goCard;

        // Array contains the data to share
        object[] content = new object[] { goCard.name };

        // You would have to set the Receivers to All in order to receive this event on the local client as well
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };
        PhotonNetwork.RaiseEvent((int)NetworkEvents.SummonCard, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void handle_TurnChange_Event(object[] data)
    {
        int receivedRound = (int)data[0];
        int receivedTurn = (int)data[1];

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
                    int playerTurn = (int)PhotonNetwork.LocalPlayer.CustomProperties["turn"];

                    if (receivedTurn == playerTurn)
                    {
                        whatTurnIsIt = receivedTurn;

                        Debug.Log("Es mi turno " + PhotonNetwork.LocalPlayer.NickName + "!");
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
                        Debug.Log("PA de " + PhotonNetwork.LocalPlayer.NickName + ": " + currentPA);

                        // 2- ASIGNAR CARTAS
                        Elements myElement = (Elements)((int)PhotonNetwork.LocalPlayer.CustomProperties["element"]);
                        Debug.Log("Mi elemento es: " + (int)myElement);
                        carsPerTurn = gameManager.getCardsByType(myElement);
                        foreach (GameObject go in carsPerTurn)
                        {
                            Card card = go.GetComponent<Card>();
                            Debug.Log("Carta: " + card.name + " obtenida!");
                        }

                        // 2.1- Tenemos que limpiar la mano anterior, tecnicamente, el cardholder
                        cleanHandHolder();
                        // 2.1- Añadir las nuevas cartas
                        generateCards = true; // Esto pone a funcionar el evento on GUI
                    }
                }
                else
                {
                    Debug.Log("La partida ya termino o no se establecio matchInCourse en el currentRoom");
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

    private void handle_SummonedCard_Event(object[] data)
    {
        string selectedCard = (string)data[0];
        if (handHolder != null)
        {
            GameObject go = GameObject.Find(selectedCard);
            go.SetActive(false);
            GameObject.Destroy(go);
        }
    }
}
