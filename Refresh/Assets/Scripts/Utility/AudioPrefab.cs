using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPrefab : MonoBehaviour
{
    /*
     * Handles playing audio when audio prefab is spawned
     */

    private AudioSource audioSource;
    private bool started;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (audioSource.isPlaying)
        {
            started = true;
        }
        else if (started)
        {
            Destroy(gameObject);
        }
    }
}
