using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Wakrio : CannonBullet
{
    public Vector3 target;
    public float arriveMagnitude = 0.1f;
    public bool arrived=false;
    public bool arriveSkill = false;
    private bool setVC = false;
    // Start is called before the first frame update
    protected override void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        hitsoundVol = hitSound.volume;
        //AudioSliderChanged(OptionSettingManager.GetInstance().effectAudioSlider.value);

        Invoke("DestroyBullet", 14f);
    }
    void Start()
    {
        currCoolDownTime = maxCoolDownTime;
        OptionSettingManager.GetInstance().SetBackgroundVolume(0f,15f);
        Invoke("SetVCPos", 6f);
    }

    private void SetVCPos()
    {
        setVC = true;
        AmmoManager.GetInstance().SkillPanelActive(SpecialMoveImage);
    }

    // Update is called once per frame
    public new void Update()
    {
        currCoolDownTime -= Time.deltaTime;
        if (currCoolDownTime <= 0 && currCoolDownTime > -50)
        {
            if (exploded)
            {
                exploded = false;
            }
            currCoolDownTime = maxCoolDownTime;
        }

        skillPs.gameObject.SetActive(true);

        Vector2 dir = target - this.transform.position;

        if (setVC)
        {
            GameManager.GetInstance().VC.m_Follow.position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        }

        if (dir.magnitude > arriveMagnitude)
        {
            if (arrived)
            {
                rb.velocity = Vector3.up *1f;
                if (PV.IsMine &&!arriveSkill)
                {
                    arriveSkill = true;
                    PV.RPC("ExplosionRPC", RpcTarget.All);
                }
            }
            else
            {
                rb.velocity = dir.normalized * 10f;
            }
        }
        else
        {
            if (!arrived)
            {
                arrived = true;
                rb.velocity =Vector3.zero;
                rb.gravityScale = .25f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!arrived || (arrived&&!hitted))
        {
            if (collision.transform.tag == "Ground")
            {
                if (PV.IsMine)
                    PV.RPC("ExplosionRPC", RpcTarget.All);
            }
            else if (collision.transform.tag == "Player") //느린쪽에 맞춰서 Hit판정
            {
                //
                //!PV.IsMine && collision.transform.GetComponent<PhotonView>().IsMine
                if (PV.IsMine)//맞은쪽에 맞춰서 Hit판정
                {
                    PV.RPC("ExplosionRPC", RpcTarget.All);
                }
            }
        }
    }
}
