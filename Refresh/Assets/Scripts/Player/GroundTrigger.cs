using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundTrigger : MonoBehaviour
{
    /*
     * Handles player ground detection
     */

    public PlayerController player;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("One Way"))
        {
            player.jumping = false;
            player.grounded = true;
            if (player.jumpBufferActive)
            {
                player.Jump();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Ground") || collision.CompareTag("One Way"))
        {
            if (player.lead)
            {
                player.PlayAudio(player.heavyLand, 0.25f, 1);
                player.CameraShake();
            }
            else
                player.PlayAudio(player.land, 0.1f, 1);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground") || collision.CompareTag("One Way"))
        {
            player.grounded = false;
            if (!player.gravityCrystal && !player.jumping)
            {
                StopCoroutine(CoyoteTime(0));
                StartCoroutine(CoyoteTime(player.coyoteTime));
            }
        }
    }

    public IEnumerator CoyoteTime(float waitTime)
    {
        player.coyoteTimeActive = true;
        yield return new WaitForSeconds(waitTime);
        player.coyoteTimeActive = false;
    }
}
