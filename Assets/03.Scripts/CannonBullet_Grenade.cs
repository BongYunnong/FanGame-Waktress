using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CannonBullet_Grenade : CannonBullet
{
    public float delay = 3f;

    float countdown;
    // Start is called before the first frame update
    void Start()
    {
        countdown = delay;
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

        countdown -= Time.deltaTime;


        if (((Input.GetMouseButtonDown(1) && PV.IsMine) ||countdown <=0f) && !skillTriggered)
        {
            PV.RPC("ExplosionRPC", RpcTarget.All);
            PV.RPC("SkillTriggerRpc", RpcTarget.AllBuffered);
            StartCoroutine("DestroyCoroutine");
            skillTriggered = true;
        }
    }

    [PunRPC]
    protected override void SkillTriggerRpc()
    {
        skillTriggered = true;
    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(2f);
        DestroyBullet();
    }


    public override  void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Ground")
        {
            hitted = true;
            //rb.velocity = Vector2.zero;
            //rb.isKinematic = true;

            if (PV.IsMine)
            {
                if (ExplodeWhenHitted)
                    PV.RPC("ExplosionRPC", RpcTarget.All);
                else
                {
                    TileManager.GetInstance().UpdateTilesHealth(this.transform.position, Damage, 0.5f);
                }
            }
            /*
            if (PV.IsMine)
                TileManager.GetInstance().UpdateTilesHealth(this.transform.position, Damage, 0.5f);
            */
            
            //if (collision.transform.GetComponent<TileScript>())
            //{
            //    collision.transform.GetComponent<TileScript>().Attacked(Damage);
            //}
        }
        else if (collision.transform.tag == "Player" ) //느린쪽에 맞춰서 Hit판정
        {
            //!PV.IsMine && collision.transform.GetComponent<PhotonView>().IsMine
            if (PV.IsMine)
            {
                if (ExplodeWhenHitted)
                    PV.RPC("ExplosionRPC", RpcTarget.All);
                else
                {
                    if (!hitted)
                        collision.transform.GetComponent<Player>().Attacked((int)Damage, Vector3.zero);
                }


                hitted = true;
                //collision.transform.GetComponent<Player>().Attacked((int)Damage, Vector3.zero);
            }
        }
    }
}
