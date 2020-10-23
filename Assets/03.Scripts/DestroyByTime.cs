using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByTime : MonoBehaviour
{
    public float delayTime=3f;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyFunc", delayTime);
    }

    void DestroyFunc()
    {
        Destroy(gameObject);
    }
    
}
