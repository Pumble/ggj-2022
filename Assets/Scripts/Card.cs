using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

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
    public Elements element;
    public CustomKeyValuePair<Elements, int>[] stats;
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
    }
}
