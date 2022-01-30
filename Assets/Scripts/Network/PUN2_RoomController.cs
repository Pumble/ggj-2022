using Photon.Pun;
// USING AGREGADOS
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PUN2_RoomController : MonoBehaviourPunCallbacks
{
    #region Vars

    //Player instance prefab, must be located in the Resources folder
    [Header("Tiempo de espera de otros jugadores")]
    public int timeBeforeMatch = 15;

    private GameManager _gameManager;

    [Header("Variables sobre slots")]
    public GameObject slotPrefab;
    public GameObject[] slots;
    public int slotsNumber = 40;
    public GameObject _board;
    private GameObject _innerBoard;

    [Header("Variables sobre los jugadores")]

    public int playersCount = 4;
    [Header("Si alg�n prefab no se encuentra, se utiliza este")]
    public GameObject defaultPrefab;
    [Header("Arrays de prefabs que se utilizar�n en las partidas online, debe estar en: Resources")]
    public List<GameObject> cavaliersSkins;

    public int defaultLife = 100;
    public int defaultAttack = 10;


    public int defaultRanking = 0;

    public int[] statsPlayers;

    public GameObject localPlayer;

    public int round = 0;
    public int turn = 0;

    [Header("Tiempo de las partidas")]
    public int turnTime = 30;
    public Text timeLabel;
    private int time;

    [Header("Gestion de la UI")]
    public Button endTurnButton;

    public GameObject pnlBuyShield;

    #endregion

    private void Awake()
    {
        _gameManager = FindObjectsOfType<GameManager>()[0];
        _innerBoard = _board.transform.GetChild(0).gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        // In case we started this demo with the wrong scene being active, simply load the menu scene
        if (PhotonNetwork.CurrentRoom == null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
            return;
        }

        // We're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
        // PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity, 0);

        /**
         * Aqui asignamos el turno correspondiente, en base al numero random que
         * sacaron cuando ingresaron a la sala
         */
        setTurnsToPlayer();

        generatBoard();

        /**
         * Ahora debemos colocar los jugadores en el tablero
         * */
        setPlayersInInitialPosition(0);
        // Cargar imagen de perfil
        //Debug.Log("Numero del elemento" + (int)PhotonNetwork.LocalPlayer.CustomProperties["element"]);
        //_gameManager.loadImgProfile((int)PhotonNetwork.LocalPlayer.CustomProperties["element"]);
        //
        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("turn", turn);
            hashtable.Add("round", round);
            hashtable.Add("matchInCourse", true);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);

            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("beginDownwiseClock", RpcTarget.All);
            photonView.RPC("nextRound", RpcTarget.All);
            photonView.RPC("nextTurn", RpcTarget.All);

            _gameManager.MatchInCourse = true;
        }
    }

    public void getStatsPlayer(int numberPlayers, int index)
    {
        statsPlayers = new int[numberPlayers];

        for (int i = 0; i < numberPlayers; i++)
        {
            Photon.Realtime.Player player = _gameManager.playersRanking[i];
            switch (index)
            {
                case 0:
                    statsPlayers[i] = (int)player.CustomProperties["laps"] * 100 + (int)player.CustomProperties["slot"];
                    break;
                case 1:
                    statsPlayers[i] = (int)player.CustomProperties["life"];
                    break;
                case 2:
                    statsPlayers[i] = (int)player.CustomProperties["shields"];
                    break;
                case 3:
                    statsPlayers[i] = (int)player.CustomProperties["attack"];
                    break;
                default:
                    break;
            }
        }
    }
    private void sortRanking()
    {
        _gameManager.playersRanking = PhotonNetwork.PlayerList;
        int numberPlayers = _gameManager.playersRanking.Length;
        int a = 0;
        int aux = 0;
        int mx = 0;
        int index = 0;
        bool sort = true;
        bool equal = false;
        Photon.Realtime.Player auxPhoton = null;

        while (sort)
        {
            sort = false;
            equal = false;
            if (index != 4)
            {
                getStatsPlayer(numberPlayers, index);
                sortMaximum(a, aux, auxPhoton, mx, index, numberPlayers);
                int k = a;
                while ((k < (numberPlayers - 1)) && (!equal))
                {
                    if (statsPlayers[k] == statsPlayers[k + 1])
                    {
                        a = k;
                        sort = true;
                        index++;
                        equal = true;
                    }
                    else
                    {
                        k++;
                    }
                }
            }
        }
        for (int i = 0; i < numberPlayers; i++)
        {
            _gameManager.playersRanking[i].CustomProperties["ranking"] = i;
        }
    }
    private void sortMaximum(int a, int aux, Photon.Realtime.Player auxPhoton, int mx, int index, int numberPlayers)
    {
        for (int i = a; i < numberPlayers; i++)
        {
            mx = i;
            for (int j = i + 1; j < numberPlayers; j++)
            {
                if (statsPlayers[j] > statsPlayers[mx])
                {
                    mx = j;
                }
            }
            aux = statsPlayers[i];
            auxPhoton = _gameManager.playersRanking[i];

            statsPlayers[i] = statsPlayers[mx];
            _gameManager.playersRanking[i] = _gameManager.playersRanking[mx];

            statsPlayers[mx] = aux;
            _gameManager.playersRanking[mx] = auxPhoton;
        }
    }
    public void menuShield()
    { // click en Shield
        pnlBuyShield.SetActive(true);
    }
    public void cancelGetShield()
    { // cancel
        pnlBuyShield.SetActive(false);
    }
    public void getShield() // Aceptar
    {
        Photon.Realtime.Player player = PhotonNetwork.LocalPlayer;
        int a = (int)player.CustomProperties["turn"];

        if ((true/*a == 1*/) && ((int)player.CustomProperties["shields"] < 2) && ((int)player.CustomProperties["PA"] >= 2))
        {
            int shields = (int)player.CustomProperties["shields"] + 1;
            int PA = (int)player.CustomProperties["PA"] - 2;

            Hashtable hashtable = new Hashtable();
            hashtable.Add("shields", shields);
            player.SetCustomProperties(hashtable);

            hashtable.Add("PA", PA);
            player.SetCustomProperties(hashtable);
        }
    }
    void OnGUI()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        //Leave this Room
        if (GUI.Button(new Rect(5, 5, 125, 25), "Dejar sala"))
        {
            PhotonNetwork.LeaveRoom();
        }

        //Show the Room name
        GUI.Label(new Rect(135, 5, 200, 25), PhotonNetwork.CurrentRoom.Name);
        /*
        //Show the list of the players connected to this Room
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)                     
        {
            //Show if this player is a Master Client. There can only be one Master Client per Room so use this to define the authoritative logic etc.)
            string isMasterClient = (PhotonNetwork.PlayerList[i].IsMasterClient ? "*" : "");
            int life = (int)PhotonNetwork.PlayerList[i].CustomProperties["life"];
            int attack = (int)PhotonNetwork.PlayerList[i].CustomProperties["attack"];
            GUI.Label(new Rect(5, 35 + 30 * i, 200, 25), PhotonNetwork.PlayerList[i].ActorNumber + "-" + isMasterClient + PhotonNetwork.PlayerList[i].NickName + "(" + life + ", " + attack + ")");
        }*/
    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.LocalPlayer != null)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("turn"))
                {
                    int masterTurn = (int)PhotonNetwork.CurrentRoom.CustomProperties["turn"];
                    int playerTurn = (int)PhotonNetwork.LocalPlayer.CustomProperties["turn"];
                    if (masterTurn == playerTurn)
                    {
                        endTurnButton.enabled = true;
                    }
                    else
                    {
                        endTurnButton.enabled = false;
                    }
                }
                else
                {
                    Debug.LogWarning("PhotonNetwork.CurrentRoom no contiene la key: turn");
                }
            }
            else
            {
                Debug.LogWarning("PhotonNetwork.LocalPlayer es null en PUN2_RoomController.cs");
            }
        }
        else
        {
            Debug.LogWarning("PhotonNetwork.CurrentRoom es null en PUN2_RoomController.cs");
        }
        if (PhotonNetwork.IsMasterClient)  // solo se puede ejecutar el 
        {
            sortRanking();
        }
        //player PA = ;
    }

    #region OVERRIDES

    public override void OnLeftRoom()
    {
        //We have left the Room, return back to the GameLobby
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
    }

    #endregion

    #region Metodos PUN

    [PunRPC]
    void beginDownwiseClock()
    {
        time = turnTime;
        timeLabel.enabled = true;
        InvokeRepeating("updateDownwiseClock", 1f, 1f);
    }

    void updateDownwiseClock()
    {
        time--;
        timeLabel.text = time.ToString();
        if (time <= 0)
        {
            // REVISAR QUIEN GANO
            CancelInvoke("updateDownwiseClock");
            timeLabel.enabled = false;

            PhotonView photonView = PhotonView.Get(this);
            photonView.RPC("nextTurn", RpcTarget.All);
        }
    }

    [PunRPC]
    void nextRound()
    {
        round++;
        Hashtable hashtable = new Hashtable();
        hashtable.Add("round", round);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
    }

    [PunRPC]
    void nextTurn()
    {
        turn++;
        if (turn > PhotonNetwork.PlayerList.Length)
        {
            turn = 1;
            photonView.RPC("nextRound", RpcTarget.All);
        }

        Hashtable hashtable = new Hashtable();
        hashtable.Add("turn", turn);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);

        Debug.Log("Iniciando --> Ronda: " + round + ",Turno #" + turn);

        NotifyTurnChangeEvent();
    }

    private void NotifyTurnChangeEvent()
    {
        // Array contains the data to share
        object[] content = new object[] { round, turn };

        // You would have to set the Receivers to All in order to receive this event on the local client as well
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };
        PhotonNetwork.RaiseEvent((int)NetworkEvents.TurnChange, content, raiseEventOptions, SendOptions.SendReliable);
    }

    #endregion

    #region OnClick Events

    public void onClick_EndTurnButton()
    {
        PhotonView photonView = PhotonView.Get(this);
        photonView.RPC("nextTurn", RpcTarget.All);
    }

    #endregion

    /// <summary>
    /// Generate the board, make it with slots
    /// </summary>
    private void generatBoard()
    {
        if (_board != null)
        {
            slots = new GameObject[slotsNumber];

            float x = -4.5f, y = 0.01f, z = -4.5f;
            int splitIn = slotsNumber / 4;
            Vector3 scale = new Vector3(1f, 0.01f, 1f);
            for (int i = 0, slotIndex = 1; i < slotsNumber; i++, slotIndex++)
            {
                // GameObject newSlot = Instantiate(slotPrefab, new Vector3(x, y, z), Quaternion.identity);
                // GameObject newSlot = PhotonNetwork.Instantiate(slotPrefab.name, new Vector3(x, y, z), Quaternion.identity);

                GameObject newSlot = Instantiate(slotPrefab);
                newSlot.transform.SetParent(_innerBoard.transform);
                newSlot.transform.localPosition = new Vector3(x, y, z);
                newSlot.transform.localScale = scale;

                newSlot.name = "Slot_" + slotIndex;

                if (slotIndex >= splitIn * 3)
                {
                    x -= scale.x;
                    z = -5.5f;
                }
                else if (slotIndex >= splitIn * 2)
                {
                    x = 5.5f;
                    z -= scale.z;
                }
                else if (slotIndex >= splitIn)
                {
                    z = 4.5f;
                    x += scale.x;
                }
                else
                {
                    z += scale.z;
                }

                newSlot.GetComponent<Slot>().index = slotIndex;
                slots[i] = newSlot;
            }
        }
    }

    /// <summary>
    /// Instantiate every player in a given slot
    /// </summary>
    private void setPlayersInInitialPosition(int initialPosition)
    {
        Slot initialSlot = slots[initialPosition].GetComponent<Slot>();
        // SOLO INSTANCIAR AL LOCAL
        Photon.Realtime.Player player = PhotonNetwork.LocalPlayer;

        int freePosition = initialSlot.getFreePosition();
        Vector3 position = initialSlot.getLocationByIndex(freePosition) + slots[initialPosition].transform.position;

        int skin = PhotonNetwork.LocalPlayer.ActorNumber;

        Debug.Log("initialSlot =" + initialSlot + "freePosition =" + freePosition + "Skin =" + skin);
        GameObject avatar = PhotonNetwork.Instantiate(
                    cavaliersSkins[skin].name,
                    position,
                    Quaternion.identity);

        avatar.name = player.NickName;
        // ASIGNAR SLOT
        Hashtable hashtable = new Hashtable();
        hashtable.Add("attack", defaultAttack);
        hashtable.Add("life", defaultLife);
        hashtable.Add("slot", initialPosition);
        hashtable.Add("element", skin);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);

        _gameManager.loadImgProfile(skin);

        Player playerData = avatar.GetComponent<Player>();
        playerData.gameManager = _gameManager;

        initialSlot.setPlayerInPosition(freePosition, avatar);
        try
        {

/*
GameObject avatar = PhotonNetwork.Instantiate(
                    cavaliersSkins[skin].name,
                    position,
                    Quaternion.identity); */

        }
        catch (Exception e)
        {
            Debug.Log("Falla aqui");
        }


        //// ASIGNAR LA CAMARA AL JUGADOR
        //if (_camera != null && player != null)
        //{
        //    _camera.GetComponent<CameraFollow>().target = avatar.transform;
        //}
        initialSlot.setPlayerInPosition(freePosition, avatar);
    }


    /// <summary>
    /// Establece en que turno van a jugar los jugadores
    /// </summary>
    private void setTurnsToPlayer()
    {
        SortedDictionary<int, Photon.Realtime.Player> sortedPlayers = new SortedDictionary<int, Photon.Realtime.Player>();
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            sortedPlayers.Add((int)player.CustomProperties["order"], player);
        }

        int localTurn = 1;
        foreach (KeyValuePair<int, Photon.Realtime.Player> player in sortedPlayers)
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("turn", localTurn);
            player.Value.SetCustomProperties(hashtable);
            Debug.Log(player.Value.NickName + " va de " + localTurn + "°, " + (int)player.Value.CustomProperties["order"]);
            localTurn++;
        }
    }
}
