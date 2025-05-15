Shader "Unlit/PortalShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            };

            struct v2f
            {
                float4 clipPos : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                o.clipPos = clipPos;
                o.screenPos = ComputeScreenPos(clipPos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenSpaceUV = i.screenPos.xy / i.screenPos.w;
                return tex2D(_MainTex, screenSpaceUV);
            }
            ENDCG
        }
    }
}
