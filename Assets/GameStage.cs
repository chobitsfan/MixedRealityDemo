using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class GameStage : MonoBehaviour
{
    public GameObject emergency;
    public Vector3 Center;
    public float Radius;
    public int CheckPointCount;
    public GameObject CheckPointSign;
    public GameObject Ring;
    public GameObject LapTimeMsg;
    public GameObject HudText;
    float hudTs = 0f;
    List<GameObject> stageObjects;
    int CheckPointPassed = 0;
    Stopwatch stopwatch;
    private void Start()
    {
        stageObjects = new List<GameObject>();
        stopwatch = new Stopwatch();
        ResetStage();
    }

    public void Warning(bool warning = true)
    {
        emergency.SetActive(warning);
    }

    public void PassCheckPoint()
    {
        CheckPointPassed++;
        if (CheckPointPassed == 1)
        {
            stopwatch.Start();
            UnityEngine.UI.Text text = HudText.GetComponent<UnityEngine.UI.Text>();
            text.text = "START";
            hudTs = 1f;
            HudText.SetActive(true);
        }
        else if (CheckPointPassed == CheckPointCount)
        {
            stopwatch.Stop();
            UnityEngine.UI.Text text = HudText.GetComponent<UnityEngine.UI.Text>();
            text.text = "FINISH";
            hudTs = 5f;
            HudText.SetActive(true);
        }
        else
        {
            UnityEngine.UI.Text text = HudText.GetComponent<UnityEngine.UI.Text>();
            text.text = "GOOD";
            hudTs = 1f;
            HudText.SetActive(true);
        }
    }

    private void Update()
    {
        long laptime = stopwatch.ElapsedMilliseconds;
        if (laptime > 0)
        {
            long mm = laptime / 60000;
            long ss = laptime / 1000 % 60;
            long ms = laptime % 1000;
            LapTimeMsg.GetComponent<UnityEngine.UI.Text>().text = mm.ToString("D2") + ":" + ss.ToString("D2") + "." + ms.ToString("D3");
        }
        if (hudTs > 0)
        {
            hudTs -= Time.deltaTime;
            if (hudTs <= 0)
            {
                HudText.SetActive(false);
            }
        }
    }

    public void ResetStage()
    {
        CheckPointPassed = 0;
        if (stageObjects.Count > 0)
        {
            foreach (GameObject obj in stageObjects)
            {
                Destroy(obj);
            }
            stageObjects.Clear();
        }
        float step = 2 * Mathf.PI / CheckPointCount;
        float rad = 0;
        for (int i = 0; i < CheckPointCount; i++)
        {
            float radius = Radius + UnityEngine.Random.Range(-1f, 1f);
            Vector3 pos = new Vector3(radius * Mathf.Cos(rad), UnityEngine.Random.Range(-1f, 1f), radius * Mathf.Sin(rad));
            GameObject checkpoint = GameObject.Instantiate(CheckPointSign, Center + pos, Quaternion.identity);
            stageObjects.Add(checkpoint);
            if (i == 0 || i == 4 || i == 8)
            {
                GameObject ring = GameObject.Instantiate(Ring, Center + pos, Quaternion.identity);
                stageObjects.Add(ring);
            }
            rad += step;
        }
    }
}
