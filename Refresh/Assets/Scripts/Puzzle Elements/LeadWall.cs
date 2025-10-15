using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class LeadWall : MonoBehaviour
{
    /*
     * Handles breakable floor conditions and save data
     */

    private GameController gameController;
    private PlayerController player;
    public string prefs;
    public AudioClip breaking;
    public ParticleSystem particles;

    int value;

    private void Awake()
    {
        gameController = GameObject.Find("Game Controller").transform.GetComponent<GameController>();
        value = PlayerPrefs.GetInt(prefs);

        PlayerPrefs.SetInt(prefs, 0);

        if (value == 1)
        {
            Destroy(gameObject);
        }

        player = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && collision.relativeVelocity.magnitude > 20 && player.fallDistance > 3f && player.lead)
        {
            BreakWall();
        }
    }

    void BreakWall()
    {
        if (gameController.glitchPeriod)
        {
            PlayerPrefs.SetInt(prefs, 1);
        }
        player.myTween.Kill();
        player.rb.velocity = new Vector2(player.rb.velocity.x, -20);
        player.PlayAudio(breaking, 1, 1);
        particles.Play();
        Destroy(gameObject);
    }
}
