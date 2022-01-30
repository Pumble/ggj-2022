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

    public GameObject[] playersRanking;
    public List<Text> txtRankings = new List<Text>();

    public Text txtNamePlayer;
    public Slider sliderLife;
    public Text txtPA;
    public Text txtShield;
    public RawImage imgProfile;

    [Header("Sobre las mecanicas")]
    public int PALimitPerPlayer = 10;

    [Header("Sobre las cartas")]
    public List<GameObject> availableCards = new List<GameObject>();
    public int cardsPerTurn = 4;

    #endregion

    public bool MatchInCourse = false;

    // Start is called before the first frame update
    void Start()
    {
        StarMusic();
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

                for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                {
                    Photon.Realtime.Player p = PhotonNetwork.PlayerList[i];
                    txtRankings[i].text = p.NickName;
                }
            }
            else
            {
                Debug.LogWarning("PhotonNetwork.LocalPlayer no existe en GameManager.cs/updateStatsUI");
            }
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

    #region Cards Methods

    public List<GameObject> getCardsByType(Elements element)
    {
        List<GameObject> cardsPerElement = new List<GameObject>();
        foreach (GameObject go in availableCards)
        {
            Card card = go.GetComponent<Card>();
            if (card.type == CardType.Trap)
            {
                // SI ES TRAMPA LA ASIGNAMOS
                cardsPerElement.Add(go);
            }
            else
            {
                if (card.elements.IndexOf(element) != -1)
                {
                    // SI LA CARTA TIENE ESTE ELEMENTO, LA ASIGNAMOS
                    cardsPerElement.Add(go);
                }
            }
        }

        // AHORA YA TENEMOS UNA LISTA DE CARTAS DISPONIBLES, HAY
        // QUE ESCOGER AL AZAR ENTRE ELLAS Y DEVOLVER LA LISTA REAL
        List<GameObject> cardsToSend = new List<GameObject>();
        if (cardsPerElement.Count > 0)
        {
            for (int i = 0; i < cardsPerTurn; i++)
            {
                int randomIndex = UnityEngine.Random.Range(0, cardsPerElement.Count);
                cardsToSend.Add(cardsPerElement[randomIndex]);
            }
        }
        else
        {
            Debug.Log("No se encontraron cartas con el elemento dado para entregar");
        }
        return cardsToSend;
    }

    #endregion
}
