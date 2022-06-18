Shader "Custom/MyShader"
{
    Properties
    {
        _TextureArray("TextureArray", 2DArray) = "white"{}
        _Specular ("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _Gloss ("Gloss", Range(8.0, 256)) = 20
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Tags{"LightMode" = "ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase_fullshadows
            #pragma target 5.0
            
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct vData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 id : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 id : TEXCOORD1;
                float4 worldPos : TEXCOORD3;
                SHADOW_COORDS(4)
            };
            
            UNITY_DECLARE_TEX2DARRAY(_TextureArray);
            fixed4 _Specular;
            float _Gloss;

            v2f vert (vData v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
                o.uv = v.uv;
                o.id = v.id;
                o.worldPos = v.vertex;

                TRANSFER_SHADOW(o);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 color = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, fixed3(i.uv, i.id.y));
                if (i.id.x < 0) {
                    return color;
                }
                
                float4 lightColor = float4(1, 1, 1, 1);

                fixed3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
                fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));

                fixed3 albedo = color.rgb;
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
                 fixed3 diffuse = lightColor.rgb * albedo * max(0, dot(i.normal, lightDir));

                fixed3 halfDir = normalize(lightDir + viewDir);
                 fixed3 specular = lightColor.rgb * _Specular.rgb * pow(max(0, dot(i.normal, halfDir)), _Gloss);
            
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz);
                return fixed4(ambient + (diffuse + specular) * atten, 1.0);
            }
            ENDCG
        }

        Pass
        { 
            Tags { "LightMode"="ForwardAdd" }
            
            Blend One One
        
            CGPROGRAM
            
            #pragma multi_compile_fwdadd
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            struct vData
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 id : TEXCOORD1;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 id : TEXCOORD1;
                float4 worldPos : TEXCOORD3;
                SHADOW_COORDS(4)
            };

            UNITY_DECLARE_TEX2DARRAY(_TextureArray);
            fixed4 _Specular;
            float _Gloss;
            
            v2f vert(vData v)
            {
                 v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
                o.uv = v.uv;
                o.id = v.id;
                o.worldPos = v.vertex;

                TRANSFER_SHADOW(o);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = UNITY_SAMPLE_TEX2DARRAY(_TextureArray, fixed3(i.uv, i.id.y));
                if (i.id.x < 0) {
                    return color;
                }
                
                float4 lightColor = float4(1, 1, 1, 1);

                fixed3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
                fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                
                fixed3 albedo = color.rgb;
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
                fixed3 diffuse = lightColor.rgb * albedo * max(0, dot(i.normal, lightDir));
                 
                 fixed3 halfDir = normalize(lightDir + viewDir);
                 fixed3 specular = lightColor.rgb * _Specular.rgb * pow(max(0, dot(i.normal, halfDir)), _Gloss);
            
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz);
 
                return fixed4((diffuse + specular) * atten, 1.0);
            }
            
            ENDCG
        }

        Pass 
        {
            Tags 
            {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct app_data 
            {
                float4 vertex : POSITION;
            };
            struct v2f 
            {
                V2F_SHADOW_CASTER;
            };
            v2f vert (app_data v) 
            {
                v2f o;
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(v2f i) : COLOR 
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Specular"
}
