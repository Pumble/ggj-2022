using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOver : MonoBehaviour
{
    public Transform transform;

    private void Awake()
    {
        transform = GetComponent<Transform>();
    }

    void OnMouseOver()
    {
        transform.position = new Vector3(transform.position.x, 0.125f, transform.position.z);
    }

    void OnMouseExit()
    {
        transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
    }
}
