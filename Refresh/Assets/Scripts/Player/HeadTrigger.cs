using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HeadTrigger : MonoBehaviour
{
    /*
     * handles player ceiling detection
     */

    public PlayerController player;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!player.gravityCrystal)
        {
            if (collision.CompareTag("Ground") && player.myTween.active && !player.falling)
                player.myTween.Kill();
        }
        else
        {
            if(collision.CompareTag("Ground") || collision.CompareTag("One Way"))
            player.grounded = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") && player.gravityCrystal)
            player.PlayAudio(player.land, 0.1f, 1);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") && player.gravityCrystal)
            player.grounded = false;
    }
}
