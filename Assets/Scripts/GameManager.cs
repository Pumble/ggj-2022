using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum Elements : ushort { Fire = 1, Water = 2, Earth = 3, Wind = 4 };
public enum PlayerState : ushort { Death = 0, Alive = 1, Winner = 2 };

public class GameManager : MonoBehaviourPun
{
    #region VARS

    [Header("Game Manager del nivel")]

    [Header("Variables sobre la musica")]
    public GameObject musicList;

    [Header("Variables sobre la UI")]
    public int gameTurnTime = 0;
    public Text txtGameTurnTime;

    public Text txtGameResult;

    public Photon.Realtime.Player[] playersRanking;
    public List<Text> txtRankings = new List<Text>();

    public Text txtNamePlayer;
    public Slider sliderLife;
    public Text txtPA;
    public Text txtShield;
    public RawImage imgProfile;

    // Imagenes de perfil de los jugadores
    public Texture imgProfileWater = null;
    public Texture imgProfileWind = null;
    public Texture imgProfileEarth = null;
    public Texture imgProfileFire = null;

    [Header("Sobre las mecanicas")]
    public int PALimitPerPlayer = 10;

    #endregion

    public bool MatchInCourse = false;

    // Start is called before the first frame update
    void Start()
    {
        StarMusic();

        playersRanking = PhotonNetwork.PlayerList;

        // Cargar imagen de perfil
        /*
        Debug.Log("Numero del elemento" + (int)PhotonNetwork.LocalPlayer.CustomProperties["element"]);
        loadImgProfile((int)PhotonNetwork.LocalPlayer.CustomProperties["element"]);
        */
    }
    public void loadImgProfile(int element)
    {// imgProfile.GetComponent<RawImage>().texture = playerLocalHost.GetComponent<Player>().imgProfile;
        Debug.Log("El elemento que llego" + element);
        switch (element)
        {
            case 1:
                imgProfile.GetComponent<RawImage>().texture = imgProfileFire;
                break;
            case 2:
                imgProfile.GetComponent<RawImage>().texture = imgProfileWater;
                break;
            case 3:
                imgProfile.GetComponent<RawImage>().texture = imgProfileEarth;
                break;
            case 4:
                imgProfile.GetComponent<RawImage>().texture = imgProfileWind;
                break;
            default:
                imgProfile.GetComponent<RawImage>().texture = imgProfileWater;
                break;
        }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        updateStatsUI();
    }

    #region Methods

    void StarMusic()
    {
        try
        {
            AudioSource[] audios = musicList.GetComponents<AudioSource>();
            int musicRandom = Random.Range(0, audios.Length - 1);
            audios[musicRandom].Play();
        }
        catch (System.Exception)
        {
            Debug.LogError("Algo paso con la musica");
        }
    }

    private void updateStatsUI()
    {
        if (PhotonNetwork.NetworkClientState == Photon.Realtime.ClientState.Joined)
        {
            if (PhotonNetwork.LocalPlayer != null)
            {
                Photon.Realtime.Player player = PhotonNetwork.LocalPlayer;

                txtNamePlayer.text = player.NickName;
                sliderLife.value = (int)player.CustomProperties["life"];
                txtPA.text = player.CustomProperties["PA"].ToString();
                txtShield.text = player.CustomProperties["shields"].ToString();
                // imgProfile.GetComponent<RawImage>().texture = playerLocalHost.GetComponent<Player>().imgProfile;
                txtGameResult.text = gameResult();

                //Game
                txtGameTurnTime.text = gameTurnTime.ToString();

                for (int i = 0; i < playersRanking.Length; i++)
                {
                    Photon.Realtime.Player p = playersRanking[i];
                    txtRankings[i].text = p.NickName;
                }
            }
            else
            {
                Debug.LogWarning("PhotonNetwork.LocalPlayer no existe en GameManager.cs/updateStatsUI");
            }
        }
        else
        {
            Debug.Log("photonView.IsMine es falso");
        }
    }

    public void startCountDown()
    {
        StartCoroutine("setTime");
    }

    IEnumerator setTime()
    {
        yield return new WaitForSeconds(1);
        gameTurnTime -= 1;
        if (gameTurnTime > 0)
        {
            StartCoroutine("setTime");
        }
        else
        {
            StartCoroutine("endOfShift");
        }
    }

    IEnumerator endOfShift()
    { // Fin del turno
        yield return new WaitForSeconds(3);
        gameTurnTime = 30;
        StartCoroutine("setTime");
    }

    public string gameResult()
    {
        string result = "En curso";
        PlayerState state = (PlayerState)((int)PhotonNetwork.LocalPlayer.CustomProperties["state"]);

        switch (state)
        {
            case PlayerState.Winner:
                result = "WIN";
                break;
            case PlayerState.Death:
                result = "GAME OVER";
                break;
            default:
                result = "En curso";
                break;
        }
        return result;
    }

    #endregion

    #region PUN EVENTS

    [PunRPC]
    void attack(Photon.Realtime.Player from, Photon.Realtime.Player to)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int life = (int)to.CustomProperties["life"] - (int)from.CustomProperties["attack"];

            Hashtable hashtable = new Hashtable();
            hashtable.Add("life", life);
            to.SetCustomProperties(hashtable);
        }
    }

    #endregion
}
