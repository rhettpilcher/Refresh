using UnityEngine;

public class AspectRatioLockStandAlone : MonoBehaviour
{
    /*
     * Locks aspect ratio in a stand alone build
     */

    [SerializeField] private Vector2 aspectRatio = new Vector2(16f, 9f);

    private void Update()
    {
        float targetAspectRatio = aspectRatio.x / aspectRatio.y;
        float currentAspectRatio = (float)Screen.width / Screen.height;

        if (currentAspectRatio > targetAspectRatio)
        {
            int newWidth = Mathf.RoundToInt(Screen.height * targetAspectRatio);
            Screen.SetResolution(newWidth, Screen.height, true);
        }
        else
        {
            int newHeight = Mathf.RoundToInt(Screen.width / targetAspectRatio);
            Screen.SetResolution(Screen.width, newHeight, true);
        }
    }
}