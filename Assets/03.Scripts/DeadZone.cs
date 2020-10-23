using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Player>())
        {
            other.transform.GetComponent<Player>().Attacked(1000, Vector3.zero);
        }
        else if(other.GetComponent<CannonBullet>())
        {
            other.GetComponent<CannonBullet>().DestroyBullet();
        }
    }
}
