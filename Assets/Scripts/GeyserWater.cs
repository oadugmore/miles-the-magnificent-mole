using UnityEngine;
using System.Collections;

public class GeyserWater : MonoBehaviour
{
    public Transform parent;

    // Use this for initialization
    void Start()
    {
        //Debug.LogWarning("This scene has a DestroyOnContact object. We should remove this.");
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("SoftGround") && collision.gameObject.transform.position.y > parent.position.y)
        {
            collision.gameObject.SetActive(false);
        }
            //Destroy(collision.gameObject);
            //Debug.LogWarning("DestroyOnContact occurred. We should remove this.");
    }
}
