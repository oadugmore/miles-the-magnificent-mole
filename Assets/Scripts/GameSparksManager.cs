using UnityEngine;
using System.Collections;
using GameSparks.Api.Responses;
using GameSparks.Api.Requests;
using GameSparks.Core;

/// <summary>
/// No longer in use
/// </summary>
public class GameSparksManager : MonoBehaviour
{
    /// <summary>The GameSparks Manager singleton</summary>
    public static GameSparksManager instance = null;
    private static int attempts = 0;

    //enforces singleton and registers GS callback
    void Awake()
    {
        if (instance == null) // check to see if the instance has a reference
        {
            instance = this; // if not, give it a reference to this class...
            DontDestroyOnLoad(this.gameObject); // and make this object persistent as we load new scenes
        }
        else // if we already have a reference then remove the extra manager from the scene
        {
            Destroy(this.gameObject);
        }
        GS.GameSparksAvailable += GameSparksAvailableChanged;
    }

    //public void Start()
    //{
        
    //    Invoke("GSAuthenticateDevice", 1f);
    //}

    void GameSparksAvailableChanged(bool available)
    {
        Debug.Log("GameSparks availability changed!");
        if (available)
        {
            if (!GS.Authenticated)
            {
                GSAuthenticateDevice();
            }
            else
            {
                GameController.current.OnGameSparksAuthenticated();
            }
        }
    }

    public void GSAuthenticateDevice()
    {
        attempts++;
        Debug.Log("Authenticating...");
        new DeviceAuthenticationRequest().Send((response) =>
        {
            if (!response.HasErrors)
            {
                Debug.Log("Device Authenticated...");
                GameController.current.OnGameSparksAuthenticated();
            }
            else
            {
                Debug.Log("Error Authenticating Device!");
                Debug.Log(response.Errors);
                if (attempts < 10)
                {
                    Debug.Log(string.Format("Trying again, attempt {0}...", attempts + 1));
                    Invoke("GSAuthenticateDevice", 1f);
                }
            }
        });
    } //end GSAuthenticateDevice

    //public void FixGSConnection(CallbackDelegate callback = null)
    //{
    //    if (GS.Authenticated)
    //    {
    //        if (callback != null) callback();
    //    }
    //    else if (GS.Available)
    //    {
    //        Debug.Log("Attempting to re-authenticate...");
    //        new DeviceAuthenticationRequest().Send(response =>
    //        {
    //            FixGSConnection(callback);
    //        });
    //    }
    //    else
    //    {
    //        GS.Reconnect();
            
    //        GS.GameSparksAvailable = (available) =>
    //        {
    //            if (available) FixGSConnection(callback);
    //            else if (callback != null) callback();
    //        };
    //    }
    //}

}
