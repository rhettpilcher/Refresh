using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWall : MonoBehaviour
{
    /*
     * Part of FireWall prefab; wooden walls that can be burnt by fire. Handles burn conditions, fire burning effect, and save data
     */

    [Header("References")]
    private Transform player;
    private PlayerController playerController;
    private GameController gameController;
    public GameObject firePrefab;
    public AudioClip fireAudio;

    [Header("Stats")]
    public string prefs;
    public float halfHeight;
    public float halfWidth;
    private int fireToSpawn = 4;
    public float interval;

    [Header("State Info")]
    private bool broken = false;
    int value; //save data value, 0 = present, 1 = destroyed

    private void Awake()
    {
        gameController = GameObject.Find("Game Controller").transform.GetComponent<GameController>();
        value = PlayerPrefs.GetInt(prefs);

        PlayerPrefs.SetInt(prefs, 0);

        if (value == 1)
        {
            Destroy(gameObject);
        }
        
        player = GameObject.Find("Player").transform;
        playerController = player.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Vector3.Distance(player.position, transform.position) <= 3 && playerController.fire && !broken)
            BreakWall();
    }

    void BreakWall()
    {
        broken = true;
        if (gameController.glitchPeriod)
        {
            PlayerPrefs.SetInt(prefs, 1);
        }
        StartCoroutine(FireSpawn(interval));
    }

    private IEnumerator FireSpawn(float waitTime)
    {
        GameObject fire = Instantiate(firePrefab, new Vector3(100, 100, 0), Quaternion.identity, transform);
        fire.transform.localPosition = new Vector3(Random.Range(-halfWidth, halfWidth), Random.Range(-halfHeight, halfHeight), 0);
        gameController.PlayAudio(fireAudio, 1, 1);
        yield return new WaitForSeconds(waitTime);
        fireToSpawn -= 1;
        if (fireToSpawn == 0)
            Destroy(gameObject);
        else
            StartCoroutine(FireSpawn(interval));
    }
}
