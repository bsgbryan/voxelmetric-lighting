#ifndef VLRP_DEBUG_INCLUDED
#define VLRP_DEBUG_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
  float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
  float4x4 unity_ObjectToWorld;
CBUFFER_END

#define UNITY_MATRIX_M unity_ObjectToWorld

CBUFFER_START(UnityPerMaterial)
	float4 _Color;
CBUFFER_END

struct VertexInput {
	float4 pos : POSITION;
};

struct VertexOutput {
	float4 clipPos : SV_POSITION;
};

VertexOutput DebugPassVertex (VertexInput input) {
	VertexOutput output;

  float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
	
  output.clipPos = mul(unity_MatrixVP, worldPos);
	
  return output;
}

float4 DebugPassFragment (VertexOutput input) : SV_TARGET {
  return _Color;
}

#endif // VLRP_DEBUG_INCLUDED