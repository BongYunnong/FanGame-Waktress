using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
public class Supply : MonoBehaviourPunCallbacks, IPunObservable
{
    int ammoIndex;
    int ammoCount;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
        {
            if (collision.transform.GetComponent<PhotonView>().IsMine)
            {
                ammoIndex = Random.Range(0, AmmoManager.GetInstance().currAmmoCounts.Count);


                if (ammoIndex >= 8 && ammoIndex <= 14)
                {
                    ammoCount = 1;
                }
                else
                {
                    ammoCount = Random.Range(1, 3);
                }

                AmmoManager.GetInstance().currAmmoCounts[ammoIndex] +=ammoCount;
                OptionSettingManager.GetInstance().Play("Wak_Emo2", false);
                OptionSettingManager.GetInstance().Play("Item", false);

                GetComponent<PhotonView>().RPC("AmmoSupplyRPC", RpcTarget.AllBuffered, ammoIndex*100+ammoCount);//AllBuffered로 해야 제대로 사라져 복제버그 X
            }
        }
    }

    [PunRPC]
    public void AmmoSupplyRPC(int index)
    {

        FloatingTextController.CreateFloatingText(AmmoManager.GetInstance().weaponIndexToString((int)(index / 100)) +"x" + (int)(index % 100), transform, Color.white);

        Destroy(gameObject);
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
    }

}
