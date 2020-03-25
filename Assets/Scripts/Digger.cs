using UnityEngine;
using System.Collections;

public class Digger : MonoBehaviour
{
    private bool canDig;
    private Collider2D digCollider;
    private float lastPlayTime;
    private int playCount;
    private bool soundOn;

    public AudioSource[] digSounds;
    public float soundDelay;

    public int PlayCount
    {
        get { return playCount; }
        set { playCount = value; Debug.Log(playCount); }
    }

    // Use this for initialization
    void Start()
    {
        canDig = false;
        digCollider = GetComponent<PolygonCollider2D>();
        soundOn = SettingsManager.effectsOn;
    }

    private void PlaySoundIfReady()
    {
        if (Time.time > lastPlayTime + soundDelay)
        {
            lastPlayTime = Time.time;
            int audioIndex = Random.Range(0, digSounds.Length);
            digSounds[audioIndex].Play();
            //PlayCount++;
        }
    }

    public void SetCanDig(bool canDig)
    {
        this.canDig = canDig;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (canDig /*&& collision.gameObject.CompareTag("SoftGround")*/)
        {
            //collision.gameObject.SendMessage("InitiateDestruction");
            //Destroy(collision.gameObject);
            collision.gameObject.SetActive(false);
            if (soundOn) PlaySoundIfReady();
        }
    }
    public void OnTriggerStay2D(Collider2D collision)
    {
        if (canDig /*&& collision.gameObject.CompareTag("SoftGround")*/)
        {
            //collision.gameObject.SendMessage("InitiateDestruction");
            //Destroy(collision.gameObject);
            collision.gameObject.SetActive(false);
            if (soundOn) PlaySoundIfReady();
        }
    }

}
