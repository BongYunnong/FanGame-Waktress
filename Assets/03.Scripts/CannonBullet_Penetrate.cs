using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CannonBullet_Penetrate : CannonBullet
{
    private void Start()
    {
        currCoolDownTime = maxCoolDownTime;
        Invoke("ActivateCol",0.1f);

        AmmoManager.GetInstance().SkillPanelActive(SpecialMoveImage);
        
    }


    private void ActivateCol()
    {
        skillTriggered = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (skillTriggered)
        {
            if (collision.transform.tag == "Ground")
            {
                if (PV.IsMine)
                {
                    PV.RPC("ExplosionRPC", RpcTarget.All);
                }
            }
            else if (collision.transform.tag == "Player")
            {
                if (PV.IsMine)//맞은쪽에 맞춰서 Hit판정
                {
                    PV.RPC("ExplosionRPC", RpcTarget.All);
                }

            }
        }
    }
}


