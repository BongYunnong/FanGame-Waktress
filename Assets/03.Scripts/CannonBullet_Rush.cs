using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class CannonBullet_Rush : CannonBullet
{
    public AudioClip skillAudio;
    public AudioSource skillAudio2;
    public Vector2 targetPos;
    public Sprite skillSprite;
    public SpriteRenderer SR;
    void Update()
    {
        base.Update();

        if (hitted)
        {
            GetComponent<CircleCollider2D>().isTrigger = true;
        }

        if (!skillTriggered) {
            base.Update();
            if ( Input.GetMouseButtonDown(1) && !hitted&& PV.IsMine)
            {
                PV.RPC("SkillTriggerRpc", RpcTarget.AllBuffered);
                targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                targetPos -= new Vector2(this.transform.position.x, this.transform.position.y);
                rb.velocity = Vector2.zero;
                skillAudio2.Play();
            }
        }
        else
        {
            SR.transform.localScale = Vector2.Lerp(SR.transform.localScale, new Vector3(2, 2, 1), Time.deltaTime * 5f); 
            SR.sprite = skillSprite;
            if (!hitted)
            {
                rb.velocity = Vector2.Lerp(rb.velocity,targetPos.normalized * 10f,Time.deltaTime*5f);
            }

            if (hitSound.clip != skillAudio)
            {
                hitSound.clip = skillAudio;
                hitSound.Play();
            }
        }
    }


    public override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Ground" || collision.transform.tag == "Supply")
        {
            hitted = true;
            if (PV.IsMine&&ExplodeWhenHitted)
                PV.RPC("ExplosionRPC", RpcTarget.All);
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;


            if (PV.IsMine)
                TileManager.GetInstance().UpdateTilesHealth(this.transform.position, Damage, 0.5f);
            //if (collision.transform.GetComponent<TileScript>())
            //{
            //    collision.transform.GetComponent<TileScript>().Attacked(Damage);
            //}
        }
        else if ( collision.transform.tag == "Player") 
        {
            //!PV.IsMine && collision.transform.GetComponent<PhotonView>().IsMine
            if (PV.IsMine)//느린쪽에 맞춰서 Hit판정
            {
                //PV.RPC("ExplosionRPC", RpcTarget.All);

                //collision.transform.GetComponent<Player>().Attacked((int)Damage, Vector3.zero);

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
