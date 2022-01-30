using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddCardsToHand : MonoBehaviour
{
    public GameObject handHolder;
    public List<GameObject> prefabs = new List<GameObject>();
    public bool generateCards = true;

    private void OnGUI()
    {
        if (generateCards)
        {
            int x = 116;
            foreach (GameObject card in prefabs)
            {
                GameObject instance = Instantiate(card);
                instance.transform.SetParent(handHolder.transform);
                instance.GetComponent<RectTransform>().position = new Vector3(x, 76, 0);
                x += 116;
            }
            generateCards = false;
        }
    }
}
