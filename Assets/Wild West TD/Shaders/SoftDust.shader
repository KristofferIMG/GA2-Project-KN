Shader "WildWestTD/SoftDust" {
 Properties { _MainTex("Soft particle",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) }
 SubShader {
  Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
  Pass {
   Tags {"LightMode"="SRPDefaultUnlit"}
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial)
   float4 _Color;
   CBUFFER_END
   struct Attributes {float4 positionOS:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   struct Varyings {float4 positionCS:SV_POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
   Varyings vert(Attributes input){Varyings o;o.positionCS=TransformObjectToHClip(input.positionOS.xyz);o.color=input.color*_Color;o.uv=input.uv;return o;}
   half4 frag(Varyings input):SV_Target{return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,input.uv)*input.color;}
   ENDHLSL
  }
 }
}

