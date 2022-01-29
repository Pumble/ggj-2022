using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHelp : MonoBehaviour
{
    // REFERENCIA
    public void OnBackScene()
    {
        SceneManager.LoadScene("GameLobby", LoadSceneMode.Single);
    }
}
