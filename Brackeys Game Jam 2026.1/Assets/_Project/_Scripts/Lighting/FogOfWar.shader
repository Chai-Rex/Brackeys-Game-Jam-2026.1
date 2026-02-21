Shader "Custom/FogOfWar"
{
    /*
        FogOfWar.shader - Quad overlay shader.

        This shader is applied to a world-space quad that follows the camera.
        The quad's UV (0,0)-(1,1) maps to the quad's surface in world space.
        The shader converts that to a fog texture UV using the quad's world rect
        and the map's world rect - both passed as simple float4 uniforms.

        No I_VP matrix. No frustum reconstruction. Just UV math.

        fogValue = 0.0  ->  black (never seen)
        fogValue = 0..1 ->  dark overlay (explored)
        fogValue = 1.0  ->  transparent (fully visible)
    */

    Properties
    {
        _FogTex       ("Fog Render Texture", 2D) = "black" {}
        _UnseenColor  ("Unseen Color",  Color)   = (0, 0, 0, 1)
        _ExploredTint ("Explored Tint", Color)   = (0.04, 0.04, 0.08, 1)
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

            // World rect of the fog map:
            //   x = map world left
            //   y = map world bottom
            //   z = map world width
            //   w = map world height
            float4 _FogWorldRect;

            // World rect of the overlay quad (set each frame by FogOfWarManager):
            //   x = quad world left
            //   y = quad world bottom
            //   z = quad world width
            //   w = quad world height
            float4 _QuadWorldRect;

            float4 _UnseenColor;
            float4 _ExploredTint;

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

            half4 Frag(Varyings IN) : SV_Target
            {
                // Convert quad UV [0,1] to world position.
                float worldX = _QuadWorldRect.x + IN.uv.x * _QuadWorldRect.z;
                float worldY = _QuadWorldRect.y + IN.uv.y * _QuadWorldRect.w;

                // Convert world position to fog texture UV.
                float2 fogUV = float2(
                    (worldX - _FogWorldRect.x) / _FogWorldRect.z,
                    (worldY - _FogWorldRect.y) / _FogWorldRect.w
                );

                // Outside the map -> fully dark.
                if (fogUV.x < 0.0 || fogUV.x > 1.0 || fogUV.y < 0.0 || fogUV.y > 1.0)
                    return half4(_UnseenColor.rgb, 1.0);

                float fogValue = SAMPLE_TEXTURE2D(_FogTex, sampler_FogTex, fogUV).r;

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
