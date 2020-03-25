using UnityEngine;
using GameSparks.Api.Requests;

[System.Obsolete]
public class LoginManager : MonoBehaviour 
{
    public void Start()
    {
        Invoke("GSAuthenticateDevice", 0.5f);
    }

    public void GSAuthenticateDevice()
    {
        new DeviceAuthenticationRequest().Send((response) =>
        {
            if (!response.HasErrors)
            {
                Debug.Log("Device Authenticated...");
            }
            else
            {
                Debug.Log("Error Authenticating Device...");
            }
        });
    }

    //if (!FB.IsInitialized)
    //{
    //    FB.Init(FacebookLogin);
    //    //FacebookLogin();
    //}
    //else
    //{
    //    FacebookLogin();
    //}


    //public void FacebookLogin()
    //{
    //    if (!FB.IsLoggedIn)
    //    {
    //        FB.Login("", GameSparksLogin);
    //    }
    //}

    //public void GameSparksLogin(FBResult result)
    //{
    //    if (FB.IsLoggedIn)
    //    {
    //        new FacebookConnectRequest().SetAccessToken(FB.AccessToken).Send((response) =>
    //        {
    //            if (response.HasErrors)
    //            {
    //                Debug.Log("Something failed when connecting to Facebook");
    //            }
    //            else
    //            {
    //                Debug.Log("GameSparks Facebook login successful");
    //            }
    //        });
    //    }
    //}

}