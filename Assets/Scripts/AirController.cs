using UnityEngine;
using System.Collections;

[System.Obsolete]
public class AirController : MonoBehaviour
{

    #region Properties
    public GameObject[] clouds;
    public GameObject raindrop;

    public bool isRaining { get; private set; }

    #endregion

    private void GenerateRaindrops()
    {
        foreach (GameObject cloud in clouds)
        {
            int posIndex = Random.Range(0, 3);
            Object.Instantiate(raindrop, cloud.GetComponentsInChildren<Transform>()[posIndex].position, Quaternion.identity);
        }
        
    }

    private IEnumerator RainCoroutine()
    {
        while (isRaining)
        {
            GenerateRaindrops();
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void BeginRaining()
    {
        isRaining = true;
        StartCoroutine(RainCoroutine());
    }

    public void StopRaining()
    {
        isRaining = false;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
