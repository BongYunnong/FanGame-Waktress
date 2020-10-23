using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingTextController : MonoBehaviour
{
    private static FloatingText popupText;
    private static GameObject canvas;

    public static void Initialize()
    {
        canvas = GameObject.Find("Canvas");
        if (!popupText)
        {
            popupText = Resources.Load<FloatingText>("PopupTextParent");
            //popupText = popupText.GetComponent<FloatingText>();
        }
    }
    public static void CreateFloatingText(string text, Transform location, Color color)
    {
        Initialize();
        FloatingText instance = Instantiate(popupText);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(new Vector2(location.position.x + Random.Range(-0.25f, 0.25f), location.position.y + Random.Range(-0.25f, 0.25f)));
        instance.transform.SetParent(canvas.transform, false);
        instance.transform.position = screenPosition;
        instance.gameObject.GetComponentInChildren<Text>().color = color;
        instance.SetText(text);
    }

    public static void CreateFloatingText_Player(string text, Transform location)
    {
        FloatingText instance = Instantiate(popupText);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(new Vector2(location.position.x + Random.Range(-0.25f, 0.25f), location.position.y + Random.Range(-0.25f, 0.25f)));
        instance.transform.SetParent(canvas.transform, false);
        instance.transform.position = screenPosition;
        instance.SetText(text);
        instance.gameObject.transform.localScale = instance.gameObject.transform.localScale * 1.5f;
        //instance.gameObject.GetComponentInChildren<Text>().color = FindObjectOfType<Player>().DamageColor;
    }
}
