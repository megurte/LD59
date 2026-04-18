Shader "Custom/FishSpriteWave"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        _WaveAmplitude("Wave Amplitude", Float) = 0.08
        _TailAmplitude("Tail Amplitude", Float) = 0.04
        _WaveFrequency("Wave Frequency", Float) = 3
        _WavePhase("Wave Phase", Float) = 6
        _HeadInfluence("Head Influence", Range(0, 1)) = 0.08
        _TailPower("Tail Power", Float) = 1.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex FishVert
            #pragma fragment FishFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnityCG.cginc"
            #include "UnitySprites.cginc"

            float _WaveAmplitude;
            float _TailAmplitude;
            float _WaveFrequency;
            float _WavePhase;
            float _HeadInfluence;
            float _TailPower;

            v2f FishVert(appdata_t input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float4 vertex = UnityFlipSprite(input.vertex, _Flip);
                output.vertex = UnityObjectToClipPos(vertex);
                output.texcoord = input.texcoord;
                output.color = input.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                output.vertex = UnityPixelSnap(output.vertex);
                #endif

                return output;
            }

            fixed4 FishFrag(v2f input) : SV_Target
            {
                float tail = pow(saturate(1.0 - input.texcoord.y), max(0.0001, _TailPower));
                float weight = lerp(_HeadInfluence, 1.0, tail);
                float bodyWave = sin(_Time.y * _WaveFrequency - tail * _WavePhase) * _WaveAmplitude * weight;
                float tailWave = sin(_Time.y * (_WaveFrequency * 1.18) - tail * (_WavePhase * 1.55)) * _TailAmplitude * tail * tail;
                float2 uv = input.texcoord;

                uv.x += bodyWave + tailWave;
                uv.y += abs(bodyWave + tailWave) * 0.035 * tail;

                fixed4 color = SampleSpriteTexture(uv) * input.color;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
