using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Voxelmetric Lighting Render Pipeline")]
public class VoxelmetricLightingRPAsset : RenderPipelineAsset {
  
	protected override RenderPipeline CreatePipeline () => new VoxelmetricLightingRP();
}