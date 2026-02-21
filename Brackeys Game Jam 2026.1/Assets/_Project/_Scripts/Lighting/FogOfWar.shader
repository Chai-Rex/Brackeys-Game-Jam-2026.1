Shader "Custom/FogOfWar"
{
    Properties
    {
        _FogTex       ("Fog Render Texture", 2D) = "black" {}
        _UnseenColor  ("Unseen Color",  Color)   = (0, 0, 0, 1)
        _ExploredTint ("Explored Tint", Color)   = (0.04, 0.04, 0.08, 1)
        _BlurRadius   ("Blur Radius (texels)", Range(0, 8)) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue"          = "Transparent"
            "RenderType"     = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest  Always
        Cull   Off

        Pass
        {
            Name "FogOfWarPass"

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_FogTex);
            SAMPLER(sampler_FogTex);

            // x = map world left (bottom-left corner of tile 0)
            // y = map world bottom
            // z = map world width  (mapSize.x * tileSize)
            // w = map world height (mapSize.y * tileSize)
            float4 _FogWorldRect;

            // x = quad world left, y = quad world bottom
            // z = quad world width, w = quad world height
            float4 _QuadWorldRect;

            float4 _UnseenColor;
            float4 _ExploredTint;
            float  _BlurRadius;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float SampleFogBlurred(float2 fogUV, float2 texelSize)
            {
                float result      = 0.0;
                float totalWeight = 0.0;

                float weights[5];
                weights[0] = 0.2270;
                weights[1] = 0.1945;
                weights[2] = 0.1216;
                weights[3] = 0.0540;
                weights[4] = 0.0162;

                for (int dy = -4; dy <= 4; dy++)
                {
                    for (int dx = -4; dx <= 4; dx++)
                    {
                        float2 sampleUV = fogUV + float2(dx, dy) * texelSize * _BlurRadius;
                        sampleUV = clamp(sampleUV, float2(0.0, 0.0), float2(1.0, 1.0));

                        int ax = abs(dx);
                        int ay = abs(dy);
                        float w = weights[ax] * weights[ay];

                        result      += SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, sampleUV).r * w;
                        totalWeight += w;
                    }
                }

                return result / totalWeight;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                // Convert quad UV to world position.
                float worldX = _QuadWorldRect.x + IN.uv.x * _QuadWorldRect.z;
                float worldY = _QuadWorldRect.y + IN.uv.y * _QuadWorldRect.w;

                // Convert world position to fog texture UV.
                // _FogWorldRect.xy is the bottom-left corner of tile 0 (CellToWorld origin).
                // The fog texture has one texel per tile, so texel N covers the tile
                // whose bottom-left corner is at origin + N * tileSize.
                // UV = (worldPos - origin) / totalWorldSize maps tile N to [N/mapSize, (N+1)/mapSize].
                // The texel CENTER is at UV = (N + 0.5) / mapSize, which Unity samples correctly
                // when FilterMode is Bilinear and we sample at the texel center.
                float2 fogUV = float2(
                    (worldX - _FogWorldRect.x) / _FogWorldRect.z,
                    (worldY - _FogWorldRect.y) / _FogWorldRect.w
                );

                // Outside the map -> fully dark.
                if (fogUV.x < 0.0 || fogUV.x > 1.0 || fogUV.y < 0.0 || fogUV.y > 1.0)
                    return half4(_UnseenColor.rgb, 1.0);

                // Get texture dimensions for blur offset.
                float texW, texH;
                _FogTex.GetDimensions(texW, texH);
                float2 texelSize = float2(1.0 / texW, 1.0 / texH);

                float fogValue;
                if (_BlurRadius < 0.01)
                    fogValue = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, fogUV).r;
                else
                    fogValue = SampleFogBlurred(fogUV, texelSize);

                if (fogValue < 0.001)
                    return half4(_UnseenColor.rgb, 1.0);

                if (fogValue > 0.999)
                    return half4(0.0, 0.0, 0.0, 0.0);

                float overlayAlpha = 1.0 - fogValue;
                half3 overlayCol   = lerp(_UnseenColor.rgb, _ExploredTint.rgb, fogValue);
                return half4(overlayCol, overlayAlpha);
            }

            ENDHLSL
        }
    }
}
