using UnityEngine;
using System.Collections;

public class GraphicsSwitcher : MonoBehaviour
{
    public Light fastLight;
    public Light fancyLight;

    // Use this for initialization
    void Start()
    {
        if (!SettingsManager.fastGraphics)
        {
            fastLight.gameObject.SetActive(false);
            fancyLight.gameObject.SetActive(true);
            QualitySettings.vSyncCount = 1;
        }
    }
    
}
