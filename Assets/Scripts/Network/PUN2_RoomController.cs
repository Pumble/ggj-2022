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
    public int matchTime = 30;
    public Text timeLabel;
    private int time;

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

        generatBoard();
        /**
         * Una vez generado el tablero, cada jugador debera instanciar su personaje,
         * pero antes, hay que asignarles un elemento
         */
        setPlayerElementByActorNumber();

        /**
         * Ahora debemos colocar los jugadores en el tablero
         * */
        setPlayersInInitialPosition(0);

        /**
         * Aqui asignamos el turno correspondiente, en base al numero random que
         * sacaron cuando ingresaron a la sala
         */
        setTurnsToPlayer();

        if (PhotonNetwork.IsMasterClient)
        {
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
        time = matchTime;
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
        }
    }

    [PunRPC]
    void nextRound()
    {
        round++;
    }

    [PunRPC]
    void nextTurn()
    {
        turn++;
    }

    #endregion

    /// <summary>
    /// Asignar el elemento a los jugadores en base a su actor number
    /// </summary>
    private void setPlayerElementByActorNumber()
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            Hashtable hashtable = new Hashtable();
            Elements element = (Elements)player.ActorNumber;
            hashtable.Add("element", (int)element);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
        }
    }

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
            sortedPlayers.Add((int)player.CustomProperties["order"], player);
        }

        int localTurn = 1;
        foreach (KeyValuePair<int, Photon.Realtime.Player> player in sortedPlayers)
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("turn", localTurn);
            player.Value.SetCustomProperties(hashtable);
            localTurn++;
        }
    }
}
