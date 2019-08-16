Shader "Voxelmetric Lighting/Debug" {
	
	Properties {
    _Color ("Color", Color) = (0.5, 0.5, 0.5, 0.5)
  }
	
	SubShader {
		
		Pass {
      HLSLPROGRAM

      #pragma target 3.5

      #pragma vertex DebugPassVertex
			#pragma fragment DebugPassFragment
			
			#include "lib/Debug.hlsl"
			
			ENDHLSL
    }
	}
}