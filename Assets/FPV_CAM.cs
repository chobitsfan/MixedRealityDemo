#define DISTORT

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPV_CAM : MonoBehaviour
{
    public Material mat;

    Texture2D distortMap;

    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
#if DISTORT
        double _CX = 639.5;
        double _CY = 359.5;
        double _FX = 795.04077928936863;
        double _FY = 795.04077928936863;
        double _K1 = -0.36042148522727718;
        double _K2 = 0.16639675638606838;
        double _P1 = -2.7878989585024992 * 0.001;
        double _P2 = -9.9880941608639822 * 0.0001;
        double _K3 = -3.0924601782932153 * 0.01;
        int camWidth = cam.targetTexture.width;
        int camHeight = cam.targetTexture.height;
        Debug.Log(SystemInfo.SupportsTextureFormat(TextureFormat.RGFloat) ? "RGFloat supported" : "RGFloat not supported");
        distortMap = new Texture2D(camWidth, camHeight, TextureFormat.RGFloat, false, true)
        {
            filterMode = FilterMode.Bilinear,
            anisoLevel = 2,
            wrapMode = TextureWrapMode.Clamp
        };
        float[] distortData = new float[camWidth * camHeight * 2];
        for (float i = 0; i < camHeight; i += 1)
        {
            for (float j = 0; j < camWidth; j += 1)
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
        distortMap.SetPixelData(distortData, 0);
        distortMap.Apply(false);
        mat.SetTexture("_DistortTex", distortMap);
#endif
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {        
        Graphics.Blit(source, destination, mat);
    }
}
