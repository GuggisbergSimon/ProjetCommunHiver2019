using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovesWithPlatform : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.GetComponent<PlayerController>().GetGrounded)
        {
            collision.collider.transform.SetParent(transform);
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>().GetGrounded)
        {
            collision.collider.transform.SetParent(null);
        }
    }
}
