using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
public class PlayerHealthContent : MonoBehaviour,IPunObservable
{
    public Text nameTxt;
    public Button KickBtn;
    public Image teamColor;
    public Slider teamHealthSlider;

    public Photon.Realtime.Player myPlayer;

    public List<Player> worms;
    private void Start()
    {
        myPlayer = GetComponent<PhotonView>().Owner;

        GetComponent<PhotonView>().RPC("doEnable", RpcTarget.AllBuffered);

        if (PhotonNetwork.IsMasterClient && myPlayer != PhotonNetwork.LocalPlayer)
        {
            KickBtn.onClick.AddListener(() => GameManager.GetInstance().KickPlayer(myPlayer.UserId));
        }
        else
        {
            KickBtn.gameObject.SetActive(false);
        }

        Invoke("initializeWorm", 1f);
        //myPlayer=GetComponent<PhotonView>().Owner;
    }

    public void initializeWorm()
    {
        myPlayer = GetComponent<PhotonView>().Owner;
        Player[] allWorms = FindObjectsOfType<Player>();
        for (int i = 0; i < allWorms.Length; i++)
        {
            if (allWorms[i].PV.Owner == myPlayer)
            {
                worms.Add(allWorms[i]);
            }
        }
    }

    private void Update()
    {
        this.transform.localScale = new Vector3(.9f, .9f, 1);
        if (myPlayer!=null)
        {
            nameTxt.text = myPlayer.NickName;
            teamColor.color = GameManager.GetInstance().ColorByIndex(PhotonTeamExtensions.GetPhotonTeam(myPlayer).Code);

            Player[] allWorms = FindObjectsOfType<Player>();
            List<Player> myWorms = new List<Player>();
            for (int i = 0; i < allWorms.Length; i++)
            {
                if (allWorms[i].PV.Owner == myPlayer)
                {
                    myWorms.Add(allWorms[i]);
                }
            }

            float totalHealth=0;
            for(int i = 0; i < myWorms.Count; i++)
            {
                totalHealth += myWorms[i].Health;
            }

            if (myWorms.Count > 0)
                teamHealthSlider.value = totalHealth / (myWorms.Count * (float)PhotonNetwork.CurrentRoom.CustomProperties["CharacterHealth"]);
            else
                teamHealthSlider.value = 0f;
        }
        else
        {

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(GetComponent<PhotonView>());
        }

        if (GameManager.GetInstance().myPlayerHealthListContent)
        {
            if (GameManager.GetInstance().myPlayerHealthListContent.gameObject != this.gameObject)
            {
                if (GetComponent<PhotonView>().IsMine)
                    PhotonNetwork.Destroy(GetComponent<PhotonView>());
            }
        }
    }

    [PunRPC]
    public void doEnable()
    {
        transform.SetParent(GameManager.GetInstance().PlayerHealthListContainer);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
