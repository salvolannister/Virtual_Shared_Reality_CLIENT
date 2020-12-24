Shader "Test"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Outline ("Outline", Range(0.00,1.00)) = 0.1
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
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half3 worldNormal : TEXCOORD1;
                half3 normal : TEXCOORD2;
            };

            float _Outline;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v, float3 normal : NORMAL)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldNormal = UnityObjectToWorldNormal(normal);
                o.normal = normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 offsetCol = tex2D(_MainTex,i.uv - _Outline);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                float3 worldPos = mul(unity_ObjectToWorld, i.vertex).xyz;
                float3 viewDir = _WorldSpaceCameraPos.xyz - worldPos;

                half NdotV = saturate(dot(i.normal, viewDir));

                if(NdotV > _Outline)
                {
                    col.rgb = 0;
                }

                return col;
            }
            ENDCG
        }
    }
}
