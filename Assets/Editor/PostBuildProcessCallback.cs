using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;

//Be sure to put this script under an a folder named "Editor" in your Unity Project.

public class PostProcessBuildCallback
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        DisableAutorotateAssertion(buildTarget, pathToBuiltProject);
    }

    private static void DisableAutorotateAssertion(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS) {
            //tvOS support was introduced in 5.3.1, causing the target file to be renamed 
//#if UNITY_5_4
            string viewControllerFile = "UnityViewControllerBaseiOS.mm";
//#else
            //string viewControllerFile = "UnityViewControllerBase.mm";
            //#endif

            //include the leading tab character in the target string so we don't re-re-comment on each Build -> Append
            string targetString = "\tNSAssert(UnityShouldAutorotate()";
            string filePath = Path.Combine(pathToBuiltProject, "Classes");
            filePath = Path.Combine(filePath, "UI");
            filePath = Path.Combine(filePath, viewControllerFile); 
            if (File.Exists(filePath)) {
                string classFile = File.ReadAllText(filePath);
                string newClassFile = classFile.Replace(targetString, "\t//NSAssert(UnityShouldAutorotate()");
                if (classFile.Length != newClassFile.Length) {
                    File.WriteAllText(filePath, newClassFile);    
                    Debug.Log("Disable iOS Autorotate Assertion succeeded for file: " + filePath);
                } else {
                    Debug.LogWarning("Disable iOS Autorotate-Assertion FAILED -- Target string not found: \"" + targetString + "\"");
                }
            } else {
                Debug.LogWarning("Disable iOS Autorotate-Assertion FAILED -- File not found: " + filePath);
            }
        }
       
    }
}
