using UnityEngine;
using UnityEngine.Rendering;

public class VoxelmetricLightingRP : RenderPipeline {

  private CommandBuffer commandBuffer = new CommandBuffer {
    name = "Voxelmetric Lighting"
  };

  private CullingResults cullingResults;

  public VoxelmetricLightingRP() {
    GraphicsSettings.lightsUseLinearIntensity = true;
  }

  protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras) {
    ScriptableCullingParameters cullingParameters;

    if (!cameras[0].TryGetCullingParameters(out cullingParameters))
      return;

    cullingResults = renderContext.Cull(ref cullingParameters);

    renderContext.SetupCameraProperties(cameras[0]);

    var drawSettings = new DrawingSettings(
      new ShaderTagId("SRPDefaultUnlit"),
      new SortingSettings()
    );

		var filterSettings = FilteringSettings.defaultValue;

    filterSettings.renderQueueRange = RenderQueueRange.opaque;

    commandBuffer.ClearRenderTarget(true, false, Color.clear);

    renderContext.DrawRenderers(cullingResults, ref drawSettings, ref filterSettings);

    renderContext.Submit();
  }
}