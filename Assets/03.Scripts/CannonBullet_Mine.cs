using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CannonBullet_Mine : CannonBullet
{
    // Update is called once per frame
    void Update()
    {
        base.Update();
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
        if (collision.transform.tag == "Ground"&&!skillTriggered)
        {
            hitted = true;

            PV.RPC("SkillTriggerRpc", RpcTarget.AllBuffered);
            if (collision.gameObject.GetComponent<TileScript>())
            {
                TileManager.GetInstance().UpdateTilesHealth(this.transform.position, 0f, radius, true);
            }
        }
        else if (collision.transform.tag == "Player")
        {
            if (PV.IsMine)//맞은쪽에 맞춰서 Hit판정
            {
                if (ExplodeWhenHitted)
                    PV.RPC("ExplosionRPC", RpcTarget.All);
                else
                {
                    if (!hitted)
                        collision.transform.GetComponent<Player>().Attacked((int)Damage, Vector3.zero);
                }
                hitted = true;
            }

        }
    }
}
