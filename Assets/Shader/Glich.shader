Shader "Custom/Glitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.1
        _ColorGlitchIntensity ("Color Glitch", Range(0, 1)) = 0.1
        _DisplacementSpeed ("Displacement Speed", Range(0, 10)) = 2.5
        _VerticalDisplacement ("Vertical Displacement", Range(0, 1)) = 0.1
        _HorizontalShake ("Horizontal Shake", Range(0, 1)) = 0.05
        _ScanLineJitter ("Scan Line Jitter", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        
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
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _GlitchIntensity;
            float _ColorGlitchIntensity;
            float _DisplacementSpeed;
            float _VerticalDisplacement;
            float _HorizontalShake;
            float _ScanLineJitter;
            
            // Random function
            float random(float2 seed)
            {
                return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _DisplacementSpeed;
                
                // Calculate random glitch values
                float randomValue = random(float2(floor(time * 10.0), floor(time * 10.0)));
                
                // Scan line jitter effect - Distort uv.y
                float scanLineJitter = random(float2(floor(i.uv.y * 100 + time), time));
                bool applyJitter = scanLineJitter < _ScanLineJitter;
                float jitterAmount = 0.01 + random(float2(i.uv.y * time, i.uv.y * time)) * 0.03;
                float2 uv = i.uv;
                
                if (applyJitter)
                {
                    uv.x += jitterAmount * (randomValue - 0.5);
                }
                
                // Vertical displacement
                float vertJitterThreshold = 1.0 - _VerticalDisplacement;
                if (randomValue > vertJitterThreshold)
                {
                    float displaceAmount = (random(float2(time, 2)) - 0.5) * 0.2;
                    uv.y = frac(uv.y + displaceAmount);
                }
                
                // Horizontal shake
                float horizontalShake = (random(float2(time, 3)) * 2 - 1) * _HorizontalShake;
                uv.x += horizontalShake;
                
                // Sample the texture with modified UV
                fixed4 col = tex2D(_MainTex, uv);
                
                // Color channel separation (RGB shift)
                if (randomValue < _ColorGlitchIntensity)
                {
                    float rShift = random(float2(time, 4)) * 0.02;
                    float gShift = random(float2(time, 5)) * 0.02;
                    float bShift = random(float2(time, 6)) * 0.02;
                    
                    col.r = tex2D(_MainTex, uv + float2(rShift, 0)).r;
                    col.g = tex2D(_MainTex, uv + float2(0, gShift)).g;
                    col.b = tex2D(_MainTex, uv + float2(bShift, 0)).b;
                }
                
                // Random block glitches
                if (random(float2(floor(uv.y * 40), time)) < _GlitchIntensity * 0.2)
                {
                    col = tex2D(_MainTex, uv + float2(random(float2(floor(uv.y * 10), time)) * 0.1, 0));
                }
                
                // Combine with original vertex color
                col *= i.color;
                
                return col;
            }
            ENDCG
        }
    }
}