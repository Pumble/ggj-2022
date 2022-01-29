using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region Public files

    public string nickname = null;
    public int order;
    public int life = 100;
    public int attack = 10;
    public int currentSlot = 0;
    public int positionInSlot = 0;
    public GameManager gameManager;

    #endregion

    public Player()
    {
        this.nickname = null;
        this.order = 0;
        this.life = 100;
        this.attack = 10;
        this.currentSlot = 0;
        this.positionInSlot = 0;
        this.gameManager = null;
    }

    public void move(int from, int to)
    {
        Slot slotFrom = gameManager.slots[from].GetComponent<Slot>();
        Slot slotTo = gameManager.slots[to].GetComponent<Slot>();

        Vector3 fromPosition = gameManager.slots[0].transform.position + slotFrom.getPlayerLocation(this.positionInSlot);
        int nextFreePosition = slotTo.getFreePosition();
        Vector3 toPosition = gameManager.slots[2].transform.position + slotTo.getLocationByIndex(nextFreePosition);

        // Remove from old position to the new one
        slotFrom.removePlayerFromLocationByIndex(this.positionInSlot);
        this.positionInSlot = nextFreePosition;

        StartCoroutine(moveToken(3f, fromPosition, toPosition, this.transform));
    }

    IEnumerator moveToken(float time, Vector3 from, Vector3 to, Transform transform)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(from, to, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
