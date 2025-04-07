Shader "Custom/RandomGlitch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitchProbability ("Glitch Probability", Range(0, 1)) = 0.03 // 3% chance
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.1
        _ColorGlitchIntensity ("Color Glitch", Range(0, 1)) = 0.1
        _DisplacementSpeed ("Displacement Speed", Range(0, 10)) = 2.5
        _VerticalDisplacement ("Vertical Displacement", Range(0, 1)) = 0.1
        _HorizontalShake ("Horizontal Shake", Range(0, 1)) = 0.05
        _ScanLineJitter ("Scan Line Jitter", Range(0, 1)) = 0.2
        _GlitchSeed ("Glitch Seed", Float) = 0 // Used to make glitches unique per object
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
                float4 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _GlitchProbability;
            float _GlitchIntensity;
            float _ColorGlitchIntensity;
            float _DisplacementSpeed;
            float _VerticalDisplacement;
            float _HorizontalShake;
            float _ScanLineJitter;
            float _GlitchSeed;
            
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
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Combine object position and seed for unique randomization per object
                float objectID = _GlitchSeed + i.worldPos.x + i.worldPos.y + i.worldPos.z;
                
                // Determine if this object should glitch (3% chance or whatever is set in _GlitchProbability)
                float shouldGlitch = random(float2(objectID, 0.42)) < _GlitchProbability;
                
                // If we shouldn't glitch, just return the texture color
                if (!shouldGlitch)
                {
                    fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                    return col;
                }
                
                // Otherwise, apply glitch effects
                float time = _Time.y * _DisplacementSpeed;
                
                // Use object ID to create different glitch patterns for different objects
                // This ensures each glitching object has its own unique pattern
                float randomBase = random(float2(floor(time * 10.0 + objectID), floor(time * 10.0)));
                
                // Glitch frequency changes based on time
                float glitchPhase = floor(time * 3.0 + objectID * 10.0);
                float glitchStrength = random(float2(glitchPhase, objectID));
                
                // Only apply glitch during certain time periods
                bool activeGlitch = glitchStrength > 0.7;
                
                // If not in active glitch period, return normal texture
                if (!activeGlitch)
                {
                    fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                    return col;
                }
                
                // Apply scan line jitter
                float scanLineJitter = random(float2(floor(i.uv.y * 100 + time + objectID), time));
                bool applyJitter = scanLineJitter < _ScanLineJitter;
                float jitterAmount = 0.01 + random(float2(i.uv.y * time + objectID, i.uv.y * time)) * 0.03;
                
                float2 uv = i.uv;
                if (applyJitter)
                {
                    uv.x += jitterAmount * (randomBase - 0.5);
                }
                
                // Vertical displacement
                float vertJitterThreshold = 1.0 - _VerticalDisplacement;
                if (randomBase > vertJitterThreshold)
                {
                    float displaceAmount = (random(float2(time + objectID, 2)) - 0.5) * 0.2;
                    uv.y = frac(uv.y + displaceAmount);
                }
                
                // Horizontal shake
                float horizontalShake = (random(float2(time + objectID, 3)) * 2 - 1) * _HorizontalShake;
                uv.x += horizontalShake;
                
                // Sample the texture with modified UV
                fixed4 col = tex2D(_MainTex, uv);
                
                // Color channel separation (RGB shift)
                if (randomBase < _ColorGlitchIntensity)
                {
                    float rShift = random(float2(time + objectID, 4)) * 0.02;
                    float gShift = random(float2(time + objectID, 5)) * 0.02;
                    float bShift = random(float2(time + objectID, 6)) * 0.02;
                    
                    col.r = tex2D(_MainTex, uv + float2(rShift, 0)).r;
                    col.g = tex2D(_MainTex, uv + float2(0, gShift)).g;
                    col.b = tex2D(_MainTex, uv + float2(bShift, 0)).b;
                }
                
                // Random block glitches
                if (random(float2(floor(uv.y * 40), time + objectID)) < _GlitchIntensity * 0.2)
                {
                    col = tex2D(_MainTex, uv + float2(random(float2(floor(uv.y * 10), time + objectID)) * 0.1, 0));
                }
                
                // Combine with original vertex color
                col *= i.color;
                
                return col;
            }
            ENDCG
        }
    }
}