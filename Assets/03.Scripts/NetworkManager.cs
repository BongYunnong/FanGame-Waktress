using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    public int JoinOrCreateIndex=0;//0:Create 1:Join by Title 2:JoinRandomly
    public InputField NickNameInput;
    public InputField TitleInput;
    public Dropdown MaxPlayerInRoomDropdown;
    public InputField CertificateInput;
    public string SelectedRoomTitle;
    public Transform RoomListContainer;
    public GameObject RoomListBtnPrefab;

    public Button[] ConnectBtns;

    public List<RoomListBtn> _Listings = new List<RoomListBtn>();

    public GameObject NetworkCanvas;
    public GameObject DisconnectPanel;
    public GameObject PressAnyKeyToStartText;
    private bool pressAnyKeyActive = false;

    public GameObject[] ConnectFailedPanels;
    public GameObject CertificateCompletePanel;
    public GameObject CertificateFailPanel;

    public Text[] roomOptionTexts;
    public Text[] revealRoomSettingTexts;
    public Image[] MapImagePanel;
    public Sprite[] MapImages;
    public GameObject RevealRoomSettingPanel;
    public int MapIndex;

    private bool Disconnecting=false;

    private bool isWooWakGood = false;

    //public static List<Photon.Realtime.Player> Team1Players = new List<Photon.Realtime.Player>();
    //public static List<Photon.Realtime.Player> Team2Players = new List<Photon.Realtime.Player>();

    // Start is called before the first frame update
    void Awake()
    {
        Screen.SetResolution(960, 540, false);
        //PhotonNetwork.SendRate = 60;
        //PhotonNetwork.SerializationRate = 30;
    }
    private void Start()
    {
        //PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsReady", false }, { "IsKicekd", false },{"CharacterCount",1 },{"Customize",101 } ,{ "ID", PhotonNetwork.LocalPlayer.UserId } });

        //PhotonHandler.AppQuits();
        //PhotonMessageInfo photonMessageInfo;

        Invoke("PressAnyKeyActive",1f);
    }

    private void PressAnyKeyActive()
    {
        pressAnyKeyActive = true;
    }

    private void Update()
    {
        if (Input.anyKey&& pressAnyKeyActive)
        {
            PressAnyKeyToStartText.SetActive(false);
            DisconnectPanel.SetActive(true);
        }

        if (NickNameInput.text=="" || NickNameInput.text.Length>10)
        {
            ConnectBtns[0].enabled = false;
            ConnectBtns[1].enabled = false;
            ConnectBtns[2].enabled = false;
        }
        else
        {
            if (TitleInput.text == "")
            {
                ConnectBtns[0].enabled = false;
            }
            else
            {
                ConnectBtns[0].enabled = true;
            }

            if (SelectedRoomTitle == "")
            {
                ConnectBtns[2].enabled = false;
            }
            else
            {
                ConnectBtns[2].enabled = true;
            }
            ConnectBtns[1].enabled = true;
        }


        MapIndex = Mathf.Clamp(MapIndex, 0, TileManager.GetInstance().MapAsset.Length);
        if (PhotonNetwork.InRoom)
        {
            if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
            {
                revealRoomSettingTexts[0].text = TileManager.GetInstance().MapAsset[MapIndex].name;
                MapImagePanel[0].sprite = MapImages[MapIndex];

                MapImagePanel[1].sprite = MapImages[(int)PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"]];
                revealRoomSettingTexts[1].text = TileManager.GetInstance().MapAsset[(int)PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"]].name;
                revealRoomSettingTexts[2].text = "캐릭터 체력 : "+((float)PhotonNetwork.CurrentRoom.CustomProperties["CharacterHealth"]).ToString();
                revealRoomSettingTexts[3].text = "캐릭터 수   : " + ((int)PhotonNetwork.CurrentRoom.CustomProperties["InitCharacterCount"]).ToString();
                revealRoomSettingTexts[4].text = "턴 시간     : " + ((float)PhotonNetwork.CurrentRoom.CustomProperties["TurnTime"]).ToString();
                revealRoomSettingTexts[5].text = "보급 확률   : " + ((int)PhotonNetwork.CurrentRoom.CustomProperties["SupplyProbability"]).ToString()+"%";
            }
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            /*
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DisconnectFunc();
            }
            */
            if ((bool)PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsKicked"))
            {
                if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["IsKicked"])
                {
                    DisconnectFunc();
                }
            }
        }
    }



    public void AddMapIndex(int param)
    {
        MapIndex += param;
        MapIndex = Mathf.Clamp(MapIndex, 0, 3);
    }

    public void ApplyRoomProperties()
    {
        //TileManager.GetInstance().MapIndex = MapIndex;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "CharacterHealth", float.Parse(roomOptionTexts[0].text) }, { "InitCharacterCount", int.Parse(roomOptionTexts[1].text) }, { "TurnTime", float.Parse(roomOptionTexts[2].text) }, { "MapIndex", MapIndex }, { "SupplyProbability", int.Parse(roomOptionTexts[3].text) } });
    }


    public void ChangeConnectIndex(int index)
    {
        JoinOrCreateIndex = index;
    }
    public void ChangeSelectedRoomTitle(Text index)
    {
        SelectedRoomTitle = index.text;
    }

    public override void OnConnectedToMaster()
    {
        ////base.OnConnectedToMaster();
        //if (JoinOrCreateIndex == 1)
        //{
        //    PhotonNetwork.JoinLobby();
        //}
        //else
        //{
        //}
        JoinOrCreateRoom();

    }
    public void JoinLobby()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public void LeaveLobby()
    {
        /*
        if (JoinOrCreateIndex == 1){
            //index가 1인 상태에서 lobby를 나가는 경우는 룸에 입장할때밖에 없음
            PhotonNetwork.LeaveLobby();
        }
        else
        {
            //index가 1이 아닌데 로비를 나가고싶다는(이미로비를 나간상태) 것은 연결을 끊고싶다는것.
            PhotonNetwork.Disconnect();
        }
        */
        PhotonNetwork.Disconnect();

    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();

        /*
        //그냥 lobby에서 나올때는 아무효과 없고(방 리스트보다가 안보는 것도 index가 이미다른것으로대체), 방 리스트를통해 접속할때는 JoinOrCrateRoom();
        if (JoinOrCreateIndex == 1)
        {
            JoinOrCreateRoom();
        }
        */
    }

    public void CertificateFunc()
    {
        if (CertificateInput.text == "wakLegend0724")
        {
            isWooWakGood = true;
            CertificateCompletePanel.SetActive(true);
        }
        else
        {
            CertificateFailPanel.SetActive(true);
        }
    }

    public void Connect() {
        GameManager.GetInstance().StartCoroutine("FadeInOut", 0f);
        StartCoroutine("ConnectCoroutine");
    }
    IEnumerator ConnectCoroutine()
    {
        yield return new WaitForSeconds(1f);
        /*
        if (JoinOrCreateIndex == 1)
        {
            PhotonNetwork.LeaveLobby();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        */
        PhotonNetwork.ConnectUsingSettings();

    }
    public void DisconnectFunc()
    {
        if (!Disconnecting)
        {
            Disconnecting = true;


            /*
            //우선 내 화면에 있는 것들은 다 지움, 다른 사람들의 화면에서는 어차피 내가 disconnect되면 주인이 없어서 다 사라짐
            foreach (Player player in FindObjectsOfType<Player>())
            {
                player.DestroyFunc();
            }
            foreach (PlayerListContent player in FindObjectsOfType<PlayerListContent>())
            {
                Destroy(player.gameObject);
            }
            foreach (PlayerHealthContent player in FindObjectsOfType<PlayerHealthContent>())
            {
                Destroy(player.gameObject);
            }
            foreach (Supply supply in FindObjectsOfType<Supply>())
            {
                Destroy(supply.gameObject);
            }
            */
            GameManager.GetInstance().LeaveRoom();

            GameManager.GetInstance().StartCoroutine("FadeInOut",0f);
            StartCoroutine("DisconnectCoroutine");
        }
    }
    IEnumerator DisconnectCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.Disconnect();
        Disconnecting = false;
    }

    public override void OnConnected()
    {
        base.OnConnected();
       
    }

    public void JoinOrCreateRoom()
    {
        //PhotonNetwork.CountOfPlayers <=4 ==>  4명 일반인 + 왁굳
        if (PhotonNetwork.CountOfPlayers <= 16 || isWooWakGood)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsReady", false }, { "CharacterCount", 1 }, { "Customize", 101 }, { "IsKicked", false } });

            PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;

            if (JoinOrCreateIndex == 0)
            {
                if (isWooWakGood)
                {
                    RoomOptions roomOptions = new RoomOptions() { };
                    roomOptions.MaxPlayers = 4;
                    roomOptions.IsVisible = true;
                    string[] customRoomProps = new string[5];
                    customRoomProps[0] = "IsGameStarted";
                    customRoomProps[1] = "CharacterHealth";
                    customRoomProps[2] = "InitCharacterCount";
                    customRoomProps[3] = "TurnTime";
                    customRoomProps[4] = "SupplyProbability";

                    roomOptions.CustomRoomPropertiesForLobby = customRoomProps;
                    roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "IsGameStarted", false }, { "CharacterHealth", 100f }, { "InitCharacterCount", 3 }, { "TurnTime", 15f }, { "MapIndex", MapIndex },{"SupplyProbability", 25} };
                    roomOptions.MaxPlayers = (byte)(MaxPlayerInRoomDropdown.value + 1);
                    PhotonNetwork.CreateRoom(TitleInput.text, roomOptions, TypedLobby.Default);
                }
                else
                {
                    ConnectFailedPanels[4].SetActive(true);
                    PhotonNetwork.Disconnect();
                }
            }
            else if (JoinOrCreateIndex == 1)
            {
                PhotonNetwork.JoinRoom(SelectedRoomTitle);
            }
            else if (JoinOrCreateIndex == 2)
            {
                PhotonNetwork.JoinRandomRoom();
            }

            PhotonTeamExtensions.JoinTeam(PhotonNetwork.LocalPlayer, 0);
        }
        else
        {

            ConnectFailedPanels[3].SetActive(true);
            PhotonNetwork.Disconnect();
        }
    }

    public override void OnJoinedRoom()
    {
        NetworkCanvas.SetActive(false);

        //StartCoroutine("DestroyBullet");
        GameManager.GetInstance().JoinRoom();
        GameManager.GetInstance().InitializeRoom();

    }




    public override void OnDisconnected(DisconnectCause cause)
    {
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsReady", false }, { "CharacterCount", 1 }, { "Customize", 101 }, { "IsKicked", false } });
        foreach (RoomListBtn rl in _Listings)
        {
            if (rl != null)
            {
                Destroy(rl.gameObject);
            }
        }
        _Listings.Clear();

        //PhotonNetwork.ConnectUsingSettings();
        //PhotonNetwork.JoinLobby();
        NetworkCanvas.SetActive(true);
        PhotonTeamExtensions.LeaveCurrentTeam(PhotonNetwork.LocalPlayer);
        //GameManager.GetInstance().LeaveRoom();
        
    }
    

    public void SwitchTeam()
    {
        if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
        {
            int tmpCode = (int)PhotonTeamExtensions.GetPhotonTeam(PhotonNetwork.LocalPlayer).Code + 1;
            print(tmpCode + "/" + PhotonTeamsManager.Instance.GetAvailableTeams().Length);
            if (tmpCode >= PhotonTeamsManager.Instance.GetAvailableTeams().Length)
            {
                tmpCode = 0;
            }
            PhotonTeamExtensions.SwitchTeam(PhotonNetwork.LocalPlayer, (byte)(tmpCode));
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        GameManager.GetInstance().ActiveFalseJoinOrCreateRoomAcceptPanels();
        if (returnCode == 32766)
        {
            //방 이름이 이미 존재할 때
            ConnectFailedPanels[0].SetActive(true);
            PhotonNetwork.Disconnect();
        }
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        print(returnCode + " / " + message);
        GameManager.GetInstance().ActiveFalseJoinOrCreateRoomAcceptPanels();
        if (returnCode == 32760)
        {
            //남은 방이 없을때
            ConnectFailedPanels[1].SetActive(true);
            PhotonNetwork.Disconnect();
        }
        else if (returnCode == 32765)
        {
            //방이 꽉 찼을때
            ConnectFailedPanels[2].SetActive(true);
            PhotonNetwork.Disconnect();
        }
        else if (returnCode == 32758)
        {
            //방이 없을떄
            ConnectFailedPanels[1].SetActive(true);
            PhotonNetwork.Disconnect();
        }
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        GameManager.GetInstance().ActiveFalseJoinOrCreateRoomAcceptPanels();
        if (returnCode == 32760)
        {
            //남은 방이 없을때
            ConnectFailedPanels[1].SetActive(true);
            PhotonNetwork.Disconnect();
        }

    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList)
            {
                //roomList에 쓸모없는 list가 있다면

                int index = _Listings.FindIndex(x => x.roomName == info.Name);
                if (index != -1)
                {
                    //roomList에 내 리스트에 있는것이 없다면, 내꺼 지움
                    Destroy(_Listings[index].gameObject);
                    _Listings.RemoveAt(index);
                }
                else
                {
                    RoomListBtn tmp = Instantiate(RoomListBtnPrefab, RoomListContainer).GetComponent<RoomListBtn>();
                    tmp.titleTxt.text = info.Name;
                    tmp.roomName = info.Name;
                    tmp.memberCountTxt.text = "[" + info.PlayerCount + "/" + info.MaxPlayers + "]";

                    if ((bool)info.CustomProperties.ContainsKey("IsGameStarted"))
                    {
                        tmp.GetComponent<Image>().color = Color.white;
                        if ((bool)info.CustomProperties["IsGameStarted"])
                            tmp.GetComponent<Button>().interactable = false;
                        else
                            tmp.GetComponent<Button>().interactable = true;
                    }
                    //else
                    //{
                    //    info.CustomProperties.Add( "IsGameStarted", false );
                    //    tmp.GetComponent<Image>().color = Color.yellow;
                    //    tmp.GetComponent<Button>().interactable = false;
                    //}
                    _Listings.Add(tmp);
                }
            }
            else
            {
                int index = _Listings.FindIndex(x => x.roomName == info.Name);
                if (index == -1)
                {
                    //roomList에 내 리스트에 있는것이 있다면 방을 생성함
                    RoomListBtn tmp = Instantiate(RoomListBtnPrefab, RoomListContainer).GetComponent<RoomListBtn>();
                    tmp.titleTxt.text = info.Name;
                    tmp.roomName = info.Name;
                    tmp.memberCountTxt.text = "[" + info.PlayerCount + "/" + info.MaxPlayers + "]";

                    if ((bool)info.CustomProperties.ContainsKey("IsGameStarted"))
                    {
                        tmp.GetComponent<Image>().color = Color.white;

                        if ((bool)info.CustomProperties["IsGameStarted"])
                            tmp.GetComponent<Button>().interactable = false;
                        else
                            tmp.GetComponent<Button>().interactable = true;
                    }

                    
                    //else
                    //{
                    //    info.CustomProperties.Add("IsGameStarted", false);

                    //    tmp.GetComponent<Image>().color = Color.yellow;
                    //    tmp.GetComponent<Button>().interactable = false;
                    //}

                    _Listings.Add(tmp);
                }
                else
                {
                    if (info.PlayerCount != _Listings[index].playerCount)
                    {
                        _Listings[index].playerCount = info.PlayerCount;
                        _Listings[index].memberCountTxt.text = "[" + info.PlayerCount + "/" + info.MaxPlayers + "]";
                    }
                }
            }
        }
    }

    /*
    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag("Bullet")) GO.GetComponent<CannonBullet>().DestroyBullet();
    }
    */

}
