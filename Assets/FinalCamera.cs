using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class FinalCamera : MonoBehaviour
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

    public RenderTexture MainCamRT;
    public Material mat;
    public bool ConnectCamera = true;

    Texture2D txt;
    static readonly int REAL_WIDTH = (int)(1280 * 640 / 795.04077928936863);
    static readonly int REAL_HEIGHT = REAL_WIDTH * 9 / 16;

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
        //Debug.Log(Screen.width + "x" + Screen.height);
        Debug.Log("copyTextureSupport:" + SystemInfo.copyTextureSupport);
        txt = new Texture2D(REAL_WIDTH, REAL_HEIGHT);

        ptr = IntPtr.Zero;
        ptr = NPlayer_Init();
        if (ConnectCamera) NPlayer_Connect(ptr, "rtsp://192.168.1.113/v1/", 1);
        bStart = false;
    }    

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.CopyTexture(MainCamRT, 0, 0, (1280-REAL_WIDTH)/2, (720-REAL_HEIGHT)/2, txt.width, txt.height, txt, 0, 0, 0, 0);
        Graphics.Blit(txt, destination, mat);
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
}
