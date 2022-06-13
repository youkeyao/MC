Shader "Custom/MyShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _TextureArray("TextureArray", 2DArray) = "white"{}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 id : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 id : TEXCOORD1;
            };
            
            UNITY_DECLARE_TEX2DARRAY(_TextureArray);

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.id = float2(v.id.y, 0);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 r = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, fixed3(i.uv, i.id.x));
                return r;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
