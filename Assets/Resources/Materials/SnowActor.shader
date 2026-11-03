Shader "InisLand/SnowActor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Saturation ("Saturation", Range(0,1)) = .84
        _Contrast ("Contrast", Range(0,1)) = .94
        _SnowTint ("Snow daylight", Color) = (.96,.98,1,1)
        _Matte ("Highlight restraint", Range(0,1)) = .1
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("Renderer color", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True"}
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SnowFrag
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
            float _Saturation,_Contrast,_Matte;
            fixed4 _SnowTint;
            fixed4 SnowFrag(v2f input) : SV_Target
            {
                fixed4 pixel=SampleSpriteTexture(input.texcoord);
                float luminance=dot(pixel.rgb,float3(.2126,.7152,.0722));
                float3 color=lerp(luminance.xxx,pixel.rgb,_Saturation);
                color=(color-.5)*_Contrast+.5;
                color*= _SnowTint.rgb;
                color=lerp(color,float3(.95,.96,.97),smoothstep(.72,1,luminance)*_Matte);
                pixel.rgb=saturate(color);
                pixel*=input.color;
                pixel.rgb*=pixel.a;
                return pixel;
            }
            ENDCG
        }
    }
}
