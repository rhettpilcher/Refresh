using UnityEngine;
using System.Runtime.InteropServices;

public class GetWebsiteDomain : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern string GetDomain();

    public string GetDomainName()
    {
        return GetDomain();
    }

    public string GetDomainNameApp()
    {
        return Application.absoluteURL;
    }
}