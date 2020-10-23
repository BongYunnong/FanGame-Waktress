using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TileManager : MonoBehaviourPunCallbacks,IPunObservable
{
    private static TileManager instance;
    public static TileManager GetInstance()
    {
        if (!instance)
        {
            instance = GameObject.FindObjectOfType<TileManager>();
            if (!instance)
                Debug.LogError("There needs to be one active MyClass script on a GameObject in your scene.");
        }

        return instance;
    }
    public List<Transform> respawnPositions;

    public List<Transform> realRespawnPositions0;
    public List<Transform> realRespawnPositions1;
    public List<Transform> realRespawnPositions2;
    public List<Transform> realRespawnPositions3;

    public List<Vector3> lobbyResapwnPos;

    //public int MapIndex;

    public GameObject lobbyMap;
    public TextAsset[] MapAsset;

    public GameObject[] MapBackgrounds;

    public List<GameObject> tilePrefabs;
    public GameObject mapContainerPrefab;
    private string[,] mapArray;
    private float[,] healthArray;
    private float[,] maxHealthArray;

    public GameObject[,] TileArray;
    public Sprite[] tileSprites;
    int rowSize, colSize;
    //public List<GameObject> currentMapTiles = new List<GameObject>();

    public void ClearAllCurrentTile()
    {
        realRespawnPositions0.Clear();
        realRespawnPositions1.Clear();
        realRespawnPositions2.Clear();
        realRespawnPositions3.Clear();

        lobbyMap.SetActive(false);

        foreach(supplyContainer sC in FindObjectsOfType<supplyContainer>())
        {
            Destroy(sC.gameObject);
        }


        for (int i = 0; i < rowSize; i++)
        {
            for (int j = 0; j < colSize; j++)
            {
                if (TileArray[i, j] != null)
                {
                    Destroy(TileArray[i, j]);
                }
            }
        }
        rowSize = 0;
        colSize = 0;
    }

    private void Start()
    {
        for (int i = 0; i < respawnPositions.Count; i++)
        {
            lobbyResapwnPos[i] = respawnPositions[i].position;
        }
    }

    private void Update()
    {
        //MapIndex = Mathf.Clamp(MapIndex,0, MapAsset.Length);
        for (int i = 0; i < rowSize; i++)
        {
            for (int j = 0; j < colSize; j++)
            {
                if (TileArray[i, j] != null)
                {
                    if (TileArray[i, j].GetComponent<TileScript>())
                    {
                        TileArray[i, j].GetComponent<TileScript>().health = healthArray[i, j];
                    }
                }
            }
        }
    }

    [PunRPC]
    public void MapGeneratorRpc()
    {
        for(int i = 0; i < MapBackgrounds.Length; i++)
        {
            MapBackgrounds[i].SetActive(false);
        }
        MapBackgrounds[(int)PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"]].SetActive(true);

        //int selectedMapIndex = Random.Range(0, MapAsset.Length);
        string currentText;
        currentText = MapAsset[(int)PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"]].text.Substring(0, MapAsset[(int)PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"]].text.Length - 1);
        string[] rows = currentText.Split('\n');
        rowSize = rows.Length;
        colSize = rows[0].Split(',').Length;
        mapArray = new string[rowSize, colSize];
        healthArray = new float[rowSize, colSize];
        maxHealthArray = new float[rowSize, colSize];
        TileArray = new GameObject[rowSize, colSize];

        for (int i = 0; i < rowSize; i++)
        {
            for (int j = 0; j < colSize; j++)
            {
                string[] row = rows[i].Split(',');

                mapArray[i, j] = row[j];

                ///tileIndex
                /// 1:풀+흙     2:흙       3:부서진돌  4:돌
                /// 5:석탄      6:금       7:다이아    8:베드락
                /// 9:대리석    10:정제돌   11:흰벽돌   12:벽돌
                /// 13:나무원목 14:정제나무 15:모래     16:나뭇잎

                if(mapArray[i,j]=="1" || mapArray[i, j] == "2" || mapArray[i, j] == "15")
                {
                    maxHealthArray[i, j] = 20;
                }
                else if (mapArray[i, j] == "3" || mapArray[i, j] == "14")
                {
                    maxHealthArray[i, j] = 25;
                }
                else if (mapArray[i, j] == "4"|| mapArray[i, j] == "5" || mapArray[i, j] == "13")
                {
                    maxHealthArray[i, j] = 30;
                }
                else if (mapArray[i, j] == "9"|| mapArray[i, j] == "10"|| mapArray[i, j] == "11"|| mapArray[i, j] == "12")
                {
                    maxHealthArray[i, j] = 40;
                }
                else if (mapArray[i, j] == "6" || mapArray[i, j] == "7")
                {
                    maxHealthArray[i, j] = 50;
                }
                else if (mapArray[i, j] == "8")
                {
                    maxHealthArray[i, j] = 10000;
                }
                else if (mapArray[i, j] == "16")
                {
                    maxHealthArray[i, j] = 10;
                }
                else
                {
                    maxHealthArray[i, j] = 0;
                }
                healthArray[i, j] = maxHealthArray[i,j];
            }
        }
    }
    IEnumerator MapGeneratorCoroutine()
    {
        yield return new WaitUntil(()=> { return rowSize > 0; });

        Vector3 instantiatePos = new Vector3();
        GameObject tmp;

        for (int i = 0; i < respawnPositions.Count; i++)
        {
            respawnPositions[i].gameObject.SetActive(false);
        }

        int respawnPosCount = 0;
        for (int i = 0; i < rowSize; i++)
        {
            for (int j = 0; j < colSize-1; j++)
            {
                print("MapGene");
                if (mapArray[i, j] == "0")
                {
                }
                else
                {
                    instantiatePos = new Vector3((j - (colSize / 2)) * 0.32f, (i - (rowSize / 2)) * -0.32f,1.1f);
                    if (mapArray[i, j] == "a")
                    {
                        respawnPositions[respawnPosCount].gameObject.SetActive(true);
                        respawnPositions[respawnPosCount].position = instantiatePos;
                        realRespawnPositions0.Add(respawnPositions[respawnPosCount]);
                        respawnPosCount++;
                    }
                    else if (mapArray[i, j] == "b")
                    {

                        respawnPositions[respawnPosCount].gameObject.SetActive(true);
                        respawnPositions[respawnPosCount].position = instantiatePos;
                        realRespawnPositions1.Add(respawnPositions[respawnPosCount]);
                        respawnPosCount++;
                    }
                    else if (mapArray[i, j] == "c")
                    {
                        respawnPositions[respawnPosCount].gameObject.SetActive(true);
                        respawnPositions[respawnPosCount].position = instantiatePos;
                        realRespawnPositions2.Add(respawnPositions[respawnPosCount]);
                        respawnPosCount++;
                    }
                    else if (mapArray[i, j] == "d")
                    {
                        respawnPositions[respawnPosCount].gameObject.SetActive(true);
                        respawnPositions[respawnPosCount].position = instantiatePos;
                        realRespawnPositions3.Add(respawnPositions[respawnPosCount]);
                        respawnPosCount++;
                    }
                    else
                    {
                        TileArray[i, j] = Instantiate(tilePrefabs[0], instantiatePos, Quaternion.identity);
                        if (TileArray[i, j].GetComponent<TileScript>())
                        {
                            TileArray[i, j].GetComponent<TileScript>().MaxHealth = maxHealthArray[i, j];
                            TileArray[i, j].GetComponent<TileScript>().health = healthArray[i, j];
                        }
                        TileArray[i, j].GetComponent<SpriteRenderer>().sprite = tileSprites[int.Parse(mapArray[i, j])-1];
                        /*
                        switch (mapArray[i,j])
                        {
                            case "0":
                                healthArray[i, j] = 30;
                                break;
                            case "1":
                                healthArray[i, j] = 30;
                                break;
                            default:
                                healthArray[i, j] = 30;
                                break;
                        }
                        */
                    }
                }

            }
        }
        yield return new WaitForSeconds(1f);
        GameManager.GetInstance().Spawn((int)PhotonNetwork.CurrentRoom.CustomProperties["InitCharacterCount"]);
        yield return new WaitForSeconds(3f);

        TurnManager.GetInstance().TurnIndex = 0;
    }

    public void ResetRespawnPos()
    {
        for (int i = 0; i < respawnPositions.Count; i++)
        {
            switch (i % 4)
            {
                case 0:
                    realRespawnPositions0.Add(respawnPositions[i]);
                    break;
                case 1:
                    realRespawnPositions1.Add(respawnPositions[i]);
                    break;
                case 2:
                    realRespawnPositions2.Add(respawnPositions[i]);
                    break;
                default:
                    realRespawnPositions3.Add(respawnPositions[i]);
                    break;
            }


            respawnPositions[i].gameObject.SetActive(true);

            respawnPositions[i].position= lobbyResapwnPos[i];
        }
    }

    public void LobbyMapGenerator()
    {
        ResetRespawnPos();
        TileManager.GetInstance().ClearAllCurrentTile();
        for (int i = 0; i < MapBackgrounds.Length; i++)
        {
            MapBackgrounds[i].SetActive(false);
        }
        lobbyMap.SetActive(true);
        GameManager.GetInstance().Spawn(-1);
    }

    public void MapGenerator()
    {
        TileManager.GetInstance().ClearAllCurrentTile();

        //GameObject tmpContainer = Instantiate("MapContainer", Vector3.zero, Quaternion.identity);

        if(PhotonNetwork.IsMasterClient)
            GetComponent<PhotonView>().RPC("MapGeneratorRpc", RpcTarget.AllBuffered);

        StartCoroutine("MapGeneratorCoroutine");
    }

    public void UpdateTilesHealth(Vector3 attackCenterPos, float power, float radius,bool mineInstall=false)
    {
        GetComponent<PhotonView>().RPC("UpdateTilesHealthRpc", RpcTarget.AllBuffered, new object[] { attackCenterPos, power, radius,mineInstall });
    }
    [PunRPC]
    public void UpdateTilesHealthRpc(Vector3 attackCenterPos, float power,float radius, bool installMine)
    {
        for (int i = 0; i < rowSize; i++)
        {
            for (int j = 0; j < colSize; j++)
            {
                if (TileArray[i, j]!=null)
                {
                    if (TileArray[i, j].GetComponent<TileScript>())
                    {
                        if (Vector3.Distance(TileArray[i, j].transform.position, attackCenterPos) <= radius)
                        {
                            var dir = (TileArray[i, j].transform.position - attackCenterPos);
                            float tmpMagnitude = Mathf.Clamp(dir.magnitude, 0, radius);
                            float wearoff = 1 - (tmpMagnitude / radius);
                            wearoff = Mathf.Clamp(wearoff, 0.5f, .95f);

                            if (power > -500)
                            {
                                healthArray[i, j] -= power * wearoff;
                                TileArray[i, j].GetComponent<TileScript>().attackedPS.Play();

                                if (installMine)
                                {
                                    TileArray[i, j].GetComponent<TileScript>().MineInstalled = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (power <= -500)
                    {
                        Vector3 instantiatePos = new Vector3((j - (colSize / 2)) * 0.32f, (i - (rowSize / 2)) * -0.32f, 1.1f);
                        if (Vector3.Distance(instantiatePos, attackCenterPos) <= radius)
                        {
                            //마인크래프트두를 위해
                            mapArray[i, j] = "1";
                            maxHealthArray[i, j] = 50;
                            healthArray[i, j] = 50;
                            TileArray[i, j] = Instantiate(tilePrefabs[0], instantiatePos, Quaternion.identity);
                            if (TileArray[i, j].GetComponent<TileScript>())
                            {
                                TileArray[i, j].GetComponent<TileScript>().MaxHealth = maxHealthArray[i, j];
                                TileArray[i, j].GetComponent<TileScript>().health = healthArray[i, j];
                            }
                            TileArray[i, j].GetComponent<SpriteRenderer>().sprite = tileSprites[int.Parse(mapArray[i, j])];
                        }
                    }
                }
            }
        }



    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
