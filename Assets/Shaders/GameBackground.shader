Shader "Unlit/GameBackground"
{
    Properties
    {
        _TopColor    ("Top Color",    Color) = (1,1,1,1)
        _BottomColor ("Bottom Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Two color properties exposed in the Inspector
            fixed4 _TopColor;
            fixed4 _BottomColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv       : TEXCOORD0;
                float4 pos      : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Use the mesh’s UV.y (0 at bottom, 1 at top) as the interpolation factor
                float t = saturate(i.uv.y);
                // Linearly interpolate (lerp) between bottom and top color
                return lerp(_BottomColor, _TopColor, t);
            }
            ENDCG
        }
    }
    // Fallback can be anything you like. We’ll just fallback to an Unlit/Color.
    FallBack "Unlit/Color"
}
