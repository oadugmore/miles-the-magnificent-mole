using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class SettingsManager : MonoBehaviour
{

    public static void Initialize()
    {
        if (!PlayerPrefs.HasKey("TouchControls"))
        {
            if (Input.touchSupported)
                touchControlsEnabled = true;
            else
                touchControlsEnabled = false;
        }
        if (!PlayerPrefs.HasKey("EasyMode"))
            easyMode = true;
        //if (!PlayerPrefs.HasKey("UploadScore"))
        //    uploadScore = true;
        if (!PlayerPrefs.HasKey("FastGraphics"))
            fastGraphics = true;
        if (!PlayerPrefs.HasKey("MusicOn"))
            musicOn = true;
        if (!PlayerPrefs.HasKey("EffectsOn"))
            effectsOn = true;
        if (!PlayerPrefs.HasKey("LoadCount"))
            loadCount = 0;
        if (!PlayerPrefs.HasKey("RateCompleted"))
            rateCompleted = false;
        if (!PlayerPrefs.HasKey("ReminderCount"))
            nextReminderLoadCount = 3;

        loadCount++;
    }

    #region Analytics
    /// <summary>
    /// Incremented each time SettingsManager is initialized.
    /// </summary>
    public static int loadCount
    {
        get { return PlayerPrefs.GetInt("LoadCount"); }
        set
        {
            PlayerPrefs.SetInt("LoadCount", value);
        }
    }

    public static bool rateCompleted
    {
        get { return (PlayerPrefs.GetInt("RateCompleted") == 1); }
        set
        {
            PlayerPrefs.SetInt("RateCompleted", value ? 1 : 0);
        }
    }

    public static int nextReminderLoadCount
    {
        get { return (PlayerPrefs.GetInt("ReminderCount")); }
        set
        {
            PlayerPrefs.SetInt("ReminderCount", value);
        }
    }
    #endregion

    public static bool touchControlsEnabled
    {
        get { return (PlayerPrefs.GetInt("TouchControls") == 1); }
        set
        {
            PlayerPrefs.SetInt("TouchControls", value ? 1 : 0);
            CrossPlatformInputManager.SwitchActiveInputMethod(value ? CrossPlatformInputManager.ActiveInputMethod.Touch : CrossPlatformInputManager.ActiveInputMethod.Hardware);
        }
    }

    public static bool easyMode
    {
        get { return (PlayerPrefs.GetInt("EasyMode") == 1); }
        set
        {
            PlayerPrefs.SetInt("EasyMode", value ? 1 : 0);
        }
    }

    //public static bool uploadScore
    //{
    //    get { return (PlayerPrefs.GetInt("UploadScore") == 1); }
    //    set
    //    {
    //        PlayerPrefs.SetInt("UploadScore", value ? 1 : 0);
    //    }
    //}

    public static bool fastGraphics
    {
        get { return (PlayerPrefs.GetInt("FastGraphics") == 1); }
        set
        {
            PlayerPrefs.SetInt("FastGraphics", value ? 1 : 0);
        }
    }

    public static bool musicOn
    {
        get { return (PlayerPrefs.GetInt("MusicOn") == 1); }
        set
        {
            PlayerPrefs.SetInt("MusicOn", value ? 1 : 0);
        }
    }

    public static bool effectsOn
    {
        get { return (PlayerPrefs.GetInt("EffectsOn") == 1); }
        set
        {
            PlayerPrefs.SetInt("EffectsOn", value ? 1 : 0);
        }
    }

}