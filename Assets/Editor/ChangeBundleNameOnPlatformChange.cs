using UnityEngine;
using System.Collections;
using UnityEditor;

[InitializeOnLoad]
public class ChangeBundleNameOnPlatformChange
{
    //EditorApplication.CallbackFunction callback;
    //BuildTarget previousTarget;
    static string iOSBundleId = "com.oadugmore.milesthemagnificentmole";
    static string androidBundleId = "com.oadugmore.milesthemole";

    // Use this for initialization
    static ChangeBundleNameOnPlatformChange()
    {
        EditorUserBuildSettings.activeBuildTargetChanged += new System.Action(ProjectWindowChanged);
        //callback = new EditorApplication.CallbackFunction(ProjectWindowChanged);
        //EditorApplication.projectWindowChanged += callback;
    }
    
    static void ProjectWindowChanged()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            PlayerSettings.applicationIdentifier = iOSBundleId;
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            PlayerSettings.applicationIdentifier = androidBundleId;
        }
    }

    // Update is called once per frame
    //void Update()
    //{

    //}
}
