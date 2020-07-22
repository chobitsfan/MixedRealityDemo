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
    double _CX = 315.467;
    double _CY = 240.9649;
    double _FX = 246.8864;
    double _FY = 249.7506;
    double _K1 = 0.21874;
    double _K2 = -0.24239;
    double _P1 = -0.00089613;
    double _P2 = 0.00064407;
    double _K3 = 0.063342;

    // Start is called before the first frame update
    void Start()
    {
        int camWidth = 640;
        int camHeight = 480;
        Debug.Log(Screen.width + "x" + Screen.height + ":" + SystemInfo.SupportsTextureFormat(TextureFormat.RGFloat));
        int width = Screen.width;
        int height = Screen.height;
        distortMap = new Texture2D(width, height, TextureFormat.RGFloat, false, true);
        distortMap.filterMode = FilterMode.Point;
        distortMap.anisoLevel = 1;
        distortMap.wrapMode = TextureWrapMode.Clamp;
        float[] distortData = new float[width * height * 2];
        for (int i = 0; i < distortData.Length; i++)
        {
            distortData[i] = -1;
        }
        for (double i = 0; i < camHeight; i+=0.5)
        {
            for (double j = 0; j < camWidth; j+=0.5)
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
                //Debug.Log(x_distort + "," + y_distort);
                int idxU = (int)Math.Round(x_distort / camWidth * width);
                int idxV = (int)Math.Round(y_distort / camHeight * height);
                if (idxU >=0 && idxV>=0 && idxU < width && idxV < height)
                {
                    int mapIdx = idxV * width * 2 + idxU * 2;
                    //Debug.Log(mapIdx);
                    distortData[mapIdx] = (float)j / camWidth;
                    distortData[mapIdx + 1] = (float)i / camHeight;
                }
            }
        }
        /*for (int i = 0; i < distortData.Length; i++)
        {
            if (distortData[i] < 0)
            {
                distortData[i] = distortData[i - 1];
            }
        }*/
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
