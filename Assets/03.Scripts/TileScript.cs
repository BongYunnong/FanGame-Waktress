using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class TileScript : MonoBehaviourPunCallbacks
{
    public float MaxHealth = 100;
    public float health = 100;
    public Sprite[] sprites;
    public SpriteRenderer sr;
    public ParticleSystem attackedPS;



    public bool MineInstalled;
    
    // Update is called once per frame
    void Update()
    {


        health = Mathf.Clamp(health, 0, MaxHealth);
        float healthIndex = (health/ MaxHealth);


        if (!MineInstalled)
        {

            if (healthIndex <= 0)
            {
                Destroy(this.gameObject);
            }
            else if (healthIndex < .1f)
            {
                sr.sprite = sprites[0];
            }
            else if (healthIndex < .3f)
            {
                sr.sprite = sprites[1];
            }
            else if (healthIndex < .6f)
            {
                sr.sprite = sprites[2];
            }
            else if (healthIndex < 1f)
            {
                sr.sprite = sprites[3];
            }
            else
            {
                sr.sprite = null;
            }
        }
        else
        {
            if (healthIndex <= 0)
            {
                Destroy(this.gameObject);
            }
            sr.sprite = sprites[4];
        }

        if (PhotonNetwork.CurrentRoom != null)
        {
            if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
            {
                Destroy(this.gameObject);
            }
        }
    }

    /*
public void ActiveFlaseTile()
{
PV.RPC("DestroyRpc", RpcTarget.AllBuffered);
}

public void Attacked(float damage)
{
PV.RPC("AttackedRPC", RpcTarget.All);
health -= damage;
}



[PunRPC]
void DestroyRpc() => Destroy(gameObject);
*/
}
