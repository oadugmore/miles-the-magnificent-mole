using UnityEngine;
using System.Collections;

public class GeyserController : MonoBehaviour
{
    private Transform player;
    //private float posY;
    private float trigger;
    private Animator anim;
    private bool disabling = false;
    private bool activated = false;
    private int index;
    private int _triggerDistance;
    private int _shortTriggerDistance;
    private bool _useShortTriggerDistance;

    private static Transform playerTransform;
    private static int count = 0;

    // Use this for initialization
    void Start()
    {
        count++;
        index = count;
        if (!playerTransform)
        {
            playerTransform = FindObjectOfType<PlayerController>().GetComponent<Transform>();
        }
        player = playerTransform;
        anim = GetComponent<Animator>();
        _triggerDistance = GameController.current.gameSceneManager.geyserTriggerDistance;
        _shortTriggerDistance = GameController.current.gameSceneManager.geyserShortTriggerDistance;
        //CalculateTrigger();
    }

    private void CalculateTrigger()
    {
        trigger = (_useShortTriggerDistance ? _shortTriggerDistance : _triggerDistance) + transform.position.y;
        //trigger = _triggerDistance + transform.position.y;
        //Debug.Log("Using trigger distance: " + (_triggerDistance));
        activated = false;
    }

    public void CalculateTriggerNextFrame(bool shortDistance = false)
    {
        
        Invoke("CalculateTrigger", 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!activated && player.position.y < trigger)
        {
            Activate();
        }
    }

    void Activate()
    {
        anim.SetTrigger("Activate");
        activated = true;
        //Debug.Log("Activated geyser " + index);
    }

    private void Deactivate()
    {
        if (anim)
        {
            gameObject.SetActive(true);
            anim.SetTrigger("Deactivate");
            //Invoke("Disable", 2f);
            //Debug.Log("Deactivated geyser " + index);
            //yield return null;
            Invoke("Disable", 0f);
            //gameObject.SetActive(false);
            //gameObject.SetActive(true);
        }
    }

    private void Disable()
    {
        gameObject.SetActive(false);
        disabling = false;
    }

    //void Disable()
    //{
    //    //deactivating = true;
    //    gameObject.SetActive(false);
    //    //deactivating = false;
    //}

    public void OnDisable()
    {
        if (!disabling)
        {
            disabling = true;
            //gameObject.SetActive(true);
            Invoke("Deactivate", 0f);
        }
    }

    //public void OnEnable()
    //{
    //var childrens = gameObject.GetComponentsInChildren<Transform>(true);
    //foreach (Transform t in childrens)
    //{
    //    t.gameObject.SetActive(true);
    //}
    //block.GetComponent<GeyserController>().Deactivate();
    //}

    //public void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.gameObject.CompareTag("SoftGround") && collision.gameObject.transform.position.y > gameObject.transform.position.y)
    //    {
    //        //collision.gameObject.SendMessage("InitiateDestruction");
    //        //Destroy(collision.gameObject);
    //        collision.gameObject.SetActive(false);
    //    }
    //}

}
