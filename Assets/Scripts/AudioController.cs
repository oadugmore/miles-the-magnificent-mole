using UnityEngine;
using System.Collections;

public class AudioController : MonoBehaviour
{
    public AudioSource audioSource;
    public bool musicOn;

    public void Awake()
    {
        musicOn = SettingsManager.musicOn;
        if (!musicOn)
        {
            audioSource.volume = 0;
        }
    }

    public void StopFadeCoroutines()
    {
        StopCoroutine("Fade_Out");
        StopCoroutine("Fade_In");
    }

    public void FadeOut(float duration, bool halfway)
    {
        if (musicOn)
        {
            StopFadeCoroutines();
            StartCoroutine(Fade_Out(duration, halfway));
        }
    }

    public void FadeIn(float duration)
    {
        Debug.Log("Starting fade in...");
        if (musicOn)
        {
            StopFadeCoroutines();
            StartCoroutine(Fade_In(duration));
        }
    }

    private IEnumerator Fade_Out(float duration, bool halfway)
    {
        float endVolume = halfway ? 0.4f : 0.0f;
        float volumeThreshold = endVolume + 0.2f;

        while (audioSource.volume > endVolume)
        {
            //audio.volume -= startVolume * Time.deltaTime / duration;
            audioSource.volume -= Time.fixedDeltaTime / (duration);
            //Debug.Log("Decreased volume!");
            yield return null;
        }
        audioSource.volume = endVolume;
        Debug.Log("Finished fading out!");
        //audio.Stop();
        //audio.volume = startVolume;
    }

    private IEnumerator Fade_In(float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume < 1)
        {
            audioSource.volume += Time.fixedDeltaTime / (duration);
            //Debug.Log("Increased volume!");
            yield return null;
        }
        audioSource.volume = 1.0f;
        Debug.Log("Finished fading in!");
    }

}
