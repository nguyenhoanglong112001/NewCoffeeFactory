Shader "UI/SpotlightOverlay"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (0,0,0,0.8)
        _SpotlightCenter ("Spotlight Center", Vector) = (0.5, 0.5, 0, 0)
        _SpotlightRadius ("Spotlight Radius", Range(0, 1)) = 0.2
        _SoftEdge ("Soft Edge", Range(0, 0.5)) = 0.1
        _Intensity ("Spotlight Intensity", Range(0, 2)) = 1.0
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Overlay" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            float4 _SpotlightCenter;
            float _SpotlightRadius;
            float _SoftEdge;
            float _Intensity;
            float4 _ClipRect;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                // Tính kho?ng cách t? pixel hi?n t?i ??n tâm spotlight
                float2 center = _SpotlightCenter.xy;
                float2 uv = IN.texcoord;
                float distance = length(uv - center);
                
                // T?o gradient t? tâm spotlight
                float spotlight = 1.0 - smoothstep(_SpotlightRadius - _SoftEdge, 
                                                  _SpotlightRadius + _SoftEdge, 
                                                  distance);
                
                // Áp d?ng hi?u ?ng spotlight
                fixed4 color = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // Gi?m ?? m? trong vùng spotlight
                color.a *= (1.0 - spotlight * _Intensity);
                
                // Clipping cho UI
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                // Lo?i b? pixel trong su?t hoàn toàn
                clip(color.a - 0.001);
                
                return color;
            }
            ENDCG
        }
    }
}