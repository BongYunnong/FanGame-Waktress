using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;
using Cinemachine;

using DG.Tweening;

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private static GameManager instance;
    public static GameManager GetInstance()
    {
        if (!instance)
        {
            instance = GameObject.FindObjectOfType<GameManager>();
            if (!instance)
                Debug.LogError("There needs to be one active MyClass script on a GameObject in your scene.");
        }

        return instance;
    }

    private PhotonView PV;

    public CinemachineVirtualCamera VC;
    private float shakeTimer=0;
    private bool cameraControl=true;
    public Vector2[] minmax;



    public static bool GameStarted = false;
    public bool isReady = false;

    public bool AngleWithW = false;

    //public int maxCharacterCount = 3;
    public List<Player> myCharacters = new List<Player>();
    public Player currentCharacter;

    public GameObject PanelForGame;
    public GameObject PanelForLobby;
    public GameObject RoomOptBtn;
    public GameObject ReadyBtn;
    public GameObject GameStartFailedPanel;

    public Transform PlayerListContainer;
    public GameObject PlayerListContent;


    public Transform PlayerHealthListContainer;
    public GameObject PlayerHealthListContent;

    //private List<GameObject> playerContentList=new List<GameObject>();
    public PlayerListContent myPlayerListContent;
    public PlayerHealthContent myPlayerHealthListContent;


    public GameObject[] JoinOrCreateRoomAcceptPanels;

    public GameObject ChatPanel;

    public GameObject ChatPanelActiveBtn;
    public InputField ChatInput;
    public Text[] ChatText;

    public GameObject WinnerPanel;
    public GameObject LoserPanel;
    public Animator FadeAnim;

    public GameObject TextBox;
    public GameObject TutorialDialogue;

    public GameObject[] HideWhenMovieMode;
    
    public void ToggleAngleWithW()
    {
        AngleWithW = !AngleWithW;
    }

    
    public void ChatPanelEnter()
    {
        if (!ChatPanel.active)
        {
            ChatPanel.SetActive(true);
            ChatPanelActiveBtn.SetActive(false);
        }

        if (ChatInput.text != "")
        {
            Send();
        }

        ChatInput.ActivateInputField();
        ChatInput.Select();
    }
    
    public void ShakeCamera(float intensity,float time)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = VC.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
        shakeTimer = time;
    }

    public void SetVCTargetPos(Vector3 pos)
    {
        VC.m_Follow.position = pos;
    }
    private void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
            {
                CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = VC.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
            }
        }

        if (PhotonNetwork.CurrentRoom!=null)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                ChatPanelEnter();
            }

            if (Input.GetKeyDown(KeyCode.Quote))
            {
                if (GameStarted)
                {
                    PanelForGame.SetActive(!PanelForGame.active);
                    PanelForLobby.SetActive(false);
                    for(int i=0;i< HideWhenMovieMode.Length; i++)
                    {
                        HideWhenMovieMode[i].SetActive(PanelForGame.active);
                    }
                }
                else
                {
                    PanelForLobby.SetActive(!PanelForLobby.active);
                    PanelForGame.SetActive(false);
                    for (int i = 0; i < HideWhenMovieMode.Length; i++)
                    {
                        HideWhenMovieMode[i].SetActive(PanelForLobby.active);
                    }
                }
            }

            float scrollData;
            scrollData = Input.GetAxis("Mouse ScrollWheel");
            VC.m_Lens.OrthographicSize = Mathf.Lerp(VC.m_Lens.OrthographicSize, VC.m_Lens.OrthographicSize - scrollData * 200, Time.deltaTime);
            VC.m_Lens.OrthographicSize = Mathf.Clamp(VC.m_Lens.OrthographicSize, 1, 7.5f);


            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (currentCharacter != null)
                    VC.m_Follow.position = new Vector3(currentCharacter.transform.position.x, currentCharacter.transform.position.y, VC.m_Follow.position.z);
                else
                {
                    Transform myTr = null;
                    for(int i = 0; i < myCharacters.Count; i++)
                    {
                        if(myCharacters[i]!=null)
                            if(myCharacters[i].PV.IsMine)
                                myTr = myCharacters[i].transform;
                    }
                    if(myTr!=null)
                        VC.m_Follow.position = new Vector3(myTr.position.x, myTr.position.y, VC.m_Follow.position.z);
                }

            }

            if (Input.GetKeyDown(KeyCode.LeftControl)|| Input.GetKeyDown(KeyCode.LeftCommand))
            {
                cameraControl = !cameraControl;
            }

            if (cameraControl)
            {
                Vector3 MoveDir = new Vector3();
                if (Input.mousePosition.x / Screen.width < 0.1f || Input.mousePosition.x / Screen.width > 0.9f)
                {

                    MoveDir.x = Input.mousePosition.x / Screen.width-0.5f;
                }
                if (Input.mousePosition.y / Screen.height < .15f || Input.mousePosition.y / Screen.height > 0.85f)
                {
                    MoveDir.y = Input.mousePosition.y / Screen.height - 0.5f;
                }

                VC.m_Follow.transform.Translate(MoveDir * 0.2f);
                float tmpX = Mathf.Clamp(VC.m_Follow.position.x, minmax[0].x, minmax[1].x);
                float tmpY = Mathf.Clamp(VC.m_Follow.position.y, minmax[0].y, minmax[1].y);
                VC.m_Follow.position = new Vector3(tmpX, tmpY, VC.m_Follow.position.z);
            }

            if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("IsGameStarted"))
            {
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsGameStarted", false } });
            }
            else
            {
                //$%$%
                if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
                {
                    int tmpRemainedTeam = 100;
                    int remainedTeamIndex=-1;
                    /*
                    for (int i = 0; i < 4; i++)
                    {
                        Photon.Realtime.Player[] members;//한 팀 내의 player
                        PhotonTeamsManager.Instance.TryGetTeamMembers((byte)(i), out members);
                        for (int j = 0; j < members.Length; j++)
                        {
                            if (members[j].CustomProperties.ContainsKey("CharacterCount"))
                            {
                                if ((int)members[j].CustomProperties["CharacterCount"] > 0)
                                {
                                    //우선 어떤 캐릭터에서든 characterCount가 1명이라도 넘기면 남은 team+1하고 다른 팀을 본다.
                                    tmpRemainedTeam++;
                                    remainedTeamIndex = i;
                                    break;
                                }
                            }
                        }
                    }
                    */

                    
                    Player[] tmpList = FindObjectsOfType<Player>();
                    if (tmpList.Length > 0)
                    {
                        //캐릭터가 남아있으면 첫번쨰 캐릭터를 먼저 승리한 팀으로 체크
                        remainedTeamIndex = PhotonTeamExtensions.GetPhotonTeam(tmpList[0].PV.Owner).Code;
                        tmpRemainedTeam=1;
                    }
                    for (int j =1; j < tmpList.Length; j++)
                    {
                        //첫번째 캐릭터 이후에 다른 살아있는 캐릭터가 있으면, 그 캐릭터와 현재 캐릭터의 팀을 비교하고, 다르다면 승리 팀을 바꾸고 팀이 2개 이상 남아있다 체크
                        //여기에서 팀을 바꾸지 않아야 remainedTeam이 1인 상태로 다음 if문 통과 가능 + 캐릭터가 남아있는 팀이 승리하게됨
                        if (remainedTeamIndex != PhotonTeamExtensions.GetPhotonTeam(tmpList[j].PV.Owner).Code) {
                            remainedTeamIndex = PhotonTeamExtensions.GetPhotonTeam(tmpList[j].PV.Owner).Code;
                            tmpRemainedTeam++;
                        }
                    }
                    
                    if (tmpRemainedTeam<=1 && GameStarted && TurnManager.GetInstance().TurnIndex>=0)
                    {
                        // PhotonNetwork.Disconnect();
                        GameStarted = false;




                        //게임 끝
                        //PhotonNetwork.LocalPlayer == tmpList[0].PV.Owner
                        //PhotonTeamExtensions.GetPhotonTeam(PhotonNetwork.LocalPlayer).Code == (byte)remainedTeamIndex
                        if (tmpList.Length > 0)
                        {
                            if (PhotonTeamExtensions.GetPhotonTeam(PhotonNetwork.LocalPlayer).Code == (byte)remainedTeamIndex)
                            {
                                WinnerPanel.SetActive(true);
                            }
                            else
                            {
                                LoserPanel.SetActive(true);
                            }
                        }
                        else
                        {
                            LoserPanel.SetActive(true);
                        }
                    }
                }
                else
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        RoomOptBtn.SetActive(true);
                    }
                    else
                    {
                        RoomOptBtn.SetActive(false);
                    }

                    if (PhotonNetwork.CurrentRoom != null)
                    {
                        int readyNum = 0;
                        int reamainedTeam = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            Photon.Realtime.Player[] members;//한 팀 내의 player
                            PhotonTeamsManager.Instance.TryGetTeamMembers((byte)(i), out members);
                            if (members.Length > 0)
                            {
                                reamainedTeam++;
                            }
                            
                            for (int j = 0; j < members.Length; j++)
                            {
                                if (members[j].CustomProperties.ContainsKey("IsReady"))
                                {
                                    if ((bool)members[j].CustomProperties["IsReady"])
                                    {
                                        //우선 어떤 캐릭터에서든 characterCount가 1명이라도 넘기면 남은 team+1하고 다른 팀을 본다.
                                        readyNum++;
                                    }
                                }
                            }
                            
                        }
                        /*
                        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
                        {
                            if (PhotonNetwork.PlayerList[i].CustomProperties.ContainsKey("IsReady"))
                            {
                                if ((bool)PhotonNetwork.PlayerList[i].CustomProperties["IsReady"])
                                    readyNum++;
                            }
                        }
                        */
                        if (PhotonNetwork.CurrentRoom.PlayerCount <= readyNum)
                        {
                            if (!GameStarted)
                            {
                                if (reamainedTeam > 1)
                                {
                                    //GameStarted = true;
                                    //PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsGameStarted", true } });
                                    
                                    GameStarted = true;
                                    PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsGameStarted", true } });

                                    GetComponent<AudioSource>().Play();
                                    Invoke("GameStart", 2.5f);
                                    StartCoroutine("FadeInOut", 1f);
                                    
                                    //if(PhotonNetwork.IsMasterClient)
                                    //    PV.RPC("GameStartRPC", RpcTarget.All);
                                }
                                else
                                {
                                    GameStartFailedPanel.SetActive(true);
                                    if(isReady)
                                        ToggleReady();
                                }
                            }
                        }
                    }
                }
            }

        }
    }

    public void KickPlayer(string id)
    {

        for(int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].UserId == id)
            {
                PhotonNetwork.PlayerList[i].SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsKicked", true } });
            }
        }
    }


    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);


        /*
        GetComponent<PhotonView>().RPC("ChatRPC", RpcTarget.All, newPlayer.NickName + "님이 참가하셨습니다.");

        PlayerListContent tmp = Instantiate(PlayerListContent, PlayerListContainer.transform).GetComponent<PlayerListContent>();

        tmp.nameTxt.text = newPlayer.NickName;
        tmp.myPlayer = newPlayer;


        playerContentList.Add(tmp.gameObject);
        */
        /*
        if (PhotonNetwork.IsMasterClient)
        {
            tmp.KickBtn.onClick.AddListener(() => KickPlayer(newPlayer.UserId));

        }
        else
        {
            tmp.KickBtn.gameObject.SetActive(false);
        }
        */
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        GetComponent<PhotonView>().RPC("ChatRPC", RpcTarget.All, otherPlayer.NickName + "님이 퇴장하셨습니다.");
        /*
        PlayerListContent[] playerContentList = FindObjectsOfType<PlayerListContent>();
        for(int i = 0; i < playerContentList.Length; i++)
        {
            if(playerContentList[i].myPlayer==null || playerContentList[i].myPlayer ==otherPlayer)
                Destroy(playerContentList[i].gameObject);
        }
        PlayerHealthContent[] playerHealthContentList = FindObjectsOfType<PlayerHealthContent>();
        for (int i = 0; i < playerHealthContentList.Length; i++)
        {
            if (playerHealthContentList[i].myPlayer == null || playerHealthContentList[i].myPlayer == otherPlayer)
                Destroy(playerHealthContentList[i].gameObject);
        }
        */
        PhotonNetwork.DestroyPlayerObjects(otherPlayer);
    }
    
    public void ActiveFalseJoinOrCreateRoomAcceptPanels(){
        
        for (int i = 0; i<JoinOrCreateRoomAcceptPanels.Length; i++)
        {
            JoinOrCreateRoomAcceptPanels[i].gameObject.SetActive(false);
        }
    }

    public void InitializeRoom()
    {
        if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("IsGameStarted"))
        {
            GameStarted = false;
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsGameStarted", false } });
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsGameStarted", false }, { "CharacterHealth", 100 }, { "InitCharacterCount", 3 }, { "TurnTime", 20 }, { "MapIndex", 0 } });
        }
        GameStarted = false;
        currentCharacter = null;

        /*
        if (myPlayerListContent)
            PhotonNetwork.Destroy(myPlayerListContent.gameObject);
        if (myPlayerHealthListContent)
            PhotonNetwork.Destroy(myPlayerHealthListContent.gameObject);

        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.PV.IsMine)
                player.DestroyFunc();
            else
                Destroy(player);
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

        //TileManager.GetInstance().MapGenerator();
        TileManager.GetInstance().LobbyMapGenerator();


        AmmoManager.GetInstance().ResetAmmoCounts(true);
        //Spawn(1);

        ChatPanel.SetActive(true);
        ChatPanelActiveBtn.SetActive(false);
        
        PanelForLobby.SetActive(true);


        ActiveFalseJoinOrCreateRoomAcceptPanels();

        isReady = false;
        ReadyBtn.GetComponent<Image>().color = Color.yellow;
        ReadyBtn.GetComponentInChildren<Text>().text = "준비";

        //myPlayerListContent = PhotonNetwork.Instantiate("PlayerListContent", PlayerListContainer.transform.position, Quaternion.identity).GetComponent<PlayerListContent>();
        //myPlayerListContent.myPlayer = PhotonNetwork.LocalPlayer;



        if (TutorialDialogue)
        {
            TutorialDialogue.SetActive(true);
        }
    }
    
    public void Spawn(int count)
    {
        if (count == -1)
        {
            GameObject player = null;
            player = PhotonNetwork.Instantiate("Worm", TileManager.GetInstance().respawnPositions[Random.Range(0,TileManager.GetInstance().respawnPositions.Count)].position, Quaternion.identity);
            myCharacters.Add(player.GetComponent<Player>());
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                GameObject player = null;

                //PhotonTeamExtensions.GetPhotonTeam(PhotonNetwork.LocalPlayer).Code
                //PhotonNetwork.LocalPlayer.ActorNumber%4
                int tmpPlayerIndex=0;
                //for (int j = 0; j < PhotonNetwork.CurrentRoom.Players.Count; j++)
                //{
                //    if (PhotonNetwork.CurrentRoom.Players[j] == PhotonNetwork.LocalPlayer)
                //    {
                //        tmpPlayerIndex = j;
                //        break;
                //    }
                //}
                /*
                foreach (KeyValuePair<int,Photon.Realtime.Player>  p in PhotonNetwork.CurrentRoom.Players)
                {
                    if (p.Value == PhotonNetwork.LocalPlayer)
                    {
                        tmpPlayerIndex++;
                        break;
                    }
                }
                */
                Vector3 respawnPos = new Vector3(Random.Range(-.15f,.15f),Random.Range(0f,0.15f));
                switch (PhotonTeamExtensions.GetPhotonTeam(PhotonNetwork.LocalPlayer).Code)
                {
                    case 0:
                        player = PhotonNetwork.Instantiate("Worm", TileManager.GetInstance().realRespawnPositions0[i % TileManager.GetInstance().realRespawnPositions0.Count].position+ respawnPos, Quaternion.identity);
                        break;
                    case 1:
                        player = PhotonNetwork.Instantiate("Worm", TileManager.GetInstance().realRespawnPositions1[i % TileManager.GetInstance().realRespawnPositions1.Count].position+ respawnPos, Quaternion.identity);
                        break;
                    case 2:
                        player = PhotonNetwork.Instantiate("Worm", TileManager.GetInstance().realRespawnPositions2[i % TileManager.GetInstance().realRespawnPositions2.Count].position+ respawnPos, Quaternion.identity);
                        break;
                    default:
                        player = PhotonNetwork.Instantiate("Worm", TileManager.GetInstance().realRespawnPositions3[i % TileManager.GetInstance().realRespawnPositions3.Count].position+ respawnPos, Quaternion.identity);
                        break;
                }
                myCharacters.Add(player.GetComponent<Player>());
            }

            while (myCharacters.Count < 4)
            {
                myCharacters.Add(null);
            }
        }

    }
    
    [PunRPC]
    public void GameStartRPC()
    {
        GameStarted = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsGameStarted", true } });

        GetComponent<AudioSource>().Play();
        Invoke("GameStart", 2.5f);
        StartCoroutine("FadeInOut", 1f);
    }
    
    public void GameStart()
    {
        GameStarted = true;

        if (isReady)
        {
            ToggleReady();
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsGameStarted", true } });
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "CharacterCount", (int)PhotonNetwork.CurrentRoom.CustomProperties["InitCharacterCount"] } });

        PanelForLobby.SetActive(false);
        PanelForGame.SetActive(true);



        for (int i = 0; i < myCharacters.Count; i++)
        {
            if (myCharacters[i] != null)
                myCharacters[i].DestroyFunc();
        }

        myCharacters.Clear();
        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.PV.IsMine)
                PhotonNetwork.Destroy(player.PV);
            //player.DestroyFunc();
            else
                Destroy(player);
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

        TileManager.GetInstance().MapGenerator();

        //Spawn((int)PhotonNetwork.CurrentRoom.CustomProperties["InitCharacterCount"] - 1);

        myPlayerHealthListContent = PhotonNetwork.Instantiate("PlayerHealthContent", PlayerHealthListContainer.transform.position, Quaternion.identity).GetComponent<PlayerHealthContent>();
        myPlayerHealthListContent.myPlayer = PhotonNetwork.LocalPlayer;

        AmmoManager.GetInstance().ResetAmmoCounts(false);


        if (TutorialDialogue)
        {
            TextBox.SetActive(false);
            Destroy(TutorialDialogue);
        }

        OptionSettingManager.GetInstance().PlayBackgroundAudio(true);
    }
    public void GameEnd()
    {
        StartCoroutine("FadeInOut",0f);
        //방을 나가지는 않음//Disconnect되지 않는다는말

        PanelForGame.SetActive(false);
        PanelForLobby.SetActive(true);
        WinnerPanel.SetActive(false);
        LoserPanel.SetActive(false);

        if (isReady)
        {
            ToggleReady();
        }
        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.CurrentRoom.IsVisible = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsGameStarted", false } });
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "CharacterCount", 1 } });

        
        for (int i = 0; i < myCharacters.Count; i++)
        {
            if (myCharacters[i] != null)
                myCharacters[i].DestroyFunc();
        }

        myCharacters.Clear();


        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.PV.IsMine)
                PhotonNetwork.Destroy(player.PV);
                //player.DestroyFunc();
            else
                Destroy(player);
        }
        foreach (Supply supply in FindObjectsOfType<Supply>())
        {
            Destroy(supply.gameObject);
        }
        /*
        foreach (PlayerListContent player in FindObjectsOfType<PlayerListContent>())
        {
            Destroy(player.gameObject);
        }
        */
        foreach (PlayerHealthContent player in FindObjectsOfType<PlayerHealthContent>())
        {
            Destroy(player.gameObject);
        }
        foreach (Supply supply in FindObjectsOfType<Supply>())
        {
            Destroy(supply.gameObject);
        }


        if (myPlayerListContent == null)
        {
            myPlayerListContent = PhotonNetwork.Instantiate("PlayerListContent", PlayerHealthListContainer.transform.position, Quaternion.identity).GetComponent<PlayerListContent>();
            myPlayerListContent.myPlayer = PhotonNetwork.LocalPlayer;

        }

        InitializeRoom();
    }

    public void JoinRoom()
    {
        GameStarted = false;
        
        if (myPlayerListContent)
            PhotonNetwork.Destroy(myPlayerListContent.gameObject);
        if (myPlayerHealthListContent)
            PhotonNetwork.Destroy(myPlayerHealthListContent.gameObject);

        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.PV.IsMine)
                PhotonNetwork.Destroy(player.PV);
            //player.DestroyFunc();
            else
                Destroy(player);
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
        
        if (myPlayerListContent == null)
        {
            myPlayerListContent = PhotonNetwork.Instantiate("PlayerListContent", PlayerListContainer.transform.position, Quaternion.identity).GetComponent<PlayerListContent>();
            myPlayerListContent.myPlayer = PhotonNetwork.LocalPlayer;
        }
    }

    public void LeaveRoom()
    {
        if (isReady)
        {
            ToggleReady();
        }
        GameStarted = false;

        ChatPanel.SetActive(false);
        PanelForGame.SetActive(false);
        PanelForLobby.SetActive(false);

        TileManager.GetInstance().ClearAllCurrentTile();


        //for (int i = 0; i < playerContentList.Count; i++)
        //{
        //    Destroy(playerContentList[i].gameObject);
        //}
        //playerContentList.Clear();

        for (int i = 0; i < myCharacters.Count; i++)
        {
            if (myCharacters[i] != null)
                myCharacters[i].DestroyFunc();
        }
        myCharacters.Clear();
        if (myPlayerListContent)
            PhotonNetwork.Destroy(myPlayerListContent.gameObject);
        if (myPlayerHealthListContent)
            PhotonNetwork.Destroy(myPlayerHealthListContent.gameObject);

        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.PV.IsMine)
                PhotonNetwork.Destroy(player.PV);
            //player.DestroyFunc();
            else
                Destroy(player);
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
    }
    
    public void ToggleReady()
    {
        isReady = !isReady;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "IsReady", isReady } });

        if (isReady)
        {
            ReadyBtn.GetComponent<Image>().color = Color.gray;
            ReadyBtn.GetComponentInChildren<Text>().text = "준비 취소";
        }
        else
        {
            ReadyBtn.GetComponent<Image>().color = Color.yellow;
            ReadyBtn.GetComponentInChildren<Text>().text = "준비";
        }
    }

    public void ApplyCustomize(int index)
    {

        int myCustom = (int)PhotonNetwork.LocalPlayer.CustomProperties["Customize"];
        if (index >= 100)
        {
            myCustom = index + myCustom % 100;
        }
        else
        {
            myCustom = index + ((int)(myCustom / 100)*100);
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Customize", myCustom } });
    }

    public Color ColorByIndex(int i)
    {
        Color tmpColor=new Color();
        switch (i)
        {
            case 0:
                tmpColor = Color.red;
                break;
            case 1:
                tmpColor = Color.blue;
                break;
            case 2:
                tmpColor = Color.yellow;
                break;
            default:
                tmpColor = Color.green;
                break;
        }
        return tmpColor;
    }

    public void Send()
    {
        string msg = PhotonNetwork.NickName + ":" + ChatInput.text;
        GetComponent<PhotonView>().RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + ":" + ChatInput.text);

        Player[] tmpPlayers = FindObjectsOfType<Player>();
        for(int i = 0; i < tmpPlayers.Length; i++)
        {
            if(tmpPlayers[i].PV.IsMine)
                tmpPlayers[i].PV.RPC("ChatRPC", RpcTarget.All, ChatInput.text);
        }

        ChatInput.text = "";

    }

    IEnumerator FadeInOut(float time)
    {
        yield return new WaitForSeconds(time);
        FadeAnim.SetBool("FadeOut", true);
        yield return new WaitForSeconds(3f);
        FadeAnim.SetBool("FadeOut", false);
    }


    [PunRPC]
    void ChatRPC(string msg)
    {
        ChatRPCDelegate(msg);
    }
    void ChatRPCDelegate(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
        {
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        }
        if (!isInput)
        {
            for (int i = 1; i < ChatText.Length; i++)
                ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
