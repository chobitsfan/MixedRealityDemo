using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPV_CAM : MonoBehaviour
{
    WebCamTexture webcamTexture;
    public Camera cam;
    public Material mat;
    Texture2D distortMap;    

    // Start is called before the first frame update
    void Start()
    {
        double _CX = 315.46700339723378;
        double _CY = 240.96490293217204;
        double _FX = 246.88640535269982;
        double _FY = 249.75063383890236;
        double _K1 = 0.21874107025134129;
        double _K2 = -0.24239137352267334;
        double _P1 = -0.00089613800498784054;
        double _P2 = 0.00064407518211666542;
        double _K3 = 0.063342985246817154;
        int camWidth = 640;
        int camHeight = 480;
        Debug.Log(SystemInfo.SupportsTextureFormat(TextureFormat.RGFloat) ? "RGFloat supported" : "RGFloat not supported");
        distortMap = new Texture2D(camWidth, camHeight, TextureFormat.RGFloat, false, true);
        distortMap.filterMode = FilterMode.Bilinear;
        distortMap.anisoLevel = 1;
        distortMap.wrapMode = TextureWrapMode.Clamp;
        float[] distortData = new float[camWidth * camHeight * 2];
        for (int i = 0; i < distortData.Length; i++)
        {
            distortData[i] = -1;
        }
        for (float i = -20; i <= camHeight + 20; i += 0.5f)
        {
            for (float j = -20; j <= camWidth + 20; j += 0.5f)
            {
                double x = (j - _CX) / _FX;
                double y = (i - _CY) / _FY;
                double r2 = x * x + y * y;
                double distort = 1 + _K1 * r2 + _K2 * r2 * r2 + _K3 * r2 * r2 * r2;
                double x_distort = x * distort;
                double y_distort = y * distort;
                x_distort += (2 * _P1 * x * y + _P2 * (r2 + 2 * x * x));
                y_distort += (_P1 * (r2 + 2 * y * y) + 2 * _P2 * x * y);
                x_distort = x_distort * _FX + _CX;
                y_distort = y_distort * _FY + _CY;
                int j_distort = (int)Math.Round(x_distort);
                int i_distort = (int)Math.Round(y_distort);
                if (i_distort >= 0 && j_distort >= 0 && i_distort < camHeight && j_distort < camWidth)
                {
                    int idx = i_distort * camWidth * 2 + j_distort * 2; //TextureFormat.RGFloat -> 2 elements (u,v) per pixel
                    distortData[idx] = j / camWidth;
                    distortData[idx + 1] = i / camHeight;
                }
            }
        }
        int count = 0;
        for (int i = 0; i < distortData.Length; i++)
        {
            if (distortData[i] < 0)
            {
                count++;
            }
        }
        Debug.Log("unfilled:" + count / 2);
        distortMap.SetPixelData(distortData, 0);
        distortMap.Apply(false);
        mat.SetTexture("_DistortTex", distortMap);

        WebCamDevice[] webCams = WebCamTexture.devices;
        foreach (WebCamDevice webCam in webCams)
        {
            if (webCam.name.StartsWith("USB2.0"))
            {
                Debug.Log("background camera:" + webCam.name);
                webcamTexture = new WebCamTexture(webCam.name);
                webcamTexture.Play();
                mat.SetTexture("_CamTex", webcamTexture);
                break;
            }
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }
}
