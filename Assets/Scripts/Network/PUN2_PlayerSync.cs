// USING AGREGADOS
using Photon.Pun;
using UnityEngine;

public class PUN2_PlayerSync : MonoBehaviourPun, IPunObservable
{
    /*
        REFERENCIA
        JUEGO EN LINEA: https://sharpcoderblog.com/blog/make-a-multiplayer-game-in-unity-3d-using-pun-2
     */

    [Header("Lista de scripts que deberian activarse solo para el jugador local. Ejemplos: playerController, movement, cameraController, etc")]
    public MonoBehaviour[] localScripts;
    [Header("Lista de GameObjects que deberian activarse solo para jugadores locales. Ejemplos: Camera, AudioListeners, etc")]
    public GameObject[] localObjects;
    //Values that will be synced over network
    Vector3 latestPos;
    Quaternion latestRot;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
        {
            //Player is local
            // gameObject.tag = "Player";


            //Add Rigidbody to make the player interact with rigidbody
            // Rigidbody r = gameObject.AddComponent<Rigidbody>();
            // Rigidbody r = gameObject.GetComponentInChildren<Rigidbody>();
            // r.isKinematic = true;
        }
        else
        {
            //Player is Remote, deactivate the scripts and object that should only be enabled for the local player
            for (int i = 0; i < localScripts.Length; i++)
            {
                localScripts[i].enabled = false;
            }
            for (int i = 0; i < localObjects.Length; i++)
            {
                localObjects[i].SetActive(false);
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            //Network player, receive data
            latestPos = (Vector3)stream.ReceiveNext();
            latestRot = (Quaternion)stream.ReceiveNext();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            //Update remote player (smooth this, this looks good, at the cost of some accuracy)
            transform.position = Vector3.Lerp(transform.position, latestPos, Time.deltaTime * 5);
            transform.rotation = Quaternion.Lerp(transform.rotation, latestRot, Time.deltaTime * 5);
        }
    }
}
