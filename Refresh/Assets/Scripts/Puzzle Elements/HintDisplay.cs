using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintDisplay : MonoBehaviour
{
    /*
     * Loops through sprites added to sprites array, used to display hints on murals
     */

    public Sprite[] sprites = new Sprite[0];
    public float time;
    private int index = 0;

    private void Start()
    {
        StartCoroutine(Loop(time));
    }

    private IEnumerator Loop(float waitTime)
    {
        transform.GetComponent<SpriteRenderer>().sprite = sprites[index];
        yield return new WaitForSeconds(waitTime);

        if (index == sprites.Length - 1)
            index = 0;
        else
            index += 1;

        StartCoroutine(Loop(time));
    }
}
