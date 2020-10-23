using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;
using Cinemachine;
using UnityEngine.Experimental.U2D.Animation;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    //public int TeamIndex;

    //public int myID;
    public int myIndex_Team;

    public Rigidbody2D rb;
    public Animator anim;
    //public SpriteRenderer sr;
    public SpriteRenderer SR_Head;
    public SpriteRenderer SR_Body;
    public SpriteRenderer SR_Emotion;
    public SpriteRenderer SR_Hat;

    public PhotonView PV;
    public Text NickNameText;
    public Text MyIndexText;
    public Image HealthImage;

    public Image ChatPanel;
    public Text ChatText;
    private float ChatAppearTime;

    bool dying = false;
    public float Health = 100;

    public static bool CanMove = true;
    public bool Jumping = false;
    public int jumpIndex = 0;


    bool attacked = false;
    bool isGround;
    public float jumpForce = 500;
    Vector3 curPos;

    public AudioSource footStepSound;
    public AudioClip[] footStepClips;
    public AudioSource JumpSound;


    private void Start()
    {
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        if (PhotonTeamExtensions.GetPhotonTeam(PV.Owner) == PhotonTeamExtensions.GetPhotonTeam(PhotonNetwork.LocalPlayer))
        {
            NickNameText.color = Color.green;
        }
        else
        {
            NickNameText.color = Color.red;
        }
        Health = (float)PhotonNetwork.CurrentRoom.CustomProperties["CharacterHealth"];
    }

    public void FootStep()
    {
        if (isGround)
        {
            footStepSound.clip = footStepClips[Random.Range(0, footStepClips.Length)];
            footStepSound.Play();
        }
        else
        {
            footStepSound.Stop();
        }
    }

    void Update()
    {
        if (PhotonNetwork.CurrentRoom != null) {
            if (HealthImage != null)
                HealthImage.fillAmount = Health / (float)PhotonNetwork.CurrentRoom.CustomProperties["CharacterHealth"];
            Health = Mathf.Clamp(Health, -1, (float)PhotonNetwork.CurrentRoom.CustomProperties["CharacterHealth"]);
        }


        /*
        MyIndexText.color =GameManager.GetInstance().ColorByIndex(PhotonTeamExtensions.GetPhotonTeam(PV.Owner).Code);
        if (MyIndexText != null)
        {
            MyIndexText.text = PhotonTeamExtensions.GetPhotonTeam(PV.Owner).Code.ToString();
           
        }
        //MyIndexText.text = isGround + "isground";
        */

        if (GameManager.GetInstance().ChatInput.isFocused)
        {
            CanMove = false;
        }
        else
        {
            CanMove = true;
        }

        bool tmpGround = isGround;
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(0, -0.25f), 0.1f, 1 << LayerMask.NameToLayer("Ground"));
        if(tmpGround==false && isGround == true && rb.velocity.y<=-5f)
        {
            Attacked(-rb.velocity.y, new Vector3(0, 0, 0));
        }


        anim.SetBool("IsGrounded", isGround);

        if (ChatText.text != "")
        {
            ChatAppearTime -= Time.deltaTime;
            if (ChatAppearTime <= 0)
            {
                ChatText.text = "";
                ChatPanel.gameObject.SetActive(false);
            }
        }




        if (Health <= 0 && !dying)
        {
            dying = true;

            if(PV.IsMine)
                PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "CharacterCount", (int)PhotonNetwork.LocalPlayer.CustomProperties["CharacterCount"] - 1 } });

            TurnManager.GetInstance().killedWormsName += "["+PV.Owner.NickName+"]";

            if(PV.Owner.IsLocal)
                OptionSettingManager.GetInstance().Play("Die_our",false);
            else
                OptionSettingManager.GetInstance().Play("Die-your", false);


            //죽는 애니메이션

            if (PhotonNetwork.IsMasterClient|| PV.IsMine)
            {
                StartCoroutine("DieCoroutine");
            }
        }


        if (rb.velocity.x >= .8f)
        {
            //sr.flipX = false;
            SR_Head.transform.localScale = new Vector3(1, 1, 1);
            //SR_Head.flipX = false;
            SR_Body.transform.localScale = new Vector3(1, 1, 1);
            SR_Body.transform.localPosition = new Vector3(0.05f, 0, 0);

            //SR_Emotion.flipY = false;
            //SR_Emotion.transform.localScale = new Vector3(1, 1, 1);
            //SR_Hat.transform.localScale = new Vector3(1, 1, 1);
        }
        else if (rb.velocity.x <= -.8f)
        {
            //sr.flipX = true;
            SR_Head.transform.localScale = new Vector3(-1, 1, 1);
            //SR_Head.flipX = true;
            SR_Body.transform.localScale = new Vector3(-1, 1, 1);
            SR_Body.transform.localPosition = new Vector3(-0.05f, 0, 0);

            //SR_Emotion.transform.localScale = new Vector3(1, -1, 1);
            //SR_Hat.transform.localScale = new Vector3(1, -1, 1);
        }

        if (PV.IsMine)
        {
            if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
            {
                Move();
            }
            else
            {
                //if ((TurnManager.TurnIndex % 4) == (int)PhotonTeamExtensions.GetPhotonTeam(PV.Owner).Code
                //    && GameManager.GetInstance().myCharacters.IndexOf(this) == ((TurnManager.TurnIndex / 4) % GameManager.GetInstance().myCharacters.Count))
                if (GameManager.GetInstance().currentCharacter == this)
                {
                    Move();
                    //PV.RPC("SetCurrentCharacterRPC", RpcTarget.AllBuffered);
                }
                else
                {
                    //턴 끝남
                    //rb.velocity = new Vector2(0, rb.velocity.y);
                    anim.SetBool("Walk", false);
                }
            }
        }


        SR_Head.GetComponent<SpriteResolver>().SetCategoryAndLabel("Head", CustomizeIndexToLabel((int)PV.Owner.CustomProperties["Customize"] / 100));
        SR_Head.GetComponent<SpriteResolver>().ResolveSpriteToSpriteRenderer();

        SR_Body.GetComponent<SpriteResolver>().SetCategoryAndLabel("Body", CustomizeIndexToLabel((int)PV.Owner.CustomProperties["Customize"] / 100));
        SR_Body.GetComponent<SpriteResolver>().ResolveSpriteToSpriteRenderer();

        SR_Hat.GetComponent<SpriteResolver>().SetCategoryAndLabel("Hat", CustomizeIndexToLabel((int)PV.Owner.CustomProperties["Customize"] % 100));
        SR_Hat.GetComponent<SpriteResolver>().ResolveSpriteToSpriteRenderer();
    }

    public void FixedUpdate()
    {

    }

    string CustomizeIndexToLabel(int index)
    {
        string outString = "A";
        switch (index)
        {
            case 1:
                outString = "A";
                break;
            case 2:
                outString = "B";
                break;
            case 3:
                outString = "C";
                break;
            case 4:
                outString = "D";
                break;
            case 5:
                outString = "E";
                break;
            case 6:
                outString = "F";
                break;
            case 7:
                outString = "G";
                break;
            case 8:
                outString = "H";
                break;
            case 9:
                outString = "I";
                break;
            case 10:
                outString = "J";
                break;
            case 11:
                outString = "K";
                break;
            case 12:
                outString = "L";
                break;
            case 13:
                outString = "M";
                break;
            case 14:
                outString = "N";
                break;
            case 15:
                outString = "O";
                break;
            case 16:
                outString = "P";
                break;
            case 17:
                outString = "Q";
                break;
            case 18:
                outString = "R";
                break;
            case 19:
                outString = "S";
                break;
            case 20:
                outString = "T";
                break;
        }
        return outString;
    }

    public void Move()
    {
        float axis = Input.GetAxisRaw("Horizontal");


        jumpIndex = 0;

        if (!attacked && !Jumping)
        {
            if (axis != 0)
            {
                if (CanMove && GetComponentInChildren<Cannon>().launchForce < 1)
                {
                    anim.SetBool("Walk", true);
                    //PV.RPC("FlipXRpc", RpcTarget.AllBuffered, axis);
                    //rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(2 * axis, rb.velocity.y), Time.deltaTime * 10f);
                    rb.velocity = new Vector2(1 * axis, rb.velocity.y);
                }
            }
            else
            {
                anim.SetBool("Walk", false);
                //rb.velocity = Vector2.Lerp(rb.velocity, new Vector2(0, rb.velocity.y), Time.deltaTime * 10f);
                if (isGround)
                    rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        if (CanMove)
        {
            if (isGround && !attacked && !Jumping)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    jumpForce = 1.5f;
                }
                else if (Input.GetKey(KeyCode.Space) && jumpForce >= 1f)
                {
                    jumpForce += 2 * Time.deltaTime;
                    jumpForce = Mathf.Clamp(jumpForce, 0, 4);
                    GetComponentInChildren<Cannon>().launchPowerImage.transform.localScale = new Vector3((jumpForce / 4), 0.05f, 1);
                }
                else if (Input.GetKeyUp(KeyCode.Space) && jumpForce > 1f)
                {
                    rb.velocity = (-GetComponentInChildren<Cannon>().Bar.transform.up+new Vector3(0,0.25f,0)) * (jumpForce);
                    GetComponentInChildren<Cannon>().launchPowerImage.transform.localScale = new Vector3(0, 1, 1);


                    jumpForce = 0;

                    //JumpSound.Play();
                    OptionSettingManager.GetInstance().Play("Jump", true);

                    if (Random.Range(0, 1f) > 0.3f)
                    {
                        OptionSettingManager.GetInstance().Play("Wak_Emo1", true);
                    }
                    anim.SetTrigger("Jump");


                    PV.RPC("SetEmotionRPC", RpcTarget.AllBuffered, "Happy");
                    //SR_Emotion.GetComponent<SpriteResolver>().SetCategoryAndLabel("Emotion", "Happy");
                    //SR_Emotion.GetComponent<SpriteResolver>().ResolveSpriteToSpriteRenderer();

                    Jumping = true;
                    Invoke("JumpEnd", 1f);
                }
            }
        }
    }
    void JumpEnd()
    {

        PV.RPC("SetEmotionRPC", RpcTarget.AllBuffered, "Idle");
        //SR_Emotion.GetComponent<SpriteResolver>().SetCategoryAndLabel("Emotion", "Idle");
        //SR_Emotion.GetComponent<SpriteResolver>().ResolveSpriteToSpriteRenderer();

        Jumping = false;
    }

    [PunRPC]
    public void ForceRPC(float damage, Vector3 force)
    {
        attacked = true;
        StartCoroutine("cancelAttacked");
        rb.velocity = Vector3.zero;

        damage = Mathf.Round(damage);

        if (damage > 0)
        {
            FloatingTextController.CreateFloatingText(damage.ToString(), transform, Color.red);
        }
        else if(damage<0)
        {
            FloatingTextController.CreateFloatingText("+"+(-1*damage).ToString(), transform, Color.green);
        }
        OptionSettingManager.GetInstance().Play("Attacked", true);

        if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
            Health -= damage;
        rb.AddForce(force);
    }

    public void Attacked(float damage, Vector3 force)
    {

        if (PV.IsMine)
            PV.RPC("ForceRPC", RpcTarget.AllBuffered, new object[] { damage, force });

        anim.SetTrigger("Jump");


        PV.RPC("SetEmotionRPC", RpcTarget.AllBuffered, "Hurt");
        //SR_Emotion.GetComponent<SpriteResolver>().SetCategoryAndLabel("Emotion", "Hurt");
        //SR_Emotion.GetComponent<SpriteResolver>().ResolveSpriteToSpriteRenderer();
    }

    public void NearAttacked(float damage, Vector3 force)
    {
        PV.RPC("ForceRPC", RpcTarget.AllBuffered, new object[] { damage, force });
        anim.SetTrigger("Jump");
        PV.RPC("SetEmotionRPC", RpcTarget.AllBuffered, "Hurt");
    }

    IEnumerator cancelAttacked()
    {
        yield return new WaitForSeconds(1f);
        while (!isGround)
            yield return null;

        attacked = false;

        yield return new WaitForSeconds(1.5f);

        PV.RPC("SetEmotionRPC",RpcTarget.AllBuffered, "Idle");
        //SR_Emotion.GetComponent<SpriteResolver>().SetCategoryAndLabel("Emotion", "Idle");
        //SR_Emotion.GetComponent<SpriteResolver>().ResolveSpriteToSpriteRenderer();
    }

    public void SetCurrentCharacter()
    {
        PV.RPC("SetCurrentCharacterRPC", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void SetCurrentCharacterRPC()
    {
        //GetComponentInChildren<Cannon>().launchDelay = -1f;
        GameManager.GetInstance().currentCharacter = this;
        GetComponentInChildren<Cannon>().launchDelay = 3f;
        TurnManager.GetInstance().SetCam();
    }
    [PunRPC]
    public void SetEmotionRPC(string emo)
    {
        SR_Emotion.GetComponent<SpriteResolver>().SetCategoryAndLabel("Emotion", emo);
        SR_Emotion.GetComponent<SpriteResolver>().ResolveSpriteToSpriteRenderer();
    }
    [PunRPC]
    public void SetAnimatorTrigger(string name)
    {
        anim.SetTrigger(name);
    }
    IEnumerator DieCoroutine()
    {
        GameManager.GetInstance().SetVCTargetPos(this.transform.position);
        TurnManager.GetInstance().SetTimeOrEventCount(3, 1);

        //킬 로그 뜨도록
        TurnManager.GetInstance().RevealKillLog();
        yield return new WaitForSeconds(1f);

        GameObject newBullet = PhotonNetwork.Instantiate("Bullet_Dead", transform.position, Quaternion.identity);
        newBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0.1f);

        yield return new WaitForSeconds(0.1f);
        DestroyFunc();
        TurnManager.GetInstance().SetTimeOrEventCount(3, -1);
    }
    public void DestroyFunc()
    {
        PV.RPC("DestroyRPC", RpcTarget.AllBuffered);//AllBuffered로 해야 제대로 사라져 복제버그 X
    }
    [PunRPC]
    public void DestroyRPC()
    {
        Destroy(gameObject);
    }

    [PunRPC]
    void ChatRPC(string msg)
    {
        OptionSettingManager.GetInstance().Play("Chat",false);
        ChatRPCDelegate(msg);
    }

    void ChatRPCDelegate(string msg)
    {
        ChatPanel.gameObject.SetActive(true);
        ChatText.text = msg;
        ChatAppearTime = 4;
    }

    //public void SendChat(string msg)
    //{
    //    ChatPanel.gameObject.SetActive(true);
    //    ChatText.text = msg;
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(PV.IsMine && collision.transform.tag == "Ground")
        {
            if (collision.transform.GetComponent<TileScript>())
            {
                if (collision.transform.GetComponent<TileScript>().MineInstalled)
                {
                    GameObject newBullet = PhotonNetwork.Instantiate("Bullet_Dead", transform.position, Quaternion.identity);
                    newBullet.GetComponent<CannonBullet>().explosionPower = 40;
                    newBullet.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0.1f);
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}