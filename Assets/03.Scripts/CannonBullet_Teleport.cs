using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CannonBullet_Teleport : CannonBullet
{
    public Transform playerTr;
    public float delay = 3f;

    float countdown;
    public GameObject TeleportEffect;
    protected override void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        hitsoundVol = hitSound.volume;
        //AudioSliderChanged(OptionSettingManager.GetInstance().effectAudioSlider.value);
    }

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


        if (((Input.GetMouseButtonDown(1) && PV.IsMine) || countdown <= 0f) && !skillTriggered)
        {
            if (PV.IsMine)
            {
                PV.RPC("SkillTriggerRpc", RpcTarget.AllBuffered);
            }
            StartCoroutine("DestroyCoroutine");
        }
    }

    [PunRPC]
    protected override void SkillTriggerRpc()
    {
        Instantiate(TeleportEffect, this.transform.position, Quaternion.identity);
        if (playerTr)
        {
            Instantiate(TeleportEffect, playerTr.position, Quaternion.identity);
            playerTr.position = this.transform.position;
        }
        skillTriggered = true;
    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(2f);
        DestroyBullet();
    }
}
