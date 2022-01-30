using Photon.Pun;
// USING AGREGADOS
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

public class PUN2_RoomController : MonoBehaviourPunCallbacks
{
    #region Vars

    //Player instance prefab, must be located in the Resources folder
    [Header("Tiempo de espera de otros jugadores")]
    public int timeBeforeMatch = 15;

    private Camera _camera;
    private GameManager _gameManager;

    [Header("Variables sobre slots")]
    public GameObject slotPrefab;
    public GameObject[] slots;
    public int slotsNumber = 40;

    [Header("Variables sobre los jugadores")]
    public int playersCount = 4;
    [Header("Si algún prefab no se encuentra, se utiliza este")]
    public GameObject defaultPrefab;
    [Header("Arrays de prefabs que se utilizarán en las partidas online, debe estar en: Resources")]
    public List<GameObject> cavaliersSkins;

    public int defaultLife = 100;
    public int defaultAttack = 10;

    public GameObject localPlayer;

    public int round = 0;
    public int turn = 0;

    [Header("Tiempo de las partidas")]
    public int turnTime = 30;
    public Text timeLabel;
    private int time;

    [Header("Gestion de la UI")]
    public Button endTurnButton;

    #endregion

    private void Awake()
    {
        _camera = Camera.main;
        _gameManager = FindObjectsOfType<GameManager>()[0];
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

        //Show the list of the players connected to this Room
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            //Show if this player is a Master Client. There can only be one Master Client per Room so use this to define the authoritative logic etc.)
            string isMasterClient = (PhotonNetwork.PlayerList[i].IsMasterClient ? "*" : "");
            int life = (int)PhotonNetwork.PlayerList[i].CustomProperties["life"];
            int attack = (int)PhotonNetwork.PlayerList[i].CustomProperties["attack"];
            GUI.Label(new Rect(5, 35 + 30 * i, 200, 25), PhotonNetwork.PlayerList[i].ActorNumber + "-" + isMasterClient + PhotonNetwork.PlayerList[i].NickName + "(" + life + ", " + attack + ")");
        }
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
        Hashtable hashtable = new Hashtable();
        hashtable.Add("turn", turn);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
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
        slots = new GameObject[slotsNumber];

        float x = 0, y = 0.0f, z = 0f;
        int splitIn = slotsNumber / 4;
        for (int i = 0, slotIndex = 1; i < slotsNumber; i++, slotIndex++)
        {
            // GameObject newSlot = Instantiate(slotPrefab, new Vector3(x, y, z), Quaternion.identity);
            GameObject newSlot = PhotonNetwork.Instantiate(slotPrefab.name, new Vector3(x, y, z), Quaternion.identity);
            newSlot.name = "Slot_" + slotIndex;

            if (slotIndex >= splitIn * 3)
            {
                x = -1;
                z++;
            }
            else if (slotIndex >= splitIn * 2)
            {
                x--;
                z = -10;
            }
            else if (slotIndex >= splitIn)
            {
                z--;
                x = 9;
            }
            else
            {
                x++;
            }

            slots[i] = newSlot;
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

        GameObject avatar = PhotonNetwork.Instantiate(
            cavaliersSkins[(int)player.CustomProperties["element"]].name,
            position,
            Quaternion.identity
        );
        avatar.name = player.NickName;

        // ASIGNAR SLOT
        Hashtable hashtable = new Hashtable();
        hashtable.Add("attack", defaultAttack);
        hashtable.Add("life", defaultLife);
        hashtable.Add("slot", initialPosition);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);

        Player playerData = avatar.GetComponent<Player>();
        playerData.gameManager = _gameManager;

        initialSlot.setPlayerInPosition(freePosition, avatar);

        // ASIGNAR LA CAMARA AL JUGADOR
        if (_camera != null && player != null)
        {
            _camera.GetComponent<CameraFollow>().target = avatar.transform;
        }
    }

    /// <summary>
    /// Establece en que turno van a jugar los jugadores
    /// </summary>
    private void setTurnsToPlayer()
    {
        SortedDictionary<int, Photon.Realtime.Player> sortedPlayers = new SortedDictionary<int, Photon.Realtime.Player>();
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log(player.NickName + ", order: " + (int)player.CustomProperties["order"]);
            sortedPlayers.Add((int)player.CustomProperties["order"], player);
        }

        int localTurn = 1;
        foreach (KeyValuePair<int, Photon.Realtime.Player> player in sortedPlayers)
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("turn", localTurn);
            player.Value.SetCustomProperties(hashtable);
            Debug.Log(player.Value.NickName + " va de " + localTurn + "°");
            localTurn++;
        }
    }
}
