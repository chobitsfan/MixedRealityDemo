using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
    public GameObject HudText;
    float ts = 0f;

    private void OnTriggerEnter(Collider other)
    {
        UnityEngine.UI.Text text = HudText.GetComponent<UnityEngine.UI.Text>();
        text.text = "GOOD";
        HudText.SetActive(true);
        ts = 1f;
    }

    private void Update()
    {
        if (ts > 0)
        {
            ts -= Time.deltaTime;
            if (ts <= 0)
            {
                HudText.SetActive(false);
            }
        }
    }
}
