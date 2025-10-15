using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabButton : MonoBehaviour
{
    /*
     * Special interactable button that opens a new instance of the game in the web browser
     */

    bool boolVar;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !boolVar)
        {
            StartCoroutine(OpenTab(0.1f));
        }
    }

    public IEnumerator OpenTab(float waitTime)
    {
        boolVar = true;
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(waitTime);
        Application.OpenURL("https://ecremprown.itch.io/refresh");
        boolVar = false;
    }
}
