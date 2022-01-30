using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// USING AGREGADOS
using UnityEngine.EventSystems;

public class MouseOverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float movement = 15f;
    RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        rectTransform.position = new Vector3(
            rectTransform.position.x,
            rectTransform.position.y + movement,
            rectTransform.position.z
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        rectTransform.position = new Vector3(
            rectTransform.position.x,
            rectTransform.position.y - movement,
            rectTransform.position.z
        );
    }
}
