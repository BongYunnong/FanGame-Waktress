using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBoxManager : MonoBehaviour {

    public GameObject textbox;
    public static ActiveTextLine currActiveTextLine;
    public Text theText;

    public TextAsset textfile;
    public string[] textLines;

    public Image board;

    public static int currentLine;
    public int EndAtLine;

    public bool isActive;

    public bool stopPlayerMovement = true;

    private bool isTyping = false;
    private bool cancelTyping = false;

    public float typeSpeed;

    public AudioSource typingSfx;



    void Start()
    {
        if (textfile != null)
        {
            textLines = (textfile.text.Split('\n'));
        }

        if(EndAtLine == 0)
        {
            EndAtLine = textLines.Length -1;
        }
    }

    
    void Update() {
        if (isActive && Input.GetKeyDown(KeyCode.Z))
        {
            if (!isTyping)
            {
                currentLine += 1;

                if (currentLine > EndAtLine)
                {
                    DisableTextBox();
                    ReloadScript(textfile);
                    currentLine = -1;
                    EndAtLine = 2000000000;
                }else
                {
                    StartCoroutine(TextScroll(textLines[currentLine]));
                }
            }
            else if (isTyping && !cancelTyping)
            {
                cancelTyping = true;
            }    
        }
        
        
        
    }

    private IEnumerator TextScroll (string lineOfText)
    {
        int letter = 0;
        theText.text = "";
        isTyping = true;
        cancelTyping = false;


        if (currActiveTextLine.images[currentLine]!=null) {
            board.gameObject.SetActive(true);
            board.sprite = currActiveTextLine.images[currentLine ];
        }
        else
        {
            board.gameObject.SetActive(false);
        }
        while (isTyping && !cancelTyping && (letter < lineOfText.Length - 1))
        {
            theText.text += lineOfText[letter];
            letter += 1;
            if (letter % 3 == 0)
            {
                //typingSfx.Play();
                OptionSettingManager.GetInstance().Play("Typing", false);
            }
            yield return new WaitForSeconds(typeSpeed);
        }
        theText.text = lineOfText;
        isTyping = false;
        cancelTyping = false;
    }

    public void EnableTextBox()
    {
        textbox.SetActive(true);
        isActive = true;

        if (stopPlayerMovement)
        {
            Player.CanMove = false;
        }

        StartCoroutine(TextScroll(textLines[currentLine]));
    }
    public void DisableTextBox()
    {
        ActiveTextLine.DestroyWhenActivated = true;
        textbox.SetActive(false);
        isActive = false;
    }

    public void ReloadScript(TextAsset theText)
    {
        if(theText != null)
        {
            textLines = new string[1];
            textLines = (theText.text.Split('\n'));
        }
    }
}
