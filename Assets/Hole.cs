using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
    public GameObject GoodText;
    float ts = 0f;

    private void OnTriggerEnter(Collider other)
    {
        GoodText.SetActive(true);
        ts = 1f;
    }

    private void Update()
    {
        if (ts > 0)
        {
            ts -= Time.deltaTime;
            if (ts <= 0)
            {
                GoodText.SetActive(false);
            }
        }
    }
}
