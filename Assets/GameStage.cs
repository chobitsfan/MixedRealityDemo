﻿using System.Diagnostics;
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
    public GameObject ScoreText;
    float hudTs = 0f;
    Stopwatch stopwatch;
    int checkPointLeft;
    int checkPointCount;
    string appStartTime;
    private void Start()
    {
        stopwatch = new Stopwatch();
        var checkPoints = GameObject.FindGameObjectsWithTag("CheckPoint");
        checkPointCount = checkPoints.Length;
        checkPointLeft = checkPointCount;
        appStartTime = System.DateTime.Now.ToString("u");
    }

    public void Warning(bool warning = true)
    {
        emergency.SetActive(warning);
    }

    public void PassCheckPoint()
    {
        if (checkPointLeft == checkPointCount)
        {
            stopwatch.Start();
            checkPointLeft--;
            UnityEngine.UI.Text text = HudText.GetComponent<UnityEngine.UI.Text>();
            text.text = "START";
            hudTs = 1f;
            HudText.SetActive(true);
        }
        else if (checkPointLeft == 1)
        {
            stopwatch.Stop();
            checkPointLeft--;
            UnityEngine.UI.Text text = HudText.GetComponent<UnityEngine.UI.Text>();
            text.text = "FINISH";
            hudTs = 2f;
            HudText.SetActive(true);

            System.IO.StreamWriter logfile = new System.IO.StreamWriter("game_log.txt", true);
            logfile.WriteLine(appStartTime + " -> " + stopwatch.ElapsedMilliseconds + " ms");
            logfile.Close();
        }
        else
        {
            checkPointLeft--;
            UnityEngine.UI.Text text = HudText.GetComponent<UnityEngine.UI.Text>();
            text.text = checkPointLeft + " TO GO";
            hudTs = 1f;
            HudText.SetActive(true);
        }
    }

    public void HitObstacle()
    {

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
        if (Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }
    }

    public void ResetStage()
    {
        var checkPoints = GameObject.FindGameObjectsWithTag("CheckPoint");
        foreach (var checkPoint in checkPoints)
        {
            checkPoint.SetActive(true);
        }
        checkPointCount = checkPoints.Length;
        checkPointLeft = checkPointCount;
        stopwatch.Reset();
    }
}
