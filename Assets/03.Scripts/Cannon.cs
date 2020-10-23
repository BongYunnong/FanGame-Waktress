using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.U2D.Animation;

public class Cannon : MonoBehaviourPunCallbacks, IPunObservable
{
    private Player player;
    public GameObject bullet;
    public GameObject launchPowerImage;
    public float maxLaunchForce;
    public float launchForce;
    float launchBurst = 1;
    public float launchDelay = 0.5f;

    public Transform shotPoint;
    public Transform Bar;
    public ParticleSystem chargeParticleSystem;

    /*
    public AudioSource chargeSound;
    public AudioSource reloadSound;

    private float chargeSoundVolume;
    private float reloadSoundVolume;
    */
    public PhotonView PV;
    Vector2 direction;
    public float angle=0;
    float mul = 1;

    public GameObject point;
    GameObject[] points;
    public int numberOfPoints;
    public float spaceBetweenPoints;

    Quaternion currRotation;

    public Sprite[] sprites;
    public Image[] weaponImg;

    public Vector2 whipBoxSize;
    private void Awake()
    {
        player = GetComponentInParent<Player>();

        if (PV.Owner == PhotonNetwork.LocalPlayer)
        {
            points = new GameObject[numberOfPoints];
            for (int i = 0; i < numberOfPoints; i++)
            {
                points[i] = Instantiate(point, shotPoint.position, Quaternion.identity);
                points[i].SetActive(false);
            }
        }

        /*
        chargeSoundVolume = chargeSound.volume;
        reloadSoundVolume = reloadSound.volume;
        AudioSliderChanged(OptionSettingManager.GetInstance().effectAudioSlider.value);
        OptionSettingManager.GetInstance().effectAudioSlider.onValueChanged.AddListener(AudioSliderChanged);
        */
    }
    public override void OnDisable()
    {
        OptionSettingManager.GetInstance().effectAudioSlider.onValueChanged.RemoveAllListeners();
    }
    // Update is called once per frame
    void Update()
    {

        //PV.RPC("FlipXRpc", RpcTarget.AllBuffered, player.sr.flipX);

        /*
        float scaleVal = 0.6f;
        if (!player.sr3.flipX)
        {
            transform.localPosition = new Vector3(.1f, -0.05f, -0.1f);
            transform.localScale = new Vector3(scaleVal, scaleVal, scaleVal);
        }
        else
        {
            transform.localPosition = new Vector3(-.1f, -0.05f, -0.1f);
            transform.localScale = new Vector3(-scaleVal, scaleVal, scaleVal);
        }
        */

        if (PV.IsMine)
        {
            if (AmmoManager.GetInstance().WeaponIndex == 7)
            {
                player.anim.SetBool("WhipEquip", true);
            }
            else
            {
                player.anim.SetBool("WhipEquip", false);
            }
        }

        if (PV.IsMine &&GameManager.GetInstance().currentCharacter==player && AmmoManager.GetInstance().WeaponIndex >= 0 && AmmoManager.GetInstance().WeaponIndex < sprites.Length)
        {
            weaponImg[0].gameObject.SetActive(true);
            weaponImg[1].sprite = sprites[AmmoManager.GetInstance().WeaponIndex];
        }
        else
            weaponImg[0].gameObject.SetActive(false);

        if (AmmoManager.GetInstance().WeaponIndex >= 0)
        {
            if (launchForce > 3)
            {
                if (!chargeParticleSystem.isPlaying)
                    chargeParticleSystem.Play();
            }
            else
            {
                chargeParticleSystem.Stop();
            }
        }

        if (player.PV.IsMine)
        {
            //#만약 턴제를 원한다면 (player.myIndex - 1 == (GameManager.TurnIndex % GameManager.teamCount)) 추가
            if (GameManager.GetInstance().currentCharacter ==player || !(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
            {
                if (launchDelay <= 0)
                {
                    //이미 쐈는지 확인
                    if (!((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"] && launchDelay > 50))
                    {
                        if (AmmoManager.GetInstance().WeaponIndex >= 0)
                        {
                            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"] && TurnManager.GetInstance().currTurnTime<.5f && launchForce>=2)
                            {
                                TurnManager.GetInstance().SetTimeOrEventCount(5.5f, 1);
                                Shoot();
                            }

                            if (Input.GetMouseButtonDown(0))
                            {
                                if (EventSystem.current.IsPointerOverGameObject())
                                {
                                    return;
                                }

                                if (AmmoManager.GetInstance().currAmmoCounts[AmmoManager.GetInstance().WeaponIndex] <= 0)
                                {
                                    OptionSettingManager.GetInstance().Play("Reload", false);
                                    //reloadSound.Play();

                                    launchPowerImage.transform.localScale = new Vector3(0, 0.05f, 1);
                                    launchPowerImage.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
                                    return;
                                }

                                //차지
                                //chargeParticleSystem.Play();
                                //chargeSound.Play();
                                OptionSettingManager.GetInstance().Play("Charge", true);

                                launchForce = 2;
                                launchBurst = 1;
                            }
                            else if (Input.GetMouseButton(0) && launchForce >= 1f)
                            {
                                launchForce += 6 * Time.deltaTime;
                                launchForce = Mathf.Clamp(launchForce, 0, maxLaunchForce);
                                launchPowerImage.transform.localScale = new Vector3((launchForce / maxLaunchForce), 0.05f, 1);

                                if (launchForce == maxLaunchForce)
                                {
                                    launchBurst -= Time.deltaTime;
                                    if (launchBurst <= 0)
                                    {
                                        Shoot();
                                    }
                                }


                                if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
                                {
                                    if (AmmoManager.GetInstance().WeaponIndex != 4 && AmmoManager.GetInstance().WeaponIndex != 5 && AmmoManager.GetInstance().WeaponIndex != 7 && AmmoManager.GetInstance().WeaponIndex != 12 && AmmoManager.GetInstance().WeaponIndex != 13)
                                    {
                                        for (int i = 0; i < numberOfPoints; i++)
                                        {
                                            points[i].SetActive(true);
                                            points[i].transform.position = PointPosition(i * spaceBetweenPoints);
                                        }
                                    }
                                }


                                player.PV.RPC("SetEmotionRPC", RpcTarget.All, "Angry");
                                //player.SR_Emotion.GetComponent<SpriteResolver>().SetCategoryAndLabel("Emotion", "Angry");
                                //player.SR_Emotion.GetComponent<SpriteResolver>().ResolveSpriteToSpriteRenderer();
                            }
                            else if (Input.GetMouseButtonUp(0) && launchForce > 1f)
                            {
                                for (int i = 0; i < numberOfPoints; i++)
                                {
                                    points[i].SetActive(false);
                                }
                                Shoot();
                            }
                        }
                    }
                }
                else
                {
                    launchDelay -= Time.deltaTime;

                    if (Input.GetMouseButtonDown(0))
                    {
                        OptionSettingManager.GetInstance().Play("Reload", false);
                        //reloadSound.Play();
                    }
                    if (launchDelay <= 0)
                    {
                        OptionSettingManager.GetInstance().Play("Reload", false);
                        //reloadSound.Play();

                        launchPowerImage.transform.localScale = new Vector3(0, 0.05f, 1);
                        launchPowerImage.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1);
                    }
                    else
                    {
                        if (launchForce <= 0)
                        {
                            launchPowerImage.transform.localScale = new Vector3(1, 0.05f, 1);
                            launchPowerImage.GetComponent<SpriteRenderer>().color = new Color(.7f, 0, 0);
                        }
                        else
                        {
                            launchPowerImage.transform.localScale = new Vector3((launchForce / maxLaunchForce), 0.05f, 1);
                        }
                    }

                }


                //-------------------------------------
                Vector2 cannonPos = transform.position;
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);


                if (GameManager.GetInstance().AngleWithW) {
                    angle += Input.GetAxisRaw("Vertical")*2;

                    float minRotation = -90;
                    float maxRotation = 90;

                    angle = Mathf.Clamp(angle, minRotation, maxRotation);

                    if (player.SR_Head.transform.localScale.x<0)
                    {
                        transform.rotation = Quaternion.Euler(0, 0, -angle - 90);
                        Bar.rotation = Quaternion.Euler(0, 0, -angle - 90);

                        //player.SR_Emotion.transform.Find("bone_1").rotation = Quaternion.Euler(0, 0, -angle + 90);
                        player.SR_Hat.transform.rotation = Quaternion.Euler(0, 0, -angle - 90);
                    }
                    else
                    {
                        transform.rotation = Quaternion.Euler(0, 0, 90 + angle);
                        Bar.localRotation = Quaternion.Euler(0, 0, 0);

                        //player.SR_Hat.transform.rotation = Quaternion.Euler(0, 0, 0);

                        //player.SR_Emotion.transform.Find("bone_1").rotation = Quaternion.Euler(0, 0, angle + 90);
                        player.SR_Hat.transform.rotation = Quaternion.Euler(0, 0, angle + 90);
                    }

                    direction = Bar.rotation.normalized * -Vector3.up;
                }
                else
                {
                    direction = mousePos - cannonPos;
                    angle = Vector2.Angle(Vector2.right, direction);
                    Vector3 cross = Vector3.Cross(Vector2.right, direction);
                    if (player.SR_Head.transform.localScale.x < 0)
                    {
                        angle = Vector2.Angle(Vector2.left, direction);
                        cross = Vector3.Cross(Vector2.left, direction);
                    }

                    if (cross.z < 0) angle = -angle;

                    float minRotation = -90;
                    float maxRotation = 90;

                    angle = Mathf.Clamp(angle, minRotation, maxRotation);

                    if (player.SR_Head.transform.localScale.x < 0)
                    {
                        transform.rotation = Quaternion.Euler(0, 0, angle-90);
                        Bar.rotation = Quaternion.Euler(0, 0, angle - 90);
                    }
                    else
                    {
                        transform.rotation = Quaternion.Euler(0, 0, 90 + angle);
                        Bar.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                }
            }
            else
            {
                //만약 시간이 지나가버렸으면 그냥 쏴버림
                if (launchForce > 1f) {
                    Shoot();
                }
            }
        }
        else
        {
            float t = Mathf.Clamp(Time.deltaTime * 10, 0f, 0.99f);
            transform.rotation = Quaternion.Lerp(transform.rotation, currRotation, t);
            player.SR_Hat.transform.rotation = Quaternion.Lerp(player.SR_Hat.transform.rotation, currRotation, t);

            launchPowerImage.transform.localScale = new Vector3((launchForce / maxLaunchForce), 0.05f, 1);
        }

        if (PhotonNetwork.CurrentRoom != null)
        {
            if (player == GameManager.GetInstance().currentCharacter || !(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
            {
                Bar.gameObject.SetActive(true);
            }
            else
            {
                Bar.gameObject.SetActive(false);
            }
        }
    }

    /*
    protected void AudioSliderChanged(float value)
    {
        chargeSound.volume = chargeSoundVolume * value;
        reloadSound.volume = reloadSoundVolume * value;
    }
    */

    void Shoot()
    {
        TurnManager.GetInstance().SetTimeOrEventCount(5.5f, -1);

        OptionSettingManager.GetInstance().Play("Charge", true, true);
        //chargeSound.Stop();
        chargeParticleSystem.Stop();
        //GameObject newBullet = Instantiate(bullet, shotPoint.position, shotPoint.rotation);
        //newBullet.GetComponent<Rigidbody2D>().velocity = transform.right * launchForce;
        //launchForce = 0;

        if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
            launchDelay = 0.5f;
        else
            launchDelay = 100f;

        float additionalForce = 0;

        AmmoManager.GetInstance().currAmmoCounts[AmmoManager.GetInstance().WeaponIndex]--;
        GameObject newBullet=null;
        switch (AmmoManager.GetInstance().WeaponIndex) {
            case 0:
                newBullet = PhotonNetwork.Instantiate("Bullet", shotPoint.position, shotPoint.rotation);
                break;
            case 1:
                newBullet = PhotonNetwork.Instantiate("Bullet_Grenade", shotPoint.position, shotPoint.rotation);
                break;
            case 2:
                newBullet = PhotonNetwork.Instantiate("Bullet_Divide", shotPoint.position, shotPoint.rotation);
                break;
            case 3:
                newBullet = PhotonNetwork.Instantiate("Bullet_Panzee", shotPoint.position, shotPoint.rotation);
                break;
            case 4:
                StartCoroutine("RapidShot", new object[] { Vector3.zero, 4 });
                newBullet = PhotonNetwork.Instantiate("Bullet_DooDoo", shotPoint.position, shotPoint.rotation);
                break;
            case 5:
                newBullet = PhotonNetwork.Instantiate("Bullet_Sniper", shotPoint.position, shotPoint.rotation);
                additionalForce +=3;
                break;
            case 6:
                newBullet = PhotonNetwork.Instantiate("Bullet_Rush", shotPoint.position, shotPoint.rotation);
                break;
            case 7:
                player.PV.RPC("SetAnimatorTrigger", RpcTarget.AllBuffered,"WhipAttack");
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(this.transform.position + -Bar.transform.up.normalized * 0.25f, whipBoxSize, angle);
                foreach (Collider2D col in collider2Ds)
                {
                    if (col.transform.tag == "Player")
                    {
                        if (col.GetComponent<Player>() != player)
                        {
                            TurnManager.GetInstance().SetTimeOrEventCount(-1, -1);
                            col.transform.GetComponent<Player>().NearAttacked(30, -Bar.transform.up * launchForce * 500);
                        }

                    }
                }
                break;
            case 8:
                newBullet = PhotonNetwork.Instantiate("Bullet_Heal", shotPoint.position, shotPoint.rotation);
                break;
            case 9:
                newBullet = PhotonNetwork.Instantiate("Bullet_Minecraft", shotPoint.position, shotPoint.rotation);
                break;
            case 10:
                newBullet = PhotonNetwork.Instantiate("Bullet_Mine", shotPoint.position, shotPoint.rotation);
                break;
            case 11:
                newBullet = PhotonNetwork.Instantiate("Bullet_Teleport", shotPoint.position, shotPoint.rotation);
                newBullet.GetComponent<CannonBullet_Teleport>().playerTr = player.transform;
                break;
            case 12:
                StartCoroutine("RapidShot", new object[] { -Bar.transform.up, 12 });
                newBullet = PhotonNetwork.Instantiate("Bullet_Laser", shotPoint.position, shotPoint.rotation);
                break;
            case 13:
                StartCoroutine("RapidShot", new object[]{ -Bar.transform.up, 13 });
                float tmpY = 3;
                if (PhotonNetwork.CurrentRoom != null)
                {
                    if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"] && (int)PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"] != 1)
                    {
                        tmpY = 7;
                    }
                }
                newBullet = PhotonNetwork.Instantiate("Bullet_AngryAngel", new Vector3(Random.Range(-10, 10), tmpY, shotPoint.position.z), shotPoint.rotation);
                break;
            case 14:
                TurnManager.GetInstance().SetTimeOrEventCount(25, 1);
                newBullet = PhotonNetwork.Instantiate("Bullet_Wakrio", shotPoint.position, shotPoint.rotation);
                break;
            case 15:
                newBullet = PhotonNetwork.Instantiate("Bullet_TurnEnd", shotPoint.position, shotPoint.rotation);
                //TurnManager.GetInstance().SetTimeOrEventCount(3, -1);
                break;
        }

        if (newBullet != null)
        {
            if (AmmoManager.GetInstance().WeaponIndex != 13)
            {
                newBullet.GetComponent<Rigidbody2D>().velocity = -Bar.transform.up * (launchForce + additionalForce);
            }
            else if(AmmoManager.GetInstance().WeaponIndex != 4 && AmmoManager.GetInstance().WeaponIndex != 12)
            {
                newBullet.GetComponent<Rigidbody2D>().velocity = -Bar.transform.up * (maxLaunchForce + additionalForce);
            }
            else
            {
                newBullet.GetComponent<Rigidbody2D>().velocity = new Vector3((-Bar.transform.up).x, -4);
            }

        }
        if (AmmoManager.GetInstance().WeaponIndex != 4 && AmmoManager.GetInstance().WeaponIndex != 12 && AmmoManager.GetInstance().WeaponIndex !=13)
        {
            launchForce = 0;
        }
        launchPowerImage.transform.localScale = new Vector3(0, 1, 1);
        player.anim.SetTrigger("Shot");


        player.PV.RPC("SetEmotionRPC", RpcTarget.All, "Idle");
        //player.SR_Emotion.GetComponent<SpriteResolver>().SetCategoryAndLabel("Emotion", "Idle");
        //player.SR_Emotion.GetComponent<SpriteResolver>().ResolveSpriteToSpriteRenderer();
    }
    IEnumerator RapidShot(object[] objs)
    {
        //Vector3 vel, int weaponIndex
        TurnManager.GetInstance().SetTimeOrEventCount(5, 1);

        launchForce -= 1;


        if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
            launchDelay = 0.6f;

        yield return new WaitForSeconds(0.1f);

        if (launchForce <= 0)
        {
            if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
                launchDelay = 1;
            else
                launchDelay = 100;


            player.PV.RPC("SetEmotionRPC", RpcTarget.All, "Idle");
            //player.SR_Emotion.GetComponent<SpriteResolver>().SetCategoryAndLabel("Emotion", "Angry");
            //player.SR_Emotion.GetComponent<SpriteResolver>().ResolveSpriteToSpriteRenderer();
        }
        else
        {
            GameObject newBullet;
            if ((int)objs[1] == 4)
            {
                newBullet = PhotonNetwork.Instantiate("Bullet_DooDoo", shotPoint.position, shotPoint.rotation);
                newBullet.GetComponent<Rigidbody2D>().velocity = -Bar.transform.up * (maxLaunchForce);
                StartCoroutine("RapidShot", new object[] { Vector3.zero, 4 });
            }
            else if ((int)objs[1] == 12)
            {
                newBullet = PhotonNetwork.Instantiate("Bullet_Laser", shotPoint.position, shotPoint.rotation);
                newBullet.GetComponent<Rigidbody2D>().velocity = (Vector3)objs[0] * (maxLaunchForce);
                StartCoroutine("RapidShot", new object[] { (Vector3)objs[0], 12 });
            }
            else
            {
                launchForce += 0.6f;

                float tmpY=3;
                if (PhotonNetwork.CurrentRoom != null)
                {
                    if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"] && (int)PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"] != 2)
                    {
                        tmpY =7;
                    }
                }

                newBullet = PhotonNetwork.Instantiate("Bullet_AngryAngel", new Vector3(Random.Range(-10,10), tmpY, shotPoint.position.z), shotPoint.rotation);
                newBullet.GetComponent<Rigidbody2D>().velocity =new Vector3(((Vector3)objs[0]).x+Random.Range(-.16f,.16f), +Random.Range(-4f, -3f));
                StartCoroutine("RapidShot", new object[] { (Vector3)objs[0], 13 });
            }

        }
    }

    Vector2 PointPosition(float t)
    {
        Vector2 position = (Vector2)shotPoint.position + (direction.normalized * launchForce * t) + 0.5f * Physics2D.gravity * (t * t);
        //Vector2 position = (Vector2)shotPoint.position + (new Vector2(mul * Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle)) * launchForce * t) + 0.5f * Physics2D.gravity * (t * t);
        return position;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.rotation);
            stream.SendNext(launchForce);
        }
        else
        {
            currRotation = (Quaternion)stream.ReceiveNext();
            launchForce = (float)stream.ReceiveNext();
        }
    }    
}
