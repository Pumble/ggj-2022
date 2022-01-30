using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOver : MonoBehaviour
{
    private Transform transform;
    private Slot slot;
    public int boardSize = 40;
    public float movement = 0.2f;
    private int splitIn;
    private Vector3 originalPosition;

    private void Awake()
    {
        transform = GetComponent<Transform>();
        slot = GetComponent<Slot>();
        int splitIn = boardSize / 4;
        originalPosition = transform.position;
    }

    void OnMouseOver()
    {

        if (slot.index >= splitIn * 3)
        {
            transform.position = new Vector3(transform.position.x - movement, transform.position.y, transform.position.z);
        }
        else if (slot.index >= splitIn * 2)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + movement);
        }
        else if (slot.index >= splitIn)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - movement);
        }
        else
        {
            transform.position = new Vector3(transform.position.x + movement, transform.position.y, transform.position.z);
        }
    }

    void OnMouseExit()
    {
        transform.position = originalPosition;
    }
}
