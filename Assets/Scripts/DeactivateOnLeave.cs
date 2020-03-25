using UnityEngine;
using System.Collections;

public class DeactivateOnLeave : MonoBehaviour
{
    private bool _destroying = true;

    public void StopDestroying()
    {
        _destroying = false;
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!_destroying) return;
        //if (!collision.gameObject.CompareTag("Water") /*&& !collision.gameObject.CompareTag("Geyser")*/)
            //Destroy(collision.gameObject);
            collision.gameObject.SetActive(false);
    }
}
