using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LeaderboardScaler : MonoBehaviour
{
    public Text referenceText;
    public RectTransform content;

    private float lastHeight = 0;

    // Use this for initialization
    void Start()
    {
        InvokeRepeating("CheckHeight", 1f, 1f);
    }

    void CheckHeight()
    {
        if (referenceText.preferredHeight != lastHeight)
        {
            lastHeight = referenceText.preferredHeight;
            content.sizeDelta = new Vector2(content.sizeDelta.x, lastHeight);
        }
    }

}
