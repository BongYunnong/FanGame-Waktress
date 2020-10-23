using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CannonBullet_Panzee : CannonBullet
{
    public float delay = 3f;
    float countdown;
    // Update is called once per frame
    void Start()
    {
        countdown = delay;
    }
    public new void  Update()
    {
        base.Update();

        countdown -= Time.deltaTime;

        if (!hitted&&!skillTriggered&& Input.GetMouseButtonDown(1) && PV.IsMine)
        {
            this.transform.localScale *= 0.75f;

            skillTriggered = true;


            for(int i = 0; i < 8; i++)
            {
                Shoot(-180 + 45 * i);
            }
            countdown = 0;
        }

        if(countdown <= 0f)
        {
            PV.RPC("ExplosionRPC", RpcTarget.All);
            DestroyBullet();
        }
    }
    void Shoot(float _angle)
    {
        _angle += angle;
        GameObject newBullet = PhotonNetwork.Instantiate("Bullet_Panzee", this.transform.position,Quaternion.identity);
        newBullet.transform.localScale *= 0.75f;
        newBullet.GetComponent<Rigidbody2D>().velocity = new Vector3(Mathf.Cos(Mathf.Deg2Rad * _angle), Mathf.Sin(Mathf.Deg2Rad * _angle), 0) * rb.velocity.magnitude;
        newBullet.GetComponent<Rigidbody2D>().gravityScale = 0;
        //newBullet.transform.Rotate(transform.right, angle);

        newBullet.GetComponent<CannonBullet_Panzee>().skillTriggered = true;
        newBullet.GetComponent<CannonBullet_Panzee>().delay = 0.5f;
    }
}
