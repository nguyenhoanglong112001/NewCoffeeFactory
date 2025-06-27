Shader "UI/CutoutHole"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,0.7)
        _HoleCenter ("Hole Center", Vector) = (0.5,0.5,0,0)
        _HoleSize ("Hole Size", Float) = 0.2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            float4 _HoleCenter;
            float _HoleSize;

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

            fixed4 frag (v2f i) : SV_Target
            {
                float dist = distance(i.uv, _HoleCenter.xy);
                if (dist < _HoleSize)
                    return fixed4(0,0,0,0); // trong su?t trong vùng l?
                return _Color;
            }
            ENDCG
        }
    }
}