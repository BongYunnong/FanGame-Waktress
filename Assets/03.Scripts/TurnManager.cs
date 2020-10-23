using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;

public class TurnManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private static TurnManager instance;
    public static TurnManager GetInstance()
    {
        if (!instance)
        {
            instance = GameObject.FindObjectOfType<TurnManager>();
            if (!instance)
                Debug.LogError("There needs to be one active MyClass script on a GameObject in your scene.");
        }

        return instance;
    }
    private PhotonView PV;
    public Text turnText;
    public Image turnTimeImg;

    public Text whoseTurnText;
    public Image whoseTurnImg;

    public Text killLogText;
    public Image killLogImg;
    public string killedWormsName;

    public int TurnIndex = -1;
    public int characterTurnIndex = 0;
    public float MaxTurnTime = 15f;
    public float currTurnTime;


    public GameObject SupplyContainer;

    public int remainedEventsCount = 0;

    public Text turnTmpText;


    public Text currentPlayersText;

    private void Start()
    {
        PV = GetComponent<PhotonView>();
        GeneSupplyContainerFunc();
    }

    public void Update()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
            {/*
                Photon.Realtime.Player[] players;
                PhotonTeamsManager.Instance.TryGetTeamMembers((byte)(TurnIndex % 4), out players);

                turnTmpText.text = TurnIndex+"/"+currTurnTime+"["+ (TurnIndex / 4) % players.Length + "]";
                */
                turnTmpText.text = TurnIndex + "/" + currTurnTime + "[" + (TurnIndex / 4) + "]";

                if (TurnIndex >= 0)
                {
                    if (currTurnTime <= 5f && currTurnTime >= 4.9f)
                    {
                        turnTimeImg.GetComponent<AudioSource>().Play();
                    }
                    else
                    {
                        turnTimeImg.GetComponent<AudioSource>().Stop();
                    }
                    if (PhotonNetwork.IsMasterClient && currTurnTime > -100)
                    {
                        currTurnTime -= Time.deltaTime;
                    }

                    if (currTurnTime > -100 && currTurnTime <= 0 && remainedEventsCount <= 0)
                    {
                        turnTimeImg.GetComponent<AudioSource>().Stop();
                        //remainedEventsCount += 100;
                        if (PhotonNetwork.IsMasterClient)
                        {
                            currTurnTime = -1000f;
                            StartCoroutine("SetTurnCoroutine");
                        }
                    }
                    /* 죽었을떄 턴 넘기기
                    if (currTurnTime>5 && remainedEventsCount<=0 &&GameManager.GetInstance().currentCharacter == null)
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            currTurnTime = -100f;
                            StopCoroutine("SetTurnCoroutine");
                            StartCoroutine("SetTurnCoroutine");
                        }
                    }
                    */

                    if (GameManager.GetInstance().currentCharacter != null)
                        turnText.text = GameManager.GetInstance().currentCharacter.PV.Owner.NickName + "'s TURN";
                    turnTimeImg.fillAmount = currTurnTime / (float)PhotonNetwork.CurrentRoom.CustomProperties["TurnTime"];
                }
                else
                {
                    currTurnTime = -1;
                    TurnIndex = -1;
                    characterTurnIndex = 0;
                    remainedEventsCount = 0;
                }
            }
            else
            {
                currTurnTime = -1;
                TurnIndex = -1;
                characterTurnIndex = 0;
                remainedEventsCount = 0;
            }
        }
    }

    public void GeneSupplyContainerFunc()
    {
        StartCoroutine("GeneSupplyContainer");
    }
    IEnumerator GeneSupplyContainer()
    {
        yield return new WaitForSeconds(Random.Range(30f, 60f));
        if (PhotonNetwork.InRoom)
            if (PhotonNetwork.IsMasterClient)
                if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
                    SupplyContainer = PhotonNetwork.Instantiate("SupplyContainer", new Vector3(10, 3, -1f), Quaternion.identity);
        StartCoroutine("GeneSupplyContainer");
    }
    /*
    public void SetTurn()
    {
        PV.RPC("SetTurnRPC", RpcTarget.AllBuffered);
    }
    */
    IEnumerator SetTurnCoroutine()
    {
        //yield return new WaitForSeconds(1f);

        currentPlayersText.text = "TurnCoroutine Playing";
        //움직이는 놈이 있는지 확인
        int remainedChar = 0;
        foreach (Player p in FindObjectsOfType<Player>())
        {
            if (p.rb.velocity.magnitude > 0.2f)
            {
                remainedChar++;
            }
        }

        if (remainedChar > 0)
        {
            yield return new WaitForSeconds(0.5f);

            //움직이고있는애가 있으면 안 움직일때까지 계속 돌리기
            StartCoroutine("SetTurnCoroutine");

            currentPlayersText.text = "TurnCoroutine Reloading3";

        }
        else
        {
            //모두 움직임이 멈췄으면 잠시후에 시작
            yield return new WaitForSeconds(0.5f);

            int tmpTurnIndex = TurnIndex;
            tmpTurnIndex++;


            int loopBreakStack = 0;
            //팀에 플레이어가 없으면 지나치기
            while (PhotonTeamsManager.Instance.GetTeamMembersCount(System.Convert.ToByte(tmpTurnIndex % 4)) <= 0)
            {
                print("This is for non player");
                //존재하는 플레이어는 다 체크할 수 있음(worm이 죽었어도)
                tmpTurnIndex++;
                loopBreakStack++;
                if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
                {
                    break;
                }
                if (loopBreakStack > 10)
                {
                    PhotonNetwork.Disconnect();
                }
            }

            //(TurnIndex/4)는 Team 내의 Turn index라고 볼 수 있음 
            //(TurnIndex/4)%팀의플레이어수 는 Team내의 player 순서까지 생각한 최종 TurnIndex라고 볼 수 있음
            //Photon.Realtime.Player[] players;
            //PhotonTeamsManager.Instance.TryGetTeamMembers((byte)(tmpTurnIndex % 4), out players);
            List<Photon.Realtime.Player> players = new List<Photon.Realtime.Player>();

            /*
            for (int i = 0; i < PhotonNetwork.CurrentRoom.Players.Count; i++)
            {
                if (PhotonNetwork.CurrentRoom.Players[i].GetPhotonTeam().Code == tmpTurnIndex % 4)
                {
                    players.Add(PhotonNetwork.CurrentRoom.Players[i]);
                }
            }
            */
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                if (PhotonNetwork.PlayerList[i].GetPhotonTeam().Code == tmpTurnIndex % 4)
                {
                    players.Add(PhotonNetwork.PlayerList[i]);
                }
            }
        

            //받을놈이 다 뒤져있으면 내 팀의 다른 사람에게 넘겨줘야한다.
            //그런데 팀에 있는  player들의 모든 character가 다 뒤졌을 경우에는 TurnIndex를 +1 해줘서 다른 팀에게 넘겨줘야한다.
            int playerLoopCount = 0;
            int tmpPlayerIndex = (tmpTurnIndex / 4) % players.Count;
            loopBreakStack = 0;
            while ((int)players[(tmpTurnIndex / 4) % players.Count].CustomProperties["CharacterCount"] <= 0)
            {
                print("This is For Dead");
                tmpTurnIndex += 4;


                playerLoopCount++;

                //playerLoopCount >= players.Count
                if (tmpPlayerIndex == (tmpTurnIndex / 4) % players.Count)
                {
                    playerLoopCount = -1;
                    break;
                }

                if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
                {
                    break;
                }


                if (loopBreakStack > 10)
                {
                    PhotonNetwork.Disconnect();
                }
            }
            if (playerLoopCount == -1)
            {
                TurnIndex++;
                StartCoroutine("SetTurnCoroutine");
                currentPlayersText.text = "Coroutine Reload2";

            }
            else
            {
                PV.RPC("SetTurnRPC", RpcTarget.All, tmpTurnIndex);
                currentPlayersText.text = "SetTurnRPC";
            }
        }

    }
    [PunRPC]
    public void SetTurnRPC(int tmp)
    {
        currentPlayersText.text = "SetTurnRPC";

        TurnIndex = tmp;
        remainedEventsCount = 0;

        //Photon.Realtime.Player[] players;
        //PhotonTeamsManager.Instance.TryGetTeamMembers((byte)(TurnIndex % 4), out players);
        List<Photon.Realtime.Player> players = new List<Photon.Realtime.Player>();
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].GetPhotonTeam().Code == TurnIndex % 4)
            {
                players.Add(PhotonNetwork.PlayerList[i]);
            }
        }


        //print("TurnRPC runned :" + TurnIndex + " / " + TurnIndex+" /"+ characterTurnIndex);

        if (players.Count > 0)
        {
            if (PhotonNetwork.LocalPlayer == players[(TurnIndex / 4) % players.Count])
            {
                int playerLoopCount = 0;
                while (GameManager.GetInstance().myCharacters[characterTurnIndex % 4] == null)
                {
                    /*
                    tmpLiveChar = 0;
                    for (int i = 0; i < GameManager.GetInstance().myCharacters.Count; i++)
                    {
                        if (GameManager.GetInstance().myCharacters[i] != null)
                        {
                            tmpLiveChar++;
                        }
                    }
                    if (tmpLiveChar <= 0)
                    {
                        break;
                    }
                    */
                    characterTurnIndex++;
                    playerLoopCount++;
                    if (!(bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
                    {
                        break;
                    }
                    if (playerLoopCount > 10)
                    {
                        break;
                    }
                }

                //print("currentCharacter setted"+"/"+characterTurnIndex+"["+tmpLiveChar+","+GameManager.GetInstance().myCharacters.Count+"]");

                if (playerLoopCount <= 10 && GameManager.GetInstance().myCharacters[characterTurnIndex % 4] != GameManager.GetInstance().currentCharacter && GameManager.GetInstance().myCharacters[characterTurnIndex % 4] != null)
                {
                    GameManager.GetInstance().myCharacters[characterTurnIndex % 4].SetCurrentCharacter();
                    currTurnTime = (float)PhotonNetwork.CurrentRoom.CustomProperties["TurnTime"];

                    characterTurnIndex++;

                    currentPlayersText.text = "TurnManageEnd";

                }
                else
                {
                    //아까 턴 캐릭터와 바꿔야 할 턴의 캐릭터가 같으면 뭔가 이상하니까 다시 TurnCoroutine돌리자
                    //setCam은 안해야함
                    TurnIndex++;
                    StartCoroutine("SetTurnCoroutine");
                    currentPlayersText.text = "ReLoadCoroutine";

                    //return;
                }
            }
            else
            {
                currentPlayersText.text = "IGotYou";

                //이 플레이어의 차례가 아닌 것
                // observe currTurnTime, SetCurrentCharacter RPC받아먹기
            }


            //지금 개나소나 setCam하니까 정확히 SetCurrentCharacter가 호출되었을때 SEtCam해보쟈
            //Invoke("SetCam", 0.5f);
        }
        else
        {
            currentPlayersText.text = "NonePlayer";

            //이럴리가없음
            //아마 난입한 사람한테 이 오류가 뜨지않았을까
        }
    }
    public void SetCam()
    {
        if (PhotonNetwork.InRoom)
        {
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
            {
                if (GameManager.GetInstance().currentCharacter != null)
                {
                    currTurnTime = (float)PhotonNetwork.CurrentRoom.CustomProperties["TurnTime"];

                    whoseTurnImg.gameObject.SetActive(true);
                    whoseTurnText.text = GameManager.GetInstance().currentCharacter.PV.Owner.NickName + "'s Turn";
                    //whoseTurnText.text = GameManager.GetInstance().myCharacters[((TurnManager.TurnIndex / 4) % GameManager.GetInstance().myCharacters.Count)].PV.Owner + "'s Turn";
                    //whoseTurnImg.color = GameManager.GetInstance().ColorByIndex(PhotonTeamsManager.Instance.GetAvailableTeams()[(TurnIndex) % 4].Code);
                    whoseTurnImg.color = GameManager.GetInstance().ColorByIndex(PhotonTeamExtensions.GetPhotonTeam(GameManager.GetInstance().currentCharacter.PV.Owner).Code);
                    GameManager.GetInstance().SetVCTargetPos(GameManager.GetInstance().currentCharacter.transform.position);
                    //GameManager.GetInstance().SetVCTargetPos(GameManager.GetInstance().currentCharacter.transform.position);
                    Invoke("TurnRevealEnd", 2f);
                }
                else
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        currTurnTime = -1000f;
                        StartCoroutine("SetTurnCoroutine");
                    }
                    whoseTurnImg.gameObject.SetActive(true);
                    whoseTurnText.text = "NOne";
                    Invoke("TurnRevealEnd", 2f);
                }
            }
        }
        //currTurnTime = (float)PhotonNetwork.CurrentRoom.CustomProperties["TurnTime"];
    }

    public void RevealKillLog()
    {
        PV.RPC("KillLogRPC", RpcTarget.All);
    }

    [PunRPC]
    private void KillLogRPC()
    {
        StartCoroutine("KillLogCoroutine");
    }

    IEnumerator KillLogCoroutine()
    {
        yield return new WaitForSeconds(1f);
        killLogText.text = killedWormsName + " 처치!";
        killLogImg.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);

        killLogImg.gameObject.SetActive(false);
        killedWormsName = "";
    }

    private void TurnRevealEnd()
    {
        whoseTurnImg.gameObject.SetActive(false);
        int probability = 25;
        if (PhotonNetwork.CurrentRoom != null)
            probability= (int)PhotonNetwork.CurrentRoom.CustomProperties["SupplyProbability"];
        if (Random.Range(0, 100) < probability)
        {
            if (PhotonNetwork.IsMasterClient)
                SupplyContainer = PhotonNetwork.Instantiate("SupplyContainer", new Vector3(0, 20, -1f), Quaternion.identity);
            currTurnTime += 5f;
        }
    }

    
    public void SetTimeOrEventCount(float setTime, int addCount)
    {
        PV.RPC("SetTimeOrEventCountRpc", RpcTarget.All, new object[] { setTime, addCount });
    }
    
    [PunRPC]
    private void SetTimeOrEventCountRpc(float setTime, int addCount)
    {
        if (setTime != -1)
            currTurnTime = setTime;

        if (addCount != 0)
            remainedEventsCount += addCount;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            stream.SendNext(currTurnTime);
        }
        else
        {
            this.currTurnTime = (float)stream.ReceiveNext();
        }

    }

}
