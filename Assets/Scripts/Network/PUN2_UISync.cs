// USING AGREGADOS
using Photon.Pun;

public class PUN2_UISync : MonoBehaviourPun, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // SOLO EL MASTER PUEDE ESCRIBIR
            if (stream.IsWriting)
            {
                //stream.SendNext(GameManager.PtsKidsTeam);
                //stream.SendNext(GameManager.PtsAdultsTeam);
                //stream.SendNext(GameManager.MatchInCourse);
            }
        }
        if (stream.IsReading)
        {
            //Network player, receive data
            //GameManager.PtsKidsTeam = (int)stream.ReceiveNext();
            //GameManager.PtsAdultsTeam = (int)stream.ReceiveNext();
            //GameManager.MatchInCourse = (bool)stream.ReceiveNext();
        }
    }
}
