using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

static class OrientationHelper
{

    public static void LockRotation(bool locked)
    {
        if (locked)
        {
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToLandscapeLeft = false;
        }
        else
        {
            InitLandscapeSupportAndroid();
            //Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToLandscapeLeft = true;
        }
    }

    /// <summary>
    /// Does nothing if not called on Android.
    /// </summary>
    public static void InitLandscapeSupportAndroid()
    {
#if UNITY_ANDROID
        bool allowAutorotate = false;
        // Check for Android lock flag.
        allowAutorotate = AndroidRotationLockUtil.AllowAutorotation();
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = allowAutorotate;
        Screen.autorotateToLandscapeLeft = allowAutorotate;
        Screen.autorotateToLandscapeRight = allowAutorotate;
        Screen.orientation = allowAutorotate ? ScreenOrientation.AutoRotation : ScreenOrientation.Portrait;
#endif
    }

}

