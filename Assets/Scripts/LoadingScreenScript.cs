using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadingScreenScript : MonoBehaviour
{
    public enum ButtonHandlerType
    {
        Menu, Game
    }

    public ButtonHandlerType buttonHandlerType;

    public void FadeInComplete()
    {
        if (buttonHandlerType == ButtonHandlerType.Game)
            GameController.current.gameSceneManager.FadeInComplete();
        else
            GameController.current.menuSceneManager.FadeInComplete();
    }

    // Use this for initialization
    //void Start()
    //{

    //}

    //public void Awake()
    //{
    //    Color c = GetComponent<Image>().color;
    //    c.a = 1;
    //    GetComponent<Image>().color = c;
    //}
}
