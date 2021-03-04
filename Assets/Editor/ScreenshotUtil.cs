using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScreenshotUtil : EditorWindow
{
    string screenshotDir;
    int screenshotNumber;
    int screenshotScale = 1;
    string prefix = "Editor";
    // [MenuItem("Tools/Take Screenshot")]
    [MenuItem("Window/Screenshot Utility")]
    static public void ShowScreenshotUtilWindow()
    {
        // Application.CaptureScreenshot(EditorUtility.SaveFilePanel("Save Screenshot As", "", "", "png"));
        
        var util = GetWindow<ScreenshotUtil>("Screenshot Utility");
        util.screenshotDir = System.IO.Directory.GetCurrentDirectory();
        util.screenshotNumber = 1;
        // window.position = new Rect(window.position.xMin + 100f, window.position.yMin + 100f, 200f, 400f);
    }
    public void CaptureScreenshot()
    {
        var outputPath = Path.Combine(screenshotDir, prefix + Screen.currentResolution.width + "x" + Screen.currentResolution.height + screenshotNumber +".png");
        Application.CaptureScreenshot(outputPath, screenshotScale);
    }

    public IEnumerator TakeAppStoreScreenshots()
    {
        var oldResolution = Screen.currentResolution;

        // iPad Pro 3rd gen (2019) 12.9" (also accepted for iPad Pro 2nd gen)
        Screen.SetResolution(2732, 2048, false);
        CaptureScreenshot();
        yield return new WaitForEndOfFrame();

        // iPhone 5.5"
        Screen.SetResolution(2208, 1242, false);
        CaptureScreenshot();
        yield return new WaitForEndOfFrame();

        // iPhone 6.5"
        Screen.SetResolution(2688, 1242, false);
        CaptureScreenshot();
        yield return new WaitForEndOfFrame();

        Screen.SetResolution(oldResolution.width, oldResolution.height, false);
    }

    private void OnGUI()
    {
        screenshotDir = EditorGUILayout.TextField("Base directory", screenshotDir);
        screenshotNumber = EditorGUILayout.IntField("Number", screenshotNumber);
        screenshotScale = EditorGUILayout.IntField("Scale", screenshotScale);
        if (GUILayout.Button("Take screenshot"))
        {
            CaptureScreenshot();
            screenshotNumber++;
        }
    }


}
