using UnityEngine;
using System.Collections;

public interface ILogDebugInfo
{
    bool debugEnabled { get; set; }
}

public class DebugHelper : MonoBehaviour
{
    private bool _enableAll;
    public bool enableAll;

    void Update()
    {
        if (enableAll != _enableAll)
        {
            foreach (ILogDebugInfo debugObject in debugObjects)
            {
                debugObject.debugEnabled = enableAll;
            }
            _enableAll = enableAll;
        }
    }

    public ILogDebugInfo[] debugObjects;
}
