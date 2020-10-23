using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActiveTextLine : MonoBehaviour {
    public bool JustStart;
    public float TimeToWait;

    public TextAsset theText;
    public Sprite[] images;

    public int startLine;
    public int endLine;

    public TextBoxManager theTextBox;

    public static bool DestroyWhenActivated;

    private GameObject player;
    public static bool CanTalk;
    public static bool talking;
    
    public bool Once=false;
    public static bool OnceEnd=false;

    public GameObject[] EnableObjects;
    public GameObject[] DisableObjects;
    
    public float PermitDistance=0.5f;

    public GameObject DownArrow;

    public bool DestroyWhenEnded = false;

    // Use this for initialization
    void Start () {
        talking = false;
        OnceEnd = false;
        DestroyWhenActivated = false;
        theTextBox = FindObjectOfType<TextBoxManager>();
        player = GameObject.FindGameObjectWithTag("Player");

    }

    // Update is called once per frame
    void Update () {
        if (JustStart)
        {
            JustStart = false;
            StartCoroutine(ActiveTextBox(TimeToWait));
        }
        if (!talking && CanTalk && (Input.GetKeyDown(KeyCode.Z)) && !OnceEnd)
        {
            talking= true;

            //Cam.GetComponent<CameraFollow>().enabled = false;

            //ActiveText = GetComponent<ActiveTextLine>();
            StartCoroutine(ActiveTextBox(TimeToWait));
        }

        if (JustStart) {
            CanTalk = true;
        }
        else
        {
            if (player != null)
            {
                if (Vector2.Distance(this.transform.position, player.transform.position) <= PermitDistance)
                {
                    //&& Vector2.Distance(Player.transform.position, this.transform.position) < 1.25f
                    CanTalk = true;
                    if (DownArrow)
                    {
                        DownArrow.SetActive(true);
                    }
                }
                else
                {
                    CanTalk = false;
                    if (DownArrow)
                    {
                        DownArrow.SetActive(false);
                    }
                }
            }
        }

        if (DestroyWhenActivated)
        {
            if (!OnceEnd)
            {
                if (Once)
                {
                    OnceEnd = true;
                }
            }
            //if (Vector2.Distance(Player.transform.position, this.transform.position) > 1.25f) {
            //    this.ActiveText.enabled = false;
            //}
            talking = false;
            print("CanAttacked Because Destroyed");
            //Player.CanAttacked = true;

            //Cam.GetComponent<CameraFollow>().enabled = true;

            CanTalk = false;
            Player.CanMove = true;
            for (int i = 0; i < EnableObjects.Length; i++)
            {
                if (EnableObjects[i])
                {
                    EnableObjects[i].SetActive(true);
                }
            }
            for (int i = 0; i < DisableObjects.Length; i++)
            {
                if (DisableObjects[i])
                {
                    DisableObjects[i].SetActive(false);
                }
            }

            if (DestroyWhenEnded)
            {
                if (DownArrow)
                {
                    DownArrow.SetActive(false);
                }
                Destroy(this.gameObject);
            }
            DestroyWhenActivated = false;

        }
    }

    //private void OnTriggerEnter2D(Collider2D coll)
    //{
    //    ActiveText = GetComponent<ActiveTextLine>();
    //    if (coll.tag == "Player")
    //    {
    //        this.ActiveText.enabled = true;
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D coll)
    //{
    //    ActiveText = GetComponent<ActiveTextLine>();
    //    if (coll.tag == "Player"&&!talking)
    //    {
    //        this.ActiveText.enabled = false;
    //    }
    //}


    IEnumerator ActiveTextBox(float WaitTime)
    {
        if (!OnceEnd)
        {
            talking = true;
            //if (Once)
            //{
            //    OnceEnd = true;
            //}
            //PlayerMove.CanMove = false;
            TextBoxManager.currActiveTextLine = this;
            TextBoxManager.currentLine = startLine;

            yield return new WaitForSeconds(WaitTime);
            theTextBox.ReloadScript(theText);

            theTextBox.EndAtLine = endLine;
            theTextBox.EnableTextBox();
        }
    }
}
