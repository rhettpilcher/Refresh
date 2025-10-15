using System.Runtime.InteropServices;
using UnityEngine;

public class LocalStorageManager : MonoBehaviour
{ 
    [DllImport("__Internal")]
    private static extern void SetLocalStorage(string key, string value);

    [DllImport("__Internal")]
    private static extern string GetLocalStorage(string key);

    [DllImport("__Internal")]
    private static extern void InitializeLocalStorageListener();

    [DllImport("__Internal")]
    private static extern void CloseWindow();

    void Start()
    {
        InitializeLocalStorageListener();
    }

    public void SetValue(string key, string value)
    {
        SetLocalStorage(key, value);
    }

    public string GetValue(string key)
    {
        return GetLocalStorage(key);
        //return "0";
    }

    public void CloseTab()
    {
        CloseWindow();
    }
}
