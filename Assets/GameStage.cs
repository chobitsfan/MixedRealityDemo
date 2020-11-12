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
        }
        else if (CheckPointPassed == CheckPointCount)
        {
            stopwatch.Stop();
        }
    }

    private void Update()
    {
        LapTimeMsg.GetComponent<UnityEngine.UI.Text>().text = (stopwatch.ElapsedMilliseconds / 10).ToString("D6");
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
        int ringNum = UnityEngine.Random.Range(0, CheckPointCount);
        for (int i = 0; i < CheckPointCount; i++)
        {
            float radius = Radius + UnityEngine.Random.Range(-1f, 1f);
            Vector3 pos = new Vector3(radius * Mathf.Cos(rad), UnityEngine.Random.Range(-1f, 1f), radius * Mathf.Sin(rad));
            GameObject checkpoint = GameObject.Instantiate(CheckPointSign, Center + pos, Quaternion.identity);
            stageObjects.Add(checkpoint);
            if (i == ringNum || i == ringNum - 3 || i == ringNum + 3)
            {
                GameObject ring = GameObject.Instantiate(Ring, Center + pos, Quaternion.identity);
                stageObjects.Add(ring);
            }
            rad += step;
        }
    }
}
