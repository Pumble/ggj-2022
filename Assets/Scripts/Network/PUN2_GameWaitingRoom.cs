// USING AGREGADOS
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PUN2_GameWaitingRoom : MonoBehaviourPunCallbacks
{
    Vector2 playerListScroll = Vector2.zero;
    public int timeToBegin = 5;
    private int time;
    public Text timeToBeginLabel;

    private bool waitingForMatch = false;

    public AudioSource audio;

    private void Start()
    {
        //In case we started this demo with the wrong scene being active, simply load the menu scene
        if (PhotonNetwork.CurrentRoom == null)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");
            return;
        }
        timeToBeginLabel.enabled = false;
        time = timeToBegin;

        if (audio != null)
        {
            audio.volume = 0.3f;
            audio.Play();
        }
    }

    #region OVERRIDES

    public override void OnLeftRoom()
    {
        //We have left the Room, return back to the GameLobby
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameLobby");

        if (audio != null)
        {
            audio.Stop();
        }
    }

    /*
    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        if (changedProps.ContainsKey("team"))
        {
            int tipo = (int)changedProps["team"];
            Debug.Log("Properties of player " + targetPlayer.NickName + " updated. Tipo: " + tipo);
        }
    }
    */
    #endregion

    #region UI

    void OnGUI()
    {
        GUI.Window(0, new Rect(Screen.width / 2 - 450, Screen.height / 2 - 200, 900, 400), LobbyWindow, "Esperando jugadores");
    }

    void LobbyWindow(int index)
    {
        if (!waitingForMatch)
        {
            //Connection Status and Room creation Button
            GUILayout.BeginHorizontal();
            GUILayout.Label("Conexión: " + PhotonNetwork.NetworkClientState);

            if (PhotonNetwork.IsConnected)
            {
                GUILayout.FlexibleSpace();

                GUILayout.Label("Sala: " + PhotonNetwork.CurrentRoom.Name);

                // CHECKEAR EL ESTADO DEL JUGADOR LOCAL
                if (areYouReady())
                {
                    if (GUILayout.Button("No estoy listo", GUILayout.Width(125)))
                        amIReady(false);
                }
                else
                {
                    if (GUILayout.Button("¡Estoy listo!", GUILayout.Width(125)))
                        amIReady(true);
                }

                // CONDICIONES PARA QUE EL MASTER PUEDA INICIAR PARTIDA
                if (PhotonNetwork.IsMasterClient)
                    if (PhotonNetwork.PlayerList.Length >= 2)
                        if (isEveryoneReady())
                            if (GUILayout.Button("¡Iniciar partida!", GUILayout.Width(125)))
                            {
                                // setCharacterTypes();
                                PhotonView photonView = PhotonView.Get(this);
                                photonView.RPC("BeginTimeWaiting", RpcTarget.All, "algo");
                            }

                GUILayout.EndHorizontal();

                //Scroll through available rooms
                playerListScroll = GUILayout.BeginScrollView(playerListScroll, true, true);

                if (PhotonNetwork.PlayerList.Length == 0)
                    GUILayout.Label("No hay jugadores por ahora...");
                else
                {
                    for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                    {
                        GUILayout.BeginHorizontal("box");
                        GUILayout.Label(PhotonNetwork.PlayerList[i].NickName, GUILayout.Width(300));
                        GUILayout.Label(PhotonNetwork.PlayerList[i].IsMasterClient ? "Master" : "-", GUILayout.Width(100));
                        GUILayout.Label(areYouReady(PhotonNetwork.PlayerList[i]) ? "Listo!" : "Atrasando", GUILayout.Width(100));
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }

                GUILayout.EndScrollView();

                // FOOTER
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Salir de la sala", GUILayout.Width(150)))
                {
                    PhotonNetwork.LeaveRoom();
                }
                GUILayout.EndHorizontal();
            }
            else
                GUILayout.EndHorizontal();
        }
    }

    #endregion

    #region UTILS

    private bool areYouReady(Photon.Realtime.Player player = null)
    {
        if (player != null)
        {
            if (player.CustomProperties.ContainsKey("ready"))
                return (bool)player.CustomProperties["ready"];
            else
                return false;
        }
        else
        {
            if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("ready"))
                return (bool)PhotonNetwork.LocalPlayer.CustomProperties["ready"];
            else
                return false;
        }
    }

    private void amIReady(bool ready)
    {
        Hashtable hashtable = new Hashtable();
        hashtable.Add("ready", ready);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
    }

    private bool isEveryoneReady()
    {
        if (PhotonNetwork.PlayerList.Length >= 2) // NECESITAMOS UN MINIMO DE 2 JUGADORES
        {
            bool weAreReady = false;
            int i = 0;
            do
            {
                weAreReady = areYouReady(PhotonNetwork.PlayerList[i]);
                i++;
            } while (weAreReady == true && i < PhotonNetwork.PlayerList.Length);
            return weAreReady;
        }
        else
            return false; // NO HAY NADIE O MUY POCOS EN LA SALA, NO PODEMOS INICIAR ASI
    }

    #endregion

    void updateTimeWaiting()
    {
        time--;
        timeToBeginLabel.text = time.ToString();
        if (time <= 0)
        {
            if (audio != null)
            {
                audio.Stop();
            }
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel("GameLevel_Juan");
            CancelInvoke("updateTimeWaiting");
        }
    }

    [PunRPC]
    void BeginTimeWaiting(string test)
    {
        waitingForMatch = true;
        timeToBeginLabel.enabled = true;
        InvokeRepeating("updateTimeWaiting", 1f, 1f);
    }
}