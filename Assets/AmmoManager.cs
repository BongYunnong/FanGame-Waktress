using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

using DG.Tweening;
public class AmmoManager : MonoBehaviour, IPunObservable
{
    private static AmmoManager instance;
    public static AmmoManager GetInstance()
    {
        if (!instance)
        {
            instance = GameObject.FindObjectOfType<AmmoManager>();
            if (!instance)
                Debug.LogError("There needs to be one active MyClass script on a GameObject in your scene.");
        }

        return instance;
    }
    public GameObject InventoryPanel;
    public GameObject SkillPanel;
    public Image SKillCharacterImage;

    public Text inventoryTxt;

    private bool InventoryOpened = false;
    public int WeaponIndex;
    public int WeaponCount;
    public PhotonView PV;

    public List<GameObject> ammoUIs = new List<GameObject>();
    public List<int> maxAmmoCountsInLobby = new List<int>();
    public List<int> initialAmmoCounts = new List<int>();
    public List<int> currAmmoCounts = new List<int>();

    public void SkillPanelActive(Sprite sp)
    {
        SkillPanel.SetActive(true);
        SKillCharacterImage.sprite = sp;
        SkillPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector3(120, 0, 0), 0.2f);

        Invoke("SkillPanelActiveFalse", 3f);
    }
    public void SkillPanelActiveFalse()
    {
        SkillPanel.SetActive(false);
        SkillPanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(-120, 20, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            InventoryOpened = !InventoryOpened;
            if(InventoryOpened)
                InventoryPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector3(-80, 75, 0), 1f);
            else
                InventoryPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector3(80, 75, 0), 1f);
        }
        for (int i = 0; i < ammoUIs.Count; i++)
        {
            if (i < WeaponCount)
            {
                ammoUIs[i].GetComponentInChildren<Text>().text = currAmmoCounts[i].ToString();
                if (currAmmoCounts[i] <= 0)
                {
                    ammoUIs[i].GetComponent<Image>().color = new Color(.5f, .5f, .5f);
                    ammoUIs[i].GetComponent<Button>().enabled = false;
                }
                else
                {
                    ammoUIs[i].GetComponent<Image>().color = new Color(1, 1, 1);
                    ammoUIs[i].GetComponent<Button>().enabled = true;
                }
            }
            else
            {
                ammoUIs[i].GetComponent<Button>().enabled = false;
                ammoUIs[i].GetComponentInChildren<Text>().text = "";
            }
        }
    }

    public void ResetAmmoCounts(bool lobby)
    {
        if (lobby)
        {
            for (int i = 0; i < currAmmoCounts.Count; i++)
            {
                currAmmoCounts[i] = maxAmmoCountsInLobby[i];
            }
        }
        else
        {
            for (int i = 0; i < currAmmoCounts.Count; i++)
            {
                currAmmoCounts[i] = initialAmmoCounts[i];
            }
        }
    }

    public string weaponIndexToString(int index)
    {
        string tmpTxt = "";
        switch (index)
        {
            case 0:
                tmpTxt = "앵그리버두";
                break;
            case 1:
                tmpTxt = "팻두";
                break;
            case 2:
                tmpTxt = "통통두";
                break;
            case 3:
                tmpTxt = "팬치";
                break;
            case 4:
                tmpTxt = "두두두두";
                break;
            case 5:
                tmpTxt = "축구공두";
                break;
            case 6:
                tmpTxt = "엄석두";
                break;
            case 7:
                tmpTxt = "왁초리";
                break;
            case 8:
                tmpTxt = "홀리 밤";
                break;
            case 9:
                tmpTxt = "마인크래프트두";
                break;
            case 10:
                tmpTxt = "감자 팬치";
                break;
            case 11:
                tmpTxt = "텔레포투";
                break;
            case 12:
                tmpTxt = "지잉좌";
                break;
            case 13:
                tmpTxt = "엔젤의 천벌";
                break;
            case 14:
                tmpTxt = "와두";
                break;
            case 15:
                tmpTxt = "축포";
                break;
            default:
                tmpTxt = "X";
                break;
        }
        return tmpTxt;
    }
    
    public void ChangeWeaponIndexBtn(int btnIndex)
    {
        WeaponIndex = btnIndex;

        inventoryTxt.text = weaponIndexToString(btnIndex);
        //PV.RPC("ChangeWeaponRPC", RpcTarget.AllBuffered, btnIndex);
    }
    /*
    [PunRPC]
    void ChangeWeaponRPC(int _keyIndex)
    {
        WeaponIndex = _keyIndex;
    }
    */
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
