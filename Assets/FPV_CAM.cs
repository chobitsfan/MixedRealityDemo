#define DISTORT

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPV_CAM : MonoBehaviour
{
    [DllImport("vplayerUnity.dll")]
    public static extern IntPtr NPlayer_Init();
    [DllImport("vplayerUnity.dll")]
    public static extern int NPlayer_Connect(IntPtr pPlayer, string url, int mode);
    [DllImport("vplayerUnity.dll")]
    public static extern int NPlayer_GetWidth(IntPtr pPlayer);
    [DllImport("vplayerUnity.dll")]
    public static extern int NPlayer_GetHeight(IntPtr pPlayer);
    [DllImport("vplayerUnity.dll")]
    public static extern int NPlayer_Uninit(IntPtr pPlayer);
    [DllImport("vplayerUnity.dll")]
    public static extern int NPlayer_ReadFrame(IntPtr pPlayer, IntPtr buffer, out UInt64 timestamp);

    public Material mat;
    public UnityEngine.UI.InputField IpInputText;
    public bool ConnectCamera;

    Texture2D distortMap;

    IntPtr ptr = IntPtr.Zero;
    bool bStart = false;
    Texture2D texY;
    Texture2D texU;
    Texture2D texV;
    int w, h;    
    byte[] buffer;
    protected IntPtr unmanagedBuffer;

    // Start is called before the first frame update
    void Start()
    {
        ptr = IntPtr.Zero;
        ptr = NPlayer_Init();        
        bStart = false;
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
#if true
        double _FX2 = 567.82342529; //getOptimalNewCameraMatrix wth alpha = 1
        double _CX2 = 634.76967966;
        double _FY2 = 569.19769287;
        double _CY2 = 352.92940841;
#else
        double _FX2 = 606.21734619; //getOptimalNewCameraMatrix wth alpha = 0
        double _CX2 = 635.51305371;
        double _FY2 = 733.26403809;
        double _CY2 = 357.52916387;
#endif
        int camWidth = 1280;
        int camHeight = 720;
        Debug.Log(SystemInfo.SupportsTextureFormat(TextureFormat.RGFloat) ? "RGFloat supported" : "RGFloat not supported");
        distortMap = new Texture2D(camWidth, camHeight, TextureFormat.RGFloat, false, true)
        {
            filterMode = FilterMode.Bilinear,
            anisoLevel = 2,
            wrapMode = TextureWrapMode.Clamp
        };
        float[] distortData = new float[camWidth * camHeight * 2];
        for (int i = 0; i < distortData.Length; i++)
        {
            distortData[i] = -1;
        }
        for (double py = 0; py < camHeight; py += 0.5)
        {
            for (double px = 0; px < camWidth; px += 0.5)
            {
                double x = (px - _CX2) / _FX2;
                double y = (py - _CY2) / _FY2;
                double r2 = x * x + y * y;
                double distort = 1 + _K1 * r2 + _K2 * r2 * r2 + _K3 * r2 * r2 * r2;
                double x_distort = x * distort;
                double y_distort = y * distort;
                x_distort += (2 * _P1 * x * y + _P2 * (r2 + 2 * x * x));
                y_distort += (_P1 * (r2 + 2 * y * y) + 2 * _P2 * x * y);
                x_distort = x_distort * _FX + _CX;
                y_distort = y_distort * _FY + _CY;
                /*if (px == 0 && py == 0)
                {
                    Debug.Log("(0, 0) -> " + x_distort + " , " + y_distort);
                }*/
                int j_distort = (int)Math.Round(x_distort);
                int i_distort = (int)Math.Round(y_distort);
                if (i_distort >= 0 && j_distort >= 0 && i_distort < camHeight && j_distort < camWidth)
                {
                    int idx = i_distort * camWidth * 2 + j_distort * 2; //TextureFormat.RGFloat -> 2 elements (u,v) per pixel
                    distortData[idx] = (float)(px / camWidth);
                    distortData[idx + 1] = (float)(py / camHeight);
                }
            }
        }
        
        distortMap.SetPixelData(distortData, 0);
        distortMap.Apply(false);
        mat.SetTexture("_DistortTex", distortMap);
#endif
    }

    public void OnConnClicked()
    {
        if (ConnectCamera) NPlayer_Connect(ptr, "rtsp://192.168.50." + IpInputText.text + "/v1/", 1);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat);
    }

    void initVideoFrameBuffer()
    {
        w = NPlayer_GetWidth(ptr);
        h = NPlayer_GetHeight(ptr);
        if (w != 0 && h != 0)
        {
            Debug.Log("width = " + w + ", height = " + h);
            int frameLen = w * h * 3;
            Debug.Log("frameLen = " + frameLen);
            buffer = new byte[frameLen];
            unmanagedBuffer = Marshal.AllocHGlobal(frameLen);

            bStart = true;

            texY = new Texture2D(w, h, TextureFormat.Alpha8, false);
            //U分量和V分量分別存放在兩張貼圖中
            texU = new Texture2D(w >> 1, h >> 1, TextureFormat.Alpha8, false);
            texV = new Texture2D(w >> 1, h >> 1, TextureFormat.Alpha8, false);
            mat.SetTexture("_YTex", texY);
            mat.SetTexture("_UTex", texU);
            mat.SetTexture("_VTex", texV);
        }
    }

    void releaseVideoFrameBuffer()
    {
        if (unmanagedBuffer == IntPtr.Zero)
            Marshal.FreeHGlobal(unmanagedBuffer);
    }

    void getVideoFameBuffer()
    {
        UInt64 timestamp;
        int frameLen = NPlayer_ReadFrame(ptr, unmanagedBuffer, out timestamp);
        Marshal.Copy(unmanagedBuffer, buffer, 0, frameLen);

        int Ycount = w * h;
        int UVcount = w * (h >> 2);
        texY.SetPixelData(buffer, 0, 0);
        texY.Apply();
        texU.SetPixelData(buffer, 0, Ycount);
        texU.Apply();
        texV.SetPixelData(buffer, 0, Ycount + UVcount);
        texV.Apply();
    }

    void Update()
    {
        if (ConnectCamera)
        {
            if (bStart)
            {
                //Debug.Log("getVideoFameBuffer");
                getVideoFameBuffer();
            }
            else
            {
                //Debug.Log("initVideoFrameBuffer");
                initVideoFrameBuffer();
            }
        }
    }

    private void OnDestroy()
    {
        //Debug.Log("VplayerUnityframeReader OnDestroy");
        NPlayer_Uninit(ptr);
        ptr = IntPtr.Zero;
        if (bStart) releaseVideoFrameBuffer();
    }

    private void Reset()
    {
        ConnectCamera = true;
    }
}
