using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using DG.Tweening;
public class CannonBullet_Wakrio : CannonBullet
{
    public GameObject Wakrio;
    public Vector3 targetPos;

    public GameObject AttackedParticle;
    private bool WakrioAppeared=false;

    protected override void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        hitsoundVol = hitSound.volume;
        //AudioSliderChanged(OptionSettingManager.GetInstance().effectAudioSlider.value);

    }

    public new void Update()
    {
        if (!skillTriggered)
        {
            base.Update();

            if ((hitted) && PV.IsMine)
            {
                targetPos = this.transform.position;
                //PV.RPC("GeneRPC", RpcTarget.AllBuffered);
                StartCoroutine("DestroyCoroutine");
                //hitSound.Play();
                //skillTriggered = true;
                PV.RPC("SkillTriggerRpc", RpcTarget.AllBuffered);
                GameObject tmp;
                tmp = PhotonNetwork.Instantiate(Wakrio.name, this.transform.position + new Vector3(-40,60, 0f), Quaternion.identity);
                tmp.GetComponent<Wakrio>().target = targetPos;

                Sequence mySequence= DOTween.Sequence();
                mySequence = DOTween.Sequence().OnStart(() =>
                {
                    AttackedParticle.transform.DOScale(new Vector3(3, 3, 3), 1f).SetEase(Ease.OutBounce);
                })
                .Append(mySequence.SetDelay(3f))
                .Append(AttackedParticle.transform.DOScale(new Vector3(0,0, 0), .5f).SetEase(Ease.OutCubic));
                AttackedParticle.SetActive(true);
            }
        }
        else
        {
            if (!WakrioAppeared)
            {
                WakrioAppeared = true;
                DestroyBulletTime = 14f;
            }
            this.transform.rotation = Quaternion.identity;

        }
    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(10f);
        DestroyBullet();
    }

    public override void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Ground" || collision.transform.tag == "Supply")
        {
            if (PV.IsMine)
            {
                hitted = true;
            }
        }
        else if (collision.transform.tag == "Player")
        {
            if (PV.IsMine)//맞은쪽에 맞춰서 Hit판정
            {
                hitted = true;
            }

        }
    }
}
