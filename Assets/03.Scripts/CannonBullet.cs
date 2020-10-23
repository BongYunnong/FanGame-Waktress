using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CannonBullet : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    public AudioSource hitSound;
    protected float hitsoundVol;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;

    public bool DestroyWhenHitted = true;
    public bool ExplodeWhenHitted = true;

    protected float currCoolDownTime = -100f;
    public float maxCoolDownTime = -100f;

    public float radius = 1f;
    public float Damage = 10;

    public float explosionForce = 100f;
    public float explosionPower = 50f;
    public GameObject explosionEffect;
    bool ExplosionInstantiated = false;
    public AudioClip ExplosionAudio;


    protected float angle;
    protected bool willDestroy = false;
    protected bool hitted=false;
    protected bool skillTriggered=false;
    protected bool exploded=false;

    public ParticleSystem skillPs;

    public bool SpecialMove=false;
    public Sprite SpecialMoveImage;

    public float DestroyBulletTime = 5f;
    
    // Start is called before the first frame update
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        hitsoundVol = hitSound.volume;
        //AudioSliderChanged(OptionSettingManager.GetInstance().effectAudioSlider.value);

        Invoke("DestroyBullet", DestroyBulletTime);
    }
    private void Start()
    {
        currCoolDownTime = maxCoolDownTime;
        if (SpecialMove)
        {
            AmmoManager.GetInstance().SkillPanelActive(SpecialMoveImage);
        }
    }
    /*
    protected virtual void AudioSliderChanged(float value)
    {
        if (hitSound != null)
            hitSound.volume = hitsoundVol * value;
    }*/


    // Update is called once per frame
    protected void Update()
    {

        currCoolDownTime -= Time.deltaTime;
        if (currCoolDownTime <= 0 && currCoolDownTime>-50)
        {
            if (exploded)
            {
                exploded = false;
            }
            currCoolDownTime = maxCoolDownTime;
        }

        if (!hitted)
        {
            angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);


            if (rb.velocity.x >= 1f)
            {
                sr.flipY = false;
            }
            else if (rb.velocity.x <= -1f)
            {
                sr.flipY = true;
            }
        }

        if (skillTriggered)
        {
            skillPs.Stop();
        }
        
        if ((DestroyWhenHitted&&hitted)&& PV.IsMine)
        {
            DestroyBullet();
            //GetComponent<CircleCollider2D>().isTrigger = true;
        }

    }

    public void DestroyBullet()
    {
        PV.RPC("DestroyRPC", RpcTarget.AllBuffered);
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Ground" ||collision.transform.tag=="Supply")
        {
            if (PV.IsMine)
            {
                if (ExplodeWhenHitted)
                    PV.RPC("ExplosionRPC", RpcTarget.All);
                else
                {
                    if(!hitted)
                        TileManager.GetInstance().UpdateTilesHealth(this.transform.position, Damage, 0.5f);
                }
            }
            //if (collision.transform.GetComponent<TileScript>())
            //{
            //    collision.transform.GetComponent<TileScript>().Attacked(Damage);
            //}
        }//!PV.IsMine && collision.transform.tag == "Player" && collision.transform.GetComponent<PhotonView>().IsMine
        else if (collision.transform.tag == "Player")
        {
            //PV.IsMine
            //!PV.IsMine && collision.transform.GetComponent<PhotonView>().IsMine
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

    public void Explode()
    {
        if (!exploded)
        {
            //땅 폭발
            TileManager.GetInstance().UpdateTilesHealth(this.transform.position, explosionPower, radius);

            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (Collider2D nearbyObject in colliders)
            {

                var dir = (nearbyObject.transform.position - transform.position);
                float tmpMagnitude = Mathf.Clamp(dir.magnitude, 0, radius);
                float wearoff = 1 - (tmpMagnitude / radius);
                wearoff = Mathf.Clamp(wearoff, 0.5f, .95f);

                Player playerScript = nearbyObject.GetComponent<Player>();
                if (playerScript != null)
                {
                    if (explosionPower <= 0)
                    {
                        if (explosionPower <= -500)
                        {
                            //마인크래프트두
                            explosionPower = 0;
                        }
                        else
                        {
                            //힐
                            explosionForce = 0;
                            wearoff = 1;
                        }
                    }
                    playerScript.Attacked(explosionPower * wearoff, (Vector3.Normalize(dir)+ new Vector3(0, 1f, 0f)) * explosionForce * 12 * wearoff);
                }
            }
        }
    }

    [PunRPC]
    protected virtual void SkillTriggerRpc()
    {
        skillTriggered = true;
    }

    [PunRPC]
    protected void DestroyRPC()
    {
        TurnManager.GetInstance().SetTimeOrEventCount(4.5f, -1);

        Destroy(gameObject);
    }

    [PunRPC]
    protected void ExplosionRPC()
    {
        Explode();

        GameManager.GetInstance().ShakeCamera(radius,0.2f);

        GameObject tmp = Instantiate(explosionEffect, transform.position, Quaternion.identity);

        foreach (Transform tr in tmp.GetComponentsInChildren<Transform>())
        {
            tr.localScale = new Vector3(radius, radius, 1f);
        }
        tmp.transform.localScale = new Vector3(radius, radius, 1f);

        if (Random.Range(0f, 1f) < 0.3f)
            OptionSettingManager.GetInstance().Play("Explosion", false);

        /*
        AudioSource tmpAudio = tmp.AddComponent<AudioSource>();
        tmpAudio.playOnAwake = true;
        tmpAudio.clip = ExplosionAudio;
        tmpAudio.volume = hitsoundVol * hitSound.volume;
        tmpAudio.
        */
        hitted = true;
        exploded = true;
    }


    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (collision.tag == "Ground")
    //    {
    //        hitted = true;

    //        if (collision.GetComponent<TileScript>())
    //        {
    //            collision.GetComponent<TileScript>().Attacked(power);
    //            DestroyBullet();
    //        }
    //    }
    //    else if (!PV.IsMine && collision.tag == "Player" && collision.GetComponent<PhotonView>().IsMine) //느린쪽에 맞춰서 Hit판정
    //    {
    //        hitted = true;

    //        collision.GetComponent<Player>().Attacked();
    //        DestroyBullet();
    //    }
    //}
}
