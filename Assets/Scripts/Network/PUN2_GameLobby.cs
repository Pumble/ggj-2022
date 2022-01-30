// USING AGREGADOS
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PUN2_GameLobby : MonoBehaviourPunCallbacks
{
    //Our player name
    string playerName = "Jugador 1";
    //Users are separated from each other by gameversion (which allows you to make breaking changes).
    string gameVersion = "1";
    //The list of created rooms
    List<RoomInfo> createdRooms = new List<RoomInfo>();
    //Use this name when creating a Room
    string roomName = "Sala 1";
    Vector2 roomListScroll = Vector2.zero;
    bool joiningRoom = false;

    // Use this for initialization
    void Start()
    {
        //This makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {
            //Set the App version before connecting
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    #region OVERRIDES

    public override void OnConnectedToMaster()
    {
        //After we connected to Master server, join the Lobby
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //After this callback, update the room list
        createdRooms = roomList;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
        joiningRoom = false;
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
        joiningRoom = false;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed got called. This can happen if the room is not existing or full or closed.");
        joiningRoom = false;
    }

    public override void OnCreatedRoom()
    {
        prepareLocalPlayerForMatch();

        //Load the Scene called GameLevel (Make sure it's added to build settings)
        PhotonNetwork.LoadLevel("GameWaitingRoom");
    }

    #endregion

    #region UI

    void OnGUI()
    {
        GUI.Window(0, new Rect(Screen.width / 2 - 450, Screen.height / 2 - 200, 900, 400), LobbyWindow, "Lobby");
    }

    void LobbyWindow(int index)
    {
        //Connection Status and Room creation Button
        GUILayout.BeginHorizontal();

        GUILayout.Label("Conexi�n: " + PhotonNetwork.NetworkClientState);

        if (joiningRoom || !PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
        {
            GUI.enabled = false;
        }

        GUILayout.FlexibleSpace();

        //Room name text field
        roomName = GUILayout.TextField(roomName, GUILayout.Width(250));

        if (GUILayout.Button("Crear sala", GUILayout.Width(125)))
        {
            if (roomName != "")
            {
                joiningRoom = true;

                RoomOptions roomOptions = new RoomOptions();
                roomOptions.IsOpen = true;
                roomOptions.IsVisible = true;
                roomOptions.MaxPlayers = (byte)4; //Set any number

                PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
            }
        }

        GUILayout.EndHorizontal();

        //Scroll through available rooms
        roomListScroll = GUILayout.BeginScrollView(roomListScroll, true, true);

        if (createdRooms.Count == 0)
        {
            GUILayout.Label("No hay salas por ahora...");
        }
        else
        {
            for (int i = 0; i < createdRooms.Count; i++)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label(createdRooms[i].Name, GUILayout.Width(400));
                GUILayout.Label(createdRooms[i].PlayerCount + "/" + createdRooms[i].MaxPlayers);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Unirse"))
                {
                    joiningRoom = true;

                    prepareLocalPlayerForMatch();

                    //Join the Room
                    PhotonNetwork.JoinRoom(createdRooms[i].Name);
                }
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndScrollView();

        //Set player name and Refresh Room button
        GUILayout.BeginHorizontal();

        GUILayout.Label("Nickname: ", GUILayout.Width(85));
        //Player name text field
        playerName = GUILayout.TextField(playerName, GUILayout.Width(250));

        GUILayout.FlexibleSpace();

        GUI.enabled = (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby || PhotonNetwork.NetworkClientState == ClientState.Disconnected) && !joiningRoom;
        if (GUILayout.Button("Refrescar", GUILayout.Width(100)))
        {
            if (PhotonNetwork.IsConnected)
            {
                //Re-join Lobby to get the latest Room list
                PhotonNetwork.JoinLobby(TypedLobby.Default);
            }
            else
            {
                //We are not connected, estabilish a new connection
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        if (GUILayout.Button("�C�mo jugar?", GUILayout.Width(140)))
        {
            SceneManager.LoadScene("GameHelp", LoadSceneMode.Single);
        }

        GUILayout.EndHorizontal();

        if (joiningRoom)
        {
            GUI.enabled = true;
            GUI.Label(new Rect(900 / 2 - 50, 400 / 2 - 10, 100, 20), "Conectando...");
        }
    }

    #endregion

    private void prepareLocalPlayerForMatch()
    {
        PhotonNetwork.NickName = playerName;
        Hashtable hashtable = new Hashtable();
        hashtable.Add("ready", false);
        hashtable.Add("attack", 0);
        hashtable.Add("life", 0);
        hashtable.Add("slot", 0);
        hashtable.Add("element", 0);
        hashtable.Add("order", UnityEngine.Random.Range(1, 100));
        hashtable.Add("turn", 1);
        hashtable.Add("PA", 4);
        hashtable.Add("shields", 0);
        hashtable.Add("laps", 0);
        hashtable.Add("state", 1);
        hashtable.Add("PAperTurn", 4);
        hashtable.Add("ranking", 0);
        PhotonNetwork.SetPlayerCustomProperties(hashtable);
    }
}