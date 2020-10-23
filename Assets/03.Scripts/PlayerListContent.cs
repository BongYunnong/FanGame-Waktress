﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
public class PlayerListContent : MonoBehaviour,IPunObservable
{
    public Text nameTxt;
    public Toggle readyToggle;
    public Button KickBtn;
    public Image teamColor;

    public Photon.Realtime.Player myPlayer;
    private void Start()
    {
        myPlayer = GetComponent<PhotonView>().Owner;
        if (PhotonNetwork.IsMasterClient && myPlayer != PhotonNetwork.LocalPlayer)
        {
            KickBtn.onClick.AddListener(() => GameManager.GetInstance().KickPlayer(myPlayer.UserId));
        }
        else
        {
            KickBtn.gameObject.SetActive(false);
        }
        GetComponent<PhotonView>().RPC("doEnable", RpcTarget.AllBuffered);


    }
    private void Update()
    {
        this.transform.localScale = new Vector3(.9f, .9f, 1);
        if (myPlayer!=null)
        {
            nameTxt.text = myPlayer.NickName;
            teamColor.color = GameManager.GetInstance().ColorByIndex(PhotonTeamExtensions.GetPhotonTeam(myPlayer).Code);
            readyToggle.isOn = (bool)myPlayer.CustomProperties["IsReady"];
        }
        else
        {
            if(PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(GetComponent<PhotonView>());
        }

        if (GameManager.GetInstance().myPlayerListContent)
        {
            if (GameManager.GetInstance().myPlayerListContent.gameObject != this.gameObject)
            {
                if (GetComponent<PhotonView>().IsMine)
                    PhotonNetwork.Destroy(GetComponent<PhotonView>());
            }
        }
    }

    [PunRPC] public void doEnable() {
        transform.SetParent(GameManager.GetInstance().PlayerListContainer);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
