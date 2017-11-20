Shader "Outlined/Silhouette Only" {
Properties {
 _OutlineColor ("Outline Color", Color) = (0.000000,0.000000,0.000000,1.000000)
 _Outline ("Outline width", Range(0.000000,0.030000)) = 0.005000
}
SubShader { 
 Tags { "QUEUE"="Transparent" }
 Pass {
  Name "BASE"
  Tags { "QUEUE"="Transparent" }
  Blend Zero One
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 2.0
#include "UnityCG.cginc"
#pragma multi_compile_fog
#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

// uniforms
float4 _OutlineColor_ST;

// vertex shader input data
struct appdata {
  float3 pos : POSITION;
  float3 uv0 : TEXCOORD0;
  UNITY_VERTEX_INPUT_INSTANCE_ID
};

// vertex-to-fragment interpolators
struct v2f {
  fixed4 color : COLOR0;
  float2 uv0 : TEXCOORD0;
  #if USING_FOG
    fixed fog : TEXCOORD1;
  #endif
  float4 pos : SV_POSITION;
  UNITY_VERTEX_OUTPUT_STEREO
};

// vertex shader
v2f vert (appdata IN) {
  v2f o;
  UNITY_SETUP_INSTANCE_ID(IN);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  half4 color = half4(0,0,0,1.1);
  float3 eyePos = mul (UNITY_MATRIX_MV, float4(IN.pos,1)).xyz;
  half3 viewDir = 0.0;
  o.color = saturate(color);
  // compute texture coordinates
  o.uv0 = IN.uv0.xy * _OutlineColor_ST.xy + _OutlineColor_ST.zw;
  // fog
  #if USING_FOG
    float fogCoord = length(eyePos.xyz); // radial fog distance
    UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
    o.fog = saturate(unityFogFactor);
  #endif
  // transform position
  o.pos = UnityObjectToClipPos(IN.pos);
  return o;
}

// textures
sampler2D _OutlineColor; // not in shader properties, assuming 2D

// fragment shader
fixed4 frag (v2f IN) : SV_Target {
  fixed4 col;
  fixed4 tex, tmp0, tmp1, tmp2;
  // SetTexture #0
  tex = tex2D (_OutlineColor, IN.uv0.xy);
  col = fixed4(0,0,0,0);
  // fog
  #if USING_FOG
    col.rgb = lerp (unity_FogColor.rgb, col.rgb, IN.fog);
  #endif
  return col;
}

// texenvs
//! TexEnv0: 01020000 01020000 [_OutlineColor] [00000000]
ENDCG
 }
 Pass {
  Name "OUTLINE"
  Tags { "LIGHTMODE"="ALWAYS" "QUEUE"="Transparent" }
  Cull Front
  Blend One OneMinusDstColor
CGPROGRAM
#line 67 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif

#include "HLSLSupport.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"
#line 10

#include "UnityCG.cginc"
 
struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
};
 
struct v2f {
	float4 pos : POSITION;
	float4 color : COLOR;
};
 
uniform float _Outline;
uniform float4 _OutlineColor;
 
v2f vert(appdata v) {
	// just make a copy of incoming vertex data but scaled according to normal direction
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
 
	float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
	float2 offset = TransformViewToProjection(norm.xy);
 
	o.pos.xy += offset * o.pos.z * _Outline;
	o.color = _OutlineColor;
	return o;
}
#line 67 ""
#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
#endif
/* UNITY: Original start of shader */
#pragma vertex vert
#pragma fragment frag
 
half4 frag(v2f i) :COLOR {
	return i.color;
}
ENDCG
 }
}
Fallback "Diffuse"
}