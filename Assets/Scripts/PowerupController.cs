using UnityEngine;
using System.Collections;

public class PowerupController : MonoBehaviour
{
    private Animator anim;
    private Collider2D coll;
    private bool disabling = false;

    public void Start()
    {
        anim = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
    }

    public void OnCollected()
    {
        //Destroy(gameObject, 1f);
        //gameObject.SetActive(true);
        Invoke("Reset", 1f);
        anim.SetTrigger("Collected");
        disabling = true;
        coll.enabled = false;
    }

    public void OnDisable()
    {
        //gameObject.SetActive(true);
        if (!disabling)
        {
            Invoke("Reset", 0f);
            disabling = true;
        }
        //OnCollected();
    }

    //private void Remove()
    //{
    //    Reset();
    //    //gameObject.SetActive(false);
    //}

    private void Reset()
    {
        if (anim != null)
        {
            gameObject.SetActive(true);
            anim.SetTrigger("Reset");
            Invoke("Disable", 0f);
            //yield return null;
            //gameObject.SetActive(false);
        }
    }

    private void Disable()
    {
        gameObject.SetActive(false);
        coll.enabled = true;
        disabling = false;
    }

}