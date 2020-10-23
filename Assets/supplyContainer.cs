using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using Cinemachine;

public class supplyContainer : MonoBehaviour
{
    //public GameObject SupplyPrefab;
    public CinemachineVirtualCamera VC;
    float tmpPriority;
    // Start is called before the first frame update
    void Start()
    {
        if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
        {
            VC.Priority = 12;
            tmpPriority = VC.Priority;
        }
        else
        {
            VC.Priority = 0;
            tmpPriority = VC.Priority;
        }


        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                //TurnManager.GetInstance().SetTimeOrEventCount((float)PhotonNetwork.CurrentRoom.CustomProperties["TurnTime"]+3, 0);
                PlaneGo();
            }
        }
        else
            GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.AllBuffered);


    }
    private void Update()
    {
        tmpPriority -= Time.deltaTime;
        VC.Priority = (int)tmpPriority;

        this.transform.localScale = new Vector3(GetComponent<Rigidbody2D>().velocity.normalized.x, 1, 1);
    }

    [PunRPC]
    public void DestroyRPC()
    {
        Destroy(gameObject);
    }

    void PlaneGo()
    {
        int tmpDirIndex = 1;
        if (Random.Range(0f, 1f) < 0.5f)
        {
            tmpDirIndex = -1;
        }

        if (PhotonNetwork.CurrentRoom!=null)
        {
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"] && (int)PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"] != 1)
            {
                this.transform.position = new Vector3(13* tmpDirIndex, 7, -1f);
            }
            else
            {
                this.transform.position = new Vector3(13 * tmpDirIndex, 3, -1f);
            }
        }
        GetComponent<Rigidbody2D>().velocity = new Vector2(-4* tmpDirIndex, 0);
        StartCoroutine("GeneSupplyCoroutine");
    }

    IEnumerator GeneSupplyCoroutine()
    {
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        if (Mathf.Abs(this.transform.position.x) > 12)
        {
            PhotonNetwork.Destroy(this.gameObject);
        }
        else
        {
            GeneSupply();
            StartCoroutine("GeneSupplyCoroutine");
        }
    }

    void GeneSupply()
    {
        GameObject tmpSupply =PhotonNetwork.Instantiate("Supply",new Vector3(this.transform.position.x,this.transform.position.y,2f), Quaternion.identity);
        tmpSupply.GetComponent<Rigidbody2D>().velocity = GetComponent<Rigidbody2D>().velocity;
    }
}
