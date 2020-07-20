﻿Shader "OpenCV Distort Map"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CamTex("Texture", 2D) = "white" {}
        _DistortTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _CamTex;
            sampler2D _DistortTex;

            fixed4 frag(v2f i) : SV_Target
            {
                float4 data = tex2D(_DistortTex, i.uv);
                fixed4 col = tex2D(_MainTex, float2(data.x,data.y));
                return col * col.a + tex2D(_CamTex, i.uv) * (1 - col.a);
                //return col;
            }
            ENDCG
        }
    }
}
