#if UNITY_EDITOR// Guard against accidental inclusion in builds
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

/// <summary>
/// Pulls the latest commit of the Gamecraft package once per Unity session.
/// Compatible with Unity 2022.3 LTS.
/// </summary>
[InitializeOnLoad]
internal static class AutoUpdateGamecraftPackage
{
    static ListRequest _listRequest;
    static AddRequest _addRequest;

    // Static ctor is executed on every domain reload; we gate with SessionState
    const string SessionKey = "Gamecraft.PackageUpdated";
    static AutoUpdateGamecraftPackage()
    {
        if (SessionState.GetBool(SessionKey, false))
            return; // Already ran in this Editor session
        SessionState.SetBool(SessionKey, true);
        // Delay one frame so Unity finishes its own UPM resolve first
        EditorApplication.delayCall += BeginUpdate;
    }

    /// <summary>Kick off a List() request; when it finishes, UPM is idle.</summary>
    static void BeginUpdate()
    {
        _listRequest = Client.List(true, false);
        EditorApplication.update += WaitForList;
    }

    /// <summary>When List() is done, start the Add() request.</summary>
    static void WaitForList()
    {
        if (!_listRequest.IsCompleted) return;

        // Even if list failed, we still try to add/refresh the package.
        _addRequest = Client.Add("https://github.com/gamecraft-ai/shipping-unity-package.git");

        EditorApplication.update -= WaitForList;
        EditorApplication.update += WaitForAdd;
    }

    /// <summary>Poll Add() until it finishes, then log and clean up.</summary>
    static void WaitForAdd()
    {
        if (!_addRequest.IsCompleted) return;

        if (_addRequest.Status == StatusCode.Success)
        {
            //Debug.Log($"[Gamecraft] Package updated.");
        }
        else
        {
            Debug.LogWarning($"[Gamecraft] Package update failed: {_addRequest.Error.message}");
        }

        EditorApplication.update -= WaitForAdd;
        _listRequest = null;
        _addRequest = null;
    }
}
#endif