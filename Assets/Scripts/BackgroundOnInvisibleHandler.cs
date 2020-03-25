using UnityEngine;
using System.Collections;
using System;

public class BackgroundOnInvisibleHandler : MonoBehaviour
{
    //public BackgroundController controller;
    public int index;
    public int height;

    private Transform mainCamera;

    public void Awake()
    {
        mainCamera = FindObjectOfType<Camera>().transform;
    }

    public void OnBecameInvisible()
    {
        if (!mainCamera) return;
        
        if (transform.position.y > mainCamera.position.y)
        {
            JumpDown();
            Debug.Log("Background " + index + " jumped down.");
        }
        else
        {
            JumpUp();
            Debug.Log("Background " + index + " jumped up.");
        }
        //if (controller.IsJumpDownNext(index))
        //{
        //    JumpDown();
        //}
        //else
        //{
        //    JumpUp();
        //}
    }

    private void JumpDown()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y - height * 2, transform.position.z);
    }

    private void JumpUp()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + height * 2, transform.position.z);
    }
}
