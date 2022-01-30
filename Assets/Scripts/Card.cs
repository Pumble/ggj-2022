using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.Runtime;

/*
 * REFERENCIAS
 * CustomKeyValuePair en el editor: https://stackoverflow.com/questions/48969329/how-to-show-the-following-keyvaluepair-in-the-unity-editorso-that-it-is-editabl
 */

public enum CardRenderType : ushort { Image = 1, Video = 2 };
public enum CardType : ushort { Enviroment = 1, Trap = 2 };

[System.Serializable]
public class CustomKeyValuePair<TKey, TValue>
{
    public CustomKeyValuePair()
    {
    }

    public CustomKeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }

    [field: SerializeField] public TKey Key { set; get; }
    [field: SerializeField] public TValue Value { set; get; }
}

public class Card : MonoBehaviour
{
    #region VARS

    public string title;
    public List<Elements> elements;
    public CardRenderType renderType;
    public CardType type;

    private VideoPlayer videoPlayer;
    private GameObject sprite;

    #endregion

    private void Awake()
    {
        videoPlayer = GetComponentInChildren<VideoPlayer>();
        if (renderType == CardRenderType.Image)
        {
            videoPlayer.enabled = false;
        }
        else if (renderType == CardRenderType.Video)
        {
            videoPlayer.enabled = true;
        }

        // Display the tooltip from the element that has mouseover or keyboard focus
        GUI.Label(new Rect(10, 40, 100, 40), GUI.tooltip);
    }

    public List<CustomKeyValuePair<Elements, int>> getStats()
    {
        List<CustomKeyValuePair<Elements, int>> stats = new List<CustomKeyValuePair<Elements, int>>();

        foreach (Elements element in (Elements[])System.Enum.GetValues(typeof(Elements)))
        {
            if (elements.IndexOf(element) != -1)
            {
                stats.Add(new CustomKeyValuePair<Elements, int>(element, 10));
            }
            else
            {
                stats.Add(new CustomKeyValuePair<Elements, int>(element, 0));
            }
        }
        return stats;
    }

    public void OnClick_SelectCard()
    {
        Debug.Log("carta seleccionada: " + title);
    }
}
