using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerStatus : MonoBehaviour
{
    public bool isTrigger = false;
    public Material common;
    public Material warn;

    int flag = 0;

    void OnTriggerStay()
    {
        isTrigger = true;
        flag = 0;
        GetComponent<Renderer>().material = warn;
    }

    void Update()
    {
        if (flag > 3) {
            isTrigger = false;
            GetComponent<Renderer>().material = common;
        }
        flag ++;
    }
}
