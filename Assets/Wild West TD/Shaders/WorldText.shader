Shader "WildWestTD/WorldText" { Properties { _MainTex ("Font", 2D) = "white" {} _Color ("Color", Color) = (1,1,1,1) } SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" } Pass { Blend SrcAlpha OneMinusSrcAlpha ZWrite Off ZTest LEqual Cull Back HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
struct A {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
struct V {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);float4 _Color;
V vert(A a){V o;o.vertex=TransformObjectToHClip(a.vertex.xyz);o.uv=a.uv;o.color=a.color*_Color;return o;}
half4 frag(V i):SV_Target {return half4(i.color.rgb,i.color.a*SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a);}
ENDHLSL } } }