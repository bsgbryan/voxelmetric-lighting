#ifndef VLRP_DEBUG_INCLUDED
#define VLRP_DEBUG_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
// #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"

// CBUFFER_START(UnityPerFrame)
//   float4x4 unity_MatrixVP;
// CBUFFER_END

// CBUFFER_START(UnityPerDraw)
//   float4x4 unity_ObjectToWorld;
// CBUFFER_END

// #define UNITY_MATRIX_M unity_ObjectToWorld

struct VertexInput {
  float3 normal : NORMAL;
	float4 pos : POSITION;
};

struct VertexOutput {
	float4 clipPos : SV_POSITION;
  float4 color : TEXCOORD1;
};

VertexOutput DebugPassVertex (VertexInput input) {
	VertexOutput output;

  float4 worldPos    = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
  float4 worldNormal = normalize(mul(UNITY_MATRIX_M, float4(input.normal.xyz, 1.0))) + 1 * 0.5;

  float x = UNITY_MATRIX_IT_MV[2].x * worldNormal.x;
  float z = UNITY_MATRIX_IT_MV[2].z * worldNormal.z;

  if (x > .5) {
    x = 1 - x;
  }

  x *= 2;

  if (z > .5) {
    z = 1 - z;
  }

  z *= 2;

  float3 diff = float3(x, 0, z);

  // float3 viewDir = UNITY_MATRIX_IT_MV[2].xyz * average;


  // float3 viewDir = positiveNormal;

  output.color   = float4(diff, 1.0);
  output.clipPos = mul(unity_MatrixVP, worldPos);
	
  return output;
}

float4 DebugPassFragment (VertexOutput input) : SV_TARGET {
  return input.color;
}

#endif // VLRP_DEBUG_INCLUDED